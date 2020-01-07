using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace N2ImageAgent.AzureBlob.Pages
{
    public class infoModel : PageModel
    {

        public void OnGet(string projectname, string id)
        {

            var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIOFeature != null)
            {
                syncIOFeature.AllowSynchronousIO = true;
            }


            if (string.IsNullOrEmpty(id))
            {
                Response.Body.Write(System.Text.Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(new Models.ImageInfo { Id = "ERROR", Tag = "NULL ID" })
                    ));


                return;
            }

            if (string.IsNullOrEmpty(projectname))
            {
                Response.Body.Write(System.Text.Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(new Models.ImageInfo { Id = "ERROR", Tag = "NULL PROJECTNAME" })
                    ));

                return;
            }


            Response.Redirect(BlobUtility.GetImageInfo(projectname.Trim().ToUpper(), id));

        }
    }
}