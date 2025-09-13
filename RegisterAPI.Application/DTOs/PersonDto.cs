namespace RegisterAPI.Application.DTOs
{
    public class PersonDto
    {
        public string Nome { get; set; } = null!;
        public string? Sexo { get; set; }
        public string? Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public string? Naturalidade { get; set; }
        public string? Nacionalidade { get; set; }
        public string Cpf { get; set; } = null!;
    }

    public class EnderecoDto
    {
        public string Rua { get; set; } = null!;
        public string Numero { get; set; } = null!;
        public string Cidade { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Cep { get; set; } = null!;
    }

    public class PersonV2Dto : PersonDto
    {
        public EnderecoDto Endereco { get; set; } = null!;
    }

    public class PersonResponseDto : PersonDto
    {
        public int Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }

    public class PersonV2ResponseDto : PersonV2Dto
    {
        public int Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}