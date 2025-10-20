# Plan implementacji widoku TripDetails

## 1. Przegląd

Widok TripDetails (`/trip/{id}`) jest centrum zarządzania pojedynczą wycieczką w aplikacji MotoNomad. Zawiera system zakładek z dwiema sekcjami: "Szczegóły" (edycja danych wycieczki + usuwanie) oraz "Towarzysze" (lista towarzyszy + dodawanie/usuwanie). Widok ładuje dane równolegle (Trip + Companions) przy inicjalizacji, obsługuje walidację biznesową (daty, wymagane pola), zapewnia bezpieczeństwo poprzez RLS (automatyczne przekierowanie jeśli użytkownik próbuje dostać się do cudzej wycieczki) oraz przyjazne UX z komunikatami sukcesu i błędów.

## 2. Routing widoku

**Ścieżka:** `/trip/{id}`

**Parametr route:** `id` (Guid) - unikalny identyfikator wycieczki

**Dostępność:** Chroniona (wymagane uwierzytelnienie + RLS weryfikuje właściciela)

**Routing w App.razor:**
```csharp
<AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
    <NotAuthorized>
        <RedirectToLogin />
    </NotAuthorized>
</AuthorizeRouteView>
```

**Nawigacja do widoku:**
- Z `/trips` → Kliknięcie karty wycieczki (TripListItem)

**Nawigacja z widoku:**
- Po usunięciu wycieczki → `/trips`
- Po zapisaniu zmian → pozostanie na `/trip/{id}` (odświeżenie danych)
- Przycisk "Wstecz" w przeglądarce → `/trips`

## 3. Struktura komponentów

```
TripDetails.razor (Strona główna)
├── LoadingSpinner.razor [podczas ładowania początkowego]
├── MudContainer (po załadowaniu danych)
│   ├── MudBreadcrumbs (nawigacja: Moje wycieczki > Nazwa wycieczki)
│   ├── MudTabs (@bind-ActivePanelIndex)
│   │   ├── MudTabPanel (Label: "Szczegóły")
│   │   │   ├── MudCard
│   │   │   │   ├── MudCardHeader ("Edycja wycieczki")
│   │   │   │   ├── MudCardContent
│   │   │   │   │   ├── MudAlert (błędy ValidationException/DatabaseException) [warunkowo]
│   │   │   │   │   └── TripForm.razor (tryb edit, Trip={trip})
│   │   │   │   └── MudCardActions
│   │   │   │       ├── MudButton ("Zapisz zmiany", Primary)
│   │   │   │       └── MudIconButton ("Usuń wycieczkę", Danger)
│   │   │   
│   │   └── MudTabPanel (Label: "Towarzysze ({companionCount})")
│   │       ├── MudButton ("Dodaj towarzysza", toggle formularza)
│   │       ├── CompanionForm.razor [warunkowo widoczny]
│   │       ├── LoadingSpinner.razor [podczas ładowania listy]
│   │       ├── EmptyState.razor [jeśli brak towarzyszy]
│   │       └── CompanionList.razor (lista towarzyszy)
│
├── Dialogi (MudDialog):
│   ├── DeleteTripConfirmationDialog.razor
│   └── DeleteCompanionConfirmationDialog.razor
│
└── Komponenty potomne:
    ├── TripForm.razor (formularz edycji, reużywalny)
    ├── CompanionForm.razor (formularz dodawania towarzysza)
    ├── CompanionList.razor (lista towarzyszy)
    ├── EmptyState.razor (stan pusty)
    └── LoadingSpinner.razor (stan ładowania)
```

## 4. Szczegóły komponentów

### 4.1 TripDetails.razor (Komponent strony)

**Opis komponentu:**
Główny komponent strony szczegółów wycieczki. Zarządza stanem ładowania danych (równoległe Task.WhenAll dla Trip + Companions), przełączaniem między zakładkami, edycją wycieczki, zarządzaniem towarzyszami oraz usuwaniem wycieczki. Obsługuje RLS security (przekierowanie jeśli NotFoundException) i wszystkie operacje CRUD dla Trip i Companions.

**Główne elementy:**
- `LoadingSpinner.razor` - globalny spinner podczas `OnInitializedAsync()` (Task.WhenAll)
- `MudContainer` (MaxWidth.Large) - kontener główny (widoczny po załadowaniu)
- `MudBreadcrumbs` - nawigacja "Moje wycieczki > Nazwa wycieczki"
- `MudTabs` - system zakładek z indeksem aktywnej zakładki
- Zakładka "Szczegóły":
  - `MudCard` z formularzem edycji (TripForm.razor)
  - `MudAlert` (błędy walidacji/bazy)
  - `MudButton` "Zapisz zmiany"
  - `MudIconButton` "Usuń wycieczkę" (ikona kosza)
- Zakładka "Towarzysze":
  - `MudButton` "Dodaj towarzysza" (toggle widoczności formularza)
  - `CompanionForm.razor` (warunkowo widoczny)
  - `CompanionList.razor` (lista) lub `EmptyState.razor`

**Obsługiwane zdarzenia:**
- `OnInitializedAsync()` - równoległe ładowanie Trip + Companions (Task.WhenAll)
- `OnParametersSetAsync()` - obsługa zmiany parametru `id` (jeśli użytkownik przechodzi między wycieczkami)
- `@bind-ActivePanelIndex` - przełączanie zakładek
- `OnSubmit` (TripForm) - edycja wycieczki (`HandleUpdateTripAsync()`)
- `OnDeleteTrip` - usunięcie wycieczki (dialog → API → nawigacja)
- `OnAddCompanion` (CompanionForm) - dodanie towarzysza
- `OnRemoveCompanion` (CompanionList) - usunięcie towarzysza (dialog → API → odświeżenie)

