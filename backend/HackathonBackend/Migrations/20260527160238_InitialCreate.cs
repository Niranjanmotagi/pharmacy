using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HackathonBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: true),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false),
                    PointsRedeemed = table.Column<int>(type: "integer", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    Composition = table.Column<string>(type: "text", nullable: false),
                    DosageForm = table.Column<string>(type: "text", nullable: false),
                    Strength = table.Column<string>(type: "text", nullable: false),
                    PackagingType = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RequiresPrescription = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    OrderNumber = table.Column<string>(type: "text", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FinalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PromotionId = table.Column<int>(type: "integer", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "text", nullable: false),
                    DeliveryPhone = table.Column<string>(type: "text", nullable: false),
                    DeliveryNotes = table.Column<string>(type: "text", nullable: false),
                    PrescriptionFile = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PromotionCode = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DiscountType = table.Column<string>(type: "text", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MinimumOrderValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    UsedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    LoyaltyPoints = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Medicines",
                columns: new[] { "Id", "Category", "Composition", "Description", "DosageForm", "ImageUrl", "Manufacturer", "Name", "PackagingType", "Price", "RequiresPrescription", "StockQuantity", "Strength" },
                values: new object[,]
                {
                    { 1, "Pain Relief", "Paracetamol 650mg", "Effective relief from fever and mild to moderate pain.", "Tablet", "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400", "Micro Labs", "Dolo 650", "Strip of 15", 35m, false, 250, "650mg" },
                    { 2, "Pain Relief", "Paracetamol 500mg", "Fast-acting fever and headache relief.", "Tablet", "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400", "GSK", "Crocin Advance", "Strip of 15", 30m, false, 200, "500mg" },
                    { 3, "Allergy", "Cetirizine HCl 10mg", "Antihistamine for allergic rhinitis and skin allergies.", "Tablet", "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400", "Cipla", "Cetirizine 10mg", "Strip of 10", 25m, false, 300, "10mg" },
                    { 4, "Antibiotic", "Amoxicillin Trihydrate 500mg", "Broad-spectrum antibiotic for bacterial infections.", "Capsule", "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400", "Cipla", "Amoxicillin 500mg", "Strip of 10", 85m, true, 150, "500mg" },
                    { 5, "Antibiotic", "Azithromycin 500mg", "Macrolide antibiotic for respiratory and skin infections.", "Tablet", "https://images.unsplash.com/photo-1576602976047-174e57a47881?w=400", "Sun Pharma", "Azithromycin 500mg", "Strip of 3", 110m, true, 120, "500mg" },
                    { 6, "Pain Relief", "Ibuprofen 400mg", "Anti-inflammatory pain reliever.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Abbott", "Ibuprofen 400mg", "Strip of 10", 40m, false, 220, "400mg" },
                    { 7, "Gastro", "Pantoprazole Sodium 40mg", "Proton pump inhibitor for acidity and ulcers.", "Tablet", "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400", "Sun Pharma", "Pantoprazole 40mg", "Strip of 15", 90m, false, 180, "40mg" },
                    { 8, "Gastro", "Omeprazole 20mg", "Reduces stomach acid production.", "Capsule", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Dr. Reddy's", "Omeprazole 20mg", "Strip of 10", 75m, false, 200, "20mg" },
                    { 9, "Diabetes", "Metformin HCl 500mg", "First-line therapy for type 2 diabetes.", "Tablet", "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400", "USV", "Metformin 500mg", "Strip of 15", 50m, true, 250, "500mg" },
                    { 10, "Diabetes", "Glimepiride 2mg", "Sulfonylurea for type 2 diabetes control.", "Tablet", "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400", "Sanofi", "Glimepiride 2mg", "Strip of 10", 95m, true, 140, "2mg" },
                    { 11, "Cardiac", "Atorvastatin Calcium 10mg", "Statin to lower cholesterol.", "Tablet", "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400", "Pfizer", "Atorvastatin 10mg", "Strip of 10", 120m, true, 160, "10mg" },
                    { 12, "Cardiac", "Amlodipine Besilate 5mg", "Calcium channel blocker for hypertension.", "Tablet", "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400", "Cipla", "Amlodipine 5mg", "Strip of 10", 60m, true, 180, "5mg" },
                    { 13, "Cardiac", "Telmisartan 40mg", "Angiotensin II receptor blocker for BP.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Glenmark", "Telmisartan 40mg", "Strip of 10", 105m, true, 150, "40mg" },
                    { 14, "Vitamins", "Ascorbic Acid 500mg", "Antioxidant immunity booster.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "HealthVit", "Vitamin C 500mg", "Bottle of 60", 150m, false, 300, "500mg" },
                    { 15, "Vitamins", "Cholecalciferol 60000 IU", "Weekly Vitamin D supplement.", "Capsule", "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400", "Mankind", "Vitamin D3 60K", "Strip of 4", 130m, false, 200, "60000 IU" },
                    { 16, "Vitamins", "Methylcobalamin 1500mcg", "Nerve health and energy support.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Zydus", "Vitamin B12 1500mcg", "Strip of 10", 80m, false, 220, "1500mcg" },
                    { 17, "Vitamins", "Calcium Carbonate 500mg + D3 250IU", "Bone health supplement.", "Tablet", "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400", "Shelcal", "Calcium + Vit D3", "Strip of 15", 110m, false, 200, "500mg" },
                    { 18, "Vitamins", "Multivitamins + Minerals", "Daily wellness supplement.", "Capsule", "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400", "Revital H", "Multivitamin Daily", "Bottle of 30", 220m, false, 180, "Multi" },
                    { 19, "Cough & Cold", "Diphenhydramine + Ammonium Chloride", "Cough syrup for dry and wet cough.", "Syrup", "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400", "J&J", "Benadryl Syrup", "Bottle 100ml", 95m, false, 160, "100ml" },
                    { 20, "Cough & Cold", "Herbal blend", "Ayurvedic cough relief.", "Syrup", "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400", "Dabur", "Honitus Cough Syrup", "Bottle 100ml", 85m, false, 180, "100ml" },
                    { 21, "Cough & Cold", "Paracetamol + Phenylephrine + CPM", "Multi-symptom cold and flu relief.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Centaur", "Sinarest", "Strip of 10", 55m, false, 220, "500mg" },
                    { 22, "Cough & Cold", "Paracetamol + Phenylephrine + Caffeine", "Cold, flu, and headache relief.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Reckitt", "D-Cold Total", "Strip of 10", 50m, false, 200, "500mg" },
                    { 23, "Cough & Cold", "Camphor + Menthol + Eucalyptus", "Topical cold relief balm.", "Ointment", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "P&G", "Vicks VapoRub", "Jar 50g", 100m, false, 240, "50g" },
                    { 24, "Hydration", "Glucose + Salts", "Rehydration salts for diarrhoea.", "Powder", "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400", "Electral", "ORS Powder", "Sachet", 22m, false, 400, "21.8g" },
                    { 25, "Gastro", "Sodium Bicarbonate + Citric Acid", "Fast acidity and gas relief.", "Powder", "https://images.unsplash.com/photo-1550572017-edd951b55104?w=400", "GSK", "Eno Fruit Salt", "Sachet", 10m, false, 500, "5g" },
                    { 26, "Gastro", "Magaldrate + Simethicone", "Antacid and anti-flatulent.", "Syrup", "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400", "Abbott", "Digene Gel", "Bottle 200ml", 130m, false, 160, "200ml" },
                    { 27, "Gastro", "Loperamide HCl 2mg", "For acute diarrhoea.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Cipla", "Loperamide 2mg", "Strip of 10", 30m, false, 300, "2mg" },
                    { 28, "Gastro", "Ondansetron 4mg", "Anti-emetic for nausea and vomiting.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Glenmark", "Ondansetron 4mg", "Strip of 10", 70m, true, 180, "4mg" },
                    { 29, "Allergy", "Montelukast + Levocetirizine", "Allergy and asthma management.", "Tablet", "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400", "Cipla", "Montair LC", "Strip of 10", 145m, true, 150, "10mg" },
                    { 30, "Allergy", "Levocetirizine 5mg", "Non-sedating antihistamine.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Dr. Reddy's", "Levocetirizine 5mg", "Strip of 10", 40m, false, 260, "5mg" },
                    { 31, "Respiratory", "Salbutamol 100mcg", "Quick relief bronchodilator.", "Inhaler", "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400", "Cipla", "Asthalin Inhaler", "Inhaler 200 doses", 175m, true, 100, "100mcg" },
                    { 32, "Respiratory", "Budesonide 200mcg", "Steroid inhaler for asthma control.", "Inhaler", "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400", "Cipla", "Budecort Inhaler", "Inhaler 200 doses", 320m, true, 80, "200mcg" },
                    { 33, "Supplements", "Iron + Folic Acid", "Anaemia support tonic.", "Syrup", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Dexorange", "Iron + Folic Acid", "Bottle 200ml", 175m, false, 140, "200ml" },
                    { 34, "Supplements", "Multivitamins + Zinc", "Multivitamin with zinc.", "Tablet", "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400", "Apex", "Zincovit Tablet", "Strip of 15", 95m, false, 220, "Multi" },
                    { 35, "Supplements", "B-Complex + C", "B-complex supplement.", "Capsule", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Pfizer", "Becosules Capsules", "Strip of 20", 60m, false, 280, "Multi" },
                    { 36, "Gastro", "Ranitidine 150mg", "H2 blocker for acidity.", "Tablet", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "GSK", "Ranitidine 150mg", "Strip of 10", 35m, false, 200, "150mg" },
                    { 37, "Pain Relief", "Diclofenac Diethylamine 1.16%", "Topical pain relief gel.", "Gel", "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400", "Volini", "Diclofenac Gel", "Tube 30g", 110m, false, 260, "30g" },
                    { 38, "Pain Relief", "Methyl Salicylate + Menthol", "Backache and muscle relief spray.", "Spray", "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400", "Reckitt", "Moov Spray", "Spray Can 55g", 175m, false, 140, "55g" },
                    { 39, "Cough & Cold", "Sodium Chloride 0.9%", "Gentle nasal congestion relief.", "Drops", "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400", "Nasivion", "Saline Nasal Drops", "Bottle 10ml", 70m, false, 200, "10ml" },
                    { 40, "Cough & Cold", "Xylometazoline 0.1%", "Decongestant nasal spray.", "Spray", "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400", "GSK", "Otrivin Nasal Spray", "Bottle 10ml", 105m, false, 180, "10ml" },
                    { 41, "Antibiotic", "Ciprofloxacin 500mg", "Broad-spectrum antibiotic.", "Tablet", "https://images.unsplash.com/photo-1576602976047-174e57a47881?w=400", "Cipla", "Ciplox 500mg", "Strip of 10", 90m, true, 130, "500mg" },
                    { 42, "Antibiotic", "Amoxicillin + Clavulanic Acid", "Combination antibiotic.", "Tablet", "https://images.unsplash.com/photo-1576602976047-174e57a47881?w=400", "GSK", "Augmentin 625", "Strip of 6", 240m, true, 100, "625mg" },
                    { 43, "Thyroid", "Levothyroxine 50mcg", "Hypothyroidism therapy.", "Tablet", "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400", "Abbott", "Thyronorm 50mcg", "Strip of 100", 145m, true, 160, "50mcg" },
                    { 44, "Gastro", "Liquid Paraffin + Milk of Magnesia", "Laxative for constipation.", "Syrup", "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400", "Abbott", "Cremaffin Plus", "Bottle 225ml", 165m, false, 140, "225ml" },
                    { 45, "Pediatric", "Paracetamol 250mg/5ml", "Child fever and pain syrup.", "Syrup", "https://images.unsplash.com/photo-1587854692152-cbe660dbde88?w=400", "P&G", "Pediatric Paracetamol Syrup", "Bottle 60ml", 65m, false, 220, "60ml" },
                    { 46, "Pediatric", "Multivitamin drops", "Daily drops for children.", "Drops", "https://images.unsplash.com/photo-1559757175-5700dde675bc?w=400", "Pfizer", "Pediatric Multivitamin Drops", "Bottle 15ml", 95m, false, 200, "15ml" },
                    { 47, "Diabetes", "Insulin Glargine", "Long-acting basal insulin.", "Injection", "https://images.unsplash.com/photo-1559757148-5c350d0d3c56?w=400", "Sanofi", "Insulin Glargine 100IU", "Cartridge 3ml", 850m, true, 60, "100IU/ml" },
                    { 48, "Cardiac", "Aspirin 75mg", "Low-dose aspirin antiplatelet.", "Tablet", "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=400", "USV", "Ecosprin 75mg", "Strip of 14", 14m, true, 320, "75mg" },
                    { 49, "Personal Care", "Ethanol 70%", "Kills 99.9% germs.", "Liquid", "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400", "Dettol", "Hand Sanitizer 500ml", "Bottle 500ml", 199m, false, 300, "500ml" },
                    { 50, "Personal Care", "3-ply mask", "Disposable surgical face masks.", "Mask", "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400", "Generic", "Surgical Face Mask (50)", "Box of 50", 150m, false, 400, "3-ply" }
                });

            migrationBuilder.InsertData(
                table: "Promotions",
                columns: new[] { "Id", "Description", "DiscountType", "DiscountValue", "EndDate", "IsActive", "MaxDiscountAmount", "MinimumOrderValue", "PromotionCode", "StartDate", "UsageLimit", "UsedCount" },
                values: new object[,]
                {
                    { 1, "10% off your order", "Percentage", 10m, new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 200m, 0m, "WELCOME10", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0 },
                    { 2, "Flat ₹50 off on orders above ₹500", "FixedAmount", 50m, new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null, 500m, "FLAT50", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 0 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "LoyaltyPoints", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { 1, "admin@bytebrigade.local", "Admin", "User", 0, "admin123", "0000000000", "Admin", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_MedicineId",
                table: "CartItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MedicineId",
                table: "OrderItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_PromotionCode",
                table: "Promotions",
                column: "PromotionCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Entities");

            migrationBuilder.DropTable(
                name: "LoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
