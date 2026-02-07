using Microsoft.EntityFrameworkCore;
using PianificazioneTurni.Infrastructure;

namespace PianificazioneTurni.Services.Pianificazione
{
    public class PianificazioneDbContext : DbContext
    {
        public PianificazioneDbContext()
        {
        }

        public PianificazioneDbContext(DbContextOptions<PianificazioneDbContext> options) : base(options)
        {
            // DataGenerator.InitializePianificazione(this);  // Moved to Startup.cs Configure method
        }

        public DbSet<Nave> Navi { get; set; }
        public DbSet<Dipendente> Dipendenti { get; set; }
        public DbSet<Assegnazione> Assegnazioni { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurazione relazione Assegnazione -> Nave
            modelBuilder.Entity<Assegnazione>()
                .HasOne(a => a.Nave)
                .WithMany(n => n.Assegnazioni)
                .HasForeignKey(a => a.NaveId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurazione relazione Assegnazione -> Dipendente
            modelBuilder.Entity<Assegnazione>()
                .HasOne(a => a.Dipendente)
                .WithMany(d => d.Assegnazioni)
                .HasForeignKey(a => a.DipendenteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
