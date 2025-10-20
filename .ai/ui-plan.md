# UI Architecture for MotoNomad

## 1. UI Structure Overview

MotoNomad is a Blazor WebAssembly application with a full interface based on MudBlazor components. The UI architecture adopts a **mobile-first** approach, ensuring responsiveness and full functionality on mobile devices, tablets, and desktops.

### Main architectural principles:

- **Framework**: Blazor WebAssembly (standalone, no backend server)
- **UI Library**: MudBlazor (Material Design)
- **Routing**: AuthorizeRouteView with route protection for authenticated users
- **Authentication State**: Cascading state service managed globally
- **Responsiveness**: Adaptive layout (MudDrawer pinned on desktop, collapsible on mobile)
- **User Feedback**: Immediate (< 200ms) through MudSnackbar, MudAlert, loading states
- **Validation**: Client-side (MudForm) before sending to API
- **Security**: AuthorizeRouteView, RLS handling by catching NotFoundException, auto-logout after 15 min inactivity

### Component structure:

```
MotoNomad.App/
├── Pages/
│   ├── Login.razor                    # Login
│   ├── Register.razor                 # Registration
│   ├── Trips/
│   │   ├── TripList.razor            # Trip list (main page)
│   │   ├── CreateTrip.razor          # Create trip
│   │   └── TripDetails.razor         # Details/edit + companions
├── Layout/
│   ├── MainLayout.razor              # Main layout (MudLayout)
│   ├── NavMenu.razor                 # Navigation menu (MudDrawer)
│   └── LoginDisplay.razor            # Login status (MudAppBar)
├── Shared/
│   ├── Components/
│   │   ├── TripForm.razor           # Trip form (create + edit)
│   │   ├── TripListItem.razor       # Trip card
│   │   ├── CompanionForm.razor      # Companion form
│   │   ├── CompanionList.razor      # Companion list
│   │   ├── EmptyState.razor         # Empty state component
│   │   └── LoadingSpinner.razor     # Loading component
│   └── Dialogs/
│       ├── DeleteTripConfirmationDialog.razor      # Delete trip dialog
│       └── DeleteCompanionConfirmationDialog.razor # Delete companion dialog
└── App.razor                         # Routing and CascadingAuthenticationState
```

---

## 2. View List

### 2.1 `/login` - Login Page

**Main goal**: Authenticate existing user

**Key information to display**:
- Login form (email, password)
- Authentication error messages
- Link to registration page

**Key view components**:
- `MudCard` - form container
- `MudTextField` - email and password fields (type="password")
- `MudButton` - "Login" button with `MudProgressCircular` during loading
- `MudAlert` (Severity.Error) - `AuthException` error messages (e.g. "Invalid credentials")
- `MudLink` - link to `/register`

**API Integration**:
- `IAuthService.LoginAsync(LoginCommand)` → `UserDto`
- On success: Redirect to `/trips`
- Errors: `AuthException` displayed as `MudAlert`

**UX, accessibility and security**:
- **UX**: Autofocus on email field, email format validation, disabled button during loading
- **Accessibility**: Aria-labels, keyboard navigation (Tab, Enter), semantic HTML tags
- **Security**: "password" type for password, HTTPS communication, no password storage in browser

---

### 2.2 `/register` - Registration Page

**Main goal**: Create new user account

**Key information to display**:
- Registration form (email, password, optional displayName)
- Error messages (email taken, weak password)
- Link to login page

**Key view components**:
- `MudCard` - form container
- `MudTextField` - email, password (min 8 chars), displayName (optional) fields
- `MudButton` - "Register" button with `MudProgressCircular`
- `MudAlert` (Severity.Error) - `AuthException` error messages
- `MudAlert` (Severity.Warning) - inline validation errors (e.g. email format, password length)
- `MudLink` - link to `/login`

**API Integration**:
- `IAuthService.RegisterAsync(RegisterCommand)` → `UserDto`
- On success: Auto login and redirect to `/trips`
- Errors: `ValidationException` (inline at fields), `AuthException` (MudAlert)

