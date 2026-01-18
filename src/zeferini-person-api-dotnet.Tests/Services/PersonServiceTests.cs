using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MongoDB.Driver;
using Xunit;
using ZeferiniPersonApi.DTOs;
using ZeferiniPersonApi.Models;
using ZeferiniPersonApi.Services;

namespace ZeferiniPersonApi.Tests.Services;

public class PersonServiceTests
{
    private readonly Mock<IMongoCollection<Person>> _personCollectionMock;
    private readonly Mock<IAsyncCursor<Person>> _cursorMock;
    private readonly Mock<IEventsService> _eventsServiceMock;
    private readonly PersonService _service;

    public PersonServiceTests()
    {
        _personCollectionMock = new Mock<IMongoCollection<Person>>();
        _cursorMock = new Mock<IAsyncCursor<Person>>();
        _eventsServiceMock = new Mock<IEventsService>();
        _service = new PersonService(_personCollectionMock.Object, _eventsServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePerson_AndPublishEvent()
    {
        // Arrange
        var dto = new CreatePersonDto { Name = "Test", Email = "test@email.com" };
        _eventsServiceMock.Setup(e => e.PublishEventAsync(It.IsAny<EventPayload>()))
            .ReturnsAsync(new Event());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Name.Should().Be(dto.Name);
        result.Email.Should().Be(dto.Email);
        _eventsServiceMock.Verify(e => e.PublishEventAsync(It.IsAny<EventPayload>()), Times.Once);
    }

    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenGuidInvalid()
    {
        // Act
        var result = await _service.GetByIdAsync("invalid-guid");

        // Assert
        result.Should().BeNull();
    }

    



}