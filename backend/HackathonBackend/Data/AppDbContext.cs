using Microsoft.EntityFrameworkCore;
using HackathonBackend.Models;

namespace HackathonBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Pharmacy tables
        public DbSet<User> Users { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Stretch tables
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

        // Keep old skeleton entity so existing migration still compiles
        public DbSet<Entity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Decimal precision ----
            modelBuilder.Entity<Medicine>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.FinalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Promotion>()
                .Property(p => p.DiscountValue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Promotion>()
                .Property(p => p.MinimumOrderValue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Promotion>()
                .Property(p => p.MaxDiscountAmount)
                .HasColumnType("decimal(18,2)");

            // ---- Indexes / uniqueness ----
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Promotion>()
                .HasIndex(p => p.PromotionCode)
                .IsUnique();

            // ---- Seed default admin (with email so register page demo works) ----
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                Password = "admin123",
                Role = "Admin",
                Email = "admin@bytebrigade.local",
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "0000000000",
                LoyaltyPoints = 0
            });

            // ---- Seed two sample promo codes ----
            modelBuilder.Entity<Promotion>().HasData(
                new Promotion
                {
                    Id = 1,
                    PromotionCode = "WELCOME10",
                    Description = "10% off your order",
                    DiscountType = "Percentage",
                    DiscountValue = 10m,
                    MinimumOrderValue = 0m,
                    MaxDiscountAmount = 200m,
                    StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    UsageLimit = null,
                    UsedCount = 0
                },
                new Promotion
                {
                    Id = 2,
                    PromotionCode = "FLAT50",
                    Description = "Flat ₹50 off on orders above ₹500",
                    DiscountType = "FixedAmount",
                    DiscountValue = 50m,
                    MinimumOrderValue = 500m,
                    MaxDiscountAmount = null,
                    StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    UsageLimit = null,
                    UsedCount = 0
                });

            // ---- Seed ~50 medicines (categories, dosage forms, packaging) ----
            SeedMedicines(modelBuilder);
        }

        private static void SeedMedicines(ModelBuilder modelBuilder)
        {
            var meds = new List<Medicine>
            {
                new() { Id = 1, Name = "Dolo 650", Category = "Pain Relief", Manufacturer = "Micro Labs", Composition = "Paracetamol 650mg", DosageForm = "Tablet", Strength = "650mg", PackagingType = "Strip of 15", Price = 35m, StockQuantity = 250, Description = "Effective relief from fever and mild to moderate pain.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400" },
                new() { Id = 2, Name = "Crocin Advance", Category = "Pain Relief", Manufacturer = "GSK", Composition = "Paracetamol 500mg", DosageForm = "Tablet", Strength = "500mg", PackagingType = "Strip of 15", Price = 30m, StockQuantity = 200, Description = "Fast-acting fever and headache relief.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400" },
                new() { Id = 3, Name = "Cetirizine 10mg", Category = "Allergy", Manufacturer = "Cipla", Composition = "Cetirizine HCl 10mg", DosageForm = "Tablet", Strength = "10mg", PackagingType = "Strip of 10", Price = 25m, StockQuantity = 300, Description = "Antihistamine for allergic rhinitis and skin allergies.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400" },
                new() { Id = 4, Name = "Amoxicillin 500mg", Category = "Antibiotic", Manufacturer = "Cipla", Composition = "Amoxicillin Trihydrate 500mg", DosageForm = "Capsule", Strength = "500mg", PackagingType = "Strip of 10", Price = 85m, StockQuantity = 150, Description = "Broad-spectrum antibiotic for bacterial infections.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400" },
                new() { Id = 5, Name = "Azithromycin 500mg", Category = "Antibiotic", Manufacturer = "Sun Pharma", Composition = "Azithromycin 500mg", DosageForm = "Tablet", Strength = "500mg", PackagingType = "Strip of 3", Price = 110m, StockQuantity = 120, Description = "Macrolide antibiotic for respiratory and skin infections.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1576602976047-174e57a47881?w=400" },
                new() { Id = 6, Name = "Ibuprofen 400mg", Category = "Pain Relief", Manufacturer = "Abbott", Composition = "Ibuprofen 400mg", DosageForm = "Tablet", Strength = "400mg", PackagingType = "Strip of 10", Price = 40m, StockQuantity = 220, Description = "Anti-inflammatory pain reliever.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 7, Name = "Pantoprazole 40mg", Category = "Gastro", Manufacturer = "Sun Pharma", Composition = "Pantoprazole Sodium 40mg", DosageForm = "Tablet", Strength = "40mg", PackagingType = "Strip of 15", Price = 90m, StockQuantity = 180, Description = "Proton pump inhibitor for acidity and ulcers.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400" },
                new() { Id = 8, Name = "Omeprazole 20mg", Category = "Gastro", Manufacturer = "Dr. Reddy's", Composition = "Omeprazole 20mg", DosageForm = "Capsule", Strength = "20mg", PackagingType = "Strip of 10", Price = 75m, StockQuantity = 200, Description = "Reduces stomach acid production.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 9, Name = "Metformin 500mg", Category = "Diabetes", Manufacturer = "USV", Composition = "Metformin HCl 500mg", DosageForm = "Tablet", Strength = "500mg", PackagingType = "Strip of 15", Price = 50m, StockQuantity = 250, Description = "First-line therapy for type 2 diabetes.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400" },
                new() { Id = 10, Name = "Glimepiride 2mg", Category = "Diabetes", Manufacturer = "Sanofi", Composition = "Glimepiride 2mg", DosageForm = "Tablet", Strength = "2mg", PackagingType = "Strip of 10", Price = 95m, StockQuantity = 140, Description = "Sulfonylurea for type 2 diabetes control.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400" },
                new() { Id = 11, Name = "Atorvastatin 10mg", Category = "Cardiac", Manufacturer = "Pfizer", Composition = "Atorvastatin Calcium 10mg", DosageForm = "Tablet", Strength = "10mg", PackagingType = "Strip of 10", Price = 120m, StockQuantity = 160, Description = "Statin to lower cholesterol.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400" },
                new() { Id = 12, Name = "Amlodipine 5mg", Category = "Cardiac", Manufacturer = "Cipla", Composition = "Amlodipine Besilate 5mg", DosageForm = "Tablet", Strength = "5mg", PackagingType = "Strip of 10", Price = 60m, StockQuantity = 180, Description = "Calcium channel blocker for hypertension.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400" },
                new() { Id = 13, Name = "Telmisartan 40mg", Category = "Cardiac", Manufacturer = "Glenmark", Composition = "Telmisartan 40mg", DosageForm = "Tablet", Strength = "40mg", PackagingType = "Strip of 10", Price = 105m, StockQuantity = 150, Description = "Angiotensin II receptor blocker for BP.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 14, Name = "Vitamin C 500mg", Category = "Vitamins", Manufacturer = "HealthVit", Composition = "Ascorbic Acid 500mg", DosageForm = "Tablet", Strength = "500mg", PackagingType = "Bottle of 60", Price = 150m, StockQuantity = 300, Description = "Antioxidant immunity booster.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 15, Name = "Vitamin D3 60K", Category = "Vitamins", Manufacturer = "Mankind", Composition = "Cholecalciferol 60000 IU", DosageForm = "Capsule", Strength = "60000 IU", PackagingType = "Strip of 4", Price = 130m, StockQuantity = 200, Description = "Weekly Vitamin D supplement.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400" },
                new() { Id = 16, Name = "Vitamin B12 1500mcg", Category = "Vitamins", Manufacturer = "Zydus", Composition = "Methylcobalamin 1500mcg", DosageForm = "Tablet", Strength = "1500mcg", PackagingType = "Strip of 10", Price = 80m, StockQuantity = 220, Description = "Nerve health and energy support.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 17, Name = "Calcium + Vit D3", Category = "Vitamins", Manufacturer = "Shelcal", Composition = "Calcium Carbonate 500mg + D3 250IU", DosageForm = "Tablet", Strength = "500mg", PackagingType = "Strip of 15", Price = 110m, StockQuantity = 200, Description = "Bone health supplement.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400" },
                new() { Id = 18, Name = "Multivitamin Daily", Category = "Vitamins", Manufacturer = "Revital H", Composition = "Multivitamins + Minerals", DosageForm = "Capsule", Strength = "Multi", PackagingType = "Bottle of 30", Price = 220m, StockQuantity = 180, Description = "Daily wellness supplement.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400" },
                new() { Id = 19, Name = "Benadryl Syrup", Category = "Cough & Cold", Manufacturer = "J&J", Composition = "Diphenhydramine + Ammonium Chloride", DosageForm = "Syrup", Strength = "100ml", PackagingType = "Bottle 100ml", Price = 95m, StockQuantity = 160, Description = "Cough syrup for dry and wet cough.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400" },
                new() { Id = 20, Name = "Honitus Cough Syrup", Category = "Cough & Cold", Manufacturer = "Dabur", Composition = "Herbal blend", DosageForm = "Syrup", Strength = "100ml", PackagingType = "Bottle 100ml", Price = 85m, StockQuantity = 180, Description = "Ayurvedic cough relief.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400" },
                new() { Id = 21, Name = "Sinarest", Category = "Cough & Cold", Manufacturer = "Centaur", Composition = "Paracetamol + Phenylephrine + CPM", DosageForm = "Tablet", Strength = "500mg", PackagingType = "Strip of 10", Price = 55m, StockQuantity = 220, Description = "Multi-symptom cold and flu relief.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 22, Name = "D-Cold Total", Category = "Cough & Cold", Manufacturer = "Reckitt", Composition = "Paracetamol + Phenylephrine + Caffeine", DosageForm = "Tablet", Strength = "500mg", PackagingType = "Strip of 10", Price = 50m, StockQuantity = 200, Description = "Cold, flu, and headache relief.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 23, Name = "Vicks VapoRub", Category = "Cough & Cold", Manufacturer = "P&G", Composition = "Camphor + Menthol + Eucalyptus", DosageForm = "Ointment", Strength = "50g", PackagingType = "Jar 50g", Price = 100m, StockQuantity = 240, Description = "Topical cold relief balm.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 24, Name = "ORS Powder", Category = "Hydration", Manufacturer = "Electral", Composition = "Glucose + Salts", DosageForm = "Powder", Strength = "21.8g", PackagingType = "Sachet", Price = 22m, StockQuantity = 400, Description = "Rehydration salts for diarrhoea.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400" },
                new() { Id = 25, Name = "Eno Fruit Salt", Category = "Gastro", Manufacturer = "GSK", Composition = "Sodium Bicarbonate + Citric Acid", DosageForm = "Powder", Strength = "5g", PackagingType = "Sachet", Price = 10m, StockQuantity = 500, Description = "Fast acidity and gas relief.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400" },
                new() { Id = 26, Name = "Digene Gel", Category = "Gastro", Manufacturer = "Abbott", Composition = "Magaldrate + Simethicone", DosageForm = "Syrup", Strength = "200ml", PackagingType = "Bottle 200ml", Price = 130m, StockQuantity = 160, Description = "Antacid and anti-flatulent.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400" },
                new() { Id = 27, Name = "Loperamide 2mg", Category = "Gastro", Manufacturer = "Cipla", Composition = "Loperamide HCl 2mg", DosageForm = "Tablet", Strength = "2mg", PackagingType = "Strip of 10", Price = 30m, StockQuantity = 300, Description = "For acute diarrhoea.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 28, Name = "Ondansetron 4mg", Category = "Gastro", Manufacturer = "Glenmark", Composition = "Ondansetron 4mg", DosageForm = "Tablet", Strength = "4mg", PackagingType = "Strip of 10", Price = 70m, StockQuantity = 180, Description = "Anti-emetic for nausea and vomiting.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 29, Name = "Montair LC", Category = "Allergy", Manufacturer = "Cipla", Composition = "Montelukast + Levocetirizine", DosageForm = "Tablet", Strength = "10mg", PackagingType = "Strip of 10", Price = 145m, StockQuantity = 150, Description = "Allergy and asthma management.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400" },
                new() { Id = 30, Name = "Levocetirizine 5mg", Category = "Allergy", Manufacturer = "Dr. Reddy's", Composition = "Levocetirizine 5mg", DosageForm = "Tablet", Strength = "5mg", PackagingType = "Strip of 10", Price = 40m, StockQuantity = 260, Description = "Non-sedating antihistamine.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 31, Name = "Asthalin Inhaler", Category = "Respiratory", Manufacturer = "Cipla", Composition = "Salbutamol 100mcg", DosageForm = "Inhaler", Strength = "100mcg", PackagingType = "Inhaler 200 doses", Price = 175m, StockQuantity = 100, Description = "Quick relief bronchodilator.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400" },
                new() { Id = 32, Name = "Budecort Inhaler", Category = "Respiratory", Manufacturer = "Cipla", Composition = "Budesonide 200mcg", DosageForm = "Inhaler", Strength = "200mcg", PackagingType = "Inhaler 200 doses", Price = 320m, StockQuantity = 80, Description = "Steroid inhaler for asthma control.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400" },
                new() { Id = 33, Name = "Iron + Folic Acid", Category = "Supplements", Manufacturer = "Dexorange", Composition = "Iron + Folic Acid", DosageForm = "Syrup", Strength = "200ml", PackagingType = "Bottle 200ml", Price = 175m, StockQuantity = 140, Description = "Anaemia support tonic.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 34, Name = "Zincovit Tablet", Category = "Supplements", Manufacturer = "Apex", Composition = "Multivitamins + Zinc", DosageForm = "Tablet", Strength = "Multi", PackagingType = "Strip of 15", Price = 95m, StockQuantity = 220, Description = "Multivitamin with zinc.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400" },
                new() { Id = 35, Name = "Becosules Capsules", Category = "Supplements", Manufacturer = "Pfizer", Composition = "B-Complex + C", DosageForm = "Capsule", Strength = "Multi", PackagingType = "Strip of 20", Price = 60m, StockQuantity = 280, Description = "B-complex supplement.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 36, Name = "Ranitidine 150mg", Category = "Gastro", Manufacturer = "GSK", Composition = "Ranitidine 150mg", DosageForm = "Tablet", Strength = "150mg", PackagingType = "Strip of 10", Price = 35m, StockQuantity = 200, Description = "H2 blocker for acidity.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 37, Name = "Diclofenac Gel", Category = "Pain Relief", Manufacturer = "Volini", Composition = "Diclofenac Diethylamine 1.16%", DosageForm = "Gel", Strength = "30g", PackagingType = "Tube 30g", Price = 110m, StockQuantity = 260, Description = "Topical pain relief gel.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400" },
                new() { Id = 38, Name = "Moov Spray", Category = "Pain Relief", Manufacturer = "Reckitt", Composition = "Methyl Salicylate + Menthol", DosageForm = "Spray", Strength = "55g", PackagingType = "Spray Can 55g", Price = 175m, StockQuantity = 140, Description = "Backache and muscle relief spray.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400" },
                new() { Id = 39, Name = "Saline Nasal Drops", Category = "Cough & Cold", Manufacturer = "Nasivion", Composition = "Sodium Chloride 0.9%", DosageForm = "Drops", Strength = "10ml", PackagingType = "Bottle 10ml", Price = 70m, StockQuantity = 200, Description = "Gentle nasal congestion relief.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400" },
                new() { Id = 40, Name = "Otrivin Nasal Spray", Category = "Cough & Cold", Manufacturer = "GSK", Composition = "Xylometazoline 0.1%", DosageForm = "Spray", Strength = "10ml", PackagingType = "Bottle 10ml", Price = 105m, StockQuantity = 180, Description = "Decongestant nasal spray.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400" },
                new() { Id = 41, Name = "Ciplox 500mg", Category = "Antibiotic", Manufacturer = "Cipla", Composition = "Ciprofloxacin 500mg", DosageForm = "Tablet", Strength = "500mg", PackagingType = "Strip of 10", Price = 90m, StockQuantity = 130, Description = "Broad-spectrum antibiotic.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1576602976047-174e57a47881?w=400" },
                new() { Id = 42, Name = "Augmentin 625", Category = "Antibiotic", Manufacturer = "GSK", Composition = "Amoxicillin + Clavulanic Acid", DosageForm = "Tablet", Strength = "625mg", PackagingType = "Strip of 6", Price = 240m, StockQuantity = 100, Description = "Combination antibiotic.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1576602976047-174e57a47881?w=400" },
                new() { Id = 43, Name = "Thyronorm 50mcg", Category = "Thyroid", Manufacturer = "Abbott", Composition = "Levothyroxine 50mcg", DosageForm = "Tablet", Strength = "50mcg", PackagingType = "Strip of 100", Price = 145m, StockQuantity = 160, Description = "Hypothyroidism therapy.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400" },
                new() { Id = 44, Name = "Cremaffin Plus", Category = "Gastro", Manufacturer = "Abbott", Composition = "Liquid Paraffin + Milk of Magnesia", DosageForm = "Syrup", Strength = "225ml", PackagingType = "Bottle 225ml", Price = 165m, StockQuantity = 140, Description = "Laxative for constipation.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400" },
                new() { Id = 45, Name = "Pediatric Paracetamol Syrup", Category = "Pediatric", Manufacturer = "P&G", Composition = "Paracetamol 250mg/5ml", DosageForm = "Syrup", Strength = "60ml", PackagingType = "Bottle 60ml", Price = 65m, StockQuantity = 220, Description = "Child fever and pain syrup.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400" },
                new() { Id = 46, Name = "Pediatric Multivitamin Drops", Category = "Pediatric", Manufacturer = "Pfizer", Composition = "Multivitamin drops", DosageForm = "Drops", Strength = "15ml", PackagingType = "Bottle 15ml", Price = 95m, StockQuantity = 200, Description = "Daily drops for children.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400" },
                new() { Id = 47, Name = "Insulin Glargine 100IU", Category = "Diabetes", Manufacturer = "Sanofi", Composition = "Insulin Glargine", DosageForm = "Injection", Strength = "100IU/ml", PackagingType = "Cartridge 3ml", Price = 850m, StockQuantity = 60, Description = "Long-acting basal insulin.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1559757148-5c350d0d3c56?w=400" },
                new() { Id = 48, Name = "Ecosprin 75mg", Category = "Cardiac", Manufacturer = "USV", Composition = "Aspirin 75mg", DosageForm = "Tablet", Strength = "75mg", PackagingType = "Strip of 14", Price = 14m, StockQuantity = 320, Description = "Low-dose aspirin antiplatelet.", RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400" },
                new() { Id = 49, Name = "Hand Sanitizer 500ml", Category = "Personal Care", Manufacturer = "Dettol", Composition = "Ethanol 70%", DosageForm = "Liquid", Strength = "500ml", PackagingType = "Bottle 500ml", Price = 199m, StockQuantity = 300, Description = "Kills 99.9% germs.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400" },
                new() { Id = 50, Name = "Surgical Face Mask (50)", Category = "Personal Care", Manufacturer = "Generic", Composition = "3-ply mask", DosageForm = "Mask", Strength = "3-ply", PackagingType = "Box of 50", Price = 150m, StockQuantity = 400, Description = "Disposable surgical face masks.", RequiresPrescription = false, ImageUrl = "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400" }
            };

            modelBuilder.Entity<Medicine>().HasData(meds);
        }
    }
}
