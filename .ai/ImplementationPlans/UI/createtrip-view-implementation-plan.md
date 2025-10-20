# Plan implementacji widoku CreateTrip

## 1. Przegląd

Widok CreateTrip (`/trip/create`) umożliwia użytkownikowi utworzenie nowej wycieczki z podstawowymi danymi: nazwa, daty rozpoczęcia i zakończenia, opis oraz rodzaj transportu. Widok wykorzystuje reużywalny komponent TripForm.razor w trybie tworzenia (bez danych początkowych). Po poprawnym utworzeniu wycieczki użytkownik zostaje przekierowany na stronę główną (`/trips`) z komunikatem sukcesu. Zgodnie z User Story US-003, cała operacja powinna zająć mniej niż 2 minuty.

## 2. Routing widoku

**Ścieżka:** `/trip/create`

**Dostępność:** Chroniona (wymagane uwierzytelnienie)

**Routing w App.razor:**
```csharp
<AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
    <NotAuthorized>
        <RedirectToLogin />
    </NotAuthorized>
</AuthorizeRouteView>
```

**Nawigacja do widoku:**
- Z `/trips` → Floating Action Button (+)
- Z `/trips` → EmptyState przycisk "Dodaj pierwszą wycieczkę"
- Z NavMenu → "Nowa wycieczka"

**Przekierowanie po sukcesie:** `/trips`

## 3. Struktura komponentów

```
CreateTrip.razor (Strona główna)
├── MudContainer
│   ├── MudText (Nagłówek: "Nowa wycieczka")
│   └── MudCard
│       ├── MudCardHeader
│       │   └── MudText (Tytuł: "Podstawowe informacje")
│       ├── MudCardContent
│       │   ├── MudAlert (błędy ValidationException/DatabaseException) [warunkowo]
│       │   └── TripForm.razor (komponent reużywalny)
│       └── MudCardActions
│           ├── MudButton ("Zapisz", Primary)
│           └── MudButton ("Anuluj", Secondary)
│
└── Komponenty potomne:
    └── TripForm.razor (formularz wycieczki, tryb create)
```

## 4. Szczegóły komponentów

### 4.1 CreateTrip.razor (Komponent strony)

**Opis komponentu:**
Główny komponent strony tworzenia wycieczki. Zawiera kontener MudCard z reużywalnym komponentem TripForm.razor. Zarządza stanem formularza, obsługuje submit (wywołanie API), wyświetla błędy oraz nawiguje po sukcesie do `/trips`.

**Główne elementy:**
- `MudContainer` (MaxWidth.Medium) - kontener centrujący
- `MudText` (Typo.h4) - nagłówek strony "Nowa wycieczka"
- `MudCard` - karta zawierająca formularz
- `MudCardHeader` - nagłówek karty "Podstawowe informacje"
- `MudCardContent` - zawartość z alertem błędów i formularzem
- `MudAlert` (Severity.Error, warunkowo) - wyświetla błędy ValidationException i DatabaseException
- `TripForm.razor` - reużywalny komponent formularza wycieczki
- `MudCardActions` - przyciski akcji (Zapisz, Anuluj)

**Obsługiwane zdarzenia:**
- `OnSubmit` (callback z TripForm) - wywołuje `HandleCreateTripAsync()`
- `OnCancel` (callback z TripForm) - nawigacja do `/trips`
- Kliknięcie "Zapisz" - walidacja + submit formularza
- Kliknięcie "Anuluj" - nawigacja do `/trips` bez zapisywania

**Warunki walidacji:**
Walidacja jest delegowana do komponentu TripForm.razor:
- **Nazwa:** Required, max 200 znaków
- **Data rozpoczęcia:** Required, valid date
- **Data zakończenia:** Required, musi być > StartDate
- **Opis:** Opcjonalne, max 2000 znaków
- **Transport:** Required, valid enum value

**Typy:**
- `CreateTripCommand` (request)
- `TripDetailDto` (response)
- `ValidationException` (błędy walidacji)
- `DatabaseException` (błędy bazy danych)

