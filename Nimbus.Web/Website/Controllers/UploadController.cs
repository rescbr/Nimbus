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
        readonly string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        readonly string[] officeExtensions = { ".docx", ".xls", ".xlsx", ".pptx", ".ppt", ".pps", ".ppsx" };
        readonly string[] pdfExtensions = { ".pdf" };

        [HttpGet]
        public ActionResult Index(string type = "", string field = "", bool popup = false, int w = 300, int h = 200)
        {
            var model = new UploadModel();
            if (type == "office")
                model.UploadAction = "upload";
            //else ifs de imagem, etc.
            else
                model.UploadAction = "upload";

            model.ReturnPreviewWidth = w;
            model.ReturnPreviewHeight = h;
            model.ReturnUploadField = field;
            model.isPopUp = popup;
            return View("Upload", model);
        }

        [HttpPost]
        public ActionResult Upload(string field = "", bool popup = false, int w = 300, int h = 200)
        {
            if (Request.Files.Count != 1 || Request.Files[0].ContentLength == 0)
            {
                var errorModel = new UploadModel(){
                    UploadAction = "upload",
                    ReturnUploadField = field,
                    ReturnPreviewWidth = w,
                    ReturnPreviewHeight = h,
                    isPopUp = popup,
                    isFatalError = true,
                    ErrorMessage = "Arquivo não enviado."
                };

                return View("Upload", errorModel);
            }

            var file = Request.Files[0];
            if (file.ContentLength > 5 * 1024 * 1024) // 5MB
            {
                var errorModel = new UploadModel()
                {
                    UploadAction = "upload",
                    ReturnUploadField = field,
                    ReturnPreviewWidth = w,
                    ReturnPreviewHeight = h,
                    isPopUp = popup,
                    isFatalError = true,
                    ErrorMessage = "O tamanho do arquivo não pode ser superior a 5MB."
                };

                return View("Upload", errorModel);
            }
            
            //nome do arquivo: /container/userid/md5(timestamp + nome arquivo).extensao
            var filename = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName).ToLower();
            var timeFileName = DateTime.UtcNow.ToFileTimeUtc().ToString() + filename;

            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            md5.ComputeHash(Encoding.Unicode.GetBytes(timeFileName));
            var uploadFileName = NimbusUser.UserId + "/" + Base32.ToString(md5.Hash).ToLower() + extension;

            var blob = new AzureBlob(Const.Azure.TopicContainer, uploadFileName);
            blob.UploadStreamToAzure(file.InputStream);

            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://").Replace("***REMOVED***", "storage.portalnimbus.com.br");

            var previewModel = new UploadModel()
            {
                Url = pathFinal,
                UploadAction = "upload",
                ReturnUploadField = field,
                ReturnPreviewWidth = w,
                ReturnPreviewHeight = h,
                isPopUp = popup,
                isFatalError = false,
                ErrorMessage = null
            };

            if (imageExtensions.Contains(extension))
                previewModel.PreviewType = UploadModel.PreviewTypeEnum.image;
            else if (officeExtensions.Contains(extension))
                previewModel.PreviewType = UploadModel.PreviewTypeEnum.office;
            else if (pdfExtensions.Contains(extension))
                previewModel.PreviewType = UploadModel.PreviewTypeEnum.pdf;
            else
                previewModel.PreviewType = UploadModel.PreviewTypeEnum.other;


            return View("UploadFinished", previewModel);
        }

        public class UploadModel
        {
            public enum PreviewTypeEnum
            {
                image,
                office,
                pdf,
                other
            }
            public string UploadAction { get; set; }
            public string ReturnUploadField { get; set; }
            public int ReturnPreviewWidth { get; set; }
            public int ReturnPreviewHeight { get; set; }
            public bool isPopUp { get; set; }
            public bool isFatalError { get; set; }
            public string ErrorMessage { get; set; }
            public PreviewTypeEnum PreviewType { get; set; }
            public string Url { get; set; }
        }
    }
}