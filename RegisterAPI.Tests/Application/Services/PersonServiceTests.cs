using FluentAssertions;
using Moq;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Application.Services;
using RegisterAPI.Domain.Entities;
using Xunit;

namespace RegisterAPI.Tests.Application.Services
{
    public class PersonServiceTests
    {
        private readonly Mock<IPersonRepository> _mockRepository;
        private readonly PersonService _personService;

        public PersonServiceTests()
        {
            _mockRepository = new Mock<IPersonRepository>();
            _personService = new PersonService(_mockRepository.Object);
        }

        [Fact]
        public void Create_WithValidData_ShouldReturnPersonResponseDto()
        {
            // Arrange
            var dto = new PersonDto
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                Sexo = "M",
                Naturalidade = "São Paulo",
                Nacionalidade = "Brasileira"
            };

            var createdPessoa = new Pessoa
            {
                Id = 1,
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                Sexo = dto.Sexo,
                Naturalidade = dto.Naturalidade,
                Nacionalidade = dto.Nacionalidade,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _mockRepository.Setup(x => x.ExistsByCpf(dto.Cpf)).Returns(false);
            _mockRepository.Setup(x => x.Create(It.IsAny<Pessoa>())).Returns(createdPessoa);

            // Act
            var result = _personService.Create(dto);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Cpf.Should().Be(dto.Cpf);
            result.Email.Should().Be(dto.Email);
            _mockRepository.Verify(x => x.Create(It.IsAny<Pessoa>()), Times.Once);
        }

        [Fact]
        public void Create_WithExistingCpf_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = new PersonDto
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30)
            };

            _mockRepository.Setup(x => x.ExistsByCpf(dto.Cpf)).Returns(true);

            // Act & Assert
            _personService.Invoking(x => x.Create(dto))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("CPF já cadastrado.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithInvalidNome_ShouldThrowArgumentException(string nome)
        {
            // Arrange
            var dto = new PersonDto
            {
                Nome = nome,
                Cpf = "209.941.790-30",
                DataNascimento = DateTime.Now.AddYears(-30)
            };

            // Act & Assert
            _personService.Invoking(x => x.Create(dto))
                .Should().Throw<ArgumentException>()
                .WithMessage("Nome é obrigatório.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithInvalidCpf_ShouldThrowArgumentException(string cpf)
        {
            // Arrange
            var dto = new PersonDto
            {
                Nome = "João Silva",
                Cpf = cpf,
                DataNascimento = DateTime.Now.AddYears(-30)
            };

            // Act & Assert
            _personService.Invoking(x => x.Create(dto))
                .Should().Throw<ArgumentException>()
                .WithMessage("CPF é obrigatório.");
        }

        [Fact]
        public void Create_WithFutureBirthDate_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = new PersonDto
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                DataNascimento = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            _personService.Invoking(x => x.Create(dto))
                .Should().Throw<ArgumentException>()
                .WithMessage("Data de nascimento não pode ser futura.");
        }

        [Fact]
        public void Update_WithValidData_ShouldReturnUpdatedPersonResponseDto()
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

            var existingPessoa = new Pessoa
            {
                Id = id,
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now.AddDays(-10)
            };

            var updatedPessoa = new Pessoa
            {
                Id = id,
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                DataCadastro = existingPessoa.DataCadastro,
                DataAtualizacao = DateTime.Now
            };

            _mockRepository.Setup(x => x.GetById(id)).Returns(existingPessoa);
            _mockRepository.Setup(x => x.GetAll()).Returns(new[] { existingPessoa });
            _mockRepository.Setup(x => x.Update(id, It.IsAny<Pessoa>())).Returns(updatedPessoa);

            // Act
            var result = _personService.Update(id, dto);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Email.Should().Be(dto.Email);
            _mockRepository.Verify(x => x.Update(id, It.IsAny<Pessoa>()), Times.Once);
        }

        [Fact]
        public void Update_WithNonExistentId_ShouldThrowArgumentException()
        {
            // Arrange
            var id = 999;
            var dto = new PersonDto
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                DataNascimento = DateTime.Now.AddYears(-30)
            };

            _mockRepository.Setup(x => x.GetById(id)).Returns((Pessoa?)null);

            // Act & Assert
            _personService.Invoking(x => x.Update(id, dto))
                .Should().Throw<ArgumentException>()
                .WithMessage("Pessoa não encontrada.");
        }

        [Fact]
        public void Delete_WithValidId_ShouldCallRepositoryDelete()
        {
            // Arrange
            var id = 1;
            var existingPessoa = new Pessoa { Id = id, Nome = "João Silva", Cpf = "209.941.790-30" };

            _mockRepository.Setup(x => x.GetById(id)).Returns(existingPessoa);

            // Act
            _personService.Delete(id);

            // Assert
            _mockRepository.Verify(x => x.Delete(id), Times.Once);
        }

        [Fact]
        public void Delete_WithNonExistentId_ShouldThrowArgumentException()
        {
            // Arrange
            var id = 999;
            _mockRepository.Setup(x => x.GetById(id)).Returns((Pessoa?)null);

            // Act & Assert
            _personService.Invoking(x => x.Delete(id))
                .Should().Throw<ArgumentException>()
                .WithMessage("Pessoa não encontrada.");
        }

        [Fact]
        public void GetById_WithValidId_ShouldReturnPersonResponseDto()
        {
            // Arrange
            var id = 1;
            var pessoa = new Pessoa
            {
                Id = id,
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _mockRepository.Setup(x => x.GetById(id)).Returns(pessoa);

            // Act
            var result = _personService.GetById(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Nome.Should().Be(pessoa.Nome);
            result.Cpf.Should().Be(pessoa.Cpf);
        }

        [Fact]
        public void GetById_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var id = 999;
            _mockRepository.Setup(x => x.GetById(id)).Returns((Pessoa?)null);

            // Act
            var result = _personService.GetById(id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetAll_ShouldReturnAllPersons()
        {
            // Arrange
            var pessoas = new[]
            {
                new Pessoa { Id = 1, Nome = "João Silva", Cpf = "209.941.790-30", DataNascimento = DateTime.Now.AddYears(-30), DataCadastro = DateTime.Now, DataAtualizacao = DateTime.Now },
                new Pessoa { Id = 2, Nome = "Maria Santos", Cpf = "209.941.790-30", DataNascimento = DateTime.Now.AddYears(-25), DataCadastro = DateTime.Now, DataAtualizacao = DateTime.Now }
            };

            _mockRepository.Setup(x => x.GetAll()).Returns(pessoas);

            // Act
            var result = _personService.GetAll();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Nome == "João Silva");
            result.Should().Contain(p => p.Nome == "Maria Santos");
        }
    }
}