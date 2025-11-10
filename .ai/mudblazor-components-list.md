# MudBlazor Components List in MotoNomad

**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025  
**Status:** Ready for Implementation

---

## Overview
This document contains a complete list of all MudBlazor components used in the MotoNomad MVP application, grouped by functional categories.

---

## 1. Layout & Navigation Components

### MudLayout
- **Usage:** `MainLayout.razor`
- **Description:** Main application layout container
- **Configuration:** Standard

### MudAppBar
- **Usage:** `MainLayout.razor`
- **Description:** Top application navigation bar
- **Configuration:** 
  - `Elevation="1"`
  - `Dense="true"`

### MudDrawer
- **Usage:** `MainLayout.razor`
- **Description:** Side navigation menu
- **Configuration:**
  - `@bind-Open="_drawerOpen"`
  - `Elevation="1"`
  - `Breakpoint="Breakpoint.Md"`
  - `Variant="@(_drawerOpen ? DrawerVariant.Persistent : DrawerVariant.Temporary)"`

### MudMainContent
- **Usage:** `MainLayout.razor`
- **Description:** Main application content area

### MudNavMenu
- **Usage:** `NavMenu.razor`
- **Description:** Navigation menu container

### MudNavLink
- **Usage:** `NavMenu.razor`
- **Description:** Navigation link in menu
- **Configuration:**
  - `Href="/trips"`, `Href="/trip/create"`, `Href="/login"`, `Href="/register"`
  - `Icon="@Icons.Material.Filled.*"`
  - `Match="NavLinkMatch.All"` (for precise matching)

---

## 2. Typography & Text Components

### MudText
- **Usage:** All views
- **Description:** Universal text component
- **Typo Variants:**
  - `Typo.h4` - Page headers (TripList: "My Trips")
  - `Typo.h5` - Titles in EmptyState
  - `Typo.h6` - Card headers (TripListItem: trip name, Login/Register: titles)
  - `Typo.body1` - Main content (dialogs, messages)
  - `Typo.body2` - Secondary content (dates, descriptions, LoginDisplay)
  - `Typo.caption` - Small information (optional hints)
- **Colors:**
  - `Color.Secondary` - Helper texts
  - `Color.Default` - Standard text

---

## 3. Form Components

### MudForm
- **Usage:** Login, Register, TripForm, CompanionForm
- **Description:** Form container with built-in validation
- **Configuration:**
  - `@ref="form"`
  - `@bind-IsValid="formValid"`

### MudTextField
- **Usage:** All forms
- **Description:** Text field
- **Configuration:**
  - `@bind-Value="model.Property"`
  - `Label="Label"`
  - `Required="true/false"`
  - `MaxLength="N"`
  - `HelperText="Hint"`
  - `InputType="InputType.Password"` (for passwords)
  - `Lines="3"` (for textarea - trip description)
  - `Validation="@CustomValidator"` (custom validation)
  - `Disabled="@IsLoading"`

### MudDatePicker
- **Usage:** TripForm (StartDate, EndDate)
- **Description:** Date picker
- **Configuration:**
  - `@bind-Date="model.Date"`
  - `Label="Start Date"`
  - `DateFormat="dd.MM.yyyy"`
  - `Required="true"`
  - `Validation="@ValidateEndDate"` (custom for EndDate)

### MudSelect<TValue>
- **Usage:** TripForm (TransportType)
- **Description:** Dropdown list
- **Configuration:**
  - `@bind-Value="model.TransportType"`
  - `Label="Transport Type"`
  - `Required="true"`

### MudSelectItem
- **Usage:** TripForm (transport options)
- **Description:** Dropdown list item
- **Configuration:**
  - `Value="@TransportType.Motorcycle"` with emoji (???, ??, ??, ??, ???)

---

## 4. Button Components

### MudButton
- **Usage:** All views
- **Description:** Universal button
- **Variants:**
  - `Variant.Filled` - Primary actions (Login, Register, Save)
  - `Variant.Text` - Helper actions (Cancel)
  - `Variant.Outlined` - (optional)
