using EventTicketSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages.Users;

public class SelectModel : PageModel
{
    private readonly DbService _db;
    public SelectModel(DbService db) { _db = db; }

    public List<EventTicketSystem.Models.User> Users { get; set; } = new();
    [BindProperty]
    public int SelectedUserId { get; set; }

    public async Task OnGetAsync()
    {
        Users = await _db.GetUsersAsync();
        if (Users.Any()) SelectedUserId = Users.First().UserId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var u = await _db.GetUserByIdAsync(SelectedUserId);
        if (u == null) return Page();
        HttpContext.Session.SetInt32("UserId", u.UserId);
        HttpContext.Session.SetString("UserName", u.Name);
        HttpContext.Session.SetString("Role", u.Role);
        TempData["Message"] = $"ようこそ、{u.Name} さん";
        return RedirectToPage("/Index");
    }
}
