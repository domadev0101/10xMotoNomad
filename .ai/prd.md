# Dokument wymagań produktu (PRD) – MotoNomad

## 1. Przegląd produktu
MotoNomad to aplikacja webowa do planowania podróży indywidualnych i grupowych (motocykl, samolot, pociąg). Umożliwia użytkownikom centralne zarządzanie wszystkimi szczegółami wyjazdu: datami, trasą, współtowarzyszami i rodzajem transportu w jednym miejscu. Aplikacja rozwiązuje problem chaosu informacyjnego, który powstaje przy używaniu wielu narzędzi (notatki, kalendarze, Excel, komunikatory).

## 2. Problem użytkownika
Podróżnicy planujący wyjazdy grupowe muszą żonglować wieloma narzędziami jednocześnie: szczegóły trasy w notatkach telefonu, lista uczestników w mailach/SMS-ach, daty w kalendarzu Google, koszty w Excelu. To prowadzi do:
- Straty czasu (15-30 min na znalezienie jednej informacji)
- Stresu i niepewności czy wszystko jest zaplanowane
- Braku synchronizacji między współtowarzyszami
- Trudności z dostępem do informacji offline w trasie

Celem rozwiązania jest skrócenie czasu planowania podróży z godzin do kilku minut oraz zapewnienie jednego źródła prawdy dla wszystkich szczegółów wyjazdu.

## 3. Wymagania funkcjonalne

### 3.1 Autoryzacja i konta użytkowników
- Rejestracja użytkownika (email + hasło) przez Supabase Auth
- Logowanie do aplikacji (Supabase Auth SDK)
- Zarządzanie sesją użytkownika (Supabase session management)
- Możliwość usunięcia konta wraz z powiązanymi danymi (soft delete w Supabase)

### 3.2 Zarządzanie podróżami (CRUD)
- Tworzenie nowej podróży z polami:
  - Nazwa podróży (wymagane)
  - Data rozpoczęcia i zakończenia (wymagane)
  - Opis (opcjonalny)
  - Rodzaj transportu (dropdown: motocykl, samolot, pociąg, samochód, inny)
- Edycja wszystkich pól istniejącej podróży
- Usuwanie podróży z potwierdzeniem
- Wyświetlanie listy wszystkich podróży użytkownika (sortowanie po dacie)

### 3.3 Zarządzanie współtowarzyszami
- Dodawanie współtowarzyszy do konkretnej podróży (imię i nazwisko + opcjonalny kontakt)
- Usuwanie współtowarzyszy z podróży
- Wyświetlanie listy współtowarzyszy dla wybranej podróży
- Licznik uczestników podróży

### 3.4 Logika biznesowa
- Walidacja dat: data zakończenia musi być późniejsza niż data rozpoczęcia
- Automatyczne obliczanie czasu trwania podróży w dniach
- Komunikaty błędów przy niepoprawnych danych

### 3.5 Interfejs użytkownika
- Responsywny design (MudBlazor) dostosowany do mobile i desktop
- Przejrzysty widok listy podróży z kluczowymi informacjami
- Szybki dostęp do głównych funkcji (max 2 kliknięcia)
- Czytelne komunikaty sukcesu i błędów

### 3.6 Jakość i deployment
- Test e2e: logowanie + utworzenie podróży + dodanie współtowarzyszy
- Pipeline CI/CD: GitHub Actions (build + testy + deploy)
- Hosting na GitHub Pages z publicznym URL
- Dokumentacja: README, instrukcja deployment

## 4. Granice produktu

### 4.1 Poza zakresem MVP:
- Tryb offline z IndexedDB (cache lokalny)
- Eksport planu podróży do PDF
- Podpowiedzi AI (atrakcje, rekomendacje tras)
- Szczegółowa organizacja transportu (rezerwacje biletów, lotów)
- Zarządzanie noclegami i budżetem
- Widok kalendarza/timeline podróży
- Udostępnianie podróży między użytkownikami
- Generowanie raportów kosztów
- Powiadomienia i przypomnienia
- Integracje z zewnętrznymi API (mapy, pogoda)
- PWA (instalowalna aplikacja mobilna)
- Import dokumentów (PDF, DOCX)
- Publiczne API dla developerów

### 4.2 Uzasadnienie granic:
Te funkcje są wartościowe, ale nie są niezbędne do rozwiązania głównego problemu: "chaos informacyjny przy planowaniu podróży". MVP skupia się na stworzeniu centralnego repozytorium informacji – jeśli podstawowa idea nie zadziała, dodatkowe funkcje też nie pomogą.

