# Wyprowadzanie kluczy (KDF) – jak to działa w PasswordManager

Dokument wyjaśnia krok po kroku, jak z hasła głównego powstają klucze, jak są
używane przy rejestracji, logowaniu i szyfrowaniu wpisów oraz co oznaczają
nazwy, stałe i zmienne używane w kodzie.

Kod, którego dotyczy dokument:

| Plik | Rola |
|---|---|
| [Service/Crypto/MasterKeyDerivation.cs](../Service/Crypto/MasterKeyDerivation.cs) | KDF: Argon2id + HKDF, klasa `DerivedKeys` |
| [Service/Crypto/AesGcmBox.cs](../Service/Crypto/AesGcmBox.cs) | szyfrowanie AES-256-GCM (z AAD) |
| [Service/AuthService.cs](../Service/AuthService.cs) | rejestracja, logowanie, klucz sejfu, weryfikacja hasła |
| [Service/EncryptionService.cs](../Service/EncryptionService.cs) | szyfrowanie wpisów kluczem sejfu |
| [Model/UserSession.cs](../Model/UserSession.cs) | klucze trzymane w pamięci w trakcie sesji |
| [Model/UserKeys.cs](../Model/UserKeys.cs), [Repository/UserKeysRepository.cs](../Repository/UserKeysRepository.cs) | tabela `user_keys` |

---

## 1. Po co KDF?

Hasło wpisane przez człowieka nie nadaje się bezpośrednio na klucz szyfrujący:

- ma mało entropii (da się je zgadywać słownikiem),
- ma dowolną długość, a AES-256 potrzebuje dokładnie 32 bajtów,
- gdyby trafiło na serwer, serwer mógłby odszyfrować sejf.

**KDF (Key Derivation Function)** zamienia hasło w klucz kryptograficzny
o stałej długości i celowo robi to **wolno i pamięciożernie**, żeby każda próba
zgadnięcia hasła była kosztowna dla atakującego.

W projekcie KDF realizuje trzy cele naraz:

1. **Zero-knowledge** – serwer (Supabase) nigdy nie dostaje hasła głównego ani
   klucza, którym da się odszyfrować dane.
2. **Odporność na brute-force** – Argon2id wymaga 64 MiB pamięci i kilku
   przebiegów na każdą próbę.
3. **Separacja kluczy** – z jednego hasła powstają dwa niezależne klucze:
   jeden do logowania, drugi do szyfrowania. Znajomość jednego nie zdradza drugiego.

---

## 2. Słownik pojęć

