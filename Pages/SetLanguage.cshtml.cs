using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventTicketSystem.Pages;

public class SetLanguageModel : PageModel
{
    public IActionResult OnGet(string culture, string returnUrl = "/")
    {
        if (culture != "ja-JP" && culture != "en-US") culture = "ja-JP";
        Response.Cookies.Append("lang", $"c={culture}|uic={culture}", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            HttpOnly = false
        });
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl)) returnUrl = "/";
        return LocalRedirect(returnUrl);
    }
}
