# MotoNomad - Roadmap Implementacji

## 📋 Spis treści

1. [Przegląd projektu](#1-przegląd-projektu)
2. [Dostępne plany implementacji](#2-dostępne-plany-implementacji)
3. [Sugerowana kolejność implementacji](#3-sugerowana-kolejność-implementacji)
4. [Graf zależności](#4-graf-zależności)
5. [Szacowany czas implementacji](#5-szacowany-czas-implementacji)
6. [Kamienie milowe](#6-kamienie-milowe)
7. [Checklist implementacji](#7-checklist-implementacji)

---

## 1. Przegląd projektu

**Projekt:** MotoNomad - Aplikacja do planowania podróży  
**Program:** 10xDevs  
**Technologia:** Blazor WebAssembly + MudBlazor + Supabase  
**Termin certyfikacji:** Listopad 2025 (opcjonalnie I termin: 16.11.2025)

### Kluczowe wymagania MVP

- ✅ Mechanizm logowania (Supabase Auth)
- ✅ Funkcja CRUD (Trips + Companions)
- ✅ Logika biznesowa (walidacja dat, obliczanie czasu podróży)
- ✅ Test e2e (logowanie + dodanie wycieczki)
- ✅ CI/CD (GitHub Actions)
- ✅ Dokumentacja (PRD, README, plany implementacji)
- ✅ Publiczny URL (GitHub Pages)

---

## 2. Dostępne plany implementacji

Wszystkie plany znajdują się w folderze `.ai/implementation-plans/UI/`:

| # | Plan | Plik | Status | Priorytet |
|---|------|------|--------|-----------|
| 1 | **Layout i Nawigacja** | `layout-and-navigation-implementation-plan.md` | ✅ Gotowy | 🔴 Wysoki |
| 2 | **Komponenty reużywalne** | `shared-components-implementation-plan.md` | ✅ Gotowy | 🔴 Wysoki |
| 3 | **Dialogi** | `dialogs-implementation-plan.md` | ✅ Gotowy | 🟡 Średni |
| 4 | **Logowanie** | `login-view-implementation-plan.md` | ✅ Gotowy | 🔴 Wysoki |
| 5 | **Rejestracja** | `register-view-implementation-plan.md` | ✅ Gotowy | 🔴 Wysoki |
| 6 | **Lista wycieczek** | `triplist-view-implementation-plan.md` | ✅ Gotowy | 🔴 Wysoki |
| 7 | **Tworzenie wycieczki** | `createtrip-view-implementation-plan.md` | ✅ Gotowy | 🔴 Wysoki |
| 8 | **Szczegóły wycieczki** | `tripdetails-view-implementation-plan.md` | ✅ Gotowy | 🔴 Wysoki |

**Legenda priorytetów:**
- 🔴 **Wysoki** - Krytyczne dla MVP, blokuje inne komponenty
- 🟡 **Średni** - Ważne dla MVP, ale nie blokujące
- 🟢 **Niski** - Nice to have, można zrobić później

---

## 3. Sugerowana kolejność implementacji

### Faza 1: Fundament

**Cel:** Przygotowanie podstawowej struktury aplikacji i infrastruktury.

**Kolejność:**

1. **Setup projektu**
   - Utworzenie projektu Blazor WASM
   - Instalacja MudBlazor i Supabase packages
   - Konfiguracja `appsettings.json`
   - Setup Supabase (utworzenie projektu, bazy danych)

2. **Layout i Nawigacja** → `layout-and-navigation-implementation-plan.md`
   - App.razor (routing + autoryzacja)
   - MainLayout.razor (MudLayout + timer bezczynności)
   - NavMenu.razor (menu boczne)
   - LoginDisplay.razor (status logowania)
   - CustomAuthenticationStateProvider

3. **Komponenty reużywalne (część podstawowa)** → `shared-components-implementation-plan.md`
   - EmptyState.razor
   - LoadingSpinner.razor

**Rezultat:** Działająca struktura aplikacji z nawigacją i podstawowymi komponentami.

---

### Faza 2: CRUD Wycieczek

**Cel:** Implementacja głównej funkcjonalności - zarządzanie wycieczkami.

**Kolejność:**

7. **Serwisy i komponenty Trip**
   - Implementacja ITripService
   - TripService.cs (CRUD operations)
   - TripForm.razor (reużywalny formularz) → `shared-components-implementation-plan.md`
   - TripListItem.razor (karta wycieczki) → `shared-components-implementation-plan.md`

8. **Lista wycieczek** → `triplist-view-implementation-plan.md`
   - TripList.razor (zakładki: Nadchodzące, Archiwalne)
   - Równoległe ładowanie (Task.WhenAll)
   - EmptyState dla pustych list
   - Floating Action Button (+)

9. **Tworzenie wycieczki** → `createtrip-view-implementation-plan.md`
   - CreateTrip.razor (użycie TripForm.razor)
   - Walidacja (nazwa, daty, transport)
   - Custom validation (EndDate > StartDate)

10. **Szczegóły wycieczki (część 1 - edycja)** → `tripdetails-view-implementation-plan.md`
    - TripDetails.razor (zakładka "Szczegóły")
    - Równoległe ładowanie Trip + Companions
    - Edycja wycieczki (TripForm w trybie edit)
    - RLS security handling (NotFoundException)

**Rezultat:** Pełny CRUD wycieczek - użytkownik może tworzyć, przeglądać, edytować i usuwać wycieczki.

---

### Faza 3: CRUD Towarzyszy

**Cel:** Implementacja zarządzania towarzyszami podróży.

**Kolejność:**

11. **Serwisy i komponenty Companion**
    - Implementacja ICompanionService
    - CompanionService.cs (CRUD operations)
    - CompanionForm.razor → `shared-components-implementation-plan.md`
    - CompanionList.razor → `shared-components-implementation-plan.md`

12. **Dialogi potwierdzenia** → `dialogs-implementation-plan.md`
    - DeleteTripConfirmationDialog.razor
    - DeleteCompanionConfirmationDialog.razor

13. **Szczegóły wycieczki (część 2 - towarzysze)** → `tripdetails-view-implementation-plan.md`
    - TripDetails.razor (zakładka "Towarzysze")
    - Dodawanie towarzyszy (CompanionForm)
    - Lista towarzyszy (CompanionList)
    - Usuwanie towarzyszy (dialog potwierdzenia)
    - Usuwanie wycieczki (dialog potwierdzenia)

**Rezultat:** Pełny CRUD towarzyszy - użytkownik może dodawać, przeglądać i usuwać towarzyszy.

---

### Faza 4: Autoryzacja

**Cel:** Implementacja logowania i rejestracji użytkowników.

**Kolejność:**

4. **AuthService i infrastruktura**
   - Implementacja IAuthService
   - AuthService.cs (Login, Register, Logout)
   - CustomAuthenticationStateProvider (aktualizacja po login/logout)

5. **Widok logowania** → `login-view-implementation-plan.md`
   - Login.razor (formularz logowania)
   - Walidacja (email, hasło)
   - Obsługa błędów AuthException

6. **Widok rejestracji** → `register-view-implementation-plan.md`
   - Register.razor (formularz rejestracji)
   - Walidacja (email, hasło, potwierdzenie hasła, displayName)
   - Obsługa błędów (email zajęty, hasło za słabe)

**Rezultat:** Działająca autoryzacja - użytkownik może się zarejestrować, zalogować i wylogować.

---

### Faza 5: Testy i finalizacja 

**Cel:** Testy, debugowanie, optymalizacja i deployment.

**Kolejność:**

14. **Testy jednostkowe i integracyjne**
    - Testy serwisów (AuthService, TripService, CompanionService)
    - Testy komponentów (bUnit)
    - Mock Supabase dla testów

15. **Testy E2E (Playwright)**
    - Test: Rejestracja + Logowanie
    - Test: Utworzenie wycieczki + Dodanie towarzysza
    - Test: Edycja wycieczki
    - Test: Usunięcie wycieczki

16. **CI/CD (GitHub Actions)**
    - Workflow: Build + Test + Deploy
    - Automatyczne deployment na GitHub Pages
    - Konfiguracja secrets (Supabase URL, Key)

17. **User Testing**
    - 5-10 sesji testowych z użytkownikami
    - Zbieranie feedbacku
    - Fixowanie krytycznych bugów

18. **Dokumentacja finalna**
    - README.md (kompletna instrukcja)
    - Deployment guide
    - User guide (opcjonalnie)

**Rezultat:** Gotowa aplikacja z testami, CI/CD i dokumentacją, wdrożona na GitHub Pages.

---

## 4. Graf zależności

```
┌─────────────────────────────────────────────────────────────┐
│                    FAZA 1: FUNDAMENT                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Setup projektu]                                           │
│         ↓                                                   │
│  [Layout + Nawigacja]  ←── [EmptyState, LoadingSpinner]    │
│         ↓                                                   │
└─────────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────────┐
│                 FAZA 2: AUTORYZACJA                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [AuthService + CustomAuthStateProvider]                    │
│         ↓                                                   │
│  [Login.razor]  ←────────┐                                 │
│         ↓                 │                                 │
│  [Register.razor] ────────┘                                 │
│         ↓                                                   │
└─────────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────────┐
│              FAZA 3: CRUD WYCIECZEK                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [TripService]                                              │
│         ↓                                                   │
│  [TripForm.razor] ←── [TripListItem.razor]                 │
│         ↓                      ↓                            │
│  [TripList.razor] ──────────────┘                          │
│         ↓                                                   │
│  [CreateTrip.razor]                                         │
│         ↓                                                   │
│  [TripDetails.razor (zakładka Szczegóły)]                   │
│         ↓                                                   │
└─────────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────────┐
│             FAZA 4: CRUD TOWARZYSZY                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [CompanionService]                                         │
│         ↓                                                   │
│  [CompanionForm.razor] ←── [CompanionList.razor]           │
│         ↓                            ↓                      │
│  [Dialogi potwierdzenia] ────────────┘                     │
│         ↓                                                   │
│  [TripDetails.razor (zakładka Towarzysze)]                  │
│         ↓                                                   │
└─────────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────────┐
│           FAZA 5: TESTY I FINALIZACJA                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Testy jednostkowe i integracyjne]                         │
│         ↓                                                   │
│  [Testy E2E (Playwright)]                                   │
│         ↓                                                   │
│  [CI/CD (GitHub Actions)]                                   │
│         ↓                                                   │
│  [User Testing + Dokumentacja]                              │
│         ↓                                                   │
│  ✅ GOTOWA APLIKACJA                                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 6. Kamienie milowe (Milestones)

### Milestone 1: Struktura aplikacji ✅

**Kryteria sukcesu:**
- ✅ Projekt Blazor WASM utworzony i skonfigurowany
- ✅ MudBlazor i Supabase packages zainstalowane
- ✅ Layout aplikacji działa (AppBar, Drawer, Main Content)
- ✅ Nawigacja działa (routing między stronami)
- ✅ Podstawowe komponenty (EmptyState, LoadingSpinner) gotowe

**Demonstracja:** Aplikacja się uruchamia, można przełączać się między pustymi stronami.

---

### Milestone 2: Autoryzacja działa ✅

**Kryteria sukcesu:**
- ✅ Użytkownik może się zarejestrować
- ✅ Użytkownik może się zalogować
- ✅ Użytkownik może się wylogować
- ✅ AuthorizeView reaguje na zmiany stanu logowania
- ✅ Timer bezczynności działa (auto-logout po 15 min)
- ✅ Błędy walidacji i uwierzytelnienia są obsługiwane

**Demonstracja:** Pełny flow: Register → Login → Nawigacja (zalogowany) → Logout → Login.

---

### Milestone 3: CRUD Wycieczek działa ✅
 
**Kryteria sukcesu:**
- ✅ Użytkownik może utworzyć wycieczkę
- ✅ Użytkownik widzi listę swoich wycieczek (zakładki: Nadchodzące, Archiwalne)
- ✅ Użytkownik może edytować wycieczkę
- ✅ Użytkownik może usunąć wycieczkę (z dialogiem potwierdzenia)
- ✅ Walidacja dat działa (EndDate > StartDate)
- ✅ Czas trwania wycieczki jest automatycznie obliczany

**Demonstracja:** Pełny flow: Create Trip → View List → Edit Trip → Delete Trip.

---

### Milestone 4: CRUD Towarzyszy działa ✅

**Kryteria sukcesu:**
- ✅ Użytkownik może dodać towarzysza do wycieczki
- ✅ Użytkownik widzi listę towarzyszy w szczegółach wycieczki
- ✅ Użytkownik może usunąć towarzysza (z dialogiem potwierdzenia)
- ✅ Licznik towarzyszy w zakładce jest dynamiczny
- ✅ EmptyState wyświetla się gdy brak towarzyszy

**Demonstracja:** Pełny flow: Create Trip → Add Companions → View List → Delete Companion.

---

### Milestone 5: Testy przechodzą ✅

**Kryteria sukcesu:**
- ✅ Testy jednostkowe serwisów przechodzą (coverage > 70%)
- ✅ Test E2E: Register + Login + Create Trip + Add Companion przechodzi
- ✅ CI/CD pipeline działa (build + test + deploy)
- ✅ Aplikacja wdrożona na GitHub Pages
- ✅ Brak krytycznych bugów

**Demonstracja:** GitHub Actions pipeline przechodzi zielono, aplikacja dostępna pod publicznym URL.

---

### Milestone 6: User Testing zakończony ✅

**Kryteria sukcesu:**
- ✅ 5-10 sesji testowych z użytkownikami przeprowadzonych
- ✅ Feedback zebrany i priorytetyzowany
- ✅ Krytyczne bugi naprawione
- ✅ User satisfaction score > 7/10
- ✅ Task completion rate > 85%

**Demonstracja:** Raport z testów użytkowników + lista naprawionych bugów.

---

### Milestone 7: Certyfikacja gotowa ✅

**Kryteria sukcesu:**
- ✅ Wszystkie obowiązkowe wymagania spełnione
- ✅ Dokumentacja kompletna (PRD, README, deployment guide)
- ✅ Aplikacja publicznie dostępna (GitHub Pages)
- ✅ Kod w repozytorium GitHub z historią commitów
- ✅ Prezentacja gotowa (demo + slajdy opcjonalnie)

**Demonstracja:** Live demo aplikacji 

---

## 7. Checklist implementacji

### ☑️ Przygotowanie środowiska

- [ ] Zainstalowany .NET 9.0 SDK
- [ ] Zainstalowany Visual Studio 2022 / VS Code / Rider
- [ ] Zainstalowany Git
- [ ] Utworzone konto Supabase (free tier)
- [ ] Utworzone repozytorium GitHub

### ☑️ Setup projektu

- [ ] Utworzony projekt Blazor WebAssembly
- [ ] Zainstalowane packages (MudBlazor, Supabase, Blazored.LocalStorage)
- [ ] Skonfigurowany `appsettings.json` (Supabase URL, Key)
- [ ] Utworzony projekt Supabase (baza danych, Auth)
- [ ] Wdrożone migracje SQL (schema + RLS + triggers)

### ☑️ Faza 1: Fundament

- [ ] App.razor (routing + autoryzacja)
- [ ] MainLayout.razor (MudLayout + timer)
- [ ] NavMenu.razor
- [ ] LoginDisplay.razor
- [ ] EmptyState.razor
- [ ] LoadingSpinner.razor
- [ ] CustomAuthenticationStateProvider

### ☑️ Faza 2: Autoryzacja

- [ ] IAuthService + AuthService.cs
- [ ] Login.razor + walidacja
- [ ] Register.razor + walidacja
- [ ] Integracja z Supabase Auth
- [ ] Obsługa błędów AuthException

### ☑️ Faza 3: CRUD Wycieczek

- [ ] ITripService + TripService.cs
- [ ] TripForm.razor (reużywalny)
- [ ] TripListItem.razor
- [ ] TripList.razor (zakładki)
- [ ] CreateTrip.razor
- [ ] TripDetails.razor (zakładka Szczegóły)
- [ ] Walidacja dat (EndDate > StartDate)

### ☑️ Faza 4: CRUD Towarzyszy

- [ ] ICompanionService + CompanionService.cs
- [ ] CompanionForm.razor
- [ ] CompanionList.razor
- [ ] DeleteTripConfirmationDialog.razor
- [ ] DeleteCompanionConfirmationDialog.razor
- [ ] TripDetails.razor (zakładka Towarzysze)

### ☑️ Faza 5: Testy i finalizacja

- [ ] Testy jednostkowe serwisów
- [ ] Testy komponentów (bUnit)
- [ ] Test E2E (Playwright): Register + Login + Create Trip + Add Companion
- [ ] GitHub Actions workflow (build + test + deploy)
- [ ] Deployment na GitHub Pages
- [ ] User testing (5-10 sesji)
- [ ] Dokumentacja (README, deployment guide)
- [ ] Prezentacja do certyfikacji

### ☑️ Opcjonalne (po MVP)

- [ ] PWA (instalowalna aplikacja)
- [ ] Tryb offline (IndexedDB)
- [ ] Export do PDF
- [ ] AI suggestions (atrakcje, trasy)
- [ ] Zarządzanie noclegami
- [ ] Budżet podróży
- [ ] Widok kalendarza/timeline
- [ ] Udostępnianie wycieczek innym użytkownikom

---

## 8. Wskazówki praktyczne

### 🎯 Strategia implementacji

**1. Start small, iterate fast**
- Zacznij od najprostszej wersji (happy path)
- Dodawaj walidację i error handling później
- Refaktoryzuj po uruchomieniu

**2. Test early, test often**
- Testuj każdy komponent po implementacji
- Nie czekaj do końca z testami E2E
- Fix bugów na bieżąco

**3. Commit often, push regularly**
- Commituj małe, atomowe zmiany
- Opisuj commity jasno (Conventional Commits)
- Push codziennie (backup + widoczność postępu)

**4. Dokumentuj na bieżąco**
- Dodawaj komentarze XML podczas pisania kodu
- Aktualizuj README przy dodawaniu funkcji
- Rób screenshoty do dokumentacji user testing

### 🐛 Debugging tips

**Blazor WASM:**
- F12 → Console - sprawdzaj błędy JS
- Używaj `console.log` dla debugowania stanu
- Blazor DevTools - inspekcja komponentów

**Supabase:**
- Supabase Dashboard → SQL Editor - testuj zapytania
- Supabase Dashboard → Auth - sprawdzaj użytkowników
- Logi RLS - sprawdzaj polityki bezpieczeństwa

**CI/CD:**
- GitHub Actions → Logs - sprawdzaj błędy buildu
- Testuj lokalnie przed pushem (`dotnet build`, `dotnet test`)

### 📚 Przydatne zasoby

**Dokumentacja:**
- [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [MudBlazor](https://mudblazor.com/)
- [Supabase](https://supabase.com/docs)
- [Playwright .NET](https://playwright.dev/dotnet/)

**Community:**
- MudBlazor Discord
- Blazor subreddit (r/Blazor)
- Stack Overflow (#blazor, #mudblazor)

---

## 9. Sukces projektu - definicja

Projekt MotoNomad będzie uznany za sukces jeśli:

### Funkcjonalne
- ✅ Użytkownik może utworzyć konto i zalogować się
- ✅ Użytkownik może stworzyć wycieczkę w < 2 minuty
- ✅ Użytkownik może dodać towarzyszy do wycieczki
- ✅ Użytkownik może edytować i usuwać wycieczki
- ✅ Walidacja dat działa poprawnie (EndDate > StartDate)
- ✅ Aplikacja działa na mobile i desktop

### Techniczne
- ✅ Kod jest czytelny i zgodny z best practices
- ✅ Testy przechodzą (unit + E2E)
- ✅ CI/CD pipeline działa automatycznie
- ✅ Aplikacja wdrożona na publicznym URL
- ✅ Brak krytycznych bugów

### User Experience
- ✅ Time to First Trip < 3 minuty
- ✅ User satisfaction > 7/10
- ✅ Task completion rate > 85%
- ✅ Responsywny design (mobile + desktop)

### Program 10xDevs
- ✅ Wszystkie obowiązkowe wymagania spełnione
- ✅ Dokumentacja kompletna
- ✅ Projekt gotowy do certyfikacji
- ✅ Demo działa bez błędów

---

## 10. Co dalej po MVP?

Po zakończeniu MVP i uzyskaniu certyfikacji, możliwe rozszerzenia:

### Priorytet 1 (najbardziej pożądane przez użytkowników)
- **Tryb offline** - cache w IndexedDB
- **PWA** - instalowalna aplikacja
- **Export do PDF** - plan podróży jako plik

### Priorytet 2 (wartość biznesowa)
- **AI suggestions** - atrakcje, restauracje, trasy
- **Zarządzanie budżetem** - koszty podróży
- **Noclegi** - rezerwacje i lista hoteli

### Priorytet 3 (nice to have)
- **Udostępnianie wycieczek** - współpraca między użytkownikami
- **Widok kalendarza** - timeline podróży
- **Integracje** - mapy, pogoda, rezerwacje

---

**Document Status:** ✅ Ready for Implementation  
**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025

**Powodzenia w implementacji! 🚀**