| Nazwa | Co to jest |
|---|---|
| **hasło główne** (master password) | Hasło wpisywane przez użytkownika. Istnieje tylko na urządzeniu, w pamięci, na czas wyprowadzania kluczy. |
| **sól** (`salt`) | Dodatkowe dane dla Argon2id, dzięki którym to samo hasło u różnych użytkowników daje różne klucze. Tutaj: `SHA-256("PasswordManager.kdf.v1|" + email)`. |
| **Argon2id** | Algorytm KDF (zwycięzca Password Hashing Competition, RFC 9106). Wersja *id* łączy odporność na ataki GPU (Argon2d) i na ataki kanałem bocznym (Argon2i). |
| **`masterKey`** | 32-bajtowy wynik Argon2id. Używany tylko do wyprowadzenia dwóch kolejnych kluczy, potem natychmiast zerowany. |
| **HKDF** | HMAC-based Key Derivation Function (RFC 5869). Tutaj używany jest tylko etap *Expand*: z jednego silnego klucza robi kilka niezależnych, rozróżnianych parametrem `info`. |
| **`info`** | Etykieta (ciąg bajtów) w HKDF. Różne `info` → zupełnie różne klucze z tego samego `masterKey`. |
| **`authKey`** | Klucz uwierzytelniający. Jego zapis Base64 jest wysyłany do Supabase **jako hasło konta**. |
| **`kek`** | *Key-Encryption Key* – klucz do szyfrowania innego klucza. Szyfruje klucz sejfu. Nigdy nie opuszcza urządzenia. |
| **klucz sejfu** (`vaultKey`) | Losowe 32 bajty. Właściwy klucz AES-256, którym szyfrowane są wpisy. **Nie** jest wyprowadzany z hasła. |
| **opakowany klucz sejfu** (`wrappedVaultKey`) | Klucz sejfu zaszyfrowany kluczem `kek` (AES-GCM). Przechowywany w tabeli `user_keys`. „Opakowanie” (*key wrapping*) = szyfrowanie klucza innym kluczem. |
| **AES-256-GCM** | Szyfr symetryczny z uwierzytelnieniem. Oprócz szyfrowania wylicza **tag**, który wykrywa każdą modyfikację danych, użycie złego klucza lub złego AAD. |
| **nonce** | 12 losowych bajtów, inne przy każdym szyfrowaniu. Nie jest tajny, ale nie może się powtórzyć dla tego samego klucza. |
| **tag** | 16 bajtów „sumy kontrolnej” AES-GCM. Odszyfrowanie bez zgodnego tagu kończy się wyjątkiem. |
| **AAD** (Associated Data) | Dane uwierzytelniane, ale nieszyfrowane. Wiążą szyfrogram z kontekstem (np. użytkownikiem i wpisem). Inne AAD przy odszyfrowaniu → błąd tagu. |
| **blob** | Jeden ciąg bajtów `nonce(12) | tag(16) | szyfrogram`, wygodny do zapisania w jednej kolumnie. |
| **separacja domen** (*domain separation*) | Dopisywanie prefiksów typu `PasswordManager.kek.v1`, żeby wynik jednej operacji nie dał się użyć w innym miejscu. |

---

## 3. Przegląd całości

```
                 email                         hasło główne
                   │                                │
   "PasswordManager.kdf.v1|" + normalize(email)     │
                   │                                │
               SHA-256                              │
                   │                                │
                 salt ───────────►  Argon2id  ◄─────┘
                               (m=64 MiB, t=3, p=4)
                                        │
                                    masterKey (32 B)
                                        │
                     ┌──────────────────┴──────────────────┐
           HKDF-Expand("…auth.v1")              HKDF-Expand("…kek.v1")
                     │                                     │
                 authKey (32 B)                        kek (32 B)
                     │                                     │
                  Base64                     AES-GCM: opakowuje / odpakowuje
                     │                                     │
         hasło konta w Supabase (GoTrue)        vaultKey (losowy, 32 B)
                                                           │
                                          AES-GCM: szyfruje każdy wpis
```

Najważniejsze: **hasło główne → dwa klucze o różnych rolach**. Serwer zna tylko
`authKey` (a dokładniej jego hash bcrypt, który przechowuje GoTrue). Z `authKey`
nie da się obliczyć `kek`, bo oba powstają z `masterKey` przez jednokierunkową
funkcję HKDF z innym `info`, a `masterKey` nigdy nie opuszcza urządzenia.

---

## 4. Wyprowadzanie kluczy krok po kroku – `MasterKeyDerivation.DeriveAsync`

Sygnatura:

```csharp
public static async Task<DerivedKeys> DeriveAsync(string email, string password)
```

### Krok 1 – normalizacja emaila

```csharp
private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
```

Supabase zapisuje emaile małymi literami. Bez normalizacji `Jan@x.pl`
i `jan@x.pl ` dałyby różne sole, czyli różne klucze – i użytkownik
„z poprawnym hasłem” nie mógłby się zalogować.

### Krok 2 – sól

```csharp
byte[] salt = SHA256.HashData(Encoding.UTF8.GetBytes(SaltPrefix + NormalizeEmail(email)));
// SaltPrefix = "PasswordManager.kdf.v1|"
```

- Sól jest **deterministyczna** (zależy tylko od emaila), bo musi być znana
  **przed** zalogowaniem – klient nie ma jeszcze dostępu do bazy, więc nie może
  pobrać losowej soli z serwera.
