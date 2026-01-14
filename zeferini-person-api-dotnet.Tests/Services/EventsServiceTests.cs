using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Xunit;
using ZeferiniPersonApi.Models;
using ZeferiniPersonApi.Services;

namespace ZeferiniPersonApi.Tests.Services;

public class EventsServiceTests
{
    [Fact]
    public void ConvertConnectionString_Converts_PostgresUri_To_ConnectionString()
    {
        // Arrange
        var uri = "postgresql://user:pass@localhost:5432/dbname";

        // Act
        var result = typeof(EventsService)
            .GetMethod("ConvertConnectionString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { uri }) as string;

        // Assert
        result.Should().Be("Host=localhost;Port=5432;Database=dbname;Username=user;Password=pass");
    }

    
    [Fact]
    public void EventPayload_Properties_Are_Required()
    {
        // Arrange & Act
        var payload = new EventPayload
        {
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = new Dictionary<string, object>()
        };

        // Assert
        payload.AggregateId.Should().Be("agg-1");
        payload.AggregateType.Should().Be("Person");
        payload.EventType.Should().Be("Created");
        payload.EventData.Should().NotBeNull();
    }    
}