# Plan implementacji komponentów reużywalnych

## 1. Przegląd

Dokument opisuje implementację wszystkich reużywalnych komponentów współdzielonych (shared components) w aplikacji MotoNomad. Komponenty te są używane w różnych widokach i zapewniają spójność UI oraz DRY (Don't Repeat Yourself). Katalog: `MotoNomad.App/Shared/Components/`.

## 2. Lista komponentów

1. **EmptyState.razor** - Przyjazny komunikat o braku danych
2. **LoadingSpinner.razor** - Uniwersalny spinner ładowania
3. **TripListItem.razor** - Karta wycieczki na liście
4. **CompanionList.razor** - Lista towarzyszy podróży
5. **CompanionForm.razor** - Formularz dodawania towarzysza
6. **TripForm.razor** - Formularz wycieczki (create/edit)

---

## 3. EmptyState.razor

### 3.1 Przegląd

Uniwersalny komponent wyświetlający przyjazny komunikat gdy brak danych do wyświetlenia. Używany w:
- TripList.razor (brak wycieczek w zakładce)
- TripDetails.razor (brak towarzyszy)

### 3.2 Struktura komponentu

```
EmptyState.razor
└── MudPaper (Elevation=0, Class="pa-8 text-center")
    ├── MudIcon (Size.Large, IconName)
    ├── MudText (Typo.h5) - Title
    ├── MudText (Typo.body1, Color.Secondary) - Message
    └── MudButton (opcjonalnie) - ButtonText
```

### 3.3 Parametry (Props)

```csharp
[Parameter] public string Title { get; set; } = string.Empty;
[Parameter] public string Message { get; set; } = string.Empty;
[Parameter] public string IconName { get; set; } = Icons.Material.Filled.Info;
[Parameter] public string? ButtonText { get; set; }
[Parameter] public EventCallback OnButtonClick { get; set; }
```

### 3.4 Implementacja

```razor
<MudPaper Elevation="0" Class="pa-8 text-center">
    <MudIcon Icon="@IconName" Size="Size.Large" Color="Color.Secondary" Class="mb-4" />
    
    <MudText Typo="Typo.h5" Class="mb-2">@Title</MudText>
    
    <MudText Typo="Typo.body1" Color="Color.Secondary" Class="mb-4">@Message</MudText>
    
    @if (!string.IsNullOrEmpty(ButtonText))
    {
        <MudButton 
            Variant="Variant.Filled" 
            Color="Color.Primary"
            OnClick="@OnButtonClick">
            @ButtonText
        </MudButton>
    }
</MudPaper>
```

### 3.5 Przykłady użycia

**TripList - brak nadchodzących wycieczek:**
```razor
<EmptyState 
    Title="Brak nadchodzących wycieczek"
    Message="Zacznij planować swoją pierwszą przygodę!"
    IconName="@Icons.Material.Filled.Map"
    ButtonText="Dodaj pierwszą wycieczkę"
    OnButtonClick="@(() => NavigationManager.NavigateTo("/trip/create"))" />
```

**TripDetails - brak towarzyszy:**
```razor
<EmptyState 
    Title="Brak towarzyszy"
    Message="Dodaj osoby, które będą Cię towarzyszyć w podróży"
    IconName="@Icons.Material.Filled.People"
    ButtonText="Dodaj pierwszego towarzysza"
    OnButtonClick="@(() => showCompanionForm = true)" />
```

---

## 4. LoadingSpinner.razor

### 4.1 Przegląd

Uniwersalny komponent wyświetlający spinner ładowania z opcjonalnym komunikatem tekstowym. Używany w:
- TripList.razor (ładowanie wycieczek)
- TripDetails.razor (ładowanie wycieczki i towarzyszy)
- Wszystkie widoki podczas operacji asynchronicznych

### 4.2 Struktura komponentu

```
LoadingSpinner.razor
└── <div> (style: display:flex, justify-content:center, align-items:center, flex-direction:column, padding:3rem)
    ├── MudProgressCircular (Indeterminate, Size.Large, Color.Primary)
    └── MudText (Typo.body2, Class="mt-2") - Message [opcjonalnie]
```

### 4.3 Parametry (Props)

```csharp
[Parameter] public string? Message { get; set; }
[Parameter] public Size Size { get; set; } = Size.Large;
```

### 4.4 Implementacja

```razor
<div style="display: flex; justify-content: center; align-items: center; flex-direction: column; padding: 3rem;">
    <MudProgressCircular 
        Indeterminate="true" 
        Size="@Size" 
        Color="Color.Primary" />
    
    @if (!string.IsNullOrEmpty(Message))
    {
        <MudText Typo="Typo.body2" Class="mt-2">@Message</MudText>
    }
</div>
```

### 4.5 Przykłady użycia

**TripList - ładowanie wycieczek:**
```razor
@if (isLoadingUpcoming)
{
    <LoadingSpinner Message="Ładowanie wycieczek..." />
}
```

**TripDetails - globalny loader:**
```razor
@if (isLoadingTrip || isLoadingCompanions)
{
    <LoadingSpinner Message="Ładowanie wycieczki..." />
}
```

**Inline (w przycisku):**
```razor
<MudButton Disabled="@isLoading">
    @if (isLoading)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate="true" />
    }
    else
    {
        <text>Zapisz</text>
    }
</MudButton>
```

---

## 5. TripListItem.razor

### 5.1 Przegląd

Komponent reprezentujący pojedynczą wycieczkę jako kartę (card). Wyświetla kluczowe informacje: ikonę transportu, nazwę, daty, czas trwania i liczbę towarzyszy. Karta jest klikalna i prowadzi do szczegółów wycieczki. Używany w:
- TripList.razor (lista nadchodzących i archiwalnych wycieczek)

### 5.2 Struktura komponentu

```
TripListItem.razor
└── MudCard (Clickable, OnClick, Style="cursor:pointer")
    ├── MudCardHeader
    │   └── CardHeaderContent
    │       └── <div> (flex, align-items:center, gap:10px)
    │           ├── MudIcon (GetTransportIcon(), Size.Large)
    │           └── MudText (Typo.h6) - Trip.Name
    └── MudCardContent
        ├── MudText (Typo.body2) - Daty (dd.MM.yyyy - dd.MM.yyyy)
        ├── MudText (Typo.body2, Color.Secondary) - Czas trwania
        └── MudChip (Size.Small, Icon=People) - Liczba towarzyszy
```

### 5.3 Parametry (Props)

```csharp
[Parameter] public TripListItemDto Trip { get; set; } = null!;
[Parameter] public EventCallback<Guid> OnTripClick { get; set; }
```

### 5.4 Typy

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

### 5.5 Implementacja

```razor
<MudCard OnClick="@(() => OnTripClick.InvokeAsync(Trip.Id))" Style="cursor: pointer; transition: box-shadow 0.3s;">
    <MudCardHeader>
        <CardHeaderContent>
            <div style="display: flex; align-items: center; gap: 10px;">
                <MudIcon Icon="@GetTransportIcon()" Size="Size.Large" Color="Color.Primary" />
                <MudText Typo="Typo.h6">@Trip.Name</MudText>
            </div>
        </CardHeaderContent>
    </MudCardHeader>
    
    <MudCardContent>
        <MudText Typo="Typo.body2" Class="mb-1">
            @Trip.StartDate.ToString("dd.MM.yyyy") - @Trip.EndDate.ToString("dd.MM.yyyy")
        </MudText>
        
        <MudText Typo="Typo.body2" Color="Color.Secondary" Class="mb-2">
            (@GetDurationLabel())
        </MudText>
        
        <MudChip Size="Size.Small" Icon="@Icons.Material.Filled.People">
            @GetCompanionLabel()
        </MudChip>
    </MudCardContent>
</MudCard>

@code {
    [Parameter] public TripListItemDto Trip { get; set; } = null!;
    [Parameter] public EventCallback<Guid> OnTripClick { get; set; }

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

### 5.6 Stylizacja (opcjonalna)

Dodaj hover effect w CSS:
```css
.trip-card:hover {
    box-shadow: 0 8px 16px rgba(0,0,0,0.15);
}
```

### 5.7 Przykład użycia

```razor
@foreach (var trip in upcomingTrips)
{
    <MudItem xs="12" sm="6" md="4">
        <TripListItem Trip="@trip" OnTripClick="HandleTripClick" />
    </MudItem>
}
```

---

## 6. CompanionList.razor

### 6.1 Przegląd

Komponent wyświetlający listę towarzyszy jako `MudList` (responsywna). Każdy towarzysz jako `MudListItem` z imieniem, nazwiskiem, kontaktem (jeśli jest) oraz ikoną kosza (usuwanie). Używany w:
- TripDetails.razor (zakładka "Towarzysze")

### 6.2 Struktura komponentu

```
CompanionList.razor
└── MudList
    └── MudListItem (dla każdego towarzysza)
        ├── <div> (flex, justify-content:space-between, align-items:center, width:100%)
        │   ├── <div>
        │   │   ├── MudText (Typo.body1) - Imię Nazwisko
        │   │   └── MudText (Typo.body2, Color.Secondary) - Kontakt [jeśli jest]
        │   └── MudIconButton (Icon=Delete, Color.Error) - Usuwanie
```

### 6.3 Parametry (Props)

```csharp
[Parameter] public List<CompanionListItemDto> Companions { get; set; } = new();
[Parameter] public EventCallback<Guid> OnRemove { get; set; }
```

### 6.4 Typy

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

### 6.5 Implementacja

```razor
<MudList>
    @foreach (var companion in Companions)
    {
        <MudListItem>
            <div style="display: flex; justify-content: space-between; align-items: center; width: 100%;">
                <div>
                    <MudText Typo="Typo.body1">
                        @companion.FirstName @companion.LastName
                    </MudText>
                    
                    @if (!string.IsNullOrEmpty(companion.Contact))
                    {
                        <MudText Typo="Typo.body2" Color="Color.Secondary">
                            @companion.Contact
                        </MudText>
                    }
                </div>
                
                <MudIconButton 
                    Icon="@Icons.Material.Filled.Delete"
                    Color="Color.Error"
                    Size="Size.Small"
                    OnClick="@(() => OnRemove.InvokeAsync(companion.Id))"
                    Title="Usuń towarzysza" />
            </div>
        </MudListItem>
    }
</MudList>

@code {
    [Parameter] public List<CompanionListItemDto> Companions { get; set; } = new();
    [Parameter] public EventCallback<Guid> OnRemove { get; set; }
}
```

### 6.6 Przykład użycia

```razor
@if (companions.Count > 0)
{
    <CompanionList 
        Companions="@companions"
        OnRemove="HandleRemoveCompanionAsync" />
}
```

---

## 7. CompanionForm.razor

### 7.1 Przegląd

Formularz dodawania nowego towarzysza do wycieczki. Zawiera pola: Imię (required), Nazwisko (required), Kontakt (opcjonalne). Używany w:
- TripDetails.razor (zakładka "Towarzysze", domyślnie ukryty)

### 7.2 Struktura komponentu

```
CompanionForm.razor
└── MudForm (@ref="form")
    ├── MudTextField (Imię)
    ├── MudTextField (Nazwisko)
    ├── MudTextField (Kontakt, opcjonalnie)
    └── <div> (przyciski)
        ├── MudButton ("Zapisz", Primary)
        └── MudButton ("Anuluj", Secondary)
```

### 7.3 Parametry (Props)

```csharp
[Parameter] public Guid TripId { get; set; }
[Parameter] public EventCallback<AddCompanionCommand> OnSubmit { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public bool IsLoading { get; set; }
```

### 7.4 Typy

```csharp
public record AddCompanionCommand
{
    public required Guid TripId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
```

**ViewModel (wewnętrzny):**
```csharp
private class CompanionFormViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Contact { get; set; }
}
```

### 7.5 Implementacja

```razor
<MudForm @ref="form" @bind-IsValid="formValid">
    <MudTextField 
        @bind-Value="model.FirstName"
        Label="Imię"
        Required="true"
        MaxLength="100"
        HelperText="Max 100 znaków"
        Disabled="@IsLoading"
        Class="mb-3" />
    
    <MudTextField 
        @bind-Value="model.LastName"
        Label="Nazwisko"
        Required="true"
        MaxLength="100"
        HelperText="Max 100 znaków"
        Disabled="@IsLoading"
        Class="mb-3" />
    
    <MudTextField 
        @bind-Value="model.Contact"
        Label="Kontakt (opcjonalnie)"
        MaxLength="255"
        HelperText="Email lub numer telefonu"
        Disabled="@IsLoading"
        Class="mb-3" />
    
    <div style="display: flex; gap: 10px; justify-content: flex-end;">
        <MudButton 
            Variant="Variant.Text"
            Color="Color.Default"
            OnClick="@(() => OnCancel.InvokeAsync())"
            Disabled="@IsLoading">
            Anuluj
        </MudButton>
        
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
    </div>
</MudForm>

@code {
    [Parameter] public Guid TripId { get; set; }
    [Parameter] public EventCallback<AddCompanionCommand> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private MudForm form = null!;
    private CompanionFormViewModel model = new();
    private bool formValid;

    private class CompanionFormViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Contact { get; set; }
    }

    private async Task HandleSubmit()
    {
        await form.Validate();
        if (!form.IsValid) return;

        var command = new AddCompanionCommand
        {
            TripId = TripId,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Contact = string.IsNullOrWhiteSpace(model.Contact) 
                ? null 
                : model.Contact.Trim()
        };

        await OnSubmit.InvokeAsync(command);
        
        // Reset formularza po udanym zapisie
        model = new CompanionFormViewModel();
    }
}
```

### 7.6 Przykład użycia

```razor
@if (showCompanionForm)
{
    <CompanionForm 
        TripId="@trip.Id"
        OnSubmit="HandleAddCompanionAsync"
        OnCancel="@(() => showCompanionForm = false)"
        IsLoading="@isAddingCompanion" />
}
```

---

## 8. TripForm.razor

### 8.1 Przegląd

Reużywalny komponent formularza wycieczki używany zarówno do tworzenia (`/trip/create`) jak i edycji (`/trip/{id}`). W trybie tworzenia pola są puste, w trybie edycji są wypełnione danymi z parametru `Trip`.

### 8.2 Struktura komponentu

```
TripForm.razor
└── MudForm (@ref="form")
    ├── MudTextField (Nazwa)
    ├── MudDatePicker (Data rozpoczęcia)
    ├── MudDatePicker (Data zakończenia, z custom validation)
    ├── MudTextField (Opis, multiline, opcjonalnie)
    ├── MudSelect<TransportType> (Rodzaj transportu)
    └── <div> (przyciski) [opcjonalnie, jeśli nie renderowane przez rodzica]
        ├── MudButton ("Zapisz", Primary)
        └── MudButton ("Anuluj", Secondary)
```

### 8.3 Parametry (Props)

```csharp
[Parameter] public TripDetailDto? Trip { get; set; }
[Parameter] public EventCallback<object> OnSubmit { get; set; } // object = CreateTripCommand lub UpdateTripCommand
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public bool IsLoading { get; set; }
[Parameter] public bool ShowButtons { get; set; } = true; // Czy renderować przyciski akcji
```

### 8.4 Typy

**CreateTripCommand:**
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

**UpdateTripCommand:**
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

**ViewModel (wewnętrzny):**
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

### 8.5 Implementacja

```razor
<MudForm @ref="form" @bind-IsValid="formValid">
    <MudTextField 
        @bind-Value="model.Name"
        Label="Nazwa wycieczki"
        Required="true"
        MaxLength="200"
        HelperText="Max 200 znaków"
        Disabled="@IsLoading"
        Class="mb-3"
        @ref="nameField" />
    
    <MudDatePicker 
        @bind-Date="model.StartDate"
        Label="Data rozpoczęcia"
        DateFormat="dd.MM.yyyy"
        Required="true"
        Disabled="@IsLoading"
        Class="mb-3" />
    
    <MudDatePicker 
        @bind-Date="model.EndDate"
        Label="Data zakończenia"
        DateFormat="dd.MM.yyyy"
        Required="true"
        Validation="@ValidateEndDate"
        Disabled="@IsLoading"
        Class="mb-3" />
    
    <MudTextField 
        @bind-Value="model.Description"
        Label="Opis (opcjonalnie)"
        Lines="3"
        MaxLength="2000"
        HelperText="Max 2000 znaków"
        Disabled="@IsLoading"
        Class="mb-3" />
    
    <MudSelect 
        @bind-Value="model.TransportType"
        Label="Rodzaj transportu"
        Required="true"
        Disabled="@IsLoading"
        Class="mb-3">
        <MudSelectItem Value="@TransportType.Motorcycle">Motocykl 🏍️</MudSelectItem>
        <MudSelectItem Value="@TransportType.Airplane">Samolot ✈️</MudSelectItem>
        <MudSelectItem Value="@TransportType.Train">Pociąg 🚂</MudSelectItem>
        <MudSelectItem Value="@TransportType.Car">Samochód 🚗</MudSelectItem>
        <MudSelectItem Value="@TransportType.Other">Inne 🌍</MudSelectItem>
    </MudSelect>
    
    @if (ShowButtons)
    {
        <div style="display: flex; gap: 10px; justify-content: flex-end; margin-top: 1rem;">
            <MudButton 
                Variant="Variant.Text"
                Color="Color.Default"
                OnClick="@(() => OnCancel.InvokeAsync())"
                Disabled="@IsLoading">
                Anuluj
            </MudButton>
            
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
                    <text>@(Trip == null ? "Zapisz" : "Zapisz zmiany")</text>
                }
            </MudButton>
        </div>
    }
