using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mobee.Server.Aspnet.Pages
{
    public class GetModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Redirect("https://github.com/username-not-important/Mobee/releases/download/WPF-Client-v0.6.1/Mobee.WPF.Client.v0.6.1-alpha.msi");
        }
    }
}
