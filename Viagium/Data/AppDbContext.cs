using Microsoft.EntityFrameworkCore;

using Viagium.Models;

namespace Viagium.Data;

public class AppDbContext : DbContext
{
    // Construtor que recebe as opções de configuração do DbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<TravelPackage> TravelPackages { get; set; }
    public DbSet<Traveler> Travelers { get; set; }
    public DbSet<TravelPackageHistory> TravelPackageHistory { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<ReservationRoom> ReservationRooms { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    
    // Configura o modelo do banco de dados
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configura o User entity
        modelBuilder.Entity<Payment>()// Configura o Payment entity
            .Property(p => p.Amount) // Configura a propriedade Amount do Payment entity
            .HasPrecision(18, 2); // Define a precisão para 18 dígitos no total e 2 dígitos decimais

        // Configura o TravelPackage entity
        modelBuilder.Entity<Reservation>()// Configura o Reservation entity
            .Property(r => r.TotalPrice) // Configura a propriedade TotalPrice do Reservation entity
            .HasPrecision(18, 2); // Define a precisão para 18 dígitos no total e 2 dígitos decimais
    
        // Configura o TravelPackageHistory entity
        modelBuilder.Entity<TravelPackage>()// Configura o TravelPackage entity
            .Property(tp => tp.Price) // Configura a propriedade Price do TravelPackage entity
            .HasPrecision(18, 2); // Define a precisão para 18 dígitos no total e 2 dígitos decimais

        // Configura o TravelPackageHistory entity
        modelBuilder.Entity<TravelPackageHistory>()// Configura o TravelPackageHistory entity
            .Property(tph => tph.Price) // Configura a propriedade Price do TravelPackageHistory entity
            .HasPrecision(18, 2); // Define a precisão para 18 dígitos no total e 2 dígitos decimais

        // Configura o Review entity
        modelBuilder.Entity<Reservation>()// Configura o Reservation entity
            .HasOne(r => r.User)// Configura a relação entre Reservation e User
            .WithMany(u => u.Reservations) // Um User pode ter várias Reservations
            .HasForeignKey(r => r.UserId) // Chave estrangeira UserId na Reservation
            .OnDelete(DeleteBehavior.Restrict);

        // Configura o Review entity
        modelBuilder.Entity<Reservation>() // Configura o Reservation entity
            .HasOne(r => r.TravelPackage) // Configura a relação entre Reservation e TravelPackage
            .WithMany() // Um TravelPackage pode ter várias Reservations
            .HasForeignKey(r => r.TravelPackageId) // Chave estrangeira TravelPackageId na Reservation
            .OnDelete(DeleteBehavior.Restrict); // Impede a exclusão de TravelPackage se houver Reservations associadas

        // Hotel - RoomType (1:N)
        modelBuilder.Entity<RoomType>()
            .HasOne(rt => rt.Hotel)
            .WithMany()
            .HasForeignKey(rt => rt.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        // RoomType - ReservationRoom (1:N)
        modelBuilder.Entity<ReservationRoom>()
            .HasOne(rr => rr.RoomType)
            .WithMany()
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reservation - ReservationRoom (1:N)
        modelBuilder.Entity<ReservationRoom>()
            .HasOne(rr => rr.Reservation)
            .WithMany()
            .HasForeignKey(r => r.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento TravelPackage - Hotel (N:1)
        modelBuilder.Entity<TravelPackage>()
            .HasOne(tp => tp.Hotel)
            .WithMany()
            .HasForeignKey(tp => tp.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimais para RoomType e ReservationRoom
        modelBuilder.Entity<RoomType>()
            .Property(rt => rt.PricePerNight)
            .HasPrecision(18, 2);
        modelBuilder.Entity<ReservationRoom>()
            .Property(rr => rr.PricePerNight)
            .HasPrecision(18, 2);
        modelBuilder.Entity<ReservationRoom>()
            .Property(rr => rr.TotalPrice)
            .HasPrecision(18, 2);
        
       
    }
}