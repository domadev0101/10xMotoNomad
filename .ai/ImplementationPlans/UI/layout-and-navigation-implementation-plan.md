# Plan implementacji Layout i Nawigacji

## 1. Przegląd

Dokument opisuje implementację głównego layoutu aplikacji (MainLayout.razor), menu nawigacyjnego (NavMenu.razor), komponentu statusu logowania (LoginDisplay.razor) oraz konfiguracji routingu i autoryzacji (App.razor). Te komponenty tworzą strukturę nawigacyjną i wizualną całej aplikacji MotoNomad.

## 2. Lista komponentów

1. **App.razor** - Główny komponent routingu z autoryzacją
2. **MainLayout.razor** - Główny layout aplikacji (MudLayout)
3. **NavMenu.razor** - Menu nawigacyjne (MudDrawer)
4. **LoginDisplay.razor** - Status zalogowania w AppBar

---

## 3. App.razor

### 3.1 Przegląd

Główny komponent aplikacji odpowiedzialny za routing, autoryzację i kaskadowy stan uwierzytelnienia. Definiuje:
- AuthorizeRouteView dla chronionych tras
- Przekierowanie niezalogowanych użytkowników na `/login`
- Obsługę 404 (NotFound)
- CascadingAuthenticationState dla całej aplikacji

### 3.2 Struktura komponentu

```
App.razor
└── CascadingAuthenticationState
    └── Router (AppAssembly)
        ├── Found
        │   └── AuthorizeRouteView (DefaultLayout=MainLayout)
        │       ├── Authorized → Rendering route
        │       └── NotAuthorized → RedirectToLogin lub "Brak uprawnień"
        └── NotFound
            └── LayoutView (Layout=MainLayout)
                └── "Strona nie znaleziona"
```

### 3.3 Implementacja

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated != true)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
                        <MudContainer MaxWidth="MaxWidth.Medium" Class="mt-8">
                            <MudAlert Severity="Severity.Warning">
                                Nie masz uprawnień do tej strony.
                            </MudAlert>
                            <MudButton 
                                Variant="Variant.Filled" 
                                Color="Color.Primary"
                                Href="/trips"
                                Class="mt-4">
                                Wróć do listy wycieczek
                            </MudButton>
                        </MudContainer>
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <MudContainer MaxWidth="MaxWidth.Medium" Class="mt-8 text-center">
                    <MudIcon Icon="@Icons.Material.Filled.Search" Size="Size.Large" Color="Color.Secondary" Class="mb-4" />
                    <MudText Typo="Typo.h4" Class="mb-2">404 - Strona nie znaleziona</MudText>
                    <MudText Typo="Typo.body1" Color="Color.Secondary" Class="mb-4">
                        Strona, której szukasz, nie istnieje.
                    </MudText>
                    <MudButton 
                        Variant="Variant.Filled" 
                        Color="Color.Primary"
                        Href="/">
                        Wróć na stronę główną
                    </MudButton>
                </MudContainer>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>

@code {
    // Brak kodu - App.razor jest tylko deklaratywny
}
```

### 3.4 RedirectToLogin (helper komponent)

Utwórz pomocniczy komponent `RedirectToLogin.razor`:

```razor
@inject NavigationManager NavigationManager

