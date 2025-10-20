# Plan implementacji widoku Login

## 1. Przegląd

Widok Login umożliwia uwierzytelnienie istniejących użytkowników aplikacji MotoNomad. Jest to publiczna strona dostępna dla niezalogowanych użytkowników. Po poprawnym zalogowaniu użytkownik zostaje przekierowany na stronę główną z listą wycieczek (`/trips`). Widok zawiera formularz z polami email i hasło, obsługę błędów uwierzytelnienia oraz link do strony rejestracji.

## 2. Routing widoku

**Ścieżka:** `/login`

**Dostępność:** Publiczna (dostępna dla niezalogowanych użytkowników)

**Routing w App.razor:**
```csharp
<RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
```

**Przekierowanie po zalogowaniu:** `/trips`

## 3. Struktura komponentów

```
Login.razor (Strona główna)
├── MudContainer
│   └── MudCard
│       ├── MudCardHeader
│       │   └── MudText (Tytuł: "Zaloguj się")
│       ├── MudCardContent
│       │   ├── MudAlert (błędy AuthException) [warunkowo]
│       │   ├── MudForm (@ref="form")
│       │   │   ├── MudTextField (Email)
│       │   │   └── MudTextField (Hasło, type="password")
│       │   └── MudProgressCircular [podczas ładowania]
│       ├── MudCardActions
│       │   └── MudButton ("Zaloguj", Primary)
│       └── MudCardActions (secondary)
│           └── MudLink ("Nie masz konta? Zarejestruj się", href="/register")
```

## 4. Szczegóły komponentów

### Login.razor (Komponent strony)

**Opis komponentu:**
Główny komponent strony logowania zawierający kompletny formularz uwierzytelnienia. Obsługuje walidację danych wejściowych, komunikację z API, wyświetlanie błędów i przekierowanie po sukcesie.

**Główne elementy:**
- `MudContainer` (MaxWidth.Small) - kontener centrujący formularz
- `MudCard` - karta zawierająca cały formularz logowania
- `MudCardHeader` - nagłówek z tytułem "Zaloguj się"
- `MudCardContent` - zawartość z formularzem i alertami
- `MudAlert` (Severity.Error, warunkowo) - wyświetla błędy AuthException
- `MudForm` - formularz z referencją dla walidacji
- `MudTextField` (Email) - pole email z walidacją formatu
- `MudTextField` (Hasło) - pole hasła z type="password"
- `MudProgressCircular` (warunkowo) - spinner podczas logowania
- `MudButton` (Primary) - przycisk "Zaloguj"
- `MudLink` - link do strony rejestracji

**Obsługiwane zdarzenia:**
- `OnValidSubmit` formularza - wywołuje metodę `HandleLoginAsync()`
- `OnClick` przycisku "Zaloguj" - submit formularza (jeśli walidacja OK)

**Warunki walidacji:**
- **Email:**
  - Wymagane pole (Required)
  - Poprawny format email (EmailAddress validation)
  - Max 255 znaków
  - Komunikat błędu: "Email jest wymagany" / "Nieprawidłowy format email"
- **Hasło:**
  - Wymagane pole (Required)
  - Min 8 znaków
  - Max 100 znaków
  - Komunikat błędu: "Hasło jest wymagane" / "Hasło musi mieć min. 8 znaków"

**Typy:**
- `LoginCommand` (request)
- `UserDto` (response)
- `AuthException` (błędy)
- `ValidationException` (błędy walidacji)

**Propsy:**
Brak (komponent strony najwyższego poziomu)

**Parametry wstrzykiwane (Dependency Injection):**
- `[Inject] IAuthService AuthService { get; set; }`
- `[Inject] NavigationManager NavigationManager { get; set; }`
- `[Inject] ISnackbar Snackbar { get; set; }`

## 5. Typy

### LoginCommand (Request DTO)
```csharp
public record LoginCommand
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
```

**Pola:**
- `Email` (string, required) - adres email użytkownika
- `Password` (string, required) - hasło użytkownika

