using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Events;

public class DetailsModel : PageModel
{
    private readonly DbService _db;
    public DetailsModel(DbService db) { _db = db; }

    public EventTicketSystem.Models.Event? Event { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Event = await _db.GetEventByIdAsync(id);
        if (Event == null) return NotFound();
        return Page();
    }
}