**UX, accessibility and security**:
- **UX**: Real-time validation, password strength indicator (optional), success message
- **Accessibility**: Aria-labels, autofocus, keyboard navigation
- **Security**: Password hashing by Supabase Auth, HTTPS, client and server-side validation

---

### 2.3 `/trips` - Trip List (main page)

**Main goal**: Overview of all user trips with quick access to details

**Key information to display**:
- Trip list divided into tabs: "Upcoming" (default) and "Past"
- For each trip: name, dates (dd.MM.yyyy - dd.MM.yyyy), duration, transport type (icon), companion count
- "New Trip" button
- Empty state (no trips) with message and action button

**Key view components**:
- `MudTabs` - "Upcoming" and "Past" tabs
- `MudButton` (Floating Action Button) - "New Trip" → `/trip/create`
- List of `TripListItem.razor` components (rendered as `MudCard`)
  - Transport icon (motorcycle 🏍️, plane ✈️, train 🚂, car 🚗, other 🌍)
  - Header: Trip name
  - Dates and duration: "06/15/2025 - 06/22/2025 (7 days)"
  - Companion count: Icon 👥 + count
  - Card click → `/trip/{id}`
- `MudSkeleton` - loading state (card placeholder)
- `EmptyState.razor` - "No trips" message with icon and "Add your first trip" button

**API Integration**:
- "Upcoming" tab: `ITripService.GetUpcomingTripsAsync()` → `IEnumerable<TripListItemDto>`
- "Past" tab: `ITripService.GetPastTripsAsync()` → `IEnumerable<TripListItemDto>`
- Data in `TripListItemDto`: Id, Name, StartDate, EndDate, DurationDays, TransportType, CompanionCount, CreatedAt

**UX, accessibility and security**:
- **UX**: Default "Upcoming" tab, sorting (upcoming from nearest, past from newest), fast loading (< 1s), refresh when returning from details (OnInitializedAsync)
- **Accessibility**: Semantic headers, keyboard navigation for tabs and cards, alt-text for icons
- **Security**: Display only logged-in user's trips (RLS on backend)

---

### 2.4 `/trip/create` - Create New Trip

**Main goal**: Create new trip with basic data (< 2 minutes according to US-003)

**Key information to display**:
- Trip form (name, dates, description, transport)
- Validation error messages
- Action buttons (Save, Cancel)

**Key view components**:
- `MudCard` - form container
- `TripForm.razor` (reusable component) containing:
  - `MudTextField` - Trip name (required, max 200 chars)
  - `MudDatePicker` - Start date (required, format dd.MM.yyyy)
  - `MudDatePicker` - End date (required, format dd.MM.yyyy, validation: > StartDate)
  - `MudTextField` - Description (optional, multiline, max 2000 chars)
  - `MudSelect<TransportType>` - Transport type (required, dropdown: Motorcycle, Plane, Train, Car, Other)
- `MudButton` (Primary) - "Save" with `MudProgressCircular` during loading
- `MudButton` (Secondary) - "Cancel" → return to `/trips`
- Inline validation - `ValidationException` errors displayed under form fields

**API Integration**:
- Submit: `ITripService.CreateTripAsync(CreateTripCommand)` → `TripDetailDto`
- On success: `MudSnackbar(Severity.Success, "Trip has been created")` + redirect to `/trips`
- Errors: `ValidationException` (inline), `DatabaseException` (MudSnackbar Error)

**UX, accessibility and security**:
- **UX**: Validation before submit, error message "End date must be later than start date", disabled button during loading, trim strings
- **Accessibility**: Autofocus on name field, keyboard navigation, aria-labels
- **Security**: Client and server-side validation, max length enforcement, trim whitespace

---

### 2.5 `/trip/{id}` - Trip Details/Edit + Companion Management

**Main goal**: Manage all trip aspects (edit details, add/remove companions, delete trip)

**Key information to display**:
- "Details" tab: Trip edit form + delete button
- "Companions[X]" tab: Companion list + add form (hidden by default)
- Companion counter in tab label

**Key view components**:

