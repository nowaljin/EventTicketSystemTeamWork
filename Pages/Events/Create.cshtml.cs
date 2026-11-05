using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Pages.Events;

public class CreateModel : PageModel
{
    private readonly DbService _db;
    public CreateModel(DbService db) { _db = db; }

    [BindProperty]
    public EventTicketSystem.Models.Event Event { get; set; } = new() { EventDate = DateTime.Now.AddDays(7), Status = "公開中" };

    public List<EventTicketSystem.Models.Category> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        Categories = await _db.GetCategoriesAsync();
        if (!Categories.Any()) return;
        Event.CategoryId = Categories.First().CategoryId;
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

        await _db.InsertEventAsync(Event);
        TempData["Message"] = "イベントを登録しました。";
        return RedirectToPage("/Admin/Events");
    }
}
