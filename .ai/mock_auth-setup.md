# ?? Mock Authentication - Instrukcja testowania UI

**Status:** ? Gotowe do u?ycia  
**Cel:** Testowanie UI bez potrzeby prawdziwego logowania  
**?? UWAGA:** NIGDY nie w??czaj tego w production!

---

## ?? Krok po kroku - Jak w??czy? mock authentication

### **Krok 1: Zdob?d? User ID z Supabase**

1. Zaloguj si? do swojego projektu Supabase: https://supabase.com
2. Przejd? do **Authentication** ? **Users**
3. Znajd? swojego testowego u?ytkownika
4. Skopiuj **User ID** (UUID format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`)
5. Skopiuj te? **Email** u?ytkownika

---

### **Krok 2: Skonfiguruj appsettings.json**

Otwórz plik: `MotoNomad.App/wwwroot/appsettings.json`

```json
{
  "Supabase": {
    "Url": "https://YOUR_PROJECT_ID.supabase.co",
    "AnonKey": "YOUR_ANON_KEY_HERE"
  },
  "MockAuth": {
    "Enabled": true,        // ? Zmie? na true
    "UserId": "your-real-user-id-here", // ? Wklej User ID z Supabase
 "Email": "your-email@example.com",  // ? Wklej email u?ytkownika
    "DisplayName": "Test User"          // ? Opcjonalnie zmie? nazw?
  }
}
```

**Przyk?ad z prawdziwymi danymi:**
```json
{
  "Supabase": {
    "Url": "https://abcdefghijklmn.supabase.co",
    "AnonKey": "eyJhbGciOiJIUzI1..."
  },
  "MockAuth": {
    "Enabled": true,
    "UserId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "Email": "john.doe@example.com",
    "DisplayName": "John Doe"
  }
}
```

---

### **Krok 3: Uruchom aplikacj?**

**W Visual Studio:**
1. Naci?nij **F5** (z debuggerem) lub **Ctrl+F5** (bez debuggera)
2. W konsoli przegl?darki (F12) zobaczysz:
   ```
   ?? MOCK AUTHENTICATION ENABLED ??
   Mock User: john.doe@example.com (ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890)
   ?? This should NEVER be enabled in production!
   ```

**Lub w terminalu:**
```powershell
cd MotoNomad.App
dotnet watch
```

---

### **Krok 4: Testuj aplikacj? jako zalogowany u?ytkownik**

Teraz mo?esz swobodnie testowa? wszystkie widoki:

? **Dost?pne bez blokady:**
- `/trips` - Lista wycieczek (wcze?niej wymaga logowania)
- `/trip/create` - Tworzenie wycieczki (wcze?niej wymaga logowania)
- `/trip/{id}` - Szczegó?y wycieczki (gdy zaimplementujemy)

? **Co dzia?a:**
- `[Authorize]` attribute pozwala przej??
- `AuthenticationStateProvider` zwraca zalogowanego u?ytkownika
- Claims zawieraj? prawdziwy User ID z Supabase
- **RLS policies w Supabase b?d? dzia?a?** (bo u?ywamy prawdziwego User ID)

? **Co zobaczysz:**
- W prawym górnym rogu: avatar + email u?ytkownika
- NavMenu pokazuje linki dla zalogowanych
- TripList ?aduje wycieczki u?ytkownika z Supabase
- CreateTrip zapisuje wycieczki do Supabase pod User ID

---

## ?? Co si? dzieje pod mask??

### **MockAuthenticationStateProvider**

Plik: `Infrastructure/Auth/MockAuthenticationStateProvider.cs`

```csharp
// Zamiast ??czy? si? z Supabase Auth:
var user = client.Auth.CurrentUser;  // ? Nie u?ywamy

// Zwracamy mock u?ytkownika z Claims:
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, mockUserId),  // ? Twój User ID
    new Claim("email", mockEmail),
    new Claim("display_name", mockDisplayName),
};
```

### **Program.cs - Conditional Registration**

```csharp
// Je?li MockAuth.Enabled = true:
if (mockAuthSettings.Enabled)
{
    builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
        new MockAuthenticationStateProvider(...));  // ? Mock
}
else
{
    builder.Services.AddScoped<AuthenticationStateProvider, 
        CustomAuthenticationStateProvider>(); // ? Prawdziwy
}
```

---

## ? Weryfikacja, ?e dzia?a

### **Test 1: Sprawd? User ID w Developer Tools**

1. Naci?nij **F12** w przegl?darce
2. W konsoli wpisz:
 ```javascript
   // Sprawd? localStorage (je?li u?ywasz)
   console.log(localStorage);
   ```
3. Powiniene? zobaczy? warning o mock auth

### **Test 2: Sprawd? claims w aplikacji**

Mo?esz doda? tymczasowy kod w `TripList.razor.cs`:

```csharp
protected override async Task OnInitializedAsync()
{
    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
    var user = authState.User;
    
    Console.WriteLine($"User authenticated: {user.Identity?.IsAuthenticated}");
    Console.WriteLine($"User ID: {user.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
    Console.WriteLine($"Email: {user.FindFirst("email")?.Value}");
    
    // ... reszta kodu
}
```

---

## ?? Wa?ne uwagi

### **Dlaczego u?ywamy prawdziwego User ID?**

Supabase u?ywa **Row Level Security (RLS)** w PostgreSQL:

```sql
-- Przyk?ad RLS policy:
CREATE POLICY "Users can view their own trips"
ON trips FOR SELECT
USING (auth.uid() = user_id);
```

**Je?li u?yjemy fake User ID:**
- ? RLS policy blokuje dost?p (user nie istnieje w auth.users)
- ? Zapytania zwracaj? puste wyniki
- ? Nie mo?na testowa? prawdziwych danych

**Je?li u?yjemy prawdziwego User ID:**
- ? RLS policy przepuszcza zapytania
- ? Widzimy prawdziwe dane u?ytkownika
- ? Mo?emy tworzy? nowe wycieczki pod tym u?ytkownikiem

---

## ?? Jak wy??czy? mock auth?

### **Przed commitem / pushem / production:**

1. Otwórz `appsettings.json`
2. Zmie? `"Enabled": false`
   ```json
   "MockAuth": {
     "Enabled": false,  // ? Wy??cz
     ...
   }
 ```
3. Zrestartuj aplikacj?
4. Teraz b?dziesz musia? si? zalogowa? przez `/login`

---

## ?? Checklist przed commitem

- [ ] `MockAuth.Enabled: false` w appsettings.json
- [ ] Nie commituj prawdziwego User ID do repo (u?yj `.gitignore`)
- [ ] Sprawd?, czy nie ma debug `Console.WriteLine` w kodzie
- [ ] Sprawd?, czy aplikacja dzia?a z prawdziwym logowaniem

---

## ?? Troubleshooting

### **Problem: Dalej nie widz? danych w TripList**

**Przyczyna:** RLS policy blokuje dost?p

**Rozwi?zanie:**
1. Sprawd?, czy User ID w `appsettings.json` jest **dok?adnie taki sam** jak w Supabase
2. Sprawd?, czy ten u?ytkownik ma wycieczki w bazie:
   ```sql
   SELECT * FROM trips WHERE user_id = 'twój-user-id';
   ```
3. Sprawd? RLS policies w Supabase Dashboard ? Database ? Policies

---

### **Problem: Console warning si? nie pokazuje**

**Przyczyna:** Console.WriteLine mo?e nie dzia?a? w Blazor WASM

**Rozwi?zanie:** Sprawd? **Browser Console** (F12 ? Console), nie Output window w VS

---

### **Problem: Aplikacja si? nie uruchamia**

**Przyczyna:** B??d w konfiguracji

**Rozwi?zanie:**
1. Sprawd?, czy `appsettings.json` ma poprawny JSON (przecinki, cudzys?owy)
2. Sprawd? Build Output w Visual Studio
3. Uruchom `dotnet build` w terminalu i sprawd? b??dy

---

## ?? Pliki zaanga?owane w mock auth

```
MotoNomad.App/
??? wwwroot/
?   ??? appsettings.json         ? Konfiguracja (Enabled: true/false)
??? Infrastructure/
?   ??? Auth/
?   ?   ??? MockAuthenticationStateProvider.cs  ? Mock provider
? ?   ??? CustomAuthenticationStateProvider.cs ? Prawdziwy provider
?   ??? Configuration/
?       ??? MockAuthSettings.cs     ? Klasa konfiguracji
??? Program.cs    ? Conditional registration
```

---

## ?? Nast?pne kroki

Po skonfigurowaniu mock auth mo?esz:

1. ? **Zobaczy? dzia?aj?ce widoki:**
   - TripList (`/trips`)
   - CreateTrip (`/trip/create`)
   
2. ? **Przetestowa? UI/UX:**
   - Layout
   - Komponenty
   - Walidacj? formularzy
 - EmptyState
   - LoadingSpinner

3. ? **Zaimplementowa? TripDetails:**
   - Krok po kroku z hot reload
   - Testowa? od razu w przegl?darce

---

**Document Status:** ? Gotowy do u?ycia  
**Created:** 2025-01-XX  
**Project:** MotoNomad MVP  
**Autor:** AI Assistant
