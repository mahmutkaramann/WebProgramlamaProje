using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YeniSalon.Models;

namespace YeniSalon.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet'ler
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Uzmanlik> Uzmanliklar { get; set; }
        public DbSet<AntrenorUzmanlik> AntrenorUzmanliklar { get; set; }
        public DbSet<AntrenorHizmet> AntrenorHizmetler { get; set; }
        public DbSet<MusaitlikSaati> MusaitlikSaatleri { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<AIEgzersizOneri> AIEgzersizOnerileri { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Çoktan çoğa ilişkiler için composite key
            modelBuilder.Entity<AntrenorUzmanlik>()
                .HasKey(au => new { au.AntrenorId, au.UzmanlikId });

            modelBuilder.Entity<AntrenorHizmet>()
                .HasKey(ah => new { ah.AntrenorId, ah.HizmetId });

            // Benzersiz (Unique) kısıtlamalar
            modelBuilder.Entity<Antrenor>()
                .HasIndex(a => a.TCKimlikNo)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.TCKimlikNo)
                .IsUnique();

            // İlişkilerin konfigürasyonu
            modelBuilder.Entity<Antrenor>()
                .HasOne(a => a.Salon)
                .WithMany(s => s.Antrenorler)
                .HasForeignKey(a => a.SalonId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Hizmet>()
                .HasOne(h => h.Salon)
                .WithMany(s => s.HizmetTurleri)
                .HasForeignKey(h => h.SalonId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MusaitlikSaati>()
                .HasOne(m => m.Antrenor)
                .WithMany(a => a.MusaitlikSaatleri)
                .HasForeignKey(m => m.AntrenorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Kullanici)
                .WithMany(u => u.Randevular)
                .HasForeignKey(r => r.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Antrenor)
                .WithMany(a => a.Randevular)
                .HasForeignKey(r => r.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Data
            modelBuilder.Entity<Uzmanlik>().HasData(
                new Uzmanlik { UzmanlikId = 1, UzmanlikAdi = "Kas Geliştirme" },
                new Uzmanlik { UzmanlikId = 2, UzmanlikAdi = "Kilo Verme" },
                new Uzmanlik { UzmanlikId = 3, UzmanlikAdi = "Fonksiyonel Antrenman" },
                new Uzmanlik { UzmanlikId = 4, UzmanlikAdi = "Yoga" },
                new Uzmanlik { UzmanlikId = 5, UzmanlikAdi = "Pilates" }
            );

            modelBuilder.Entity<Salon>().HasData(
                new Salon
                {
                    SalonId = 1,
                    SalonAdi = "Merkez Spor Salonu",
                    Adres = "Serdivan / Sakarya",
                    Telefon = "(555) 555 55 55",
                    Email = "merkez@sporsalonu.com",
                    AcilisSaati = TimeSpan.FromHours(6),
                    KapanisSaati = TimeSpan.FromHours(23)
                }
            );

            modelBuilder.Entity<Hizmet>().HasData(
                new Hizmet
                {
                    HizmetId = 1,
                    HizmetAdi = "Kişisel Antrenman",
                    Aciklama = "Bire bir antrenör eşliğinde kişiye özel antrenman programı",
                    SureDakika = 60,
                    Ucret = 150.00m,
                    Kategori = "Fitness",
                    SalonId = 1,
                    AktifMi = true
                },
                new Hizmet
                {
                    HizmetId = 2,
                    HizmetAdi = "Grup Fitness",
                    Aciklama = "Grup halinde yapılan fitness dersi",
                    SureDakika = 45,
                    Ucret = 50.00m,
                    Kategori = "Fitness",
                    SalonId = 1,
                    AktifMi = true
                },
                new Hizmet
                {
                    HizmetId = 3,
                    HizmetAdi = "Yoga Dersi",
                    Aciklama = "Esneklik ve rahatlama için yoga seansı",
                    SureDakika = 60,
                    Ucret = 80.00m,
                    Kategori = "Yoga",
                    SalonId = 1,
                    AktifMi = true
                },
                new Hizmet
                {
                    HizmetId = 4,
                    HizmetAdi = "Boks Dersi",
                    Aciklama = "Profesyonel boks dersi.",
                    SureDakika = 90,
                    Ucret = 130.00m,
                    Kategori = "Boks",
                    SalonId = 1,
                    AktifMi = true
                },
                new Hizmet
                {
                    HizmetId = 5,
                    HizmetAdi = "Pilates Dersi",
                    Aciklama = "Esneklik ve rahatlama için Pilates seansı",
                    SureDakika = 60,
                    Ucret = 100.00m,
                    Kategori = "Pilates",
                    SalonId = 1,
                    AktifMi = true
                },
                new Hizmet
                {
                    HizmetId = 6,
                    HizmetAdi = "Karate Dersi",
                    Aciklama = "Alanında uzman eğitmen ile karate dersi",
                    SureDakika = 75,
                    Ucret = 120.00m,
                    Kategori = "Karate",
                    SalonId = 1,
                    AktifMi = true
                }
            );
        }
    }
}