using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Reservations;

public class CompleteModel : PageModel
{
    private readonly DbService _db;
    public CompleteModel(DbService db) { _db = db; }

    public EventTicketSystem.Models.Reservation? Reservation { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Reservation = await _db.GetReservationByIdAsync(id);
        if (Reservation == null) return NotFound();
        return Page();
    }
}