- Prefiks `PasswordManager.kdf.v1|` to separacja domen: ta sama aplikacja
  hashuje email także w innym miejscu (Windows Hello, `biometric.json`) i oba
  wyniki nie mogą być takie same.
- `v1` w prefiksie: przy zmianie schematu soli zmieni się wersja.
- SHA-256 daje stałe 32 bajty niezależnie od długości emaila.

### Krok 3 – Argon2id → `masterKey`

```csharp
using Argon2id argon2 = new(passwordBytes)
{
    Salt = salt,
    MemorySize = Argon2MemoryKiB,       // 64 * 1024 KiB = 64 MiB
    Iterations = Argon2Iterations,      // 3
    DegreeOfParallelism = Argon2Parallelism // 4
};
masterKey = argon2.GetBytes(KeySize);   // 32 bajty
```

| Parametr | Stała w kodzie | Wartość | Znaczenie |
|---|---|---|---|
| `m` (pamięć) | `Argon2MemoryKiB` | 65 536 KiB = 64 MiB | Każda próba hasła wymaga tyle RAM. Utrudnia masowe zgadywanie na GPU/ASIC. |
| `t` (iteracje) | `Argon2Iterations` | 3 | Liczba przebiegów po całej pamięci – koszt czasowy. |
| `p` (równoległość) | `Argon2Parallelism` | 4 | Liczba niezależnych „pasów” liczonych równolegle. |
| długość wyniku | `KeySize` | 32 B | Rozmiar `masterKey`. |

Szczegóły implementacyjne:

- Hasło jest zamieniane na bajty UTF-8 (`passwordBytes`), a po użyciu tablica
  jest **zerowana** (`CryptographicOperations.ZeroMemory`).
- Argon2id liczy się w `Task.Run`, czyli poza wątkiem UI – trwa zauważalny
  ułamek sekundy i aplikacja nie może się w tym czasie „zawiesić”.
- Biblioteka: `Konscious.Security.Cryptography.Argon2`.

### Krok 4 – HKDF-Expand → `authKey` i `kek`

```csharp
byte[] authKey = HKDF.Expand(HashAlgorithmName.SHA256, masterKey, KeySize, AuthKeyInfo);
byte[] kek     = HKDF.Expand(HashAlgorithmName.SHA256, masterKey, KeySize, KekInfo);
// AuthKeyInfo = "PasswordManager.auth.v1"
// KekInfo     = "PasswordManager.kek.v1"
```

- Używany jest tylko etap **Expand** HKDF. Etap *Extract* służy do
  „wygładzenia” danych o nierównej losowości; wynik Argon2id jest już
  jednolicie losowy, więc Extract nie jest potrzebny.
- Różne `info` → dwa klucze, które z punktu widzenia atakującego są
  niezależne. Mając `authKey`, nie da się policzyć ani `masterKey`, ani `kek`.

### Krok 5 – sprzątanie i wynik

```csharp
finally
{
    CryptographicOperations.ZeroMemory(passwordBytes);
    CryptographicOperations.ZeroMemory(masterKey);
}
return new DerivedKeys(authKey, kek);
```

`masterKey` istnieje w pamięci tylko przez chwilę. Wynik to obiekt
`DerivedKeys`:

| Składowa | Opis |
|---|---|
| `AuthKey` | 32 B, klucz uwierzytelniający |
| `Kek` | 32 B, klucz do opakowania klucza sejfu |
| `AuthKeyBase64` | `AuthKey` jako Base64 (44 znaki) – hasło konta w Supabase |
| `Dispose()` | zeruje `AuthKey` i `Kek` |

W kodzie zawsze używany jest wzorzec:

```csharp
using DerivedKeys keys = await MasterKeyDerivation.DeriveAsync(email, password);
```

więc klucze są zerowane zaraz po wyjściu z metody.

> **Dlaczego Base64?** GoTrue przyjmuje hasło jako tekst. Base64 z 32 losowych
> bajtów zawiera tylko litery, cyfry, `+`, `/` i `=` – dlatego w ustawieniach
> Supabase trzeba **wyłączyć wymagania co do znaków hasła** (np. „wymagaj
> znaku specjalnego”), bo nie zawsze będą spełnione. Siła tego „hasła” to
> pełne 256 bitów, więc reguły i tak nic by nie dawały.

