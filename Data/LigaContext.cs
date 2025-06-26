using LBAChamps.Models;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Data;

/// <summary>
/// DbContext da aplicação – usa SQL Server.
/// </summary>
public class LigaContext : DbContext
{
    public LigaContext(DbContextOptions<LigaContext> options) : base(options) { }

    // DbSets -----------------------------------------------------------------
    public DbSet<Liga> Ligas => Set<Liga>();
    public DbSet<Time> Times => Set<Time>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Partida> Partidas => Set<Partida>();
    public DbSet<Jogador> Jogadores => Set<Jogador>();
    public DbSet<EstatisticasPartida> EstatisticasPartidas => Set<EstatisticasPartida>();
    public DbSet<Noticia> Noticias => Set<Noticia>();

    // Modelo -----------------------------------------------------------------
    protected override void OnModelCreating(ModelBuilder m)
    {
        //---------------------------------------------------------------------
        // LIGA
        //---------------------------------------------------------------------
        m.Entity<Liga>()
            .HasIndex(l => new { l.Nome, l.Esporte })
            .IsUnique();

        //---------------------------------------------------------------------
        // TIME  (1 Liga -> * Times) – Delete: RESTRICT
        //---------------------------------------------------------------------
        m.Entity<Time>()
            .HasOne(t => t.Liga)
            .WithMany(l => l.Times)
            .HasForeignKey(t => t.IdLiga)
            .OnDelete(DeleteBehavior.Restrict);

        //---------------------------------------------------------------------
        // JOGADOR  (1 Time -> * Jogadores) – Delete: CASCADE
        //---------------------------------------------------------------------
        m.Entity<Jogador>()
            .HasOne(j => j.Time)
            .WithMany(t => t.Jogadores)
            .HasForeignKey(j => j.IdTime)
            .OnDelete(DeleteBehavior.Cascade);

        m.Entity<Jogador>()
         .HasIndex(j => new { j.IdTime, j.NumeroCamisa })
         .IsUnique();

        //---------------------------------------------------------------------
        // PARTIDA
        //---------------------------------------------------------------------
        m.Entity<Partida>()
            .HasOne(p => p.Liga)
            .WithMany(l => l.Partidas)
            .HasForeignKey(p => p.IdLiga)
            .OnDelete(DeleteBehavior.Cascade);

        m.Entity<Partida>()
            .HasOne(p => p.TimeCasa)
            .WithMany(t => t.PartidasCasa)
            .HasForeignKey(p => p.IdTimeCasa)
            .OnDelete(DeleteBehavior.Restrict);

        m.Entity<Partida>()
            .HasOne(p => p.TimeFora)
            .WithMany(t => t.PartidasFora)
            .HasForeignKey(p => p.IdTimeFora)
            .OnDelete(DeleteBehavior.Restrict);

        m.Entity<Partida>()
            .HasCheckConstraint("CK_Partida_TimesDiferentes",
                                "[IdTimeCasa] <> [IdTimeFora]");

        //---------------------------------------------------------------------
        // ESTATÍSTICAS DA PARTIDA
        //---------------------------------------------------------------------
        m.Entity<EstatisticasPartida>()
            .HasOne(e => e.Partida)
            .WithMany(p => p.Estatisticas)
            .HasForeignKey(e => e.IdPartida)
            .OnDelete(DeleteBehavior.Cascade);

        m.Entity<EstatisticasPartida>()
            .HasOne(e => e.Jogador)
            .WithMany(j => j.Estatisticas)
            .HasForeignKey(e => e.IdJogador)
            .OnDelete(DeleteBehavior.Cascade);

        m.Entity<EstatisticasPartida>()
            .HasIndex(e => new { e.IdPartida, e.IdJogador })
            .IsUnique();

        /* ------------------------------------------------
           NOTÍCIA  (opcionalmente ligada a uma Liga)
        ------------------------------------------------ */
        m.Entity<Noticia>()
             .HasOne(n => n.Liga)        // navegação de Noticia → Liga
             .WithMany(l => l.Noticias)  // (adicione a coleção em Liga ou use WithMany())
             .HasForeignKey(n => n.IdLiga)       // FK é IdLiga (int?)
             .OnDelete(DeleteBehavior.SetNull);  // se liga for excluída, notícia permanece

        m.Entity<Noticia>()
            .Property(n => n.DataPublicacao)
            .HasDefaultValueSql("GETUTCDATE()");

        // Você pode criar um índice para ordenar rápido por data:
        m.Entity<Noticia>()
            .HasIndex(n => n.DataPublicacao);

    }
}
