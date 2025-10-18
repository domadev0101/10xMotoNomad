# Health Check - Quick Start Guide

## ?? How to Run Health Check

### Step 1: Configure Supabase Settings

#### For local environment (Supabase Local):
Edit `MotoNomad.App/wwwroot/appsettings.Development.json`:

```json
{
  "Supabase": {
    "Url": "http://127.0.0.1:54321",
    "AnonKey": "your_local_anon_key"
  }
}
```

**Note:** Default anon key for local Supabase is already in the file.

#### For production environment (Supabase Cloud):
Edit `MotoNomad.App/wwwroot/appsettings.json`:

```json
{
  "Supabase": {
    "Url": "https://your-project-id.supabase.co",
    "AnonKey": "your_production_anon_key"
  }
}
```

### Step 2: Run the Application

```bash
cd MotoNomad.App
dotnet run
```

### Step 3: Open Health Check

1. Navigate to the application in browser (default: `http://localhost:5000`)
2. Click **"Health Check"** in the navigation menu (heart icon)
3. Click the **"?? Run Health Check"** button

## ?? What Does Health Check Test?

### Test 1: Client Initialization ?
- Verifies that Supabase client was properly initialized at application startup
- **Expected result:** ? SUCCESS

### Test 2: Database Connectivity ?
- Tests if Supabase client instance can be obtained
- **Expected result:** ? SUCCESS

### Test 3: Table Access (trips) ?
- Attempts to query the `trips` table
- Checks if table exists and is accessible
- **Possible errors:**
  - Table doesn't exist ? Run migrations
  - RLS policy blocks access ? Configure RLS policies

### Test 4: Auth Status ?
- Checks authentication status
- For unauthenticated user: "No active user session (anonymous access)" is **normal**
- **Expected result:** ? SUCCESS

## ?? Troubleshooting

### ? Connection Failed
**Problem:** Cannot connect to Supabase

**Solution:**
1. Verify URL and AnonKey are correct in `appsettings.json`
2. For local Supabase - ensure it's running (`supabase start`)
3. Check application logs in console

### ? Table Access Failed
**Problem:** Cannot access `trips` table

**Solution:**
1. Run migrations: `supabase db reset` (locally) or execute migrations in Supabase Studio
2. Verify that `trips` table exists
3. Configure RLS policies to allow anonymous access (for testing):
   ```sql
   -- In Supabase SQL Editor
   ALTER TABLE trips ENABLE ROW LEVEL SECURITY;
   
   CREATE POLICY "Allow anonymous read access" ON trips
   FOR SELECT
   TO anon
   USING (true);
   ```

### ? Auth Status Failed
**Problem:** Error checking authentication status

**Solution:**
1. This test rarely fails
2. If error occurs, check browser console logs (F12)
3. Restart the application

## ?? How to Read Results?

### ? Green (Success)
- Test passed successfully
- Connection works correctly

### ? Red (Failed)
- Test failed
- Check error details in test card
- See "Troubleshooting" section above

### ?? Diagnostic Information
- **Status:** Connected/Not Initialized
- **Environment:** Development/Production
- **Last Check:** Time of last test
- **Duration:** Test execution time in ms

## ?? Next Steps

After all tests pass successfully:
1. ? Database connection works
2. ? You can start implementing application services
3. ? Health Check will always be available as a diagnostic tool

## ?? Tips

- You can run Health Check at any time
- Useful for debugging connection issues
- Use during deployment to verify configuration
- Link to Health Check: `/health`

---

**Author:** AI Assistant  
**Created:** 2025-01-18  
**Version:** 1.0