</MudForm>

@code {
    [Parameter] public TripDetailDto? Trip { get; set; }
    [Parameter] public EventCallback<object> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool ShowButtons { get; set; } = true;

    private MudForm form = null!;
    private TripFormViewModel model = new();
    private bool formValid;
    private ElementReference nameField;

    private class TripFormViewModel
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public TransportType TransportType { get; set; } = TransportType.Motorcycle;
    }

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
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Trip == null) // Tylko w trybie tworzenia
        {
            await nameField.FocusAsync();
        }
    }

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

    private async Task HandleSubmit()
    {
        await form.Validate();
        if (!form.IsValid) return;

        object command;
        
        if (Trip == null)
        {
            // Tryb tworzenia - CreateTripCommand
            command = new CreateTripCommand
            {
                Name = model.Name.Trim(),
                StartDate = DateOnly.FromDateTime(model.StartDate!.Value),
                EndDate = DateOnly.FromDateTime(model.EndDate!.Value),
                Description = string.IsNullOrWhiteSpace(model.Description) 
                    ? null 
                    : model.Description.Trim(),
                TransportType = model.TransportType
            };
        }
        else
        {
            // Tryb edycji - UpdateTripCommand
            command = new UpdateTripCommand
            {
                Id = Trip.Id,
                Name = model.Name.Trim(),
                StartDate = DateOnly.FromDateTime(model.StartDate!.Value),
                EndDate = DateOnly.FromDateTime(model.EndDate!.Value),
                Description = string.IsNullOrWhiteSpace(model.Description) 
                    ? null 
                    : model.Description.Trim(),
                TransportType = model.TransportType
            };
        }

        await OnSubmit.InvokeAsync(command);
    }
}
```

### 8.6 Przykłady użycia

**CreateTrip (tryb tworzenia):**
```razor
<TripForm 
    Trip="null" 
    OnSubmit="HandleCreateTripAsync" 
    OnCancel="HandleCancel"
    IsLoading="@isLoading" />
