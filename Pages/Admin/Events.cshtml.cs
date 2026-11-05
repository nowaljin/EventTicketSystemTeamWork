using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Admin;

public class EventsModel : PageModel
{
    private readonly DbService _db;
    public EventsModel(DbService db) { _db = db; }

    public List<EventTicketSystem.Models.Event> Events { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetString("Role") != "admin")
        {
            TempData["Error"] = "管理者権限が必要です。";
            return RedirectToPage("/Users/Select");
        }
        Events = await _db.GetEventsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostStopAsync(int id)
    {
        if (HttpContext.Session.GetString("Role") != "admin") return RedirectToPage("/Users/Select");
        await _db.DeleteOrStopEventAsync(id, false);
        TempData["Message"] = "公開停止しました。";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (HttpContext.Session.GetString("Role") != "admin") return RedirectToPage("/Users/Select");
        await _db.DeleteOrStopEventAsync(id, true);
        TempData["Message"] = "削除しました。";
        return RedirectToPage();
    }
}
