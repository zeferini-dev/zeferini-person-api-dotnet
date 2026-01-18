using Xunit;
using ZeferiniPersonApi.Models;
using System;
using System.Collections.Generic;

public class EventValidatorTests
{
    private readonly EventValidator _validator = new();

    [Fact]
    public void ValidEvent_PassesValidation()
    {
        var ev = new Event
        {
            AggregateId = "agg-1",
            AggregateType = "type",
            EventType = "created",
            EventData = new Dictionary<string, object> { { "key", "value" } },
            Version = 1
        };
        var result = _validator.Validate(ev);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AggregateId_Required()
    {
        var ev = new Event { AggregateId = "", AggregateType = "type", EventType = "created", EventData = new(), Version = 1 };
        var result = _validator.Validate(ev);
        Assert.Contains(result.Errors, e => e.PropertyName == "AggregateId");
    }

    [Fact]
    public void AggregateType_Required()
    {
        var ev = new Event { AggregateId = "agg-1", AggregateType = "", EventType = "created", EventData = new(), Version = 1 };
        var result = _validator.Validate(ev);
        Assert.Contains(result.Errors, e => e.PropertyName == "AggregateType");
    }

    [Fact]
    public void EventType_Required()
    {
        var ev = new Event { AggregateId = "agg-1", AggregateType = "type", EventType = "", EventData = new(), Version = 1 };
        var result = _validator.Validate(ev);
        Assert.Contains(result.Errors, e => e.PropertyName == "EventType");
    }

    [Fact]
    public void EventData_Required()
    {
        var ev = new Event { AggregateId = "agg-1", AggregateType = "type", EventType = "created", EventData = null, Version = 1 };
        var result = _validator.Validate(ev);
        Assert.Contains(result.Errors, e => e.PropertyName == "EventData");
    }

    [Fact]
    public void Version_Negative_Fails()
    {
        var ev = new Event { AggregateId = "agg-1", AggregateType = "type", EventType = "created", EventData = new(), Version = -1 };
        var result = _validator.Validate(ev);
        Assert.Contains(result.Errors, e => e.PropertyName == "Version");
    }
}
