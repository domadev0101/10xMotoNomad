# Plan implementacji dialogów potwierdzenia

## 1. Przegląd

Dokument opisuje implementację dialogów potwierdzenia (confirmation dialogs) w aplikacji MotoNomad. Dialogi używają systemu MudDialog z MudBlazor i służą do potwierdzania krytycznych operacji (usuwanie wycieczki, usuwanie towarzysza). Katalog: `MotoNomad.App/Shared/Dialogs/`.

## 2. Lista dialogów

1. **DeleteTripConfirmationDialog.razor** - Potwierdzenie usunięcia wycieczki
2. **DeleteCompanionConfirmationDialog.razor** - Potwierdzenie usunięcia towarzysza

---

## 3. DeleteTripConfirmationDialog.razor

### 3.1 Przegląd

Dialog MudBlazor z potwierdzeniem usunięcia wycieczki. Wyświetla nazwę wycieczki i komunikat o nieodwracalności operacji. Używany w:
- TripDetails.razor (zakładka "Szczegóły", kliknięcie ikony kosza)

### 3.2 Struktura komponentu

```
DeleteTripConfirmationDialog.razor
└── MudDialog
    ├── DialogContent
    │   ├── MudText (Typo.body1) - Komunikat z nazwą wycieczki
    │   └── MudAlert (Severity.Warning) - Ostrzeżenie o nieodwracalności
    └── DialogActions
        ├── MudButton ("Anuluj", Color.Default)
        └── MudButton ("Usuń", Color.Error)
```

### 3.3 Parametry (Props)

```csharp
[CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
[Parameter] public string TripName { get; set; } = string.Empty;
```

**Opis parametrów:**
- `MudDialog` - Kaskadowy parametr z MudBlazor, umożliwia zamknięcie dialogu i zwrócenie wyniku
- `TripName` - Nazwa wycieczki do wyświetlenia w komunikacie

### 3.4 Implementacja

```razor
<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.body1" Class="mb-3">
            Czy na pewno chcesz usunąć wycieczkę <strong>@TripName</strong>?
        </MudText>
        
        <MudAlert Severity="Severity.Warning" Dense="true">
            Ta operacja jest nieodwracalna. Wszystkie dane związane z wycieczką, 
            w tym lista towarzyszy, zostaną trwale usunięte.
        </MudAlert>
    </DialogContent>
    
    <DialogActions>
        <MudButton 
            Variant="Variant.Text" 
            Color="Color.Default"
            OnClick="Cancel">
            Anuluj
        </MudButton>
        
        <MudButton 
            Variant="Variant.Filled" 
            Color="Color.Error"
            OnClick="Confirm">
            Usuń
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string TripName { get; set; } = string.Empty;

    private void Cancel() => MudDialog.Cancel();
    
    private void Confirm() => MudDialog.Close(DialogResult.Ok(true));
}
```

### 3.5 Wywołanie dialogu (przykład z TripDetails.razor)

```csharp
private async Task HandleDeleteTrip()
{
    // Utworzenie parametrów dialogu
    var parameters = new DialogParameters 
    { 
        { "TripName", trip!.Name } 
    };
    
    // Opcje dialogu
    var options = new DialogOptions 
    { 
        CloseOnEscapeKey = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };
    
    // Otwarcie dialogu
    var dialog = await DialogService.ShowAsync<DeleteTripConfirmationDialog>(
        "Potwierdzenie usunięcia", 
        parameters,
        options);
    
    // Oczekiwanie na wynik
    var result = await dialog.Result;
    
    // Sprawdzenie czy użytkownik potwierdził
    if (result.Canceled) 
        return;

    // Wykonanie operacji usunięcia
    try
    {
        await TripService.DeleteTripAsync(trip.Id);
        Snackbar.Add($"Wycieczka '{trip.Name}' została usunięta.", Severity.Success);
        NavigationManager.NavigateTo("/trips");
    }
    catch (DatabaseException ex)
    {
        Snackbar.Add("Nie udało się usunąć wycieczki. Spróbuj ponownie.", Severity.Error);
    }
}
```

### 3.6 Wygląd dialogu

**Tytuł:** "Potwierdzenie usunięcia"  
**Treść:** 
- "Czy na pewno chcesz usunąć wycieczkę **[Nazwa wycieczki]**?"
- Alert ostrzegawczy: "Ta operacja jest nieodwracalna. Wszystkie dane związane z wycieczką, w tym lista towarzyszy, zostaną trwale usunięte."

**Przyciski:**
- "Anuluj" (domyślny, po lewej)
- "Usuń" (czerwony, po prawej)

### 3.7 Interakcje użytkownika

