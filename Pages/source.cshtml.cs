using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace N2ImageAgent.AzureBlob.Pages
{
    public class sourceModel : PageModel
    {
        public void OnGet(string projectname, string id)
        {


            if (string.IsNullOrEmpty(id))
            {
                Response.Redirect(Startup.ErrorImage);
            }
            else
            {

                if (Startup.MemCacheUrlPool.ContainsKey("0_0_" + id))
                {
                    try
                    {
                        var tmpSign = Startup.MemCacheUrlPool.GetValueOrDefault("0_0_" + id);

                        if (tmpSign.UTCExpire > DateTime.UtcNow)
                        {
                            Response.Redirect(tmpSign.Url);
                        }
                        else
                        {
                            Startup.MemCacheUrlPool.TryRemove("0_0_" + id, out _);
                        }
                    }
                    catch
                    {

                    }
                }


                var res = BlobUtility.IsFileExisted(id + ".gif", projectname.Trim().ToUpper());
                var _keepseconds = Startup.GetProjectKeepSeconds(projectname.Trim().ToUpper());
                if (res)
                {
                    var path = "";
                    var para = "";
                    DateTime checkDate = DateTime.UtcNow;
                    BlobUtility.GetUriAndPermission(id + ".gif", out path, out para, out checkDate, _keepseconds, projectname);

                    Startup.MemCacheUrlPool.TryRemove("0_0_" + id, out _);
                    var tmpCache = new Models.CacheInfo { Url = path + para, UTCExpire = checkDate };
                    Startup.MemCacheUrlPool.TryAdd("0_0_" + id, tmpCache);

                    Response.Redirect(path + para);
                }
                else
                {
                    Response.Redirect(Startup.NotFoundImage);
                }

            }

        }
    }
}