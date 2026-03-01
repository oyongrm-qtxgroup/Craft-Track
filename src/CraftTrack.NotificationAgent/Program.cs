using Microsoft.Data.Sqlite;
using Microsoft.Toolkit.Uwp.Notifications;

/// <summary>
/// CraftTrack Notification Agent — a standalone console app intended to run
/// via Windows Task Scheduler (e.g., daily at 9 AM). It reads the CraftTrack
/// SQLite database and fires toast notifications for:
///   - Orders due within 3 days
///   - Orders due within 1 day
///   - Overdue orders
///   - Low-stock inventory items
/// </summary>

string dbFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "CraftTrack");
string dbPath = Path.Combine(dbFolder, "crafttrack.db");

if (!File.Exists(dbPath))
{
    Console.WriteLine($"Database not found at: {dbPath}");
    return;
}

string connectionString = $"Data Source={dbPath}";
var today = DateTime.Today;

try
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    CheckOverdueOrders(connection, today);
    CheckUpcomingDeadlines(connection, today, days: 1, "OrderDueIn1Day", "due TOMORROW");
    CheckUpcomingDeadlines(connection, today, days: 3, "OrderDueIn3Days", "due within 3 days");
    CheckLowStockItems(connection);

    Console.WriteLine("Notification check completed.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
}

void CheckOverdueOrders(SqliteConnection conn, DateTime refDate)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT o.Id, o.DueDate, c.FullName
        FROM Orders o
        LEFT JOIN Customers c ON o.CustomerId = c.Id
        WHERE o.DueDate < @today
          AND o.Status NOT IN ('Completed', 'Cancelled')
        ORDER BY o.DueDate ASC
        """;
    cmd.Parameters.AddWithValue("@today", refDate.ToString("yyyy-MM-dd"));

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        int orderId = reader.GetInt32(0);
        string dueDate = reader.GetString(1);
        string customer = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2);

        if (!NotificationExists(conn, orderId, "OrderOverdue", refDate))
        {
            InsertNotification(conn, orderId, "OrderOverdue",
                $"OVERDUE: Order #{orderId} for {customer} was due {dueDate}");

            ShowToast("⚠️ Overdue Order",
                $"Order #{orderId} for {customer} was due {dueDate}",
                "CraftTrack");
        }
    }
}

void CheckUpcomingDeadlines(SqliteConnection conn, DateTime refDate, int days, string type, string label)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT o.Id, o.DueDate, c.FullName
        FROM Orders o
        LEFT JOIN Customers c ON o.CustomerId = c.Id
        WHERE date(o.DueDate) BETWEEN @today AND @target
          AND o.Status NOT IN ('Completed', 'Cancelled')
        ORDER BY o.DueDate ASC
        """;
    cmd.Parameters.AddWithValue("@today", refDate.ToString("yyyy-MM-dd"));
    cmd.Parameters.AddWithValue("@target", refDate.AddDays(days).ToString("yyyy-MM-dd"));

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        int orderId = reader.GetInt32(0);
        string dueDate = reader.GetString(1);
        string customer = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2);

        if (!NotificationExists(conn, orderId, type, refDate))
        {
            InsertNotification(conn, orderId, type,
                $"Order #{orderId} for {customer} is {label} ({dueDate})");

            ShowToast($"📅 Upcoming Deadline",
                $"Order #{orderId} for {customer} is {label}",
                "CraftTrack");
        }
    }
}

void CheckLowStockItems(SqliteConnection conn)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT Id, Name, Quantity, LowStockThreshold
        FROM InventoryItems
        WHERE Quantity <= LowStockThreshold AND LowStockThreshold > 0
        """;

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        int itemId = reader.GetInt32(0);
        string name = reader.GetString(1);
        int qty = reader.GetInt32(2);
        int threshold = reader.GetInt32(3);

        if (!LowStockNotificationExists(conn, itemId, DateTime.Today))
        {
            InsertLowStockNotification(conn, itemId,
                $"Low stock: {name} has {qty} remaining (threshold: {threshold})");

            ShowToast("📦 Low Stock Alert",
                $"{name}: {qty} remaining (threshold: {threshold})",
                "CraftTrack");
        }
    }
}

bool NotificationExists(SqliteConnection conn, int orderId, string type, DateTime refDate)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT COUNT(1) FROM Notifications
        WHERE RelatedOrderId = @orderId AND Type = @type
          AND date(CreatedAt) = @date
        """;
    cmd.Parameters.AddWithValue("@orderId", orderId);
    cmd.Parameters.AddWithValue("@type", type);
    cmd.Parameters.AddWithValue("@date", refDate.ToString("yyyy-MM-dd"));
    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
}

bool LowStockNotificationExists(SqliteConnection conn, int itemId, DateTime refDate)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        SELECT COUNT(1) FROM Notifications
        WHERE RelatedInventoryItemId = @itemId AND Type = 'LowStock'
          AND date(CreatedAt) = @date
        """;
    cmd.Parameters.AddWithValue("@itemId", itemId);
    cmd.Parameters.AddWithValue("@date", refDate.ToString("yyyy-MM-dd"));
    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
}

void InsertNotification(SqliteConnection conn, int orderId, string type, string message)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        INSERT INTO Notifications (Type, Message, RelatedOrderId, IsAcknowledged, CreatedAt)
        VALUES (@type, @message, @orderId, 0, @now)
        """;
    cmd.Parameters.AddWithValue("@type", type);
    cmd.Parameters.AddWithValue("@message", message);
    cmd.Parameters.AddWithValue("@orderId", orderId);
    cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("O"));
    cmd.ExecuteNonQuery();
}

void InsertLowStockNotification(SqliteConnection conn, int itemId, string message)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
        INSERT INTO Notifications (Type, Message, RelatedInventoryItemId, IsAcknowledged, CreatedAt)
        VALUES ('LowStock', @message, @itemId, 0, @now)
        """;
    cmd.Parameters.AddWithValue("@message", message);
    cmd.Parameters.AddWithValue("@itemId", itemId);
    cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("O"));
    cmd.ExecuteNonQuery();
}

void ShowToast(string title, string body, string appName)
{
    try
    {
        new ToastContentBuilder()
            .AddText(title)
            .AddText(body)
            .AddAttributionText(appName)
            .Show();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Toast error: {ex.Message}");
    }
}
