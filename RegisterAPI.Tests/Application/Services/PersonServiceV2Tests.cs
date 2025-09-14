using FluentAssertions;
using Moq;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Application.Services;
using RegisterAPI.Domain.Entities;
using Xunit;

namespace RegisterAPI.Tests.Application.Services
{
    public class PersonServiceV2Tests
    {
        private readonly Mock<IPersonRepositoryV2> _mockRepository;
        private readonly PersonServiceV2 _personService;

        public PersonServiceV2Tests()
        {
            _mockRepository = new Mock<IPersonRepositoryV2>();
            _personService = new PersonServiceV2(_mockRepository.Object);
        }

        [Fact]
        public void Create_WithValidData_ShouldReturnPersonV2ResponseDto()
        {
            // Arrange
            var dto = new PersonV2Dto
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                Sexo = "M",
                Naturalidade = "São Paulo",
                Nacionalidade = "Brasileira",
                Endereco = new EnderecoDto
                {
                    Rua = "Rua das Flores",
                    Numero = "123",
                    Cidade = "São Paulo",
                    Estado = "SP",
                    Cep = "01234-567"
                }
            };

            var createdPessoa = new PessoaV2
            {
                Id = 1,
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                Sexo = dto.Sexo,
                Naturalidade = dto.Naturalidade,
                Nacionalidade = dto.Nacionalidade,
                Endereco = new Endereco
                {
                    Rua = dto.Endereco.Rua,
                    Numero = dto.Endereco.Numero,
                    Cidade = dto.Endereco.Cidade,
                    Estado = dto.Endereco.Estado,
                    Cep = dto.Endereco.Cep
                },
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _mockRepository.Setup(x => x.ExistsByCpf(dto.Cpf)).Returns(false);
            _mockRepository.Setup(x => x.Create(It.IsAny<PessoaV2>())).Returns(createdPessoa);

            // Act
            var result = _personService.Create(dto);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Cpf.Should().Be(dto.Cpf);
            result.Email.Should().Be(dto.Email);
            result.Endereco.Should().NotBeNull();
            result.Endereco.Rua.Should().Be(dto.Endereco.Rua);
            result.Endereco.Cidade.Should().Be(dto.Endereco.Cidade);
            _mockRepository.Verify(x => x.Create(It.IsAny<PessoaV2>()), Times.Once);
        }

        [Fact]
        public void Create_WithExistingCpf_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = new PersonV2Dto
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                Endereco = new EnderecoDto
                {
                    Rua = "Rua das Flores",
                    Numero = "123",
                    Cidade = "São Paulo",
                    Estado = "SP",
                    Cep = "01234-567"
                }
            };

            _mockRepository.Setup(x => x.ExistsByCpf(dto.Cpf)).Returns(true);

            // Act & Assert
            _personService.Invoking(x => x.Create(dto))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("CPF já cadastrado.");
        }

        [Fact]
        public void Create_WithInvalidEndereco_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = new PersonV2Dto
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                DataNascimento = DateTime.Now.AddYears(-30),
                Endereco = new EnderecoDto
                {
                    Rua = "", // Rua vazia
                    Numero = "123",
                    Cidade = "São Paulo",
                    Estado = "SP",
                    Cep = "01234-567"
                }
            };

            // Act & Assert
            _personService.Invoking(x => x.Create(dto))
                .Should().Throw<ArgumentException>()
                .WithMessage("Rua é obrigatória.");
        }

        [Fact]
        public void Update_WithValidData_ShouldReturnUpdatedPersonV2ResponseDto()
        {
            // Arrange
            var id = 1;
            var dto = new PersonV2Dto
            {
                Nome = "João Silva Atualizado",
                Cpf = "209.941.790-30",
                Email = "joao.updated@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                Endereco = new EnderecoDto
                {
                    Rua = "Rua das Palmeiras",
                    Numero = "456",
                    Cidade = "Rio de Janeiro",
                    Estado = "RJ",
                    Cep = "20000-000"
                }
            };

            var existingPessoa = new PessoaV2
            {
                Id = id,
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                Endereco = new Endereco
                {
                    Rua = "Rua das Flores",
                    Numero = "123",
                    Cidade = "São Paulo",
                    Estado = "SP",
                    Cep = "01234-567"
                },
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now.AddDays(-10)
            };

            var updatedPessoa = new PessoaV2
            {
                Id = id,
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                Endereco = new Endereco
                {
                    Rua = dto.Endereco.Rua,
                    Numero = dto.Endereco.Numero,
                    Cidade = dto.Endereco.Cidade,
                    Estado = dto.Endereco.Estado,
                    Cep = dto.Endereco.Cep
                },
                DataCadastro = existingPessoa.DataCadastro,
                DataAtualizacao = DateTime.Now
            };

            _mockRepository.Setup(x => x.GetById(id)).Returns(existingPessoa);
            _mockRepository.Setup(x => x.GetAll()).Returns(new[] { existingPessoa });
            _mockRepository.Setup(x => x.Update(id, It.IsAny<PessoaV2>())).Returns(updatedPessoa);

            // Act
            var result = _personService.Update(id, dto);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Email.Should().Be(dto.Email);
            result.Endereco.Rua.Should().Be(dto.Endereco.Rua);
            result.Endereco.Cidade.Should().Be(dto.Endereco.Cidade);
            _mockRepository.Verify(x => x.Update(id, It.IsAny<PessoaV2>()), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnAllPersonsV2()
        {
            // Arrange
            var pessoas = new[]
            {
                new PessoaV2
                {
                    Id = 1,
                    Nome = "João Silva",
                    Cpf = "209.941.790-30",
                    DataNascimento = DateTime.Now.AddYears(-30),
                    Endereco = new Endereco { Rua = "Rua A", Numero = "123", Cidade = "São Paulo", Estado = "SP", Cep = "01234-567" },
                    DataCadastro = DateTime.Now,
                    DataAtualizacao = DateTime.Now
                },
                new PessoaV2
                {
                    Id = 2,
                    Nome = "Maria Santos",
                    Cpf = "209.941.790-30",
                    DataNascimento = DateTime.Now.AddYears(-25),
                    Endereco = new Endereco { Rua = "Rua B", Numero = "456", Cidade = "Rio de Janeiro", Estado = "RJ", Cep = "20000-000" },
                    DataCadastro = DateTime.Now,
                    DataAtualizacao = DateTime.Now
                }
            };

            _mockRepository.Setup(x => x.GetAll()).Returns(pessoas);

            // Act
            var result = _personService.GetAll();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Nome == "João Silva");
            result.Should().Contain(p => p.Nome == "Maria Santos");
            result.All(p => p.Endereco != null).Should().BeTrue();
        }
    }
}