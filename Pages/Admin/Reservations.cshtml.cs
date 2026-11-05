using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Admin;

public class ReservationsModel : PageModel
{
    private readonly DbService _db;
    public ReservationsModel(DbService db) { _db = db; }

    public List<EventTicketSystem.Models.Reservation> Reservations { get; set; } = new();
    public int SalesTotal { get; set; }
    public List<(string EventName, int Count)> Popular { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? EventName { get; set; }
    [BindProperty(SupportsGet = true)] public string? UserName { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetString("Role") != "admin")
        {
            TempData["Error"] = "管理者権限が必要です。";
            return RedirectToPage("/Users/Select");
        }
        Reservations = await _db.GetReservationsAsync(eventName: EventName, userName: UserName, status: string.IsNullOrWhiteSpace(Status) ? null : Status);
        SalesTotal = await _db.GetSalesTotalAsync();
        Popular = await _db.GetPopularEventsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        if (HttpContext.Session.GetString("Role") != "admin") return RedirectToPage("/Users/Select");
        await _db.UpdateReservationStatusAsync(id, "キャンセル済");
        TempData["Message"] = "状態をキャンセル済に変更しました。";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRestoreAsync(int id)
    {
        if (HttpContext.Session.GetString("Role") != "admin") return RedirectToPage("/Users/Select");
        await _db.UpdateReservationStatusAsync(id, "予約済");
        TempData["Message"] = "状態を予約済に戻しました。";
        return RedirectToPage();
    }
}
