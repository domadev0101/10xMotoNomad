# Product Requirements Document (PRD) – MotoNomad

## 1. Product Overview
MotoNomad is a web application for planning individual and group trips (motorcycle, airplane, train). It enables users to centrally manage all trip details: dates, routes, companions, and transportation type in one place. The application solves the information chaos problem that arises when using multiple tools (notes, calendars, Excel, messengers).

## 2. User Problem
Travelers planning group trips must juggle multiple tools simultaneously: route details in phone notes, participant list in emails/SMS, dates in Google Calendar, costs in Excel. This leads to:
- Time waste (15-30 minutes to find a single piece of information)
- Stress and uncertainty about whether everything is planned
- Lack of synchronization between companions
- Difficulty accessing information offline during the trip

The solution's goal is to reduce trip planning time from hours to a few minutes and provide a single source of truth for all trip details.

## 3. Functional Requirements

### 3.1 Authorization and User Accounts
- User registration (email + password) through Supabase Auth
- Login to application (Supabase Auth SDK)
- User session management (Supabase session management)
- Ability to delete account along with associated data (soft delete in Supabase)

### 3.2 Trip Management (CRUD)
- Creating new trip with fields:
  - Trip name (required)
  - Start and end date (required)
  - Description (optional)
  - Transportation type (dropdown: motorcycle, airplane, train, car, other)
- Editing all fields of existing trip
- Deleting trip with confirmation
- Displaying list of all user's trips (sorted by date)

### 3.3 Companion Management
- Adding companions to specific trip (first and last name + optional contact)
- Removing companions from trip
- Displaying list of companions for selected trip
- Trip participant counter

### 3.4 Business Logic
- Date validation: end date must be later than start date
- Automatic calculation of trip duration in days
- Error messages for incorrect data

### 3.5 User Interface
- Responsive design (MudBlazor) adapted for mobile and desktop
- Clear trip list view with key information
- Quick access to main functions (max 2 clicks)
- Readable success and error messages

### 3.6 Quality and Deployment
- E2E test: login + create trip + add companions
- CI/CD pipeline: GitHub Actions (build + tests + deploy)
- Hosting on GitHub Pages with public URL
- Documentation: README, deployment instructions

## 4. Product Boundaries

### 4.1 Out of MVP Scope:
- Offline mode with IndexedDB (local cache)
- Export trip plan to PDF
- AI suggestions (attractions, route recommendations)
- Detailed transportation organization (ticket, flight reservations)
- Accommodation and budget management
- Calendar/timeline trip view
- Trip sharing between users
- Cost report generation
- Notifications and reminders
- External API integrations (maps, weather)
- PWA (installable mobile app)
- Document import (PDF, DOCX)
- Public API for developers

### 4.2 Boundary Justification:
These features are valuable but not essential to solving the main problem: "information chaos when planning trips." The MVP focuses on creating a central information repository – if the basic idea doesn't work, additional features won't help either.

## 5. User Stories

### US-001: Account Registration
**As** a new user  
**I want** to register in the application  
**So that** I can save my trips and access them from different devices

**Acceptance Criteria:**
- Registration form contains fields: email and password (minimum 8 characters)
- Email is validated (correct format)
- After correct completion, account is created and user is automatically logged in
- Confirmation message is displayed for successful registration

### US-002: Application Login
**As** a registered user  
**I want** to be able to log in  
**So that** I have access to my trips

