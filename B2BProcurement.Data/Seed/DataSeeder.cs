using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Data.Seed
{
    /// <summary>
    /// Veritabanı için demo verileri oluşturur.
    /// </summary>
    public static class DataSeeder
    {
        /// <summary>
        /// Demo verileri yükler.
        /// </summary>
        /// <param name="context">Veritabanı bağlamı.</param>
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Eğer veri varsa seed yapma
            if (await context.Sectors.AnyAsync())
                return;

            // Sektörleri ekle
            var sectors = await SeedSectorsAsync(context);

            // Paketleri ekle
            var packages = await SeedPackagesAsync(context);

            // Şirketleri ekle
            var companies = await SeedCompaniesAsync(context, sectors, packages);

            // Her şirket için kullanıcı, malzeme ve tedarikçi ekle
            await SeedUsersAsync(context, companies);
            await SeedMaterialsAsync(context, companies, sectors);
            await SeedSuppliersAsync(context, companies, sectors);

            // RFQ ve Quotation ekle
            await SeedRfqAndQuotationsAsync(context, companies, sectors);
        }

        #region Sectors

        private static async Task<List<Sector>> SeedSectorsAsync(ApplicationDbContext context)
        {
            var sectors = new List<Sector>
            {
                new Sector { Name = "Otomotiv", NameEn = "Automotive", Code = "OTO", Description = "Otomotiv yan sanayi ve yedek parça sektörü" },
                new Sector { Name = "Tekstil", NameEn = "Textile", Code = "TKS", Description = "Tekstil ve konfeksiyon sektörü" },
                new Sector { Name = "Gıda", NameEn = "Food", Code = "GDA", Description = "Gıda üretimi ve işleme sektörü" },
                new Sector { Name = "İnşaat", NameEn = "Construction", Code = "INS", Description = "İnşaat ve yapı malzemeleri sektörü" },
                new Sector { Name = "Elektronik", NameEn = "Electronics", Code = "ELK", Description = "Elektronik ve elektrik sektörü" },
                new Sector { Name = "Kimya", NameEn = "Chemistry", Code = "KIM", Description = "Kimya ve petrokimya sektörü" }
            };

            await context.Sectors.AddRangeAsync(sectors);
            await context.SaveChangesAsync();

            return sectors;
        }

        #endregion

        #region Packages

        private static async Task<List<Package>> SeedPackagesAsync(ApplicationDbContext context)
        {
            var packages = new List<Package>
            {
                new Package
                {
                    Name = "Temel",
                    Price = 500m,
                    MaxUsers = 2,
                    MaxMaterials = 100,
                    MaxRfqPerMonth = 10,
                    CanUseSapIntegration = false
                },
                new Package
                {
                    Name = "Orta",
                    Price = 1500m,
                    MaxUsers = 5,
                    MaxMaterials = 500,
                    MaxRfqPerMonth = 50,
                    CanUseSapIntegration = false
                },
                new Package
                {
                    Name = "Premium",
                    Price = 3500m,
                    MaxUsers = 0, // Sınırsız
                    MaxMaterials = 0, // Sınırsız
                    MaxRfqPerMonth = 0, // Sınırsız
                    CanUseSapIntegration = true
                }
            };

            await context.Packages.AddRangeAsync(packages);
            await context.SaveChangesAsync();

            return packages;
        }

        #endregion

        #region Companies

        private static async Task<List<Company>> SeedCompaniesAsync(
            ApplicationDbContext context, 
            List<Sector> sectors, 
            List<Package> packages)
        {
            var otomotiv = sectors.First(s => s.Code == "OTO");
            var tekstil = sectors.First(s => s.Code == "TKS");
            var insaat = sectors.First(s => s.Code == "INS");

            var temel = packages.First(p => p.Name == "Temel");
            var orta = packages.First(p => p.Name == "Orta");
            var premium = packages.First(p => p.Name == "Premium");

            var companies = new List<Company>
            {
                new Company
                {
                    CompanyName = "ABC Otomotiv A.Ş.",
                    TaxNumber = "1234567890",
                    TaxOffice = "Kadıköy VD",
                    Address = "Organize Sanayi Bölgesi 1. Cadde No:15",
                    City = "İstanbul",
                    Phone = "+90 216 555 0001",
                    Email = "info@abcotomotiv.com",
                    SectorId = otomotiv.Id,
                    PackageId = premium.Id
                },
                new Company
                {
                    CompanyName = "XYZ Tekstil Ltd. Şti.",
                    TaxNumber = "2345678901",
                    TaxOffice = "Bursa VD",
                    Address = "BOSB 5. Sokak No:42",
                    City = "Bursa",
                    Phone = "+90 224 555 0002",
                    Email = "info@xyztekstil.com",
                    SectorId = tekstil.Id,
                    PackageId = orta.Id
                },
                new Company
                {
                    CompanyName = "Demo Metal San. Tic. Ltd. Şti.",
                    TaxNumber = "3456789012",
                    TaxOffice = "Ankara VD",
                    Address = "1. OSB 12. Cadde No:8",
                    City = "Ankara",
                    Phone = "+90 312 555 0003",
                    Email = "info@demometal.com",
                    SectorId = insaat.Id,
                    PackageId = temel.Id
                }
            };

            await context.Companies.AddRangeAsync(companies);
            await context.SaveChangesAsync();

            return companies;
        }

        #endregion

        #region Users

        private static async Task SeedUsersAsync(ApplicationDbContext context, List<Company> companies)
        {
            var users = new List<User>();

            foreach (var company in companies)
            {
                var companyPrefix = company.CompanyName.Split(' ')[0].ToLower();
                users.Add(new User
                {
                    Email = $"admin@{companyPrefix}.com",
                    PasswordHash = BCryptHash("Demo123!"), // Demo için basit hash
                    FirstName = "Admin",
                    LastName = company.CompanyName.Split(' ')[0],
                    Phone = company.Phone,
                    Role = UserRole.CompanyAdmin,
                    CompanyId = company.Id
                });
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        private static string BCryptHash(string password)
        {
            // Demo için basit hash - gerçek projede BCrypt kullanılmalı
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password + "_hashed"));
        }

        #endregion

        #region Materials

        private static async Task SeedMaterialsAsync(
            ApplicationDbContext context, 
            List<Company> companies, 
            List<Sector> sectors)
        {
            var materials = new List<Material>();

            // ABC Otomotiv için malzemeler
            var abcCompany = companies.First(c => c.CompanyName.Contains("ABC"));
            var otomotiv = sectors.First(s => s.Code == "OTO");
            var otomotivMalzemeler = new[]
            {
                ("OTO-001", "Fren Balatası", "Adet"),
                ("OTO-002", "Motor Yağı 10W40", "Litre"),
                ("OTO-003", "Hava Filtresi", "Adet"),
                ("OTO-004", "Akü 12V 60Ah", "Adet"),
                ("OTO-005", "V Kayışı", "Adet")
            };
            foreach (var (code, name, unit) in otomotivMalzemeler)
            {
                materials.Add(new Material { Code = code, Name = name, Unit = unit, CompanyId = abcCompany.Id, SectorId = otomotiv.Id });
            }

            // XYZ Tekstil için malzemeler
            var xyzCompany = companies.First(c => c.CompanyName.Contains("XYZ"));
            var tekstil = sectors.First(s => s.Code == "TKS");
            var tekstilMalzemeler = new[]
            {
                ("TKS-001", "Pamuk İplik", "Kg"),
                ("TKS-002", "Polyester Kumaş", "Metre"),
                ("TKS-003", "Fermuar YKK", "Adet"),
                ("TKS-004", "Düğme Plastik", "Adet"),
                ("TKS-005", "Etiket Dokuma", "Adet")
            };
            foreach (var (code, name, unit) in tekstilMalzemeler)
            {
                materials.Add(new Material { Code = code, Name = name, Unit = unit, CompanyId = xyzCompany.Id, SectorId = tekstil.Id });
            }

            // Demo Metal için malzemeler
            var demoCompany = companies.First(c => c.CompanyName.Contains("Demo"));
            var insaat = sectors.First(s => s.Code == "INS");
            var insaatMalzemeler = new[]
            {
                ("INS-001", "Nervürlü Demir 12mm", "Ton"),
                ("INS-002", "Çimento CEM II", "Ton"),
                ("INS-003", "Kum 0-4mm", "M3"),
                ("INS-004", "Tuğla 19x9x5", "Adet"),
                ("INS-005", "Beton Pompası Kiralama", "Gün")
            };
            foreach (var (code, name, unit) in insaatMalzemeler)
            {
                materials.Add(new Material { Code = code, Name = name, Unit = unit, CompanyId = demoCompany.Id, SectorId = insaat.Id });
            }

            await context.Materials.AddRangeAsync(materials);
            await context.SaveChangesAsync();
        }

        #endregion

        #region Suppliers

        private static async Task SeedSuppliersAsync(
            ApplicationDbContext context, 
            List<Company> companies, 
            List<Sector> sectors)
        {
            var suppliers = new List<Supplier>();

            // Her firma için 3 tedarikçi
            foreach (var company in companies)
            {
                for (int i = 1; i <= 3; i++)
                {
                    suppliers.Add(new Supplier
                    {
                        Name = $"Tedarikçi {company.CompanyName.Split(' ')[0]} - {i}",
                        TaxNumber = $"99{company.Id}000{i}",
                        Email = $"supplier{i}@example.com",
                        Phone = $"+90 555 {100 + company.Id}{i}",
                        CompanyId = company.Id,
                        SectorId = company.SectorId
                    });
                }
            }

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }

        #endregion

        #region RFQ & Quotations

        private static async Task SeedRfqAndQuotationsAsync(
            ApplicationDbContext context, 
            List<Company> companies, 
            List<Sector> sectors)
        {
            var abcCompany = companies.First(c => c.CompanyName.Contains("ABC"));
            var xyzCompany = companies.First(c => c.CompanyName.Contains("XYZ"));
            var demoCompany = companies.First(c => c.CompanyName.Contains("Demo"));
            var otomotiv = sectors.First(s => s.Code == "OTO");

            // ABC'den RFQ oluştur
            var rfq = new RFQ
            {
                RfqNumber = "RFQ-2026-0001",
                Title = "2026 Yılı Otomotiv Yedek Parça Alımı",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Currency = "TRY",
                Status = RfqStatus.Published,
                Visibility = RfqVisibility.AllSector,
                CompanyId = abcCompany.Id,
                SectorId = otomotiv.Id
            };

            await context.RFQs.AddAsync(rfq);
            await context.SaveChangesAsync();

            // RFQ Kalemleri
            var rfqItems = new List<RFQItem>
            {
                new RFQItem
                {
                    RfqId = rfq.Id,
                    Description = "Fren Balatası Ön Set",
                    Quantity = 500,
                    Unit = "Set",
                    TechnicalSpecs = "Aşınma sensörlü, OEM kalite, araç modelleri: Fiat, Ford, VW"
                },
                new RFQItem
                {
                    RfqId = rfq.Id,
                    Description = "Motor Yağı 10W40 Sentetik",
                    Quantity = 1000,
                    Unit = "Litre",
                    TechnicalSpecs = "API SN/CF, ACEA A3/B4, VW 502.00/505.00"
                },
                new RFQItem
                {
                    RfqId = rfq.Id,
                    Description = "Hava Filtresi Universal",
                    Quantity = 200,
                    Unit = "Adet",
                    TechnicalSpecs = "Panel tip, 250x200x50mm, filtrasyon: 99.5%"
                }
            };

            await context.RFQItems.AddRangeAsync(rfqItems);
            await context.SaveChangesAsync();

            // RFQ İletişim Kişisi
            var rfqContact = new RFQContact
            {
                RfqId = rfq.Id,
                Name = "Ahmet Yılmaz",
                Email = "ahmet.yilmaz@abcotomotiv.com",
                Phone = "+90 532 555 0001"
            };

            await context.RFQContacts.AddAsync(rfqContact);
            await context.SaveChangesAsync();

            // Quotation 1 - XYZ'den
            var quotation1 = new Quotation
            {
                QuotationNumber = "QUO-2026-0001",
                RfqId = rfq.Id,
                SupplierCompanyId = xyzCompany.Id,
                Status = QuotationStatus.Submitted,
                TotalAmount = 0,
                ValidUntil = DateTime.Now.AddDays(45)
            };

            await context.Quotations.AddAsync(quotation1);
            await context.SaveChangesAsync();

            // Quotation 1 Kalemleri
            var quo1Items = new List<QuotationItem>
            {
                new QuotationItem { QuotationId = quotation1.Id, RfqItemId = rfqItems[0].Id, UnitPrice = 250m, OfferedQuantity = 500, TotalPrice = 125000m, DeliveryDate = DateTime.Now.AddDays(7), ApprovalStatus = ApprovalStatus.Pending },
                new QuotationItem { QuotationId = quotation1.Id, RfqItemId = rfqItems[1].Id, UnitPrice = 45m, OfferedQuantity = 1000, TotalPrice = 45000m, DeliveryDate = DateTime.Now.AddDays(5), ApprovalStatus = ApprovalStatus.Pending },
                new QuotationItem { QuotationId = quotation1.Id, RfqItemId = rfqItems[2].Id, UnitPrice = 85m, OfferedQuantity = 200, TotalPrice = 17000m, DeliveryDate = DateTime.Now.AddDays(10), ApprovalStatus = ApprovalStatus.Pending }
            };

            await context.QuotationItems.AddRangeAsync(quo1Items);
            quotation1.TotalAmount = quo1Items.Sum(i => i.TotalPrice);
            await context.SaveChangesAsync();

            // Quotation 2 - Demo'dan
            var quotation2 = new Quotation
            {
                QuotationNumber = "QUO-2026-0002",
                RfqId = rfq.Id,
                SupplierCompanyId = demoCompany.Id,
                Status = QuotationStatus.Submitted,
                TotalAmount = 0,
                ValidUntil = DateTime.Now.AddDays(30)
            };

            await context.Quotations.AddAsync(quotation2);
            await context.SaveChangesAsync();

            // Quotation 2 Kalemleri
            var quo2Items = new List<QuotationItem>
            {
                new QuotationItem { QuotationId = quotation2.Id, RfqItemId = rfqItems[0].Id, UnitPrice = 220m, OfferedQuantity = 500, TotalPrice = 110000m, DeliveryDate = DateTime.Now.AddDays(14), ApprovalStatus = ApprovalStatus.Pending },
                new QuotationItem { QuotationId = quotation2.Id, RfqItemId = rfqItems[1].Id, UnitPrice = 50m, OfferedQuantity = 1000, TotalPrice = 50000m, DeliveryDate = DateTime.Now.AddDays(7), ApprovalStatus = ApprovalStatus.Pending },
                new QuotationItem { QuotationId = quotation2.Id, RfqItemId = rfqItems[2].Id, UnitPrice = 75m, OfferedQuantity = 200, TotalPrice = 15000m, DeliveryDate = DateTime.Now.AddDays(7), ApprovalStatus = ApprovalStatus.Pending }
            };

            await context.QuotationItems.AddRangeAsync(quo2Items);
            quotation2.TotalAmount = quo2Items.Sum(i => i.TotalPrice);
            await context.SaveChangesAsync();
        }

        #endregion
    }
}