---

## 5. Rejestracja – `AuthService.SignUpAsync`

```
1. keys = DeriveAsync(email, hasło)
2. Supabase.Auth.SignUp(email, keys.AuthKeyBase64)
3. keys.Dispose()  (using)
```

- Do Supabase trafia **tylko** `AuthKeyBase64`. Hasło główne nie opuszcza urządzenia.
- Klucz sejfu **nie** jest tu tworzony. Przy włączonym potwierdzaniu emaila
  po `SignUp` nie ma jeszcze sesji, więc RLS i tak nie pozwoliłby zapisać
  wiersza w `user_keys`. Klucz powstaje przy pierwszym logowaniu.
- Błąd GoTrue (`GotrueException`) jest zamieniany na `SupabaseAuthException`.

---

## 6. Logowanie – `AuthService.SignInAsync`

### 6.1 Uwierzytelnienie

```
1. keys = DeriveAsync(email, hasło)
2. session = Supabase.Auth.SignIn(email, keys.AuthKeyBase64)
3. userId  = Supabase.Auth.CurrentUser.Id
```

Złe hasło → inny `authKey` → GoTrue odrzuca logowanie (`SupabaseAuthException`).

### 6.2 Pobranie lub utworzenie klucza sejfu – `LoadOrCreateVaultKeyAsync(kek, userId)`

Najpierw budowane jest AAD klucza sejfu:

```csharp
aad = UTF8("PasswordManager.vault-key.v1|" + userId)
```

Następnie `UserKeysRepository.GetAsync()` pobiera wiersz z `user_keys`
(RLS zwraca tylko wiersz zalogowanego użytkownika).

**Przypadek A – brak wiersza (pierwsze logowanie):**

```
vaultKey = RandomNumberGenerator.GetBytes(32)
wrapped  = AesGcmBox.SealBlob(kek, vaultKey, aad)     // nonce|tag|ct = 60 B
INSERT user_keys (kdf_version = 1, wrapped_vault_key = Base64(wrapped))
```

`user_id` nie jest wysyłany – baza wstawia go sama (`DEFAULT auth.uid()`),
więc nie da się utworzyć kluczy w imieniu innego użytkownika.

**Przypadek B – wiersz istnieje:**

```
sprawdź kdf_version == MasterKeyDerivation.CurrentVersion   (inaczej błąd)
wrapped  = Base64Decode(wrapped_vault_key)                   (FormatException → błąd)
vaultKey = AesGcmBox.OpenBlob(kek, wrapped, aad)            (zły tag → błąd)
```

`OpenBlob` uda się tylko wtedy, gdy:

- `kek` jest poprawny (czyli hasło i email są poprawne),
- AAD się zgadza (wiersz należy do tego `userId`),
- dane nie zostały zmodyfikowane.

### 6.3 Zapis do sesji

```csharp
userSession.Clear();
userSession.VaultKey        = vaultKey;
userSession.WrappedVaultKey = wrappedVaultKey;
userSession.UserId          = userId;
userSession.Email           = request.Email;
```

`kek` i `authKey` są w tym momencie zerowane (`using`). W pamięci zostaje tylko
klucz sejfu – to on jest potrzebny do pracy z wpisami.

### 6.4 Obsługa błędów

Jeżeli logowanie do Supabase się udało, ale klucza sejfu nie da się
otworzyć, aplikacja:

1. wywołuje `SignOut()` – żeby nie zostać w stanie „zalogowany, ale bez klucza”,
2. rzuca `VaultUnlockException`,
3. `LoginRegisterViewModel` pokazuje komunikat *„Unable to unlock the vault”*.

Typowa przyczyna: hasło zostało zresetowane mailem przez Supabase. Wtedy
`authKey` w GoTrue jest nowy, ale klucz sejfu w `user_keys` wciąż jest
opakowany starym `kek`.

