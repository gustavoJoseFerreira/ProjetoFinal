using Microsoft.EntityFrameworkCore;

namespace BankSystemAPI.Models
{
    public class BankContext : DbContext
    {
        public BankContext(DbContextOptions<BankContext> options) : base(options)
        {
        }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Permissao> Permissoes { get; set; }
        public DbSet<UtilizadorPermissao> UtilizadorPermissoes { get; set; }
        public DbSet<ContaBancaria> ContasBancarias { get; set; }
        public DbSet<Movimento> Movimentos { get; set; }
        public DbSet<Transferencia> Transferencias { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<LogAcesso> LogsAcesso { get; set; }
        public DbSet<LogErro> LogsErros { get; set; }
        public DbSet<Backup> Backups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração da chave composta para UtilizadorPermissoes
            modelBuilder.Entity<UtilizadorPermissao>()
                .HasKey(up => new { up.UtilizadorID, up.PermissaoID });

            // Configuração dos relacionamentos
            modelBuilder.Entity<UtilizadorPermissao>()
                .HasOne(up => up.Utilizador)
                .WithMany(u => u.UtilizadorPermissoes)
                .HasForeignKey(up => up.UtilizadorID);

            modelBuilder.Entity<UtilizadorPermissao>()
                .HasOne(up => up.Permissao)
                .WithMany(p => p.UtilizadorPermissoes)
                .HasForeignKey(up => up.PermissaoID);

            // Configuração para Transferencias
            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.ContaOrigem)
                .WithMany(c => c.TransferenciasEnviadas)
                .HasForeignKey(t => t.ContaOrigemID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transferencia>()
                .HasOne(t => t.ContaDestino)
                .WithMany(c => c.TransferenciasRecebidas)
                .HasForeignKey(t => t.ContaDestinoID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuração para garantir que o IBAN seja único
            modelBuilder.Entity<ContaBancaria>()
                .HasIndex(c => c.IBAN)
                .IsUnique();

            // Configuração para garantir que o Email seja único
            modelBuilder.Entity<Utilizador>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configuração para garantir que o NIF seja único
            modelBuilder.Entity<Utilizador>()
                .HasIndex(u => u.NIF)
                .IsUnique();

            // Configuração para garantir que o CartaoCidadao seja único
            modelBuilder.Entity<Utilizador>()
                .HasIndex(u => u.CartaoCidadao)
                .IsUnique();
        }
    }
}