@code {
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo("/login");
    }
}
```

### 3.5 Kluczowe funkcjonalności

**CascadingAuthenticationState:**
- Propaguje stan uwierzytelnienia do wszystkich komponentów potomnych
- Umożliwia używanie `[CascadingParameter] Task<AuthenticationState>` w komponentach
- Automatycznie aktualizuje UI po zmianie stanu logowania

**AuthorizeRouteView:**
- Automatycznie sprawdza atrybut `[Authorize]` na stronach
- Renderuje komponent jeśli użytkownik ma dostęp
- Renderuje `NotAuthorized` jeśli brak dostępu

**Routing:**
- Automatyczne dopasowanie URL do komponentów z `@page`
- Obsługa parametrów route (np. `/trip/{id:guid}`)
- Fallback do NotFound jeśli brak dopasowania

---

## 4. MainLayout.razor

### 4.1 Przegląd

Główny layout aplikacji używający MudLayout z MudBlazor. Zawiera:
- MudAppBar (górny pasek z logo i LoginDisplay)
- MudDrawer (menu boczne - NavMenu)
- Main content area (@Body)
- Timer bezczynności (auto-logout po 15 minutach)

### 4.2 Struktura komponentu

```
MainLayout.razor
└── MudLayout
    ├── MudAppBar (Fixed, Dense)
    │   ├── MudIconButton (DrawerToggle) [mobile only]
    │   ├── MudText ("MotoNomad" + logo)
    │   ├── MudSpacer
    │   └── LoginDisplay.razor
    ├── MudDrawer (@bind-Open, Breakpoint.Md, Variant)
    │   └── NavMenu.razor
    └── MudMainContent
        └── MudContainer (MaxWidth.ExtraLarge)
            └── @Body
```

### 4.3 Implementacja

```razor
@inherits LayoutComponentBase
@inject IAuthService AuthService
@inject NavigationManager NavigationManager
@inject ISnackbar Snackbar

<MudLayout>
    <MudAppBar Elevation="1" Dense="true">
        <MudIconButton 
            Icon="@Icons.Material.Filled.Menu" 
            Color="Color.Inherit" 
            Edge="Edge.Start" 
            OnClick="@ToggleDrawer" 
            Class="drawer-toggle" />
        
        <MudText Typo="Typo.h6" Class="ml-3">
            🏍️ MotoNomad
        </MudText>
        
        <MudSpacer />
        
        <LoginDisplay />
    </MudAppBar>

    <MudDrawer 
        @bind-Open="_drawerOpen" 
        Elevation="1" 
        Breakpoint="Breakpoint.Md"
        Variant="@(_drawerOpen ? DrawerVariant.Persistent : DrawerVariant.Temporary)">
        <NavMenu />
    </MudDrawer>

    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4 mb-4">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    private bool _drawerOpen = true;
    private System.Timers.Timer? _inactivityTimer;
    private const int InactivityTimeoutMinutes = 15;

    protected override void OnInitialized()
    {
        InitializeInactivityTimer();
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
        ResetInactivityTimer();
    }

    private void InitializeInactivityTimer()
    {
        _inactivityTimer = new System.Timers.Timer(InactivityTimeoutMinutes * 60 * 1000);
        _inactivityTimer.Elapsed += async (sender, args) =>
        {
            await HandleInactivityTimeout();
        };
        _inactivityTimer.Start();
    }

    private void ResetInactivityTimer()
    {
        _inactivityTimer?.Stop();
        _inactivityTimer?.Start();
    }

    private async Task HandleInactivityTimeout()
    {
        if (await AuthService.IsAuthenticatedAsync())
        {
            await AuthService.LogoutAsync();
            await InvokeAsync(() =>
            {
                Snackbar.Add("Sesja wygasła z powodu bezczynności. Zaloguj się ponownie.", Severity.Warning);
                NavigationManager.NavigateTo("/login");
            });
        }
    }

    public void Dispose()
    {
        _inactivityTimer?.Stop();
        _inactivityTimer?.Dispose();
    }
}
```

### 4.4 Stylizacja (wwwroot/css/app.css)

```css
/* Ukrycie przycisku drawer-toggle na desktop */
@media (min-width: 960px) {
    .drawer-toggle {
        display: none;
    }
}

/* Responsywne odstępy */
.mud-main-content {
    padding-top: 64px; /* Wysokość AppBar */
}

