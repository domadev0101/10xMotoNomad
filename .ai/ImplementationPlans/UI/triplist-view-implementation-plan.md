# Plan implementacji widoku TripList

## 1. Przegląd

Widok TripList (`/trips`) jest główną stroną aplikacji MotoNomad po zalogowaniu. Wyświetla wszystkie wycieczki użytkownika podzielone na dwie zakładki: "Nadchodzące" (domyślna) i "Archiwalne". Każda wycieczka jest reprezentowana jako karta (TripListItem) z kluczowymi informacjami: nazwa, daty, czas trwania, rodzaj transportu i liczba towarzyszy. Widok zawiera Floating Action Button do szybkiego tworzenia nowej wycieczki oraz obsługuje stan pusty (brak wycieczek) z przyjaznym komunikatem i przyciskiem akcji.

## 2. Routing widoku

**Ścieżka:** `/trips`

**Dostępność:** Chroniona (wymagane uwierzytelnienie)

**Routing w App.razor:**
```csharp
<AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
    <NotAuthorized>
        <RedirectToLogin />
    </NotAuthorized>
</AuthorizeRouteView>
```

**Przekierowanie po zalogowaniu:** To jest domyślna strona po zalogowaniu

**Nawigacja z widoku:**
- Kliknięcie karty wycieczki → `/trip/{id}`
- Floating Action Button "Nowa wycieczka" → `/trip/create`

## 3. Struktura komponentów

```
TripList.razor (Strona główna)
├── MudContainer
│   ├── MudText (Nagłówek: "Moje wycieczki")
│   ├── MudTabs (@bind-ActivePanelIndex)
│   │   ├── MudTabPanel (Label: "Nadchodzące")
│   │   │   ├── LoadingSpinner.razor [podczas ładowania]
│   │   │   ├── EmptyState.razor [jeśli brak wycieczek]
│   │   │   └── MudGrid
│   │   │       └── TripListItem.razor (n razy)
│   │   └── MudTabPanel (Label: "Archiwalne")
│   │       ├── LoadingSpinner.razor [podczas ładowania]
│   │       ├── EmptyState.razor [jeśli brak wycieczek]
│   │       └── MudGrid
│   │           └── TripListItem.razor (n razy)
│   └── MudFab (Floating Action Button: "Nowa wycieczka")
│
├── Komponenty potomne:
│   ├── TripListItem.razor (wyświetla pojedynczą wycieczkę)
│   ├── EmptyState.razor (stan pusty)
│   └── LoadingSpinner.razor (stan ładowania)
```

## 4. Szczegóły komponentów

### 4.1 TripList.razor (Komponent strony)

**Opis komponentu:**
Główny komponent strony listy wycieczek. Zarządza stanem ładowania danych, przełączaniem między zakładkami (Nadchodzące/Archiwalne), wyświetlaniem kart wycieczek i obsługą stanów pustych. Wywołuje API równolegle dla obu zakładek podczas inicjalizacji.

**Główne elementy:**
- `MudContainer` (MaxWidth.Large) - kontener główny
- `MudText` (Typo.h4) - nagłówek "Moje wycieczki"
- `MudTabs` - system zakładek z aktywną zakładką "Nadchodzące"
- `MudTabPanel` x2 - zakładki dla nadchodzących i archiwalnych wycieczek
- `MudGrid` - siatka responsywna dla kart wycieczek
- `TripListItem.razor` - komponent karty wycieczki (renderowany dla każdej wycieczki)
- `EmptyState.razor` - wyświetlany jeśli `trips.Count == 0`
- `LoadingSpinner.razor` - wyświetlany podczas `isLoading == true`
- `MudFab` - Floating Action Button (+) do tworzenia nowej wycieczki

**Obsługiwane zdarzenia:**
- `OnInitializedAsync()` - ładowanie danych z API (Task.WhenAll dla obu list)
- `@bind-ActivePanelIndex` - przełączanie zakładek
- `OnTripClick(Guid tripId)` - nawigacja do `/trip/{id}`
- `OnCreateTripClick()` - nawigacja do `/trip/create`