## 5. Historyjki użytkowników

### US-001: Rejestracja konta
**Jako** nowy użytkownik  
**Chcę** się zarejestrować w aplikacji  
**Aby** móc zapisywać swoje podróże i mieć do nich dostęp z różnych urządzeń

**Kryteria akceptacji:**
- Formularz rejestracyjny zawiera pola: email i hasło (minimum 8 znaków)
- Email jest walidowany (poprawny format)
- Po poprawnym wypełnieniu konto jest tworzone i użytkownik zostaje automatycznie zalogowany
- Wyświetlany jest komunikat potwierdzający pomyślną rejestrację

### US-002: Logowanie do aplikacji
**Jako** zarejestrowany użytkownik  
**Chcę** móc się zalogować  
**Aby** mieć dostęp do moich podróży

**Kryteria akceptacji:**
- Formularz logowania zawiera pola: email i hasło
- Po podaniu prawidłowych danych użytkownik zostaje przekierowany do listy podróży
- Błędne dane wyświetlają czytelny komunikat o błędzie
- Sesja jest zachowana (nie wymaga ponownego logowania przy kolejnej wizycie)

### US-003: Tworzenie nowej podróży
**Jako** zalogowany użytkownik  
**Chcę** utworzyć nową podróż z podstawowymi danymi  
**Aby** mieć punkt startowy do szczegółowego planowania

**Kryteria akceptacji:**
- Formularz zawiera pola: nazwa (wymagane), data start (wymagane), data koniec (wymagane), opis (opcjonalny), transport (dropdown)
- Data końca musi być późniejsza niż data startu – w przeciwnym razie wyświetla się błąd walidacji
- Transport wybierany z listy: motocykl, samolot, pociąg, samochód, inny
- Po zapisie wyświetla się komunikat sukcesu
- Nowa podróż pojawia się natychmiast na liście podróży
- Cała operacja zajmuje mniej niż 2 minuty

### US-004: Przeglądanie listy podróży
**Jako** użytkownik z kilkoma zaplanowanymi podróżami  
**Chcę** zobaczyć wszystkie moje podróże na jednej liście  
**Aby** szybko odnaleźć interesujący mnie wyjazd

**Kryteria akceptacji:**
- Lista pokazuje wszystkie podróże użytkownika
- Każda podróż wyświetla: nazwę, daty, rodzaj transportu, czas trwania w dniach
- Podróże są sortowane od najnowszej daty rozpoczęcia
- Wyraźne oznaczenie podróży przeszłych vs przyszłych
- Kliknięcie na podróż przenosi do widoku szczegółów

### US-005: Edycja podróży
**Jako** organizator wyjazdu  
**Chcę** edytować szczegóły istniejącej podróży  
**Aby** aktualizować plany w miarę ich zmiany

**Kryteria akceptacji:**
- Formularz edycji jest identyczny jak formularz tworzenia
- Wszystkie pola są pre-wypełnione aktualnymi danymi
- Ta sama walidacja jak przy tworzeniu
- Po zapisie widoczny komunikat sukcesu i zmiany są natychmiast widoczne na liście
- Możliwość anulowania edycji (powrót bez zmian)

### US-006: Usuwanie podróży
**Jako** użytkownik  
**Chcę** usunąć podróż, której już nie planuję  
**Aby** utrzymać listę aktualną i przejrzystą

**Kryteria akceptacji:**
- Przycisk "Usuń" dostępny w widoku szczegółów podróży
- Dialog potwierdzenia: "Czy na pewno chcesz usunąć [nazwa]? Ta operacja jest nieodwracalna."
- Po potwierdzeniu podróż znika z listy
- Komunikat: "Podróż została usunięta"
- Usunięcie podróży usuwa także wszystkich powiązanych współtowarzyszy

### US-007: Dodawanie współtowarzyszy
**Jako** organizator grupowego wyjazdu  
**Chcę** dodać współtowarzyszy do konkretnej podróży  
**Aby** mieć listę wszystkich uczestników w jednym miejscu

**Kryteria akceptacji:**
- Formularz w widoku szczegółów podróży
- Pola: Imię i nazwisko (wymagane), Kontakt - email lub telefon (opcjonalny)
- Po dodaniu współtowarzysz pojawia się na liście
- Wyświetlany jest komunikat sukcesu
- Licznik pokazuje aktualną liczbę współtowarzyszy

### US-008: Wyświetlanie listy współtowarzyszy
**Jako** użytkownik  
**Chcę** zobaczyć listę wszystkich osób jadących w konkretnej podróży  
**Aby** wiedzieć kto będzie uczestniczył