**Warunki walidacji:**
- Walidacja formularza Trip delegowana do TripForm.razor
- Walidacja formularza Companion delegowana do CompanionForm.razor
- RLS security: jeśli `ITripService.GetTripByIdAsync()` zwróci `NotFoundException` → przekierowanie na `/trips`

**Typy:**
- `Guid` (parametr route `id`)
- `TripDetailDto` (dane wycieczki)
- `List<CompanionListItemDto>` (lista towarzyszy)
- `UpdateTripCommand` (request edycji)
- `AddCompanionCommand` (request dodania towarzysza)

**Propsy:**
Brak (komponent strony najwyższego poziomu)

**Parametry:**
```csharp
[Parameter] public Guid Id { get; set; }
```

**Parametry wstrzykiwane (Dependency Injection):**
- `[Inject] ITripService TripService { get; set; }`
- `[Inject] ICompanionService CompanionService { get; set; }`
- `[Inject] NavigationManager NavigationManager { get; set; }`
- `[Inject] ISnackbar Snackbar { get; set; }`
- `[Inject] IDialogService DialogService { get; set; }`

### 4.2 TripForm.razor (Komponent formularza - reużywalny)

**Opis komponentu:**
Ten sam komponent co w CreateTrip, ale w trybie edycji (parametr `Trip` nie-null, pola wypełnione). Szczegóły implementacji w `createtrip-view-implementation-plan.md`.

**Propsy:**
```csharp
[Parameter] public TripDetailDto? Trip { get; set; } // Wypełnione w trybie edit
[Parameter] public EventCallback<UpdateTripCommand> OnSubmit { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public bool IsLoading { get; set; }
```

### 4.3 CompanionForm.razor (Komponent formularza towarzysza)

**Opis komponentu:**
Formularz dodawania nowego towarzysza do wycieczki. Zawiera pola: Imię (required), Nazwisko (required), Kontakt (opcjonalne). Domyślnie ukryty, pokazywany po kliknięciu przycisku "Dodaj towarzysza". Po zapisaniu: formularz ukrywany, pola czyszczone, lista towarzyszy odświeżana.

**Główne elementy:**
- `MudForm` (@ref="form")
- `MudTextField` (Imię) - Required, MaxLength(100)
- `MudTextField` (Nazwisko) - Required, MaxLength(100)
- `MudTextField` (Kontakt) - Opcjonalne, MaxLength(255)
- `MudButton` ("Zapisz", Primary)
- `MudButton` ("Anuluj", Secondary)

**Obsługiwane zdarzenia:**
- `@bind-Value` dla każdego pola
- `OnValidSubmit` - wywołanie callbacku `OnSubmit`
- Kliknięcie "Anuluj" - wywołanie callbacku `OnCancel`

**Warunki walidacji:**
- **Imię:**
  - Required: "Imię jest wymagane"
  - MaxLength(100): "Imię nie może przekraczać 100 znaków"
- **Nazwisko:**
  - Required: "Nazwisko jest wymagane"
  - MaxLength(100): "Nazwisko nie może przekraczać 100 znaków"
- **Kontakt:**
  - Opcjonalne
  - MaxLength(255): "Kontakt nie może przekraczać 255 znaków"

**Typy:**
- `AddCompanionCommand` (output)

**Propsy:**
```csharp
[Parameter] public Guid TripId { get; set; }
[Parameter] public EventCallback<AddCompanionCommand> OnSubmit { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public bool IsLoading { get; set; }
```

### 4.4 CompanionList.razor (Komponent listy towarzyszy)

**Opis komponentu:**
Lista towarzyszy wyświetlana jako `MudList` (responsywna). Każdy towarzysz jako `MudListItem` z imieniem, nazwiskiem, kontaktem (jeśli jest) oraz ikoną kosza (usuwanie). Kliknięcie ikony kosza wywołuje dialog potwierdzenia.

**Główne elementy:**
- `MudList` - kontener listy
- `MudListItem` (dla każdego towarzysza):
  - `MudText` (Typo.body1) - Imię i Nazwisko
  - `MudText` (Typo.body2, Secondary) - Kontakt (jeśli jest)
  - `MudIconButton` (ikona kosza) - usuwanie

**Obsługiwane zdarzenia:**
- Kliknięcie ikony kosza - wywołanie callbacku `OnRemove` z `companionId`

**Warunki walidacji:**
Brak (komponent tylko wyświetla dane)

**Typy:**
- `List<CompanionListItemDto>` - lista towarzyszy

**Propsy:**
```csharp
[Parameter] public List<CompanionListItemDto> Companions { get; set; } = new();
[Parameter] public EventCallback<Guid> OnRemove { get; set; }
```

### 4.5 DeleteTripConfirmationDialog.razor (Dialog potwierdzenia)

**Opis komponentu:**
Dialog MudBlazor z potwierdzeniem usunięcia wycieczki. Wyświetla nazwę wycieczki i komunikat "Ta operacja jest nieodwracalna". Przyciski: "Anuluj" (Secondary), "Usuń" (Danger/Error).

**Główne elementy:**
- `MudDialog`
- `MudDialogContent` - komunikat z nazwą wycieczki
- `MudDialogActions` - przyciski

