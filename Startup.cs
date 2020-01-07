using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace N2ImageAgent.AzureBlob
{
    public class Startup
    {


        public static string ServerToekn { get; set; }
        public static string BlobName { get; set; }
        public static string AzureStorageConnectionString { get; set; }

        public static string ErrorImage { get; set; }
        public static string NotFoundImage { get; set; }

        public static Dictionary<string, int> ProjectKeepSecondsSettings { get; set; }

        public static ConcurrentDictionary<string, Models.CacheInfo> MemCacheUrlPool { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddRazorPages();
            services.AddControllers();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().AddRazorPagesOptions(
                               opts => opts.Conventions.AddPageRoute("/source", "source/{projectname?}/{id?}")
                                                       .AddPageRoute("/image", "image/{projectname?}/{id?}/{w?}/{h?}")
                                                        .AddPageRoute("/info", "info/{projectname?}/{id?}")

           );


            //// If using Kestrel:
            //services.Configure<KestrelServerOptions>(options =>
            //{
            //    options.AllowSynchronousIO = true;
            //});

            //// If using IIS:
            //services.Configure<IISServerOptions>(options =>
            //{
            //    options.AllowSynchronousIO = true;
            //});
        }

        public static int GetProjectKeepSeconds(string projectName)
        {
            if (ProjectKeepSecondsSettings == null) return 0;

            if (ProjectKeepSecondsSettings.ContainsKey(projectName)) return ProjectKeepSecondsSettings[projectName];

            return 0;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            ServerToekn = Configuration.GetValue<string>("uploadtoken");
            BlobName = Configuration.GetValue<string>("blobcontainer");
            AzureStorageConnectionString = Configuration.GetValue<string>("azure_blob_connectionstring");
            ErrorImage = Configuration.GetValue<string>("errorimage");
            NotFoundImage = Configuration.GetValue<string>("notfound");


            //載入 appsettings.json  projectsinfo 的設定秒數，這寫法有點爛，改天有機會在好好研究 下
            ProjectKeepSecondsSettings = new Dictionary<string, int>();
            for (var i = 0; i <= int.MaxValue; i++)
            {
                if (!string.IsNullOrEmpty(Configuration["projectsinfo" + ":" + i + ":" + "Key"]))
                {
                    int tmpSec = 0;
                    int.TryParse(Configuration["projectsinfo" + ":" + i + ":" + "Value"], out tmpSec);
                    ProjectKeepSecondsSettings.Add(Configuration["projectsinfo" + ":" + i + ":" + "Key"].Trim().ToUpper(), tmpSec);
                }
                else
                {
                    break;
                }

            }


            MemCacheUrlPool = new ConcurrentDictionary<string, Models.CacheInfo>();


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //  app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });


        }
    }
}