**Kliknięcie "Anuluj":**
- Zamknięcie dialogu
- Zwrócenie `DialogResult(Canceled = true)`
- Brak dalszych akcji

**Kliknięcie "Usuń":**
- Zamknięcie dialogu
- Zwrócenie `DialogResult(Ok, Data = true)`
- Wywołanie `TripService.DeleteTripAsync()`

**Naciśnięcie Escape:**
- Identyczne działanie jak "Anuluj"

**Kliknięcie poza dialogiem (backdrop):**
- Identyczne działanie jak "Anuluj"

---

## 4. DeleteCompanionConfirmationDialog.razor

### 4.1 Przegląd

Dialog MudBlazor z potwierdzeniem usunięcia towarzysza. Wyświetla imię i nazwisko towarzysza. Używany w:
- TripDetails.razor (zakładka "Towarzysze", kliknięcie ikony kosza przy towarzyszu)

### 4.2 Struktura komponentu

```
DeleteCompanionConfirmationDialog.razor
└── MudDialog
    ├── DialogContent
    │   └── MudText (Typo.body1) - Komunikat z imieniem i nazwiskiem
    └── DialogActions
        ├── MudButton ("Anuluj", Color.Default)
        └── MudButton ("Usuń", Color.Error)
```

### 4.3 Parametry (Props)

```csharp
[CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
[Parameter] public string FirstName { get; set; } = string.Empty;
[Parameter] public string LastName { get; set; } = string.Empty;
```

**Opis parametrów:**
- `MudDialog` - Kaskadowy parametr z MudBlazor
- `FirstName` - Imię towarzysza
- `LastName` - Nazwisko towarzysza

### 4.4 Implementacja

```razor
<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.body1">
            Czy na pewno chcesz usunąć <strong>@FirstName @LastName</strong> z wycieczki?
        </MudText>
    </DialogContent>
    
    <DialogActions>
        <MudButton 
            Variant="Variant.Text" 
            Color="Color.Default"
            OnClick="Cancel">
            Anuluj
        </MudButton>
        
        <MudButton 
            Variant="Variant.Filled" 
            Color="Color.Error"
            OnClick="Confirm">
            Usuń
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string FirstName { get; set; } = string.Empty;
    [Parameter] public string LastName { get; set; } = string.Empty;

    private void Cancel() => MudDialog.Cancel();
    
    private void Confirm() => MudDialog.Close(DialogResult.Ok(true));
}
```

### 4.5 Wywołanie dialogu (przykład z TripDetails.razor)

```csharp
private async Task HandleRemoveCompanionAsync(Guid companionId)
{
    // Znalezienie towarzysza w liście (dla wyświetlenia imienia i nazwiska)
    var companion = companions.FirstOrDefault(c => c.Id == companionId);
    if (companion == null) return;

    // Utworzenie parametrów dialogu
    var parameters = new DialogParameters 
    { 
        { "FirstName", companion.FirstName },
        { "LastName", companion.LastName }
    };
    
    // Opcje dialogu
    var options = new DialogOptions 
    { 
        CloseOnEscapeKey = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };
    
    // Otwarcie dialogu
    var dialog = await DialogService.ShowAsync<DeleteCompanionConfirmationDialog>(
        "Potwierdzenie usunięcia", 
        parameters,
        options);
    
    // Oczekiwanie na wynik
    var result = await dialog.Result;
    
    // Sprawdzenie czy użytkownik potwierdził
    if (result.Canceled) 
        return;

    // Wykonanie operacji usunięcia
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

### 4.6 Wygląd dialogu

**Tytuł:** "Potwierdzenie usunięcia"  
**Treść:** "Czy na pewno chcesz usunąć **[Imię] [Nazwisko]** z wycieczki?"

**Przyciski:**
- "Anuluj" (domyślny, po lewej)
- "Usuń" (czerwony, po prawej)

**Uwaga:** Ten dialog jest prostszy niż DeleteTripConfirmationDialog - brak alertu ostrzegawczego, ponieważ usunięcie towarzysza jest mniej krytyczną operacją (nie usuwa całej wycieczki).

### 4.7 Interakcje użytkownika

**Kliknięcie "Anuluj":**
- Zamknięcie dialogu
- Zwrócenie `DialogResult(Canceled = true)`
- Brak dalszych akcji

**Kliknięcie "Usuń":**
- Zamknięcie dialogu
- Zwrócenie `DialogResult(Ok, Data = true)`
- Wywołanie `CompanionService.RemoveCompanionAsync()`
- Odświeżenie listy towarzyszy

**Naciśnięcie Escape:**
- Identyczne działanie jak "Anuluj"

**Kliknięcie poza dialogiem (backdrop):**
- Identyczne działanie jak "Anuluj"

---

## 5. Wspólne elementy i best practices

### 5.1 DialogOptions (wspólne dla obu dialogów)

```csharp
var options = new DialogOptions 
{ 
    CloseOnEscapeKey = true,        // Zamknięcie przez Escape
    MaxWidth = MaxWidth.Small,      // Szerokość dialogu (Small = 600px)
    FullWidth = true,               // Wykorzystanie pełnej dostępnej szerokości
    DisableBackdropClick = false,   // Zamknięcie przez kliknięcie poza dialogiem
    Position = DialogPosition.Center // Wyśrodkowanie na ekranie
};
```

### 5.2 Wzorzec wywołania dialogu

**Krok 1:** Przygotowanie parametrów
```csharp
var parameters = new DialogParameters 
{ 
    { "ParameterName", value } 
};
```

**Krok 2:** Otwarcie dialogu
```csharp
var dialog = await DialogService.ShowAsync<DialogComponent>(
    "Tytuł dialogu", 
    parameters,
    options);
