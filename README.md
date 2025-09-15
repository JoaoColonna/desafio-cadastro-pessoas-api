# API de Cadastro de Pessoas

API desenvolvida em .NET para gerenciar o cadastro de pessoas.

## Requisitos

- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- [PostgreSQL]
- (Opcional) [Docker](https://www.docker.com/) para rodar a aplicação e o banco em containers

## Como rodar localmente

1. **Clone o repositório**
   ```bash
   git clone https://github.com/JoaoColonna/desafio-cadastro-pessoas-api.git
   cd desafio-cadastro-pessoas-api
   ```

2. **Configure o banco de dados**
   - Por padrão, a conexão está definida no arquivo `appsettings.Development.json`.
   - Atualize a string de conexão conforme o exemplo:
   ```bash
   "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Port=5432;Database=apidb;Username=postgres;Password=your_password"
    }
   ```

3. **Restaurar pacotes e buildar o projeto**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Executar as migrations (se aplicável)**
   ```bash
   dotnet ef database update --project RegisterAPI.Infrasctructure --startup-project RegisterAPI
   ```

5. **Rodar a aplicação**
   ```bash
   dotnet run
   ```
   - A API estará disponível em `http://localhost:5432` ou `https://localhost:7162`.

## Testando a API

Acesse '/swagger/index.html' para acessar o Swagger e ter acesso a documentação dos endpoints.

```bash
https://localhost:7162/swagger/index.html
```

## Rodando com Docker (Opcional)

Se preferir rodar usando Docker:

```bash
docker build -t cadastro-pessoas-api .
docker run -p 5000:80 cadastro-pessoas-api
```

## Considerações finais

- Certifique-se de que o banco de dados está rodando e acessível pela aplicação.
- Caso precise de mais informações, consulte o código.
