using Basket.API.Basket.CheckoutBasket;
using Basket.API.Data;
using Basket.API.DTOs;
using Basket.API.Models;
using BuildingBlocks.Messaging.Events;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Moq;
using Xunit;

namespace Basket.API.Tests.Basket.CheckoutBasket
{
    public class CheckoutBasketCommandHandlerTests
    {
        private readonly Mock<IBasketRepository> _mockBasketRepository;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly CheckoutBasketCommandHandler _handler;

        public CheckoutBasketCommandHandlerTests()
        {
            _mockBasketRepository = new Mock<IBasketRepository>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            _handler = new CheckoutBasketCommandHandler(_mockBasketRepository.Object, _mockPublishEndpoint.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenBasketExists()
        {
            // Arrange
            var basketCheckoutDto = new BasketCheckoutDTO { Username = "testuser" };
            var command = new CheckoutBasketCommand(basketCheckoutDto);
            var basket = new ShoppingCart("testuser");

            _mockBasketRepository.Setup(x => x.GetBasket("testuser", It.IsAny<CancellationToken>()))
                                .ReturnsAsync(basket);

            _mockBasketRepository.Setup(x => x.DeleteBasket("testuser", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            _mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<BasketCheckoutEvent>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockBasketRepository.Verify(x => x.GetBasket("testuser", It.IsAny<CancellationToken>()), Times.Once);
            _mockBasketRepository.Verify(x => x.DeleteBasket("testuser", It.IsAny<CancellationToken>()), Times.Once);
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<BasketCheckoutEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenBasketDoesNotExist()
        {
            // Arrange
            var basketCheckoutDto = new BasketCheckoutDTO { Username = "testuser" };
            var command = new CheckoutBasketCommand(basketCheckoutDto);

            _mockBasketRepository.Setup(x => x.GetBasket("testuser", It.IsAny<CancellationToken>()))
                                .ReturnsAsync((ShoppingCart)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            _mockBasketRepository.Verify(x => x.GetBasket("testuser", It.IsAny<CancellationToken>()), Times.Once);
            _mockBasketRepository.Verify(x => x.DeleteBasket(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<BasketCheckoutEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenCommandIsInvalid()
        {
            // Arrange
            var basketCheckoutDto = new BasketCheckoutDTO { Username = "" }; // Invalid username
            var command = new CheckoutBasketCommand(basketCheckoutDto);
            var validator = new CheckoutBasketCommandValidator();

            // Act & Assert
            var validationResult = await validator.ValidateAsync(command);
            validationResult.IsValid.Should().BeFalse();
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Username is required");
        }
    }
}