### UserDto (Response DTO)
```csharp
public record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

**Pola:**
- `Id` (Guid) - unikalny identyfikator użytkownika
- `Email` (string) - adres email
- `DisplayName` (string?, opcjonalne) - nazwa wyświetlana
- `AvatarUrl` (string?, opcjonalne) - URL awatara
- `CreatedAt` (DateTime) - data utworzenia konta

### ViewModel (stan komponentu)
```csharp
private class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

## 6. Zarządzanie stanem

**Zmienne stanu komponentu:**

```csharp
private MudForm form = null!;
private LoginViewModel model = new();
private bool isLoading = false;
private string? errorMessage = null;
```

**Opis zmiennych:**
- `form` - referencja do MudForm dla ręcznej walidacji
- `model` - ViewModel zawierający dane formularza (email, hasło)
- `isLoading` - flaga stanu ładowania (true podczas wywołania API)
- `errorMessage` - komunikat błędu z AuthException (null jeśli brak błędu)

**Przepływ stanu:**
1. Użytkownik wypełnia pola (Email, Password) → aktualizacja `model`
2. Kliknięcie "Zaloguj" → `isLoading = true`, `errorMessage = null`
3. Wywołanie `AuthService.LoginAsync(command)`
4. Sukces → przekierowanie na `/trips`
5. Błąd → `errorMessage = ex.Message`, `isLoading = false`

**Brak potrzeby custom hooka** - stan zarządzany lokalnie w komponencie.

## 7. Integracja API

**Endpoint:** `IAuthService.LoginAsync(LoginCommand command)`

**Typ żądania:** `LoginCommand`
```csharp
new LoginCommand 
{ 
    Email = model.Email.Trim(), 
    Password = model.Password 
}
```

**Typ odpowiedzi:** `Task<UserDto>`

**Obsługa sukcesu:**
```csharp
var user = await AuthService.LoginAsync(command);
Snackbar.Add("Zalogowano pomyślnie!", Severity.Success);
NavigationManager.NavigateTo("/trips");
```

**Obsługa błędów:**
```csharp
catch (AuthException ex)
{
    errorMessage = ex.Message; // np. "Nieprawidłowe dane logowania"
}
catch (Exception ex)
{
    errorMessage = "Wystąpił nieoczekiwany błąd. Spróbuj ponownie.";
    // Logowanie błędu do konsoli
}
finally
{
    isLoading = false;
}
```

## 8. Interakcje użytkownika

### 8.1 Wypełnienie formularza
**Akcja:** Użytkownik wpisuje email i hasło  
**Efekt:**
- Aktualizacja `model.Email` i `model.Password`
- Walidacja na bieżąco (inline) jeśli pole było już touched
- Komunikaty błędów walidacji pod polami

### 8.2 Kliknięcie przycisku "Zaloguj"
**Akcja:** Użytkownik klika przycisk "Zaloguj" lub naciska Enter  
**Warunek:** Formularz musi być poprawnie zwalidowany  
**Efekt:**
- `isLoading = true` (przycisk disabled, pokazuje spinner)
- `errorMessage = null` (czyszczenie poprzednich błędów)
- Wywołanie `HandleLoginAsync()`
- Po sukcesie: przekierowanie na `/trips`
- Po błędzie: wyświetlenie `MudAlert` z komunikatem błędu

### 8.3 Kliknięcie linku "Zarejestruj się"
**Akcja:** Użytkownik klika link "Nie masz konta? Zarejestruj się"  
**Efekt:** Przekierowanie na `/register`

### 8.4 Autofocus
**Akcja:** Załadowanie strony  
**Efekt:** Automatyczne ustawienie fokusa na polu Email (`@ref`, `FocusAsync()` w `OnAfterRenderAsync`)

## 9. Warunki i walidacja

### 9.1 Walidacja pola Email

**Komponent:** `MudTextField` (Email)

**Warunki:**
- `Required` - pole nie może być puste
- `EmailAddress` - musi być poprawny format email (zawiera @, domenę)
- `MaxLength(255)` - max 255 znaków

**Komunikaty błędów:**
- Puste pole: "Email jest wymagany"
- Nieprawidłowy format: "Nieprawidłowy format email"
- Za długi: "Email nie może przekraczać 255 znaków"