---

## 7. Szyfrowanie wpisów – `EncryptionService`

Klucz sejfu szyfruje każdy wpis osobno:

```
id        = Guid.CreateVersion7()   (tylko dla nowego wpisu)
plaintext = UTF8(JSON wpisu)        (bez Id – [JsonIgnore])
aad       = UTF8("PasswordManager.entry.v2|" + userId + "|" + id)
(nonce, tag, ct) = AES-256-GCM(vaultKey, plaintext, aad)
→ wiersz password: id, version = 2, content = Base64(ct), nonce, tag (Base64)
```

- `id` generuje klient (UUIDv7: losowy, ale rosnący w czasie), bo musi być
  znany **przed** szyfrowaniem, żeby trafić do AAD.
- Dzięki AAD nie da się przenieść szyfrogramu do innego wiersza ani do innego
  użytkownika – odszyfrowanie się nie powiedzie.
- `version` (`EncryptionService.CurrentVersion = 2`) określa format.
  `Decrypt` odrzuca inne wersje.

Wpisy **nie** są szyfrowane bezpośrednio kluczem z hasła – patrz rozdział 9.

---

## 8. Weryfikacja hasła bez serwera – `VerifyMasterPasswordAsync`

Używana np. przed włączeniem Windows Hello (`MainViewModel`):

```
keys = DeriveAsync(session.Email, hasło)
try  AesGcmBox.OpenBlob(keys.Kek, session.WrappedVaultKey, aad)  → true
catch CryptographicException                                     → false
```

Nie wymaga połączenia z Supabase: poprawne hasło daje `kek`, który przechodzi
kontrolę tagu AES-GCM. Odpakowany klucz jest od razu zerowany.

**Windows Hello** przechowuje (zaszyfrowane kluczem z TPM) email i hasło
główne, a przy logowaniu przez Hello wywołuje zwykłe `SignInAsync` – więc
przechodzi dokładnie tę samą ścieżkę KDF co logowanie hasłem.

---

## 9. Dlaczego losowy klucz sejfu, a nie klucz z hasła?

Można by szyfrować wpisy bezpośrednio kluczem `kek`. Wprowadzenie osobnego,
losowego `vaultKey` daje:

| Korzyść | Wyjaśnienie |
|---|---|
| Zmiana hasła bez przeszyfrowania danych | Wystarczy ponownie opakować `vaultKey` nowym `kek` (jeden wiersz w `user_keys`), zamiast odszyfrowywać i szyfrować wszystkie wpisy. |
| Zmiana parametrów KDF | Tak samo – nowa wersja KDF to tylko nowe opakowanie klucza. |
| Pełna entropia klucza | `vaultKey` ma 256 bitów losowości niezależnie od siły hasła. |
| Szybka weryfikacja hasła offline | `wrappedVaultKey` służy jako „sprawdzian” hasła (rozdział 8). |

---

## 10. Wylogowanie

`AuthService.SignOut()` → `Supabase.Auth.SignOut()` + `UserSession.Clear()`,
które zeruje `VaultKey` w pamięci i czyści `UserId`, `Email`,
`WrappedVaultKey`.

---

## 11. Co wie serwer, a czego nie

| Dane | Supabase | Urządzenie |
|---|---|---|
| hasło główne | ✗ | tylko chwilowo, podczas KDF |
| `masterKey` | ✗ | tylko chwilowo, zerowany |
| `authKey` | tylko jako hash bcrypt (GoTrue) | chwilowo |
| `kek` | ✗ | chwilowo, zerowany |
| `vaultKey` | ✗ (tylko opakowany) | w `UserSession` do wylogowania |
| `wrapped_vault_key` | ✓ (`user_keys`) | w `UserSession` |
| treść wpisów | ✗ (tylko szyfrogram, nonce, tag) | po odszyfrowaniu |
| email, `user_id` | ✓ | ✓ |

Nawet pełny dostęp do bazy (np. wyciek lub administrator) nie wystarcza do
odszyfrowania sejfu – potrzebne jest hasło główne, a każda próba jego
zgadnięcia kosztuje jedno wywołanie Argon2id (64 MiB, 3 przebiegi).