@media (max-width: 600px) {
    .mud-main-content {
        padding-top: 56px; /* Mniejsza wysokość AppBar na mobile */
    }
}
```

### 4.5 Kluczowe funkcjonalności

**Responsywność drawera:**
- **Desktop (md+):** Drawer przypiętych (`Variant.Persistent`), zawsze widoczny
- **Mobile (< md):** Drawer wysuwany (`Variant.Temporary`), ukryty domyślnie
- Breakpoint: 960px (Breakpoint.Md)

**Timer bezczynności:**
- Automatyczny logout po 15 minutach bezczynności
- Reset timera przy każdej interakcji (np. toggle drawera)
- Snackbar z komunikatem po wylogowaniu
- Przekierowanie na `/login`

**Interakcje resetujące timer:**
- Kliknięcie przycisku menu (ToggleDrawer)
- Nawigacja między stronami (można dodać w OnParametersSet)
- Scroll strony (można dodać event listener)

---

## 5. NavMenu.razor

### 5.1 Przegląd

Menu nawigacyjne aplikacji wyświetlane w MudDrawer. Zawiera:
- Logo/nagłówek aplikacji
- Linki nawigacyjne dla zalogowanych użytkowników
- Linki nawigacyjne dla niezalogowanych użytkowników
- Dynamiczną zmianę zawartości w zależności od stanu uwierzytelnienia

### 5.2 Struktura komponentu

```
NavMenu.razor
└── <div> (pa-4)
    ├── MudText (Typo.h6) - Logo/Nagłówek
    ├── MudDivider
    ├── MudNavMenu
    │   ├── AuthorizeView
    │   │   ├── Authorized
    │   │   │   ├── MudNavLink ("Moje wycieczki")
    │   │   │   ├── MudNavLink ("Nowa wycieczka")
    │   │   │   └── MudNavLink ("Wyloguj", OnClick)
    │   │   └── NotAuthorized
    │   │       ├── MudNavLink ("Zaloguj")
    │   │       └── MudNavLink ("Zarejestruj")
```

### 5.3 Implementacja

```razor
@inject IAuthService AuthService
@inject NavigationManager NavigationManager
@inject ISnackbar Snackbar

<div class="pa-4">
    <MudText Typo="Typo.h6" Class="mb-2">
        🏍️ MotoNomad
    </MudText>
    <MudDivider Class="mb-4" />
    
    <MudNavMenu>
        <AuthorizeView>
            <Authorized>
                <MudNavLink 
                    Href="/trips" 
                    Icon="@Icons.Material.Filled.Map"
                    Match="NavLinkMatch.All">
                    Moje wycieczki
                </MudNavLink>
                
                <MudNavLink 
                    Href="/trip/create" 
                    Icon="@Icons.Material.Filled.Add">
                    Nowa wycieczka
                </MudNavLink>
                
                <MudDivider Class="my-2" />
                
                <MudNavLink 
                    OnClick="@HandleLogout"
                    Icon="@Icons.Material.Filled.Logout">
                    Wyloguj
                </MudNavLink>
            </Authorized>
            
            <NotAuthorized>
                <MudNavLink 
                    Href="/login" 
                    Icon="@Icons.Material.Filled.Login">
                    Zaloguj
                </MudNavLink>
                
                <MudNavLink 
                    Href="/register" 
                    Icon="@Icons.Material.Filled.PersonAdd">
                    Zarejestruj
                </MudNavLink>
            </NotAuthorized>
        </AuthorizeView>
    </MudNavMenu>
</div>

