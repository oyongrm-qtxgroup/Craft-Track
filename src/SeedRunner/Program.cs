using CraftTrack.Data;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using Microsoft.EntityFrameworkCore;

// ─── Setup ───
var dbPath = CraftTrackDbContext.GetDatabasePath();
Console.WriteLine($"Database: {dbPath}");

var options = new DbContextOptionsBuilder<CraftTrackDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

using var ctx = new CraftTrackDbContext(options);
ctx.Database.Migrate(); // ensure schema exists

var rng = new Random(42);

// ─── Truncate all tables (order matters for FK constraints) ───
Console.Write("Truncating tables...");
ctx.Database.ExecuteSqlRaw("DELETE FROM RejectRecords");
ctx.Database.ExecuteSqlRaw("DELETE FROM Notifications");
ctx.Database.ExecuteSqlRaw("DELETE FROM StockAdjustments");
ctx.Database.ExecuteSqlRaw("DELETE FROM Expenses");
ctx.Database.ExecuteSqlRaw("DELETE FROM OrderItems");
ctx.Database.ExecuteSqlRaw("DELETE FROM ProductMaterials");
ctx.Database.ExecuteSqlRaw("DELETE FROM Orders");
ctx.Database.ExecuteSqlRaw("DELETE FROM Products");
ctx.Database.ExecuteSqlRaw("DELETE FROM InventoryItems");
ctx.Database.ExecuteSqlRaw("DELETE FROM Customers");
// Reset auto-increment
ctx.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence");
Console.WriteLine(" done.");

// ─── 1. Customers (15) ───
Console.Write("Seeding customers...");
var customers = new List<Customer>();
var customerNames = new[]
{
    "Maria Santos", "Juan dela Cruz", "Angela Reyes", "Mark Villanueva", "Christine Bautista",
    "Ramon Aquino", "Jessica Flores", "Paolo Garcia", "Nina Cruz", "Robert Tan",
    "Sophia Lim", "Carlos Mendoza", "Patricia Ramos", "Daniel Navarro", "Victoria Castillo"
};
var phones = new[] { "0917-123-4567", "0918-234-5678", "0919-345-6789", "0920-456-7890", "0921-567-8901",
    "0922-678-9012", "0923-789-0123", "0925-890-1234", "0926-901-2345", "0927-012-3456",
    "0928-111-2222", "0929-333-4444", "0930-555-6666", "0931-777-8888", "0932-999-0000" };

for (int i = 0; i < customerNames.Length; i++)
{
    customers.Add(new Customer
    {
        FullName = customerNames[i],
        Phone = phones[i],
        Notes = i % 3 == 0 ? "Regular customer" : null,
        CreatedAt = DateTime.Now.AddDays(-rng.Next(30, 365))
    });
}
ctx.Customers.AddRange(customers);
ctx.SaveChanges();
Console.WriteLine($" {customers.Count}");

