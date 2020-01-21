# N2Image Agent + Azure Blob 2020

Before You Know
----
這是我之前開發的 https://github.com/donma/N2ImageAgent.AzureBlob ，為何不去修改原本的版本呢？因為，有些朋友已經用在線上的專案，如果更改會很麻煩，加上我很懶，於是就保留遠本的重新改些一份 為 2020 版本，基於原本的再去改寫得更符合大家給我的建議

改寫後主要修正下列幾點 : 

* 更改存在 Blob 上面的結構，支援多專案共用，用 ProjectName 區隔開來

  於 Blob 上結構如下 : 

  ```
  Blob Container
        -Project1
            -source
              -images
                -Image Source Files
              -info
                -Image Source Infomation
            -thumbs
              -100_100
                -All Image Thumbnail size in 100x100
              -200_200
                -All Image Thumbnail size in 200x200
              -...
        -Project2
            -source
              -images
                -Image Source Files
              -info
                -Image Source Infomation
            -thumbs
              -100_100
                -All Image Thumbnail size in 100x100
              -200_200
                -All Image Thumbnail size in 200x200
              -...
   ```           

* 如果短時間內取過，且時效還在記憶體中 cache 圖片的網址，避免一直跟 blob 去確定檔案是否存在
* 優化 appsettings.json
* 升級到 .net core 3.1

特別感謝 Eleven Hsiao 幫忙優化

Intro
----
這是一套基於 .net Core 3.1 + Azure Storage Blob , 這主要功能可以幫你把圖放到 Blob 上面 

你可以輕易透過 http://yourdomain.com/source/[PROJECT_NAME]/[IMAGE_ID] 得到你的圖片

如果您想要圖片圖片變成 寬度100高度隨比例縮放，只需要透過 http://yourdomain.com/image/[PROJECT_NAME]/[IMAGE_ID]/100 取得

如果您想要圖片圖片變成 高度200寬度隨比例縮放，只需要透過 http://yourdomain.com/image/[PROJECT_NAME]/[IMAGE_ID]/0/200 取得

如果不設定，預設會押時間到 UTC當天的 23:59:59 ，避免瀏覽器因為無 cache 導致要重複 loading.

您甚至可以從 http://yourdomain.com/info/[PROJECT_NAME]/[IMAGE_ID] 得到該圖片的原始資訊像是這樣


Image Info Sample
----
```json
{"Id":"01d1z3nrvgzhyde8qc9dvjg88e","Width":5120,"Height":2466,"Extension":"jpeg","Tag":"測試"}
```

使用教學
----
首先，您得先看一下 appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "azure_blob_connectionstring": "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT_NAME;AccountKey=YOUR_ACCOUNT_KEY;EndpointSuffix=core.windows.net",
  "blobcontainer": "n2imageagent2020",
  "uploadtoken": "your_token",
  "errorimage": "https://upload.wikimedia.org/wikipedia/commons/thumb/5/57/Blue_Screen_of_Death.png/800px-Blue_Screen_of_Death.png",
  "notfound": "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/600px-No_image_available.svg.png",
  "projectsinfo": [
    {
      "Key": "PROJECT_NAME_WITH_UNLIMIT_TIME",
      "Value": 0
    },
    {
      "Key": "PROJECT_NAME_WITH_LIMIT_10_SECONDS",
      "Value": 10
    },
    {
      "Key": "SAMPLE1",
      "Value": 4
    }
  ]
}


```
azure_blob_connectionstring : 您在 Azure 上面的 connection string 

blobcontainer : 所建立起來的 blob name (請注意，這邊 azure 那邊規定是全小寫，不可以有特殊符號)

uploadtoken :  使用者上傳需要給的 token

user_token_life_seconds : 使用者看到圖片，之後多久之後那張就會失效 

errorimage : 發生錯誤去的圖片網址

notfound : 找不到圖片去的網址

projectsinfo: 請對應專案名稱，全大寫，如果您的專案是 SAMPLE1 並且設定為每張圖只能活4秒，就是設定如範本 SAMPLE1 ，如果您沒有設定或是設定為0 則會押 token 至當天。

C# Upload Sample
----
```C#
        private string UploadImage(string file)
        {
            var src = System.IO.File.ReadAllBytes(file);
            Stream  stream = new MemoryStream(src);
            
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            
            HttpContent fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = "xxx.jpg" };
            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(new StringContent("your_token"), "token");
                formData.Add(new StringContent("SAMPLE1"), "projectname"); //new add in 2020
                formData.Add(new StringContent("測試"), "tag");
                // 如果你指定filename 他就會覆蓋原本的圖片
                // formData.Add(new StringContent("testgif"), "filename");
                // 但是如果你這樣做了，原本的快取圖不會更改，請您需要透過管理工具，把 
                // BlobContainer/[PROJECT_NAME]/thumbs/width_height/[ID].gif 檔案刪除重新製作縮圖
                formData.Add(fileStreamContent, "file");
                // Remember change your domain to https://yourdomain.com/api/upload to upload image.
                var response = client.PostAsync("https://localhost:44325/api/upload", formData).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }
        
        
         var result = UploadImage(AppDomain.CurrentDomain.BaseDirectory + "sample.jpg");
         
         //response success 
         //success:imageid
         //sample
         //success:01d1z45bm0nw8r4hfqcr6zbv26
         
         //response error
         //error:error result
         //sample
         //error:token null
```




[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)


