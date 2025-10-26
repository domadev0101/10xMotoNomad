# Test Plan for MotoNomad Project

## 1. Introduction and Testing Objectives

This document outlines the comprehensive testing strategy for the MotoNomad project, a Blazor WebAssembly Single Page Application (SPA) for planning motorcycle trips. The primary goal of this test plan is to ensure the application's quality, reliability, security, and performance before deployment.

**Key Objectives:**

*   **Verify Core Functionality:** Ensure all CRUD (Create, Read, Update, Delete) operations for trips, companions, and user profiles work as expected.
*   **Ensure Security:** Validate that the application's data isolation mechanisms, particularly Supabase's Row Level Security (RLS), are correctly implemented and prevent unauthorized data access.
*   **Validate User Authentication:** Confirm that the user registration, login, logout, and session management processes are robust and secure.
*   **Assess UI/UX:** Ensure the user interface is intuitive, responsive across various devices, and free of visual defects.
*   **Validate Integrations:** Test the reliability of integrations with external services, namely Supabase (database and auth) and OpenRouter (AI suggestions), including graceful error handling.
*   **Confirm Deployment Integrity:** Verify that the CI/CD pipeline correctly builds, tests, and deploys a production-ready application.

## 2. Scope of Testing

### In Scope

*   **Frontend Application:** The entire MotoNomad Blazor WebAssembly application (`MotoNomad.App`).
*   **Component-Level Logic:** All individual Blazor components, including forms, lists, and dialogs.
*   **Application Services:** All client-side services responsible for business logic and communication with Supabase.
*   **API Integration:** The application's interaction with the Supabase REST API (PostgREST) and Auth API (GoTrue).
*   **Security Policies:** Verification of the implemented Row Level Security policies from the user's perspective.
*   **AI Assistant:** The functionality of the AI trip suggestion feature and its integration with the OpenRouter API.
*   **Cross-Browser Compatibility:** Testing on the latest versions of major web browsers (Chrome, Firefox, Safari).

### Out of Scope

*   **Supabase Infrastructure:** The underlying infrastructure of Supabase (PostgreSQL database, PostgREST server, GoTrue server) is considered a stable, external service and will not be tested.
*   **OpenRouter AI Models:** The performance or quality of the AI models provided by OpenRouter is not in scope. Testing will focus only on the integration layer.
*   **GitHub Pages/Actions Infrastructure:** The hosting and CI/CD platforms themselves will not be tested.
*   **Third-Party Libraries:** The internal workings of libraries like MudBlazor, `supabase-csharp`, etc., are not in scope, only their integration within the application.

## 3. Types of Testing

A multi-layered testing approach will be adopted to ensure comprehensive coverage.

| Test Type                 | Description                                                                                                                                                                 | Tools              | Owner                |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------ | -------------------- |
| **Unit Testing**          | Testing individual Blazor components in isolation to verify their rendering logic and behavior based on parameters and user interactions. Focus on `.razor.cs` logic.         | bUnit              | Developer            |
| **Integration Testing**   | Testing the interaction between application services and external APIs (Supabase, OpenRouter). This will require a dedicated test environment to verify API calls and responses. | .NET Test Runner   | QA Engineer/Developer |
| **End-to-End (E2E) Testing** | Simulating full user scenarios in a real browser environment, from login to trip creation and management. Verifies the complete application flow.                           | Playwright for .NET | QA Engineer          |
| **Security Testing**      | A specialized form of E2E testing focused on verifying the Row Level Security policies. Involves using multiple user accounts to confirm data isolation.                      | Playwright for .NET | QA Engineer          |
| **UI & Usability Testing** | Manual and automated testing to check for visual consistency, responsiveness, accessibility, and overall user experience.                                                     | Playwright, Manual | QA Engineer          |
| **Smoke Testing**         | A subset of critical E2E tests run after each deployment to the production/staging environment to quickly verify that the main functionalities are working correctly.         | Playwright for .NET | CI/CD (Automated)    |

## 4. Test Scenarios for Key Functionalities

This section outlines high-level test scenarios for the application's most critical features.

### 4.1. Authentication and Authorization

