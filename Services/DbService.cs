using MySqlConnector;
using EventTicketSystem.Models;

namespace EventTicketSystem.Services;

public class DbService
{
    private readonly string _connStr;
    public DbService(IConfiguration config)
    {
        _connStr = config.GetConnectionString("Default")
            ?? "Server=localhost;Port=3306;Database=event_system;Uid=root;Pwd=;";
    }

    private MySqlConnection Conn() => new MySqlConnection(_connStr);

    // ---------- Users ----------
    public async Task<List<User>> GetUsersAsync()
    {
        var list = new List<User>();
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("SELECT user_id,user_code,name,email,role,created_at FROM users ORDER BY user_id", conn);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            list.Add(new User {
                UserId = r.GetInt32("user_id"),
                UserCode = r.GetString("user_code"),
                Name = r.GetString("name"),
                Email = r.GetString("email"),
                Role = r.GetString("role"),
                CreatedAt = r.GetDateTime("created_at")
            });
        }
        return list;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("SELECT user_id,user_code,name,email,role,created_at FROM users WHERE user_id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var r = await cmd.ExecuteReaderAsync();
        if (await r.ReadAsync())
        {
            return new User {
                UserId = r.GetInt32("user_id"),
                UserCode = r.GetString("user_code"),
                Name = r.GetString("name"),
                Email = r.GetString("email"),
                Role = r.GetString("role"),
                CreatedAt = r.GetDateTime("created_at")
            };
        }
        return null;
    }

