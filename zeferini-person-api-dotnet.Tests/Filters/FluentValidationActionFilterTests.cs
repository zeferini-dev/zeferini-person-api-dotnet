using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Xunit;
using ZeferiniPersonApi.Filters;    

namespace ZeferiniPersonApi.Tests.Filters;

public class FluentValidationActionFilterTests
{
    [Fact]
    public void OnActionExecuting_ValidArgument_DoesNotSetResult()
    {
        // Arrange
        var argument = new object();
        var validatorMock = new Mock<IValidator<object>>();
        validatorMock
            .Setup(v => v.Validate(It.IsAny<IValidationContext>()))
            .Returns(new ValidationResult());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<object>)))
            .Returns(validatorMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            ActionDescriptor = new ControllerActionDescriptor()
        };

        var actionArguments = new Dictionary<string, object> { { "dto", argument } };
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments,
            controller: null
        );

        var filter = new FluentValidationActionFilter();

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuting_InvalidArgument_SetsBadRequestResult()
    {
        // Arrange
        var argument = new object();
        var errors = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };
        var validatorMock = new Mock<IValidator<object>>();
        validatorMock
            .Setup(v => v.Validate(It.IsAny<IValidationContext>()))
            .Returns(new ValidationResult(errors));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<object>)))
            .Returns(validatorMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new ControllerActionDescriptor()
        };

        var actionArguments = new Dictionary<string, object> { { "dto", argument } };
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments,
            controller: null
        );

        var filter = new FluentValidationActionFilter();

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(context.Result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public void OnActionExecuting_NullArgument_DoesNothing()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            ActionDescriptor = new ControllerActionDescriptor()
        };

        var actionArguments = new Dictionary<string, object> { { "dto", null! } };
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments,
            controller: null
        );

        var filter = new FluentValidationActionFilter();

        // Act
        filter.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuted_DoesNothing()
    {
        // Arrange
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new ControllerActionDescriptor()
        };
        var context = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            controller: null
        );
        var filter = new FluentValidationActionFilter();

        // Act & Assert
        filter.OnActionExecuted(context);
        // No exception means pass
    }
}