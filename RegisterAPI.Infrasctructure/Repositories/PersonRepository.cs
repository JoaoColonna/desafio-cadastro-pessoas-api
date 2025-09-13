using Microsoft.EntityFrameworkCore;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Domain.Entities;
using RegisterAPI.Infrasctructure.Database;

namespace RegisterAPI.Infrasctructure.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly AppDbContext _context;

        public PersonRepository(AppDbContext context)
        {
            _context = context;
        }

        public Pessoa Create(Pessoa pessoa)
        {
            pessoa.DataCadastro = DateTime.UtcNow;
            pessoa.DataAtualizacao = DateTime.UtcNow;
            
            _context.Pessoas.Add(pessoa);
            _context.SaveChanges();
            
            return pessoa;
        }

        public Pessoa Update(int id, Pessoa pessoa)
        {
            var existingPessoa = _context.Pessoas.Find(id);
            if (existingPessoa == null)
                throw new InvalidOperationException($"Pessoa com ID {id} não encontrada.");

            existingPessoa.Nome = pessoa.Nome;
            existingPessoa.Sexo = pessoa.Sexo;
            existingPessoa.Email = pessoa.Email;
            existingPessoa.DataNascimento = pessoa.DataNascimento;
            existingPessoa.Naturalidade = pessoa.Naturalidade;
            existingPessoa.Nacionalidade = pessoa.Nacionalidade;
            existingPessoa.Cpf = pessoa.Cpf;
            existingPessoa.DataAtualizacao = DateTime.UtcNow;

            _context.SaveChanges();
            return existingPessoa;
        }

        public void Delete(int id)
        {
            var pessoa = _context.Pessoas.Find(id);
            if (pessoa != null)
            {
                _context.Pessoas.Remove(pessoa);
                _context.SaveChanges();
            }
        }

        public Pessoa? GetById(int id)
        {
            return _context.Pessoas.Find(id);
        }

        public IEnumerable<Pessoa> GetAll()
        {
            return _context.Pessoas.OrderBy(p => p.Id).ToList();
        }

        public bool ExistsByCpf(string cpf)
        {
            return _context.Pessoas.Any(p => p.Cpf == cpf);
        }
    }
}