**Obsługiwane zdarzenia:**
- Kliknięcie "Usuń" - zwrócenie `DialogResult(true)`
- Kliknięcie "Anuluj" - zwrócenie `DialogResult(false)`

**Propsy:**
```csharp
[CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
[Parameter] public string TripName { get; set; } = string.Empty;
```

### 4.6 DeleteCompanionConfirmationDialog.razor (Dialog potwierdzenia)

**Opis komponentu:**
Dialog MudBlazor z potwierdzeniem usunięcia towarzysza. Wyświetla imię i nazwisko towarzysza. Przyciski: "Anuluj", "Usuń".

**Główne elementy:**
- `MudDialog`
- `MudDialogContent` - komunikat z imieniem i nazwiskiem
- `MudDialogActions` - przyciski

**Obsługiwane zdarzenia:**
- Kliknięcie "Usuń" - zwrócenie `DialogResult(true)`
- Kliknięcie "Anuluj" - zwrócenie `DialogResult(false)`

**Propsy:**
```csharp
[CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
[Parameter] public string FirstName { get; set; } = string.Empty;
[Parameter] public string LastName { get; set; } = string.Empty;
```

## 5. Typy

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

### UpdateTripCommand (Request DTO)
```csharp
public record UpdateTripCommand
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
}
```

### CompanionListItemDto (Response DTO)
```csharp
public record CompanionListItemDto
{
    public required Guid Id { get; init; }
    public required Guid TripId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
```

### AddCompanionCommand (Request DTO)
```csharp
public record AddCompanionCommand
{
    public required Guid TripId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
```

### CompanionFormViewModel (wewnętrzny ViewModel)
```csharp
private class CompanionFormViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Contact { get; set; }
}
```

## 6. Zarządzanie stanem

**Zmienne stanu komponentu TripDetails.razor:**

```csharp
private TripDetailDto? trip = null;
private List<CompanionListItemDto> companions = new();
private bool isLoadingTrip = false;
private bool isLoadingCompanions = false;
private bool isUpdatingTrip = false;
private bool isAddingCompanion = false;
private bool showCompanionForm = false;
private int activeTabIndex = 0; // 0 = Szczegóły, 1 = Towarzysze
private string? errorMessage = null;
```

**Opis zmiennych:**
- `trip` - dane wycieczki (null podczas ładowania lub jeśli nie znaleziono)
- `companions` - lista towarzyszy
- `isLoadingTrip` - flaga ładowania wycieczki
- `isLoadingCompanions` - flaga ładowania towarzyszy
- `isUpdatingTrip` - flaga zapisu zmian wycieczki
- `isAddingCompanion` - flaga dodawania towarzysza
- `showCompanionForm` - widoczność formularza dodawania towarzysza
- `activeTabIndex` - aktywna zakładka (0 lub 1)
- `errorMessage` - komunikat błędu (ValidationException, DatabaseException)

**Przepływ stanu:**

### Inicjalizacja (OnInitializedAsync):
```csharp
isLoadingTrip = true;
isLoadingCompanions = true;

// Równoległe ładowanie (Task.WhenAll)
var tripTask = TripService.GetTripByIdAsync(Id);
var companionsTask = CompanionService.GetCompanionsByTripIdAsync(Id);

await Task.WhenAll(tripTask, companionsTask);

trip = tripTask.Result;
companions = companionsTask.Result.ToList();

isLoadingTrip = false;
isLoadingCompanions = false;
```

### Edycja wycieczki:
1. Użytkownik edytuje pola w TripForm
2. Kliknięcie "Zapisz zmiany" → `isUpdatingTrip = true`, `errorMessage = null`
3. Wywołanie `TripService.UpdateTripAsync(command)`
4. Sukces → Snackbar, odświeżenie `trip`, `isUpdatingTrip = false`
5. Błąd → `errorMessage = ex.Message`, `isUpdatingTrip = false`

### Usunięcie wycieczki:
1. Kliknięcie ikony kosza → otwarcie DeleteTripConfirmationDialog
2. Jeśli potwierdzono → wywołanie `TripService.DeleteTripAsync(Id)`
3. Sukces → Snackbar, nawigacja do `/trips`
4. Błąd → Snackbar Error

### Dodawanie towarzysza:
1. Kliknięcie "Dodaj towarzysza" → `showCompanionForm = true`
2. Wypełnienie formularza
3. Kliknięcie "Zapisz" → `isAddingCompanion = true`, `showCompanionForm = false`
4. Wywołanie `CompanionService.AddCompanionAsync(command)`
5. Sukces → odświeżenie listy `companions`, Snackbar, `isAddingCompanion = false`
6. Błąd → Snackbar Error, `isAddingCompanion = false`

### Usuwanie towarzysza:
1. Kliknięcie ikony kosza → otwarcie DeleteCompanionConfirmationDialog
2. Jeśli potwierdzono → wywołanie `CompanionService.RemoveCompanionAsync(companionId)`
3. Sukces → odświeżenie listy `companions`, Snackbar
4. Błąd → Snackbar Error

**Brak potrzeby custom hooka** - stan zarządzany lokalnie w komponencie.

## 7. Integracja API

### 7.1 Ładowanie wycieczki

**Endpoint:** `ITripService.GetTripByIdAsync(Guid tripId)`

**Typ żądania:** `Guid` (parametr `Id` z route)

**Typ odpowiedzi:** `Task<TripDetailDto>`

**Obsługa sukcesu:**
```csharp
trip = await TripService.GetTripByIdAsync(Id);
```

