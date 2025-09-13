using Microsoft.EntityFrameworkCore;
using RegisterAPI.Domain.Entities;

namespace RegisterAPI.Infrasctructure.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Pessoa> Pessoas { get; set; }
        public DbSet<PessoaV2> PessoasV2 { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da entidade Pessoa
            modelBuilder.Entity<Pessoa>(entity =>
            {
                entity.ToTable("Pessoa");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Sexo).HasMaxLength(1);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.DataNascimento).IsRequired();
                entity.Property(e => e.Naturalidade).HasMaxLength(255);
                entity.Property(e => e.Nacionalidade).HasMaxLength(255);
                entity.Property(e => e.Cpf).IsRequired().HasMaxLength(14);
                entity.Property(e => e.DataCadastro).IsRequired();
                entity.Property(e => e.DataAtualizacao).IsRequired();
                
                entity.HasIndex(e => e.Cpf).IsUnique();
            });

            // Configuração da entidade PessoaV2 - como entidade independente
            modelBuilder.Entity<PessoaV2>(entity =>
            {
                entity.ToTable("PessoaV2");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Sexo).HasMaxLength(1);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.DataNascimento).IsRequired();
                entity.Property(e => e.Naturalidade).HasMaxLength(255);
                entity.Property(e => e.Nacionalidade).HasMaxLength(255);
                entity.Property(e => e.Cpf).IsRequired().HasMaxLength(14);
                entity.Property(e => e.DataCadastro).IsRequired();
                entity.Property(e => e.DataAtualizacao).IsRequired();
                
                entity.HasIndex(e => e.Cpf).IsUnique();

                // Configuração do Endereco como owned type
                entity.OwnsOne(p => p.Endereco, endereco =>
                {
                    endereco.Property(e => e.Rua).IsRequired().HasMaxLength(255).HasColumnName("EnderecoRua");
                    endereco.Property(e => e.Numero).IsRequired().HasMaxLength(10).HasColumnName("EnderecoNumero");
                    endereco.Property(e => e.Cidade).IsRequired().HasMaxLength(255).HasColumnName("EnderecoCidade");
                    endereco.Property(e => e.Estado).IsRequired().HasMaxLength(2).HasColumnName("EnderecoEstado");
                    endereco.Property(e => e.Cep).IsRequired().HasMaxLength(9).HasColumnName("EnderecoCep");
                });
            });

            // Configuração da entidade User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}