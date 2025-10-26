# Kitchen Sink - Documentation

## Overview

**Kitchen Sink** is a demonstration page showcasing all MudBlazor components used in the MotoNomad MVP application. It serves as:

1. **Component Catalog** - visual presentation of all UI components used
2. **Visual Documentation** - shows how components look and behave
3. **Testing Tool** - enables quick verification of styling and responsiveness
4. **Developer Reference** - usage examples for each component

---

## Page Access

**URL:** `/kitchen-sink`

**Availability:** Public (accessible to all users)

**Navigation:** 
- Direct link: `{localhost:port}/kitchen-sink`
- Can be added to NavMenu during development (remove before production)

---

## Page Structure

The Kitchen Sink page is divided into 17 sections, each presenting a different category of components:

### 1. Layout & Navigation
- `MudNavMenu`
- `MudNavLink`
- Navigation examples (My Trips, New Trip, Login)

### 2. Typography - MudText
- All Typo variants (h3, h4, h5, h6, body1, body2, caption)
- Text colors (Default, Secondary)
- Application usage examples

### 3. Buttons
- `MudButton` - all variants (Filled, Text, Outlined)
- `MudButton` - all colors (Primary, Error, Default)
- `MudButton` - disabled state
- `MudIconButton` - action icons (Menu, Logout, Delete, Edit)
- `MudFab` - Floating Action Button

### 4. Form Components
- `MudForm` - form container
- `MudTextField` - standard, password, multiline
- `MudDatePicker` - date picker with formatting
- `MudSelect` + `MudSelectItem` - dropdown list (transport)

### 5. Cards
- `MudCard` - trip card (TripListItem example)
- `MudCard` - login form (Login example)
- `MudCardHeader`, `MudCardContent`, `MudCardActions`

### 6. Tabs
- `MudTabs` + `MudTabPanel`
- Tab examples: Upcoming / Archived / Details

### 7. Feedback Components
- `MudAlert` - all severity levels (Success, Error, Warning, Info)
- `MudProgressCircular` - all sizes (Large, Medium, Small)
- `MudProgressLinear` - indeterminate and with value
- `ISnackbar` - Snackbar demo button

### 8. Icons
- **Transport Icons** - TwoWheeler, Flight, Train, DirectionsCar, TravelExplore
- **Navigation Icons** - Map, Add, People, Delete, Edit, History, Login, Logout
- Icon coloring examples

### 9. List - Companions
- `MudList` + `MudListItem`
- Companion list example with Delete buttons
- Layout: First/Last Name + Contact + Actions

### 10. Chips
- `MudChip` - different sizes (Small, Default)
- `MudChip` - with icons (People)
- `MudChip` - different colors (Primary, Secondary, Success)
- Text examples ("3 companions", "1 companion", "No companions")

### 11. Empty State (Custom Component)
- Complete EmptyState example
- Icon + Title + Message + Button
- Exact implementation used in TripList

### 12. Loading Spinner (Custom Component)
- Complete LoadingSpinner example
- `MudProgressCircular` + `MudText` with message
- Exact implementation used in application

### 13. Dialog Content (Preview)
- Confirmation dialog content visualization
- `MudAlert` (Warning) + text + action buttons
- DeleteTripConfirmationDialog example

### 14. Grid System
- `MudGrid` + `MudItem`
- Breakpoint examples:
  - `xs="12"` - full width on mobile
  - `xs="12" sm="6"` - half width on tablet+
  - `xs="12" sm="6" md="4"` - 1/3 width on desktop
- Responsiveness in practice

### 15. Links & Dividers
- `MudLink` - standard, Primary, Secondary
- `MudDivider` - different margins (my-4, my-2)
- Section separator

### 16. Spacer
- `MudSpacer` - flexible space
- Layout example: Left Element <spacer> Right Element
- Usage in AppBar (logo + spacer + LoginDisplay)

### 17. Container MaxWidth
- `MaxWidth.Small` (600px) - Login, Register
- `MaxWidth.Medium` (960px) - TripDetails
- `MaxWidth.Large` (1280px) - TripList
- Different width visualizations

---

## Usage in Development

### When creating new components:

1. Open `/kitchen-sink` in browser
2. Find the component you want to use
3. Check its parameters and configuration
4. Copy sample code to your component
5. Adjust parameters to your needs

### When testing responsiveness:

1. Open `/kitchen-sink` in DevTools (F12)
2. Switch to Responsive Design Mode
3. Test different resolutions:
   - Mobile: 375px (iPhone SE)
   - Tablet: 768px (iPad)
   - Desktop: 1280px and above
4. Check how components scale

### When debugging styles:

1. Open `/kitchen-sink`
2. Use Inspect Element (F12) on problematic component
3. Compare CSS with Kitchen Sink version
4. Identify differences and fix

