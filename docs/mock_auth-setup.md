# Mock Authentication - UI Testing Instructions

**Status:** Ready to use  
**Purpose:** Testing UI without the need for real login  
**WARNING:** NEVER enable this in production!

---

## Step by Step - How to Enable Mock Authentication

### **Step 1: Get User ID from Supabase**

1. Log in to your Supabase project: https://supabase.com
2. Go to **Authentication** → **Users**
3. Find your test user
4. Copy the **User ID** (UUID format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`)
5. Also copy the user's **Email**

---

### **Step 2: Configure appsettings.json**

Open the file: `MotoNomad.App/wwwroot/appsettings.json`

```json
{
  "Supabase": {
    "Url": "https://YOUR_PROJECT_ID.supabase.co",
    "AnonKey": "YOUR_ANON_KEY_HERE"
  },
  "MockAuth": {
    "Enabled": true,        // Change to true
    "UserId": "your-real-user-id-here", // Paste User ID from Supabase
    "Email": "your-email@example.com",  // Paste user's email
    "DisplayName": "Test User"          // Optionally change the name
  }
}
```

**Example with real data:**
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

### **Step 3: Run the Application**

**In Visual Studio:**
1. Press **F5** (with debugger) or **Ctrl+F5** (without debugger)
2. In the browser console (F12) you will see:
   ```
   MOCK AUTHENTICATION ENABLED
   Mock User: john.doe@example.com (ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890)
   This should NEVER be enabled in production!
   ```

**Or in terminal:**
```powershell
cd MotoNomad.App
dotnet watch
```

---

### **Step 4: Test the Application as a Logged-in User**

Now you can freely test all views:

**Available without lock:**
- `/trips` - Trip list (previously required login)
- `/trip/create` - Trip creation (previously required login)
- `/trip/{id}` - Trip details (when we implement it)

**What works:**
- `[Authorize]` attribute allows passage
- `AuthenticationStateProvider` returns a logged-in user
- Claims contain real User ID from Supabase
- **RLS policies in Supabase will work** (because we use real User ID)

**What you will see:**
- In the top right corner: avatar + user's email
- NavMenu shows links for logged-in users
- TripList loads user's trips from Supabase
- CreateTrip saves trips to Supabase under User ID

---

## What Happens Under the Hood?

### **MockAuthenticationStateProvider**

File: `Infrastructure/Auth/MockAuthenticationStateProvider.cs`

```csharp
// Instead of connecting to Supabase Auth:
var user = client.Auth.CurrentUser;  // We don't use this

// We return a mock user with Claims:
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, mockUserId),  // Your User ID
    new Claim("email", mockEmail),
    new Claim("display_name", mockDisplayName),
};
```

### **Program.cs - Conditional Registration**

```csharp
// If MockAuth.Enabled = true:
if (mockAuthSettings.Enabled)
{
    builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
        new MockAuthenticationStateProvider(...));  // Mock
}
else
{
    builder.Services.AddScoped<AuthenticationStateProvider, 
        CustomAuthenticationStateProvider>(); // Real
}
```

---

## Verification That It Works

### **Test 1: Check User ID in Developer Tools**

1. Press **F12** in the browser
2. In the console, type:
 ```javascript
   // Check localStorage (if you use it)
   console.log(localStorage);
   ```
3. You should see a warning about mock auth

### **Test 2: Check Claims in the Application**

You can add temporary code in `TripList.razor.cs`:

```csharp
protected override async Task OnInitializedAsync()
{
    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
    var user = authState.User;
    
    Console.WriteLine($"User authenticated: {user.Identity?.IsAuthenticated}");
    Console.WriteLine($"User ID: {user.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
    Console.WriteLine($"Email: {user.FindFirst("email")?.Value}");
    
    // ... rest of the code
}
```

---

## Important Notes

### **Why Do We Use Real User ID?**

Supabase uses **Row Level Security (RLS)** in PostgreSQL:

```sql
-- Example RLS policy:
CREATE POLICY "Users can view their own trips"
ON trips FOR SELECT
USING (auth.uid() = user_id);
```

**If we use fake User ID:**
- RLS policy blocks access (user does not exist in auth.users)
- Queries return empty results
- Cannot test real data

**If we use real User ID:**
- RLS policy allows queries
- We see real user data
- We can create new trips under that user

---

## How to Disable Mock Auth?

### **Before commit / push / production:**

1. Open `appsettings.json`
2. Change `"Enabled": false`
   ```json
   "MockAuth": {
     "Enabled": false,  // Disable
     ...
   }
   ```
3. Restart the application
4. Now you will have to log in via `/login`

---

## Checklist Before Commit

- [ ] `MockAuth.Enabled: false` in appsettings.json
- [ ] Do not commit real User ID to repo (use `.gitignore`)
- [ ] Check if there are no debug `Console.WriteLine` in code
- [ ] Check if application works with real login

---

## Troubleshooting

### **Problem: Still Don't See Data in TripList**

**Cause:** RLS policy blocks access

**Solution:**
1. Check if User ID in `appsettings.json` is **exactly the same** as in Supabase
2. Check if that user has trips in the database:
   ```sql
   SELECT * FROM trips WHERE user_id = 'your-user-id';
   ```
3. Check RLS policies in Supabase Dashboard → Database → Policies

---

### **Problem: Console Warning Doesn't Show**

**Cause:** Console.WriteLine may not work in Blazor WASM

**Solution:** Check **Browser Console** (F12 → Console), not Output window in VS

---

### **Problem: Application Doesn't Start**

**Cause:** Error in configuration

**Solution:**
1. Check if `appsettings.json` has correct JSON (commas, quotes)
2. Check Build Output in Visual Studio
3. Run `dotnet build` in terminal and check errors

---

## Files Involved in Mock Auth

```
MotoNomad.App/
├── wwwroot/
│   └── appsettings.json         # Configuration (Enabled: true/false)
├── Infrastructure/
│   ├── Auth/
│   │   ├── MockAuthenticationStateProvider.cs  # Mock provider
│   │   └── CustomAuthenticationStateProvider.cs # Real provider
│   └── Configuration/
│       └── MockAuthSettings.cs     # Configuration class
└── Program.cs    # Conditional registration
```

---

## Next Steps

After configuring mock auth, you can:

1. **See Working Views:**
   - TripList (`/trips`)
   - CreateTrip (`/trip/create`)
   
2. **Test UI/UX:**
   - Layout
   - Components
   - Form validations
   - EmptyState
   - LoadingSpinner

3. **Implement TripDetails:**
   - Step by step with hot reload
   - Test immediately in browser