**Warunki walidacji:**
Brak (widok tylko wyświetla dane, nie ma formularzy)

**Typy:**
- `IEnumerable<TripListItemDto>` - lista wycieczek nadchodzących
- `IEnumerable<TripListItemDto>` - lista wycieczek archiwalnych

**Propsy:**
Brak (komponent strony najwyższego poziomu)

**Parametry wstrzykiwane (Dependency Injection):**
- `[Inject] ITripService TripService { get; set; }`
- `[Inject] NavigationManager NavigationManager { get; set; }`
- `[Inject] ISnackbar Snackbar { get; set; }`

### 4.2 TripListItem.razor (Komponent karty wycieczki)

**Opis komponentu:**
Reużywalny komponent reprezentujący pojedynczą wycieczkę jako kartę. Wyświetla kluczowe informacje: ikonę transportu, nazwę, daty (dd.MM.yyyy - dd.MM.yyyy), czas trwania i liczbę towarzyszy. Karta jest klikalna i prowadzi do szczegółów wycieczki.

**Główne elementy:**
- `MudCard` (Clickable, OnClick) - kontener karty
- `MudCardHeader` - nagłówek z ikoną transportu i nazwą wycieczki
  - `MudIcon` - ikona transportu (motocykl, samolot, pociąg, samochód, inne)
  - `MudText` (Typo.h6) - nazwa wycieczki
- `MudCardContent` - zawartość karty
  - `MudText` (Typo.body2) - daty wycieczki
  - `MudText` (Typo.body2) - czas trwania (dni)
  - `MudChip` (Size.Small) - liczba towarzyszy z ikoną 👥

**Obsługiwane zdarzenia:**
- `OnClick` (MudCard) - wywołuje `OnTripClick` callback z `Trip.Id`

**Warunki walidacji:**
Brak (komponent tylko wyświetla dane)

**Typy:**
- `TripListItemDto` - dane wycieczki

**Propsy:**
```csharp
[Parameter] public TripListItemDto Trip { get; set; } = null!;
[Parameter] public EventCallback<Guid> OnTripClick { get; set; }
```

### 4.3 EmptyState.razor (Komponent stanu pustego)

**Opis komponentu:**
Uniwersalny komponent wyświetlający przyjazny komunikat gdy brak danych do wyświetlenia. Używany w widoku TripList gdy użytkownik nie ma żadnych wycieczek w danej zakładce.

**Główne elementy:**
- `MudPaper` (Elevation=0, centrowany tekst)
- `MudIcon` (Size.Large) - duża ikona ilustrująca stan pusty
- `MudText` (Typo.h5) - tytuł komunikatu
- `MudText` (Typo.body1) - dodatkowy opis
- `MudButton` (opcjonalnie) - przycisk akcji (np. "Dodaj pierwszą wycieczkę")

**Obsługiwane zdarzenia:**
- `OnButtonClick` - callback przycisku akcji (jeśli przycisk jest renderowany)

**Typy:**
Brak (komponent tylko prezentacyjny)

**Propsy:**
```csharp
[Parameter] public string Title { get; set; } = string.Empty;
[Parameter] public string Message { get; set; } = string.Empty;
[Parameter] public string IconName { get; set; } = Icons.Material.Filled.Info;
[Parameter] public string? ButtonText { get; set; }
[Parameter] public EventCallback OnButtonClick { get; set; }
```

**Przykłady użycia w TripList:**
- Zakładka "Nadchodzące" (brak wycieczek):
  - Title: "Brak nadchodzących wycieczek"
  - Message: "Zacznij planować swoją pierwszą przygodę!"
  - IconName: Icons.Material.Filled.Map
  - ButtonText: "Dodaj pierwszą wycieczkę"
  - OnButtonClick: → `/trip/create`
