using Xunit;
using FluentAssertions;
using NSubstitute;
using MediatR;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Basket.API.Tests.Basket.CheckoutBasket
{
    public class CheckoutBasketEndpointTestsTests
    {
        private readonly ISender _sender;
        private readonly CheckoutBasketEndpoint _endpoint;

        public CheckoutBasketEndpointTests()
        {
            _sender = Substitute.For<ISender>();
            _endpoint = new CheckoutBasketEndpoint();
        }

        [Fact]
        public async Task CheckoutBasket_WhenRequestIsValid_ShouldReturnOkResponse()
        {
            // Arrange
            var basketCheckoutDto = new BasketCheckoutDTO
            {
                // Set up your DTO properties here
            };

            var request = new CheckoutBasketRequest(basketCheckoutDto);
            
            _sender.Send(Arg.Any<CheckoutBasketCommand>())
                .Returns(new CheckoutBasketCommandResult { IsSuccess = true });

            // Act
            var app = WebApplication.Create();
            _endpoint.AddRoutes(app);

            var endpoint = app.DataSources
                .FirstOrDefault()
                ?.Endpoints
                .FirstOrDefault(e => e.DisplayName?.Contains("CheckoutBasket") ?? false) as RouteEndpoint;

            var requestDelegate = endpoint?.RequestDelegate;
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.RequestServices = Substitute.For<IServiceProvider>();
            httpContext.RequestServices.GetService(typeof(ISender)).Returns(_sender);

            // Act
            await requestDelegate!(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
            
            await _sender.Received(1)
                .Send(Arg.Any<CheckoutBasketCommand>());
        }

        [Fact]
        public async Task CheckoutBasket_WhenCheckoutFails_ShouldReturnFailureResponse()
        {
            // Arrange
            var basketCheckoutDto = new BasketCheckoutDTO
            {
                // Set up your DTO properties here
            };

            var request = new CheckoutBasketRequest(basketCheckoutDto);
            
            _sender.Send(Arg.Any<CheckoutBasketCommand>())
                .Returns(new CheckoutBasketCommandResult { IsSuccess = false });

            // Act
            var app = WebApplication.Create();
            _endpoint.AddRoutes(app);

            var endpoint = app.DataSources
                .FirstOrDefault()
                ?.Endpoints
                .FirstOrDefault(e => e.DisplayName?.Contains("CheckoutBasket") ?? false) as RouteEndpoint;

            var requestDelegate = endpoint?.RequestDelegate;
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = "POST";
            httpContext.RequestServices = Substitute.For<IServiceProvider>();
            httpContext.RequestServices.GetService(typeof(ISender)).Returns(_sender);

            // Act
            await requestDelegate!(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
            
            var response = await httpContext.Response.ReadFromJsonAsync<CheckoutBasketResponse>();
            response.Should().NotBeNull();
            response!.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void AddRoutes_ShouldConfigureEndpointCorrectly()
        {
            // Arrange
            var app = WebApplication.Create();

            // Act
            _endpoint.AddRoutes(app);

            // Assert
            var endpoint = app.DataSources
                .FirstOrDefault()
                ?.Endpoints
                .FirstOrDefault(e => e.DisplayName?.Contains("CheckoutBasket") ?? false) as RouteEndpoint;

            endpoint.Should().NotBeNull();
            endpoint!.RoutePattern.RawText.Should().Be("/basket/checkout");
            endpoint.Metadata.Should().Contain(m => m is ProducesResponseTypeMetadata meta 
                && meta.StatusCode == StatusCodes.Status201Created);
            endpoint.Metadata.Should().Contain(m => m is ProducesResponseTypeMetadata meta 
                && meta.StatusCode == StatusCodes.Status400BadRequest);
        }
    }
}