using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using CqrsWithMediatR.API.Controllers;
using CqrsWithMediatR.Write.Features.Products.Commands;
using CqrsWithMediatR.Read.Features.Products.Queries;
using FluentAssertions;
using AppDomainEntityFramework.Entities;
using System.Collections.Generic;
using System.Linq;
using System;
using CqrsWithMediatR.Exceptions;

namespace CqrsWithMediatR.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ProductController(_mediatorMock.Object);
        }

        [Fact]
        public async Task Create_Should_Return_CreatedAtActionResult()
        {
            // Arrange
            var command = new CreateProductCommand("Test Product", 100);

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(33);

            // Act
            var result = await _controller.Create(command);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();

            var createdResult = result as CreatedAtActionResult;
            createdResult?.Value.Should().Be(33);
        }

        [Fact]
        public async Task UpdateProduct_Should_Return_NoContent_When_Successful()
        {
            // Arrange
            var command = new UpdateProductCommand
            {
                Id = 10,
                Name = "Updated Product",
                Price = 500,
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateProduct(10, command);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_Should_Return_BadRequest_When_Id_Mismatch()
        {
            // Arrange
            var command = new UpdateProductCommand
            {
                Id = 10,
                Name = "Updated Product",
                Price = 500,
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };

            // Act
            var result = await _controller.UpdateProduct(1, command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Product ID mismatch.");
        }

        [Fact]
        public async Task Update_Should_Return_Conflict_On_ConcurrencyException()
        {
            // Arrange
            var command = new UpdateProductCommand
            {
                Id = 10,
                Name = "Updated Product",
                Price = 500,
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConcurrencyException("Row version mismatch.", new Exception("Inner Exception")));

            // Act
            var result = await _controller.UpdateProduct(10, command);

            // Assert
            result.Should().BeOfType<ConflictObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { message = "Row version mismatch." });
        }

        [Fact]
        public async Task GetById_Should_Return_Product_When_Found()
        {
            // Arrange
            ProductReadOnly product = new ProductReadOnly
            {
                Id = 1,
                Name = "Product ABC",
                Price = 100.00m,
                RowVersionBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 })
            };

            var query = new GetProductByIdQuery(1);

            _mediatorMock
                .Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            var okResult = result as OkObjectResult;
            var productReadOnly = okResult?.Value as ProductReadOnly;

            productReadOnly.Should().NotBeNull();
            productReadOnly.Id.Should().Be(1);
            productReadOnly.Name.Should().Be("Product ABC");
            productReadOnly.Price.Should().Be(100.00m);
            productReadOnly.RowVersionBase64.Should().Be(Convert.ToBase64String(new byte[] { 1, 2, 3 }));
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_NotFound()
        {
            // Arrange
            ProductReadOnly? product = null;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product!);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetAll_Should_Return_Ok_With_Products()
        {
            // Arrange
            List<ProductReadOnly> rtnProducts = new List<ProductReadOnly>();
            rtnProducts.Add(new ProductReadOnly
            {
                Id = 1,
                Name = "Product ABC",
                Price = 100.00m,
                RowVersionBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 })
            });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(rtnProducts);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            var okResult = result as OkObjectResult;
            var products = okResult?.Value as List<ProductReadOnly>;

            products.Should().NotBeNull();
            products.Should().HaveCount(1);

            var productReadOnly = products!.First();
            productReadOnly.Id.Should().Be(1);
            productReadOnly.Name.Should().Be("Product ABC");
            productReadOnly.Price.Should().Be(100);
            productReadOnly.RowVersionBase64.Should().Be(Convert.ToBase64String(new byte[] { 1, 2, 3 }));
        }

        [Fact]
        public async Task GetProductsByPrice_Should_Return_Products_Filtered()
        {
            // Arrange
            decimal price = 100;
            string operation = ">=";

            var expectedProducts = new List<ProductReadOnly>
            {
                new() { Id = 1, Name = "Product A", Price = 150 },
                new() { Id = 2, Name = "Product B", Price = 200 }
            };

            GetProductsByPriceQuery query = new GetProductsByPriceQuery(price, operation);

            _mediatorMock
                .Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await _controller.GetProductsByPrice(price, operation);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(expectedProducts);

            var okResult = result as OkObjectResult;
            var foundProducts = okResult?.Value as List<ProductReadOnly>;

            foundProducts.Should().NotBeNull();
            foundProducts.Should().HaveCount(2);

            var foundProduct = foundProducts[0]; 
            foundProduct.Id.Should().Be(1);
            foundProduct.Name.Should().Be("Product A");
            foundProduct.Price.Should().Be(150);

            foundProduct = foundProducts[1];
            foundProduct.Id.Should().Be(2);
            foundProduct.Name.Should().Be("Product B");
            foundProduct.Price.Should().Be(200);
        }
    }
}