**Wpływ na UI:**
- Czerwony border pola przy błędzie
- Komunikat błędu pod polem
- Przycisk "Zaloguj" disabled jeśli walidacja nie przechodzi

### 9.2 Walidacja pola Hasło

**Komponent:** `MudTextField` (Password)

**Warunki:**
- `Required` - pole nie może być puste
- `MinLength(8)` - min 8 znaków
- `MaxLength(100)` - max 100 znaków

**Komunikaty błędów:**
- Puste pole: "Hasło jest wymagane"
- Za krótkie: "Hasło musi mieć minimum 8 znaków"
- Za długie: "Hasło nie może przekraczać 100 znaków"

**Wpływ na UI:**
- Czerwony border pola przy błędzie
- Komunikat błędu pod polem
- Przycisk "Zaloguj" disabled jeśli walidacja nie przechodzi

### 9.3 Walidacja formularza (przed submitem)

**Metoda:** `await form.Validate()`

**Warunek:** `form.IsValid == true`

**Efekt:**
- Jeśli `IsValid == false`: formularz nie zostaje wysłany, błędy wyświetlone inline
- Jeśli `IsValid == true`: wywołanie `AuthService.LoginAsync()`

### 9.4 Błędy AuthException (z backendu)

**Przykładowe komunikaty:**
- "Nieprawidłowe dane logowania"
- "Konto zostało zablokowane"
- "Zbyt wiele prób logowania. Spróbuj później."

**Wyświetlenie:**
- `MudAlert(Severity.Error)` nad formularzem
- Tekst: `{errorMessage}`
- Kolor: czerwony

## 10. Obsługa błędów

### 10.1 AuthException (błędy uwierzytelnienia)

**Scenariusze:**
- Nieprawidłowy email lub hasło
- Konto nieaktywne/zablokowane
- Problemy z Supabase Auth

**Obsługa:**
```csharp
catch (AuthException ex)
{
    errorMessage = ex.Message;
    isLoading = false;
}
```

**UI:**
- `MudAlert(Severity.Error)` nad formularzem z tekstem błędu
- Pozostawienie wartości w polach (użytkownik może poprawić)
- Przycisk "Zaloguj" aktywny ponownie

### 10.2 ValidationException (błędy walidacji)

**Scenariusze:**
- Walidacja po stronie serwera nie przeszła (mimo walidacji klienta)

**Obsługa:**
```csharp
catch (ValidationException ex)
{
    errorMessage = ex.Message;
    isLoading = false;
}
```

**UI:**
- Podobnie jak AuthException
- Rzadki przypadek (klient waliduje przed wysłaniem)

### 10.3 Błędy sieciowe / Supabase niedostępny

**Scenariusze:**
- Brak połączenia z internetem
- Supabase API nie odpowiada
- Timeout

**Obsługa:**
```csharp
catch (Exception ex)
{
    errorMessage = "Wystąpił problem z połączeniem. Sprawdź internet i spróbuj ponownie.";
    isLoading = false;
    // Logowanie pełnego błędu do konsoli dla debugowania
}
```

**UI:**
- `MudAlert(Severity.Error)` z ogólnym komunikatem
- Sugestia sprawdzenia połączenia internetowego

### 10.4 Edge cases

**Użytkownik już zalogowany:**
- Ochrona przez routing: zalogowany użytkownik przekierowany z `/login` na `/trips`
- Implementacja w `OnInitializedAsync()`:
  ```csharp
  if (await AuthService.IsAuthenticatedAsync())
  {
      NavigationManager.NavigateTo("/trips");
  }
  ```

**Enter w polu formularza:**
- MudForm automatycznie obsługuje Enter → submit
- Walidacja działa tak samo jak przy kliknięciu przycisku

## 11. Kroki implementacji

### Krok 1: Utworzenie pliku komponentu
- Utwórz plik `MotoNomad.App/Pages/Login.razor`
- Ustaw routing: `@page "/login"`
- Dodaj dyrektywy `@inject` dla IAuthService, NavigationManager, ISnackbar

### Krok 2: Implementacja ViewModelu i stanu
- Zdefiniuj klasę `LoginViewModel` z polami Email i Password
- Zadeklaruj zmienne stanu: `form`, `model`, `isLoading`, `errorMessage`
- Zainicjalizuj `model = new LoginViewModel()`

