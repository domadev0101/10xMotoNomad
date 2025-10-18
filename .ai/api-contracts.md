# API Contracts - MotoNomad

**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025  
**Status:** Ready for Implementation

---

## 1. Overview

This document defines all Data Transfer Objects (DTOs) and Command objects used in the MotoNomad application. DTOs represent data returned from services to UI components, while Commands represent requests to perform operations. All types are immutable where possible and use record types for concise syntax and value-based equality.

---

## 2. Authentication Contracts

### 2.1 Commands

#### RegisterCommand

User registration request with email, password, and optional display name.

```csharp
namespace MotoNomad.Application.Commands.Auth;

public record RegisterCommand
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? DisplayName { get; init; }
}
```

**Validation Rules:**
- `Email`: Required, valid email format, max 255 characters
- `Password`: Required, min 8 characters, max 100 characters
- `DisplayName`: Optional, max 100 characters

#### LoginCommand

User authentication request with credentials.

```csharp
namespace MotoNomad.Application.Commands.Auth;

public record LoginCommand
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
```

**Validation Rules:**
- `Email`: Required, valid email format
- `Password`: Required, min 8 characters

### 2.2 DTOs

#### UserDto

Authenticated user information returned after login or registration.

```csharp
namespace MotoNomad.Application.DTOs.Auth;

public record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

**Field Descriptions:**
- `Id`: Unique user identifier (from auth.users)
- `Email`: User's email address
- `DisplayName`: Optional display name from profile
- `AvatarUrl`: Optional avatar image URL from profile
- `CreatedAt`: Account creation timestamp (UTC)

---

## 3. Trip Contracts

### 3.1 Commands

#### CreateTripCommand

Request to create a new trip with validation.

```csharp
namespace MotoNomad.Application.Commands.Trips;

public record CreateTripCommand
{
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
}
```

**Validation Rules:**
- `Name`: Required, max 200 characters, non-empty after trim
- `StartDate`: Required, valid date
- `EndDate`: Required, must be > StartDate
- `Description`: Optional, max 2000 characters
- `TransportType`: Required, valid enum value (0-4)

#### UpdateTripCommand

Request to update existing trip with all fields.

```csharp
namespace MotoNomad.Application.Commands.Trips;

public record UpdateTripCommand
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
}
```

**Validation Rules:**
- `Id`: Required, must exist and belong to current user
- All other fields: Same validation as CreateTripCommand

#### DeleteTripCommand

Request to delete trip (simple wrapper for clarity).

```csharp
namespace MotoNomad.Application.Commands.Trips;

public record DeleteTripCommand
{
    public required Guid Id { get; init; }
}
```

**Validation Rules:**
- `Id`: Required, must exist and belong to current user

### 3.2 DTOs

#### TripListItemDto

Lightweight trip representation for list views.

```csharp
namespace MotoNomad.Application.DTOs.Trips;

