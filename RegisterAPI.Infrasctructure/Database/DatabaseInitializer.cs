using Microsoft.EntityFrameworkCore;

namespace RegisterAPI.Infrasctructure.Database
{
    public class DatabaseInitializer
    {
        private readonly AppDbContext _context;

        public DatabaseInitializer(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitializeDatabaseAsync()
        {
            // Criar banco de dados e aplicar migrações
            await _context.Database.EnsureCreatedAsync();
        }
    }
}