**Obsługa błędów:**
```csharp
catch (NotFoundException)
{
    // Wycieczka nie istnieje lub użytkownik nie jest właścicielem (RLS)
    Snackbar.Add("Nie znaleziono wycieczki.", Severity.Warning);
    NavigationManager.NavigateTo("/trips");
}
catch (UnauthorizedException)
{
    NavigationManager.NavigateTo("/login");
}
```

### 7.2 Ładowanie towarzyszy

**Endpoint:** `ICompanionService.GetCompanionsByTripIdAsync(Guid tripId)`

**Typ żądania:** `Guid` (parametr `Id` z route)

**Typ odpowiedzi:** `Task<IEnumerable<CompanionListItemDto>>`

**Obsługa sukcesu:**
```csharp
companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
```

**Obsługa błędów:**
Identyczna jak dla ładowania wycieczki (RLS sprawdza właściciela).

### 7.3 Równoległe ładowanie (optymalizacja)

**Implementacja:**
```csharp
try
{
    var tripTask = TripService.GetTripByIdAsync(Id);
    var companionsTask = CompanionService.GetCompanionsByTripIdAsync(Id);

    await Task.WhenAll(tripTask, companionsTask);

    trip = tripTask.Result;
    companions = companionsTask.Result.ToList();
}
catch (NotFoundException)
{
    Snackbar.Add("Nie znaleziono wycieczki.", Severity.Warning);
    NavigationManager.NavigateTo("/trips");
}
```

### 7.4 Edycja wycieczki

**Endpoint:** `ITripService.UpdateTripAsync(UpdateTripCommand command)`

**Typ żądania:** `UpdateTripCommand`
```csharp
new UpdateTripCommand
{
    Id = trip.Id,
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
trip = await TripService.UpdateTripAsync(command);
Snackbar.Add("Zmiany zostały zapisane!", Severity.Success);
// Pozostanie na /trip/{id}, dane odświeżone
```

**Obsługa błędów:**
```csharp
catch (ValidationException ex)
{
    errorMessage = ex.Message;
}
catch (DatabaseException ex)
{
    errorMessage = "Nie udało się zapisać zmian. Spróbuj ponownie.";
}
```

### 7.5 Usuwanie wycieczki

**Endpoint:** `ITripService.DeleteTripAsync(Guid tripId)`

**Typ żądania:** `Guid` (trip.Id)

**Typ odpowiedzi:** `Task` (void)

**Obsługa sukcesu:**
```csharp
await TripService.DeleteTripAsync(trip.Id);
Snackbar.Add($"Wycieczka '{trip.Name}' została usunięta.", Severity.Success);
NavigationManager.NavigateTo("/trips");
```

**Obsługa błędów:**
```csharp
catch (NotFoundException)
{
    Snackbar.Add("Nie znaleziono wycieczki.", Severity.Warning);
    NavigationManager.NavigateTo("/trips");
}
catch (DatabaseException ex)
{
    Snackbar.Add("Nie udało się usunąć wycieczki. Spróbuj ponownie.", Severity.Error);
}
```

### 7.6 Dodawanie towarzysza

**Endpoint:** `ICompanionService.AddCompanionAsync(AddCompanionCommand command)`

**Typ żądania:** `AddCompanionCommand`
```csharp
new AddCompanionCommand
{
    TripId = trip.Id,
    FirstName = model.FirstName.Trim(),
    LastName = model.LastName.Trim(),
    Contact = string.IsNullOrWhiteSpace(model.Contact) 
        ? null 
        : model.Contact.Trim()
}
```

**Typ odpowiedzi:** `Task<CompanionDto>`

**Obsługa sukcesu:**
```csharp
var companion = await CompanionService.AddCompanionAsync(command);
// Odświeżenie listy towarzyszy
companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
Snackbar.Add($"Dodano towarzysza: {companion.FirstName} {companion.LastName}", Severity.Success);
showCompanionForm = false; // Ukrycie formularza
```

**Obsługa błędów:**
```csharp
catch (ValidationException ex)
{
    Snackbar.Add(ex.Message, Severity.Warning);
}
catch (DatabaseException ex)
{
    Snackbar.Add("Nie udało się dodać towarzysza. Spróbuj ponownie.", Severity.Error);
}
```

### 7.7 Usuwanie towarzysza

**Endpoint:** `ICompanionService.RemoveCompanionAsync(Guid companionId)`

**Typ żądania:** `Guid` (companion.Id)

**Typ odpowiedzi:** `Task` (void)

**Obsługa sukcesu:**
```csharp
await CompanionService.RemoveCompanionAsync(companionId);
// Odświeżenie listy towarzyszy
companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
Snackbar.Add("Towarzysz został usunięty.", Severity.Success);
```

**Obsługa błędów:**
```csharp
catch (NotFoundException)
{
    Snackbar.Add("Nie znaleziono towarzysza.", Severity.Warning);
    // Odświeżenie listy (może został już usunięty)
    companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
}
catch (DatabaseException ex)
{
    Snackbar.Add("Nie udało się usunąć towarzysza. Spróbuj ponownie.", Severity.Error);
}
```

## 8. Interakcje użytkownika

### 8.1 Ładowanie strony
**Akcja:** Użytkownik wchodzi na `/trip/{id}` (kliknięcie karty z `/trips`)  
**Efekt:**
- Wyświetlenie globalnego LoadingSpinner (cała strona)
- Równoległe wywołanie `GetTripByIdAsync()` i `GetCompanionsByTripIdAsync()`
- Po załadowaniu: wyświetlenie zakładek, domyślnie "Szczegóły"
- Jeśli NotFoundException: przekierowanie na `/trips` z komunikatem