**Propsy:**
Brak (komponent strony najwyższego poziomu)

**Parametry wstrzykiwane (Dependency Injection):**
- `[Inject] ITripService TripService { get; set; }`
- `[Inject] NavigationManager NavigationManager { get; set; }`
- `[Inject] ISnackbar Snackbar { get; set; }`

### 4.2 TripForm.razor (Komponent formularza)

**Opis komponentu:**
Reużywalny komponent formularza wycieczki używany zarówno do tworzenia (`/trip/create`) jak i edycji (`/trip/{id}`). W trybie tworzenia pola są puste, w trybie edycji są wypełnione danymi z parametru `Trip`. Komponent obsługuje walidację wszystkich pól zgodnie z wymaganiami biznesowymi (EndDate > StartDate) i zwraca dane przez callbacki.

**Główne elementy:**
- `MudForm` (@ref="form") - formularz z referencją
- `MudTextField` (Nazwa) - pole tekstowe z walidacją
- `MudDatePicker` (Data rozpoczęcia) - wybór daty z walidacją
- `MudDatePicker` (Data zakończenia) - wybór daty z walidacją zgodności
- `MudTextField` (Opis) - wieloliniowe pole tekstowe, opcjonalne
- `MudSelect<TransportType>` (Rodzaj transportu) - dropdown z enumem
- Przyciski akcji (renderowane przez rodzica lub wewnątrz komponentu)

**Obsługiwane zdarzenia:**
- `@bind-Value` dla każdego pola - aktualizacja ViewModelu
- `OnValidSubmit` - wywołanie callbacku `OnSubmit`
- Custom validation dla dat (EndDate > StartDate)

**Warunki walidacji:**
- **Nazwa:**
  - Required: "Nazwa wycieczki jest wymagana"
  - MaxLength(200): "Nazwa nie może przekraczać 200 znaków"
  - Trim przed submitem
- **Data rozpoczęcia:**
  - Required: "Data rozpoczęcia jest wymagana"
  - Valid date: "Nieprawidłowa data"
- **Data zakończenia:**
  - Required: "Data zakończenia jest wymagana"
  - Valid date: "Nieprawidłowa data"
  - Custom validation: EndDate > StartDate: "Data zakończenia musi być późniejsza niż data rozpoczęcia"
- **Opis:**
  - Opcjonalne (brak Required)
  - MaxLength(2000): "Opis nie może przekraczać 2000 znaków"
  - Trim przed submitem
- **Transport:**
  - Required: "Rodzaj transportu jest wymagany"
  - Valid enum: wartość 0-4

**Typy:**
- `TripDetailDto?` (opcjonalne - dla trybu edycji)
- `CreateTripCommand` lub `UpdateTripCommand` (wewnętrzny ViewModel)

**Propsy:**
```csharp
[Parameter] public TripDetailDto? Trip { get; set; }
[Parameter] public EventCallback<CreateTripCommand> OnSubmit { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public bool IsLoading { get; set; }
```

## 5. Typy

### CreateTripCommand (Request DTO)
```csharp
public record CreateTripCommand
{
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
}
```

**Pola:**
- `Name` (string, required) - nazwa wycieczki, trim przed wysłaniem
- `StartDate` (DateOnly, required) - data rozpoczęcia
- `EndDate` (DateOnly, required) - data zakończenia, musi być > StartDate
- `Description` (string?, opcjonalne) - opis wycieczki, trim przed wysłaniem
- `TransportType` (enum, required) - rodzaj transportu (0-4)

### TripDetailDto (Response DTO)
```csharp
public record TripDetailDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
    public required int DurationDays { get; init; }
    public required List<CompanionListItemDto> Companions { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
```

**Pola:**
- `Id` (Guid) - unikalny identyfikator wycieczki (wygenerowany przez backend)
- `UserId` (Guid) - ID właściciela wycieczki
- `Name` (string) - nazwa wycieczki
- `StartDate` (DateOnly) - data rozpoczęcia
- `EndDate` (DateOnly) - data zakończenia
- `Description` (string?, opcjonalne) - opis wycieczki
- `TransportType` (enum) - rodzaj transportu
- `DurationDays` (int) - czas trwania w dniach (obliczony przez backend)
- `Companions` (lista) - lista towarzyszy (pusta przy tworzeniu)
- `CreatedAt` (DateTime) - data utworzenia (UTC)
- `UpdatedAt` (DateTime) - data ostatniej modyfikacji (UTC)