- **Colors:**
  - `Color.Primary` - Primary actions
  - `Color.Error` - Delete (in dialogs)
  - `Color.Default` - Cancel, back
  - `Color.Inherit` - LoginDisplay (buttons in AppBar)
- **Configuration:**
  - `ButtonType="ButtonType.Submit"` (forms)
  - `Disabled="@(!formValid || IsLoading)"`
  - `OnClick="@HandleMethod"`
  - `Href="/path"` (navigation without onClick)

### MudIconButton
- **Usage:** MainLayout, LoginDisplay, CompanionList, TripDetails
- **Description:** Icon button (no text)
- **Configuration:**
  - `Icon="@Icons.Material.Filled.Menu"` (toggle drawer)
  - `Icon="@Icons.Material.Filled.Logout"` (logout in AppBar)
  - `Icon="@Icons.Material.Filled.Delete"` (delete companion)
  - `Icon="@Icons.Material.Filled.Edit"` (edit - optional)
  - `Color="Color.Error"` (for Delete)
  - `Size="Size.Small"` (in lists)
  - `Edge="Edge.Start"` (in AppBar)
  - `Title="Hint"` (tooltip)

### MudFab (Floating Action Button)
- **Usage:** TripList
- **Description:** Round button in bottom right corner
- **Configuration:**
  - `Color="Color.Primary"`
  - `StartIcon="@Icons.Material.Filled.Add"`
  - `OnClick="@(() => NavigationManager.NavigateTo("/trip/create"))"`
  - `Style="position: fixed; bottom: 20px; right: 20px;"`

---

## 5. Card Components

### MudCard
- **Usage:** Login, Register, TripListItem
- **Description:** Material Design card container
- **Configuration:**
  - `Elevation="5"` (Login/Register forms)
  - `OnClick="@(() => OnTripClick.InvokeAsync(Trip.Id))"` (TripListItem)
  - `Style="cursor: pointer;"` (TripListItem)

### MudCardHeader
- **Usage:** Login, Register, TripListItem
- **Description:** Card header

### MudCardContent
- **Usage:** Login, Register, TripListItem, TripForm
- **Description:** Card content

### MudCardActions
- **Usage:** Login, Register, TripForm, dialogs
- **Description:** Button section in card
- **Configuration:**
  - `Class="justify-center"` (for Login/Register links)
  - `Class="justify-end"` (for action buttons)

---

## 6. Container & Grid Components

### MudContainer
- **Usage:** All views
- **Description:** Responsive container
- **Configuration:**
  - `MaxWidth.Small` - Login, Register (forms)
  - `MaxWidth.Large` - TripList (main page)
  - `MaxWidth.ExtraLarge` - MainLayout (@Body)
  - `Class="mt-4 mb-4"` (margins)
  - `Class="mt-8 text-center"` (404, Unauthorized)

### MudGrid
- **Usage:** TripList
- **Description:** Responsive column grid
- **Configuration:** Standard (12-column system)

### MudItem
- **Usage:** TripList (trip cards)
- **Description:** Grid item
- **Configuration:**
  - `xs="12"` - 1 column on mobile
  - `sm="6"` - 2 columns on tablet
  - `md="4"` - 3 columns on desktop

---

## 7. Tab Components

### MudTabs
- **Usage:** TripList, TripDetails
- **Description:** Tab system
- **Configuration:**
  - `@bind-ActivePanelIndex="activeTabIndex"`
  - TripList: "Upcoming" / "Archived"
  - TripDetails: "Details" / "Companions"

### MudTabPanel
- **Usage:** TripList, TripDetails
- **Description:** Tab content
- **Configuration:**
  - `Text="Upcoming"`, `Text="Archived"`, etc.

---

## 8. Dialog Components

### MudDialog
- **Usage:** DeleteTripConfirmationDialog, DeleteCompanionConfirmationDialog
- **Description:** Dialog container
- **Configuration:** Standard

### DialogContent
- **Usage:** Confirmation dialogs
- **Description:** Dialog content

