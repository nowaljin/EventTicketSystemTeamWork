using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Events;

public class EditModel : PageModel
{
    private readonly DbService _db;
    public EditModel(DbService db) { _db = db; }

    [BindProperty]
    public EventTicketSystem.Models.Event Event { get; set; } = new();

    public List<EventTicketSystem.Models.Category> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ev = await _db.GetEventByIdAsync(id);
        if (ev == null) return NotFound();
        Event = ev;
        Categories = await _db.GetCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await _db.GetCategoriesAsync();
        if (string.IsNullOrWhiteSpace(Event.EventName))
            ModelState.AddModelError("Event.EventName", "イベント名は必須です。");
        if (string.IsNullOrWhiteSpace(Event.Venue))
            ModelState.AddModelError("Event.Venue", "会場は必須です。");
        if (Event.EventDate == default)
            ModelState.AddModelError("Event.EventDate", "開催日時は必須です。");
        if (Event.Price < 0)
            ModelState.AddModelError("Event.Price", "料金は0円以上です。");
        if (Event.Capacity < 1)
            ModelState.AddModelError("Event.Capacity", "定員は1以上です。");

        if (!ModelState.IsValid) return Page();

        await _db.UpdateEventAsync(Event);
        TempData["Message"] = "イベントを更新しました。";
        return RedirectToPage("/Admin/Events");
    }
}
