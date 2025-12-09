using Microsoft.EntityFrameworkCore;
using PianificazioneTurni.Infrastructure;
using PianificazioneTurni.Services.Shared;

namespace PianificazioneTurni.Services
{
    public class TemplateDbContext : DbContext
    {
        public TemplateDbContext()
        {
        }

        public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
        {
            DataGenerator.InitializeUsers(this);
        }

        public DbSet<User> Users { get; set; }
    }
}