### TransportType (Enum)
```csharp
public enum TransportType
{
    Motorcycle = 0,  // Motocykl
    Airplane = 1,    // Samolot
    Train = 2,       // Pociąg
    Car = 3,         // Samochód
    Other = 4        // Inne
}
```

### TripFormViewModel (wewnętrzny ViewModel TripForm)
```csharp
private class TripFormViewModel
{
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public TransportType TransportType { get; set; } = TransportType.Motorcycle;
}
```

**Uwaga:** MudDatePicker używa `DateTime?`, więc ViewModel używa `DateTime?` dla dat. Konwersja do `DateOnly` następuje przed utworzeniem komendy.

## 6. Zarządzanie stanem

**Zmienne stanu komponentu CreateTrip.razor:**

```csharp
private bool isLoading = false;
private string? errorMessage = null;
```

**Opis zmiennych:**
- `isLoading` - flaga stanu ładowania (true podczas wywołania API)
- `errorMessage` - komunikat błędu z ValidationException lub DatabaseException (null jeśli brak błędu)

**Zmienne stanu komponentu TripForm.razor:**

```csharp
private MudForm form = null!;
private TripFormViewModel model = new();
```

**Opis zmiennych:**
- `form` - referencja do MudForm dla ręcznej walidacji
- `model` - ViewModel zawierający dane formularza (nazwa, daty, opis, transport)

**Przepływ stanu:**

1. **Inicjalizacja:**
   - CreateTrip.razor: `isLoading = false`, `errorMessage = null`
   - TripForm.razor: `model = new TripFormViewModel()` (puste pola)

2. **Wypełnianie formularza:**
   - Użytkownik wpisuje dane → aktualizacja `model` w TripForm
   - Walidacja inline (MudForm)

3. **Submit formularza:**
   - TripForm wywołuje callback `OnSubmit` z `CreateTripCommand`
   - CreateTrip.razor: `isLoading = true`, `errorMessage = null`
   - Wywołanie `TripService.CreateTripAsync(command)`

4. **Sukces:**
   - Snackbar "Wycieczka została utworzona!"
   - Nawigacja do `/trips`

5. **Błąd:**
   - `errorMessage = ex.Message`
   - `isLoading = false`
   - Wyświetlenie MudAlert w CreateTrip.razor

6. **Anulowanie:**
   - TripForm wywołuje callback `OnCancel`
   - CreateTrip.razor: Nawigacja do `/trips` (bez zapisywania)

**Brak potrzeby custom hooka** - stan zarządzany lokalnie w komponentach.

## 7. Integracja API

**Endpoint:** `ITripService.CreateTripAsync(CreateTripCommand command)`

**Typ żądania:** `CreateTripCommand`
```csharp
new CreateTripCommand
{
    Name = model.Name.Trim(),
    StartDate = DateOnly.FromDateTime(model.StartDate!.Value),
    EndDate = DateOnly.FromDateTime(model.EndDate!.Value),
    Description = string.IsNullOrWhiteSpace(model.Description) 
        ? null 
        : model.Description.Trim(),
    TransportType = model.TransportType
}
```

**Typ odpowiedzi:** `Task<TripDetailDto>`

**Obsługa sukcesu:**
```csharp
var trip = await TripService.CreateTripAsync(command);
Snackbar.Add($"Wycieczka '{trip.Name}' została utworzona!", Severity.Success);
NavigationManager.NavigateTo("/trips");
```