- Zakładka "Archiwalne" (brak wycieczek):
  - Title: "Brak archiwalnych wycieczek"
  - Message: "Twoje zakończone podróże pojawią się tutaj."
  - IconName: Icons.Material.Filled.History
  - ButtonText: null (brak przycisku)

### 4.4 LoadingSpinner.razor (Komponent ładowania)

**Opis komponentu:**
Uniwersalny komponent wyświetlający spinner ładowania z opcjonalnym komunikatem tekstowym. Używany podczas ładowania danych z API.

**Główne elementy:**
- Kontener centrujący (flexbox)
- `MudProgressCircular` (Indeterminate, Color.Primary, Size.Large)
- `MudText` (Typo.body2, opcjonalnie) - komunikat ładowania

**Obsługiwane zdarzenia:**
Brak

**Typy:**
Brak

**Propsy:**
```csharp
[Parameter] public string? Message { get; set; }
```

**Przykłady użycia w TripList:**
- Ładowanie nadchodzących wycieczek: Message = "Ładowanie wycieczek..."
- Ładowanie archiwalnych wycieczek: Message = "Ładowanie wycieczek..."

## 5. Typy

### TripListItemDto (Response DTO)
```csharp
public record TripListItemDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required int DurationDays { get; init; }
    public required TransportType TransportType { get; init; }
    public required int CompanionCount { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

**Pola:**
- `Id` (Guid) - unikalny identyfikator wycieczki
- `Name` (string) - nazwa wycieczki
- `StartDate` (DateOnly) - data rozpoczęcia
- `EndDate` (DateOnly) - data zakończenia
- `DurationDays` (int) - czas trwania w dniach (obliczony przez backend)
- `TransportType` (enum) - rodzaj transportu (0-4)
- `CompanionCount` (int) - liczba towarzyszy
- `CreatedAt` (DateTime) - data utworzenia wycieczki

### TransportType (Enum)
```csharp
public enum TransportType
{
    Motorcycle = 0,  // 🏍️
    Airplane = 1,    // ✈️
    Train = 2,       // 🚂
    Car = 3,         // 🚗
    Other = 4        // 🌍
}
```

**Mapowanie ikon:**
- `Motorcycle` → `Icons.Material.Filled.TwoWheeler`
- `Airplane` → `Icons.Material.Filled.Flight`
- `Train` → `Icons.Material.Filled.Train`
- `Car` → `Icons.Material.Filled.DirectionsCar`
- `Other` → `Icons.Material.Filled.TravelExplore`

## 6. Zarządzanie stanem

**Zmienne stanu komponentu TripList.razor:**

```csharp
private List<TripListItemDto> upcomingTrips = new();
private List<TripListItemDto> pastTrips = new();
private bool isLoadingUpcoming = false;
private bool isLoadingPast = false;
private int activeTabIndex = 0; // 0 = Nadchodzące (domyślna), 1 = Archiwalne
```

**Opis zmiennych:**
- `upcomingTrips` - lista nadchodzących wycieczek (startDate >= today)
- `pastTrips` - lista archiwalnych wycieczek (startDate < today)
- `isLoadingUpcoming` - flaga ładowania dla zakładki "Nadchodzące"
- `isLoadingPast` - flaga ładowania dla zakładki "Archiwalne"
- `activeTabIndex` - indeks aktywnej zakładki (0 lub 1)

**Przepływ stanu:**

1. **Inicjalizacja (`OnInitializedAsync`):**
   ```csharp
   isLoadingUpcoming = true;
   isLoadingPast = true;
   
   // Równoległe ładowanie obu list (Task.WhenAll)
   var upcomingTask = TripService.GetUpcomingTripsAsync();
   var pastTask = TripService.GetPastTripsAsync();
   
   await Task.WhenAll(upcomingTask, pastTask);
   
   upcomingTrips = upcomingTask.Result.ToList();
   pastTrips = pastTask.Result.ToList();
   
   isLoadingUpcoming = false;
   isLoadingPast = false;
   ```

2. **Przełączanie zakładek:**
   - Użytkownik klika zakładkę → `activeTabIndex` zmienia się na 0 lub 1
   - Zawartość zakładki jest już załadowana (brak ponownego ładowania)

3. **Odświeżanie listy (po powrocie ze szczegółów):**
   - Blazor automatycznie wywołuje `OnInitializedAsync()` przy nawigacji
   - Alternatywnie: implementacja `OnParametersSetAsync()` dla bardziej kontrolowanego odświeżania

4. **Kliknięcie karty wycieczki:**
   - `TripListItem` wywołuje `OnTripClick` callback z `Trip.Id`
   - TripList wywołuje `NavigationManager.NavigateTo($"/trip/{tripId}")`

**Brak potrzeby custom hooka** - stan zarządzany lokalnie w komponencie.

## 7. Integracja API

### 7.1 Ładowanie nadchodzących wycieczek

**Endpoint:** `ITripService.GetUpcomingTripsAsync()`

**Typ żądania:** Brak (metoda bez parametrów)

**Typ odpowiedzi:** `Task<IEnumerable<TripListItemDto>>`

**Sortowanie:** Backend zwraca posortowane ASC według `StartDate` (najbliższa data pierwsza)

**Obsługa sukcesu:**
```csharp
upcomingTrips = (await TripService.GetUpcomingTripsAsync()).ToList();
```

**Obsługa błędów:**
```csharp
catch (UnauthorizedException)
{
    // Użytkownik niezalogowany - przekierowanie na /login
    NavigationManager.NavigateTo("/login");
}
catch (DatabaseException ex)
{
    Snackbar.Add("Nie udało się załadować wycieczek. Spróbuj ponownie.", Severity.Error);
    // Logowanie błędu
}
```

### 7.2 Ładowanie archiwalnych wycieczek

**Endpoint:** `ITripService.GetPastTripsAsync()`

**Typ żądania:** Brak (metoda bez parametrów)

**Typ odpowiedzi:** `Task<IEnumerable<TripListItemDto>>`

**Sortowanie:** Backend zwraca posortowane DESC według `StartDate` (najnowsza data pierwsza)

**Obsługa sukcesu:**
```csharp
pastTrips = (await TripService.GetPastTripsAsync()).ToList();
```

**Obsługa błędów:**
Identyczna jak dla nadchodzących wycieczek.

### 7.3 Równoległe ładowanie (optymalizacja)

**Implementacja:**
```csharp
protected override async Task OnInitializedAsync()
{
    isLoadingUpcoming = true;
    isLoadingPast = true;

    try
    {
        // Równoległe wywołania API
        var upcomingTask = TripService.GetUpcomingTripsAsync();
        var pastTask = TripService.GetPastTripsAsync();

        await Task.WhenAll(upcomingTask, pastTask);

        upcomingTrips = upcomingTask.Result.ToList();
        pastTrips = pastTask.Result.ToList();
    }
    catch (UnauthorizedException)
    {
        NavigationManager.NavigateTo("/login");
    }
    catch (Exception ex)
    {
        Snackbar.Add("Wystąpił błąd podczas ładowania wycieczek.", Severity.Error);
        // Logowanie błędu
    }
    finally
    {
        isLoadingUpcoming = false;
        isLoadingPast = false;
        StateHasChanged();
    }
}
```

**Korzyści:**
- Oba wywołania API wykonują się równocześnie (szybsze ładowanie)
- Jeden blok try-catch dla obu operacji
- Użytkownik widzi pełne dane szybciej

## 8. Interakcje użytkownika

### 8.1 Ładowanie strony
**Akcja:** Użytkownik wchodzi na `/trips`  
**Efekt:**
- Wyświetlenie LoadingSpinner w obu zakładkach (`isLoadingUpcoming` i `isLoadingPast` = true)
- Równoległe wywołanie `GetUpcomingTripsAsync()` i `GetPastTripsAsync()`
- Po załadowaniu: wyświetlenie kart wycieczek lub EmptyState
- Domyślna zakładka: "Nadchodzące" (activeTabIndex = 0)

### 8.2 Przełączanie zakładek
**Akcja:** Użytkownik klika zakładkę "Archiwalne"  
**Efekt:**
- `activeTabIndex` zmienia się na 1
- Wyświetlenie zawartości zakładki (lista wycieczek lub EmptyState)
- Brak ponownego ładowania danych (już załadowane przy inicjalizacji)

### 8.3 Kliknięcie karty wycieczki
**Akcja:** Użytkownik klika kartę wycieczki (TripListItem)  
**Efekt:**
- Wywołanie `OnTripClick` callback z `Trip.Id`
- Nawigacja do `/trip/{id}`
- Załadowanie szczegółów wycieczki

### 8.4 Kliknięcie Floating Action Button
**Akcja:** Użytkownik klika FAB "+" w prawym dolnym rogu  
**Efekt:**
- Nawigacja do `/trip/create`
- Otwarcie formularza tworzenia nowej wycieczki

### 8.5 Kliknięcie przycisku w EmptyState
**Akcja:** Użytkownik klika "Dodaj pierwszą wycieczkę" w EmptyState  
**Efekt:**
- Nawigacja do `/trip/create`
- Identyczny efekt jak FAB

### 8.6 Odświeżanie listy (powrót z innych stron)
**Akcja:** Użytkownik wraca na `/trips` z `/trip/{id}` lub `/trip/create`  
**Efekt:**
- Blazor automatycznie wywołuje `OnInitializedAsync()`
- Ponowne załadowanie obu list (aktualizacja danych)
- Wyświetlenie zaktualizowanej listy wycieczek

## 9. Warunki i walidacja

### 9.1 Wyświetlanie LoadingSpinner

**Warunek:** `isLoadingUpcoming == true` lub `isLoadingPast == true`

**Komponent:** Zakładka "Nadchodzące" lub "Archiwalne"

**Efekt:**
- Wyświetlenie `LoadingSpinner.razor` z komunikatem "Ładowanie wycieczek..."
- Ukrycie listy wycieczek i EmptyState

### 9.2 Wyświetlanie EmptyState

**Warunek:** `upcomingTrips.Count == 0` lub `pastTrips.Count == 0`

**Komponent:** Zakładka "Nadchodzące" lub "Archiwalne"

**Efekt:**
- Wyświetlenie `EmptyState.razor` z odpowiednim komunikatem
- Ukrycie listy wycieczek (MudGrid)
- Dla "Nadchodzące": przycisk "Dodaj pierwszą wycieczkę"
- Dla "Archiwalne": brak przycisku (tylko komunikat)

### 9.3 Wyświetlanie listy wycieczek

**Warunek:** `upcomingTrips.Count > 0` lub `pastTrips.Count > 0`

**Komponent:** Zakładka "Nadchodzące" lub "Archiwalne"

**Efekt:**
- Wyświetlenie `MudGrid` z kartami wycieczek (TripListItem)
- Każda karta klikalna (nawigacja do szczegółów)
- Responsywna siatka (1 kolumna mobile, 2-3 kolumny desktop)

### 9.4 Formatowanie dat w TripListItem

**Warunek:** Zawsze (dla każdej wycieczki)

**Format:** `dd.MM.yyyy - dd.MM.yyyy`

**Przykład:** "15.06.2025 - 22.06.2025"

**Implementacja:**
```csharp
$"{Trip.StartDate:dd.MM.yyyy} - {Trip.EndDate:dd.MM.yyyy}"
```

### 9.5 Wyświetlanie czasu trwania w TripListItem

**Warunek:** Zawsze (dla każdej wycieczki)

**Format:** `({DurationDays} dni)` lub `({DurationDays} dzień)` dla 1 dnia

**Przykład:** "(7 dni)"

**Implementacja:**
```csharp
var daysLabel = Trip.DurationDays == 1 ? "dzień" : "dni";
$"({Trip.DurationDays} {daysLabel})"
```

### 9.6 Wyświetlanie liczby towarzyszy w TripListItem

**Warunek:** Zawsze (dla każdej wycieczki)

**Format:** `MudChip` z ikoną 👥 i tekstem liczby

**Przykład:** "3 towarzyszy" lub "1 towarzysz"

**Implementacja:**
```csharp
var companionLabel = Trip.CompanionCount == 1 ? "towarzysz" : "towarzyszy";
var chipText = Trip.CompanionCount == 0 
    ? "Brak towarzyszy" 
    : $"{Trip.CompanionCount} {companionLabel}";