### DialogActions
- **Usage:** Confirmation dialogs
- **Description:** Dialog button section

### IDialogService
- **Usage:** TripDetails
- **Description:** Service for opening dialogs
- **Method:** `DialogService.ShowAsync<DialogComponent>(title, parameters, options)`

---

## 9. Feedback Components

### MudAlert
- **Usage:** Login, Register, TripDetails, DeleteTripConfirmationDialog
- **Description:** Alert message
- **Severity:**
  - `Severity.Error` - Errors (invalid login credentials, API errors)
  - `Severity.Warning` - Warnings (irreversible operations in dialogs)
  - `Severity.Success` - Success (optional, usually we use Snackbar)
  - `Severity.Info` - Information
- **Configuration:**
  - `Dense="true"` (in dialogs)
  - Conditionally: `@if (errorMessage != null)`

### ISnackbar
- **Usage:** All views
- **Description:** Service for displaying Toast notifications
- **Methods:**
  - `Snackbar.Add("Message", Severity.Success)`
  - `Snackbar.Add("Error", Severity.Error)`
  - `Snackbar.Add("Warning", Severity.Warning)`
- **Examples:**
  - Success: "Logged in successfully!", "Trip has been created!"
  - Error: "Failed to load trips", "An unexpected error occurred"
  - Warning: "Session expired due to inactivity"

### MudProgressCircular
- **Usage:** LoadingSpinner, buttons with loading
- **Description:** Circular loading spinner
- **Configuration:**
  - `Indeterminate="true"`
  - `Size="Size.Large"` (LoadingSpinner)
  - `Size="Size.Small"` (form buttons)
  - `Color="Color.Primary"`

---

## 10. List Components

### MudList
- **Usage:** CompanionList
- **Description:** Material Design list

### MudListItem
- **Usage:** CompanionList (each companion)
- **Description:** List item
- **Content:** First name, last name, contact (optional), Delete button

---

## 11. Chip Components

### MudChip
- **Usage:** TripListItem (companion count)
- **Description:** Chip/badge label
- **Configuration:**
  - `Size="Size.Small"`
  - `Icon="@Icons.Material.Filled.People"`
  - Text: "3 companions" / "1 companion" / "No companions"

---

## 12. Icon Components

### MudIcon
- **Usage:** Everywhere (navigation, cards, buttons, EmptyState)
- **Description:** Material Design icon
- **Icons Used:**
  - **Navigation:**
    - `Icons.Material.Filled.Map` - My Trips
    - `Icons.Material.Filled.Add` - New Trip
    - `Icons.Material.Filled.Login` - Login
    - `Icons.Material.Filled.Logout` - Logout
    - `Icons.Material.Filled.PersonAdd` - Register
  - **Transport:**
    - `Icons.Material.Filled.TwoWheeler` - Motorcycle ???
    - `Icons.Material.Filled.Flight` - Airplane ??
    - `Icons.Material.Filled.Train` - Train ??
    - `Icons.Material.Filled.DirectionsCar` - Car ??
    - `Icons.Material.Filled.TravelExplore` - Other ???
  - **Actions:**
    - `Icons.Material.Filled.Delete` - Delete
    - `Icons.Material.Filled.Edit` - Edit
    - `Icons.Material.Filled.People` - Companions
    - `Icons.Material.Filled.Menu` - Menu (drawer toggle)
    - `Icons.Material.Filled.Search` - Search (404)
  - **EmptyState:**
    - `Icons.Material.Filled.Map` - No trips
    - `Icons.Material.Filled.History` - No archived
    - `Icons.Material.Filled.Info` - Information
- **Configuration:**
  - `Size="Size.Large"` (EmptyState, transport icons in cards)
  - `Size="Size.Small"` (inline in text)
  - `Color="Color.Primary"` / `Color.Secondary"` / `Color.Error"`

---

## 13. Divider Component

### MudDivider
- **Usage:** NavMenu (section separation)
- **Description:** Separator line
- **Configuration:**
  - `Class="mb-4"` (margin)
  - `Class="my-2"` (between menu items)

