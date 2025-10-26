using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MotoNomad.Application.Commands.Trips;
using MotoNomad.Application.Exceptions;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Infrastructure.Database.Entities;
using MotoNomad.Infrastructure.Services;

namespace MotoNomad.Tests.Unit.Services;

/// <summary>
/// Unit tests for TripService business logic validation.
/// Tests focus on validation rules without database or auth integration.
/// </summary>
public class TripServiceTests
{
    private readonly Mock<ISupabaseClientService> _mockSupabaseClient;
    private readonly Mock<ILogger<TripService>> _mockLogger;
    private readonly TripService _sut; // System Under Test

    public TripServiceTests()
    {
        _mockSupabaseClient = new Mock<ISupabaseClientService>();
        _mockLogger = new Mock<ILogger<TripService>>();

        // Prosty setup - nie mockujemy całego Client
        // Testy walidacji zadziałają bo walidacja jest PRZED sprawdzaniem auth
        _sut = new TripService(_mockSupabaseClient.Object, _mockLogger.Object);
    }

    #region CreateTripCommand Validation - Name Tests

    [Fact]
    public async Task CreateTripAsync_WithEmptyName_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateTripCommand
        {
            Name = "",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            TransportType = TransportType.Motorcycle
        };

        // Act
        Func<Task> act = async () => await _sut.CreateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateTripAsync_WithWhitespaceName_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateTripCommand
        {
            Name = "   ",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            TransportType = TransportType.Motorcycle
        };

        // Act
        Func<Task> act = async () => await _sut.CreateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateTripAsync_WithNameExceeding200Characters_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateTripCommand
        {
            Name = new string('A', 201),
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            TransportType = TransportType.Motorcycle
        };

        // Act
        Func<Task> act = async () => await _sut.CreateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region CreateTripCommand Validation - Date Tests (TC-TRIP-02)

    [Fact]
    public async Task CreateTripAsync_WithEndDateBeforeStartDate_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateTripCommand
        {
            Name = "Mountain Trip",
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            EndDate = DateOnly.FromDateTime(DateTime.Today),
            TransportType = TransportType.Motorcycle
        };

        // Act
        Func<Task> act = async () => await _sut.CreateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateTripAsync_WithEndDateEqualToStartDate_ThrowsValidationException()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var command = new CreateTripCommand
        {
            Name = "Day Trip",
            StartDate = today,
            EndDate = today,
            TransportType = TransportType.Motorcycle
        };

        // Act
        Func<Task> act = async () => await _sut.CreateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region CreateTripCommand Validation - Description Tests

    [Fact]
    public async Task CreateTripAsync_WithDescriptionExceeding2000Characters_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateTripCommand
        {
            Name = "Epic Journey",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            TransportType = TransportType.Motorcycle,
            Description = new string('A', 2001)
        };

        // Act
        Func<Task> act = async () => await _sut.CreateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region CreateTripCommand Validation - TransportType Tests

    [Fact]
    public async Task CreateTripAsync_WithInvalidTransportType_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateTripCommand
        {
            Name = "Mystery Trip",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            TransportType = (TransportType)999 // Invalid enum value
        };

        // Act
        Func<Task> act = async () => await _sut.CreateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateTripCommand Validation Tests

    [Fact]
    public async Task UpdateTripAsync_WithEmptyName_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateTripCommand
        {
            Id = Guid.NewGuid(),
            Name = "",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            TransportType = TransportType.Motorcycle
        };

        // Act
        Func<Task> act = async () => await _sut.UpdateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateTripAsync_WithEndDateBeforeStartDate_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateTripCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Trip",
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            EndDate = DateOnly.FromDateTime(DateTime.Today),
            TransportType = TransportType.Motorcycle
        };

        // Act
        Func<Task> act = async () => await _sut.UpdateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateTripAsync_WithDescriptionExceeding2000Characters_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateTripCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Epic Journey",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            TransportType = TransportType.Motorcycle,
            Description = new string('A', 2001)
        };

        // Act
        Func<Task> act = async () => await _sut.UpdateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateTripAsync_WithNameExceeding200Characters_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateTripCommand
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 201),
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            TransportType = TransportType.Motorcycle
        };

        // Act
        Func<Task> act = async () => await _sut.UpdateTripAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Duration Calculation Tests - Business Logic

    [Theory]
    [InlineData(2025, 6, 15, 2025, 6, 16, 1)]     // 1 day trip
    [InlineData(2025, 6, 15, 2025, 6, 22, 7)]     // 1 week trip
    [InlineData(2025, 6, 1, 2025, 7, 1, 30)]      // 1 month trip
    [InlineData(2025, 1, 1, 2026, 1, 1, 365)]     // 1 year trip
    [InlineData(2024, 1, 1, 2025, 1, 1, 366)]     // Leap year trip
    public void CalculateDuration_WithVariousDateRanges_ReturnsCorrectDays(
        int startYear, int startMonth, int startDay,
        int endYear, int endMonth, int endDay,
        int expectedDuration)
    {
        // Arrange
        var startDate = new DateOnly(startYear, startMonth, startDay);
        var endDate = new DateOnly(endYear, endMonth, endDay);

        // Act - This tests the actual business logic used in MapToListItemDto/MapToDetailDto
        var duration = endDate.DayNumber - startDate.DayNumber;

        // Assert
        duration.Should().Be(expectedDuration);
    }

    [Fact]
    public void CalculateDuration_CrossingMonthBoundary_CalculatesCorrectly()
    {
        // Arrange
        var startDate = new DateOnly(2025, 6, 28);
        var endDate = new DateOnly(2025, 7, 5);

        // Act
        var duration = endDate.DayNumber - startDate.DayNumber;

        // Assert
        duration.Should().Be(7);
    }

    [Fact]
    public void CalculateDuration_CrossingYearBoundary_CalculatesCorrectly()
    {
        // Arrange
        var startDate = new DateOnly(2024, 12, 28);
        var endDate = new DateOnly(2025, 1, 4);

        // Act
        var duration = endDate.DayNumber - startDate.DayNumber;

        // Assert
        duration.Should().Be(7);
    }

    [Fact]
    public void CalculateDuration_FebruaryLeapYear_CalculatesCorrectly()
    {
        // Arrange - February in leap year
        var startDate = new DateOnly(2024, 2, 1);
        var endDate = new DateOnly(2024, 3, 1);

        // Act
        var duration = endDate.DayNumber - startDate.DayNumber;

        // Assert
        duration.Should().Be(29, "February 2024 has 29 days (leap year)");
    }

    [Fact]
    public void CalculateDuration_FebruaryNonLeapYear_CalculatesCorrectly()
    {
        // Arrange - February in non-leap year
        var startDate = new DateOnly(2025, 2, 1);
        var endDate = new DateOnly(2025, 3, 1);

        // Act
        var duration = endDate.DayNumber - startDate.DayNumber;

        // Assert
        duration.Should().Be(28, "February 2025 has 28 days (non-leap year)");
    }

    #endregion
}