**Obsługa błędów:**
```csharp
catch (UnauthorizedException)
{
    // Sesja wygasła
    Snackbar.Add("Sesja wygasła. Zaloguj się ponownie.", Severity.Warning);
    NavigationManager.NavigateTo("/login");
}
catch (ValidationException ex)
{
    // Błędy walidacji z backendu (np. EndDate <= StartDate mimo walidacji klienta)
    errorMessage = ex.Message;
}
catch (DatabaseException ex)
{
    // Błędy bazy danych
    errorMessage = "Nie udało się utworzyć wycieczki. Spróbuj ponownie.";
    // Logowanie szczegółów błędu
}
catch (Exception ex)
{
    errorMessage = "Wystąpił nieoczekiwany błąd. Spróbuj ponownie.";
    // Logowanie błędu
}
finally
{
    isLoading = false;
}
```

## 8. Interakcje użytkownika

### 8.1 Wypełnienie formularza
**Akcja:** Użytkownik wpisuje dane w polach formularza  
**Efekt:**
- Aktualizacja `model` w TripForm.razor
- Walidacja inline (jeśli pole było touched)
- Komunikaty błędów walidacji pod polami
- Autofocus na polu "Nazwa" przy załadowaniu strony

### 8.2 Wybór daty rozpoczęcia
**Akcja:** Użytkownik otwiera MudDatePicker i wybiera datę  
**Efekt:**
- Aktualizacja `model.StartDate`
- Jeśli `EndDate` już wybrana: walidacja warunku `EndDate > StartDate`
- Format wyświetlania: dd.MM.yyyy

### 8.3 Wybór daty zakończenia
**Akcja:** Użytkownik otwiera MudDatePicker i wybiera datę  
**Efekt:**
- Aktualizacja `model.EndDate`
- Walidacja warunku `EndDate > StartDate`
- Jeśli warunek nie spełniony: komunikat błędu "Data zakończenia musi być późniejsza niż data rozpoczęcia"

### 8.4 Wybór rodzaju transportu
**Akcja:** Użytkownik otwiera MudSelect i wybiera transport  
**Efekt:**
- Aktualizacja `model.TransportType`
- Wyświetlenie wybranej wartości (Motocykl, Samolot, Pociąg, Samochód, Inne)

### 8.5 Kliknięcie przycisku "Zapisz"
**Akcja:** Użytkownik klika przycisk "Zapisz"  
**Warunek:** Formularz musi być poprawnie zwalidowany (wszystkie required pola wypełnione, EndDate > StartDate)  
**Efekt:**
- Walidacja formularza (`await form.Validate()`)
- Jeśli walidacja OK:
  - `isLoading = true` (przycisk disabled, spinner)
  - TripForm wywołuje callback `OnSubmit` z `CreateTripCommand`
  - CreateTrip wywołuje `TripService.CreateTripAsync()`
  - Po sukcesie: Snackbar + przekierowanie na `/trips`
  - Po błędzie: wyświetlenie MudAlert z komunikatem
- Jeśli walidacja NOK:
  - Formularz nie wysyłany
  - Błędy wyświetlone inline pod polami

### 8.6 Kliknięcie przycisku "Anuluj"
**Akcja:** Użytkownik klika przycisk "Anuluj"  
**Efekt:**
- TripForm wywołuje callback `OnCancel`
- CreateTrip: Nawigacja do `/trips` (bez zapisywania)
- Brak potwierdzenia (dane formularza są tracone)

### 8.7 Wpisywanie opisu (opcjonalnie)
**Akcja:** Użytkownik wpisuje opis w polu wieloliniowym  
**Efekt:**
- Aktualizacja `model.Description`
- Pole opcjonalne, brak walidacji required
- Walidacja MaxLength(2000) przy przekroczeniu limitu

## 9. Warunki i walidacja

### 9.1 Walidacja pola Nazwa

**Komponent:** `MudTextField` (TripForm.razor)

**Warunki:**
- `Required` - pole nie może być puste
- `MaxLength(200)` - max 200 znaków
- Trim przed submitem

**Komunikaty błędów:**
- Puste pole: "Nazwa wycieczki jest wymagana"
- Za długa: "Nazwa nie może przekraczać 200 znaków"

**Wpływ na UI:**
- Czerwony border pola przy błędzie
- Komunikat błędu pod polem
- Przycisk "Zapisz" disabled jeśli walidacja nie przechodzi