public record TripListItemDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required int DurationDays { get; init; }
    public required TransportType TransportType { get; init; }
    public required int CompanionCount { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

**Field Descriptions:**
- `Id`: Unique trip identifier
- `Name`: Trip name
- `StartDate`: Trip start date
- `EndDate`: Trip end date
- `DurationDays`: Calculated duration (EndDate - StartDate)
- `TransportType`: Mode of transportation
- `CompanionCount`: Number of companions for this trip
- `CreatedAt`: Trip creation timestamp (UTC)

#### TripDetailDto

Complete trip information including companions for detail views.

```csharp
namespace MotoNomad.Application.DTOs.Trips;

public record TripDetailDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
    public required int DurationDays { get; init; }
    public required List<CompanionListItemDto> Companions { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
```

**Field Descriptions:**
- All fields from TripListItemDto plus:
- `UserId`: Owner user ID (for authorization checks in UI)
- `Description`: Optional trip description
- `Companions`: List of all companions for this trip
- `UpdatedAt`: Last modification timestamp (UTC)

---

## 4. Companion Contracts

### 4.1 Commands

#### AddCompanionCommand

Request to add new companion to a trip.

```csharp
namespace MotoNomad.Application.Commands.Companions;

public record AddCompanionCommand
{
    public required Guid TripId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
```

**Validation Rules:**
- `TripId`: Required, must exist and belong to current user
- `FirstName`: Required, max 100 characters, non-empty after trim
- `LastName`: Required, max 100 characters, non-empty after trim
- `Contact`: Optional, max 255 characters (email or phone format recommended but not enforced)

#### UpdateCompanionCommand

Request to update existing companion information.

```csharp
namespace MotoNomad.Application.Commands.Companions;

public record UpdateCompanionCommand
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
```

**Validation Rules:**
- `Id`: Required, must exist and trip must belong to current user
- All other fields: Same validation as AddCompanionCommand

#### RemoveCompanionCommand

Request to remove companion from trip.

```csharp
namespace MotoNomad.Application.Commands.Companions;

public record RemoveCompanionCommand
{
    public required Guid Id { get; init; }
}
```

**Validation Rules:**
- `Id`: Required, must exist and trip must belong to current user

### 4.2 DTOs

#### CompanionListItemDto

Lightweight companion representation for lists.

```csharp
namespace MotoNomad.Application.DTOs.Companions;

public record CompanionListItemDto
{
    public required Guid Id { get; init; }
    public required Guid TripId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
```

**Field Descriptions:**
- `Id`: Unique companion identifier
- `TripId`: Associated trip ID
- `FirstName`: Companion's first name
- `LastName`: Companion's last name
- `Contact`: Optional contact information (email or phone)

#### CompanionDto

Complete companion information (currently identical to list item, kept separate for future extensibility).

```csharp
namespace MotoNomad.Application.DTOs.Companions;

public record CompanionDto
{
    public required Guid Id { get; init; }
    public required Guid TripId { get; init; }
    public Guid? UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

**Field Descriptions:**
- All fields from CompanionListItemDto plus:
- `UserId`: Optional link to system user (for future trip sharing)
- `CreatedAt`: Companion creation timestamp (UTC)

---

## 5. Profile Contracts

### 5.1 Commands

#### UpdateDisplayNameCommand

Request to update user's display name.

```csharp
namespace MotoNomad.Application.Commands.Profile;

public record UpdateDisplayNameCommand
{
    public required string DisplayName { get; init; }
}
```

**Validation Rules:**
- `DisplayName`: Required, max 100 characters, non-empty after trim

#### UpdateAvatarUrlCommand

Request to update user's avatar URL.

```csharp
namespace MotoNomad.Application.Commands.Profile;

public record UpdateAvatarUrlCommand
{
    public required string AvatarUrl { get; init; }
}
```

**Validation Rules:**
- `AvatarUrl`: Required, valid URL format, max 500 characters

### 5.2 DTOs

#### ProfileDto

User profile information.

```csharp
namespace MotoNomad.Application.DTOs.Profile;

public record ProfileDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
```

**Field Descriptions:**
- `Id`: User ID (same as auth.users.id)
- `Email`: User's email address
- `DisplayName`: Optional display name
- `AvatarUrl`: Optional avatar image URL
- `CreatedAt`: Profile creation timestamp (UTC)
- `UpdatedAt`: Last profile update timestamp (UTC)

---

## 6. Shared Types

### 6.1 Enums

#### TransportType

Transportation mode for trips.

```csharp
namespace MotoNomad.Infrastructure.Database.Entities;

public enum TransportType
{
    Motorcycle = 0,
    Airplane = 1,
    Train = 2,
    Car = 3,
    Other = 4
}
```

**Database Mapping:**
- Stored as INTEGER in PostgreSQL
- Values 0-4 enforced by CHECK constraint
- UI displays localized string names

---

## 7. Validation Rules Summary

### 7.1 Common Validation Patterns

**Email Validation:**
```csharp
public static bool IsValidEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email)) return false;
    
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}
```

**String Length Validation:**
- Trim all string inputs before validation
- Reject strings that are empty after trimming (for required fields)
- Enforce max length limits as specified per field

**Date Validation:**
- Ensure EndDate > StartDate for trips
- Use DateOnly type for date-only fields (no time component)
- Convert to/from PostgreSQL DATE type

**URL Validation:**
```csharp
public static bool IsValidUrl(string url)
{
    return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
}
```

### 7.2 Validation Exception Messages

Services should throw `ValidationException` with clear, user-friendly messages:

**Authentication:**
- "Email address is required"
- "Email address format is invalid"
- "Password must be at least 8 characters"
- "Display name cannot exceed 100 characters"

**Trips:**
- "Trip name is required"
- "Trip name cannot exceed 200 characters"
- "End date must be after start date"
- "Description cannot exceed 2000 characters"
- "Invalid transport type"

**Companions:**
- "First name is required"
- "Last name is required"
- "First name cannot exceed 100 characters"
- "Last name cannot exceed 100 characters"
- "Contact information cannot exceed 255 characters"

**Profile:**
- "Display name is required"
- "Display name cannot exceed 100 characters"
- "Avatar URL format is invalid"
- "Avatar URL cannot exceed 500 characters"

---

## 8. Mapping Guidelines

### 8.1 Entity to DTO Mapping

**Trip Entity → TripListItemDto:**
```csharp
public static TripListItemDto ToListItemDto(Trip entity, int companionCount)
{
    return new TripListItemDto
    {
        Id = entity.Id,
        Name = entity.Name,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        DurationDays = entity.DurationDays,
        TransportType = entity.TransportType,
        CompanionCount = companionCount,
        CreatedAt = entity.CreatedAt
    };
}
```

**Trip Entity → TripDetailDto:**
```csharp
public static TripDetailDto ToDetailDto(Trip entity, List<CompanionListItemDto> companions)
{
    return new TripDetailDto
    {
        Id = entity.Id,
        UserId = entity.UserId,
        Name = entity.Name,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Description = entity.Description,
        TransportType = entity.TransportType,
        DurationDays = entity.DurationDays,
        Companions = companions,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
```

**Companion Entity → CompanionListItemDto:**
```csharp
public static CompanionListItemDto ToListItemDto(Companion entity)
{
    return new CompanionListItemDto
    {
        Id = entity.Id,
        TripId = entity.TripId,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Contact = entity.Contact
    };
}
```

### 8.2 Command to Entity Mapping

**CreateTripCommand → Trip Entity:**
```csharp
public static Trip ToEntity(CreateTripCommand command, Guid userId)
{
    return new Trip
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = command.Name.Trim(),
        StartDate = command.StartDate,
        EndDate = command.EndDate,
        Description = command.Description?.Trim(),
        TransportType = command.TransportType,
        // DurationDays calculated by database GENERATED column
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
```

**AddCompanionCommand → Companion Entity:**
```csharp
public static Companion ToEntity(AddCompanionCommand command)
{
    return new Companion
    {
        Id = Guid.NewGuid(),
        TripId = command.TripId,
        UserId = null, // MVP doesn't link companions to users
        FirstName = command.FirstName.Trim(),
        LastName = command.LastName.Trim(),
        Contact = command.Contact?.Trim(),
        CreatedAt = DateTime.UtcNow
    };
}
```

---

## 9. JSON Serialization

### 9.1 System.Text.Json Configuration

Configure JSON serialization in `Program.cs` for Blazor WebAssembly:

```csharp
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.Converters.Add(new JsonStringEnumConverter());
    options.Converters.Add(new DateOnlyJsonConverter());
});
```

### 9.2 DateOnly JSON Converter

Custom converter for DateOnly type (required for .NET 6+):

```csharp
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.ParseExact(reader.GetString()!, DateFormat, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}
```

### 9.3 Example JSON Representations

**TripDetailDto:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "userId": "987e6543-e21b-12d3-a456-426614174111",
  "name": "Bieszczady Adventure",
  "startDate": "2025-06-15",
  "endDate": "2025-06-22",
  "description": "Summer motorcycle trip",
  "transportType": "Motorcycle",
  "durationDays": 7,
  "companions": [
    {
      "id": "456e7890-e12b-34d5-a678-426614174222",
      "tripId": "123e4567-e89b-12d3-a456-426614174000",
      "firstName": "Jan",
      "lastName": "Kowalski",
      "contact": "jan@example.com"
    }
  ],
  "createdAt": "2025-05-01T10:30:00Z",
  "updatedAt": "2025-05-15T14:20:00Z"
}
```

**CreateTripCommand:**
```json
{
  "name": "Berlin Conference",
  "startDate": "2025-09-10",
  "endDate": "2025-09-12",
  "description": "Business travel",
  "transportType": "Airplane"
}
```

---

## 10. Error Response Contracts

### 10.1 ErrorDto

Standard error response format for all exceptions.

```csharp
namespace MotoNomad.Application.DTOs.Common;

public record ErrorDto
{
    public required string Type { get; init; }
    public required string Message { get; init; }
    public string? Detail { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }
}
```

**Field Descriptions:**
- `Type`: Exception type (ValidationException, NotFoundException, etc.)
- `Message`: User-friendly error message
- `Detail`: Optional technical details (only in development)
- `ValidationErrors`: Optional field-level validation errors (field name → error messages)

### 10.2 Example Error Responses

**ValidationException:**
```json
{
  "type": "ValidationException",
  "message": "Validation failed",
  "validationErrors": {
    "Name": ["Trip name is required"],
    "EndDate": ["End date must be after start date"]
  }
}
```

**NotFoundException:**
```json
{
  "type": "NotFoundException",
  "message": "Trip not found"
}
```

**UnauthorizedException:**
```json
{
  "type": "UnauthorizedException",
  "message": "Please log in to continue"
}
```

---

## 11. Best Practices

### 11.1 DTO Design Principles

1. **Immutability**: Use `init` accessors for all properties
2. **Explicitness**: Use `required` for mandatory fields
3. **Nullability**: Be explicit with `?` for optional fields
4. **Value Semantics**: Use `record` types for value-based equality
5. **Single Responsibility**: Separate DTOs for different contexts (list vs. detail)

### 11.2 Command Design Principles

1. **Intent-Revealing**: Name commands after user actions
2. **Complete**: Include all necessary data for operation
3. **Validated**: Throw ValidationException for invalid data
4. **Idempotent**: Where possible, design for idempotency
5. **Atomic**: Each command performs single logical operation

### 11.3 Naming Conventions

**DTOs:**
- Suffix with `Dto`: `TripDto`, `UserDto`
- Use `ListItemDto` for lightweight list representations
- Use `DetailDto` for complete information views

**Commands:**
- Verb prefix + noun + `Command`: `CreateTripCommand`, `UpdateProfileCommand`
- Use business domain language, not CRUD verbs in UI-facing names

**Properties:**
- PascalCase for C# properties
- camelCase for JSON serialization (configured globally)

---

## 12. Implementation Checklist

Before implementing contracts:

- [ ] Review all DTO and Command definitions
- [ ] Ensure validation rules are clearly documented
- [ ] Confirm mapping strategies between entities and DTOs
- [ ] Set up JSON serialization configuration
- [ ] Create custom converters (DateOnly, enums)
- [ ] Define error response format
- [ ] Document example JSON payloads
- [ ] Create validation helper methods

---

**Document Status:** ✅ Ready for Implementation  
**Companion Document:** `services-plan.md`  
**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025