// ─── 2. Inventory Items (20) ───
Console.Write("Seeding inventory items...");
var inventoryData = new (string Name, InventoryCategory Cat, string Unit, decimal Qty, decimal Threshold, decimal Cost)[]
{
    ("Sublimation Paper A4", InventoryCategory.RawMaterial, "sheets", 500, 100, 3.50m),
    ("Sublimation Paper A3", InventoryCategory.RawMaterial, "sheets", 200, 50, 7.00m),
    ("Sublimation Ink Cyan", InventoryCategory.RawMaterial, "ml", 1000, 200, 0.85m),
    ("Sublimation Ink Magenta", InventoryCategory.RawMaterial, "ml", 800, 200, 0.85m),
    ("Sublimation Ink Yellow", InventoryCategory.RawMaterial, "ml", 900, 200, 0.85m),
    ("Sublimation Ink Black", InventoryCategory.RawMaterial, "ml", 1200, 200, 0.85m),
    ("White Ceramic Mug 11oz", InventoryCategory.RawMaterial, "pcs", 150, 30, 45.00m),
    ("White Ceramic Mug 15oz", InventoryCategory.RawMaterial, "pcs", 80, 20, 55.00m),
    ("Cotton T-Shirt Blank (S)", InventoryCategory.RawMaterial, "pcs", 50, 15, 95.00m),
    ("Cotton T-Shirt Blank (M)", InventoryCategory.RawMaterial, "pcs", 60, 15, 95.00m),
    ("Cotton T-Shirt Blank (L)", InventoryCategory.RawMaterial, "pcs", 70, 15, 95.00m),
    ("Cotton T-Shirt Blank (XL)", InventoryCategory.RawMaterial, "pcs", 40, 15, 100.00m),
    ("Mouse Pad Blank", InventoryCategory.RawMaterial, "pcs", 100, 25, 35.00m),
    ("Phone Case Blank (iPhone)", InventoryCategory.RawMaterial, "pcs", 60, 15, 50.00m),
    ("Tumbler Stainless 20oz", InventoryCategory.RawMaterial, "pcs", 45, 10, 180.00m),
    ("Heat Transfer Vinyl", InventoryCategory.RawMaterial, "meters", 30, 10, 120.00m),
    ("Packaging Box Small", InventoryCategory.RawMaterial, "pcs", 200, 50, 12.00m),
    ("Bubble Wrap", InventoryCategory.RawMaterial, "meters", 50, 15, 25.00m),
    ("Heat Press Machine", InventoryCategory.ToolAndEquipment, "unit", 2, 1, 15000.00m),
    ("Sublimation Printer", InventoryCategory.ToolAndEquipment, "unit", 1, 1, 25000.00m),
};

var inventoryItems = new List<InventoryItem>();
foreach (var d in inventoryData)
{
    inventoryItems.Add(new InventoryItem
    {
        Name = d.Name, Category = d.Cat, UnitOfMeasure = d.Unit,
        Quantity = d.Qty, LowStockThreshold = d.Threshold, UnitCost = d.Cost,
        CreatedAt = DateTime.Now.AddDays(-rng.Next(60, 400))
    });
}
ctx.InventoryItems.AddRange(inventoryItems);
ctx.SaveChanges();
Console.WriteLine($" {inventoryItems.Count}");

// ─── 3. Products with BOM (10) ───
Console.Write("Seeding products...");
var products = new List<Product>
{
    new() { Name = "Custom Mug 11oz", Description = "Personalized sublimation mug 11oz", DefaultPrice = 250.00m, LaborCost = 30.00m, OverheadCost = 15.00m },
    new() { Name = "Custom Mug 15oz", Description = "Personalized sublimation mug 15oz", DefaultPrice = 350.00m, LaborCost = 35.00m, OverheadCost = 15.00m },
    new() { Name = "Custom T-Shirt (M)", Description = "Sublimation print cotton tee Medium", DefaultPrice = 450.00m, LaborCost = 50.00m, OverheadCost = 20.00m },
    new() { Name = "Custom T-Shirt (L)", Description = "Sublimation print cotton tee Large", DefaultPrice = 450.00m, LaborCost = 50.00m, OverheadCost = 20.00m },
    new() { Name = "Custom T-Shirt (XL)", Description = "Sublimation print cotton tee XL", DefaultPrice = 500.00m, LaborCost = 55.00m, OverheadCost = 20.00m },
    new() { Name = "Custom Mouse Pad", Description = "Full-print sublimation mouse pad", DefaultPrice = 180.00m, LaborCost = 20.00m, OverheadCost = 10.00m },
    new() { Name = "Custom Phone Case", Description = "Sublimation iPhone case", DefaultPrice = 300.00m, LaborCost = 25.00m, OverheadCost = 15.00m },
    new() { Name = "Custom Tumbler 20oz", Description = "Stainless steel sublimation tumbler", DefaultPrice = 650.00m, LaborCost = 60.00m, OverheadCost = 25.00m },
    new() { Name = "Vinyl T-Shirt (M)", Description = "Heat transfer vinyl tee Medium", DefaultPrice = 500.00m, LaborCost = 60.00m, OverheadCost = 20.00m },
    new() { Name = "Custom Photo Print A3", Description = "A3 sublimation photo print", DefaultPrice = 150.00m, LaborCost = 15.00m, OverheadCost = 5.00m },
};
ctx.Products.AddRange(products);
ctx.SaveChanges();