```

## 10. Obsługa błędów

### 10.1 UnauthorizedException (użytkownik niezalogowany)

**Scenariusz:**
- Sesja wygasła
- Użytkownik próbuje wejść na `/trips` bez zalogowania

**Obsługa:**
```csharp
catch (UnauthorizedException)
{
    NavigationManager.NavigateTo("/login");
}
```

**UI:**
- Automatyczne przekierowanie na `/login`
- Opcjonalnie: Snackbar "Sesja wygasła. Zaloguj się ponownie."

### 10.2 DatabaseException (błąd ładowania danych)

**Scenariusz:**
- Problemy z połączeniem do Supabase
- Błąd zapytania SQL
- Timeout

**Obsługa:**
```csharp
catch (DatabaseException ex)
{
    Snackbar.Add("Nie udało się załadować wycieczek. Spróbuj ponownie.", Severity.Error);
    // Logowanie błędu do konsoli
}
finally
{
    isLoadingUpcoming = false;
    isLoadingPast = false;
}
```

**UI:**
- Snackbar z komunikatem błędu (czerwony)
- Ukrycie LoadingSpinner
- Wyświetlenie EmptyState z komunikatem "Nie udało się załadować wycieczek"
- Opcjonalnie: przycisk "Spróbuj ponownie" w EmptyState

### 10.3 Brak połączenia z internetem

**Scenariusz:**
- Użytkownik offline
- Supabase API niedostępne

**Obsługa:**
```csharp
catch (Exception ex)
{
    Snackbar.Add("Sprawdź połączenie z internetem i spróbuj ponownie.", Severity.Error);
    // Logowanie błędu
}
```

**UI:**
- Snackbar z komunikatem błędu
- EmptyState z ikoną "brak połączenia"
- Przycisk "Spróbuj ponownie" (ponowne wywołanie `OnInitializedAsync`)

### 10.4 Edge cases

**Puste listy w obu zakładkach:**
- Nowy użytkownik bez żadnych wycieczek
- UI: EmptyState w obu zakładkach z przyciskiem "Dodaj pierwszą wycieczkę"

**Wszystkie wycieczki nadchodzące (brak archiwalnych):**
- UI: EmptyState w zakładce "Archiwalne" z komunikatem "Twoje zakończone podróże pojawią się tutaj"

**Wszystkie wycieczki archiwalne (brak nadchodzących):**
- UI: EmptyState w zakładce "Nadchodzące" z przyciskiem "Zaplanuj nową wycieczkę"

**Bardzo długa lista wycieczek (>50):**
- Future enhancement: paginacja lub infinite scroll
- MVP: wyświetlenie wszystkich wycieczek (Supabase radzi sobie z dużymi listami)

## 11. Kroki implementacji

### Krok 1: Utworzenie pliku komponentu głównego
- Utwórz plik `MotoNomad.App/Pages/TripList.razor`
- Ustaw routing: `@page "/trips"`
- Dodaj atrybut: `@attribute [Authorize]` (ochrona trasy)
- Dodaj dyrektywy `@inject` dla ITripService, NavigationManager, ISnackbar

### Krok 2: Implementacja zmiennych stanu
```csharp
@code {
    private List<TripListItemDto> upcomingTrips = new();
    private List<TripListItemDto> pastTrips = new();
    private bool isLoadingUpcoming = false;
    private bool isLoadingPast = false;
    private int activeTabIndex = 0;
}
```

### Krok 3: Utworzenie struktury UI (MudBlazor)
- Dodaj `MudContainer(MaxWidth.Large)` jako główny kontener
- Wewnątrz: `MudText(Typo.h4)` z nagłówkiem "Moje wycieczki"
- Dodaj `MudTabs` z `@bind-ActivePanelIndex="activeTabIndex"`

### Krok 4: Implementacja zakładki "Nadchodzące"
```razor
<MudTabPanel Text="Nadchodzące">
    @if (isLoadingUpcoming)
    {
        <LoadingSpinner Message="Ładowanie wycieczek..." />
    }
    else if (upcomingTrips.Count == 0)
    {
        <EmptyState 
            Title="Brak nadchodzących wycieczek"
            Message="Zacznij planować swoją pierwszą przygodę!"
            IconName="@Icons.Material.Filled.Map"
            ButtonText="Dodaj pierwszą wycieczkę"
            OnButtonClick="@(() => NavigationManager.NavigateTo("/trip/create"))" />
    }
    else
    {
        <MudGrid>
            @foreach (var trip in upcomingTrips)
            {
                <MudItem xs="12" sm="6" md="4">
                    <TripListItem Trip="@trip" OnTripClick="HandleTripClick" />
                </MudItem>
            }
        </MudGrid>
    }
