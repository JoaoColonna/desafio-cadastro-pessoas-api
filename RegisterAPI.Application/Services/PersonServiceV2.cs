using RegisterAPI.Application.Interfaces;
using RegisterAPI.Domain.Entities;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Utils;

namespace RegisterAPI.Application.Services
{
    public class PersonServiceV2 : IPersonServiceV2
    {
        private readonly IPersonRepositoryV2 _repository;

        public PersonServiceV2(IPersonRepositoryV2 repository)
        {
            _repository = repository;
        }

        public PersonV2ResponseDto Create(PersonV2Dto dto)
        {
            // Valida��es
            ValidatePersonV2Dto(dto);

            // Verificar se CPF j� existe
            if (_repository.ExistsByCpf(dto.Cpf))
            {
                throw new InvalidOperationException("CPF j� cadastrado.");  
            }

            // Convers�o DTO -> Entidade
            var pessoa = new PessoaV2
            {
                Nome = dto.Nome,
                Sexo = dto.Sexo,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                Naturalidade = dto.Naturalidade,
                Nacionalidade = dto.Nacionalidade,
                Cpf = dto.Cpf,
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

            // Chama reposit�rio
            var createdPessoa = _repository.Create(pessoa);

            // Convers�o Entidade -> DTO
            return MapToResponseDto(createdPessoa);
        }

        public PersonV2ResponseDto Update(int id, PersonV2Dto dto)
        {
            // Valida��es
            ValidatePersonV2Dto(dto);

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
            var pessoa = new PessoaV2
            {
                Nome = dto.Nome,
                Sexo = dto.Sexo,
                Email = dto.Email,
                DataNascimento = dto.DataNascimento,
                Naturalidade = dto.Naturalidade,
                Nacionalidade = dto.Nacionalidade,
                Cpf = dto.Cpf,
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

        public PersonV2ResponseDto? GetById(int id)
        {
            var pessoa = _repository.GetById(id);
            return pessoa != null ? MapToResponseDto(pessoa) : null;
        }

        public IEnumerable<PersonV2ResponseDto> GetAll()
        {
            var pessoas = _repository.GetAll();
            return pessoas.Select(MapToResponseDto);
        }

        private static void ValidatePersonV2Dto(PersonV2Dto dto)
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

            // Valida��es do endere�o (obrigat�rio na V2)
            if (dto.Endereco == null)
                throw new ArgumentException("Endere�o � obrigat�rio.");

            if (string.IsNullOrWhiteSpace(dto.Endereco.Rua))
                throw new ArgumentException("Rua � obrigat�ria.");

            if (string.IsNullOrWhiteSpace(dto.Endereco.Numero))
                throw new ArgumentException("N�mero � obrigat�rio.");

            if (string.IsNullOrWhiteSpace(dto.Endereco.Cidade))
                throw new ArgumentException("Cidade � obrigat�ria.");

            if (string.IsNullOrWhiteSpace(dto.Endereco.Estado))
                throw new ArgumentException("Estado � obrigat�rio.");

            if (string.IsNullOrWhiteSpace(dto.Endereco.Cep))
                throw new ArgumentException("CEP � obrigat�rio.");
        }

        private static PersonV2ResponseDto MapToResponseDto(PessoaV2 pessoa)
        {
            return new PersonV2ResponseDto
            {
                Id = pessoa.Id,
                Nome = pessoa.Nome,
                Sexo = pessoa.Sexo,
                Email = pessoa.Email,
                DataNascimento = pessoa.DataNascimento,
                Naturalidade = pessoa.Naturalidade,
                Nacionalidade = pessoa.Nacionalidade,
                Cpf = pessoa.Cpf,
                Endereco = new EnderecoDto
                {
                    Rua = pessoa.Endereco.Rua,
                    Numero = pessoa.Endereco.Numero,
                    Cidade = pessoa.Endereco.Cidade,
                    Estado = pessoa.Endereco.Estado,
                    Cep = pessoa.Endereco.Cep
                },
                DataCadastro = pessoa.DataCadastro,
                DataAtualizacao = pessoa.DataAtualizacao
            };
        }
    }
}