### 9.2 Walidacja pola Data rozpoczęcia

**Komponent:** `MudDatePicker` (TripForm.razor)

**Warunki:**
- `Required` - data musi być wybrana
- Valid date - nieprawidłowa data wyłapywana przez MudDatePicker

**Komunikaty błędów:**
- Brak daty: "Data rozpoczęcia jest wymagana"

**Format:** dd.MM.yyyy

**Wpływ na UI:**
- Czerwony border przy błędzie
- Komunikat błędu pod polem

### 9.3 Walidacja pola Data zakończenia

**Komponent:** `MudDatePicker` (TripForm.razor)

**Warunki:**
- `Required` - data musi być wybrana
- Valid date - nieprawidłowa data wyłapywana przez MudDatePicker
- **Custom validation**: `EndDate > StartDate`

**Komunikaty błędów:**
- Brak daty: "Data zakończenia jest wymagana"
- Nieprawidłowy zakres: "Data zakończenia musi być późniejsza niż data rozpoczęcia"

**Implementacja custom validation:**
```csharp
private Func<DateTime?, string?> ValidateEndDate => (endDate) =>
{
    if (!endDate.HasValue)
        return "Data zakończenia jest wymagana";
    
    if (!model.StartDate.HasValue)
        return null; // Brak walidacji jeśli StartDate nie wybrana
    
    if (endDate.Value <= model.StartDate.Value)
        return "Data zakończenia musi być późniejsza niż data rozpoczęcia";
    
    return null;
};
```

**Wpływ na UI:**
- Czerwony border przy błędzie
- Komunikat błędu pod polem
- Przycisk "Zapisz" disabled

### 9.4 Walidacja pola Opis

**Komponent:** `MudTextField` (TripForm.razor, multiline)

**Warunki:**
- Pole opcjonalne (brak Required)
- `MaxLength(2000)` - max 2000 znaków
- Trim przed submitem

**Komunikaty błędów:**
- Za długi: "Opis nie może przekraczać 2000 znaków"

**Wpływ na UI:**
- Komunikat błędu pod polem jeśli za długi
- Licznik znaków (opcjonalnie): "X / 2000"

### 9.5 Walidacja pola Rodzaj transportu

**Komponent:** `MudSelect<TransportType>` (TripForm.razor)

**Warunki:**
- `Required` - wartość musi być wybrana
- Valid enum value (0-4)

**Komunikaty błędów:**
- Brak wyboru: "Rodzaj transportu jest wymagany"

**Opcje:**
- Motocykl (0)
- Samolot (1)
- Pociąg (2)
- Samochód (3)
- Inne (4)

**Wpływ na UI:**
- Czerwony border przy błędzie
- Komunikat błędu pod polem

### 9.6 Walidacja formularza (przed submitem)

**Metoda:** `await form.Validate()`

**Warunek:** `form.IsValid == true`

**Efekt:**
- Jeśli `IsValid == false`: formularz nie zostaje wysłany, błędy wyświetlone inline
- Jeśli `IsValid == true`: wywołanie callbacku `OnSubmit` z `CreateTripCommand`

## 10. Obsługa błędów

### 10.1 ValidationException (błędy walidacji z backendu)

**Scenariusze:**
- Walidacja po stronie serwera nie przeszła (mimo walidacji klienta)
- Backend wykrył nieprawidłowe daty (np. przez błąd w konwersji DateTime → DateOnly)
- Nazwa zawiera niedozwolone znaki

**Obsługa:**
```csharp
catch (ValidationException ex)
{
    errorMessage = ex.Message;
    isLoading = false;
}
```

**UI:**
- `MudAlert(Severity.Error)` w CreateTrip.razor nad formularzem
- Tekst: `{errorMessage}`
- Pozostawienie wartości w polach (użytkownik może poprawić)
- Przycisk "Zapisz" aktywny ponownie

### 10.2 UnauthorizedException (sesja wygasła)

**Scenariusze:**
- Sesja użytkownika wygasła podczas wypełniania formularza
- Token JWT nieważny

