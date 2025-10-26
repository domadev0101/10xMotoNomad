# OpenRouter API Setup Guide

This guide explains how to configure OpenRouter API for AI-powered trip suggestions in MotoNomad.

## What is OpenRouter?

OpenRouter is a unified API gateway that provides access to multiple AI models (GPT-4, Claude, Llama, etc.) through a single interface. It's used in MotoNomad to generate intelligent trip suggestions based on user input.

## Features Using OpenRouter

- **AI Trip Suggestions**: Get intelligent destination and activity recommendations
- **Smart Itinerary Planning**: AI-powered daily schedule suggestions
- **Route Optimization**: Suggestions for best routes based on transportation type

## Setup Instructions

### Step 1: Create OpenRouter Account

1. Go to [OpenRouter.ai](https://openrouter.ai)
2. Click "Sign Up" or "Login"
3. Sign up using your preferred method (GitHub, Google, or Email)

### Step 2: Get API Key

1. After logging in, go to [Keys](https://openrouter.ai/keys)
2. Click "Create Key"
3. Give your key a name (e.g., "MotoNomad Dev")
4. Copy the generated API key (starts with `sk-or-v1-...`)
5. **Important**: Store this key securely - you won't be able to see it again!

### Step 3: Add Credits (Optional)

OpenRouter uses a pay-as-you-go model:

- New accounts may receive free credits
- Typical costs: $0.0002-0.03 per request (depending on model)
- Add credits at [Settings > Credits](https://openrouter.ai/settings/credits)

**Note**: The app will gracefully handle cases where you run out of credits.

### Step 4: Configure MotoNomad

Edit `MotoNomad.App/wwwroot/appsettings.json`:

```json
{
  "Supabase": {
    "Url": "YOUR_SUPABASE_URL",
    "AnonKey": "YOUR_SUPABASE_ANON_KEY"
  },
  "OpenRouter": {
  "ApiKey": "sk-or-v1-YOUR-ACTUAL-API-KEY-HERE",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "HttpReferer": "https://domadev0101.github.io/10xMotoNomad",
    "AppTitle": "MotoNomad - Travel Planning App",
    "TimeoutSeconds": 60,
    "MaxRetries": 3,
    "MaxConcurrentRequests": 5,
    "MinRequestDelayMs": 100
}
}
```

**Replace** `sk-or-v1-YOUR-ACTUAL-API-KEY-HERE` with your actual API key from Step 2.

### Step 5: Verify Configuration

1. Run the application:
   ```bash
   cd MotoNomad.App
   dotnet run
   ```

2. Check the console logs for:
   - ✅ No warnings about missing API key
   - ❌ If you see: "OpenRouter API key is not configured properly" - check your appsettings.json

3. Test AI features:
   - Create a new trip
   - Click "Get AI Suggestions" button
   - If configured correctly, you should see AI-generated suggestions

## Configuration Options

### Required Settings

- **ApiKey**: Your OpenRouter API key (from Step 2)
- **BaseUrl**: `https://openrouter.ai/api/v1` (don't change)
- **HttpReferer**: Your app's URL (for OpenRouter analytics)
- **AppTitle**: Your app's name (for OpenRouter analytics)

### Optional Settings

- **TimeoutSeconds** (default: 60): Maximum time to wait for API response
- **MaxRetries** (default: 3): Number of retry attempts on failure
- **MaxConcurrentRequests** (default: 5): Maximum parallel API calls
- **MinRequestDelayMs** (default: 100): Minimum delay between requests (rate limiting)

## Running Without OpenRouter

**Good news**: MotoNomad works perfectly without OpenRouter configured!

- Core features (trip management, companions) work normally
- AI suggestions button will show an error message
- No crashes or blocking issues
- You can add the API key later

## Troubleshooting

### Error: "OpenRouter API key is not configured"

**Solution**: Add a valid API key to `appsettings.json`

### Error: "Received non-JSON response"

**Causes**:
1. Invalid API key
2. Missing API key
3. API key without credits
4. Incorrect BaseUrl configuration (missing trailing slash)

**Solution**:
1. Verify your API key is correct (starts with `sk-or-v1-`)
2. Check you have credits: [OpenRouter Settings](https://openrouter.ai/settings/credits)
3. Verify `BaseUrl` is exactly `https://openrouter.ai/api/v1` (no trailing slash needed - handled automatically)
4. Try creating a new API key
5. Check console logs for detailed error messages

### Error: "Rate limit exceeded"

**Causes**: Too many requests in short time

**Solution**:
1. Wait 1 minute and try again
2. Increase `MinRequestDelayMs` in appsettings.json
3. Add more credits to increase rate limits

### Error: "Insufficient credits"

**Solution**: Add credits at [OpenRouter Settings](https://openrouter.ai/settings/credits)

### AI Suggestions Not Working

**Checklist**:
- [ ] API key is correct and starts with `sk-or-v1-`
- [ ] API key is not a placeholder (`your-api-key-here`)
- [ ] You have credits in your OpenRouter account
- [ ] No console errors about OpenRouter
- [ ] Internet connection is working

## Security Best Practices

### ⚠️ Important Security Notes

1. **Never commit API keys to Git**
   - `appsettings.json` is in `.gitignore`
   - Use `appsettings.example.json` for templates

2. **Never expose API keys in client code**
   - Keys are only used server-side in services
   - Not accessible via browser DevTools

3. **Rotate keys regularly**
   - Create new keys every few months
   - Delete old keys after rotation

4. **Use separate keys for dev/prod**
   - Development: Low credit limit
   - Production: Higher limit with monitoring

5. **Monitor usage**
 - Check [OpenRouter Dashboard](https://openrouter.ai/activity)
   - Set up billing alerts

## Cost Management

### Typical Usage Costs

| Model | Cost per Request | Recommended For |
|-------|-----------------|-----------------|
| GPT-3.5 Turbo | ~$0.0002 | Development & Testing |
| GPT-4 Turbo | ~$0.01 | Production (high quality) |
| Claude 3 Haiku | ~$0.0005 | Production (balanced) |
| Llama 3 8B | ~$0.0001 | Budget option |

### Cost Estimation

- **Development**: ~$0.10-0.50/day (50-100 test requests)
- **Production**: ~$1-10/day (depending on usage)

### Cost Optimization Tips

1. Use cheaper models for development
2. Cache AI responses when possible
3. Implement request batching
4. Set credit limits to prevent overspending

## API Key Management

### Creating Multiple Keys

Create separate keys for:
- **Development**: Local testing with low limits
- **Staging**: Pre-production testing
- **Production**: Live application with monitoring

### Key Naming Convention

```
MotoNomad-Dev-YourName
MotoNomad-Staging
MotoNomad-Production
```

### Revoking Keys

If a key is compromised:
1. Go to [OpenRouter Keys](https://openrouter.ai/keys)
2. Click "Delete" next to the compromised key
3. Create a new key
4. Update `appsettings.json` with new key

## Further Resources

- **OpenRouter Documentation**: https://openrouter.ai/docs
- **API Reference**: https://openrouter.ai/docs/api-reference
- **Model Comparison**: https://openrouter.ai/models
- **Pricing**: https://openrouter.ai/docs/pricing
- **Status Page**: https://status.openrouter.ai

## Support

### OpenRouter Support

- **Discord**: [OpenRouter Community](https://discord.gg/openrouter)
- **Email**: support@openrouter.ai
- **GitHub Issues**: [OpenRouter GitHub](https://github.com/OpenRouterTeam/openrouter-runner)

### MotoNomad Support

- **GitHub Issues**: [10xMotoNomad Issues](https://github.com/domadev0101/10xMotoNomad/issues)
- **Documentation**: `.ai/` directory in repository

---

**Last Updated**: December 2024  
**Version**: 1.0  
**Status**: Production Ready
