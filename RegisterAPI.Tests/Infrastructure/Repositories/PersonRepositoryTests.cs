using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RegisterAPI.Domain.Entities;
using RegisterAPI.Infrasctructure.Database;
using RegisterAPI.Infrasctructure.Repositories;
using Xunit;

namespace RegisterAPI.Tests.Infrastructure.Repositories
{
    public class PersonRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly PersonRepository _repository;

        public PersonRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new PersonRepository(_context);
        }

        [Fact]
        public void Create_WithValidPessoa_ShouldReturnCreatedPessoa()
        {
            // Arrange
            var pessoa = new Pessoa
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                Sexo = "M",
                Naturalidade = "São Paulo",
                Nacionalidade = "Brasileira"
            };

            // Act
            var result = _repository.Create(pessoa);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Nome.Should().Be(pessoa.Nome);
            result.Cpf.Should().Be(pessoa.Cpf);
            result.DataCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GetById_WithExistingId_ShouldReturnPessoa()
        {
            // Arrange
            var pessoa = new Pessoa
            {
                Nome = "Maria Santos",
                Cpf = "209.941.790-30",
                Email = "maria@example.com",
                DataNascimento = DateTime.Now.AddYears(-25),
                DataCadastro = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            _context.Pessoas.Add(pessoa);
            _context.SaveChanges();

            // Act
            var result = _repository.GetById(pessoa.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(pessoa.Id);
            result.Nome.Should().Be(pessoa.Nome);
            result.Cpf.Should().Be(pessoa.Cpf);
        }

        [Fact]
        public void GetById_WithNonExistentId_ShouldReturnNull()
        {
            // Act
            var result = _repository.GetById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Update_WithValidData_ShouldReturnUpdatedPessoa()
        {
            // Arrange
            var originalPessoa = new Pessoa
            {
                Nome = "João Silva",
                Cpf = "209.941.790-30",
                Email = "joao@example.com",
                DataNascimento = DateTime.Now.AddYears(-30),
                DataCadastro = DateTime.UtcNow.AddDays(-1),
                DataAtualizacao = DateTime.UtcNow.AddDays(-1)
            };

            _context.Pessoas.Add(originalPessoa);
            _context.SaveChanges();

            var updatedData = new Pessoa
            {
                Nome = "João Silva Atualizado",
                Cpf = "209.941.790-30",
                Email = "joao.updated@example.com",
                DataNascimento = originalPessoa.DataNascimento,
                DataCadastro = originalPessoa.DataCadastro
            };

            // Act
            var result = _repository.Update(originalPessoa.Id, updatedData);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(updatedData.Nome);
            result.Email.Should().Be(updatedData.Email);
            result.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.DataCadastro.Should().Be(originalPessoa.DataCadastro);
        }

        [Fact]
        public void Delete_WithExistingId_ShouldRemovePessoa()
        {
            // Arrange
            var pessoa = new Pessoa
            {
                Nome = "João para Deletar",
                Cpf = "11111111111",
                DataNascimento = DateTime.Now.AddYears(-30),
                DataCadastro = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            _context.Pessoas.Add(pessoa);
            _context.SaveChanges();
            var pessoaId = pessoa.Id;

            // Act
            _repository.Delete(pessoaId);

            // Assert
            var deletedPessoa = _context.Pessoas.Find(pessoaId);
            deletedPessoa.Should().BeNull();
        }

        [Fact]
        public void GetAll_ShouldReturnAllPessoas()
        {
            // Arrange
            var pessoas = new[]
            {
                new Pessoa { Nome = "João", Cpf = "11111111111", DataNascimento = DateTime.Now.AddYears(-30), DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow },
                new Pessoa { Nome = "Maria", Cpf = "22222222222", DataNascimento = DateTime.Now.AddYears(-25), DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow },
                new Pessoa { Nome = "Pedro", Cpf = "33333333333", DataNascimento = DateTime.Now.AddYears(-40), DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow }
            };

            _context.Pessoas.AddRange(pessoas);
            _context.SaveChanges();

            // Act
            var result = _repository.GetAll();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(p => p.Nome == "João");
            result.Should().Contain(p => p.Nome == "Maria");
            result.Should().Contain(p => p.Nome == "Pedro");
        }

        [Fact]
        public void ExistsByCpf_WithExistingCpf_ShouldReturnTrue()
        {
            // Arrange
            var pessoa = new Pessoa
            {
                Nome = "João",
                Cpf = "209.941.790-30",
                DataNascimento = DateTime.Now.AddYears(-30),
                DataCadastro = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            _context.Pessoas.Add(pessoa);
            _context.SaveChanges();

            // Act
            var result = _repository.ExistsByCpf("209.941.790-30");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ExistsByCpf_WithNonExistentCpf_ShouldReturnFalse()
        {
            // Act
            var result = _repository.ExistsByCpf("99999999999");

            // Assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}