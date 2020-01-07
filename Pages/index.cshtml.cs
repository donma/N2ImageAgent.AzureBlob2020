using Microsoft.AspNetCore.Mvc.RazorPages;

namespace N2ImageAgent.AzureBlob
{
    public class indexModel : PageModel
    {
        public void OnGet()
        {
            Response.Redirect("/index.html");
        }
    }
}