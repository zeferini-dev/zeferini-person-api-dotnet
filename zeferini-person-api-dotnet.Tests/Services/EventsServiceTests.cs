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
    public async Task PublishEventAsync_Calls_SaveEventToDatabaseAsync_And_Returns_Event()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EventsService>>();
        var dataSourceMock = new Mock<NpgsqlDataSource>();
        var service = new TestableEventsService(loggerMock.Object, dataSourceMock.Object);

        var payload = new EventPayload
        {
            AggregateId = "agg-1",
            AggregateType = "Person",
            EventType = "Created",
            EventData = new Dictionary<string, object> { { "key", "value" } },
            Metadata = new Dictionary<string, object?> { { "meta", "data" } }
        };

        // Act
        var evt = await service.PublishEventAsync(payload);

        // Assert
        evt.AggregateId.Should().Be(payload.AggregateId);
        evt.AggregateType.Should().Be(payload.AggregateType);
        evt.EventType.Should().Be(payload.EventType);
        evt.EventData.Should().BeEquivalentTo(payload.EventData);
        evt.Metadata.Should().BeEquivalentTo(payload.Metadata);
        evt.Id.Should().NotBeEmpty();
        evt.Version.Should().Be(1);
        service.SaveEventCalled.Should().BeTrue();
    }

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
    public void Dispose_Calls_DataSource_Dispose()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EventsService>>();
        var dataSourceMock = new Mock<NpgsqlDataSource>();
        var service = new EventsService(loggerMock.Object, dataSourceMock.Object);

        // Act
        service.Dispose();

        // Assert
        dataSourceMock.Verify(ds => ds.Dispose(), Times.Once);
    }

    // Classe de teste para interceptar SaveEventToDatabaseAsync
    private class TestableEventsService : EventsService
    {
        public bool SaveEventCalled { get; private set; }

        public TestableEventsService(ILogger<EventsService> logger, NpgsqlDataSource dataSource)
            : base(logger, dataSource)
        {
        }

        protected override Task SaveEventToDatabaseAsync(Event eventEntity)
        {
            SaveEventCalled = true;
            return Task.CompletedTask;
        }
    }
}