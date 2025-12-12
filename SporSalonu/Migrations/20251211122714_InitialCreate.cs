using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace YeniSalon.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TCKimlikNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cinsiyet = table.Column<int>(type: "int", nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Boy = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UyelikBaslangic = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UyelikBitis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UyelikTuru = table.Column<int>(type: "int", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    ProfilFotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Salonlar",
                columns: table => new
                {
                    SalonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalonAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcilisSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    KapanisSaati = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salonlar", x => x.SalonId);
                });

            migrationBuilder.CreateTable(
                name: "Uzmanliklar",
                columns: table => new
                {
                    UzmanlikId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UzmanlikAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uzmanliklar", x => x.UzmanlikId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIEgzersizOnerileri",
                columns: table => new
                {
                    OneriId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IstekTipi = table.Column<int>(type: "int", nullable: false),
                    GirilenBilgi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Boy = table.Column<int>(type: "int", nullable: true),
                    Kilo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Yas = table.Column<int>(type: "int", nullable: true),
                    Cinsiyet = table.Column<int>(type: "int", nullable: true),
                    Hedef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIYaniti = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AIGorselUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIEgzersizOnerileri", x => x.OneriId);
                    table.ForeignKey(
                        name: "FK_AIEgzersizOnerileri_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Antrenorler",
                columns: table => new
                {
                    AntrenorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TCKimlikNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cinsiyet = table.Column<int>(type: "int", nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeneyimYili = table.Column<int>(type: "int", nullable: false),
                    UzmanlikAlanlari = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HaftalikCalismaSaati = table.Column<int>(type: "int", nullable: true),
                    SaatlikUcret = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProfilFotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    SalonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Antrenorler", x => x.AntrenorId);
                    table.ForeignKey(
                        name: "FK_Antrenorler_Salonlar_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salonlar",
                        principalColumn: "SalonId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Hizmetler",
                columns: table => new
                {
                    HizmetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HizmetAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SureDakika = table.Column<int>(type: "int", nullable: false),
                    Ucret = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    SalonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hizmetler", x => x.HizmetId);
                    table.ForeignKey(
                        name: "FK_Hizmetler_Salonlar_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salonlar",
                        principalColumn: "SalonId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AntrenorUzmanliklar",
                columns: table => new
                {
                    AntrenorId = table.Column<int>(type: "int", nullable: false),
                    UzmanlikId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntrenorUzmanliklar", x => new { x.AntrenorId, x.UzmanlikId });
                    table.ForeignKey(
                        name: "FK_AntrenorUzmanliklar_Antrenorler_AntrenorId",
                        column: x => x.AntrenorId,
                        principalTable: "Antrenorler",
                        principalColumn: "AntrenorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AntrenorUzmanliklar_Uzmanliklar_UzmanlikId",
                        column: x => x.UzmanlikId,
                        principalTable: "Uzmanliklar",
                        principalColumn: "UzmanlikId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusaitlikSaatleri",
                columns: table => new
                {
                    MusaitlikId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AntrenorId = table.Column<int>(type: "int", nullable: false),
                    Gun = table.Column<int>(type: "int", nullable: false),
                    BaslangicSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    BitisSaati = table.Column<TimeSpan>(type: "time", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusaitlikSaatleri", x => x.MusaitlikId);
                    table.ForeignKey(
                        name: "FK_MusaitlikSaatleri_Antrenorler_AntrenorId",
                        column: x => x.AntrenorId,
                        principalTable: "Antrenorler",
                        principalColumn: "AntrenorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AntrenorHizmetler",
                columns: table => new
                {
                    AntrenorId = table.Column<int>(type: "int", nullable: false),
                    HizmetId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntrenorHizmetler", x => new { x.AntrenorId, x.HizmetId });
                    table.ForeignKey(
                        name: "FK_AntrenorHizmetler_Antrenorler_AntrenorId",
                        column: x => x.AntrenorId,
                        principalTable: "Antrenorler",
                        principalColumn: "AntrenorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AntrenorHizmetler_Hizmetler_HizmetId",
                        column: x => x.HizmetId,
                        principalTable: "Hizmetler",
                        principalColumn: "HizmetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    RandevuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AntrenorId = table.Column<int>(type: "int", nullable: false),
                    HizmetId = table.Column<int>(type: "int", nullable: false),
                    RandevuTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Not = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.RandevuId);
                    table.ForeignKey(
                        name: "FK_Randevular_Antrenorler_AntrenorId",
                        column: x => x.AntrenorId,
                        principalTable: "Antrenorler",
                        principalColumn: "AntrenorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Randevular_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Randevular_Hizmetler_HizmetId",
                        column: x => x.HizmetId,
                        principalTable: "Hizmetler",
                        principalColumn: "HizmetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Salonlar",
                columns: new[] { "SalonId", "AcilisSaati", "Adres", "Email", "KapanisSaati", "SalonAdi", "Telefon" },
                values: new object[] { 1, new TimeSpan(0, 6, 0, 0, 0), "Serdivan / Sakarya", "merkez@sporsalonu.com", new TimeSpan(0, 23, 0, 0, 0), "Merkez Spor Salonu", "(555) 555 55 55" });

            migrationBuilder.InsertData(
                table: "Uzmanliklar",
                columns: new[] { "UzmanlikId", "UzmanlikAdi" },
                values: new object[,]
                {
                    { 1, "Kas Geliştirme" },
                    { 2, "Kilo Verme" },
                    { 3, "Fonksiyonel Antrenman" },
                    { 4, "Yoga" },
                    { 5, "Pilates" }
                });

            migrationBuilder.InsertData(
                table: "Hizmetler",
                columns: new[] { "HizmetId", "Aciklama", "AktifMi", "HizmetAdi", "Kategori", "SalonId", "SureDakika", "Ucret" },
                values: new object[,]
                {
                    { 1, "Bire bir antrenör eşliğinde kişiye özel antrenman programı", true, "Kişisel Antrenman", "Fitness", 1, 60, 150.00m },
                    { 2, "Grup halinde yapılan fitness dersi", true, "Grup Fitness", "Fitness", 1, 45, 50.00m },
                    { 3, "Esneklik ve rahatlama için yoga seansı", true, "Yoga Dersi", "Yoga", 1, 60, 80.00m },
                    { 4, "Profesyonel boks dersi.", true, "Boks Dersi", "Boks", 1, 90, 130.00m },
                    { 5, "Esneklik ve rahatlama için Pilates seansı", true, "Pilates Dersi", "Pilates", 1, 60, 100.00m },
                    { 6, "Alanında uzman eğitmen ile karate dersi", true, "Karate Dersi", "Karate", 1, 75, 120.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIEgzersizOnerileri_KullaniciId",
                table: "AIEgzersizOnerileri",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_AntrenorHizmetler_HizmetId",
                table: "AntrenorHizmetler",
                column: "HizmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Antrenorler_SalonId",
                table: "Antrenorler",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Antrenorler_TCKimlikNo",
                table: "Antrenorler",
                column: "TCKimlikNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AntrenorUzmanliklar_UzmanlikId",
                table: "AntrenorUzmanliklar",
                column: "UzmanlikId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TCKimlikNo",
                table: "AspNetUsers",
                column: "TCKimlikNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Hizmetler_SalonId",
                table: "Hizmetler",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_MusaitlikSaatleri_AntrenorId",
                table: "MusaitlikSaatleri",
                column: "AntrenorId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_AntrenorId",
                table: "Randevular",
                column: "AntrenorId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_HizmetId",
                table: "Randevular",
                column: "HizmetId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_KullaniciId",
                table: "Randevular",
                column: "KullaniciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIEgzersizOnerileri");

            migrationBuilder.DropTable(
                name: "AntrenorHizmetler");

            migrationBuilder.DropTable(
                name: "AntrenorUzmanliklar");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "MusaitlikSaatleri");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "Uzmanliklar");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Antrenorler");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Hizmetler");

            migrationBuilder.DropTable(
                name: "Salonlar");
        }
    }
}
