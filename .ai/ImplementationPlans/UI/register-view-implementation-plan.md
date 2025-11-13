# Plan implementacji widoku Register

## 1. Przegląd

Widok Register umożliwia utworzenie nowego konta użytkownika w aplikacji MotoNomad. Jest to publiczna strona dostępna dla niezalogowanych użytkowników. Po poprawnej rejestracji użytkownik zostaje przekierowany na stronę logowania (`/login`), gdzie może się zalogować używając nowo utworzonych danych. Widok zawiera formularz rejestracyjny z polami email, hasło oraz opcjonalnie displayName, obsługę błędów walidacji i uwierzytelnienia oraz link do strony logowania.

## 2. Routing widoku

**Ścieżka:** `/register`

**Dostępność:** Publiczna (dostępna dla niezalogowanych użytkowników)

**Routing w App.razor:**
```csharp
<RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
```

**Przekierowanie po rejestracji:** `/login` (użytkownik musi się zalogować ręcznie)

## 3. Struktura komponentów

```
Register.razor (Strona główna)
├── MudContainer
│   └── MudCard
│       ├── MudCardHeader
│       │   └── MudText (Tytuł: "Zarejestruj się")
│       ├── MudCardContent
│       │   ├── MudAlert (błędy AuthException) [warunkowo]
│       │   ├── MudForm (@ref="form")
│       │   │   ├── MudTextField (Email)
│       │   │   ├── MudTextField (Hasło, type="password")
│       │   │   ├── MudTextField (Powtórz hasło, type="password")
│       │   │   └── MudTextField (Nazwa wyświetlana, opcjonalne)
│       │   └── MudProgressCircular [podczas ładowania]
│       ├── MudCardActions
│       │   └── MudButton ("Zarejestruj", Primary)
│       └── MudCardActions (secondary)
│           └── MudLink ("Masz już konto? Zaloguj się", href="/login")
```

## 4. Szczegóły komponentów

### Register.razor (Komponent strony)

**Opis komponentu:**
Główny komponent strony rejestracji zawierający kompletny formularz tworzenia konta. Obsługuje walidację danych wejściowych (w tym zgodność haseł), komunikację z API, wyświetlanie błędów, automatyczne logowanie i przekierowanie po sukcesie.

**Główne elementy:**
- `MudContainer` (MaxWidth.Small) - kontener centrujący formularz
- `MudCard` - karta zawierająca cały formularz rejestracji
- `MudCardHeader` - nagłówek z tytułem "Zarejestruj się"
- `MudCardContent` - zawartość z formularzem i alertami
- `MudAlert` (Severity.Error, warunkowo) - wyświetla błędy AuthException i ValidationException
- `MudAlert` (Severity.Warning, warunkowo) - inline błędy walidacji dla konkretnych pól
- `MudForm` - formularz z referencją dla walidacji
- `MudTextField` (Email) - pole email z walidacją formatu
- `MudTextField` (Hasło) - pole hasła z type="password", min 8 znaków
- `MudTextField` (Powtórz hasło) - pole potwierdzenia hasła z walidacją zgodności
- `MudTextField` (Nazwa wyświetlana) - opcjonalne pole dla displayName
- `MudProgressCircular` (warunkowo) - spinner podczas rejestracji
- `MudButton` (Primary) - przycisk "Zarejestruj"
- `MudLink` - link do strony logowania

**Obsługiwane zdarzenia:**
- `OnValidSubmit` formularza - wywołuje metodę `HandleRegisterAsync()`
- `OnClick` przycisku "Zarejestruj" - submit formularza (jeśli walidacja OK)
- `@bind-Value` dla każdego pola - aktualizacja ViewModelu

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
- **Powtórz hasło:**
  - Wymagane pole (Required)
  - Musi być identyczne z polem Hasło
  - Komunikat błędu: "Hasła muszą być identyczne"
- **Nazwa wyświetlana:**
  - Opcjonalne pole
  - Max 100 znaków
  - Komunikat błędu: "Nazwa nie może przekraczać 100 znaków"

**Typy:**
- `RegisterCommand` (request)
- `UserDto` (response)
- `AuthException` (błędy uwierzytelnienia - np. email zajęty)
- `ValidationException` (błędy walidacji)

**Propsy:**
Brak (komponent strony najwyższego poziomu)

**Parametry wstrzykiwane (Dependency Injection):**
- `[Inject] IAuthService AuthService { get; set; }`
- `[Inject] NavigationManager NavigationManager { get; set; }`
- `[Inject] ISnackbar Snackbar { get; set; }`

## 5. Typy

### RegisterCommand (Request DTO)
```csharp
public record RegisterCommand
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? DisplayName { get; init; }
}
```

