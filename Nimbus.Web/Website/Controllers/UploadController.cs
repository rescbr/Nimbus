using Nimbus.Web.Security;
using Nimbus.Web.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [Authorize]
    public class UploadController : NimbusWebController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View("Upload");
        }

        [HttpPost]
        public ActionResult Upload()
        {
            if (Request.Files.Count != 1)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Bad Request" });
            }

            var file = Request.Files[0];

            if (file.ContentLength == 0 || file.ContentLength > 5 * 1024 * 1024) // 5MB
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.RequestEntityTooLarge;
                return Json(new { error = "Too Large" });
            }
            
            //nome do arquivo: /container/userid/md5(timestamp + nome arquivo).extensao
            var filename = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            var timeFileName = DateTime.UtcNow.ToFileTimeUtc().ToString() + filename;

            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            md5.ComputeHash(Encoding.Unicode.GetBytes(timeFileName));
            var uploadFileName = NimbusUser.UserId + "/" + Base32.ToString(md5.Hash).ToLower() + extension;

            var blob = new AzureBlob(Const.Azure.TopicContainer, uploadFileName);
            blob.UploadStreamToAzure(file.InputStream);

            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://");

            //pega o nome do arquivo
            //nome final = onde vai ser armazendo
            //pega o caminho da pasta que vai ser gravado o arquivo e sava
            //retorna a img JA salva  para o json colocar na tela 

            return Json(new { url = pathFinal });
        }      
    }
}