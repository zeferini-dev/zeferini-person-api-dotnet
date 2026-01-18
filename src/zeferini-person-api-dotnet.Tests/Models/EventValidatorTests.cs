using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using ZeferiniPersonApi.Models;

namespace ZeferiniPersonApi.Tests.Models;

public class EventValidatorTests
{
    private readonly EventValidator _validator = new();

    [Fact]
    public void Should_Be_Valid_When_All_Properties_Are_Valid()
    {
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

        var result = _validator.Validate(evt);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Should_Have_Error_When_AggregateId_Is_Empty()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "",
            AggregateType = "Person",
            EventType = "Created",
            EventData = new Dictionary<string, object>(),
            Metadata = new Dictionary<string, object?>(),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var result = _validator.Validate(evt);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AggregateId");
    }

    [Fact]
    public void Should_Have_Error_When_AggregateType_Is_Empty()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "",
            EventType = "Created",
            EventData = new Dictionary<string, object>(),
            Metadata = new Dictionary<string, object?>(),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var result = _validator.Validate(evt);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AggregateType");
    }

    [Fact]
    public void Should_Have_Error_When_EventType_Is_Empty()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "",
            EventData = new Dictionary<string, object>(),
            Metadata = new Dictionary<string, object?>(),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var result = _validator.Validate(evt);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EventType");
    }

    [Fact]
    public void Should_Have_Error_When_EventData_Is_Null()
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = null!,
            Metadata = new Dictionary<string, object?>(),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var result = _validator.Validate(evt);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EventData");
    }

    [Fact]
    public void Should_Have_Error_When_Version_Is_Negative()
    {
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

        var result = _validator.Validate(evt);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Version");
    }
}