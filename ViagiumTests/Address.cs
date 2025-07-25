using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Viagium.Controllers;
using Viagium.EntitiesDTO;
using Viagium.Repository;
using Viagium.Services;
using System.Text.Json;
using AutoMapper;
using Viagium.Repository.Interface;

namespace ViagiumTests
{
    public class AddressControllerTests
    {
        [Fact]
        public async Task Create_ValidAddress_ReturnsCreatedAtActionWithAddress()
        {
            // --- ARRANGE ---
            var fakeAddressDto = new AddressDTO
            {
                StreetName = "Rua Teste",
                AddressNumber = 123,
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01000-000",
                Country = "Brasil",
                AffiliateId = 0,
                Affiliate = null,
                HotelId = 1,
                Hotel = null
            };

            var mockRepo = new Mock<IAddressRepository>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Viagium.Models.Address>())).Returns(Task.CompletedTask);

            var mockUoW = new Mock<IUnitOfWork>();
            mockUoW.Setup(u => u.AddressRepository).Returns(mockRepo.Object);
            mockUoW.Setup(u => u.SaveAsync()).ReturnsAsync(1);

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<Viagium.Models.Address>(It.IsAny<AddressDTO>())).Returns(new Viagium.Models.Address
            {
                AdressId = 1,
                StreetName = fakeAddressDto.StreetName,
                AddressNumber = fakeAddressDto.AddressNumber,
                Neighborhood = fakeAddressDto.Neighborhood,
                City = fakeAddressDto.City,
                State = fakeAddressDto.State,
                ZipCode = fakeAddressDto.ZipCode,
                Country = fakeAddressDto.Country,
                AffiliateId = fakeAddressDto.AffiliateId,
                HotelId = fakeAddressDto.HotelId
            });
            mockMapper.Setup(m => m.Map<AddressDTO>(It.IsAny<Viagium.Models.Address>())).Returns(fakeAddressDto);

            var service = new AddressService(mockUoW.Object, mockMapper.Object);
            var controller = new AddressController(service);

            // --- ACT ---
            var result = await controller.Create(fakeAddressDto);

            // --- ASSERT ---
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Value.Should().NotBeNull();
            createdResult.Value.Should().BeAssignableTo<AddressDTO>();
            var returnedAddress = createdResult.Value as AddressDTO;
            returnedAddress.StreetName.Should().Be(fakeAddressDto.StreetName);
            returnedAddress.AddressNumber.Should().Be(fakeAddressDto.AddressNumber);
            returnedAddress.City.Should().Be(fakeAddressDto.City);

            // Log do retorno da rota
            var retornoJson = JsonSerializer.Serialize(returnedAddress);
            Console.WriteLine(retornoJson);
        }
    }
}
