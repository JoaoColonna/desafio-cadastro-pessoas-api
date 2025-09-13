using Microsoft.EntityFrameworkCore;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Domain.Entities;
using RegisterAPI.Infrasctructure.Database;

namespace RegisterAPI.Infrasctructure.Repositories
{
    public class PersonRepositoryV2 : IPersonRepositoryV2
    {
        private readonly AppDbContext _context;

        public PersonRepositoryV2(AppDbContext context)
        {
            _context = context;
        }

        public PessoaV2 Create(PessoaV2 pessoa)
        {
            pessoa.DataCadastro = DateTime.UtcNow;
            pessoa.DataAtualizacao = DateTime.UtcNow;
            
            _context.PessoasV2.Add(pessoa);
            _context.SaveChanges();
            
            return pessoa;
        }

        public PessoaV2 Update(int id, PessoaV2 pessoa)
        {
            var existingPessoa = _context.PessoasV2.Find(id);
            if (existingPessoa == null)
                throw new InvalidOperationException($"Pessoa com ID {id} não encontrada.");

            existingPessoa.Nome = pessoa.Nome;
            existingPessoa.Sexo = pessoa.Sexo;
            existingPessoa.Email = pessoa.Email;
            existingPessoa.DataNascimento = pessoa.DataNascimento;
            existingPessoa.Naturalidade = pessoa.Naturalidade;
            existingPessoa.Nacionalidade = pessoa.Nacionalidade;
            existingPessoa.Cpf = pessoa.Cpf;
            existingPessoa.Endereco = pessoa.Endereco;
            existingPessoa.DataAtualizacao = DateTime.UtcNow;

            _context.SaveChanges();
            return existingPessoa;
        }

        public void Delete(int id)
        {
            var pessoa = _context.PessoasV2.Find(id);
            if (pessoa != null)
            {
                _context.PessoasV2.Remove(pessoa);
                _context.SaveChanges();
            }
        }

        public PessoaV2? GetById(int id)
        {
            return _context.PessoasV2.Find(id);
        }

        public IEnumerable<PessoaV2> GetAll()
        {
            return _context.PessoasV2.OrderBy(p => p.Id).ToList();
        }

        public bool ExistsByCpf(string cpf)
        {
            return _context.PessoasV2.Any(p => p.Cpf == cpf);
        }
    }
}