```

**Krok 3:** Oczekiwanie na wynik
```csharp
var result = await dialog.Result;
```

**Krok 4:** Obsługa wyniku
```csharp
if (result.Canceled) 
{
    // Użytkownik anulował
    return;
}

// Użytkownik potwierdził - wykonanie operacji
```

### 5.3 Accessibility (dostępność)

**Keyboard navigation:**
- `Tab` - przełączanie między przyciskami
- `Enter` - potwierdzenie (focus na przycisku "Usuń")
- `Escape` - anulowanie

**Screen reader:**
- MudDialog automatycznie dodaje `role="dialog"` i `aria-modal="true"`
- Tytuł dialogu automatycznie przypisany jako `aria-labelledby`
- Treść dialogu automatycznie przypisana jako `aria-describedby`

**Focus management:**
- Po otwarciu dialogu: focus automatycznie na pierwszym elemencie interaktywnym
- Po zamknięciu dialogu: focus wraca do elementu, który wywołał dialog

### 5.4 Responsywność

**Mobile (xs):**
- MaxWidth.Small = pełna szerokość ekranu z marginesami
- Przyciski stackowane pionowo jeśli za mało miejsca

**Desktop (md+):**
- MaxWidth.Small = 600px
- Dialog wyśrodkowany na ekranie
- Przyciski obok siebie

### 5.5 Obsługa błędów

**Błąd podczas usuwania:**
- Dialog zostaje zamknięty (nie pozostaje otwarty)
- Snackbar z komunikatem błędu wyświetlany w komponencie rodzica
- Brak automatycznego ponowienia operacji

**NotFoundException:**
- Snackbar z komunikatem ostrzeżenia
- Odświeżenie listy (dla towarzyszy)
- Przekierowanie na `/trips` (dla wycieczki)

---

## 6. Kroki implementacji

### Krok 1: Utworzenie struktury folderów
```
MotoNomad.App/Shared/Dialogs/
├── DeleteTripConfirmationDialog.razor
└── DeleteCompanionConfirmationDialog.razor
```

### Krok 2: Implementacja DeleteTripConfirmationDialog.razor
- Utwórz plik zgodnie z sekcją 3.4
- Dodaj `@using MudBlazor` na górze pliku (jeśli nie jest w _Imports.razor)

### Krok 3: Implementacja DeleteCompanionConfirmationDialog.razor
- Utwórz plik zgodnie z sekcją 4.4
- Dodaj `@using MudBlazor` na górze pliku (jeśli nie jest w _Imports.razor)

### Krok 4: Rejestracja IDialogService w Program.cs

Upewnij się, że `IDialogService` jest zarejestrowany:
```csharp
builder.Services.AddMudServices();
```

MudBlazor automatycznie rejestruje `IDialogService` przy wywołaniu `AddMudServices()`.

### Krok 5: Integracja z TripDetails.razor

**Wstrzyknięcie IDialogService:**
```razor
@inject IDialogService DialogService
```

**Implementacja HandleDeleteTrip:**
```csharp
private async Task HandleDeleteTrip()
{
    var parameters = new DialogParameters { { "TripName", trip!.Name } };
    var options = new DialogOptions 
    { 
        CloseOnEscapeKey = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };
    
    var dialog = await DialogService.ShowAsync<DeleteTripConfirmationDialog>(
        "Potwierdzenie usunięcia", 
        parameters,
        options);
    
    var result = await dialog.Result;
    if (result.Canceled) return;

    // ... reszta implementacji
}
```

**Implementacja HandleRemoveCompanionAsync:**
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
    
    var options = new DialogOptions 
    { 
        CloseOnEscapeKey = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };
    
    var dialog = await DialogService.ShowAsync<DeleteCompanionConfirmationDialog>(
        "Potwierdzenie usunięcia", 
        parameters,
        options);
    
    var result = await dialog.Result;
    if (result.Canceled) return;

    // ... reszta implementacji
}
```

