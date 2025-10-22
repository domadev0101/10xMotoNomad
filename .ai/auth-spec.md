# Specyfikacja Techniczna: Modu? Autentykacji U?ytkowników (MotoNomad)

## 1. Wprowadzenie

Niniejszy dokument opisuje architektur? i implementacj? modu?u rejestracji, logowania, wylogowywania i odzyskiwania has?a w aplikacji MotoNomad. Specyfikacja bazuje na wymaganiach zdefiniowanych w PRD oraz na przyj?tym stosie technologicznym (Blazor WASM + Supabase).

## 2. Architektura Interfejsu U?ytkownika (Frontend)

### 2.1. Zmiany w Strukturze Aplikacji

#### Nowe Strony (Pages)
- **`Pages/Auth/Register.razor`**: Strona z formularzem rejestracji nowego u?ytkownika. Dost?pna publicznie.
- **`Pages/Auth/Login.razor`**: Strona z formularzem logowania. Dost?pna publicznie.
- **`Pages/Auth/ForgotPassword.razor`**: Strona z formularzem do wys?ania linku resetuj?cego has?o.
- **`Pages/Auth/ResetPassword.razor`**: Strona umo?liwiaj?ca ustawienie nowego has?a po klikni?ciu w link z maila. B?dzie odczytywa? token z adresu URL.

#### Modyfikacja Layoutów i Komponentów
- **`Layout/MainLayout.razor`**:
  - Zostanie owini?ty w komponent `CascadingAuthenticationState`, aby stan uwierzytelnienia by? dost?pny w ca?ej aplikacji.
  - B?dzie warunkowo renderowa? `NavMenu` w zale?no?ci od stanu zalogowania.
- **`Layout/NavMenu.razor`**:
  - Wykorzysta `AuthorizeView` do dynamicznego wy?wietlania linków.
  - **Dla niezalogowanych (`<NotAuthorized>`)**: Wy?wietli linki "Zaloguj" i "Zarejestruj".
  - **Dla zalogowanych (`<Authorized>`)**: Wy?wietli linki "Moje Podró?e", "Profil" oraz przycisk "Wyloguj".
- **`Shared/RedirectToLogin.razor`**:
  - Komponent pomocniczy, który b?dzie u?ywany na stronach wymagaj?cych autoryzacji. Je?li u?ytkownik nie jest zalogowany, automatycznie przekieruje go na stron? `/login`.
- **`App.razor`**:
  - Zostanie zmodyfikowany, aby u?ywa? `AuthorizeRouteView` dla stron wymagaj?cych autoryzacji. Umo?liwi to zabezpieczenie dost?pu do np. `/trips` i automatyczne przekierowanie na stron? logowania zdefiniowan? w `RedirectToLogin`.
- **Strony chronione (np. `Trips/TripList.razor`)**:
  - Dodany zostanie atrybut `@attribute [Authorize]` na górze pliku, aby uniemo?liwi? dost?p niezalogowanym u?ytkownikom.

### 2.2. Formularze i Komponenty

- **Formularz Rejestracji (`Register.razor`)**:
  - Pola: `Email`, `Password`, `ConfirmPassword`.
  - Walidacja:
    - Wszystkie pola wymagane.
    - Email musi mie? poprawny format.
    - Has?o musi mie? min. 8 znaków (zgodnie z PRD).
    - `ConfirmPassword` musi by? identyczne z `Password`.
  - Logika: Po pomy?lnej walidacji wywo?uje `IAuthService.RegisterAsync()`. Po sukcesie przekierowuje na list? podró?y (`/trips`). W razie b??du (np. u?ytkownik ju? istnieje) wy?wietla komunikat z `MudSnackbar`.

- **Formularz Logowania (`Login.razor`)**:
  - Pola: `Email`, `Password`.
  - Walidacja: Pola wymagane.
  - Logika: Wywo?uje `IAuthService.LoginAsync()`. Po sukcesie zapisuje sesj? i przekierowuje na `/trips`. W razie b??du ("Nieprawid?owe dane logowania") wy?wietla komunikat.

- **Formularz Odzyskiwania Has?a (`ForgotPassword.razor`)**:
  - Pole: `Email`.
  - Logika: Wywo?uje `IAuthService.SendPasswordResetEmailAsync()`. Po wywo?aniu zawsze wy?wietla komunikat o powodzeniu (ze wzgl?dów bezpiecze?stwa, aby nie ujawnia?, czy dany email istnieje w bazie).

### 2.3. Scenariusze U?ytkownika i Obs?uga B??dów

- **Rejestracja**:
  - **Sukces**: U?ytkownik jest tworzony w Supabase, automatycznie logowany, a sesja zapisywana w `LocalStorage`. Nast?puje przekierowanie do `/trips`.
  - **B??d**: Komunikat "U?ytkownik o tym adresie email ju? istnieje." lub "Has?o jest zbyt s?abe.".
- **Logowanie**:
  - **Sukces**: Sesja jest pobierana z Supabase i zapisywana w `LocalStorage`. Nast?puje od?wie?enie stanu autentykacji i przekierowanie.
  - **B??d**: Komunikat "Nieprawid?owy adres email lub has?o.".
- **Dost?p do chronionej strony**: Niezalogowany u?ytkownik próbuj?cy wej?? na `/trips` jest przekierowywany na `/login`.
- **Wylogowanie**: Sesja jest usuwana z `LocalStorage` i Supabase. U?ytkownik jest przekierowywany na stron? g?ówn?.