```

**TripDetails (tryb edycji):**
```razor
<TripForm 
    Trip="@trip" 
    OnSubmit="HandleUpdateTripAsync"
    OnCancel="@(() => NavigationManager.NavigateTo("/trips"))"
    IsLoading="@isUpdatingTrip" />
```

---

## 9. Kroki implementacji

### Krok 1: Utworzenie struktury folderów
```
MotoNomad.App/Shared/Components/
├── EmptyState.razor
├── LoadingSpinner.razor
├── TripListItem.razor
├── CompanionList.razor
├── CompanionForm.razor
└── TripForm.razor
```

### Krok 2: Implementacja EmptyState.razor
- Utwórz plik zgodnie z sekcją 3
- Przetestuj z różnymi kombinacjami parametrów

### Krok 3: Implementacja LoadingSpinner.razor
- Utwórz plik zgodnie z sekcją 4
- Przetestuj z różnymi rozmiarami (Small, Medium, Large)

### Krok 4: Implementacja TripListItem.razor
- Utwórz plik zgodnie z sekcją 5
- Przetestuj z różnymi typami transportu
- Przetestuj hover effect

### Krok 5: Implementacja CompanionList.razor
- Utwórz plik zgodnie z sekcją 6
- Przetestuj z listą pustą i niepustą
- Przetestuj responsywność na mobile

### Krok 6: Implementacja CompanionForm.razor
- Utwórz plik zgodnie z sekcją 7
- Przetestuj walidację wszystkich pól
- Przetestuj reset formularza po zapisie

### Krok 7: Implementacja TripForm.razor
- Utwórz plik zgodnie z sekcją 8
- Przetestuj w trybie tworzenia (Trip = null)
- Przetestuj w trybie edycji (Trip != null)
- Przetestuj custom validation dla dat
- Przetestuj autofocus w trybie tworzenia

### Krok 8: Testy integracyjne
- Przetestuj wszystkie komponenty w kontekście widoków
- Sprawdź responsywność na różnych urządzeniach
- Sprawdź dostępność (keyboard navigation, screen readers)

### Krok 9: Dokumentacja
- Dodaj komentarze XML dla parametrów publicznych
- Udokumentuj przykłady użycia w komentarzach
- Zaktualizuj README z informacjami o komponentach reużywalnych