using RegisterAPI.Domain.Entities;
using System.Collections.Generic;

namespace RegisterAPI.Application.Interfaces
{
    public interface IPersonRepository
    {
        Pessoa Create(Pessoa pessoa);
        Pessoa Update(int id, Pessoa pessoa);
        void Delete(int id);
        Pessoa? GetById(int id);
        IEnumerable<Pessoa> GetAll();
        bool ExistsByCpf(string cpf);
    }

    public interface IPersonRepositoryV2
    {
        PessoaV2 Create(PessoaV2 pessoa);
        PessoaV2 Update(int id, PessoaV2 pessoa);
        void Delete(int id);
        PessoaV2? GetById(int id);
        IEnumerable<PessoaV2> GetAll();
        bool ExistsByCpf(string cpf);
    }
}
