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
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
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

    // Classe auxiliar para o teste
    private class TestDto { }
}