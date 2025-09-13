using RegisterAPI.Application.Interfaces;
using RegisterAPI.Domain.Entities;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Utils;

namespace RegisterAPI.Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _repository;

        public PersonService(IPersonRepository repository)
        {
            _repository = repository;
        }

        public PersonResponseDto Create(PersonDto dto)
        {
            // Valida��es
            ValidatePersonDto(dto);

            // Verificar se CPF j� existe
            if (_repository.ExistsByCpf(dto.Cpf))
            {
                throw new InvalidOperationException("CPF j� cadastrado.");
            }

            // Convers�o DTO -> Entidade
            var pessoa = new Pessoa
            {
                Nome = dto.Nome,
                Sexo = dto.Sexo,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                Naturalidade = dto.Naturalidade,
                Nacionalidade = dto.Nacionalidade,
                Cpf = dto.Cpf,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Chama reposit�rio
            var createdPessoa = _repository.Create(pessoa);

            // Convers�o Entidade -> DTO
            return MapToResponseDto(createdPessoa);
        }

        public PersonResponseDto Update(int id, PersonDto dto)
        {
            // Valida��es
            ValidatePersonDto(dto);

            // Verificar se pessoa existe
            var existingPessoa = _repository.GetById(id);
            if (existingPessoa == null)
            {
                throw new ArgumentException("Pessoa n�o encontrada.");
            }

            // Verificar se CPF j� existe (exceto para a pr�pria pessoa)
            var pessoaComCpf = _repository.GetAll().FirstOrDefault(p => p.Cpf == dto.Cpf);
            if (pessoaComCpf != null && pessoaComCpf.Id != id)
            {
                throw new InvalidOperationException("CPF j� cadastrado para outra pessoa.");
            }

            // Convers�o DTO -> Entidade
            var pessoa = new Pessoa
            {
                Nome = dto.Nome,
                Sexo = dto.Sexo,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                Naturalidade = dto.Naturalidade,
                Nacionalidade = dto.Nacionalidade,
                Cpf = dto.Cpf,
                DataCadastro = existingPessoa.DataCadastro,
                DataAtualizacao = DateTime.Now
            };

            // Chama reposit�rio
            var updatedPessoa = _repository.Update(id, pessoa);

            // Convers�o Entidade -> DTO
            return MapToResponseDto(updatedPessoa);
        }

        public void Delete(int id)
        {
            var existingPessoa = _repository.GetById(id);
            if (existingPessoa == null)
            {
                throw new ArgumentException("Pessoa n�o encontrada.");
            }

            _repository.Delete(id);
        }

        public PersonResponseDto? GetById(int id)
        {
            var pessoa = _repository.GetById(id);
            return pessoa != null ? MapToResponseDto(pessoa) : null;
        }

        public IEnumerable<PersonResponseDto> GetAll()
        {
            var pessoas = _repository.GetAll();
            return pessoas.Select(MapToResponseDto);
        }

        private static void ValidatePersonDto(PersonDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new ArgumentException("Nome � obrigat�rio.");

            if (string.IsNullOrWhiteSpace(dto.Cpf))
                throw new ArgumentException("CPF � obrigat�rio.");

            if (!CpfValidator.IsValid(dto.Cpf))
                throw new ArgumentException("CPF inv�lido.");

            if (dto.DataNascimento == default)
                throw new ArgumentException("Data de nascimento � obrigat�ria.");

            if (dto.DataNascimento > DateTime.Now)
                throw new ArgumentException("Data de nascimento n�o pode ser futura.");

            if (!string.IsNullOrWhiteSpace(dto.Email) && !EmailValidator.IsValid(dto.Email))
                throw new ArgumentException("Email inv�lido.");
        }

        private static PersonResponseDto MapToResponseDto(Pessoa pessoa)
        {
            return new PersonResponseDto
            {
                Id = pessoa.Id,
                Nome = pessoa.Nome,
                Sexo = pessoa.Sexo,
                Email = pessoa.Email,
                DataNascimento = pessoa.DataNascimento,
                Naturalidade = pessoa.Naturalidade,
                Nacionalidade = pessoa.Nacionalidade,
                Cpf = pessoa.Cpf,
                DataCadastro = pessoa.DataCadastro,
                DataAtualizacao = pessoa.DataAtualizacao
            };
        }
    }
}