### 8.2 Przełączanie zakładek
**Akcja:** Użytkownik klika zakładkę "Towarzysze"  
**Efekt:**
- `activeTabIndex` zmienia się na 1
- Wyświetlenie zawartości zakładki (lista towarzyszy lub EmptyState)
- Dane już załadowane (brak ponownego ładowania)

### 8.3 Edycja danych wycieczki (zakładka "Szczegóły")
**Akcja:** Użytkownik edytuje pola w TripForm i klika "Zapisz zmiany"  
**Warunek:** Formularz musi być poprawnie zwalidowany  
**Efekt:**
- `isUpdatingTrip = true` (przycisk disabled, spinner)
- Wywołanie `UpdateTripAsync(command)`
- Po sukcesie: Snackbar "Zmiany zostały zapisane!", odświeżenie `trip`, pozostanie na `/trip/{id}`
- Po błędzie: MudAlert z komunikatem błędu

### 8.4 Usuwanie wycieczki (zakładka "Szczegóły")
**Akcja:** Użytkownik klika ikonę kosza "Usuń wycieczkę"  
**Efekt:**
- Otwarcie DeleteTripConfirmationDialog z nazwą wycieczki
- Jeśli "Anuluj": zamknięcie dialogu (brak akcji)
- Jeśli "Usuń": wywołanie `DeleteTripAsync(id)`, Snackbar, przekierowanie na `/trips`

### 8.5 Pokazywanie formularza towarzysza (zakładka "Towarzysze")
**Akcja:** Użytkownik klika przycisk "Dodaj towarzysza"  
**Efekt:**
- `showCompanionForm = true`
- Wyświetlenie CompanionForm.razor z pustymi polami
- Autofocus na polu "Imię"

### 8.6 Dodawanie towarzysza (zakładka "Towarzysze")
**Akcja:** Użytkownik wypełnia CompanionForm i klika "Zapisz"  
**Warunek:** Formularz musi być poprawnie zwalidowany (Imię i Nazwisko required)  
**Efekt:**
- `isAddingCompanion = true`, `showCompanionForm = false`
- Wywołanie `AddCompanionAsync(command)`
- Po sukcesie: odświeżenie listy `companions`, Snackbar, formularz ukryty
- Po błędzie: Snackbar Error, `showCompanionForm = true` (formularz widoczny ponownie)

### 8.7 Anulowanie dodawania towarzysza
**Akcja:** Użytkownik klika "Anuluj" w CompanionForm  
**Efekt:**
- `showCompanionForm = false`
- Ukrycie formularza (bez zapisywania)
- Brak potwierdzenia

### 8.8 Usuwanie towarzysza (zakładka "Towarzysze")
**Akcja:** Użytkownik klika ikonę kosza przy towarzyszu  
**Efekt:**
- Otwarcie DeleteCompanionConfirmationDialog z imieniem i nazwiskiem
- Jeśli "Anuluj": zamknięcie dialogu (brak akcji)
- Jeśli "Usuń": wywołanie `RemoveCompanionAsync(companionId)`, odświeżenie listy, Snackbar

### 8.9 Breadcrumbs - nawigacja
**Akcja:** Użytkownik klika "Moje wycieczki" w breadcrumbs  
**Efekt:**
- Nawigacja do `/trips`

## 9. Warunki i walidacja

### 9.1 Wyświetlanie globalnego LoadingSpinner

**Warunek:** `isLoadingTrip == true || isLoadingCompanions == true`

**Komponent:** TripDetails.razor (główny poziom)

**Efekt:**
- Wyświetlenie `LoadingSpinner.razor` z komunikatem "Ładowanie wycieczki..."
- Ukrycie całej zawartości (MudContainer, zakładki)

### 9.2 Wyświetlanie MudContainer (po załadowaniu)

**Warunek:** `trip != null && !isLoadingTrip && !isLoadingCompanions`

**Komponent:** TripDetails.razor

**Efekt:**
- Wyświetlenie MudBreadcrumbs, MudTabs i całej zawartości

### 9.3 Wyświetlanie błędów edycji (zakładka "Szczegóły")

**Warunek:** `!string.IsNullOrEmpty(errorMessage)`

**Komponent:** Zakładka "Szczegóły"

**Efekt:**
- Wyświetlenie `MudAlert(Severity.Error)` nad formularzem TripForm
- Tekst: `{errorMessage}`

### 9.4 Wyświetlanie formularza towarzysza (zakładka "Towarzysze")

**Warunek:** `showCompanionForm == true`

**Komponent:** Zakładka "Towarzysze"

**Efekt:**
- Wyświetlenie CompanionForm.razor
- Ukrycie przycisku "Dodaj towarzysza" (podczas wypełniania formularza)

### 9.5 Wyświetlanie listy towarzyszy (zakładka "Towarzysze")

**Warunek:** `companions.Count > 0 && !isLoadingCompanions`

**Komponent:** Zakładka "Towarzysze"

**Efekt:**
- Wyświetlenie CompanionList.razor z listą towarzyszy
- Każdy towarzysz z ikoną kosza (usuwanie)

### 9.6 Wyświetlanie EmptyState (zakładka "Towarzysze")

**Warunek:** `companions.Count == 0 && !isLoadingCompanions`

**Komponent:** Zakładka "Towarzysze"

