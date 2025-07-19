using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Viagium.Controller;
using Viagium.Models;
using Viagium.Repository;
using Viagium.Services;
using Xunit;
using System.Text.Json; // Adicionado para serialização

namespace ViagiumTests
{
    public class TravelPackageControllerTests
    {
        [Fact]
        // Nome: Create_ValidTravelPackage_ReturnsCreatedAtActionWithPackage
        public async Task Create_ValidTravelPackage_ReturnsCreatedAtActionWithPackage()
        {
            // --- ARRANGE ---
            var fakePackage = new TravelPackage
            {
                TravelPackagesId = 1,
                HotelId = 1,
                Title = "Pacote Teste",
                Description = "Descrição do pacote teste.",
                OriginAddressId = 10,
                DestinationAddressId = 20, // diferente do origin
                ImageUrl = "https://imagem.com/pacote.jpg",
                Duration = 5,
                MaxPeople = 2,
                VehicleType = "Ônibus",
                Price = 1000.00m,
                CreatedBy = 99,
                IsActive = true
            };

            var mockRepo = new Mock<ITravelPackageRepository>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<TravelPackage>())).Returns(Task.CompletedTask);

            var mockUoW = new Mock<IUnitOfWork>();
            mockUoW.Setup(u => u.TravelPackageRepository).Returns(mockRepo.Object);
            mockUoW.Setup(u => u.SaveAsync()).ReturnsAsync(1);

            var service = new TravelPackageService(mockUoW.Object);
            var controller = new TravelPackageController(service);

            // --- ACT ---
            var result = await controller.Create(fakePackage);

            // --- ASSERT ---
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Value.Should().NotBeNull();
            createdResult.Value.Should().BeAssignableTo<TravelPackage>();
            var returnedPackage = createdResult.Value as TravelPackage;
            returnedPackage.Title.Should().Be(fakePackage.Title);
            returnedPackage.Price.Should().Be(fakePackage.Price);
            returnedPackage.OriginAddressId.Should().NotBe(returnedPackage.DestinationAddressId);

            // Log do retorno da rota
            var retornoJson = JsonSerializer.Serialize(returnedPackage);
            Console.WriteLine(retornoJson);
        }
    }
}