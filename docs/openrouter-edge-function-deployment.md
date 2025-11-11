# OpenRouter Edge Function Deployment Guide

## Overview

Supabase Edge Functions acts as a secure proxy between the Blazor WebAssembly client and OpenRouter API:
- ✅ OpenRouter API key is hidden on the server side (Edge Function)
- ✅ Client only sends Supabase authentication (JWT token)
- ✅ Edge Function verifies the user and forwards the request to OpenRouter
- ✅ Increased security (no API key exposure in the browser)

## Architecture

```
Blazor WASM Client
    ↓ (POST + Supabase JWT Token)
Edge Function (openrouter-proxy)
    ↓ (POST + OpenRouter API Key)
OpenRouter API
    ↓ (JSON Response)
Edge Function
    ↓ (JSON Response)
Blazor Client
```

## Project Files

### Edge Function

**Location**: `MotoNomad.App/Infrastructure/Database/supabase/functions/`

Structure:
```
functions/
├── _shared/
│   └── cors.ts      # Shared CORS configuration
└── openrouter-proxy/
    └── index.ts     # Main Edge Function file
```

### C# Configuration

**Files**:
- `MotoNomad.App/Infrastructure/Configuration/OpenRouterSettings.cs` - Extended configuration
- `MotoNomad.App/Infrastructure/Services/OpenRouterService.cs` - Proxy support
- `MotoNomad.App/wwwroot/appsettings.json` - Application configuration

## Deployment Steps

### Step 1: Deploy Edge Function to Supabase