**Efekt:**
- Wyświetlenie EmptyState.razor:
  - Title: "Brak towarzyszy"
  - Message: "Dodaj osoby, które będą Cię towarzyszyć w podróży"
  - IconName: Icons.Material.Filled.People
  - ButtonText: "Dodaj pierwszego towarzysza"
  - OnButtonClick: → `showCompanionForm = true`

### 9.7 Licznik towarzyszy w etykiecie zakładki

**Warunek:** Zawsze

**Format:** "Towarzysze ({companions.Count})"

**Przykład:** "Towarzysze (3)"

**Efekt:**
- Dynamiczna aktualizacja licznika po dodaniu/usunięciu towarzysza

### 9.8 Walidacja formularza Trip

**Delegowana do TripForm.razor:**
- Nazwa: Required, MaxLength(200)
- StartDate: Required
- EndDate: Required, > StartDate
- Opis: Opcjonalne, MaxLength(2000)
- Transport: Required

**Szczegóły:** Zobacz `createtrip-view-implementation-plan.md`

### 9.9 Walidacja formularza Companion

**Implementowana w CompanionForm.razor:**
- Imię: Required, MaxLength(100)
- Nazwisko: Required, MaxLength(100)
- Kontakt: Opcjonalne, MaxLength(255)

## 10. Obsługa błędów

### 10.1 NotFoundException (wycieczka nie znaleziona lub brak dostępu)

**Scenariusze:**
- Użytkownik próbuje wejść na `/trip/{cudzej_wycieczki}`
- RLS blokuje dostęp (użytkownik nie jest właścicielem)
- Wycieczka została już usunięta

**Obsługa:**
```csharp
catch (NotFoundException)
{
    Snackbar.Add("Nie znaleziono wycieczki.", Severity.Warning);
    NavigationManager.NavigateTo("/trips");
}
```

**UI:**
- Snackbar z komunikatem ostrzeżenia
- Automatyczne przekierowanie na `/trips`

### 10.2 UnauthorizedException (sesja wygasła)

**Scenariusze:**
- Sesja użytkownika wygasła podczas przeglądania szczegółów
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

### 10.3 ValidationException (błędy walidacji edycji)

**Scenariusze:**
- Walidacja po stronie serwera nie przeszła (mimo walidacji klienta)
- Backend wykrył nieprawidłowe daty lub dane

**Obsługa:**
```csharp
catch (ValidationException ex)
{
    errorMessage = ex.Message;
    isUpdatingTrip = false;
}
```

**UI:**
- `MudAlert(Severity.Error)` w zakładce "Szczegóły" nad formularzem
- Pozostawienie wartości w polach (użytkownik może poprawić)
- Przycisk "Zapisz zmiany" aktywny ponownie

### 10.4 DatabaseException (błąd zapisu/usunięcia)

**Scenariusze:**
- Problemy z połączeniem do Supabase
- Timeout zapytania SQL
- Naruszenie constraints

**Obsługa:**
```csharp
catch (DatabaseException ex)
{
    Snackbar.Add("Nie udało się zapisać zmian. Spróbuj ponownie.", Severity.Error);
    isUpdatingTrip = false;
    // Logowanie błędu
}
```

**UI:**
- Snackbar z komunikatem błędu (czerwony)
- Pozostawienie danych w formularzu
- Użytkownik może spróbować ponownie

### 10.5 Błędy podczas dodawania/usuwania towarzysza

**Obsługa:**
- ValidationException → Snackbar Warning
- DatabaseException → Snackbar Error
- NotFoundException (przy usuwaniu) → Snackbar Warning + odświeżenie listy

### 10.6 Edge cases

**Użytkownik usuwa wycieczkę podczas gdy ktoś inny ją przegląda:**
- Backend zwraca NotFoundException
- UI przekierowuje na `/trips` z komunikatem

**Użytkownik próbuje dodać towarzysza z tym samym imieniem i nazwiskiem:**
- Backend akceptuje (brak unique constraint na imię+nazwisko)
- To jest poprawne zachowanie (może być dwóch towarzyszy o takim samym imieniu)

**Użytkownik przełącza się między zakładkami podczas ładowania:**
- Zakładki są widoczne dopiero po załadowaniu danych (Task.WhenAll)
- Brak możliwości przełączania podczas ładowania

**Użytkownik edytuje wycieczkę i przełącza się na zakładkę "Towarzysze" przed zapisaniem:**
- Zmiany w formularzu nie są tracone (state TripForm.razor)
- Użytkownik może wrócić do zakładki "Szczegóły" i zapisać zmiany

**Bardzo długa lista towarzyszy (>50):**
- UI wyświetla wszystkie (MudList radzi sobie z dużymi listami)
- Future enhancement: paginacja

## 11. Kroki implementacji

### Krok 1: Utworzenie pliku komponentu głównego
- Utwórz plik `MotoNomad.App/Pages/TripDetails.razor`
- Ustaw routing: `@page "/trip/{id:guid}"`
- Dodaj atrybut: `@attribute [Authorize]`
- Dodaj dyrektywy `@inject` dla wszystkich serwisów

### Krok 2: Implementacja parametru i zmiennych stanu
```csharp
@code {
    [Parameter] public Guid Id { get; set; }
    
    private TripDetailDto? trip = null;
    private List<CompanionListItemDto> companions = new();
    private bool isLoadingTrip = false;
    private bool isLoadingCompanions = false;
    private bool isUpdatingTrip = false;
    private bool isAddingCompanion = false;
    private bool showCompanionForm = false;
    private int activeTabIndex = 0;
    private string? errorMessage = null;
}
```

