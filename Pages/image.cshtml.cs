using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace N2ImageAgent.AzureBlob.Pages
{
    public class imageModel : PageModel
    {
        private int _w;
        private int _h;
        private string _projectname;
        private int _keepseconds;

        public async Task<ActionResult> OnGet(string projectname, string id, string w, string h)
        {

            if (string.IsNullOrEmpty(id))
            {
              //  Response.Redirect(Startup.ErrorImage);
                return new RedirectResult(Startup.ErrorImage, true);
            }
            if (string.IsNullOrEmpty(projectname))
            {
                // Response.Redirect(Startup.ErrorImage);
                return new RedirectResult(Startup.ErrorImage, true);
            }
            _projectname = projectname.Trim().ToUpper();

            #region PreHanlder Check Cache

            if (Startup.MemCacheUrlPool.ContainsKey(w + "_" + h + "_" + id))
            {
                try
                {
                    var tmpSign = Startup.MemCacheUrlPool.GetValueOrDefault(w + "_" + h + "_" + id);

                    if (tmpSign.UTCExpire > DateTime.UtcNow)
                    {
                        //  Response.Redirect(tmpSign.Url);
                        return new RedirectResult(tmpSign.Url, true);
                    }
                    else
                    {
                        Startup.MemCacheUrlPool.TryRemove(w + "_" + h + "_" + id, out _);
                    }
                }
                catch
                {

                }
            }

            #endregion


            var res = BlobUtility.IsFileExisted(id + ".gif", _projectname);

            if (res)
            {
                _keepseconds = Startup.GetProjectKeepSeconds(_projectname);
                int.TryParse(w, out _w);
                int.TryParse(h, out _h);




                #region  原圖處理 : _w=0 , _h=0


                if (_w == 0 && _h == 0)
                {
                     return new RedirectResult("/source/" + _projectname+"/" + id, true); 
                }

                #endregion




                if (BlobUtility.IsFileExisted(id + ".gif", _projectname, "thumbs/" + w + "_" + h))
                {
                    var path = "";
                    var para = "";
                    DateTime checkDate = DateTime.UtcNow;
                    BlobUtility.GetUriAndPermission(id + ".gif", out path, out para, out checkDate, _keepseconds, _projectname, "thumbs/" + w + "_" + h);

                    //Create Cache
                    Startup.MemCacheUrlPool.TryRemove(w + "_" + h + "_" + id, out _);
                    var tmpCache = new Models.CacheInfo { Url = path + para, UTCExpire = checkDate };
                    Startup.MemCacheUrlPool.TryAdd(w + "_" + h + "_" + id, tmpCache);

                    //  Response.Redirect(path + para);
                    return new RedirectResult(path + para, true);
                }

                #region  寬圖處理 : _w>0 , _h=0

                var source = BlobUtility.DownloadFileFromBlob(_projectname, id + ".gif");
                var info = BlobUtility.ReadInfoFromBlob(_projectname, id);

                var random = NUlid.Ulid.NewUlid().ToString().ToLower();

                System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar);
                System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar + "thumbswap");
                if (_w > 0 && _h == 0)
                {
                    var thumbHandler = new ImageUtility();
                    var source2 = thumbHandler.MakeThumbnail(source, _w, _h, "W");
                    thumbHandler.ImageSaveFile(source2, AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar + "thumbswap" + Path.DirectorySeparatorChar + id + "_" + random + ".gif", info.Extension);
                    BlobUtility.UpoloadImage(id, AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar + "thumbswap" + Path.DirectorySeparatorChar + id + "_" + random + ".gif", _projectname, "thumbs/" + w + "_" + h);
                    source.Dispose();
                    source2.Dispose();
                }

                #endregion


                #region  高圖處理 : _w=0 , _h>0

                else if (_w == 0 && _h > 0)
                {
                    var thumbHandler = new ImageUtility();
                    var source2 = thumbHandler.MakeThumbnail(source, _w, _h, "H");
                    thumbHandler.ImageSaveFile(source2, AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar + "thumbswap" + Path.DirectorySeparatorChar + id + "_" + random + ".gif", info.Extension);
                    BlobUtility.UpoloadImage(id, AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar + "thumbswap" + Path.DirectorySeparatorChar + id + "_" + random + ".gif", _projectname, "thumbs/" + w + "_" + h);
                    source.Dispose();
                    source2.Dispose();

                }

                #endregion

                #region  強迫處理 : _w>0 , _h>0

                else if (_w > 0 && _h > 0)
                {
                    var thumbHandler = new ImageUtility();
                    var source2 = thumbHandler.MakeThumbnail(source, _w, _h, "WH");
                    thumbHandler.ImageSaveFile(source2, AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar + "thumbswap" + Path.DirectorySeparatorChar + id + "_" + random + ".gif", info.Extension);
                    BlobUtility.UpoloadImage(id, AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar + "thumbswap" + Path.DirectorySeparatorChar + id + "_" + random + ".gif", _projectname, "thumbs/" + w + "_" + h);
                    source.Dispose();
                    source2.Dispose();
                }


                var tmpPath = "";
                var tmpPara = "";


                DateTime checkDate2 = DateTime.UtcNow;
                BlobUtility.GetUriAndPermission(id + ".gif", out tmpPath, out tmpPara, out checkDate2, _keepseconds, _projectname, "thumbs/" + w + "_" + h);

                //Create Cache
                Startup.MemCacheUrlPool.TryRemove(w + "_" + h + "_" + id, out _);
                var tmpCache2 = new Models.CacheInfo { Url = tmpPath + tmpPara, UTCExpire = checkDate2 };
                Startup.MemCacheUrlPool.TryAdd(w + "_" + h + "_" + id, tmpCache2);


                System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + _projectname + Path.DirectorySeparatorChar + "thumbswap" + Path.DirectorySeparatorChar + id + "_" + random + ".gif");
                // Response.Redirect(tmpPath + tmpPara);
                return new RedirectResult(tmpPath + tmpPara, true);
                #endregion

            }
            else
            {
                //Response.Redirect(Startup.NotFoundImage);
                return new RedirectResult(Startup.NotFoundImage, true);
            }


        }
    }
}