**Acceptance Criteria:**
- Login form contains fields: email and password
- After providing correct credentials, user is redirected to trip list
- Incorrect credentials display clear error message
- Session is maintained (doesn't require re-login on next visit)

### US-003: Creating New Trip
**As** a logged-in user  
**I want** to create a new trip with basic data  
**So that** I have a starting point for detailed planning

**Acceptance Criteria:**
- Form contains fields: name (required), start date (required), end date (required), description (optional), transport (dropdown)
- End date must be later than start date – otherwise validation error is displayed
- Transport selected from list: motorcycle, airplane, train, car, other
- Success message is displayed after save
- New trip appears immediately on trip list
- Entire operation takes less than 2 minutes

### US-004: Viewing Trip List
**As** a user with several planned trips  
**I want** to see all my trips on one list  
**So that** I can quickly find the trip I'm interested in

**Acceptance Criteria:**
- List shows all user's trips
- Each trip displays: name, dates, transport type, duration in days
- Trips are sorted from newest start date
- Clear indication of past vs future trips
- Clicking on trip navigates to details view

### US-005: Editing Trip
**As** a trip organizer  
**I want** to edit details of existing trip  
**So that** I can update plans as they change

**Acceptance Criteria:**
- Edit form is identical to creation form
- All fields are pre-filled with current data
- Same validation as when creating
- Success message after save and changes are immediately visible on list
- Ability to cancel edit (return without changes)

### US-006: Deleting Trip
**As** a user  
**I want** to delete a trip I'm no longer planning  
**So that** I keep the list current and clear

**Acceptance Criteria:**
- "Delete" button available in trip details view
- Confirmation dialog: "Are you sure you want to delete [name]? This operation is irreversible."
- After confirmation, trip disappears from list
- Message: "Trip has been deleted"
- Deleting trip also removes all associated companions

### US-007: Adding Companions
**As** a group trip organizer  
**I want** to add companions to specific trip  
**So that** I have a list of all participants in one place

**Acceptance Criteria:**
- Form in trip details view
- Fields: First and last name (required), Contact - email or phone (optional)
- After adding, companion appears on list
- Success message is displayed
- Counter shows current number of companions

### US-008: Displaying Companion List
**As** a user  
**I want** to see a list of all people going on specific trip  
**So that** I know who will be participating

**Acceptance Criteria:**
- List displayed in trip details view
- Each companion shows: first name, last name, contact (if provided)
- Counter: "Companions: X"
- If no companions: message "No companions added yet"

### US-009: Removing Companions
**As** an organizer  
**I want** to remove companions who resigned from the trip  
**So that** the list is current

**Acceptance Criteria:**
- "Delete" button next to each companion
- Confirmation: "Remove [name] from trip?"
- After confirmation, person disappears from list
- Updated number of companions

### US-010: Secure Data Access
**As** a logged-in user  
**I want** to be sure that my trips are not accessible to other users  
**So that** I maintain privacy of my plans

**Acceptance Criteria:**
- Only logged-in user can view, edit, and delete their trips
- No access to other users' trips
- Data is stored in compliance with GDPR
- Ability to delete account along with all data upon request

## 6. Success Metrics

### 6.1 Functional Metrics (User Behavior)
- **Time to First Trip:** < 3 minutes from login to creating first trip
- **Trip Creation Success Rate:** > 90% of users successfully create trip
- **Companion Addition Rate:** > 70% of trips have added companions
- **Return Visit Rate (7 days):** > 40% of users return within a week
- **Trip Edit Frequency:** Average 2+ edits per trip

### 6.2 Technical Metrics (System Quality)
- **Uptime:** > 99% availability
- **Page Load Time:** < 3s (first visit), < 1s (subsequent visits)
- **Test Pass Rate:** 100% of e2e tests pass in CI/CD pipeline
- **Build Success Rate:** > 95% of builds complete successfully
- **Error Rate:** < 5% of operations end in error

### 6.3 User Metrics (Satisfaction)
- **Task Completion Rate:** > 85% of users complete task "create trip with 2 companions" in user testing
- **User Satisfaction:** > 7/10 in post-MVP survey
- **Perceived Usefulness:** > 8/10 "This solves my problem" in user testing
- **Recommendation Rate:** > 60% of users would recommend the app to friends

### 6.4 Data Collection Method
**User Testing Phase (before certification):**
- 5-10 user tests with friends and 10xDevs community
- Task completion observation + questionnaire
- Qualitative feedback collection

**MVP in Production Phase (after certification):**
- Basic analytics (GDPR compliant)
- Application logs (errors, operation times)
- Optional in-app surveys

## 7. Non-Functional Requirements

### 7.1 Performance
- Home page load time: < 3s (first load), < 1s (subsequent)
- UI responsiveness: feedback on user actions < 200ms
- Scalability: application runs smoothly up to 500 trips per user

### 7.2 Security
- Passwords hashed by Supabase Auth (bcrypt)
- HTTPS for all connections (GitHub Pages + Supabase)
- Row Level Security (RLS) in Supabase - user sees only their data
- Session management with automatic logout after inactivity
- Data validation on client side (Blazor) and server (Supabase RLS policies)

### 7.3 Usability (UX)
- Mobile-first: fully functional application on phone
- Responsive: adaptation to tablet/desktop
- Accessibility: basic aria-labels, appropriate color contrast
- Clear error and success messages
- Loading states for long-running operations

### 7.4 Legal Compliance
- Personal data stored in compliance with GDPR
- Right to access and delete data upon user request
- Ability to delete account along with all associated data

## 8. Technology

### 8.1 Technology Stack
- **Frontend:** Blazor WebAssembly (standalone - no .NET backend)
- **UI Framework:** MudBlazor
- **Backend/Database:** Supabase (PostgreSQL + Auth + Storage)
- **Authorization:** Supabase Auth (email/password)
- **API Client:** Supabase C# Client Library
- **CI/CD:** GitHub Actions
- **Hosting:** GitHub Pages (static files)
- **Testing:** Playwright or bUnit (e2e tests)

### 8.2 Choice Justification
- **Blazor WebAssembly:** one language (C#), modern SPA framework, runs 100% client-side
- **MudBlazor:** ready-made components, responsive, Material Design
- **Supabase:** 
  - Free tier sufficient for MVP (500MB storage, 50K monthly active users)
  - Built-in authentication (no need to implement own)
  - PostgreSQL with Row Level Security (database-level security)
  - Real-time capabilities (for future)
  - Ready REST API and SDK for C#
  - Doesn't require own backend - ideal for GitHub Pages
- **GitHub Pages:** free hosting for static files (Blazor WASM), automatic deploy

## 9. Completion Criteria (10xDevs Checklist)

### 9.1 Mandatory Requirements ✅
- [x] Login mechanism (Supabase Auth)
- [x] Function with business logic (date validation + trip duration calculation)
- [x] CRUD function (CRUD Trips + CRUD Companions through Supabase API)
- [x] Working test (e2e test: login + create trip + add companion)
- [x] CI/CD on GitHub Actions (build + tests + deploy)
- [x] Documentation (PRD, README, deployment instructions)
- [x] User testing (5-10 user testing sessions + feedback)

### 9.2 Optional Requirements (for distinction) ⭐
- [x] Public URL (GitHub Pages: `username.github.io/MotoNomad`)
- [ ] Installable PWA app (out of MVP scope)
- [x] Submission in first deadline (target: November 16, 2025)
- [x] Custom project (MotoNomad - not template)
- [x] 10xCards (all mandatory + optional URL)

## 10. Timeline

### Phase 1: Setup (Week 1)
- Create GitHub repository
- Create Supabase project (free tier)
- Setup Blazor WASM + MudBlazor
- Install Supabase C# Client
- CI/CD pipeline (build + deploy to GitHub Pages)
- **Milestone:** Empty application builds and deploys to GitHub Pages

### Phase 2: Authorization (Week 2)
- Configure Supabase Auth
- Integrate Supabase Auth SDK in Blazor
- Pages: Registration, Login
- Session management
- **Milestone:** User can register and log in through Supabase

### Phase 3: Trip CRUD (Week 3)
- Create tables in Supabase (trips)
- Configure Row Level Security (RLS)
- Create, edit, delete forms
- Date validation, trip list
- Supabase API integration
- **Milestone:** Full CRUD for trips working

### Phase 4: Companions (Week 4)
- Create companions table in Supabase
- Configure RLS for companions
- Adding, displaying, removing
- Foreign key: companion -> trip
- **Milestone:** Companion management working

### Phase 5: Testing and Polish (Week 5)
- Implement e2e tests
- User testing (5-10 people)
- Fixes from feedback
- **Milestone:** All tests passing

### Phase 6: Certification (Week 6)
- Final check of 10xDevs requirements
- Final documentation
- Program submission
- **Milestone:** Project ready for certification

---

**Document Status:** ✅ Ready for Certification  
**Project:** MotoNomad  
**Program:** 10xDevs  
**Date:** October 2025