---

## 12. Stałe i nazwy – zestawienie

| Nazwa w kodzie | Wartość | Gdzie | Rola |
|---|---|---|---|
| `MasterKeyDerivation.CurrentVersion` | `1` | `MasterKeyDerivation` | wersja parametrów KDF, zapisywana jako `user_keys.kdf_version` |
| `SaltPrefix` | `"PasswordManager.kdf.v1\|"` | `MasterKeyDerivation` | separacja domen dla soli |
| `AuthKeyInfo` | `"PasswordManager.auth.v1"` | `MasterKeyDerivation` | `info` HKDF dla `authKey` |
| `KekInfo` | `"PasswordManager.kek.v1"` | `MasterKeyDerivation` | `info` HKDF dla `kek` |
| `Argon2MemoryKiB` | `65536` | `MasterKeyDerivation` | 64 MiB pamięci |
| `Argon2Iterations` | `3` | `MasterKeyDerivation` | przebiegi Argon2id |
| `Argon2Parallelism` | `4` | `MasterKeyDerivation` | pasy Argon2id |
| `KeySize` | `32` | `MasterKeyDerivation`, `AesGcmBox` | długość kluczy (256 bitów) |
| `AesGcmBox.NonceSize` | `12` | `AesGcmBox` | nonce AES-GCM (96 bitów) |
| `AesGcmBox.TagSize` | `16` | `AesGcmBox` | tag AES-GCM (128 bitów) |
| `VaultKeyAadPrefix` | `"PasswordManager.vault-key.v1\|"` | `AuthService` | AAD opakowanego klucza sejfu (+ `userId`) |
| `EncryptionService.CurrentVersion` | `2` | `EncryptionService` | format szyfrogramu wpisu (`password.version`) |
| `EntryAadPrefix` | `"PasswordManager.entry.v2\|"` | `EncryptionService` | AAD wpisu (+ `userId\|id`) |

Kolumny w bazie:

| Tabela.kolumna | Zawartość |
|---|---|
| `user_keys.user_id` | właściciel (`auth.uid()`), klucz główny |
| `user_keys.kdf_version` | wersja KDF, którą opakowano klucz |
| `user_keys.wrapped_vault_key` | Base64 bloba `nonce\|tag\|ct` (60 B → 80 znaków) |
| `password.id` | UUIDv7 wpisu, część AAD |
| `password.version` | format szyfrogramu (2) |
| `password.content` / `nonce` / `tag` | Base64 szyfrogramu, nonce i tagu |

---

## 13. Konsekwencje i ograniczenia

- **Zapomniane hasło = utrata sejfu.** Nikt (także administrator Supabase)
  nie może odtworzyć `kek`. Reset hasła mailem pozwoli się zalogować, ale
  sejfu nie odblokuje (`VaultUnlockException`).
- **Zmiana emaila zmienia sól**, a więc wszystkie klucze. Obsługa zmiany
  emaila wymagałaby ponownego opakowania klucza sejfu nowym `kek`.
- **Parametry KDF są stałe dla wersji.** Klient musi je znać przed
  logowaniem, więc nie są pobierane z serwera. Zmiana któregokolwiek
  parametru (pamięć, iteracje, prefiksy, `info`) wymaga nowej
  `CurrentVersion` i migracji: zalogować się starą wersją, opakować
  `vaultKey` nowym `kek`, zapisać nowy `kdf_version` i zmienić hasło konta
  w Supabase na nowy `authKey`.
- **Deterministyczna sól** pozwala atakującemu przygotować obliczenia dla
  konkretnego emaila z wyprzedzeniem, ale nie dla wielu kont naraz – każdy
  email ma inną sól, więc tablice tęczowe nie działają.
- **Zerowanie pamięci jest częściowe.** Tablice bajtów są zerowane, ale
  hasło jako `string` w .NET jest niezmienne i może pozostać w pamięci
  do czasu działania Garbage Collectora.
