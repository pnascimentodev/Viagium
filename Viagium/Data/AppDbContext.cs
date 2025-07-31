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
    public DbSet<PackageSchedule> PackageSchedules { get; set; }
    public DbSet<Traveler> Travelers { get; set; }
    public DbSet<TravelPackageHistory> TravelPackageHistory { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<ReservationRoom> ReservationRooms { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Affiliate> Affiliates { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<RoomTypeAmenity> RoomTypeAmenities { get; set; }
    public DbSet<HotelAmenity> HotelAmenities { get; set; }


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
            .WithMany(tp => tp.Reservations) // Corrigido: define o lado "muitos" explicitamente
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
            .WithMany(r => r.ReservationRooms)
            .HasForeignKey(rr => rr.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento Affiliate - Hotel (1:N)
        modelBuilder.Entity<Hotel>()
            .HasOne(h => h.Affiliate)
            .WithMany(a => a.Hotels)
            .HasForeignKey(h => h.AffiliateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento Reservation - Payment (1:1)
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Payment)
            .WithOne(p => p.Reservation)
            .HasForeignKey<Reservation>(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false); // Tornando o relacionamento opcional

        // Relacionamento Reservation - Hotel (N:1) - Novo relacionamento direto
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Hotel)
            .WithMany(h => h.Reservations)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false); // Opcional porque pode ser nulo

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

        //Relacionamento Afilliate - Address (1:1)
        modelBuilder.Entity<Affiliate>()
            .HasOne(a => a.Address)
            .WithOne(ad => ad.Affiliate)
            .HasForeignKey<Affiliate>(a => a.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Hotel>()
            .HasOne(h => h.Address)
            .WithOne(ad => ad.Hotel)
            .HasForeignKey<Hotel>(h => h.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        //Relacionamento entre RoomType e Room (1:N)
        modelBuilder.Entity<RoomType>()
            .HasMany(rt => rt.Rooms)
            .WithOne(r => r.RoomType)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //Relactionamento entre TravelPackage OriginAddress e DestinationAddress (1:1)
        modelBuilder.Entity<TravelPackage>()
            .HasOne(tp => tp.OriginAddress)
            .WithMany()
            .HasForeignKey(tp => tp.OriginAddressId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<TravelPackage>()
            .HasOne(tp => tp.DestinationAddress)
            .WithMany()
            .HasForeignKey(tp => tp.DestinationAddressId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<RoomTypeAmenity>()
            .HasKey(rta => new { rta.RoomTypeId, rta.AmenityId });

        modelBuilder.Entity<RoomTypeAmenity>()
            .HasOne(rta => rta.RoomType)
            .WithMany(rt => rt.RoomTypeAmenities)
            .HasForeignKey(rta => rta.RoomTypeId);

        modelBuilder.Entity<RoomTypeAmenity>()
            .HasOne(rta => rta.Amenity)
            .WithMany(a => a.RoomTypeAmenities)
            .HasForeignKey(rta => rta.AmenityId);
        
        modelBuilder.Entity<HotelAmenity>()
            .HasKey(ha => new { ha.HotelId, ha.AmenityId });

        modelBuilder.Entity<HotelAmenity>()
            .HasOne(ha => ha.Hotel)
            .WithMany(h => h.HotelAmenity)
            .HasForeignKey(ha => ha.HotelId);

        modelBuilder.Entity<HotelAmenity>()
            .HasOne(ha => ha.Amenity)
            .WithMany(a => a.HotelAmenity)
            .HasForeignKey(ha => ha.AmenityId);

        // Relacionamento TravelPackage - PackageSchedule (1:N)
        modelBuilder.Entity<PackageSchedule>()
            .HasOne(ps => ps.TravelPackage)
            .WithMany(tp => tp.PackageSchedules)
            .HasForeignKey(ps => ps.TravelPackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}