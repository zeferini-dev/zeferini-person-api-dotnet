using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using  FluentAssertions;
using Xunit;
using ZeferiniPersonApi.Models;
using ZeferiniPersonApi.Services;

namespace ZeferiniPersonApi.Tests.Services;

public class EventsServiceTests_Simplified
{
    [Fact]
    public void EventPayload_ShouldSetProperties_Correctly()
    {
        // Arrange & Act
        var payload = new EventPayload
        {
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = new Dictionary<string, object> { { "key", "value" } },
            Metadata = new Dictionary<string, object?> { { "source", "test" } }
        };

        // Assert
        payload.AggregateId.Should().Be("agg-1");
        payload.AggregateType.Should().Be("Person");
        payload.EventType.Should().Be("Created");
        payload.EventData.Should().HaveCount(1);
        payload.Metadata.Should().HaveCount(1);
    }

    [Fact]
    public void EventPayload_ShouldHave_Required_Properties()
    {
        // Arrange
        var payload = new EventPayload
        {
            AggregateId = "test",
            AggregateType = "test",
            EventType = "test",
            EventData = new Dictionary<string, object>()
        };

        // Act & Assert
        payload.AggregateId.Should().NotBeNull();
        payload.AggregateType.Should().NotBeNull();
        payload.EventType.Should().NotBeNull();
        payload.EventData.Should().NotBeNull();
    }

    [Fact]
    public void EventPayload_Metadata_Can_BeNull()
    {
        // Arrange & Act
        var payload = new EventPayload
        {
            AggregateId = "test",
            AggregateType = "test",
            EventType = "test",
            EventData = new Dictionary<string, object>(),
            Metadata = null
        };

        // Assert
        payload.Metadata.Should().BeNull();
    }
}