**Pola:**
- `Email` (string, required) - adres email użytkownika
- `Password` (string, required) - hasło użytkownika
- `DisplayName` (string?, opcjonalne) - nazwa wyświetlana

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
private class RegisterViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
```

**Pola:**
- `Email` (string) - email wprowadzony przez użytkownika
- `Password` (string) - hasło
- `ConfirmPassword` (string) - potwierdzenie hasła (tylko dla walidacji UI)
- `DisplayName` (string?, opcjonalne) - nazwa wyświetlana

## 6. Zarządzanie stanem

**Zmienne stanu komponentu:**

```csharp
private MudForm form = null!;
private RegisterViewModel model = new();
private bool isLoading = false;
private string? errorMessage = null;
```

**Opis zmiennych:**
- `form` - referencja do MudForm dla ręcznej walidacji
- `model` - ViewModel zawierający dane formularza (email, hasło, confirmPassword, displayName)
- `isLoading` - flaga stanu ładowania (true podczas wywołania API)
- `errorMessage` - komunikat błędu z AuthException lub ValidationException (null jeśli brak błędu)

**Przepływ stanu:**
1. Użytkownik wypełnia pola → aktualizacja `model`
2. Kliknięcie "Zarejestruj" → walidacja formularza (w tym zgodność haseł)
3. Jeśli walidacja OK → `isLoading = true`, `errorMessage = null`
4. Wywołanie `AuthService.RegisterAsync(command)`
5. Sukces → przekierowanie na `/login`
6. Błąd → `errorMessage = ex.Message`, `isLoading = false`

**Brak potrzeby custom hooka** - stan zarządzany lokalnie w komponencie.

## 7. Integracja API

**Endpoint:** `IAuthService.RegisterAsync(RegisterCommand command)`

**Typ żądania:** `RegisterCommand`
```csharp
new RegisterCommand 
{ 
    Email = model.Email.Trim(), 
    Password = model.Password,
    DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) 
        ? null 
        : model.DisplayName.Trim()
}
```

**Typ odpowiedzi:** `Task<UserDto>`

**Obsługa sukcesu:**
```csharp
var user = await AuthService.RegisterAsync(command);
Snackbar.Add($"Witaj, {user.DisplayName ?? user.Email}! Konto zostało utworzone. Zaloguj się.", Severity.Success);
// Użytkownik musi się ręcznie zalogować (nie jest automatycznie logowany po rejestracji)
NavigationManager.NavigateTo("/login");
```

**Obsługa błędów:**
```csharp
catch (AuthException ex)
{
    // Błędy typu: email zajęty, problemy z Supabase Auth
    errorMessage = ex.Message;
}
catch (ValidationException ex)
{
    // Błędy walidacji po stronie serwera
    errorMessage = ex.Message;
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
**Akcja:** Użytkownik wpisuje email, hasło, potwierdzenie hasła i opcjonalnie nazwę  
**Efekt:**
- Aktualizacja `model.Email`, `model.Password`, `model.ConfirmPassword`, `model.DisplayName`
- Walidacja na bieżąco (inline) jeśli pole było już touched
- Komunikaty błędów walidacji pod polami
- Specjalna walidacja dla pola "Powtórz hasło" - sprawdzenie czy `ConfirmPassword == Password`

### 8.2 Kliknięcie przycisku "Zarejestruj"
**Akcja:** Użytkownik klika przycisk "Zarejestruj" lub naciska Enter  
**Warunek:** Formularz musi być poprawnie zwalidowany (w tym zgodność haseł)  
**Efekt:**
- `isLoading = true` (przycisk disabled, pokazuje spinner)
- `errorMessage = null` (czyszczenie poprzednich błędów)
- Wywołanie `HandleRegisterAsync()`
- Po sukcesie: Snackbar z komunikatem + przekierowanie na `/login` (użytkownik musi się zalogować ręcznie)
- Po błędzie: wyświetlenie `MudAlert` z komunikatem błędu

### 8.3 Kliknięcie linku "Zaloguj się"
**Akcja:** Użytkownik klika link "Masz już konto? Zaloguj się"  
**Efekt:** Przekierowanie na `/login`

### 8.4 Autofocus
**Akcja:** Załadowanie strony  
**Efekt:** Automatyczne ustawienie fokusa na polu Email (`@ref`, `FocusAsync()` w `OnAfterRenderAsync`)

### 8.5 Walidacja zgodności haseł (real-time)
**Akcja:** Użytkownik wpisuje znaki w polu "Powtórz hasło"  
**Efekt:**
- Na bieżąco sprawdzanie czy `ConfirmPassword == Password`
- Jeśli różne: komunikat błędu "Hasła muszą być identyczne" pod polem
- Jeśli identyczne: brak błędu, zielony check (opcjonalnie)

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
- Przycisk "Zarejestruj" disabled jeśli walidacja nie przechodzi

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
- Opcjonalnie: Wskaźnik siły hasła (future enhancement)

### 9.3 Walidacja pola Powtórz hasło

**Komponent:** `MudTextField` (ConfirmPassword)

**Warunki:**
- `Required` - pole nie może być puste
- Custom validation: `ConfirmPassword == Password`

**Komunikaty błędów:**
- Puste pole: "Potwierdzenie hasła jest wymagane"
- Niezgodność: "Hasła muszą być identyczne"

**Implementacja custom validation:**
```csharp
private Func<string, string?> MatchPasswordValidation => (value) =>
{
    if (string.IsNullOrEmpty(value))
        return "Potwierdzenie hasła jest wymagane";
    
    if (value != model.Password)
        return "Hasła muszą być identyczne";
    
    return null;
};
```

**Wpływ na UI:**
- Czerwony border pola przy błędzie
- Komunikat błędu pod polem
- Walidacja w czasie rzeczywistym przy wpisywaniu

### 9.4 Walidacja pola Nazwa wyświetlana

**Komponent:** `MudTextField` (DisplayName)

**Warunki:**
- Pole opcjonalne (brak Required)
- `MaxLength(100)` - max 100 znaków

**Komunikaty błędów:**
- Za długa: "Nazwa nie może przekraczać 100 znaków"

**Wpływ na UI:**
- Komunikat błędu pod polem jeśli za długa
- Brak blokowania submitu jeśli puste (pole opcjonalne)

### 9.5 Walidacja formularza (przed submitem)

**Metoda:** `await form.Validate()`

**Warunek:** `form.IsValid == true`

**Efekt:**
- Jeśli `IsValid == false`: formularz nie zostaje wysłany, błędy wyświetlone inline
- Jeśli `IsValid == true`: wywołanie `AuthService.RegisterAsync()`

### 9.6 Błędy AuthException (z backendu)

**Przykładowe komunikaty:**
- "Email jest już zajęty" (najczęstszy błąd)
- "Hasło za słabe" (jeśli Supabase ma dodatkowe wymagania)
- "Nie można utworzyć konta. Spróbuj ponownie."

**Wyświetlenie:**
- `MudAlert(Severity.Error)` nad formularzem
- Tekst: `{errorMessage}`
- Kolor: czerwony

## 10. Obsługa błędów

### 10.1 AuthException (błędy rejestracji)

**Scenariusze:**
- Email już zajęty (najczęstszy przypadek)
- Hasło nie spełnia wymagań Supabase Auth
- Problemy z połączeniem do Supabase

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
- Przycisk "Zarejestruj" aktywny ponownie
- Focus na polu Email jeśli błąd dotyczy adresu

### 10.2 ValidationException (błędy walidacji serwera)

**Scenariusze:**
- Walidacja po stronie serwera nie przeszła (mimo walidacji klienta)
- Nieprawidłowe znaki w nazwie wyświetlanej

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
- `MudAlert(Severity.Warning)` jeśli błąd dotyczy konkretnego pola

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
- Ochrona przez routing: zalogowany użytkownik przekierowany z `/register` na `/trips`
- Implementacja w `OnInitializedAsync()`:
  ```csharp
  if (await AuthService.IsAuthenticatedAsync())
  {
      NavigationManager.NavigateTo("/trips");
  }
  ```

**Email z białymi znakami:**
- Automatyczne `Trim()` przed wysłaniem do API
- Walidacja formatu email wyłapie nieprawidłowe wartości

**DisplayName puste (whitespace):**
- Jeśli `string.IsNullOrWhiteSpace(model.DisplayName)` → wysyłane jako `null`
- Backend akceptuje `null` dla opcjonalnego pola

**Hasło widoczne przez przypadek:**
- Pole typu `InputType.Password` (gwiazdki/kropki)
- Opcjonalnie: przycisk "Pokaż hasło" (toggle visibility) - future enhancement

## 11. Kroki implementacji

### Krok 1: Utworzenie pliku komponentu
- Utwórz plik `MotoNomad.App/Pages/Register.razor`
- Ustaw routing: `@page "/register"`
- Dodaj dyrektywy `@inject` dla IAuthService, NavigationManager, ISnackbar

### Krok 2: Implementacja ViewModelu i stanu
- Zdefiniuj klasę `RegisterViewModel` z polami Email, Password, ConfirmPassword, DisplayName
- Zadeklaruj zmienne stanu: `form`, `model`, `isLoading`, `errorMessage`
- Zainicjalizuj `model = new RegisterViewModel()`

### Krok 3: Utworzenie struktury UI (MudBlazor)
- Dodaj `MudContainer(MaxWidth.Small)` jako główny kontener
- Wewnątrz kontenera: `MudCard` z Elevation="5"
- Dodaj `MudCardHeader` z tytułem "Zarejestruj się"
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
- Utwórz `MudTextField` dla Powtórz hasło:
  - `@bind-Value="model.ConfirmPassword"`
  - `Label="Powtórz hasło"`
  - `InputType="InputType.Password"`
  - `Required="true"`
  - `Validation="@MatchPasswordValidation"`
- Utwórz `MudTextField` dla Nazwa wyświetlana:
  - `@bind-Value="model.DisplayName"`
  - `Label="Nazwa wyświetlana (opcjonalnie)"`
  - `Required="false"`
  - `Validation="@(new MaxLengthAttribute(100))"`

### Krok 5: Implementacja custom validation dla zgodności haseł
```csharp
private Func<string, string?> MatchPasswordValidation => (value) =>
{
    if (string.IsNullOrEmpty(value))
        return "Potwierdzenie hasła jest wymagane";
    
    if (value != model.Password)
        return "Hasła muszą być identyczne";
    
    return null;
};
```

### Krok 6: Implementacja przycisku rejestracji
- Dodaj `MudCardActions` z `MudButton`:
  - `ButtonType="ButtonType.Submit"`
  - `Variant="Variant.Filled"`
  - `Color="Color.Primary"`
  - `OnClick="HandleRegisterAsync"`
  - `Disabled="@(isLoading || !formValid)"`
- Wewnątrz przycisku:
  - Warunkowo: `<MudProgressCircular>` jeśli `isLoading`
  - Tekst: "Zarejestruj" jeśli `!isLoading`

### Krok 7: Dodanie linku do logowania
- Dodaj drugą sekcję `MudCardActions` (Class="justify-center")
- Wewnątrz: `MudLink` z href="/login" i tekstem "Masz już konto? Zaloguj się"

### Krok 8: Implementacja metody HandleRegisterAsync()
```csharp
private async Task HandleRegisterAsync()
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
        var command = new RegisterCommand
        {
            Email = model.Email.Trim(),
            Password = model.Password,
            DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) 
                ? null 
                : model.DisplayName.Trim()
        };

        // Wywołanie API
        var user = await AuthService.RegisterAsync(command);

        // Sukces - użytkownik przekierowany na stronę logowania (musi się zalogować ręcznie)
        Snackbar.Add($"Witaj, {user.DisplayName ?? user.Email}! Konto zostało utworzone. Zaloguj się.", Severity.Success);
        NavigationManager.NavigateTo("/login");
    }
    catch (AuthException ex)
    {
        errorMessage = ex.Message;
    }
    catch (ValidationException ex)
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

### Krok 9: Implementacja autofocus
```csharp
private ElementReference emailField;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await emailField.FocusAsync();
    }
}
```

### Krok 10: Implementacja przekierowania zalogowanego użytkownika
```csharp
protected override async Task OnInitializedAsync()
{
    if (await AuthService.IsAuthenticatedAsync())
    {
        NavigationManager.NavigateTo("/trips");
    }
}
```

### Krok 11: Dodanie podpowiedzi dla pola hasła (opcjonalnie)
- Dodaj `MudText` (Typo.caption) pod polem Hasło z tekstem:
  - "Hasło musi mieć minimum 8 znaków"
- Zmień kolor na Color.Info lub Color.Default

### Krok 12: Stylizacja i finalizacja
- Dodaj style w sekcji `<style>` jeśli potrzeba
- Upewnij się, że formularz jest responsywny (mobile + desktop)
- Sprawdź działanie keyboard navigation (Tab, Enter)

### Krok 13: Testy
- Przetestuj poprawną rejestrację (wszystkie pola wypełnione)
- Przetestuj rejestrację bez opcjonalnej nazwy
- Przetestuj błąd "email zajęty" (próba rejestracji z istniejącym emailem)
- Przetestuj walidację:
  - Puste pola wymagane
  - Nieprawidłowy format email
  - Hasło za krótkie
  - Niezgodność haseł (Password != ConfirmPassword)
  - Nazwa za długa
- Przetestuj scenariusze błędów sieciowych
- Przetestuj przekierowanie już zalogowanego użytkownika
- Przetestuj przekierowanie na /login po pomyślnej rejestracji
- Przetestuj ręczne logowanie po rejestracji
- Przetestuj dostępność (keyboard navigation, screen reader)

### Krok 14: Dokumentacja
- Dodaj komentarze XML dla publicznych metod
- Udokumentuj logikę walidacji zgodności haseł
- Zaktualizuj README jeśli trzeba