#### "Details" Tab:
- `MudTabs` - tab system
- `TripForm.razor` (in edit mode, filled with `TripDetailDto` data)
- `MudButton` (Primary) - "Save Changes" with `MudProgressCircular`
- `MudIconButton` (Danger) - Trash icon "Delete Trip" → calls `DeleteTripConfirmationDialog`
- After save: `MudSnackbar(Severity.Success)` + stay on `/trip/{id}` page + data refresh

#### "Companions" Tab:
- `MudButton` - "Add Companion" (toggles form visibility)
- `CompanionForm.razor` (hidden by default):
  - `MudTextField` - First name (required, max 100 chars)
  - `MudTextField` - Last name (required, max 100 chars)
  - `MudTextField` - Contact (optional, max 255 chars, email or phone)
  - `MudButton` - "Save", "Cancel"
- `CompanionList.razor` - companion list as `MudList` (not MudTable for RWD):
  - `MudListItem` for each companion:
    - First and last name (main text)
    - Contact (secondary text, if exists)
    - `MudIconButton` (Danger) - Trash icon "Delete" → calls `DeleteCompanionConfirmationDialog`
- `EmptyState.razor` - "No companions" message + "Add your first companion" button
- `MudSkeleton` - list loading state
- Companion counter in tab label: "Companions (3)"

**API Integration**:
- **Parallel loading** (Task.WhenAll):
  - `ITripService.GetTripByIdAsync(id)` → `TripDetailDto`
  - `ICompanionService.GetCompanionsByTripIdAsync(id)` → `IEnumerable<CompanionListItemDto>`