// BOM - ProductMaterials
var bom = new List<ProductMaterial>
{
    // Custom Mug 11oz: mug + sublimation paper A4 + inks
    new() { ProductId = products[0].Id, InventoryItemId = inventoryItems[6].Id, QuantityRequired = 1 },      // White Mug 11oz
    new() { ProductId = products[0].Id, InventoryItemId = inventoryItems[0].Id, QuantityRequired = 1 },      // Sublimation Paper A4
    new() { ProductId = products[0].Id, InventoryItemId = inventoryItems[2].Id, QuantityRequired = 5 },      // Ink Cyan
    new() { ProductId = products[0].Id, InventoryItemId = inventoryItems[3].Id, QuantityRequired = 3 },      // Ink Magenta

    // Custom Mug 15oz
    new() { ProductId = products[1].Id, InventoryItemId = inventoryItems[7].Id, QuantityRequired = 1 },
    new() { ProductId = products[1].Id, InventoryItemId = inventoryItems[0].Id, QuantityRequired = 2 },
    new() { ProductId = products[1].Id, InventoryItemId = inventoryItems[2].Id, QuantityRequired = 8 },

    // Custom T-Shirt (M)
    new() { ProductId = products[2].Id, InventoryItemId = inventoryItems[9].Id, QuantityRequired = 1 },      // T-Shirt M
    new() { ProductId = products[2].Id, InventoryItemId = inventoryItems[0].Id, QuantityRequired = 2 },      // Paper A4
    new() { ProductId = products[2].Id, InventoryItemId = inventoryItems[2].Id, QuantityRequired = 10 },     // Ink Cyan

    // Custom T-Shirt (L)
    new() { ProductId = products[3].Id, InventoryItemId = inventoryItems[10].Id, QuantityRequired = 1 },     // T-Shirt L
    new() { ProductId = products[3].Id, InventoryItemId = inventoryItems[0].Id, QuantityRequired = 2 },
    new() { ProductId = products[3].Id, InventoryItemId = inventoryItems[3].Id, QuantityRequired = 10 },

    // Custom T-Shirt (XL)
    new() { ProductId = products[4].Id, InventoryItemId = inventoryItems[11].Id, QuantityRequired = 1 },
    new() { ProductId = products[4].Id, InventoryItemId = inventoryItems[1].Id, QuantityRequired = 1 },      // Paper A3

    // Custom Mouse Pad
    new() { ProductId = products[5].Id, InventoryItemId = inventoryItems[12].Id, QuantityRequired = 1 },     // Mouse Pad Blank
    new() { ProductId = products[5].Id, InventoryItemId = inventoryItems[0].Id, QuantityRequired = 1 },

    // Custom Phone Case
    new() { ProductId = products[6].Id, InventoryItemId = inventoryItems[13].Id, QuantityRequired = 1 },     // Phone Case Blank
    new() { ProductId = products[6].Id, InventoryItemId = inventoryItems[0].Id, QuantityRequired = 1 },

    // Custom Tumbler
    new() { ProductId = products[7].Id, InventoryItemId = inventoryItems[14].Id, QuantityRequired = 1 },     // Tumbler Blank
    new() { ProductId = products[7].Id, InventoryItemId = inventoryItems[0].Id, QuantityRequired = 2 },

    // Vinyl T-Shirt (M)
    new() { ProductId = products[8].Id, InventoryItemId = inventoryItems[9].Id, QuantityRequired = 1 },      // T-Shirt M
    new() { ProductId = products[8].Id, InventoryItemId = inventoryItems[15].Id, QuantityRequired = 0.5m },   // HTV

    // Custom Photo Print A3
    new() { ProductId = products[9].Id, InventoryItemId = inventoryItems[1].Id, QuantityRequired = 1 },      // Paper A3
    new() { ProductId = products[9].Id, InventoryItemId = inventoryItems[2].Id, QuantityRequired = 15 },
    new() { ProductId = products[9].Id, InventoryItemId = inventoryItems[3].Id, QuantityRequired = 10 },
    new() { ProductId = products[9].Id, InventoryItemId = inventoryItems[4].Id, QuantityRequired = 12 },
    new() { ProductId = products[9].Id, InventoryItemId = inventoryItems[5].Id, QuantityRequired = 8 },
};
ctx.ProductMaterials.AddRange(bom);
ctx.SaveChanges();
Console.WriteLine($" {products.Count} products, {bom.Count} BOM entries");

