# Tech Stack - MotoNomad

## Frontend
- .NET 9.0
- Blazor WebAssembly (standalone)
- C# 
- MudBlazor (UI Components)

## Backend & Database
- Supabase (PostgreSQL)
- supabase-csharp (Official C# Client)
- postgrest-csharp (REST API Client)
- Supabase Auth (Authentication & Authorization)

## Hosting & CI/CD
- GitHub Pages (Static Site Hosting)
- GitHub Actions (Continuous Integration & Deployment)

## Testing
- bUnit (Blazor Component Testing)
- Playwright for .NET (End-to-End Testing)

## Development Tools
- Visual Studio 2022 / Visual Studio Code
- Git & GitHub
- .NET CLI

## Architecture
- **Frontend Architecture:** Single Page Application (SPA)
- **Data Flow:** Client → Supabase REST API → PostgreSQL
- **Authentication:** Supabase Auth with JWT tokens
- **Security:** Row Level Security (RLS) in Supabase

## Key Dependencies
- Microsoft.AspNetCore.Components.WebAssembly
- MudBlazor
- Supabase Community Libraries
- System.Net.Http (for API calls)

## Deployment Pipeline
1. Push to `main` branch
2. GitHub Actions triggers build
3. Run automated tests (bUnit + Playwright)
4. Build Blazor WebAssembly release
5. Deploy to GitHub Pages
6. Live at: `username.github.io/MotoNomad`

## Why This Stack?

### Blazor WebAssembly
- Single language (C#) for entire application
- Runs entirely in browser (no server required)
- Perfect for GitHub Pages hosting
- Strong typing and IntelliSense support

### Supabase
- Free tier sufficient for MVP (500MB storage, 50K MAU)
- Built-in authentication (no custom implementation needed)
- PostgreSQL with Row Level Security
- REST API automatically generated
- Real-time capabilities for future features
- No backend code required

### MudBlazor
- Material Design components
- Fully responsive
- Rich component library
- Excellent documentation

### GitHub Pages
- Free hosting for static sites
- Automatic HTTPS
- Simple deployment via GitHub Actions
- Perfect for Blazor WebAssembly

---

**Last Updated:** October 2025  
**Project:** MotoNomad  
**Program:** 10xDevs