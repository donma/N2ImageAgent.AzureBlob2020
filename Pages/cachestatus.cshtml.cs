using Microsoft.AspNetCore.Mvc.RazorPages;

namespace N2ImageAgent.AzureBlob
{
    public class cachestatusModel : PageModel
    {
        public void OnGet()
        {
            //For Developer Check Use.

            //var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
            //if (syncIOFeature != null)
            //{
            //    syncIOFeature.AllowSynchronousIO = true;
            //}


            //Response.Body.Write(System.Text.Encoding.UTF8.GetBytes("PROJECT SETTING INFO \r\n"));

            //Response.Body.Write(System.Text.Encoding.UTF8.GetBytes(
            //   JsonConvert.SerializeObject(Startup.ProjectKeepSecondsSettings)
            //   ));



            //Response.Body.Write(System.Text.Encoding.UTF8.GetBytes(" \r\nCACHE INFO \r\n"));


            //Response.Body.Write(System.Text.Encoding.UTF8.GetBytes(
            //   JsonConvert.SerializeObject(Startup.MemCacheUrlPool)
            //   ));



        }
    }
}