### Krok 6: Testy

**DeleteTripConfirmationDialog:**
- Przetestuj otwarcie dialogu (kliknięcie ikony kosza)
- Przetestuj anulowanie (kliknięcie "Anuluj")
- Przetestuj anulowanie (Escape)
- Przetestuj anulowanie (kliknięcie poza dialogiem)
- Przetestuj potwierdzenie (kliknięcie "Usuń")
- Sprawdź wyświetlanie nazwy wycieczki
- Sprawdź alert ostrzegawczy

**DeleteCompanionConfirmationDialog:**
- Przetestuj otwarcie dialogu (kliknięcie ikony kosza przy towarzyszu)
- Przetestuj anulowanie
- Przetestuj potwierdzenie
- Sprawdź wyświetlanie imienia i nazwiska

**Keyboard navigation:**
- Tab między przyciskami
- Enter na przycisku "Usuń"
- Escape zamyka dialog

**Responsywność:**
- Przetestuj na mobile (dialog pełna szerokość)
- Przetestuj na desktop (dialog 600px)

### Krok 7: Dostępność (Accessibility)

**Screen reader:**
- Przetestuj z NVDA lub JAWS
- Sprawdź czy tytuł i treść są poprawnie odczytywane
- Sprawdź czy focus management działa poprawnie

**Kontrast kolorów:**
- Przycisk "Usuń" (czerwony) musi mieć kontrast min 4.5:1
- Alert ostrzegawczy musi być czytelny

### Krok 8: Dokumentacja

- Dodaj komentarze XML dla parametrów publicznych
- Udokumentuj wzorzec wywołania dialogów w komentarzach
- Dodaj przykłady użycia w dokumentacji projektu

---

## 7. Potencjalne rozszerzenia (future enhancements)

### 7.1 Dialog z polem tekstowym potwierdzenia

Dla krytycznych operacji (np. usuwanie wycieczki) można dodać wymóg wpisania nazwy:

```razor
<MudTextField 
    @bind-Value="confirmationText"
    Label="Wpisz nazwę wycieczki aby potwierdzić"
    HelperText="@($"Wpisz: {TripName}")" />

<MudButton 
    Color="Color.Error"
    Disabled="@(confirmationText != TripName)"
    OnClick="Confirm">
    Usuń
</MudButton>
```

**Uwaga:** Zgodnie z session notes, w MVP używamy prostego dialogu bez wymagania wpisywania nazwy.

### 7.2 Dialog z licznikiem czasu

Dla szczególnie krytycznych operacji można dodać licznik (np. 3 sekundy) przed aktywacją przycisku "Usuń":

```csharp
private int countdown = 3;
private Timer? timer;

protected override void OnInitialized()
{
    timer = new Timer(1000);
    timer.Elapsed += (sender, args) =>
    {
        if (countdown > 0)
        {
            countdown--;
            InvokeAsync(StateHasChanged);
        }
        else
        {
            timer?.Stop();
        }
    };
    timer.Start();
}
```

### 7.3 Dialog z checkbox "Nie pokazuj więcej"

Dla doświadczonych użytkowników można dodać opcję pominięcia dialogu w przyszłości:

```razor
<MudCheckBox @bind-Value="dontShowAgain">
    Nie pokazuj tego komunikatu ponownie
</MudCheckBox>
```

---

## 8. Podsumowanie

Dialogi potwierdzenia w MotoNomad:
- ✅ Proste i przyjazne użytkownikowi
- ✅ Spójne z Material Design (MudBlazor)
- ✅ Dostępne (keyboard navigation, screen readers)
- ✅ Responsywne (mobile + desktop)
- ✅ Łatwe do rozszerzenia (wzorzec reużywalny)

**Kluczowe decyzje projektowe:**
- Prosty dialog bez wymagania wpisywania nazwy (zgodnie z session notes)
- Alert ostrzegawczy dla usuwania wycieczki (krytyczna operacja)
- Brak alertu dla usuwania towarzysza (mniej krytyczna operacja)
- Zamknięcie przez Escape i backdrop click (wygoda użytkownika)

**Bezpieczeństwo:**
- Wymóg potwierdzenia dla wszystkich operacji usuwania
- Jasny komunikat o nieodwracalności operacji
- Wyświetlenie nazwy/imienia usuwanego elementu (weryfikacja)

---

**Document Status:** ✅ Ready for Implementation  
**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025