| Test Case ID | Scenario                                         | Steps                                                                                                                                                                                                                                                            | Expected Result                                                                                                                                          | Priority |
| :----------- | :----------------------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------- | :------- |
| **TC-AUTH-01** | Successful User Registration & Login             | 1. Navigate to `/register`. 2. Fill in valid email, password, and optional display name. 3. Submit the form. 4. Verify redirection to `/trips`. 5. Logout. 6. Navigate to `/login` and sign in with the new credentials.                                           | User is successfully registered, logged in, and redirected. The display name is shown correctly.                                                         | Highest  |
| **TC-AUTH-02** | Registration with an Existing Email              | 1. Attempt to register with an email that is already in use.                                                                                                                                                                                                     | An appropriate error message ("This email is already registered.") is displayed. The user is not registered.                                             | Highest  |
| **TC-AUTH-03** | Login with Invalid Credentials                   | 1. Navigate to `/login`. 2. Enter a valid email but an incorrect password. 3. Submit the form.                                                                                                                                                                   | An error message ("Invalid email or password.") is displayed. The user is not logged in.                                                                 | Highest  |
| **TC-AUTH-04** | Accessing a Protected Route while Unauthorized   | 1. Ensure the user is logged out. 2. Attempt to navigate directly to `/trips` or `/profile`.                                                                                                                                                                    | The user is redirected to the `/login` page.                                                                                                             | Highest  |

### 4.2. Data Security (Row Level Security)

| Test Case ID | Scenario                   | Steps                                                                                                                                                                                            | Expected Result                                                                                                | Priority |
| :----------- | :------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------- | :------- |
| **TC-SEC-01**  | Data Isolation Between Users | 1. Register and log in as `User A`. 2. Create a new trip named "Trip A" and note its ID. 3. Logout. 4. Register and log in as `User B`. 5. Attempt to navigate directly to the URL for "Trip A". | `User B` is shown a "Trip not found" error or is redirected to their own trip list. `User B` cannot see or edit "Trip A". | Highest  |

### 4.3. Trip Management (CRUD)

| Test Case ID | Scenario                             | Steps                                                                                                                                                                                              | Expected Result                                                                                                                             | Priority |
| :----------- | :----------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------ | :------- |
| **TC-TRIP-01** | Create a New Trip with Valid Data      | 1. Login. 2. Navigate to `/trip/create`. 3. Fill in all required fields (Name, Start Date, End Date, Transport Type). 4. Submit the form.                                                           | The trip is created successfully. The user is redirected to the `/trips` list, and the new trip appears in the "Upcoming" tab.            | High     |
| **TC-TRIP-02** | Create a Trip with Invalid Data      | 1. Login. 2. Navigate to `/trip/create`. 3. Attempt to submit the form with an end date that is before the start date.                                                                            | Form validation prevents submission, and an error message ("End date must be after start date") is displayed.                                | High     |
| **TC-TRIP-03** | Edit an Existing Trip                  | 1. Login. 2. Navigate to the details page of an existing trip. 3. Change the trip's name and description. 4. Save the changes.                                                                     | The trip data is updated successfully. A success notification is shown. The updated data persists after a page refresh.                  | High     |
| **TC-TRIP-04** | Delete a Trip                        | 1. Login. 2. Navigate to the details page of a trip. 3. Click the delete icon. 4. Confirm the deletion in the dialog.                                                                               | The trip is permanently deleted. The user is redirected to the `/trips` list, and the trip no longer appears.                             | High     |

### 4.4. Companion Management

| Test Case ID | Scenario                              | Steps                                                                                                                                                                                                       | Expected Result                                                                                                                                     | Priority |
| :----------- | :------------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------- | :------- |
| **TC-COMP-01** | Add and Remove a Companion              | 1. Login. 2. Navigate to a trip's detail page and go to the "Companions" tab. 3. Add a new companion with a first name and last name. 4. Verify the companion appears in the list. 5. Remove the companion. | The companion is successfully added and then removed from the list. The companion count on the tab updates accordingly.                                | Medium   |

### 4.5. AI Assistant

