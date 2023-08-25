using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mobee.Server.Aspnet.Pages
{
    public class InfoModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Redirect("https://epiclibrary.ir/Mobee");
        }
    }
}