</MudTabPanel>
```

### Krok 5: Implementacja zakładki "Archiwalne"
```razor
<MudTabPanel Text="Archiwalne">
    @if (isLoadingPast)
    {
        <LoadingSpinner Message="Ładowanie wycieczek..." />
    }
    else if (pastTrips.Count == 0)
    {
        <EmptyState 
            Title="Brak archiwalnych wycieczek"
            Message="Twoje zakończone podróże pojawią się tutaj."
            IconName="@Icons.Material.Filled.History" />
    }
    else
    {
        <MudGrid>
            @foreach (var trip in pastTrips)
            {
                <MudItem xs="12" sm="6" md="4">
                    <TripListItem Trip="@trip" OnTripClick="HandleTripClick" />
                </MudItem>
            }
        </MudGrid>
    }
</MudTabPanel>
```

### Krok 6: Dodanie Floating Action Button
```razor
<MudFab 
    Color="Color.Primary" 
    StartIcon="@Icons.Material.Filled.Add" 
    OnClick="@(() => NavigationManager.NavigateTo("/trip/create"))"
    Style="position: fixed; bottom: 20px; right: 20px;" />
```

### Krok 7: Implementacja metody OnInitializedAsync()
```csharp
protected override async Task OnInitializedAsync()
{
    isLoadingUpcoming = true;
    isLoadingPast = true;

    try
    {
        // Równoległe ładowanie
        var upcomingTask = TripService.GetUpcomingTripsAsync();
        var pastTask = TripService.GetPastTripsAsync();

        await Task.WhenAll(upcomingTask, pastTask);

        upcomingTrips = upcomingTask.Result.ToList();
        pastTrips = pastTask.Result.ToList();
    }
    catch (UnauthorizedException)
    {
        NavigationManager.NavigateTo("/login");
    }
    catch (DatabaseException ex)
    {
        Snackbar.Add("Nie udało się załadować wycieczek.", Severity.Error);
        // TODO: Logowanie błędu
    }
    catch (Exception ex)
    {
        Snackbar.Add("Wystąpił nieoczekiwany błąd.", Severity.Error);
        // TODO: Logowanie błędu
    }
    finally
    {
        isLoadingUpcoming = false;
        isLoadingPast = false;
        StateHasChanged();
    }
}
```

### Krok 8: Implementacja metody HandleTripClick()
```csharp
private void HandleTripClick(Guid tripId)
{
    NavigationManager.NavigateTo($"/trip/{tripId}");
}
```

### Krok 9: Utworzenie komponentu TripListItem.razor
- Utwórz plik `MotoNomad.App/Shared/Components/TripListItem.razor`
- Zdefiniuj parametry:
  ```csharp
  [Parameter] public TripListItemDto Trip { get; set; } = null!;
  [Parameter] public EventCallback<Guid> OnTripClick { get; set; }
  ```

### Krok 10: Implementacja UI dla TripListItem
```razor
<MudCard OnClick="@(() => OnTripClick.InvokeAsync(Trip.Id))" Style="cursor: pointer;">
    <MudCardHeader>
        <CardHeaderContent>
            <div style="display: flex; align-items: center; gap: 10px;">
                <MudIcon Icon="@GetTransportIcon()" Size="Size.Large" />
                <MudText Typo="Typo.h6">@Trip.Name</MudText>
            </div>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        <MudText Typo="Typo.body2">
            @Trip.StartDate.ToString("dd.MM.yyyy") - @Trip.EndDate.ToString("dd.MM.yyyy")
        </MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary">
            (@GetDurationLabel())
        </MudText>
        <MudChip Size="Size.Small" Icon="@Icons.Material.Filled.People">
            @GetCompanionLabel()
        </MudChip>
    </MudCardContent>