**Obsługa:**
```csharp
catch (UnauthorizedException)
{
    Snackbar.Add("Sesja wygasła. Zaloguj się ponownie.", Severity.Warning);
    NavigationManager.NavigateTo("/login");
}
```

**UI:**
- Snackbar z komunikatem ostrzeżenia
- Automatyczne przekierowanie na `/login`
- Dane formularza są tracone

### 10.3 DatabaseException (błąd zapisu do bazy)

**Scenariusze:**
- Problemy z połączeniem do Supabase
- Timeout zapytania SQL
- Naruszenie constraints (rzadkie - RLS już sprawdzony)

**Obsługa:**
```csharp
catch (DatabaseException ex)
{
    errorMessage = "Nie udało się utworzyć wycieczki. Spróbuj ponownie.";
    isLoading = false;
    // Logowanie szczegółów błędu do konsoli
}
```

**UI:**
- `MudAlert(Severity.Error)` z komunikatem błędu
- Pozostawienie wartości w polach
- Użytkownik może spróbować ponownie (kliknięcie "Zapisz")

### 10.4 Błędy sieciowe / Supabase niedostępny

**Scenariusze:**
- Brak połączenia z internetem
- Supabase API niedostępne

**Obsługa:**
```csharp
catch (Exception ex)
{
    errorMessage = "Wystąpił problem z połączeniem. Sprawdź internet i spróbuj ponownie.";
    isLoading = false;
    // Logowanie błędu
}
```

**UI:**
- `MudAlert(Severity.Error)` z komunikatem błędu
- Sugestia sprawdzenia połączenia internetowego

### 10.5 Edge cases

**Użytkownik przerywa wypełnianie i klika "Anuluj":**
- Dane formularza są tracone (brak auto-save)
- Brak potwierdzenia "Czy na pewno chcesz anulować?"
- Natychmiastowa nawigacja do `/trips`

**Użytkownik wybiera EndDate przed StartDate:**
- Walidacja błyskawicznie wyłapuje błąd
- Komunikat "Data zakończenia musi być późniejsza niż data rozpoczęcia"
- Przycisk "Zapisz" disabled

**Użytkownik wpisuje bardzo długą nazwę (> 200 znaków):**
- Walidacja MaxLength wyłapuje błąd
- Komunikat "Nazwa nie może przekraczać 200 znaków"
- Licznik znaków (opcjonalnie): "250 / 200"

**Użytkownik nie wybiera rodzaju transportu:**
- Walidacja Required wyłapuje błąd
- Komunikat "Rodzaj transportu jest wymagany"
- Przycisk "Zapisz" disabled

**Użytkownik wypełnia tylko wymagane pola (bez opisu):**
- Walidacja OK (opis opcjonalny)
- Submit formularza działa poprawnie
- Backend zapisuje `Description` jako `null`

## 11. Kroki implementacji

### Krok 1: Utworzenie pliku komponentu głównego
- Utwórz plik `MotoNomad.App/Pages/CreateTrip.razor`
- Ustaw routing: `@page "/trip/create"`
- Dodaj atrybut: `@attribute [Authorize]` (ochrona trasy)
- Dodaj dyrektywy `@inject` dla ITripService, NavigationManager, ISnackbar

### Krok 2: Implementacja zmiennych stanu CreateTrip.razor
```csharp
@code {
    private bool isLoading = false;
    private string? errorMessage = null;
}
```

### Krok 3: Utworzenie struktury UI CreateTrip.razor
```razor
<MudContainer MaxWidth="MaxWidth.Medium">
    <MudText Typo="Typo.h4" Class="mb-4">Nowa wycieczka</MudText>
    
    <MudCard Elevation="5">
        <MudCardHeader>
            <MudText Typo="Typo.h6">Podstawowe informacje</MudText>
        </MudCardHeader>
        
        <MudCardContent>
            @if (!string.IsNullOrEmpty(errorMessage))
            {
                <MudAlert Severity="Severity.Error" Class="mb-4">
                    @errorMessage
                </MudAlert>
            }
            
            <TripForm 
                Trip="null" 
                OnSubmit="HandleCreateTripAsync" 
                OnCancel="HandleCancel"
                IsLoading="@isLoading" />
        </MudCardContent>
    </MudCard>
</MudContainer>
```