| Test Case ID | Scenario                                  | Steps                                                                                                                                                                 | Expected Result                                                                                                                                          | Priority |
| :----------- | :---------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------- | :------- |
| **TC-AI-01**   | Generate and Apply AI Suggestions       | 1. Login. 2. On the trip creation/edit form, fill in the name, dates, and transport type. 3. Click "Generate AI Suggestions". 4. After suggestions appear, click "Apply to Description". | The AI Assistant generates a relevant description and highlights. The generated text is successfully appended to the trip's description field.         | Medium   |
| **TC-AI-02**   | Handle AI Service Error (No API Key)    | 1. Configure the application without an OpenRouter API key. 2. Attempt to generate AI suggestions.                                                                      | A user-friendly error message is displayed in the AI panel, indicating that the API key is not configured. The application does not crash.            | High     |

## 5. Test Environment

*   **Test Platform:** A dedicated Supabase project will be used for all automated and manual testing. This project will be seeded with test data, including at least two distinct user accounts to test RLS policies.
*   **Frontend Environment:** Tests will be executed against a locally running instance of the Blazor application or an instance deployed to a dedicated staging environment on GitHub Pages.
*   **Browsers:**
    *   Google Chrome (Latest Version)
    *   Mozilla Firefox (Latest Version)
    *   Apple Safari (Latest Version)

## 6. Testing Tools

*   **Component Testing:** bUnit
*   **End-to-End & Security Testing:** Playwright for .NET
*   **Test Management & Execution:** .NET CLI (`dotnet test`)
*   **Source Control & CI/CD:** Git, GitHub, GitHub Actions
*   **Issue Tracking:** GitHub Issues

## 7. Test Schedule

Testing will be conducted in parallel with development sprints.

*   **Sprint-level Testing:** Developers are responsible for writing unit tests for their features. QA will develop E2E tests for completed user stories within the sprint.
*   **Regression Testing:** A full suite of automated E2E tests will be run before each release or merge to the `main` branch.
*   **Smoke Testing:** An automated smoke test suite will be triggered by the GitHub Actions pipeline after every successful deployment.

## 8. Test Acceptance Criteria

A feature or release is considered ready for deployment when the following criteria are met:

*   **Test Case Execution:** 100% of planned test cases have been executed.
*   **Pass Rate:** 100% of "Highest" and "High" priority test cases are passing.
*   **Defect Status:**
    *   No open "Blocker" or "Critical" severity bugs.
    *   All "Major" severity bugs have a resolution plan.
*   **Code Coverage:** Unit test code coverage meets the project's defined threshold (e.g., >70%).
*   **Automated Tests:** The CI/CD pipeline, including all automated tests (unit, E2E), passes successfully.

## 9. Roles and Responsibilities

*   **Developers:**
    *   Writing and maintaining unit tests (bUnit) for their code.
    *   Fixing bugs identified during the testing process.
    *   Ensuring the CI pipeline passes before merging code.
*   **QA Engineer:**
    *   Creating and maintaining this Test Plan.
    *   Developing and maintaining automated integration and E2E test suites (Playwright).
    *   Performing manual usability and exploratory testing.
    *   Reporting and triaging bugs.
    *   Verifying bug fixes.
*   **Project Manager/Lead:**
    *   Prioritizing bug fixes.
    *   Making the final go/no-go decision for releases based on test results.

## 10. Bug Reporting Procedure

All defects will be tracked using GitHub Issues.

*   **Creation:** Any team member can create a bug report.
*   **Template:** A standardized bug report template will be used, including:
    *   **Title:** A clear, concise summary of the issue.
    *   **Environment:** (e.g., Browser, OS, Local/Staging).
    *   **Steps to Reproduce:** Detailed, numbered steps to replicate the bug.
    *   **Expected Result:** What should have happened.
    *   **Actual Result:** What actually happened.
    *   **Screenshots/Logs:** Any relevant attachments.
*   **Labels:** Issues will be labeled with `bug` and a priority label (`p1-critical`, `p2-major`, `p3-minor`).
*   **Lifecycle:** New -> Acknowledged -> In Progress -> Ready for QA -> Closed.