# Changelog - .NET 10 Migration

## [10.0.0] - 2025-01-30

### 🎉 Major Update: Migrated to .NET 10

#### Added
- Full support for .NET 10.0 runtime
- Updated to C# 14 language features
- Support for latest Blazor WebAssembly improvements

#### Changed
- **Framework Target**: net9.0 → net10.0 (all projects)
- **Microsoft.AspNetCore.Components.Authorization**: 9.0.10 → 10.0.2
- **Microsoft.AspNetCore.Components.WebAssembly**: 9.0.9 → 10.0.2
- **Microsoft.AspNetCore.Components.WebAssembly.DevServer**: 9.0.9 → 10.0.2
- **Microsoft.Extensions.Configuration.Binder**: 9.0.10 → 10.0.2
- **Microsoft.Extensions.Http**: 9.0.10 → 10.0.2
- **Microsoft.Extensions.Configuration**: 9.0.10 → 10.0.2 (E2ETests)
- **Microsoft.Extensions.Configuration.EnvironmentVariables**: 9.0.10 → 10.0.2 (E2ETests)
- **Microsoft.Extensions.Configuration.Json**: 9.0.10 → 10.0.2 (E2ETests)
- **Microsoft.Extensions.Configuration.UserSecrets**: 9.0.10 → 10.0.2 (E2ETests)
- **MudBlazor**: 8.13.0 → 8.15.0

#### Verified (No Changes Required)
- **supabase-csharp**: 0.16.2 (confirmed .NET 10 compatibility)
- **Blazored.LocalStorage**: 4.5.0 (confirmed .NET 10 compatibility)
- **xUnit**: 2.9.2
- **Moq**: 4.20.72
- **FluentAssertions**: 6.12.1
- **Microsoft.Playwright.NUnit**: 1.55.0

#### Testing
- ✅ All 20 unit tests passing
- ✅ All 5 active E2E tests passing
- ✅ 0 build errors
- ✅ Manual testing confirmed

#### CI/CD Updates
- ✅ Updated `.github/workflows/ci.yml` to .NET 10.0.x SDK
- ✅ Updated `.github/workflows/deploy.yml` to .NET 10.0.x SDK
- ✅ Updated `.github/workflows/pr-check.yml` to .NET 10.0.x SDK
- ✅ Updated Playwright path in pr-check workflow (net9.0 → net10.0)
- ✅ README.md already reflects .NET 10.0 requirements

#### Known Warnings
- ⚠️ NU1902: Vulnerability warnings in supabase-csharp dependencies (non-blocking)
- ⚠️ CS8604, CS8602, CS8601: Null reference warnings (non-blocking)
- ⚠️ MUD0002: MudBlazor analyzer warnings (cosmetic)

#### Performance Improvements
- Faster runtime execution with .NET 10 optimizations
- Improved Blazor WebAssembly startup time
- Better IL trimming and linking

#### Developer Experience
- Updated SDK requirement: .NET 10.0 SDK or later
- Enhanced IDE support with latest tooling
- Better debugging experience

---

## Previous Versions

### [9.0.x] - Before 2025-01-30
- Running on .NET 9.0
- Microsoft packages v9.0.x
- MudBlazor v8.13.0