### Krok 4: Implementacja metody HandleCreateTripAsync()
```csharp
private async Task HandleCreateTripAsync(CreateTripCommand command)
{
    isLoading = true;
    errorMessage = null;

    try
    {
        var trip = await TripService.CreateTripAsync(command);
        Snackbar.Add($"Wycieczka '{trip.Name}' została utworzona!", Severity.Success);
        NavigationManager.NavigateTo("/trips");
    }
    catch (UnauthorizedException)
    {
        Snackbar.Add("Sesja wygasła. Zaloguj się ponownie.", Severity.Warning);
        NavigationManager.NavigateTo("/login");
    }
    catch (ValidationException ex)
    {
        errorMessage = ex.Message;
    }
    catch (DatabaseException ex)
    {
        errorMessage = "Nie udało się utworzyć wycieczki. Spróbuj ponownie.";
        // TODO: Logowanie błędu
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

### Krok 5: Implementacja metody HandleCancel()
```csharp
private void HandleCancel()
{
    NavigationManager.NavigateTo("/trips");
}
```

### Krok 6: Utworzenie komponentu TripForm.razor
- Utwórz plik `MotoNomad.App/Shared/Components/TripForm.razor`
- Zdefiniuj parametry:
  ```csharp
  [Parameter] public TripDetailDto? Trip { get; set; }
  [Parameter] public EventCallback<CreateTripCommand> OnSubmit { get; set; }
  [Parameter] public EventCallback OnCancel { get; set; }
  [Parameter] public bool IsLoading { get; set; }
  ```

### Krok 7: Implementacja zmiennych stanu TripForm.razor
```csharp
@code {
    private MudForm form = null!;
    private TripFormViewModel model = new();
    
    private class TripFormViewModel
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public TransportType TransportType { get; set; } = TransportType.Motorcycle;
    }
}
```

### Krok 8: Inicjalizacja danych w OnInitialized (TripForm)
```csharp
protected override void OnInitialized()
{
    if (Trip != null)
    {
        // Tryb edycji - wypełnienie danych
        model.Name = Trip.Name;
        model.StartDate = Trip.StartDate.ToDateTime(TimeOnly.MinValue);
        model.EndDate = Trip.EndDate.ToDateTime(TimeOnly.MinValue);
        model.Description = Trip.Description;
        model.TransportType = Trip.TransportType;
    }
    // Tryb tworzenia - pola puste (domyślne wartości ViewModelu)
}
```

### Krok 9: Implementacja UI formularza (TripForm.razor)
```razor
<MudForm @ref="form" @bind-IsValid="formValid">
    <MudTextField 
        @bind-Value="model.Name"
        Label="Nazwa wycieczki"
        Required="true"
        MaxLength="200"
        HelperText="Max 200 znaków"
        Disabled="@IsLoading" />
    
    <MudDatePicker 
        @bind-Date="model.StartDate"
        Label="Data rozpoczęcia"
        DateFormat="dd.MM.yyyy"
        Required="true"
        Disabled="@IsLoading" />
    
    <MudDatePicker 
        @bind-Date="model.EndDate"
        Label="Data zakończenia"
        DateFormat="dd.MM.yyyy"
        Required="true"
        Validation="@ValidateEndDate"
        Disabled="@IsLoading" />
    
    <MudTextField 
        @bind-Value="model.Description"
        Label="Opis (opcjonalnie)"
        Lines="3"
        MaxLength="2000"
        HelperText="Max 2000 znaków"
        Disabled="@IsLoading" />
    
    <MudSelect 
        @bind-Value="model.TransportType"
        Label="Rodzaj transportu"
        Required="true"
        Disabled="@IsLoading">
        <MudSelectItem Value="@TransportType.Motorcycle">Motocykl</MudSelectItem>
        <MudSelectItem Value="@TransportType.Airplane">Samolot</MudSelectItem>
        <MudSelectItem Value="@TransportType.Train">Pociąg</MudSelectItem>
        <MudSelectItem Value="@TransportType.Car">Samochód</MudSelectItem>
        <MudSelectItem Value="@TransportType.Other">Inne</MudSelectItem>
    </MudSelect>
