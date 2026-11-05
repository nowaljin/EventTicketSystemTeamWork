using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Reservations;

public class CreateModel : PageModel
{
    private readonly DbService _db;
    public CreateModel(DbService db) { _db = db; }

    public EventTicketSystem.Models.Event? Event { get; set; }

    [BindProperty(SupportsGet = true)]
    public int EventId { get; set; }

    [BindProperty]
    public int Quantity { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(int? eventId)
    {
        if (eventId.HasValue) EventId = eventId.Value;
        if (HttpContext.Session.GetInt32("UserId") is null)
        {
            TempData["Error"] = "利用者を選択してください。";
            return RedirectToPage("/Users/Select");
        }
        Event = await _db.GetEventByIdAsync(EventId);
        if (Event == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HttpContext.Session.GetInt32("UserId") is null)
        {
            TempData["Error"] = "利用者を選択してください。";
            return RedirectToPage("/Users/Select");
        }
        Event = await _db.GetEventByIdAsync(EventId);
        if (Event == null) return NotFound();

        if (Quantity < 1)
            ModelState.AddModelError("Quantity", "予約枚数は1以上です。");
        if (Quantity > Event.RemainingSeats)
            ModelState.AddModelError("Quantity", "残席数を超える予約はできません。");

        if (!ModelState.IsValid) return Page();

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var code = await _db.GenerateReservationCodeAsync();
        var totalPrice = Event.Price * Quantity; // server-side calc

        var res = new EventTicketSystem.Models.Reservation
        {
            ReservationCode = code,
            UserId = userId,
            EventId = EventId,
            Quantity = Quantity,
            TotalPrice = totalPrice,
            Status = "予約済"
        };
        var newId = await _db.InsertReservationAsync(res);

        // store for complete page
        TempData["ReservationId"] = newId;
        return RedirectToPage("/Reservations/Complete", new { id = newId });
    }
}
