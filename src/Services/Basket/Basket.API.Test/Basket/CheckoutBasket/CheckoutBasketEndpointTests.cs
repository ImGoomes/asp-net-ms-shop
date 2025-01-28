using System.Net;
using System.Net.Http.Json;
using Basket.API.Basket.CheckoutBasket;
using Basket.API.DTOs;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;

namespace Basket.API.Test.Basket.CheckoutBasket;

public class CheckoutBasketEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ISender> _mockSender;

    public CheckoutBasketEndpointTests()
    {
        _mockSender = new Mock<ISender>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace ISender with the mock
                    services.AddSingleton(_mockSender.Object);
                });
            });
    }

    [Fact]
    public async Task CheckoutBasket_ShouldReturnOk_WhenCommandIsSuccessful()
    {
        // Arrange
        var request = new CheckoutBasketRequest(new BasketCheckoutDTO() { Username = "user_x" });
        var commandResult = new CheckoutBasketResult(true);

        _mockSender
            .Setup(x => x.Send(It.IsAny<CheckoutBasketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResult);

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/basket/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await response.Content.ReadFromJsonAsync<CheckoutBasketResponse>();
        responseBody.Should().NotBeNull();
        responseBody!.IsSuccess.Should().BeTrue();
        _mockSender.Verify(x => x.Send(It.IsAny<CheckoutBasketCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckoutBasket_ShouldReturnBadRequest_WhenCommandFailsValidation()
    {
        // Arrange
        var request = new CheckoutBasketRequest(new BasketCheckoutDTO { Username = "" }); // Invalid username
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/basket/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CheckoutBasket_ShouldReturnOk_WhenBasketDoesNotExist()
    {
        // Arrange
        var request = new CheckoutBasketRequest(new BasketCheckoutDTO { Username = "user_non" });
        var commandResult = new CheckoutBasketResult(false);

        _mockSender
            .Setup(x => x.Send(It.IsAny<CheckoutBasketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResult);

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/basket/checkout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await response.Content.ReadFromJsonAsync<CheckoutBasketResponse>();
        responseBody.Should().NotBeNull();
        responseBody!.IsSuccess.Should().BeFalse();
        _mockSender.Verify(x => x.Send(It.IsAny<CheckoutBasketCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}