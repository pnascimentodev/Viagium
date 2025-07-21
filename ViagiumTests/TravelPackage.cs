using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Viagium.Controllers;
using Viagium.Models;
using Viagium.Repository;
using Viagium.Services;
using System.Text.Json; // Adicionado para serializa��o

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
                Description = "Descri��o do pacote teste.",
                OriginAddressId = 10,
                DestinationAddressId = 20, // diferente do origin
                ImageUrl = "https://imagem.com/pacote.jpg",
                Duration = 5,
                MaxPeople = 2,
                VehicleType = "�nibus",
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

        [Fact]
        // Nome: GetAllTravelPackages_ReturnsListOfPackages
        public async Task GetAllTravelPackages_ReturnsListOfPackages()
        {
            // --- ARRANGE ---
            var fakePackages = new List<TravelPackage>
            {
                new TravelPackage
                {
                    TravelPackagesId = 1,
                    HotelId = 1,
                    Title = "Pacote 1",
                    Description = "Descrição do pacote 1.",
                    OriginAddressId = 10,
                    DestinationAddressId = 20,
                    ImageUrl = "https://imagem.com/pacote1.jpg",
                    Duration = 5,
                    MaxPeople = 2,
                    VehicleType = "Ônibus",
                    Price = 1000.00m,
                    CreatedBy = 99,
                    IsActive = true
                },
                new TravelPackage
                {
                    TravelPackagesId = 2,
                    HotelId = 2,
                    Title = "Pacote 2",
                    Description = "Descrição do pacote 2.",
                    OriginAddressId = 11,
                    DestinationAddressId = 21,
                    ImageUrl = "https://imagem.com/pacote2.jpg",
                    Duration = 7,
                    MaxPeople = 4,
                    VehicleType = "Van",
                    Price = 2000.00m,
                    CreatedBy = 100,
                    IsActive = true
                }
            };

            var mockRepo = new Mock<ITravelPackageRepository>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(fakePackages);

            var mockUoW = new Mock<IUnitOfWork>();
            mockUoW.Setup(u => u.TravelPackageRepository).Returns(mockRepo.Object);

            var service = new TravelPackageService(mockUoW.Object);
            var controller = new TravelPackageController(service);

            // --- ACT ---
            var result = await controller.GetAllTravelPackages();

            // --- ASSERT ---
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().NotBeNull();
            okResult.Value.Should().BeAssignableTo<IEnumerable<TravelPackage>>();
            var returnedPackages = okResult.Value as IEnumerable<TravelPackage>;
            returnedPackages.Should().HaveCount(2);
            returnedPackages.Should().Contain(p => p.Title == "Pacote 1");
            returnedPackages.Should().Contain(p => p.Title == "Pacote 2");
        }
    }
}