using Microsoft.EntityFrameworkCore;

namespace FunctionAppAcoes.Data
{
    public class AcoesContext : DbContext
    {
        public DbSet<Acao> Acoes { get; set; }

        public AcoesContext(DbContextOptions<AcoesContext> options) :
            base(options)
        {
        }        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Acao>()
                .HasKey(a => a.Id);
        }
    }
}