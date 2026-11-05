namespace EventTicketSystem.Models;

public class User
{
    public int UserId { get; set; }
    public string UserCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "user";   // user / admin
    public DateTime CreatedAt { get; set; }
}

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
}

public class Event
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Venue { get; set; } = "";
    public int Price { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = "公開中";
    public DateTime CreatedAt { get; set; }

    // computed
    public int ReservedCount { get; set; }
    public int RemainingSeats => Capacity - ReservedCount;
}

public class Reservation
{
    public int ReservationId { get; set; }
    public string ReservationCode { get; set; } = "";
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public string UserCode { get; set; } = "";
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public int Quantity { get; set; }
    public int TotalPrice { get; set; }
    public string Status { get; set; } = "予約済";
    public DateTime ReservedAt { get; set; }
}