- **Edit trip**: `ITripService.UpdateTripAsync(UpdateTripCommand)` → `TripDetailDto`
- **Delete trip**: `ITripService.DeleteTripAsync(id)` → redirect to `/trips`
- **Add companion**: `ICompanionService.AddCompanionAsync(AddCompanionCommand)` → `CompanionDto` → refresh list
- **Delete companion**: `ICompanionService.RemoveCompanionAsync(companionId)` → refresh list
- **Errors**: 
  - `NotFoundException` (attempt to access someone else's trip) → redirect to `/trips`
  - `ValidationException` → inline at fields
  - `DatabaseException` → MudSnackbar Error

**UX, accessibility and security**:
- **UX**: 
  - Global loader during loading (Task.WhenAll)
  - Companion form hidden by default (better mobile UX)
  - After adding companion: form hidden, list refreshed, success Snackbar
  - After editing trip: stay on page, data refreshed
  - After deleting trip: redirect to `/trips`
  - After deleting companion: list refreshed, counter updated
- **Accessibility**: Keyboard navigation for tabs, aria-labels, focus on form fields
- **Security**: 
  - RLS (backend) blocks access to others' trips
  - UI catches `NotFoundException` and redirects to `/trips`
  - Validation before submit

---

### 2.6 System Dialogs (MudDialog)

#### DeleteTripConfirmationDialog.razor

**Main goal**: Confirm trip deletion (US-006)

**Key information**:
- Title: "Delete trip?"
- Content: "Are you sure you want to delete trip '[trip name]'? This action cannot be undone."
- Buttons: "Cancel" (Secondary), "Delete" (Danger/Error)

**Components**:
- `MudDialog`
- `MudDialogContent`
- `MudDialogActions`

**UX**: Simple dialog without requiring name input (according to session notes decision)

#### DeleteCompanionConfirmationDialog.razor

**Main goal**: Confirm companion deletion (US-009)

**Key information**:
- Title: "Delete companion?"
- Content: "Are you sure you want to remove [first name] [last name] from the trip?"
- Buttons: "Cancel" (Secondary), "Delete" (Danger/Error)

**Components**:
- `MudDialog`
- `MudDialogContent`
- `MudDialogActions`

---

## 3. User Journey Map

### 3.1 Main Flow: Create Trip with Companions (US-003, US-007)

```
1. START → User opens application
   ↓
2. /login → Login form
   ↓ (IAuthService.LoginAsync)
   ↓
3. /trips → Trip list ("Upcoming" tab default)
   ↓ (click "New Trip")
   ↓
4. /trip/create → TripForm.razor form
   ↓ (fill: name, dates, transport)
   ↓ (validation: EndDate > StartDate)
   ↓ (click "Save")
   ↓ (ITripService.CreateTripAsync)
   ↓
5. MudSnackbar(Success: "Trip has been created")
   ↓ (redirect)
   ↓
6. /trips → New trip visible on list
   ↓ (click trip card)
   ↓
7. /trip/{id} → "Details" tab (default)
   ↓ (go to "Companions" tab)
   ↓
8. "Companions" tab → Empty state: "No companions"
   ↓ (click "Add companion")
   ↓
9. CompanionForm.razor → Form visible
   ↓ (fill: first name, last name, contact)
   ↓ (click "Save")
   ↓ (ICompanionService.AddCompanionAsync)
   ↓
10. MudSnackbar(Success: "Companion has been added")
    ↓
11. CompanionForm hidden, CompanionList refreshed
    ↓
    Counter in tab: "Companions (1)"
    ↓
12. END → User can add more companions or return to list
```

**Completion time**: < 3 minutes (according to PRD success metric)

---

### 3.2 Flow: Edit Trip (US-005)

```
1. /trips → Trip list
   ↓ (click trip card)
   ↓
2. /trip/{id} → "Details" tab (default)
   ↓ (ITripService.GetTripByIdAsync - TripForm filled with data)
   ↓
3. Edit form fields (e.g. change dates, description)
   ↓ (real-time validation)
   ↓ (click "Save Changes")
   ↓ (ITripService.UpdateTripAsync)
   ↓
4. MudSnackbar(Success: "Changes have been saved")
   ↓
5. Stay on /trip/{id}, data refreshed
   ↓
6. END → User can go to "Companions" tab or return to list
```

---

### 3.3 Flow: Delete Trip (US-006)

```
1. /trip/{id} → "Details" tab
   ↓ (click trash icon "Delete Trip")
   ↓
2. DeleteTripConfirmationDialog → MudDialog
   Message: "Are you sure you want to delete '[name]'? This action cannot be undone."
   ↓ (click "Delete")
   ↓ (ITripService.DeleteTripAsync)
   ↓
3. MudSnackbar(Success: "Trip has been deleted")
   ↓ (redirect)
   ↓
4. /trips → Trip disappeared from list
   ↓
5. END
```

**Note**: Deleting trip automatically deletes all companions (CASCADE in database)

---

### 3.4 Flow: Delete Companion (US-009)

```
1. /trip/{id} → "Companions" tab
   ↓ (CompanionList.razor displays list)
   ↓ (click trash icon next to companion)
   ↓
2. DeleteCompanionConfirmationDialog → MudDialog
   Message: "Are you sure you want to remove [first name] [last name] from the trip?"
   ↓ (click "Delete")
   ↓ (ICompanionService.RemoveCompanionAsync)
   ↓
3. MudSnackbar(Success: "Companion has been removed")
   ↓
4. CompanionList refreshed, counter updated
   ↓
5. END → User stays in "Companions" tab
```

---

### 3.5 Flow: RLS Error Handling (US-010)

```
1. User manually types /trip/{another_user_id} in address bar
   ↓ (ITripService.GetTripByIdAsync)
   ↓ (RLS in Supabase blocks query)
   ↓
2. Backend returns no data → NotFoundException
   ↓ (component catches exception in try-catch)
   ↓
3. Redirect to /trips
   ↓ (optionally: MudSnackbar(Error, "Trip not found"))
   ↓
4. END → User sees only their own trips
```

---

### 3.6 Flow: Auto-logout After Inactivity

```
1. User logged in, browsing /trips or /trip/{id}
   ↓
2. Timer in MainLayout.razor: No activity for 15 minutes
   ↓
3. Timer triggers IAuthService.LogoutAsync()
   ↓
4. MudSnackbar(Warning: "Session expired. Please log in again.")
   ↓ (redirect)
   ↓
5. /login
   ↓
6. END → User must log in again
```

---

## 4. Layout and Navigation Structure

### 4.1 Main Layout (MainLayout.razor)

```
┌─────────────────────────────────────────────────────┐
│              MudAppBar (Top)                        │
│  [Logo] MotoNomad     [LoginDisplay] [Logout]      │
├────────┬────────────────────────────────────────────┤
│        │                                            │
│  Mud   │                                            │
│  Draw  │         Main Content (@Body)               │
│  er    │                                            │
│        │         - /login                           │
│  Nav   │         - /register                        │
│  Menu  │         - /trips (default after login)     │
│        │         - /trip/create                     │
│  (Side)│         - /trip/{id}                       │
│        │                                            │
│        │                                            │
│        │                                            │
└────────┴────────────────────────────────────────────┘
```

**Layout components**:
- `MudLayout` - main container
- `MudAppBar` - header (logo, LoginDisplay, logout button)
- `MudDrawer` - side menu (NavMenu.razor)
  - **Desktop**: Drawer pinned (`Variant="DrawerVariant.Persistent"`), always visible
  - **Mobile**: Drawer hidden (`Breakpoint="Breakpoint.Md"`), expandable by hamburger button in AppBar
- `@Body` - main page content

---

### 4.2 Navigation Menu (NavMenu.razor)

**Menu items for authenticated users** (MudNavLink):

1. **My Trips** → `/trips`
   - Icon: 🗺️ (MudIcon Name="Icons.Material.Filled.Map")
   - Default page after login
   - Auto-highlighting active route (Match="NavLinkMatch.All")

2. **New Trip** → `/trip/create`
   - Icon: ➕ (MudIcon Name="Icons.Material.Filled.Add")
   - Quick access to create trip

3. **Logout** → `IAuthService.LogoutAsync()` + redirect to `/login`
   - Icon: 🚪 (MudIcon Name="Icons.Material.Filled.Logout")
   - Click calls service method and redirects

**Menu items for non-authenticated users**:
- **Login** → `/login`
- **Register** → `/register`

**Notes**:
- Menu reacts to authentication state (CascadingAuthenticationState)
- `MudNavLink` automatically highlights active route
- On mobile menu is hidden, expandable by button in AppBar

---

### 4.3 Routing and Route Protection (App.razor)

**Public routes** (available for non-authenticated):
- `/login` - Login
- `/register` - Registration

**Protected routes** (require authorization):
- `/trips` - Trip list (default after login)
- `/trip/create` - Create trip
- `/trip/{id}` - Trip details/edit

**Protection implementation**:
```csharp
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated != true)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
                        <p>You don't have permission to access this page.</p>
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <p>Page not found.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

**Default redirects**:
- Non-authenticated trying to access protected route → `/login`
- Authenticated entering `/` → `/trips`

---

### 4.4 Navigation Between Views (summary)

**Main navigation flows**:

```
/login (non-authenticated)
  ↓ [Login]
  ↓
/trips (authenticated, default)
  ↓ [Click "New Trip"]
  ↓
/trip/create
  ↓ [Save]
  ↓
/trips (redirect after success)
  ↓ [Click trip card]
  ↓
/trip/{id} ("Details" tab)
  ├─ [Save Changes] → stay on /trip/{id}
  ├─ [Delete] → redirect to /trips
  └─ ["Companions" tab] → stay on /trip/{id}
```

**Navigation principles**:
- Max 2 clicks to main functions (according to PRD)
- `/trips` as central hub
- Breadcrumbs NOT used (unnecessary in flat structure)
- Browser "Back" button works correctly (Blazor routing)

---

## 5. Key Components

### 5.1 TripForm.razor (reusable trip form)

**Goal**: Common form for creating (`/trip/create`) and editing (`/trip/{id}`) trip

**Parameters**:
- `[Parameter] TripDetailDto? Trip { get; set; }` - Trip data (null for creating)
- `[Parameter] EventCallback OnSubmit { get; set; }` - Callback after save
- `[Parameter] EventCallback OnCancel { get; set; }` - Callback after cancel

**Components**:
- `MudForm` (with reference `@ref="form"`)
- `MudTextField` - Name (Validation: Required, MaxLength(200))
- `MudDatePicker` - Start date (Validation: Required, Format: dd.MM.yyyy)
- `MudDatePicker` - End date (Validation: Required, > StartDate)
- `MudTextField` - Description (Multiline, MaxLength(2000), Optional)
- `MudSelect<TransportType>` - Transport type (Dropdown, Required)
- `MudButton` - "Save" (Primary, calls OnSubmit)
- `MudButton` - "Cancel" (Secondary, calls OnCancel)

**Validation**:
- MudForm validation before submit
- Inline error messages under fields
- "Save" button disabled if form invalid

**UX**:
- In edit mode: Form filled with data from `Trip` parameter
- In create mode: Empty fields
- Trim all strings before submit

---

### 5.2 TripListItem.razor (trip card on list)

**Goal**: Visual representation of trip on `/trips` list

**Parameters**:
- `[Parameter] TripListItemDto Trip { get; set; }`
- `[Parameter] EventCallback<Guid> OnClick { get; set; }`

**Components**:
- `MudCard` (clickable, calls OnClick with Trip.Id)
- `MudCardHeader` - Transport icon + Trip name
- `MudCardContent`:
  - Dates: "06/15/2025 - 06/22/2025"
  - Duration: "(7 days)"
  - Companion count: "👥 3 companions"
- Hover effect (MudPaper Elevation)

**Transport icons**:
- Motorcycle: 🏍️ (Icons.Material.Filled.TwoWheeler)
- Plane: ✈️ (Icons.Material.Filled.Flight)
- Train: 🚂 (Icons.Material.Filled.Train)
- Car: 🚗 (Icons.Material.Filled.DirectionsCar)
- Other: 🌍 (Icons.Material.Filled.TravelExplore)

---

### 5.3 CompanionList.razor (companion list)

**Goal**: Display companion list in "Companions" tab

**Parameters**:
- `[Parameter] List<CompanionListItemDto> Companions { get; set; }`
- `[Parameter] EventCallback<Guid> OnRemove { get; set; }`

**Components**:
- `MudList` (not MudTable for RWD)
- `MudListItem` for each companion:
  - Main text: "[First Name] [Last Name]"
  - Secondary text: "[Contact]" (if exists)
  - `MudIconButton` - Trash icon (Danger), calls OnRemove with CompanionId
- `EmptyState.razor` - if list empty
- `MudSkeleton` - loading state

**UX**:
- Responsive list (MudList better than MudTable on mobile)
- Delete icon visible on hover (desktop) or always (mobile)

---

### 5.4 CompanionForm.razor (companion form)

**Goal**: Add new companion

**Parameters**:
- `[Parameter] Guid TripId { get; set; }`
- `[Parameter] EventCallback OnSubmit { get; set; }`
- `[Parameter] EventCallback OnCancel { get; set; }`

**Components**:
- `MudForm`
- `MudTextField` - First name (Validation: Required, MaxLength(100))
- `MudTextField` - Last name (Validation: Required, MaxLength(100))
- `MudTextField` - Contact (Optional, MaxLength(255))
- `MudButton` - "Save" (Primary)
- `MudButton` - "Cancel" (Secondary)

**UX**:
- Hidden by default, shown by "Add Companion" button
- After save: Form hidden, fields cleared

---

### 5.5 EmptyState.razor (empty state component)

**Goal**: Friendly communication of no data

**Parameters**:
- `[Parameter] string Title { get; set; }` - Title (e.g. "No trips")
- `[Parameter] string Message { get; set; }` - Message (e.g. "Start planning your first adventure!")
- `[Parameter] string IconName { get; set; }` - MudIcon icon
- `[Parameter] string? ButtonText { get; set; }` - Button text (optional)
- `[Parameter] EventCallback OnButtonClick { get; set; }` - Button callback

**Components**:
- `MudPaper` (elevation=0, centered text)
- `MudIcon` - Large icon (Size.Large)
- `MudText` (Typo.h5) - Title
- `MudText` (Typo.body1) - Message
- `MudButton` (optional) - Action button

**Usage examples**:
- `/trips` - "No trips" + "Add your first trip" button
- `/trip/{id}` "Companions" tab - "No companions" + "Add your first companion" button

---

### 5.6 LoadingSpinner.razor (loading component)

**Goal**: Universal loading spinner

**Parameters**:
- `[Parameter] string? Message { get; set; }` - Optional text (e.g. "Loading trips...")

**Components**:
- `MudProgressCircular` (Indeterminate, Color.Primary)
- `MudText` (Typo.body2) - Message (optional)

**Usage**:
- Global loader for `/trip/{id}` page during Task.WhenAll
- Button loading states (built into MudButton)

---

### 5.7 LoginDisplay.razor (login status in AppBar)

**Goal**: Display logged-in user information in MudAppBar

**Parameters**:
- `[CascadingParameter] Task<AuthenticationState> AuthenticationStateTask { get; set; }`

**Components**:
- `AuthorizeView`:
  - **Authorized**: 
    - `MudText` - "Hello, [DisplayName or Email]!"
    - `MudIconButton` - Logout (Icons.Material.Filled.Logout)
  - **NotAuthorized**:
    - `MudButton` - "Login" → `/login`
    - `MudButton` - "Register" → `/register`

**UX**:
- On mobile: Icons instead of full buttons (to save space)
- On desktop: Full buttons with text

---

## 6. Loading States and Error Handling

### 6.1 Loading States

**Lists (e.g. `/trips`, companion list)**:
- `MudSkeleton` - card placeholder during loading
- Number of skeletons: 3-5 for better perception

**Action buttons**:
- `MudButton` with `Loading="@isLoading"`
- `MudProgressCircular` inside button
- Button disabled during operation

**Details page (`/trip/{id}`)**:
- Task.WhenAll for parallel loading Trip + Companions
- One global `LoadingSpinner.razor` for entire page
- After loading: Display tabs

---

### 6.2 Empty States

**No trips (`/trips`)**:
- `EmptyState.razor`:
  - Icon: 🗺️
  - Title: "No trips"
  - Message: "Start planning your first adventure!"
  - Button: "Add your first trip" → `/trip/create`

**No companions (`/trip/{id}` "Companions" tab)**:
- `EmptyState.razor`:
  - Icon: 👥
  - Title: "No companions"
  - Message: "Add people who will accompany you on the journey"
  - Button: "Add your first companion" → show form

---

### 6.3 Error Handling

**ValidationException (validation errors)**:
- Display inline under form fields (MudForm)
- Examples: "End date must be later than start date", "Field required"

**AuthException (authentication errors)**:
- `MudAlert(Severity.Error)` above `/login` or `/register` form
- Examples: "Invalid credentials", "Email already taken", "Password too weak"

**NotFoundException (resource not found)**:
- Catch in component try-catch
- Redirect to `/trips`
- Optionally: `MudSnackbar(Severity.Warning, "Trip not found")`

**DatabaseException (database errors)**:
- `MudSnackbar(Severity.Error, "An error occurred. Please try again.")`
- Error logging to console (ILogger)

**UnauthorizedException (no authorization)**:
- Redirect to `/login`
- `MudSnackbar(Severity.Warning, "Session expired. Please log in again.")`

---

## 7. Responsiveness and Accessibility

### 7.1 Responsiveness (mobile-first)

**MudBlazor Breakpoints**:
- `xs` (< 600px) - Phones
- `sm` (600px - 960px) - Small tablets
- `md` (960px - 1280px) - Tablets
- `lg` (1280px - 1920px) - Desktop
- `xl` (> 1920px) - Large monitors

**Adaptive elements**:
- **MudDrawer**: 
  - Desktop (md+): Drawer pinned (Variant.Persistent)
  - Mobile (xs-sm): Drawer hidden (Breakpoint.Md), expandable by button
- **Forms**: 
  - MudTextField 100% width on mobile
  - On desktop: Max width 600px, centered
- **Lists**:
  - `/trips`: Trip cards one below another on mobile, 2-3 column grid on desktop
  - Companions: MudList (not MudTable) for better mobile responsiveness
- **Buttons**:
  - Floating Action Button for "New Trip" on mobile
  - Full button in AppBar on desktop

---

### 7.2 Accessibility (WCAG 2.1)

**Keyboard navigation**:
- All interactive elements accessible via Tab
- Enter on trip cards → go to `/trip/{id}`
- Escape closes dialogs
- Focus states visible (outline)

**Screen readers**:
- Aria-labels for all icons and buttons without text
- Semantic HTML tags (nav, main, article)
- MudBlazor automatically adds aria-\* attributes

**Color contrast**:
- Min 4.5:1 for text (WCAG AA)
- Min 3:1 for UI components
- MudBlazor Theme ensures proper contrasts

**Forms**:
- Label for each field (MudTextField automatically)
- Error messages linked to fields (aria-describedby)
- Autofocus on first field (only where sensible)

---

## 8. UI Security

### 8.1 Route Protection

**AuthorizeRouteView**:
- Blocks access to `/trips`, `/trip/create`, `/trip/{id}` for non-authenticated
- Redirect to `/login`

**RLS (Row Level Security) in Supabase**:
- Backend automatically filters queries by `auth.uid()`
- UI catches `NotFoundException` when trying to access someone else's trip
- Redirect to `/trips`

---

### 8.2 Inactivity Timer

**Implementation in MainLayout.razor**:
- `System.Timers.Timer` with 15 minute interval
- Timer reset on every user interaction (click, scroll)
- After timer triggers:
  1. `IAuthService.LogoutAsync()`
  2. `MudSnackbar(Severity.Warning, "Session expired. Please log in again.")`
  3. Redirect to `/login`

---

### 8.3 Validation and Sanitization

**Client-side validation**:
- MudForm with validation rules before submit
- Prevent sending invalid data

**Sanitization**:
- Trim all strings before sending
- Enforcement max length (200 chars name, 2000 description)
- Blazor automatically escapes HTML (XSS prevention)

**HTTPS**:
- GitHub Pages automatically enforces HTTPS
- Supabase communication only through HTTPS

---

## 9. User Stories → UI Mapping Summary

| User Story | View | Key Components | API |
|------------|------|----------------|-----|
| US-001: Registration | `/register` | MudCard, MudTextField, MudButton, MudAlert | IAuthService.RegisterAsync |
| US-002: Login | `/login` | MudCard, MudTextField, MudButton, MudAlert | IAuthService.LoginAsync |
| US-003: Create Trip | `/trip/create` | TripForm.razor, MudButton | ITripService.CreateTripAsync |
| US-004: Trip List | `/trips` | MudTabs, TripListItem.razor, EmptyState | ITripService.GetUpcomingTripsAsync, GetPastTripsAsync |
| US-005: Edit Trip | `/trip/{id}` ("Details" tab) | TripForm.razor (edit mode) | ITripService.GetTripByIdAsync, UpdateTripAsync |
| US-006: Delete Trip | `/trip/{id}` + Dialog | DeleteTripConfirmationDialog | ITripService.DeleteTripAsync |
| US-007: Add Companions | `/trip/{id}` ("Companions" tab) | CompanionForm.razor, CompanionList | ICompanionService.AddCompanionAsync |
| US-008: Companion List | `/trip/{id}` ("Companions" tab) | CompanionList.razor, EmptyState | ICompanionService.GetCompanionsByTripIdAsync |
| US-009: Delete Companion | `/trip/{id}` + Dialog | CompanionList + DeleteCompanionConfirmationDialog | ICompanionService.RemoveCompanionAsync |
| US-010: Secure Access | All views | AuthorizeRouteView, try-catch NotFoundException | RLS in Supabase |

**All User Stories from PRD are fully covered by UI architecture.**

---

**Document ready for implementation** ✅  
**Project**: MotoNomad MVP  
**Program**: 10xDevs  
**Date**: October 2025  
**Certification deadline**: November 2025