// ─── 4. Orders with OrderItems (30 orders) ───
Console.Write("Seeding orders...");
var statuses = new[] { OrderStatus.Active, OrderStatus.InProgress, OrderStatus.Completed, OrderStatus.Completed, OrderStatus.Completed, OrderStatus.Cancelled };
var severities = new[] { SeverityLevel.Low, SeverityLevel.Low, SeverityLevel.High, SeverityLevel.Rush };
var allOrders = new List<Order>();

for (int i = 0; i < 30; i++)
{
    var cust = customers[rng.Next(customers.Count)];
    var status = statuses[rng.Next(statuses.Length)];
    var dateOrdered = DateTime.Today.AddDays(-rng.Next(1, 180));
    var dueDate = dateOrdered.AddDays(rng.Next(3, 21));
    var severity = severities[rng.Next(severities.Length)];

    // 1-4 items per order
    var itemCount = rng.Next(1, 5);
    var orderItems = new List<OrderItem>();
    decimal subTotal = 0;
    for (int j = 0; j < itemCount; j++)
    {
        var prod = products[rng.Next(products.Count)];
        var qty = rng.Next(1, 6);
        var unitPrice = prod.DefaultPrice + rng.Next(-20, 50);
        orderItems.Add(new OrderItem
        {
            ProductId = prod.Id,
            ItemType = prod.Name,
            Quantity = qty,
            UnitPrice = unitPrice
        });
        subTotal += qty * unitPrice;
    }

    var discountType = rng.Next(4) == 0 ? DiscountType.Percentage : (rng.Next(4) == 0 ? DiscountType.FixedAmount : DiscountType.None);
    decimal discountValue = discountType switch
    {
        DiscountType.Percentage => rng.Next(5, 15),
        DiscountType.FixedAmount => rng.Next(50, 200),
        _ => 0
    };
    decimal discountAmount = discountType switch
    {
        DiscountType.Percentage => Math.Round(subTotal * discountValue / 100m, 2),
        DiscountType.FixedAmount => Math.Min(discountValue, subTotal),
        _ => 0
    };
    decimal totalPrice = Math.Max(0, subTotal - discountAmount);

    decimal amountPaid = status == OrderStatus.Completed ? totalPrice
        : status == OrderStatus.Cancelled ? 0
        : rng.Next(3) == 0 ? 0 : Math.Round(totalPrice * (decimal)(rng.NextDouble() * 0.8 + 0.1), 2);

    var paymentType = amountPaid >= totalPrice ? PaymentType.FullPayment
        : amountPaid > 0 ? PaymentType.Downpayment : PaymentType.Unpaid;

    allOrders.Add(new Order
    {
        CustomerId = cust.Id,
        DateOrdered = dateOrdered,
        DueDate = dueDate,
        Severity = severity,
        Status = status,
        TotalPrice = totalPrice,
        AmountPaid = amountPaid,
        DiscountType = discountType,
        DiscountValue = discountValue,
        PaymentType = paymentType,
        Notes = i % 5 == 0 ? "Rush order - priority handling" : null,
        CreatedAt = dateOrdered,
        OrderItems = orderItems
    });
}
ctx.Orders.AddRange(allOrders);
ctx.SaveChanges();
Console.WriteLine($" {allOrders.Count} orders, {allOrders.Sum(o => o.OrderItems.Count)} items");

