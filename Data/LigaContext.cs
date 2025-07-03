using LBAChamps.Models;
using Microsoft.EntityFrameworkCore;

namespace LBAChamps.Data;


public class LigaContext : DbContext
{
    public LigaContext(DbContextOptions<LigaContext> options) : base(options) { }

    public DbSet<Liga> Ligas => Set<Liga>();
    public DbSet<Time> Times => Set<Time>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Partida> Partidas => Set<Partida>();
    public DbSet<Jogador> Jogadores => Set<Jogador>();
    public DbSet<EstatisticasPartida> EstatisticasPartidas => Set<EstatisticasPartida>();
    public DbSet<Noticia> Noticias => Set<Noticia>();


    protected override void OnModelCreating(ModelBuilder m)
    {

        m.Entity<Liga>()
            .HasIndex(l => new { l.Nome, l.Esporte })
            .IsUnique();


        m.Entity<Time>()
            .HasOne(t => t.Liga)
            .WithMany(l => l.Times)
            .HasForeignKey(t => t.IdLiga)
            .OnDelete(DeleteBehavior.Restrict);


        m.Entity<Jogador>()
            .HasOne(j => j.Time)
            .WithMany(t => t.Jogadores)
            .HasForeignKey(j => j.IdTime)
            .OnDelete(DeleteBehavior.Cascade);

        m.Entity<Jogador>()
         .HasIndex(j => new { j.IdTime, j.NumeroCamisa })
         .IsUnique();


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


        m.Entity<Noticia>()
             .HasOne(n => n.Liga)
             .WithMany(l => l.Noticias)
             .HasForeignKey(n => n.IdLiga)
             .OnDelete(DeleteBehavior.SetNull);

        m.Entity<Noticia>()
            .Property(n => n.DataPublicacao)
            .HasDefaultValueSql("GETUTCDATE()");


        m.Entity<Noticia>()
            .HasIndex(n => n.DataPublicacao);

    }
}