### Krok 3: Utworzenie struktury UI (MudBlazor)
- Dodaj `MudContainer(MaxWidth.Small)` jako główny kontener
- Wewnątrz kontenera: `MudCard` z Elevation="5"
- Dodaj `MudCardHeader` z tytułem "Zaloguj się"
- Utwórz `MudCardContent` z warunkiem wyświetlania `MudAlert` jeśli `errorMessage != null`

### Krok 4: Implementacja formularza
- Dodaj `MudForm` z `@ref="form"` i `@bind-IsValid="formValid"`
- Utwórz `MudTextField` dla Email:
  - `@bind-Value="model.Email"`
  - `Label="Email"`
  - `Required="true"`
  - `Validation="@(new EmailAddressAttribute())"`
- Utwórz `MudTextField` dla Hasło:
  - `@bind-Value="model.Password"`
  - `Label="Hasło"`
  - `InputType="InputType.Password"`
  - `Required="true"`
  - `Validation="@(new MinLengthAttribute(8))"`

### Krok 5: Implementacja przycisku logowania
- Dodaj `MudCardActions` z `MudButton`:
  - `ButtonType="ButtonType.Submit"`
  - `Variant="Variant.Filled"`
  - `Color="Color.Primary"`
  - `OnClick="HandleLoginAsync"`
  - `Disabled="@(isLoading || !formValid)"`
- Wewnątrz przycisku:
  - Warunkowo: `<MudProgressCircular>` jeśli `isLoading`
  - Tekst: "Zaloguj" jeśli `!isLoading`

### Krok 6: Dodanie linku do rejestracji
- Dodaj drugą sekcję `MudCardActions` (Class="justify-center")
- Wewnątrz: `MudLink` z href="/register" i tekstem "Nie masz konta? Zarejestruj się"

### Krok 7: Implementacja metody HandleLoginAsync()
```csharp
private async Task HandleLoginAsync()
{
    // Walidacja formularza
    await form.Validate();
    if (!form.IsValid) return;

    // Rozpoczęcie ładowania
    isLoading = true;
    errorMessage = null;

    try
    {
        // Utworzenie komendy
        var command = new LoginCommand
        {
            Email = model.Email.Trim(),
            Password = model.Password
        };

        // Wywołanie API
        var user = await AuthService.LoginAsync(command);

        // Sukces
        Snackbar.Add("Zalogowano pomyślnie!", Severity.Success);
        NavigationManager.NavigateTo("/trips");
    }
    catch (AuthException ex)
    {
        errorMessage = ex.Message;
    }
    catch (Exception ex)
    {
        errorMessage = "Wystąpił nieoczekiwany błąd. Spróbuj ponownie.";
        // TODO: Logowanie błędu
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

### Krok 8: Implementacja autofocus
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await emailField.FocusAsync();
    }
}
```

### Krok 9: Implementacja przekierowania zalogowanego użytkownika
```csharp
protected override async Task OnInitializedAsync()
{
    if (await AuthService.IsAuthenticatedAsync())
    {
        NavigationManager.NavigateTo("/trips");
    }
}
```

### Krok 10: Stylizacja i finalizacja
- Dodaj style w sekcji `<style>` jeśli potrzeba (np. max-width formularza)
- Przetestuj responsywność na mobile i desktop
- Sprawdź działanie keyboard navigation (Tab, Enter)

### Krok 11: Testy
- Przetestuj poprawne logowanie (prawidłowe dane)
- Przetestuj błędne logowanie (nieprawidłowe hasło, nieistniejący email)
- Przetestuj walidację (puste pola, nieprawidłowy format email, za krótkie hasło)
- Przetestuj scenariusze błędów sieciowych
- Przetestuj przekierowanie już zalogowanego użytkownika
- Przetestuj dostępność (keyboard navigation, screen reader)

### Krok 12: Dokumentacja
- Dodaj komentarze XML dla publicznych metod
- Udokumentuj nietypowe rozwiązania w kodzie
- Zaktualizuj README jeśli trzeba