## 3. Logika Warstwy Aplikacji i Infrastruktury

Poniewa? aplikacja dzia?a w modelu Blazor WASM Standalone, ca?a logika znajduje si? po stronie klienta. Komunikacja z "backendem" odbywa si? poprzez API Supabase.

### 3.1. Kontrakty (Interfejsy)

- **`Application/Interfaces/IAuthService.cs`**:
  ```csharp
  public interface IAuthService
  {
      Task<User?> GetCurrentUser();
      Task LoginAsync(string email, string password);
      Task RegisterAsync(string email, string password, string confirmPassword);
      Task LogoutAsync();
      Task SendPasswordResetEmailAsync(string email);
      Task UpdatePasswordAsync(string newPassword);
  }
  ```

### 3.2. Serwisy (Implementacje)

- **`Infrastructure/Services/AuthService.cs`**:
  - Implementuje `IAuthService`.
  - Wstrzykuje klienta Supabase (`Supabase.Client`) oraz `Blazored.LocalStorage.ILocalStorageService`.
  - Metody serwisu b?d? opakowywa? wywo?ania `supabase.Auth`, np. `supabase.Auth.SignUp()`, `supabase.Auth.SignInWithPassword()`.
  - B?dzie zarz?dza? sesj? u?ytkownika, zapisuj?c j? w `LocalStorage` po udanym logowaniu/rejestracji i usuwaj?c po wylogowaniu.
  - B?dzie obs?ugiwa? wyj?tki zwracane przez Supabase (np. `Gotrue.Exceptions.BadRequestException`) i mapowa? je na zrozumia?e dla u?ytkownika b??dy.

### 3.3. Modele Danych (DTOs)

- **`Application/DTOs/Auth/RegisterRequest.cs`**: Model dla formularza rejestracji z adnotacjami walidacyjnymi.
- **`Application/DTOs/Auth/LoginRequest.cs`**: Model dla formularza logowania z adnotacjami.

## 4. System Autentykacji (Integracja z Supabase)

### 4.1. Konfiguracja
- **`Program.cs`**:
  - Zostanie zarejestrowany klient Supabase jako Singleton, pobieraj?c `Url` i `AnonKey` z `appsettings.json`.
  - Zostan? zarejestrowane serwisy: `IAuthService` i `AuthService`.
  - Zostanie dodana obs?uga autoryzacji Blazora: `AddAuthorizationCore()`.
  - `SupabaseAuthenticationStateProvider` zostanie zarejestrowany jako implementacja `AuthenticationStateProvider`.
- **`Infrastructure/Auth/SupabaseAuthenticationStateProvider.cs`**:
  - Nowa klasa dziedzicz?ca po `AuthenticationStateProvider`.
  - W konstruktorze wstrzyknie `ILocalStorageService` i klienta Supabase.
  - Metoda `GetAuthenticationStateAsync` b?dzie sprawdza?, czy w `LocalStorage` istnieje zapisana sesja.
    - Je?li tak, odtworzy sesj? w kliencie Supabase i zwróci `ClaimsPrincipal` z danymi u?ytkownika (ID, email).
    - Je?li nie, zwróci pusty `ClaimsPrincipal` dla anonimowego u?ytkownika.
  - B?dzie zawiera? metody `MarkUserAsAuthenticated()` i `MarkUserAsLoggedOut()`, które zaktualizuj? stan i powiadomi? Blazora o zmianie (`NotifyAuthenticationStateChanged`).

### 4.2. Zabezpieczenia (Supabase)
- **Row Level Security (RLS)**: Kluczowy element bezpiecze?stwa.
  - Dla tabeli `Trips` zostanie w??czona polityka RLS, która pozwoli na odczyt/zapis/modyfikacj? rekordu tylko wtedy, gdy `user_id` w rekordzie jest równe `auth.uid()`. To gwarantuje, ?e u?ytkownicy widz? tylko swoje dane, nawet je?li u?ywaj? tego samego klucza `AnonKey`.
- **Szablony E-mail**: W panelu Supabase zostan? skonfigurowane szablony wiadomo?ci email dla potwierdzenia rejestracji (je?li w??czone) i resetowania has?a.

## 5. Podsumowanie Kluczowych Kroków Implementacji

1.  **Struktura**: Utworzenie nowych plików dla stron, serwisów i DTOs w odpowiednich katalogach.
2.  **Konfiguracja DI**: Rejestracja `Supabase.Client`, `IAuthService`, `AuthenticationStateProvider` i `ILocalStorageService` w `Program.cs`.
3.  **UI**: Implementacja formularzy w `Register.razor` i `Login.razor` z u?yciem `MudForm` i walidacji.
4.  **Layout**: Modyfikacja `MainLayout.razor` i `NavMenu.razor` w celu dynamicznego renderowania UI w zale?no?ci od stanu autoryzacji.
5.  **Logika**: Implementacja `AuthService` z logik? opakowuj?c? klienta Supabase.
6.  **Stan Aplikacji**: Stworzenie `SupabaseAuthenticationStateProvider` do zarz?dzania stanem uwierzytelnienia w ca?ej aplikacji.
7.  **Routing**: Zabezpieczenie stron (np. `/trips`) za pomoc? atrybutu `[Authorize]` i `AuthorizeRouteView`.
8.  **Supabase**: W??czenie i skonfigurowanie polityk RLS dla tabel w bazie danych.