---

## 14. Paper Component

### MudPaper
- **Usage:** EmptyState
- **Description:** Container with shadow (Material Design)
- **Configuration:**
  - `Elevation="0"` (EmptyState - no shadow)
  - `Class="pa-8 text-center"` (padding, centering)

---

## 15. Spacer Component

### MudSpacer
- **Usage:** MainLayout (AppBar)
- **Description:** Flexible space (pushes elements to ends)
- **Configuration:** Standard (between logo and LoginDisplay)

---

## 16. Link Component

### MudLink
- **Usage:** Login, Register
- **Description:** Link with Material Design styling
- **Configuration:**
  - `Href="/login"`, `Href="/register"`
  - Text: "Don't have an account? Register", "Already have an account? Login"

---

## 17. Authorization Components (Blazor + MudBlazor)

### AuthorizeView
- **Usage:** NavMenu, LoginDisplay, App.razor
- **Description:** Conditional rendering based on authentication state
- **Sections:**
  - `<Authorized>` - Logged in user
  - `<NotAuthorized>` - Not logged in user

### AuthorizeRouteView
- **Usage:** App.razor
- **Description:** Routing with authorization
- **Configuration:**
  - `RouteData="@routeData"`
  - `DefaultLayout="@typeof(MainLayout)"`
  - `<NotAuthorized>` ? RedirectToLogin

### CascadingAuthenticationState
- **Usage:** App.razor
- **Description:** Propagates authentication state to all components

---

## 18. Custom Components (Reusable)

### EmptyState.razor
- **Description:** Empty state with icon, title, description and button
- **Parameters:**
  - `string Title`
  - `string Message`
  - `string IconName`
  - `string? ButtonText`
  - `EventCallback OnButtonClick`

### LoadingSpinner.razor
- **Description:** Loading spinner with optional message
- **Parameters:**
  - `string? Message`
  - `Size Size` (default: Size.Large)

### TripListItem.razor
- **Description:** Trip card in list
- **Parameters:**
  - `TripListItemDto Trip`
  - `EventCallback<Guid> OnTripClick`

### CompanionList.razor
- **Description:** Companion list with Delete button
- **Parameters:**
  - `List<CompanionListItemDto> Companions`
  - `EventCallback<Guid> OnRemove`

### CompanionForm.razor
- **Description:** Add companion form
- **Parameters:**
  - `Guid TripId`
  - `EventCallback<AddCompanionCommand> OnSubmit`
  - `EventCallback OnCancel`
  - `bool IsLoading`

### TripForm.razor
- **Description:** Trip form (create/edit)
- **Parameters:**
  - `TripDetailDto? Trip` (null = create, not null = edit)
  - `EventCallback<object> OnSubmit` (CreateTripCommand or UpdateTripCommand)
  - `EventCallback OnCancel`
  - `bool IsLoading`
  - `bool ShowButtons` (default: true)

---

## Summary Statistics

**Total unique MudBlazor components:** 35+

**Most frequently used:**
1. MudText (everywhere)
2. MudButton (all forms, actions)
3. MudTextField (forms)
4. MudIcon (navigation, cards, EmptyState)
5. MudCard (forms, trip cards)
6. MudContainer (layout of all pages)

**Critical components for MVP:**
- MudForm + MudTextField + MudButton (Login/Register/Trip/Companion forms)
- MudCard + MudCardContent (card UI)
- MudTabs + MudTabPanel (TripList, TripDetails)
- MudDialog + IDialogService (delete confirmations)
- ISnackbar (user feedback)
- MudLayout + MudAppBar + MudDrawer (application structure)

**Optional components (nice to have):**
- MudFab (can be replaced with regular MudButton)
- MudChip (can be replaced with MudText)
- MudDivider (can be replaced with CSS border)

---

**Document ready for implementation** ✅  
**Project**: MotoNomad MVP  
**Program**: 10xDevs  
**Date**: October 2025  
**Certification deadline**: November 2025