### Krok 3: Implementacja OnInitializedAsync z równoległym ładowaniem
```csharp
protected override async Task OnInitializedAsync()
{
    isLoadingTrip = true;
    isLoadingCompanions = true;

    try
    {
        // Równoległe ładowanie
        var tripTask = TripService.GetTripByIdAsync(Id);
        var companionsTask = CompanionService.GetCompanionsByTripIdAsync(Id);

        await Task.WhenAll(tripTask, companionsTask);

        trip = tripTask.Result;
        companions = companionsTask.Result.ToList();
    }
    catch (NotFoundException)
    {
        Snackbar.Add("Nie znaleziono wycieczki.", Severity.Warning);
        NavigationManager.NavigateTo("/trips");
    }
    catch (UnauthorizedException)
    {
        Snackbar.Add("Sesja wygasła. Zaloguj się ponownie.", Severity.Warning);
        NavigationManager.NavigateTo("/login");
    }
    catch (Exception ex)
    {
        Snackbar.Add("Wystąpił błąd podczas ładowania wycieczki.", Severity.Error);
        // TODO: Logowanie błędu
    }
    finally
    {
        isLoadingTrip = false;
        isLoadingCompanions = false;
        StateHasChanged();
    }
}
```

### Krok 4: Utworzenie struktury UI (główny poziom)
```razor
@if (isLoadingTrip || isLoadingCompanions)
{
    <LoadingSpinner Message="Ładowanie wycieczki..." />
}
else if (trip != null)
{
    <MudContainer MaxWidth="MaxWidth.Large">
        <MudBreadcrumbs Items="@breadcrumbItems" Class="mb-4" />
        
        <MudTabs @bind-ActivePanelIndex="activeTabIndex">
            <MudTabPanel Text="Szczegóły">
                @* Zakładka Szczegóły - Krok 5 *@
            </MudTabPanel>
            
            <MudTabPanel Text="@($"Towarzysze ({companions.Count})")">
                @* Zakładka Towarzysze - Krok 6 *@
            </MudTabPanel>
        </MudTabs>
    </MudContainer>
}
```

### Krok 5: Implementacja zakładki "Szczegóły"
```razor
<MudTabPanel Text="Szczegóły">
    <MudCard Class="mt-4">
        <MudCardHeader>
            <MudText Typo="Typo.h6">Edycja wycieczki</MudText>
        </MudCardHeader>
        
        <MudCardContent>
            @if (!string.IsNullOrEmpty(errorMessage))
            {
                <MudAlert Severity="Severity.Error" Class="mb-4">
                    @errorMessage
                </MudAlert>
            }
            
            <TripForm 
                Trip="@trip" 
                OnSubmit="HandleUpdateTripAsync"
                OnCancel="@(() => NavigationManager.NavigateTo("/trips"))"
                IsLoading="@isUpdatingTrip" />
        </MudCardContent>
        
        <MudCardActions>
            <MudIconButton 
                Icon="@Icons.Material.Filled.Delete"
                Color="Color.Error"
                OnClick="HandleDeleteTrip"
                Title="Usuń wycieczkę" />
        </MudCardActions>
    </MudCard>
</MudTabPanel>
```

### Krok 6: Implementacja zakładki "Towarzysze"
```razor
<MudTabPanel Text="@($"Towarzysze ({companions.Count})")">
    <MudCard Class="mt-4">
        <MudCardHeader>
            <MudText Typo="Typo.h6">Lista towarzyszy</MudText>
            @if (!showCompanionForm)
            {
                <MudSpacer />
                <MudButton 
                    StartIcon="@Icons.Material.Filled.Add"
                    Color="Color.Primary"
                    OnClick="@(() => showCompanionForm = true)">
                    Dodaj towarzysza
                </MudButton>
            }
        </MudCardHeader>
        
        <MudCardContent>
            @if (showCompanionForm)
            {
                <CompanionForm 
                    TripId="@trip.Id"
                    OnSubmit="HandleAddCompanionAsync"
                    OnCancel="@(() => showCompanionForm = false)"
                    IsLoading="@isAddingCompanion" />
                
                <MudDivider Class="my-4" />
            }
            
            @if (isLoadingCompanions)
            {
                <LoadingSpinner Message="Ładowanie towarzyszy..." />
            }
            else if (companions.Count == 0)
            {
                <EmptyState 
                    Title="Brak towarzyszy"
                    Message="Dodaj osoby, które będą Cię towarzyszyć w podróży"
                    IconName="@Icons.Material.Filled.People"
                    ButtonText="Dodaj pierwszego towarzysza"
                    OnButtonClick="@(() => showCompanionForm = true)" />
            }
            else
            {
                <CompanionList 
                    Companions="@companions"
                    OnRemove="HandleRemoveCompanionAsync" />
            }
        </MudCardContent>
    </MudCard>
</MudTabPanel>
```

### Krok 7: Implementacja HandleUpdateTripAsync
```csharp
private async Task HandleUpdateTripAsync(UpdateTripCommand command)
{
    isUpdatingTrip = true;
    errorMessage = null;

    try
    {
        trip = await TripService.UpdateTripAsync(command);
        Snackbar.Add("Zmiany zostały zapisane!", Severity.Success);
    }
    catch (ValidationException ex)
    {
        errorMessage = ex.Message;
    }
    catch (DatabaseException ex)
    {
        errorMessage = "Nie udało się zapisać zmian. Spróbuj ponownie.";
        // TODO: Logowanie błędu
    }
    catch (Exception ex)
    {
        errorMessage = "Wystąpił nieoczekiwany błąd.";
        // TODO: Logowanie błędu
    }
    finally
    {
        isUpdatingTrip = false;
        StateHasChanged();
    }
}
```