    public async Task<User?> GetUserByCodeAsync(string code)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("SELECT user_id,user_code,name,email,role,created_at FROM users WHERE user_code=@c", conn);
        cmd.Parameters.AddWithValue("@c", code);
        using var r = await cmd.ExecuteReaderAsync();
        if (await r.ReadAsync())
        {
            return new User {
                UserId = r.GetInt32("user_id"),
                UserCode = r.GetString("user_code"),
                Name = r.GetString("name"),
                Email = r.GetString("email"),
                Role = r.GetString("role"),
                CreatedAt = r.GetDateTime("created_at")
            };
        }
        return null;
    }

    // ---------- Categories ----------
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var list = new List<Category>();
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("SELECT category_id,category_name FROM categories ORDER BY category_id", conn);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            list.Add(new Category { CategoryId = r.GetInt32("category_id"), CategoryName = r.GetString("category_name") });
        return list;
    }

    // ---------- Events ----------
    public async Task<List<Event>> GetEventsAsync(string? name = null, int? categoryId = null, DateTime? date = null, string? venue = null, bool onlyPublished = false, bool includeAll = false)
    {
        var list = new List<Event>();
        var where = new List<string>();
        var sql = @"SELECT e.event_id,e.category_id,c.category_name,e.event_name,e.description,e.event_date,e.venue,e.price,e.capacity,e.status,e.created_at,
                    (SELECT COALESCE(SUM(quantity),0) FROM reservations r WHERE r.event_id=e.event_id AND r.status='予約済') AS reserved_count
                    FROM events e JOIN categories c ON e.category_id=c.category_id";
        if (!string.IsNullOrWhiteSpace(name)) { where.Add("e.event_name LIKE @name"); }
        if (categoryId.HasValue && categoryId.Value > 0) { where.Add("e.category_id=@cat"); }
        if (date.HasValue) { where.Add("DATE(e.event_date)=@dt"); }
        if (!string.IsNullOrWhiteSpace(venue)) { where.Add("e.venue LIKE @venue"); }
        if (onlyPublished) { where.Add("e.status='公開中'"); }
        if (where.Count > 0) sql += " WHERE " + string.Join(" AND ", where);
        sql += " ORDER BY e.event_date";

        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        if (!string.IsNullOrWhiteSpace(name)) cmd.Parameters.AddWithValue("@name", "%" + name + "%");
        if (categoryId.HasValue && categoryId.Value > 0) cmd.Parameters.AddWithValue("@cat", categoryId.Value);
        if (date.HasValue) cmd.Parameters.AddWithValue("@dt", date.Value.Date);
        if (!string.IsNullOrWhiteSpace(venue)) cmd.Parameters.AddWithValue("@venue", "%" + venue + "%");
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            var ev = new Event {
                EventId = r.GetInt32("event_id"),
                CategoryId = r.GetInt32("category_id"),
                CategoryName = r.GetString("category_name"),
                EventName = r.GetString("event_name"),
                Description = r.IsDBNull(r.GetOrdinal("description")) ? null : r.GetString("description"),
                EventDate = r.GetDateTime("event_date"),
                Venue = r.GetString("venue"),
                Price = r.GetInt32("price"),
                Capacity = r.GetInt32("capacity"),
                Status = r.GetString("status"),
                CreatedAt = r.GetDateTime("created_at"),
                ReservedCount = r.GetInt32("reserved_count")
            };
            list.Add(ev);
        }
        return list;
    }

    public async Task<Event?> GetEventByIdAsync(int id)
    {
        var list = await GetEventsAsync();
        // simpler: direct query
        using var conn = Conn();
        await conn.OpenAsync();
        var sql = @"SELECT e.event_id,e.category_id,c.category_name,e.event_name,e.description,e.event_date,e.venue,e.price,e.capacity,e.status,e.created_at,
                    (SELECT COALESCE(SUM(quantity),0) FROM reservations r WHERE r.event_id=e.event_id AND r.status='予約済') AS reserved_count
                    FROM events e JOIN categories c ON e.category_id=c.category_id WHERE e.event_id=@id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var r = await cmd.ExecuteReaderAsync();
        if (await r.ReadAsync())
        {
            return new Event {
                EventId = r.GetInt32("event_id"),
                CategoryId = r.GetInt32("category_id"),
                CategoryName = r.GetString("category_name"),
                EventName = r.GetString("event_name"),
                Description = r.IsDBNull(r.GetOrdinal("description")) ? null : r.GetString("description"),
                EventDate = r.GetDateTime("event_date"),
                Venue = r.GetString("venue"),
                Price = r.GetInt32("price"),
                Capacity = r.GetInt32("capacity"),
                Status = r.GetString("status"),
                CreatedAt = r.GetDateTime("created_at"),
                ReservedCount = r.GetInt32("reserved_count")
            };
        }
        return null;
    }

    public async Task<int> InsertEventAsync(Event ev)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(@"INSERT INTO events (category_id,event_name,description,event_date,venue,price,capacity,status)
            VALUES (@cat,@name,@desc,@date,@venue,@price,@cap,@status); SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@cat", ev.CategoryId);
        cmd.Parameters.AddWithValue("@name", ev.EventName);
        cmd.Parameters.AddWithValue("@desc", (object?)ev.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@date", ev.EventDate);
        cmd.Parameters.AddWithValue("@venue", ev.Venue);
        cmd.Parameters.AddWithValue("@price", ev.Price);
        cmd.Parameters.AddWithValue("@cap", ev.Capacity);
        cmd.Parameters.AddWithValue("@status", ev.Status);
        var id = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task UpdateEventAsync(Event ev)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(@"UPDATE events SET category_id=@cat,event_name=@name,description=@desc,event_date=@date,venue=@venue,price=@price,capacity=@cap,status=@status
            WHERE event_id=@id", conn);
        cmd.Parameters.AddWithValue("@cat", ev.CategoryId);
        cmd.Parameters.AddWithValue("@name", ev.EventName);
        cmd.Parameters.AddWithValue("@desc", (object?)ev.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@date", ev.EventDate);
        cmd.Parameters.AddWithValue("@venue", ev.Venue);
        cmd.Parameters.AddWithValue("@price", ev.Price);
        cmd.Parameters.AddWithValue("@cap", ev.Capacity);
        cmd.Parameters.AddWithValue("@status", ev.Status);
        cmd.Parameters.AddWithValue("@id", ev.EventId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteOrStopEventAsync(int id, bool hardDelete = false)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        if (hardDelete)
        {
            using var cmd = new MySqlCommand("DELETE FROM events WHERE event_id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }
        else
        {
            using var cmd = new MySqlCommand("UPDATE events SET status='公開停止' WHERE event_id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // ---------- Reservations ----------
    public async Task<string> GenerateReservationCodeAsync()
    {
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM reservations WHERE reservation_code LIKE @p", conn);
        cmd.Parameters.AddWithValue("@p", "R" + datePart + "%");
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return $"R{datePart}{(count + 1):D4}";
    }

    public async Task<int> InsertReservationAsync(Reservation res)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(@"INSERT INTO reservations (reservation_code,user_id,event_id,quantity,total_price,status)
            VALUES (@code,@uid,@eid,@qty,@total,'予約済'); SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@code", res.ReservationCode);
        cmd.Parameters.AddWithValue("@uid", res.UserId);
        cmd.Parameters.AddWithValue("@eid", res.EventId);
        cmd.Parameters.AddWithValue("@qty", res.Quantity);
        cmd.Parameters.AddWithValue("@total", res.TotalPrice);
        var id = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task<List<Reservation>> GetReservationsAsync(int? userId = null, int? eventId = null, string? status = null, string? userName = null, string? eventName = null)
    {
        var list = new List<Reservation>();
        var where = new List<string>();
        var sql = @"SELECT r.reservation_id,r.reservation_code,r.user_id,u.user_code,u.name AS user_name,r.event_id,e.event_name,e.event_date,r.quantity,r.total_price,r.status,r.reserved_at
                    FROM reservations r JOIN users u ON r.user_id=u.user_id JOIN events e ON r.event_id=e.event_id";
        if (userId.HasValue) where.Add("r.user_id=@uid");
        if (eventId.HasValue) where.Add("r.event_id=@eid");
        if (!string.IsNullOrWhiteSpace(status)) where.Add("r.status=@status");
        if (!string.IsNullOrWhiteSpace(userName)) where.Add("u.name LIKE @uname");
        if (!string.IsNullOrWhiteSpace(eventName)) where.Add("e.event_name LIKE @ename");
        if (where.Count > 0) sql += " WHERE " + string.Join(" AND ", where);
        sql += " ORDER BY r.reserved_at DESC";

        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        if (userId.HasValue) cmd.Parameters.AddWithValue("@uid", userId.Value);
        if (eventId.HasValue) cmd.Parameters.AddWithValue("@eid", eventId.Value);
        if (!string.IsNullOrWhiteSpace(status)) cmd.Parameters.AddWithValue("@status", status);
        if (!string.IsNullOrWhiteSpace(userName)) cmd.Parameters.AddWithValue("@uname", "%" + userName + "%");
        if (!string.IsNullOrWhiteSpace(eventName)) cmd.Parameters.AddWithValue("@ename", "%" + eventName + "%");
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            list.Add(new Reservation {
                ReservationId = r.GetInt32("reservation_id"),
                ReservationCode = r.GetString("reservation_code"),
                UserId = r.GetInt32("user_id"),
                UserCode = r.GetString("user_code"),
                UserName = r.GetString("user_name"),
                EventId = r.GetInt32("event_id"),
                EventName = r.GetString("event_name"),
                EventDate = r.GetDateTime("event_date"),
                Quantity = r.GetInt32("quantity"),
                TotalPrice = r.GetInt32("total_price"),
                Status = r.GetString("status"),
                ReservedAt = r.GetDateTime("reserved_at")
            });
        }
        return list;
    }

    public async Task<Reservation?> GetReservationByIdAsync(int id)
    {
        var list = await GetReservationsAsync();
        return list.FirstOrDefault(x => x.ReservationId == id);
    }

    public async Task UpdateReservationStatusAsync(int id, string status)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("UPDATE reservations SET status=@s WHERE reservation_id=@id", conn);
        cmd.Parameters.AddWithValue("@s", status);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> GetReservedCountAsync(int eventId)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("SELECT COALESCE(SUM(quantity),0) FROM reservations WHERE event_id=@eid AND status='予約済'", conn);
        cmd.Parameters.AddWithValue("@eid", eventId);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    // ---------- Aggregates (bonus) ----------
    public async Task<int> GetSalesTotalAsync()
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand("SELECT COALESCE(SUM(total_price),0) FROM reservations WHERE status='予約済'", conn);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<(string EventName, int Count)>> GetPopularEventsAsync(int top = 5)
    {
        var list = new List<(string,int)>();
        using var conn = Conn();
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(@"SELECT e.event_name, COALESCE(SUM(r.quantity),0) AS cnt
            FROM events e LEFT JOIN reservations r ON e.event_id=r.event_id AND r.status='予約済'
            GROUP BY e.event_id,e.event_name ORDER BY cnt DESC LIMIT @top", conn);
        cmd.Parameters.AddWithValue("@top", top);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            list.Add((r.GetString("event_name"), r.GetInt32("cnt")));
        return list;
    }
}
