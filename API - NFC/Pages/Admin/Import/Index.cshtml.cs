using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API___NFC.Pages.Admin.Import
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
