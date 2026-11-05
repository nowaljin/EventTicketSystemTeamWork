using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages;

public class IndexModel : PageModel
{
    private readonly DbService _db;
    public IndexModel(DbService db) { _db = db; }

    public List<EventTicketSystem.Models.Event> UpcomingEvents { get; set; } = new();

    public async Task OnGetAsync()
    {
        var all = await _db.GetEventsAsync(onlyPublished: true);
        UpcomingEvents = all.Where(e => e.EventDate >= DateTime.Now.AddDays(-1)).ToList();
        if (!UpcomingEvents.Any()) UpcomingEvents = all.Take(10).ToList();
    }
}
