using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentValidation;
using Xunit;
using ZeferiniPersonApi.Models;

namespace ZeferiniPersonApi.Tests.Models;

public class EventTests
{
    [Fact]
    public void Event_ShouldInitialize_WithDefaultValues()
    {
        // Arrange & Act
        var evt = new Event();

        // Assert
        evt.Id.Should().BeEmpty();
        evt.AggregateId.Should().BeEmpty();
        evt.AggregateType.Should().BeEmpty();
        evt.EventType.Should().BeEmpty();
        evt.EventData.Should().BeEmpty();
        evt.Metadata.Should().BeEmpty();
        evt.Version.Should().Be(0);
        evt.Timestamp.Should().Be(default);
        evt.CreatedAt.Should().Be(default);
    }

    [Fact]
    public void Event_ShouldSetProperties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var eventData = new Dictionary<string, object> { { "key", "value" } };
        var metadata = new Dictionary<string, object?> { { "source", "test" } };

        // Act
        var evt = new Event
        {
            Id = id,
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = eventData,
            Metadata = metadata,
            Version = 1,
            Timestamp = timestamp,
            CreatedAt = timestamp
        };

        // Assert
        evt.Id.Should().Be(id);
        evt.AggregateId.Should().Be("agg-1");
        evt.AggregateType.Should().Be("Person");
        evt.EventType.Should().Be("Created");
        evt.EventData.Should().BeEquivalentTo(eventData);
        evt.Metadata.Should().BeEquivalentTo(metadata);
        evt.Version.Should().Be(1);
        evt.Timestamp.Should().Be(timestamp);
        evt.CreatedAt.Should().Be(timestamp);
    }

    [Fact]
    public void ValidateAndThrow_ShouldNotThrow_WhenEventIsValid()
    {
        // Arrange
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = new Dictionary<string, object> { { "key", "value" } },
            Metadata = new Dictionary<string, object?>(),
            Version = 0,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Action act = () => evt.ValidateAndThrow();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateAndThrow_ShouldThrow_WhenAggregateIdIsEmpty()
    {
        // Arrange
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "",
            AggregateType = "Person",
            EventType = "Created",
            EventData = new Dictionary<string, object>(),
            Metadata = new Dictionary<string, object?>(),
            Version = 0,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Action act = () => evt.ValidateAndThrow();
        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.PropertyName == "AggregateId"));
    }

    [Fact]
    public void ValidateAndThrow_ShouldThrow_WhenAggregateTypeIsEmpty()
    {
        // Arrange
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "",
            EventType = "Created",
            EventData = new Dictionary<string, object>(),
            Metadata = new Dictionary<string, object?>(),
            Version = 0,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Action act = () => evt.ValidateAndThrow();
        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.PropertyName == "AggregateType"));
    }

    [Fact]
    public void ValidateAndThrow_ShouldThrow_WhenEventTypeIsEmpty()
    {
        // Arrange
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "",
            EventData = new Dictionary<string, object>(),
            Metadata = new Dictionary<string, object?>(),
            Version = 0,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Action act = () => evt.ValidateAndThrow();
        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.PropertyName == "EventType"));
    }

    [Fact]
    public void ValidateAndThrow_ShouldThrow_WhenEventDataIsNull()
    {
        // Arrange
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = null!,
            Metadata = new Dictionary<string, object?>(),
            Version = 0,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Action act = () => evt.ValidateAndThrow();
        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.PropertyName == "EventData"));
    }

    [Fact]
    public void ValidateAndThrow_ShouldThrow_WhenVersionIsNegative()
    {
        // Arrange
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = new Dictionary<string, object>(),
            Metadata = new Dictionary<string, object?>(),
            Version = -1,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Action act = () => evt.ValidateAndThrow();
        act.Should().Throw<ValidationException>()
            .Where(e => e.Errors.Any(err => err.PropertyName == "Version"));
    }

    [Fact]
    public void ValidateAndThrow_ShouldNotThrow_WhenVersionIsZero()
    {
        // Arrange
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = new Dictionary<string, object>(),
            Metadata = new Dictionary<string, object?>(),
            Version = 0,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Action act = () => evt.ValidateAndThrow();

        // Assert
        act.Should().NotThrow();
    }
}