</MudCard>

@code {
    private string GetTransportIcon() => Trip.TransportType switch
    {
        TransportType.Motorcycle => Icons.Material.Filled.TwoWheeler,
        TransportType.Airplane => Icons.Material.Filled.Flight,
        TransportType.Train => Icons.Material.Filled.Train,
        TransportType.Car => Icons.Material.Filled.DirectionsCar,
        _ => Icons.Material.Filled.TravelExplore
    };

    private string GetDurationLabel()
    {
        var daysLabel = Trip.DurationDays == 1 ? "dzień" : "dni";
        return $"{Trip.DurationDays} {daysLabel}";
    }

    private string GetCompanionLabel()
    {
        if (Trip.CompanionCount == 0) return "Brak towarzyszy";
        var label = Trip.CompanionCount == 1 ? "towarzysz" : "towarzyszy";
        return $"{Trip.CompanionCount} {label}";
    }
}
```

### Krok 11: Utworzenie komponentu EmptyState.razor
- Utwórz plik `MotoNomad.App/Shared/Components/EmptyState.razor`
- Implementuj zgodnie ze specyfikacją w sekcji 4.3

### Krok 12: Utworzenie komponentu LoadingSpinner.razor
- Utwórz plik `MotoNomad.App/Shared/Components/LoadingSpinner.razor`
- Implementuj zgodnie ze specyfikacją w sekcji 4.4

### Krok 13: Stylizacja i responsywność
- Dodaj style CSS dla kart (hover effect)
- Przetestuj responsywność na mobile (1 kolumna) i desktop (2-3 kolumny)
- Upewnij się, że FAB nie zasłania kart na mobile

### Krok 14: Testy
- Przetestuj ładowanie listy (oba zakładki)
- Przetestuj stan pusty (brak wycieczek)
- Przetestuj kliknięcie karty (nawigacja do szczegółów)
- Przetestuj FAB (nawigacja do tworzenia)
- Przetestuj przełączanie zakładek
- Przetestuj błędy (brak połączenia, sesja wygasła)
- Przetestuj równoległe ładowanie (sprawdź sieć w DevTools)
- Przetestuj różne scenariusze:
  - Wszystkie wycieczki nadchodzące
  - Wszystkie wycieczki archiwalne
  - Mix nadchodzących i archiwalnych
  - Nowy użytkownik (brak wycieczek)

### Krok 15: Dokumentacja
- Dodaj komentarze XML dla publicznych metod
- Udokumentuj równoległe ładowanie danych
- Zaktualizuj README z informacjami o głównej stronie