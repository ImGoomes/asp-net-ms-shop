using Basket.API.Basket.CheckoutBasket;
using Basket.API.Basket.CheckoutBasket.Commands;
using Carter;
using FluentAssertions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Basket.API.Tests.Basket.CheckoutBasket
{
    public class CheckoutBasketEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly CheckoutBasketEndpoint _endpoint;

        public CheckoutBasketEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new CheckoutBasketEndpoint();
        }

        [Fact]
        public async Task CheckoutBasket_ShouldReturnCreated_WhenCommandSucceeds()
        {
            // Arrange
            var request = new CheckoutBasketRequest(new BasketCheckoutDTO());
            var command = request.Adapt<CheckoutBasketCommand>();
            var result = new CheckoutBasketResponse(true);

            _mockSender.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(result);

            var httpContext = new DefaultHttpContext();
            var routeBuilder = new RouteBuilder(new ApplicationBuilder(new ServiceCollection().BuildServiceProvider()));
            var app = new EndpointRouteBuilder(routeBuilder);

            _endpoint.AddRoutes(app);

            // Act
            var endpoint = app.DataSources.First().Endpoints.First();
            var handler = endpoint.RequestDelegate;
            var response = await handler(httpContext);

            // Assert
            response.Should().BeOfType<Ok<CheckoutBasketResponse>>();
            var okResult = response as Ok<CheckoutBasketResponse>;
            okResult?.Value.Should().BeEquivalentTo(result);
        }

        [Fact]
        public async Task CheckoutBasket_ShouldReturnBadRequest_WhenCommandFails()
        {
            // Arrange
            var request = new CheckoutBasketRequest(new BasketCheckoutDTO());
            var command = request.Adapt<CheckoutBasketCommand>();
            var result = new CheckoutBasketResponse(false);

            _mockSender.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(result);

            var httpContext = new DefaultHttpContext();
            var routeBuilder = new RouteBuilder(new ApplicationBuilder(new ServiceCollection().BuildServiceProvider()));
            var app = new EndpointRouteBuilder(routeBuilder);

            _endpoint.AddRoutes(app);

            // Act
            var endpoint = app.DataSources.First().Endpoints.First();
            var handler = endpoint.RequestDelegate;
            var response = await handler(httpContext);

            // Assert
            response.Should().BeOfType<BadRequest<ProblemDetails>>();
        }
    }
}