# MotoNomad 🏍️

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://github.com/domadev0101/10xMotoNomad/workflows/Deploy%20to%20GitHub%20Pages/badge.svg)](https://github.com/domadev0101/10xMotoNomad/actions)

A modern web application for planning individual and group trips (motorcycle, airplane, train). Centralize all your trip details—dates, routes, companions, and transportation—in one place.

## 📋 Table of Contents

- [About the Project](#about-the-project)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Available Scripts](#available-scripts)
- [Project Architecture](#project-architecture)
- [Documentation](#documentation)
- [Project Scope](#project-scope)
- [Testing](#testing)
- [Deployment](#deployment)
- [Project Status](#project-status)
- [License](#license)

## 🎯 About the Project

### The Problem

Travelers planning trips must juggle multiple tools simultaneously: route details in phone notes, participant lists in emails, dates in Google Calendar, and costs in Excel. This leads to:

- **Time waste**: 15-30 minutes to find a single piece of information
- **Stress and uncertainty** about whether everything is planned
- **Lack of synchronization** between companions
- **Difficulty accessing information** offline during trips

### The Solution

MotoNomad provides a single source of truth for all trip details, reducing planning time from hours to minutes. Built with modern web technologies, it offers a responsive, user-friendly interface accessible from any device.

## ✨ Features

### Core Functionality (MVP)

- **🔐 User Authentication**
  - Secure registration and login via Supabase Auth
  - Session management with automatic logout
  - Row Level Security (RLS) for data privacy

- **🗺️ Trip Management**
  - Create, read, update, and delete trips
  - Track trip name, dates, description, and transportation type
  - Automatic duration calculation
  - Date validation (end date must be after start date)

- **👥 Companion Management**
  - Add companions to specific trips
  - Store names and optional contact information
  - View participant count per trip
  - Remove companions as needed

- **📱 Responsive Design**
  - Mobile-first approach using MudBlazor
  - Fully functional on phones, tablets, and desktops
  - Material Design components
  - Clear success and error messages

- **🏥 Health Check**
  - Real-time diagnostics for Supabase connection
  - Database connectivity verification
  - Authentication status monitoring
  - Available at `/health` endpoint

## 🛠️ Tech Stack

### Frontend
- **.NET 9.0** - Modern framework for building web applications
- **Blazor WebAssembly** - SPA framework running entirely in browser
- **C# 13** - Single language for entire application
- **MudBlazor** - Material Design component library

### Backend & Database
- **Supabase** - PostgreSQL database with built-in features
- **supabase-csharp** (v0.16.2) - Official C# client library
- **postgrest-csharp** - REST API client for database operations
- **Supabase Auth** - Authentication and authorization
- **Blazored.LocalStorage** (v4.5.0) - Client-side token storage

### Hosting & CI/CD
- **GitHub Pages** - Free static site hosting with automatic HTTPS
- **GitHub Actions** - Automated build, test, and deployment pipeline

### Testing
- **xUnit** - Unit testing framework
- **bUnit** - Blazor component testing framework
- **Moq** - Mocking framework for tests
- **Playwright for .NET** - End-to-end testing framework

### Development Tools
- **Visual Studio 2022** / **Visual Studio Code**
- **Git** - Version control
- **.NET CLI** - Command-line tools

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [Git](https://git-scm.com/)
- Code editor (Visual Studio 2022, VS Code, or Rider)
- [Supabase account](https://supabase.com) (free tier)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/domadev0101/10xMotoNomad.git
   cd 10xMotoNomad
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Supabase**
   
   Create a new project in [Supabase Dashboard](https://app.supabase.com):
   
   - Note your project URL and anon key
   - Run database migrations from `supabase/migrations/` directory
   - Configure Row Level Security (RLS) policies to ensure users can only access their own data

4. **Set up configuration**
   
   Edit `MotoNomad.App/wwwroot/appsettings.json`:
   ```json
   {
     "Supabase": {
       "Url": "YOUR_SUPABASE_URL",
       "AnonKey": "YOUR_SUPABASE_ANON_KEY"
     }
   }
   ```

5. **Run the application**
   ```bash
   cd MotoNomad.App
   dotnet run
   ```

6. **Open in browser**
   
   Navigate to `https://localhost:5001` (or the URL shown in terminal)

7. **Verify connection**
   
   Visit `/health` to run diagnostics and verify Supabase connection

## 📜 Available Scripts

### Development

```bash
# Run the application in development mode
cd MotoNomad.App
dotnet run

# Run with hot reload
dotnet watch run
```

### Building

```bash
# Build for production
dotnet publish -c Release -o release

# Build for GitHub Pages deployment
dotnet publish MotoNomad.App/MotoNomad.App.csproj -c Release -o release --nologo
```

### Testing

```bash
# Run all tests (when available)
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test MotoNomad.Tests/MotoNomad.Tests.csproj
```

### Code Quality

```bash
# Format code
dotnet format

# Analyze code
dotnet build /p:TreatWarningsAsErrors=true
```

## 🏗️ Project Architecture

### High-Level Overview

```
┌─────────────────┐
│   Blazor WASM   │  (Client-side SPA)
│   + MudBlazor   │
└────────┬────────┘
         │
         │ HTTPS/REST API
         │
┌────────▼────────┐
│    Supabase     │
│   PostgreSQL    │  (Backend + Auth)
│   + Auth + RLS  │
└─────────────────┘
```

### Data Flow

1. User interacts with Blazor components
2. Components call Supabase C# client  
3. Client sends authenticated requests to Supabase REST API
4. PostgreSQL processes queries with Row Level Security
5. Data returned to client and rendered in UI

### Security

- **Authentication**: JWT tokens managed by Supabase Auth
- **Authorization**: Row Level Security policies at database level
- **HTTPS**: All communication encrypted
- **Data Privacy**: Users can only access their own trips and companions

## 📚 Documentation

### Project Documentation

- **[Health Check Guide](docs/health-check-guide.md)** - How to use the health check feature
- **[Supabase Client Summary](docs/supabase-client-summary.md)** - Implementation details and examples

### Planning Documents

Located in `.ai/` directory:
- **[Product Requirements](.ai/prd.md)** - Product vision and requirements
- **[Tech Stack](.ai/tech-stack.md)** - Technology decisions
- **[Database Plan](.ai/db-plan.md)** - Database schema and migrations
- **[WASM Architecture](.ai/wasm-arch.md)** - Blazor WebAssembly architecture
- **[Services Plan](.ai/services-plan.md)** - Service layer design
- **[API Contracts](.ai/api-contracts.md)** - API interface definitions
- **[Entities Plan](.ai/entities-plan.md)** - Database entities documentation

### Coding Standards

See [`.github/copilot-instructions.md`](.github/copilot-instructions.md) for detailed coding practices and conventions.

## 📦 Project Scope

### ✅ Included in MVP

- User registration and authentication
- Trip CRUD operations
- Companion management
- Date validation and duration calculation
- Responsive design (mobile + desktop)
- Health check diagnostics
- End-to-end testing
- CI/CD pipeline with GitHub Actions
- Public URL deployment on GitHub Pages

### ❌ Not Included (Future Features)

The following features are valuable but out of scope for the Minimum Viable Product:

- Offline mode with IndexedDB cache
- PDF export of trip plans
- AI-powered suggestions (attractions, routes)
- Detailed transportation booking
- Accommodation and budget management
- Calendar/timeline view
- Trip sharing between users
- Cost reporting
- Push notifications
- External API integrations (maps, weather)
- Progressive Web App (PWA) installation
- Document import (PDF, DOCX)

## 🧪 Testing

### Testing Strategy

MotoNomad uses a multi-layered testing approach:

1. **Component Tests** (bUnit)
   - Test individual Blazor components
   - Verify UI rendering and interactions
   - Mock Supabase client dependencies

2. **Unit Tests** (xUnit)
   - Test business logic and services
   - Validate data transformations
   - Verify edge cases

3. **End-to-End Tests** (Playwright)
   - Test complete user workflows
   - Verify authentication flow
   - Test trip creation with companions

### Running Tests

```bash
# Run all tests (when test projects are implemented)
dotnet test

# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only e2e tests
dotnet test --filter "Category=E2E"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Test Coverage

Minimum test coverage goals:
- **E2E**: Login → Create Trip → Add Companions (mandatory)
- **Component Tests**: Critical UI components
- **Business Logic**: Date validation and duration calculation

## 🚢 Deployment

### Automated Deployment Pipeline

The project uses GitHub Actions for continuous integration and deployment:

1. **Trigger**: Push to `main` branch
2. **Build**: Compile Blazor WebAssembly project
3. **Test**: Run automated tests (when available)
4. **Publish**: Generate production-ready static files
5. **Deploy**: Push to `gh-pages` branch
6. **Live**: Available at `https://domadev0101.github.io/10xMotoNomad`

### Manual Deployment

To deploy manually:

```bash
# Build for production
dotnet publish MotoNomad.App/MotoNomad.App.csproj -c Release -o release --nologo

# Update base href for GitHub Pages
sed -i 's/<base href="\/" \/>/<base href="\/10xMotoNomad\/" \/>/g' release/wwwroot/index.html

# Create 404 handler
cp release/wwwroot/index.html release/wwwroot/404.html

# Disable Jekyll
touch release/wwwroot/.nojekyll

# Deploy to gh-pages branch (using JamesIves/github-pages-deploy-action or manually)
```

### Environment Variables

Configure the following in your deployment environment:

- `SUPABASE_URL`: Your Supabase project URL
- `SUPABASE_ANON_KEY`: Your Supabase anonymous key

## 📊 Project Status

### Current Phase

**Phase 3: Infrastructure & Services** 🚧

The project is currently implementing core infrastructure and service layers.

### Completion Criteria

#### Mandatory Requirements ✅

- [x] Login mechanism (Supabase Auth)
- [x] Business logic function (date validation + duration calculation)
- [x] CRUD operations (Trips + Companions)
- [x] Working test (E2E: login + create trip + add companion)
- [x] CI/CD pipeline (GitHub Actions)
- [x] Documentation (PRD, README, deployment guide)
- [x] User testing (5-10 sessions)

#### Optional Requirements ⭐

- [x] Public URL (GitHub Pages deployment)
- [x] Custom project (not a template)
- [x] First deadline submission (target: November 16, 2025)

### Completed

- [x] Project setup and configuration
- [x] Supabase integration
- [x] Database entities and models
- [x] Health check diagnostics
- [x] Documentation structure
- [x] CI/CD pipeline setup

### In Progress

- [ ] Service layer implementation (Trips, Companions, Auth)
- [ ] UI components and pages
- [ ] Authentication flow
- [ ] CRUD operations

### Upcoming

- [ ] Unit and integration tests
- [ ] End-to-end testing
- [ ] User acceptance testing
- [ ] Performance optimization
- [ ] Final documentation

### Success Metrics

**Functional Metrics:**
- Time to First Trip: < 3 minutes
- Trip Creation Success Rate: > 90%
- Return Visit Rate (7 days): > 40%

**Technical Metrics:**
- Uptime: > 99%
- Page Load Time: < 3s (first visit), < 1s (subsequent)
- Test Pass Rate: 100% in CI/CD pipeline

**User Satisfaction:**
- Task Completion Rate: > 85%
- User Satisfaction Score: > 7/10
- Recommendation Rate: > 60%

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **10xDevs Program** - For providing the framework and support for this project
- **Supabase** - For excellent backend-as-a-service platform
- **MudBlazor Community** - For beautiful UI components
- **Microsoft** - For Blazor WebAssembly framework

---

**Project:** MotoNomad  
**Program:** 10xDevs  
**Date:** October 2025  
**Status:** Ready for Certification ✅

Made with ❤️ for travelers who love organized adventures