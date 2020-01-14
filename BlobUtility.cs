using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;

namespace N2ImageAgent.AzureBlob
{
    public static class BlobUtility
    {
        private static Microsoft.WindowsAzure.Storage.CloudStorageAccount CloudStorage;
        private static Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient CloudBlobClient;
        private static Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer CloudBlobContainer;

        static BlobUtility()
        {
            CloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(Startup.AzureStorageConnectionString);
            CloudBlobClient = CloudStorage.CreateCloudBlobClient();
            CloudBlobContainer = CloudBlobClient.GetContainerReference(Startup.BlobName);
            var res = CloudBlobContainer.CreateIfNotExistsAsync().Result;

        }

        public static Models.ImageInfo ReadInfoFromBlob(string projectName, string id)
        {
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentNullException("projectName");
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            
            projectName = projectName.ToUpper();
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                CloudBlobContainer.GetDirectoryReference(projectName + "/source/info");

            var result = cloudBlobDirectory.GetBlockBlobReference(id + ".json").DownloadTextAsync().Result;

            return JsonConvert.DeserializeObject<Models.ImageInfo>(result);
        }


        public static Image DownloadFileFromBlob(string projectName, string fileName, string blobPath = "source/images")
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentNullException("projectName");

            projectName = projectName.ToUpper();
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                CloudBlobContainer.GetDirectoryReference(projectName + "/" + blobPath);

            System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + projectName + Path.DirectorySeparatorChar + "downloadsource");

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + projectName + Path.DirectorySeparatorChar + "downloadsource" + Path.DirectorySeparatorChar + fileName))
            {
                cloudBlobDirectory.GetBlockBlobReference(fileName).DownloadToFileAsync(AppDomain.CurrentDomain.BaseDirectory + projectName + Path.DirectorySeparatorChar + "downloadsource" + Path.DirectorySeparatorChar + fileName, System.IO.FileMode.Create).GetAwaiter().GetResult();
            }
            var res = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + projectName + Path.DirectorySeparatorChar + "downloadsource" + Path.DirectorySeparatorChar + fileName);

            return res;

        }


        public static void GetUriAndPermission(string fileName, out string fileUri, out string signUriPara, out DateTime expireDateTime, int keepSeconds, string projectName, string blobPath = "source/images")
        {

            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                CloudBlobContainer.GetDirectoryReference(projectName + "/" + blobPath);

            projectName = projectName.ToUpper();
            var res = CloudBlobContainer.GetPermissionsAsync().Result;

            if (keepSeconds > 0)
            {
                expireDateTime = DateTime.UtcNow.AddSeconds(keepSeconds);
                var sharedPolicy = new Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy()
                {

                    SharedAccessExpiryTime = expireDateTime,
                    Permissions = Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions.Read,

                };

                signUriPara = CloudBlobContainer.GetSharedAccessSignature(sharedPolicy, null);
            }
            else
            {
                expireDateTime = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1);
                var sharedPolicy = new Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy()
                {
                    SharedAccessExpiryTime = expireDateTime,
                    Permissions = Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions.Read,

                };
                signUriPara = CloudBlobContainer.GetSharedAccessSignature(sharedPolicy, null);
            }

            fileUri = cloudBlobDirectory.GetBlockBlobReference(fileName).Uri.ToString();
        }

        public static bool IsFileExisted(string fileName, string projectName, string blobPath = "source/images")
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentNullException("projectName");

            projectName = projectName.ToUpper();
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                CloudBlobContainer.GetDirectoryReference(projectName + "/" + blobPath);

            return cloudBlobDirectory.GetBlockBlobReference(fileName).ExistsAsync().Result;

        }

        public static void UpoloadImage(string id, string localImagePath, string projectName, string filePath = "source/images/")
        {
            if (string.IsNullOrEmpty(localImagePath)) throw new ArgumentNullException("localImagePath");
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentNullException("projectName");

            projectName = projectName.ToUpper();
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                 CloudBlobContainer.GetDirectoryReference(projectName + "/" + filePath);

            var bFileInfo = cloudBlobDirectory.GetBlockBlobReference(id + ".gif");

            bFileInfo.Properties.ContentType = "image/gif";

            bFileInfo.UploadFromFileAsync(localImagePath).GetAwaiter().GetResult();

        }


        public static void UpoloadImageSource(string filePath, string projectName, string id)
        {
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentNullException("projectName");
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

            projectName = projectName.ToUpper();
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                CloudBlobContainer.GetDirectoryReference(projectName + "/" + "source/images/");



            var bFileInfo = cloudBlobDirectory.GetBlockBlobReference(id + ".gif");

            bFileInfo.Properties.ContentType = "image/gif";

            bFileInfo.UploadFromFileAsync(filePath).GetAwaiter().GetResult();

        }

        public static void UpoloadImageInfoSource(string filePath, string projectName, string id)
        {
            if (string.IsNullOrEmpty(projectName)) throw new ArgumentNullException("projectName");
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

            projectName = projectName.ToUpper();
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
               CloudBlobContainer.GetDirectoryReference(projectName + "/" + "source/info/");

            var bFileInfo = cloudBlobDirectory.GetBlockBlobReference(id + ".json");
            bFileInfo.Properties.ContentType = "application/json; charset=utf-8";
            bFileInfo.UploadTextAsync(System.IO.File.ReadAllText(filePath)).GetAwaiter().GetResult();

        }



        public static string GetImageInfo(string projectName, string id)
        {

            var cloudBlobDirectory =
          CloudBlobContainer.GetDirectoryReference(projectName + "/source/info");

            projectName = projectName.ToUpper();
            var res = CloudBlobContainer.GetPermissionsAsync().Result;

            var sharedPolicy = new Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = new DateTime(2099, 12, 31),
                Permissions = Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions.Read,
            };
            var signUriPara = CloudBlobContainer.GetSharedAccessSignature(sharedPolicy, null);
            var fileUri = cloudBlobDirectory.GetBlockBlobReference(id + ".json").Uri.ToString();

            return fileUri + signUriPara;

        }

    }
}