// ─── 5. Expenses (40) ───
Console.Write("Seeding expenses...");
var expenseCategories = Enum.GetValues<ExpenseCategory>();
var expenseDescs = new Dictionary<ExpenseCategory, string[]>
{
    [ExpenseCategory.Materials] = ["Bulk sublimation paper", "Ink refill batch", "Mug blanks restock", "T-shirt blanks order", "Tumbler wholesale"],
    [ExpenseCategory.Tools] = ["Heat press pad replacement", "Cutter blade set", "Spray adhesive", "Color calibration tool"],
    [ExpenseCategory.Packaging] = ["Bubble wrap roll", "Small boxes 50pcs", "Packing tape", "Styrofoam inserts"],
    [ExpenseCategory.Utilities] = ["Electricity bill", "Internet subscription", "Water bill"],
    [ExpenseCategory.Other] = ["Delivery fee", "Shop cleaning supplies", "Business permit renewal", "Freelancer design fee"],
};

var expenses = new List<Expense>();
for (int i = 0; i < 40; i++)
{
    var cat = expenseCategories[rng.Next(expenseCategories.Length)];
    var descs = expenseDescs[cat];
    expenses.Add(new Expense
    {
        Date = DateTime.Today.AddDays(-rng.Next(0, 180)),
        Category = cat,
        Description = descs[rng.Next(descs.Length)],
        Amount = Math.Round((decimal)(rng.NextDouble() * 4500 + 100), 2),
        OrderId = rng.Next(5) == 0 ? allOrders[rng.Next(allOrders.Count)].Id : null,
        CreatedAt = DateTime.Now.AddDays(-rng.Next(0, 180))
    });
}
ctx.Expenses.AddRange(expenses);
ctx.SaveChanges();
Console.WriteLine($" {expenses.Count}");

// ─── 6. Stock Adjustments (25) ───
Console.Write("Seeding stock adjustments...");
var adjustments = new List<StockAdjustment>();
for (int i = 0; i < 25; i++)
{
    var item = inventoryItems[rng.Next(inventoryItems.Count)];
    var type = rng.Next(3) == 0 ? AdjustmentType.Deduct : AdjustmentType.Add;
    var qty = Math.Round((decimal)(rng.NextDouble() * 20 + 1), 2);

    adjustments.Add(new StockAdjustment
    {
        InventoryItemId = item.Id,
        AdjustmentType = type,
        QuantityChange = qty,
        Note = type == AdjustmentType.Add ? "Restock" : "Used in production",
        AdjustedAt = DateTime.Now.AddDays(-rng.Next(0, 120))
    });
}
ctx.StockAdjustments.AddRange(adjustments);
ctx.SaveChanges();
Console.WriteLine($" {adjustments.Count}");

// ─── 7. Reject Records (12) ───
Console.Write("Seeding reject records...");
var rejectReasons = Enum.GetValues<RejectReason>();
var allOrderItems = ctx.OrderItems.ToList();
var rejects = new List<RejectRecord>();

// Order-level rejects
for (int i = 0; i < 6; i++)
{
    var oi = allOrderItems[rng.Next(allOrderItems.Count)];
    var reason = rejectReasons[rng.Next(rejectReasons.Length)];
    var qty = rng.Next(1, 3);
    rejects.Add(new RejectRecord
    {
        Date = DateTime.Today.AddDays(-rng.Next(0, 90)),
        RejectReason = reason,
        Notes = reason switch
        {
            RejectReason.WrongPrinting => "Colors were off, reprinting needed",
            RejectReason.IncorrectDesign => "Customer provided wrong file",
            RejectReason.MaterialDefect => "Mug had a crack before printing",
            RejectReason.MachineMalfunction => "Heat press temperature spike",
            _ => "Quality check failed"
        },
        Quantity = qty,
        EstimatedCost = Math.Round(qty * (decimal)(rng.NextDouble() * 150 + 30), 2),
        OrderItemId = oi.Id,
        CreatedAt = DateTime.Now.AddDays(-rng.Next(0, 90))
    });
}