**Kryteria akceptacji:**
- Lista wyświetlana w widoku szczegółów podróży
- Każdy współtowarzysz pokazuje: imię, nazwisko, kontakt (jeśli podany)
- Licznik: "Współtowarzyszy: X"
- Jeśli brak współtowarzyszy: komunikat "Nie dodano jeszcze współtowarzyszy"

### US-009: Usuwanie współtowarzyszy
**Jako** organizator  
**Chcę** usunąć współtowarzyszy, którzy zrezygnowali z wyjazdu  
**Aby** lista była aktualna

**Kryteria akceptacji:**
- Przycisk "Usuń" przy każdym współtowiarzyszu
- Potwierdzenie: "Usunąć [imię] z podróży?"
- Po potwierdzeniu osoba znika z listy
- Zaktualizowana liczba współtowarzyszy

### US-010: Bezpieczny dostęp do danych
**Jako** zalogowany użytkownik  
**Chcę** mieć pewność, że moje podróże nie są dostępne dla innych użytkowników  
**Aby** zachować prywatność moich planów

**Kryteria akceptacji:**
- Tylko zalogowany użytkownik może wyświetlać, edytować i usuwać swoje podróże
- Nie ma dostępu do podróży innych użytkowników
- Dane są przechowywane zgodnie z RODO
- Możliwość usunięcia konta wraz ze wszystkimi danymi na życzenie

## 6. Metryki sukcesu

### 6.1 Metryki funkcjonalne (zachowanie użytkowników)
- **Time to First Trip:** < 3 minuty od logowania do utworzenia pierwszej podróży
- **Trip Creation Success Rate:** > 90% użytkowników pomyślnie tworzy podróż
- **Companion Addition Rate:** > 70% podróży ma dodanych współtowarzyszy
- **Return Visit Rate (7 dni):** > 40% użytkowników wraca w ciągu tygodnia
- **Trip Edit Frequency:** Średnio 2+ edycje na podróż

### 6.2 Metryki techniczne (jakość systemu)
- **Uptime:** > 99% dostępności
- **Page Load Time:** < 3s (pierwsza wizyta), < 1s (kolejne wizyty)
- **Test Pass Rate:** 100% testów e2e przechodzi w pipeline CI/CD
- **Build Success Rate:** > 95% buildów kończy się sukcesem
- **Error Rate:** < 5% operacji kończy się błędem

### 6.3 Metryki użytkownika (satysfakcja)
- **Task Completion Rate:** > 85% użytkowników kończy zadanie "utwórz podróż z 2 współtowarzyszami" w user testingu
- **User Satisfaction:** > 7/10 w ankiecie post-MVP
- **Perceived Usefulness:** > 8/10 "To rozwiązuje mój problem" w user testingu
- **Recommendation Rate:** > 60% użytkowników poleciłoby aplikację znajomym

### 6.4 Metoda zbierania danych
**Faza User Testing (przed certyfikacją):**
- 5-10 testów użytkownika ze znajomymi i społecznością 10xDevs
- Obserwacja wykonania zadań + kwestionariusz
- Zbieranie feedbacku jakościowego

**Faza MVP w produkcji (po certyfikacji):**
- Podstawowe analytics (zgodne z RODO)
- Logi aplikacji (błędy, czasy operacji)
- Opcjonalne ankiety w aplikacji

## 7. Wymagania niefunkcjonalne

### 7.1 Wydajność
- Czas ładowania strony głównej: < 3s (first load), < 1s (kolejne)
- Responsywność UI: feedback na akcje użytkownika < 200ms
- Skalowalność: aplikacja działa płynnie do 500 podróży na użytkownika

### 7.2 Bezpieczeństwo
- Hasła hashowane przez Supabase Auth (bcrypt)
- HTTPS dla wszystkich połączeń (GitHub Pages + Supabase)
- Row Level Security (RLS) w Supabase - użytkownik widzi tylko swoje dane
- Session management z automatycznym wylogowaniem po bezczynności
- Walidacja danych po stronie klienta (Blazor) i serwera (Supabase RLS policies)

### 7.3 Użyteczność (UX)
- Mobile-first: aplikacja w pełni funkcjonalna na telefonie
- Responsive: adaptacja do tablet/desktop
- Dostępność: podstawowe aria-labels, odpowiedni kontrast kolorów
- Jasne komunikaty błędów i sukcesów
- Loading states dla długotrwałych operacji