### Krok 8: Implementacja HandleDeleteTrip z dialogiem
```csharp
private async Task HandleDeleteTrip()
{
    var parameters = new DialogParameters 
    { 
        { "TripName", trip!.Name } 
    };
    
    var dialog = await DialogService.ShowAsync<DeleteTripConfirmationDialog>(
        "Potwierdzenie usunięcia", 
        parameters);
    
    var result = await dialog.Result;
    
    if (result.Canceled) return;

    try
    {
        await TripService.DeleteTripAsync(trip.Id);
        Snackbar.Add($"Wycieczka '{trip.Name}' została usunięta.", Severity.Success);
        NavigationManager.NavigateTo("/trips");
    }
    catch (DatabaseException ex)
    {
        Snackbar.Add("Nie udało się usunąć wycieczki. Spróbuj ponownie.", Severity.Error);
        // TODO: Logowanie błędu
    }
}
```

### Krok 9: Implementacja HandleAddCompanionAsync
```csharp
private async Task HandleAddCompanionAsync(AddCompanionCommand command)
{
    isAddingCompanion = true;
    showCompanionForm = false;

    try
    {
        var companion = await CompanionService.AddCompanionAsync(command);
        
        // Odświeżenie listy towarzyszy
        companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
        
        Snackbar.Add($"Dodano towarzysza: {companion.FirstName} {companion.LastName}", Severity.Success);
    }
    catch (ValidationException ex)
    {
        Snackbar.Add(ex.Message, Severity.Warning);
        showCompanionForm = true; // Pokaż formularz ponownie
    }
    catch (DatabaseException ex)
    {
        Snackbar.Add("Nie udało się dodać towarzysza. Spróbuj ponownie.", Severity.Error);
        showCompanionForm = true;
    }
    finally
    {
        isAddingCompanion = false;
        StateHasChanged();
    }
}
```

### Krok 10: Implementacja HandleRemoveCompanionAsync z dialogiem
```csharp
private async Task HandleRemoveCompanionAsync(Guid companionId)
{
    var companion = companions.FirstOrDefault(c => c.Id == companionId);
    if (companion == null) return;

    var parameters = new DialogParameters 
    { 
        { "FirstName", companion.FirstName },
        { "LastName", companion.LastName }
    };
    
    var dialog = await DialogService.ShowAsync<DeleteCompanionConfirmationDialog>(
        "Potwierdzenie usunięcia", 
        parameters);
    
    var result = await dialog.Result;
    
    if (result.Canceled) return;

    try
    {
        await CompanionService.RemoveCompanionAsync(companionId);
        
        // Odświeżenie listy towarzyszy
        companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
        
        Snackbar.Add("Towarzysz został usunięty.", Severity.Success);
    }
    catch (NotFoundException)
    {
        Snackbar.Add("Nie znaleziono towarzysza.", Severity.Warning);
        // Odświeżenie listy (może został już usunięty)
        companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
    }
    catch (DatabaseException ex)
    {
        Snackbar.Add("Nie udało się usunąć towarzysza. Spróbuj ponownie.", Severity.Error);
    }
    finally
    {
        StateHasChanged();
    }
}
```

### Krok 11: Utworzenie komponentu CompanionForm.razor
- Utwórz plik `MotoNomad.App/Shared/Components/CompanionForm.razor`
- Implementuj zgodnie ze specyfikacją w sekcji 4.3
- Struktura analogiczna do TripForm.razor

### Krok 12: Utworzenie komponentu CompanionList.razor
- Utwórz plik `MotoNomad.App/Shared/Components/CompanionList.razor`
- Implementuj zgodnie ze specyfikacją w sekcji 4.4

### Krok 13: Utworzenie dialogów potwierdzenia
- Utwórz `MotoNomad.App/Shared/Dialogs/DeleteTripConfirmationDialog.razor`
- Utwórz `MotoNomad.App/Shared/Dialogs/DeleteCompanionConfirmationDialog.razor`
- Implementuj zgodnie ze specyfikacjami w sekcjach 4.5 i 4.6

### Krok 14: Implementacja breadcrumbs
```csharp
private List<BreadcrumbItem> breadcrumbItems => new()
{
    new BreadcrumbItem("Moje wycieczki", href: "/trips"),
    new BreadcrumbItem(trip?.Name ?? "Wycieczka", href: null, disabled: true)
};
```

### Krok 15: Testy
- Przetestuj ładowanie wycieczki (równoległe Task.WhenAll)
- Przetestuj RLS security (próba dostępu do cudzej wycieczki)
- Przetestuj edycję wycieczki (zapisywanie zmian)
- Przetestuj usuwanie wycieczki (dialog + przekierowanie)
- Przetestuj dodawanie towarzysza (formularz + odświeżenie listy)
- Przetestuj usuwanie towarzysza (dialog + odświeżenie listy)
- Przetestuj przełączanie zakładek
- Przetestuj EmptyState (brak towarzyszy)
- Przetestuj wszystkie scenariusze błędów
- Przetestuj responsywność (mobile + desktop)
- Przetestuj dostępność (keyboard navigation)

### Krok 16: Dokumentacja
- Dodaj komentarze XML dla publicznych metod
- Udokumentuj równoległe ładowanie danych (Task.WhenAll)
- Udokumentuj RLS security handling
- Zaktualizuj README