using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Events;

public class IndexModel : PageModel
{
    private readonly DbService _db;
    public IndexModel(DbService db) { _db = db; }

    public List<EventTicketSystem.Models.Event> Events { get; set; } = new();
    public List<EventTicketSystem.Models.Category> Categories { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? Name { get; set; }
    [BindProperty(SupportsGet = true)] public int CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Date { get; set; }
    [BindProperty(SupportsGet = true)] public string? Venue { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _db.GetCategoriesAsync();
        Events = await _db.GetEventsAsync(Name, CategoryId > 0 ? CategoryId : null, Date, Venue, onlyPublished: true);
    }
}