@code {
    private async Task HandleLogout()
    {
        try
        {
            await AuthService.LogoutAsync();
            Snackbar.Add("Wylogowano pomyślnie!", Severity.Success);
            NavigationManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            Snackbar.Add("Wystąpił błąd podczas wylogowania.", Severity.Error);
            // TODO: Logowanie błędu
        }
    }
}
```

### 5.4 Kluczowe funkcjonalności

**AuthorizeView:**
- Automatycznie reaguje na zmiany stanu uwierzytelnienia
- Wyświetla różne linki dla zalogowanych/niezalogowanych użytkowników
- Nie wymaga ręcznego sprawdzania `IsAuthenticated`

**NavLinkMatch:**
- `NavLinkMatch.All` dla `/trips` - dokładne dopasowanie (aktywny tylko na `/trips`)
- Domyślnie `NavLinkMatch.Prefix` - aktywny dla wszystkich podścieżek

**Ikony:**
- Icons.Material.Filled.Map - mapa (Moje wycieczki)
- Icons.Material.Filled.Add - plus (Nowa wycieczka)
- Icons.Material.Filled.Logout - wyjście (Wyloguj)
- Icons.Material.Filled.Login - wejście (Zaloguj)
- Icons.Material.Filled.PersonAdd - dodaj osobę (Zarejestruj)

**HandleLogout:**
- Wywołuje `IAuthService.LogoutAsync()`
- Wyświetla Snackbar z komunikatem sukcesu
- Przekierowuje na `/login`
- Obsługuje błędy wylogowania

---

## 6. LoginDisplay.razor

### 6.1 Przegląd

Komponent wyświetlający status zalogowania w MudAppBar. Pokazuje:
- Powitanie użytkownika (dla zalogowanych): "Cześć, [DisplayName lub Email]!"
- Przycisk "Wyloguj" (dla zalogowanych)
- Przyciski "Zaloguj" i "Zarejestruj" (dla niezalogowanych)

### 6.2 Struktura komponentu

```
LoginDisplay.razor
└── AuthorizeView
    ├── Authorized
    │   ├── MudText ("Cześć, [DisplayName]!")
    │   └── MudIconButton (Logout)
    └── NotAuthorized
        ├── MudButton ("Zaloguj") [desktop]
        ├── MudIconButton (Login) [mobile]
        ├── MudButton ("Zarejestruj") [desktop]
        └── MudIconButton (PersonAdd) [mobile]
```

### 6.3 Implementacja

```razor
@inject IAuthService AuthService
@inject NavigationManager NavigationManager
@inject ISnackbar Snackbar

<AuthorizeView>
    <Authorized>
        <div style="display: flex; align-items: center; gap: 10px;">
            <MudText Typo="Typo.body2" Class="login-display-text">
                Cześć, @GetDisplayName(context)!
            </MudText>
            
            <MudIconButton 
                Icon="@Icons.Material.Filled.Logout"
                Color="Color.Inherit"
                OnClick="@HandleLogout"
                Title="Wyloguj" />
        </div>
    </Authorized>
    
    <NotAuthorized>
        <div style="display: flex; gap: 10px;">
            @* Desktop - pełne przyciski *@
            <MudButton 
                Variant="Variant.Text" 
                Color="Color.Inherit"
                Href="/login"
                Class="login-display-button">
                Zaloguj
            </MudButton>
            
            <MudButton 
                Variant="Variant.Filled" 
                Color="Color.Primary"
                Href="/register"
                Class="login-display-button">
                Zarejestruj
            </MudButton>
            
            @* Mobile - ikony *@
            <MudIconButton 
                Icon="@Icons.Material.Filled.Login"
                Color="Color.Inherit"
                Href="/login"
                Class="login-display-icon"
                Title="Zaloguj" />
            
            <MudIconButton 
                Icon="@Icons.Material.Filled.PersonAdd"
                Color="Color.Primary"
                Href="/register"
                Class="login-display-icon"
                Title="Zarejestruj" />
        </div>
    </NotAuthorized>
</AuthorizeView>

@code {
    private string GetDisplayName(AuthenticationState authState)
    {
        var user = authState.User;
        var displayName = user.FindFirst("display_name")?.Value;
        var email = user.FindFirst("email")?.Value;
        
        return displayName ?? email?.Split('@')[0] ?? "Użytkownik";
    }

    private async Task HandleLogout()
    {
        try
        {
            await AuthService.LogoutAsync();
            Snackbar.Add("Wylogowano pomyślnie!", Severity.Success);
            NavigationManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            Snackbar.Add("Wystąpił błąd podczas wylogowania.", Severity.Error);
            // TODO: Logowanie błędu
        }
    }
}
```

### 6.4 Stylizacja responsywna (wwwroot/css/app.css)

```css
/* Desktop - pokazuj pełne przyciski */
@media (min-width: 600px) {
    .login-display-button {
        display: inline-flex;
    }
    .login-display-icon {
        display: none;
    }
}