// Inventory-level rejects
for (int i = 0; i < 6; i++)
{
    var inv = inventoryItems[rng.Next(0, 16)]; // raw materials only
    var reason = rng.Next(2) == 0 ? RejectReason.MaterialDefect : RejectReason.QualityControl;
    var qty = Math.Round((decimal)(rng.NextDouble() * 5 + 1), 2);
    rejects.Add(new RejectRecord
    {
        Date = DateTime.Today.AddDays(-rng.Next(0, 60)),
        RejectReason = reason,
        Notes = $"Defective {inv.Name} batch",
        Quantity = qty,
        EstimatedCost = Math.Round(qty * inv.UnitCost, 2),
        InventoryItemId = inv.Id,
        CreatedAt = DateTime.Now.AddDays(-rng.Next(0, 60))
    });
}
ctx.RejectRecords.AddRange(rejects);
ctx.SaveChanges();
Console.WriteLine($" {rejects.Count}");

// ─── 8. Notifications (15) ───
Console.Write("Seeding notifications...");
var notifTypes = Enum.GetValues<NotificationType>();
var notifications = new List<Notification>();

// Due soon / overdue
var upcomingOrders = allOrders.Where(o => o.Status == OrderStatus.Active || o.Status == OrderStatus.InProgress).Take(5).ToList();
foreach (var ord in upcomingOrders)
{
    notifications.Add(new Notification
    {
        Type = NotificationType.OrderDueIn3Days,
        Message = $"Order #{ord.Id} for {customers.First(c => c.Id == ord.CustomerId).FullName} is due in 3 days.",
        RelatedOrderId = ord.Id,
        CreatedAt = DateTime.Now.AddDays(-rng.Next(0, 5))
    });
}

// Rush orders
var rushOrders = allOrders.Where(o => o.Severity == SeverityLevel.Rush).Take(3).ToList();
foreach (var ord in rushOrders)
{
    notifications.Add(new Notification
    {
        Type = NotificationType.RushOrderCreated,
        Message = $"Rush order #{ord.Id} created — prioritize immediately!",
        RelatedOrderId = ord.Id,
        CreatedAt = ord.CreatedAt
    });
}

// Low stock
var lowStockItems = inventoryItems.Where(i => i.Category == InventoryCategory.RawMaterial).Take(4).ToList();
foreach (var item in lowStockItems)
{
    notifications.Add(new Notification
    {
        Type = NotificationType.LowStock,
        Message = $"{item.Name} stock is low ({item.Quantity:N0} {item.UnitOfMeasure} remaining).",
        RelatedInventoryItemId = item.Id,
        CreatedAt = DateTime.Now.AddDays(-rng.Next(0, 10))
    });
}

// Reject notifications
foreach (var r in rejects.Take(3))
{
    notifications.Add(new Notification
    {
        Type = NotificationType.RejectCreated,
        Message = $"Item rejected: {r.Notes ?? "No details"} — estimated loss ₱{r.EstimatedCost:N2}",
        CreatedAt = r.CreatedAt
    });
}

ctx.Notifications.AddRange(notifications);
ctx.SaveChanges();
Console.WriteLine($" {notifications.Count}");

Console.WriteLine("\n✅ Seed complete!");
Console.WriteLine($"   Customers: {ctx.Customers.Count()}");
Console.WriteLine($"   Products: {ctx.Products.Count()}");
Console.WriteLine($"   Inventory Items: {ctx.InventoryItems.Count()}");
Console.WriteLine($"   Orders: {ctx.Orders.Count()}");
Console.WriteLine($"   Order Items: {ctx.OrderItems.Count()}");
Console.WriteLine($"   Expenses: {ctx.Expenses.Count()}");
Console.WriteLine($"   Stock Adjustments: {ctx.StockAdjustments.Count()}");
Console.WriteLine($"   Reject Records: {ctx.RejectRecords.Count()}");
Console.WriteLine($"   Notifications: {ctx.Notifications.Count()}");
Console.WriteLine($"   Product Materials: {ctx.ProductMaterials.Count()}");