### 7.4 Zgodność prawna
- Dane osobowe przechowywane zgodnie z RODO
- Prawo do wglądu i usunięcia danych na życzenie użytkownika
- Możliwość usunięcia konta wraz z wszystkimi powiązanymi danymi

## 8. Technologia

### 8.1 Stack technologiczny
- **Frontend:** Blazor WebAssembly (standalone - bez backend .NET)
- **UI Framework:** MudBlazor
- **Backend/Baza danych:** Supabase (PostgreSQL + Auth + Storage)
- **Autoryzacja:** Supabase Auth (email/password)
- **API Client:** Supabase C# Client Library
- **CI/CD:** GitHub Actions
- **Hosting:** GitHub Pages (static files)
- **Testy:** Playwright lub bUnit (testy e2e)

### 8.2 Uzasadnienie wyboru
- **Blazor WebAssembly:** jeden język (C#), nowoczesny framework SPA, działa 100% po stronie klienta
- **MudBlazor:** gotowe komponenty, responsive, Material Design
- **Supabase:** 
  - Darmowy tier wystarczający dla MVP (500MB storage, 50K monthly active users)
  - Wbudowana autoryzacja (nie trzeba implementować własnej)
  - PostgreSQL z Row Level Security (bezpieczeństwo na poziomie bazy)
  - Real-time capabilities (na przyszłość)
  - Gotowe REST API i SDK dla C#
  - Nie wymaga własnego backendu - idealne dla GitHub Pages
- **GitHub Pages:** darmowy hosting dla static files (Blazor WASM), automatyczny deploy

## 9. Kryteria zaliczenia (10xDevs Checklist)

### 9.1 Wymagania obowiązkowe ✅
- [x] Mechanizm logowania (Supabase Auth)
- [x] Funkcja z logiką biznesową (walidacja dat + obliczanie czasu podróży)
- [x] Funkcja CRUD (CRUD Trips + CRUD Companions przez Supabase API)
- [x] Działający test (test e2e: logowanie + create trip + add companion)
- [x] CI/CD na GitHub Actions (build + testy + deploy)
- [x] Dokumentacja (PRD, README, instrukcja deployment)
- [x] Testy użytkownika (5-10 sesji user testing + feedback)

### 9.2 Wymagania opcjonalne (dla wyróżnienia) ⭐
- [x] Publiczny URL (GitHub Pages: `username.github.io/MotoNomad`)
- [ ] Instalowalna aplikacja PWA (poza zakresem MVP)
- [x] Zgłoszenie w I terminie (target: 16.11.2025)
- [x] Customowy projekt (MotoNomad - nie szablon)
- [x] 10xCards (wszystkie obowiązkowe + opcjonalne URL)

## 10. Harmonogram

### Faza 1: Setup (Tydzień 1)
- Utworzenie repozytorium GitHub
- Utworzenie projektu w Supabase (darmowy tier)
- Setup Blazor WASM + MudBlazor
- Instalacja Supabase C# Client
- CI/CD pipeline (build + deploy to GitHub Pages)
- **Milestone:** Pusta aplikacja buduje się i deployuje na GitHub Pages

### Faza 2: Autoryzacja (Tydzień 2)
- Konfiguracja Supabase Auth
- Integracja Supabase Auth SDK w Blazor
- Strony: Rejestracja, Logowanie
- Session management
- **Milestone:** Użytkownik może się zarejestrować i zalogować przez Supabase

### Faza 3: CRUD Podróży (Tydzień 3)
- Utworzenie tabel w Supabase (trips)
- Konfiguracja Row Level Security (RLS)
- Formularz tworzenia, edycji, usuwania
- Walidacja dat, lista podróży
- Integracja z Supabase API
- **Milestone:** Pełne CRUD dla podróży działa

### Faza 4: Współtowarzysze (Tydzień 4)
- Utworzenie tabeli companions w Supabase
- Konfiguracja RLS dla companions
- Dodawanie, wyświetlanie, usuwanie
- Foreign key: companion -> trip
- **Milestone:** Zarządzanie współtowarzyszami działa

### Faza 5: Testy i polish (Tydzień 5)
- Implementacja testów e2e
- User testing (5-10 osób)
- Poprawki z feedbacku
- **Milestone:** Wszystkie testy przechodzą

### Faza 6: Certyfikacja (Tydzień 6)
- Finalne sprawdzenie wymagań 10xDevs
- Dokumentacja finalna
- Zgłoszenie do programu
- **Milestone:** Projekt gotowy do certyfikacji

---

**Status dokumentu:** ✅ Ready for Certification  
**Projekt:** MotoNomad  
**Program:** 10xDevs  
**Data:** Październik 2025