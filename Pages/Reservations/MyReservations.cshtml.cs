using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Reservations;

public class MyReservationsModel : PageModel
{
    private readonly DbService _db;
    public MyReservationsModel(DbService db) { _db = db; }

    public List<EventTicketSystem.Models.Reservation> Reservations { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetInt32("UserId") is null)
        {
            TempData["Error"] = "利用者を選択してください。";
            return RedirectToPage("/Users/Select");
        }
        var uid = HttpContext.Session.GetInt32("UserId")!.Value;
        Reservations = await _db.GetReservationsAsync(userId: uid);
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        if (HttpContext.Session.GetInt32("UserId") is null) return RedirectToPage("/Users/Select");
        var uid = HttpContext.Session.GetInt32("UserId")!.Value;
        var all = await _db.GetReservationsAsync(userId: uid);
        var r = all.FirstOrDefault(x => x.ReservationId == id);
        if (r == null || r.Status != "予約済") { TempData["Error"] = "キャンセルできません。"; return RedirectToPage(); }
        if (r.EventDate <= DateTime.Now) { TempData["Error"] = "開催後の予約はキャンセルできません。"; return RedirectToPage(); }
        await _db.UpdateReservationStatusAsync(id, "キャンセル済");
        TempData["Message"] = "予約をキャンセルしました。";
        return RedirectToPage();
    }
}
