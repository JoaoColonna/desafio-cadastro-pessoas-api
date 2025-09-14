using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Controllers;
using Xunit;

namespace RegisterAPI.Tests.Controllers
{
    public class PersonControllerTests
    {
        private readonly Mock<IPersonService> _mockPersonService;
        private readonly PersonController _controller;

        public PersonControllerTests()
        {
            _mockPersonService = new Mock<IPersonService>();
            _controller = new PersonController(_mockPersonService.Object);
        }

        [Fact]
        public void Create_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var dto = new PersonDto
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30)
            };

            var responseDto = new PersonResponseDto
            {
                Id = 1,
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _mockPersonService.Setup(x => x.Create(dto)).Returns(responseDto);

            // Act
            var result = _controller.Create(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().Be(responseDto);
            createdResult.ActionName.Should().Be(nameof(_controller.GetById));
        }

        [Fact]
        public void GetAll_ShouldReturnOkWithAllPersons()
        {
            // Arrange
            var persons = new List<PersonResponseDto>
            {
                new() { Id = 1, Nome = "João Silva", Cpf = "209.941.790-30", DataNascimento = DateTime.Now.AddYears(-30), DataCadastro = DateTime.Now, DataAtualizacao = DateTime.Now },
                new() { Id = 2, Nome = "Maria Santos", Cpf = "209.941.790-30", DataNascimento = DateTime.Now.AddYears(-25), DataCadastro = DateTime.Now, DataAtualizacao = DateTime.Now }
            };

            _mockPersonService.Setup(x => x.GetAll()).Returns(persons);

            // Act
            var result = _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as IEnumerable<PersonResponseDto>;
            response.Should().HaveCount(2);
        }

        [Fact]
        public void GetById_WithExistingId_ShouldReturnOkWithPerson()
        {
            // Arrange
            var id = 1;
            var person = new PersonResponseDto
            {
                Id = id,
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                DataNascimento = DateTime.Now.AddYears(-30),
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _mockPersonService.Setup(x => x.GetById(id)).Returns(person);

            // Act
            var result = _controller.GetById(id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(person);
        }

        [Fact]
        public void GetById_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var id = 999;
            _mockPersonService.Setup(x => x.GetById(id)).Returns((PersonResponseDto?)null);

            // Act
            var result = _controller.GetById(id);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be("Pessoa não encontrada.");
        }

        [Fact]
        public void Update_WithValidData_ShouldReturnOkWithUpdatedPerson()
        {
            // Arrange
            var id = 1;
            var dto = new PersonDto
            {
                Nome = "João Silva Atualizado",
                Cpf = "209.941.790-30",
                Email = "joao.updated@example.com",
                DataNascimento = DateTime.Now.AddYears(-30)
            };

            var updatedPerson = new PersonResponseDto
            {
                Id = id,
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now
            };

            _mockPersonService.Setup(x => x.Update(id, dto)).Returns(updatedPerson);

            // Act
            var result = _controller.Update(id, dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(updatedPerson);
        }

        [Fact]
        public void Delete_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var id = 1;
            _mockPersonService.Setup(x => x.Delete(id));

            // Act
            var result = _controller.Delete(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockPersonService.Verify(x => x.Delete(id), Times.Once);
        }
    }
}