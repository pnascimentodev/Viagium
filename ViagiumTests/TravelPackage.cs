using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Viagium.Controllers;
using Viagium.Models;
using Viagium.Services;
using Xunit;
using System.Text.Json;
using Viagium.EntitiesDTO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ViagiumTests
{
    public class TravelPackageControllerTests
    {
        [Fact]
        public async Task Create_ValidTravelPackage_ReturnsCreatedAtActionWithPackage()
        {
            // --- ARRANGE ---
            var fakeDto = new CreateTravelPackageDTO
            {
                HotelId = 1,
                Title = "Pacote Teste",
                Description = "Descrição do pacote teste.",
                OriginAddress = "Rua A",
                DestinationAddress = "Rua B",
                ImageUrl = "https://imagem.com/pacote.jpg",
                Duration = 5,
                MaxPeople = 2,
                VehicleType = "Ônibus",
                Price = 1000.00m,
                CreatedBy = 99,
                IsActive = true
            };

            // Mock da classe Address para controlar o retorno do ToString()
            var mockAddress = new Mock<Address>();
            mockAddress.Setup(a => a.ToString()).Returns(fakeDto.DestinationAddress);

            var fakePackage = new TravelPackage
            {
                TravelPackagesId = 1,
                HotelId = 1,
                Title = fakeDto.Title,
                Description = fakeDto.Description,
                OriginAddressId = 10,
                DestinationAddressId = 20,
                DestinationAddress = mockAddress.Object, // Usa o objeto mockado
                ImageUrl = fakeDto.ImageUrl,
                Duration = fakeDto.Duration,
                MaxPeople = fakeDto.MaxPeople,
                VehicleType = fakeDto.VehicleType,
                Price = fakeDto.Price,
                CreatedBy = fakeDto.CreatedBy,
                IsActive = true,
                CreatedAt = System.DateTime.Now
            };

            var mockService = new Mock<ITravelPackage>();
            mockService.Setup(s => s.AddAsync(It.IsAny<CreateTravelPackageDTO>()))
                       .ReturnsAsync(fakePackage);

            var mockLogger = new Mock<ILogger<TravelPackageController>>();
            var controller = new TravelPackageController(mockService.Object, mockLogger.Object);

            // --- ACT ---
            var result = await controller.Create(fakeDto);

            // --- ASSERT ---
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult.Value.Should().NotBeNull();

            var json = JsonSerializer.Serialize(createdResult.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            dict.Should().NotBeNull();

            // CORREÇÃO: Converta os valores de JsonElement para os tipos corretos antes de comparar.
            ((JsonElement)dict["title"]).GetString().Should().Be(fakeDto.Title);
            ((JsonElement)dict["destination"]).GetString().Should().Be(fakeDto.DestinationAddress);
            ((JsonElement)dict["price"]).GetDecimal().Should().Be(fakeDto.Price);
        }
    }
}