---

## How to Add a New Component

If you're adding a new MudBlazor component to the application:

1. Open `MotoNomad.App/Pages/Dev/KitchenSink.razor`
2. Find the appropriate section or create a new one
3. Add `MudPaper` with example:
   ```razor
   <MudPaper Class="pa-4 mb-4" Elevation="2">
       <MudText Typo="Typo.h4" Class="mb-3">18. New Component</MudText>
       
       <!-- Usage example -->
       <MudNewComponent />
   </MudPaper>
   ```
4. Add parameter descriptions in comments
5. Update `.ai/mudblazor-components-list.md`

---

## Interactive Elements

The Kitchen Sink page contains several interactive elements for testing:

### Forms
- All form fields are functional
- You can enter text, select dates, change values
- **Note:** Data is not saved (no backend)

### Buttons
- All buttons have hover effects
- `MudButton` - click shows ripple effect
- `MudIconButton` - tooltips on hover
- Snackbar button - demo notification display

### Tabs
- Tabs are fully functional
- Click switches content
- Active tab is highlighted

### Navigation
- `MudNavLink` - shows active state (underline)
- Hover effects on all links

---

## Responsiveness

The Kitchen Sink page is fully responsive:

### Mobile (< 600px)
- Single column for all cards
- Stacked buttons (vertical)
- Full width forms

### Tablet (600px - 960px)
- 2 columns for cards (MudGrid: xs="12" sm="6")
- Buttons side by side (horizontal)

### Desktop (> 960px)
- 3 columns for cards (MudGrid: xs="12" sm="6" md="4")
- Maximum container width: 1280px (MaxWidth.Large)
- All components in full glory

---

## Usage in E2E Tests

Kitchen Sink can be used in Playwright tests:

```csharp
[Test]
public async Task KitchenSink_AllComponentsRender()
{
    await Page.GotoAsync("/kitchen-sink");
    
    // Check if all sections are visible
    await Expect(Page.Locator("text=1. Layout & Navigation")).ToBeVisibleAsync();
    await Expect(Page.Locator("text=2. Typography")).ToBeVisibleAsync();
    // ... etc.
    
    // Check interactivity
    await Page.ClickAsync("button:has-text('Show Snackbar')");
    await Expect(Page.Locator("text=This is a sample Snackbar!")).ToBeVisibleAsync();
}

[Test]
public async Task KitchenSink_ResponsiveDesign()
{
    // Mobile
    await Page.SetViewportSizeAsync(new ViewportSize { Width = 375, Height = 667 });
    await Page.GotoAsync("/kitchen-sink");
    // ... assertions

    // Desktop
    await Page.SetViewportSizeAsync(new ViewportSize { Width = 1920, Height = 1080 });
    await Page.ReloadAsync();
    // ... assertions
}
```

---

## Maintenance

### When to update Kitchen Sink:

1. **Adding new MudBlazor component** - add example in appropriate section
2. **Styling changes** - update examples to reflect current styles
3. **New variants of existing components** - add examples of new variants
4. **Refactoring** - ensure examples are current after refactoring

### What should NOT be in Kitchen Sink:

1. **Business logic** - UI presentation only
2. **API calls** - no backend integration
3. **Form validation** - forms are demonstration only
4. **Routing** - links can go to `#` or be inactive

---

## Deployment

### Development
- Kitchen Sink available at `/kitchen-sink`
- Visible in menu (optional)

### Production
- **Option 1:** Hide Kitchen Sink (remove from navigation, but keep page)
- **Option 2:** Remove page completely before deployment
- **Option 3:** Restrict access to admins only (add `[Authorize(Roles = "Admin")]`)

**Recommendation for MotoNomad MVP:** Keep Kitchen Sink in production with public access as a "Design System" for users and developers. This is consistent with open-source practices.

---

## Useful Links

- [MudBlazor Documentation](https://mudblazor.com/)
- [MudBlazor Component Gallery](https://mudblazor.com/components)
- [Material Design Guidelines](https://material.io/design)

---

## FAQ

**Q: Is Kitchen Sink mandatory for MVP?**  
A: It's not mandatory, but **highly recommended**. It simplifies development and testing.

**Q: Can I use Kitchen Sink as a Design System for clients?**  
A: Yes! Kitchen Sink is excellent visual documentation for stakeholders.

**Q: How often should Kitchen Sink be updated?**  
A: With every UI change or addition of a new component.

**Q: Does Kitchen Sink affect bundle size?**  
A: No, if it's a separate page. Blazor WASM loads components lazily.

**Q: Can I add my own custom components?**  
A: Yes! Sections 11 and 12 are already custom components (EmptyState, LoadingSpinner).