</MudForm>

<MudCardActions>
    <MudButton 
        Variant="Variant.Filled" 
        Color="Color.Primary"
        OnClick="HandleSubmit"
        Disabled="@(!formValid || IsLoading)">
        @if (IsLoading)
        {
            <MudProgressCircular Size="Size.Small" Indeterminate="true" />
        }
        else
        {
            <text>Zapisz</text>
        }
    </MudButton>
    
    <MudButton 
        Variant="Variant.Text"
        Color="Color.Default"
        OnClick="@(() => OnCancel.InvokeAsync())"
        Disabled="@IsLoading">
        Anuluj
    </MudButton>
</MudCardActions>
```

### Krok 10: Implementacja custom validation dla EndDate (TripForm)
```csharp
private Func<DateTime?, string?> ValidateEndDate => (endDate) =>
{
    if (!endDate.HasValue)
        return "Data zakończenia jest wymagana";
    
    if (!model.StartDate.HasValue)
        return null;
    
    if (endDate.Value <= model.StartDate.Value)
        return "Data zakończenia musi być późniejsza niż data rozpoczęcia";
    
    return null;
};
```

### Krok 11: Implementacja metody HandleSubmit (TripForm)
```csharp
private async Task HandleSubmit()
{
    await form.Validate();
    if (!form.IsValid) return;

    var command = new CreateTripCommand
    {
        Name = model.Name.Trim(),
        StartDate = DateOnly.FromDateTime(model.StartDate!.Value),
        EndDate = DateOnly.FromDateTime(model.EndDate!.Value),
        Description = string.IsNullOrWhiteSpace(model.Description) 
            ? null 
            : model.Description.Trim(),
        TransportType = model.TransportType
    };

    await OnSubmit.InvokeAsync(command);
}
```

### Krok 12: Dodanie autofocus na polu Nazwa (TripForm)
```csharp
private ElementReference nameField;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && Trip == null) // Tylko w trybie tworzenia
    {
        await nameField.FocusAsync();
    }
}
```

W MudTextField dla Name dodaj: `@ref="nameField"`

### Krok 13: Stylizacja i finalizacja
- Dodaj marginesy między polami formularza (Class="mb-3")
- Upewnij się, że formularz jest responsywny
- Przetestuj keyboard navigation (Tab, Enter)

### Krok 14: Testy
- Przetestuj utworzenie wycieczki (wszystkie pola poprawne)
- Przetestuj walidację:
  - Puste pole Nazwa
  - Nazwa za długa (> 200 znaków)
  - Brak daty rozpoczęcia
  - Brak daty zakończenia
  - EndDate <= StartDate (błąd walidacji)
  - Opis za długi (> 2000 znaków)
  - Brak wyboru transportu
- Przetestuj submit formularza (kliknięcie "Zapisz")
- Przetestuj anulowanie (kliknięcie "Anuluj")
- Przetestuj scenariusze błędów:
  - Sesja wygasła (UnauthorizedException)
  - Błąd bazy danych (DatabaseException)
  - Błąd walidacji z backendu (ValidationException)
  - Brak połączenia z internetem
- Przetestuj autofocus na polu Nazwa
- Przetestuj dostępność (keyboard navigation, screen reader)
- Przetestuj responsywność (mobile + desktop)
- Zmierz czas wypełnienia formularza (cel: < 2 minuty zgodnie z US-003)

### Krok 15: Dokumentacja
- Dodaj komentarze XML dla publicznych metod
- Udokumentuj custom validation dla dat
- Udokumentuj reuż How do you want to continue? Please provide your next message.

ywalność TripForm.razor (używany też w EditTrip)
- Zaktualizuj README z informacjami o tworzeniu wycieczek