/* Mobile - pokazuj ikony */
@media (max-width: 599px) {
    .login-display-button {
        display: none;
    }
    .login-display-icon {
        display: inline-flex;
    }
    .login-display-text {
        display: none; /* Opcjonalnie: ukryj "Cześć, [name]" na mobile */
    }
}
```

### 6.5 Kluczowe funkcjonalności

**GetDisplayName:**
- Pobiera `display_name` z claims użytkownika (jeśli jest)
- Fallback na część przed @ z emaila
- Fallback na "Użytkownik" jeśli brak obu

**Responsywność:**
- **Desktop (≥600px):** Pełne przyciski z tekstem
- **Mobile (<600px):** Ikony bez tekstu (oszczędność miejsca)
- Opcjonalnie: ukrycie powitania na mobile

**HandleLogout:**
- Identyczna implementacja jak w NavMenu
- Wylogowanie + Snackbar + przekierowanie

---

## 7. Integracja z Supabase Auth

### 7.1 CustomAuthenticationStateProvider

Utwórz custom provider dla Supabase Auth:

```csharp
// MotoNomad.App/Infrastructure/Auth/CustomAuthenticationStateProvider.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISupabaseClientService _supabaseClient;

    public CustomAuthenticationStateProvider(ISupabaseClientService supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var client = _supabaseClient.GetClient();
        var user = client.Auth.CurrentUser;

        if (user == null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("email", user.Email ?? string.Empty),
        };

        // Dodanie display_name jeśli jest w metadata
        if (user.UserMetadata?.ContainsKey("display_name") == true)
        {
            claims.Add(new Claim("display_name", user.UserMetadata["display_name"]?.ToString() ?? string.Empty));
        }

        var identity = new ClaimsIdentity(claims, "supabase");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
```

### 7.2 Rejestracja w Program.cs

```csharp
// Program.cs
using Microsoft.AspNetCore.Components.Authorization;

// ... inne usługi

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
```

### 7.3 Aktualizacja stanu po logowaniu/wylogowaniu

W `AuthService`:

```csharp
public class AuthService : IAuthService
{
    private readonly ISupabaseClientService _supabaseClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(
        ISupabaseClientService supabaseClient,
        AuthenticationStateProvider authStateProvider)
    {
        _supabaseClient = supabaseClient;
        _authStateProvider = authStateProvider;
    }

    public async Task<UserDto> LoginAsync(LoginCommand command)
    {
        var client = _supabaseClient.GetClient();
        var session = await client.Auth.SignIn(command.Email, command.Password);
        
        // Notify o zmianie stanu
        if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
        {
            customProvider.NotifyAuthenticationStateChanged();
        }

        return MapToUserDto(session.User);
    }

    public async Task LogoutAsync()
    {
        var client = _supabaseClient.GetClient();
        await client.Auth.SignOut();
        
        // Notify o zmianie stanu
        if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
        {
            customProvider.NotifyAuthenticationStateChanged();
        }
    }
}
```

---

## 8. Kroki implementacji

### Krok 1: Implementacja App.razor
- Utwórz/zaktualizuj `MotoNomad.App/App.razor`
- Dodaj `CascadingAuthenticationState`
- Skonfiguruj `AuthorizeRouteView` z `MainLayout`
- Dodaj obsługę NotFound (404)

### Krok 2: Utworzenie RedirectToLogin.razor
- Utwórz `MotoNomad.App/Shared/RedirectToLogin.razor`
- Implementuj przekierowanie na `/login`

### Krok 3: Implementacja MainLayout.razor
- Utwórz/zaktualizuj `MotoNomad.App/Layout/MainLayout.razor`
- Dodaj `MudLayout`, `MudAppBar`, `MudDrawer`, `MudMainContent`
- Implementuj timer bezczynności (15 minut)
- Dodaj event Dispose dla timera

### Krok 4: Implementacja NavMenu.razor
- Utwórz/zaktualizuj `MotoNomad.App/Layout/NavMenu.razor`
- Dodaj `AuthorizeView` z różnymi linkami dla zalogowanych/niezalogowanych
- Implementuj `HandleLogout`

### Krok 5: Implementacja LoginDisplay.razor
- Utwórz `MotoNomad.App/Shared/LoginDisplay.razor`
- Dodaj `AuthorizeView` z responsywnymi przyciskami
- Implementuj `GetDisplayName` i `HandleLogout`

### Krok 6: Stylizacja responsywna
- Dodaj style do `wwwroot/css/app.css`:
  - Ukrycie drawer-toggle na desktop
  - Responsywne przyciski LoginDisplay
  - Padding dla MudMainContent

### Krok 7: Implementacja CustomAuthenticationStateProvider
- Utwórz `MotoNomad.App/Infrastructure/Auth/CustomAuthenticationStateProvider.cs`
- Implementuj `GetAuthenticationStateAsync()`
- Dodaj `NotifyAuthenticationStateChanged()`

### Krok 8: Rejestracja w Program.cs
- Zarejestruj `CustomAuthenticationStateProvider` jako `AuthenticationStateProvider`
- Dodaj `AddAuthorizationCore()`

### Krok 9: Aktualizacja AuthService
- Dodaj wywołanie `NotifyAuthenticationStateChanged()` w `LoginAsync` i `LogoutAsync`
- Wstrzyknij `AuthenticationStateProvider` w konstruktorze

### Krok 10: Testy

**Routing:**
- Przetestuj nawigację między stronami
- Przetestuj 404 (nieistniejąca strona)
- Przetestuj przekierowanie niezalogowanego na `/login`

**Layout:**
- Przetestuj responsywność (mobile + desktop)
- Przetestuj drawer (toggle, persistent/temporary)
- Przetestuj timer bezczynności (wylogowanie po 15 min)

**Nawigacja:**
- Przetestuj linki dla zalogowanych użytkowników
- Przetestuj linki dla niezalogowanych użytkowników
- Przetestuj wylogowanie z NavMenu
- Przetestuj aktywny link (podświetlenie)

**LoginDisplay:**
- Przetestuj wyświetlanie nazwy użytkownika
- Przetestuj responsywność przycisków (desktop/mobile)
- Przetestuj wylogowanie z AppBar

**Authentication State:**
- Przetestuj automatyczne odświeżenie UI po logowaniu
- Przetestuj automatyczne odświeżenie UI po wylogowaniu
- Przetestuj `AuthorizeView` w różnych komponentach

### Krok 11: Dokumentacja
- Dodaj komentarze XML dla kluczowych metod
- Udokumentuj timer bezczynności
- Udokumentuj responsywność layoutu
- Zaktualizuj README

---

## 9. Podsumowanie

Layout i nawigacja w MotoNomad:
- ✅ Material Design (MudBlazor)
- ✅ Responsywny (mobile-first)
- ✅ Dostępny (keyboard navigation, ARIA)
- ✅ Bezpieczny (autoryzacja, auto-logout)
- ✅ Intuicyjny (jasna struktura menu)

**Kluczowe funkcjonalności:**
- Automatyczne przekierowanie niezalogowanych na `/login`
- Dynamiczne menu w zależności od stanu logowania
- Timer bezczynności z auto-wylogowaniem
- Responsywny drawer (persistent/temporary)
- Spójny design w całej aplikacji

**Bezpieczeństwo:**
- AuthorizeRouteView blokuje dostęp do chronionych stron
- CustomAuthenticationStateProvider zarządza stanem
- Timer bezczynności zwiększa bezpieczeństwo
- Logout dostępny z dwóch miejsc (NavMenu + AppBar)

---

**Document Status:** ✅ Ready for Implementation  
**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025