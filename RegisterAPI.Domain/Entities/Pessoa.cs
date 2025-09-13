namespace RegisterAPI.Domain.Entities
{
    public class Pessoa
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string? Sexo { get; set; }
        public string? Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public string? Naturalidade { get; set; }
        public string? Nacionalidade { get; set; }
        public string Cpf { get; set; } = null!;
        public DateTime DataCadastro { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }

    public class Endereco
    {
        public string Rua { get; set; } = null!;
        public string Numero { get; set; } = null!;
        public string Cidade { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Cep { get; set; } = null!;
    }

    public class PessoaV2
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string? Sexo { get; set; }
        public string? Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public string? Naturalidade { get; set; }
        public string? Nacionalidade { get; set; }
        public string Cpf { get; set; } = null!;
        public DateTime DataCadastro { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public Endereco Endereco { get; set; } = null!;
    }
}