**Requirements**:
- Installed [Supabase CLI](https://supabase.com/docs/guides/cli)
- Logged in to Supabase: `supabase login`
- Linked to project: `supabase link --project-ref YOUR_PROJECT_REF`

**Deploy**:
```bash
cd MotoNomad.App/Infrastructure/Database/supabase

# Deploy single function
supabase functions deploy openrouter-proxy

# Deploy all functions
supabase functions deploy
```

**Verification**:
```bash
# Check function status
supabase functions list

# Function URL will be: https://YOUR_PROJECT.supabase.co/functions/v1/openrouter-proxy
```

### Step 2: Configure Supabase Secrets

Edge Function requires OpenRouter API key as a secret:

```bash
# Set OpenRouter API key as secret
supabase secrets set OPENROUTER_API_KEY=sk-or-v1-YOUR-ACTUAL-API-KEY

# Verify secret was set
supabase secrets list
```

⚠️ **IMPORTANT**: Never commit real API keys to Git!

### Step 3: Update appsettings.json

**Development** (`appsettings.Development.json`):
```json
{
  "OpenRouter": {
    "ApiKey": "sk-or-v1-YOUR-DEV-KEY",
 "UseEdgeFunctionProxy": false,
    "EdgeFunctionUrl": "http://127.0.0.1:54321/functions/v1/openrouter-proxy"
  }
}
```

**Production** (`appsettings.json`):
```json
{
  "OpenRouter": {
    "ApiKey": "sk-or-v1-PLACEHOLDER",
    "UseEdgeFunctionProxy": true,
    "EdgeFunctionUrl": "https://yscgrwfkuiicqlemqzem.supabase.co/functions/v1/openrouter-proxy"
  }
}
```

### Step 4: Local Testing (Optional)

**Run Edge Functions locally**:
```bash
# In supabase/ directory
supabase functions serve openrouter-proxy --env-file .env.local

# Function will be available at: http://127.0.0.1:54321/functions/v1/openrouter-proxy
```

**Create `.env.local` file**:
```
OPENROUTER_API_KEY=sk-or-v1-YOUR-DEV-KEY
```

**Test manually with curl**:
```bash
curl -X POST http://127.0.0.1:54321/functions/v1/openrouter-proxy \
  -H "Authorization: Bearer YOUR_SUPABASE_ANON_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "google/gemma-3-27b-it:free",
    "messages": [
      {"role": "user", "content": "Hello!"}
    ],
    "max_tokens": 10
  }'
```

### Step 5: Production Deployment

**GitHub Actions**:

Add secret in GitHub repository:
```
Settings → Secrets → Actions → New repository secret
Name: SUPABASE_ACCESS_TOKEN
Value: YOUR_SUPABASE_ACCESS_TOKEN
```

Get token:
```bash
supabase login
supabase access-token
```

**Workflow** (`.github/workflows/deploy-edge-functions.yml`):
```yaml
name: Deploy Edge Functions

on:
  push:
    branches: [main]
    paths:
      - 'MotoNomad.App/Infrastructure/Database/supabase/functions/**'

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
   
      - name: Setup Supabase CLI
        uses: supabase/setup-cli@v1
        with:
          version: latest
 
      - name: Link to Supabase project
        run: |
     supabase link --project-ref ${{ secrets.SUPABASE_PROJECT_REF }}
        env:
          SUPABASE_ACCESS_TOKEN: ${{ secrets.SUPABASE_ACCESS_TOKEN }}
    
      - name: Deploy Edge Functions
        run: |
        cd MotoNomad.App/Infrastructure/Database/supabase
   supabase functions deploy
        env:
      SUPABASE_ACCESS_TOKEN: ${{ secrets.SUPABASE_ACCESS_TOKEN }}
```

## Security Configuration

### CORS

**Production** - change in `_shared/cors.ts`:
```typescript
export const corsHeaders = {
  "Access-Control-Allow-Origin": "https://domadev0101.github.io", // Only your domain
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
  "Access-Control-Allow-Methods": "POST, OPTIONS",
};
```

### Rate Limiting

Edge Function doesn't have built-in rate limiting - it's handled by:
1. **OpenRouter** - account limits
2. **Supabase Auth** - JWT rate limiting
3. **Client-side** - OpenRouterService has MinRequestDelayMs

Optionally add Supabase Edge Middleware for rate limiting.

## Monitoring and Diagnostics

### Edge Function Logs

**In Supabase dashboard**:
```
Edge Functions → openrouter-proxy → Logs
```

**Via CLI**:
```bash
supabase functions logs openrouter-proxy
```

### Metrics

Supabase dashboard shows:
- Number of function invocations
- Execution time
- Error rate
- Resource usage

### Debugging

**Add logs in Edge Function**:
```typescript
console.log("Request received:", {
  model: requestBody.model,
  messagesCount: requestBody.messages.length
});

console.error("OpenRouter API error:", {
  status: openRouterResponse.status,
  body: responseText
});
```

## Migration from Direct API to Edge Function Proxy

### Step 1: Deploy Edge Function
```bash
supabase functions deploy openrouter-proxy
supabase secrets set OPENROUTER_API_KEY=sk-or-v1-YOUR-KEY
```

### Step 2: Test with Feature Flag
```json
{
  "OpenRouter": {
    "UseEdgeFunctionProxy": true,
 "EdgeFunctionUrl": "https://YOUR_PROJECT.supabase.co/functions/v1/openrouter-proxy"
  }
}
```

### Step 3: Deploy to Production
- Change `UseEdgeFunctionProxy: true` in appsettings.json
- Remove API key from client (leave placeholder)
- Deploy application

### Step 4: Remove Old Key
- Remove `ApiKey` from appsettings.json (leave placeholder for documentation)
- Generate new key in OpenRouter (for security)
- Update secret in Supabase

## Troubleshooting

### Problem: "OpenRouter API key not configured on server"

**Cause**: Secret was not set in Supabase

**Solution**:
```bash
supabase secrets set OPENROUTER_API_KEY=sk-or-v1-YOUR-KEY
supabase functions deploy openrouter-proxy # Redeploy after changing secrets
```

### Problem: "Missing authorization header"

**Cause**: Missing Supabase JWT token in request

**Solution**: Check if OpenRouterService adds Authorization header

### Problem: "CORS error"

**Cause**: Incorrect CORS configuration

**Solution**:
1. Check `_shared/cors.ts`
2. Make sure domain is allowed
3. Redeploy: `supabase functions deploy openrouter-proxy`

### Problem: "Timeout"

**Cause**: OpenRouter API slow / timeout

**Solution**:
- Increase `TimeoutSeconds` in appsettings.json
- Check OpenRouter status: https://status.openrouter.ai

## Costs

### Edge Functions (Supabase)
- **Free tier**: 500,000 invocations / month
- **Pro**: $0.00000225 per request ($2.25 per million)

### OpenRouter API
- According to selected model (no changes vs direct API)
- No additional costs for proxy

## Best Practices

### ✅ DO:
- Use Edge Function Proxy in production
- Store API keys as Supabase Secrets
- Monitor Edge Function logs
- Use CORS with specific domains
- Test locally before deployment
- Version Edge Functions (e.g., `openrouter-proxy-v2`)

### ❌ DON'T:
- Don't commit API keys to Git
- Don't use `Access-Control-Allow-Origin: *` in production
- Don't expose service role key
- Don't skip authentication in Edge Function
- Don't deploy without testing locally

## Usage Examples

### JavaScript/TypeScript Client

```typescript
const response = await fetch(
  'https://YOUR_PROJECT.supabase.co/functions/v1/openrouter-proxy',
  {
    method: 'POST',
    headers: {
    'Authorization': `Bearer ${supabaseAnonKey}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      model: 'google/gemma-3-27b-it:free',
  messages: [
{ role: 'user', content: 'Hello!' }
      ],
      max_tokens: 100
 })
  }
);

const data = await response.json();
console.log(data.choices[0].message.content);
```

### C# (OpenRouterService - already implemented)

```csharp
var request = new ChatCompletionRequest
{
    Model = "google/gemma-3-27b-it:free",
Messages = new List<ChatMessage>
    {
        ChatMessage.User("Hello!")
    },
    MaxTokens = 100
};

var response = await _openRouterService.SendChatCompletionAsync(request);
```

## Additional Resources

- [Supabase Edge Functions Docs](https://supabase.com/docs/guides/functions)
- [OpenRouter API Docs](https://openrouter.ai/docs)
- [Deno Deploy](https://deno.com/deploy) - Platform for Edge Functions
- [Supabase CLI Reference](https://supabase.com/docs/reference/cli)

