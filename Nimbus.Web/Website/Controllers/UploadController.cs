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
            else if (type == "topicimg")
                model.UploadAction = "topicimg";
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
        public ActionResult TopicImg(string field = "")
        {
            if (Request.Files.Count != 1 || Request.Files[0].ContentLength == 0)
            {
                var errorModel = new UploadModel()
                {
                    UploadAction = "topicimg",
                    ReturnUploadField = field,
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
                    UploadAction = "topicimg",
                    ReturnUploadField = field,
                    isFatalError = true,
                    ErrorMessage = "O tamanho do arquivo não pode ser superior a 5MB."
                };

                return View("Upload", errorModel);
            }


            //nome do arquivo: /container/tp150x100/md5(timestamp + nome arquivo).jpg
            var filename = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName).ToLower();
            var timeFileName = DateTime.UtcNow.ToFileTimeUtc().ToString() + filename;

            if (!imageExtensions.Contains(extension))
            {
                //nao fazer upload
                var errorModel = new UploadModel()
                {
                    UploadAction = "topicimg",
                    ReturnUploadField = field,
                    isFatalError = true,
                    ErrorMessage = "Não é permitido o envio de arquivos com extensão " + extension + "."
                };

                return View("Upload", errorModel);
            }

            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            md5.ComputeHash(Encoding.Unicode.GetBytes(timeFileName));
            var fileHash = Base32.ToString(md5.Hash).ToLower() + ".jpg";
            var uploadFileName150x100 = "tp150x100/" + fileHash;
            var uploadFileName60x60 = "tp60x60/" + fileHash;

            var image = new ImageManipulation(Request.Files[0].InputStream);

            //faz upload da imagem 150x100
            image.FitSize(150, 100);
            var imageStream = image.SaveToJpeg();
            var blob = new AzureBlob(Const.Azure.TopicContainer, uploadFileName150x100);
            blob.UploadStreamToAzure(imageStream);

            image.FitSize(60, 60);
            var image60x60Stream = image.SaveToJpeg();
            var blob60x60 = new AzureBlob(Const.Azure.TopicContainer, uploadFileName60x60);
            blob60x60.UploadStreamToAzure(image60x60Stream);

            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://").Replace("***REMOVED***", "storage.portalnimbus.com.br");

            var previewModel = new UploadModel()
            {
                Url = pathFinal,
                UploadAction = "topicimg",
                ReturnUploadField = field,
                isFatalError = false,
                ErrorMessage = null,
                PreviewType = UploadModel.PreviewTypeEnum.none
            };

            return View("UploadFinished", previewModel);

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

            UploadModel.PreviewTypeEnum previewType;

            if (imageExtensions.Contains(extension))
                previewType = UploadModel.PreviewTypeEnum.image;
            else if (officeExtensions.Contains(extension))
                previewType = UploadModel.PreviewTypeEnum.office;
            else if (pdfExtensions.Contains(extension))
                previewType = UploadModel.PreviewTypeEnum.pdf;
            else
            {
                //nao fazer upload
                var errorModel = new UploadModel()
                {
                    UploadAction = "upload",
                    ReturnUploadField = field,
                    ReturnPreviewWidth = w,
                    ReturnPreviewHeight = h,
                    isPopUp = popup,
                    isFatalError = true,
                    ErrorMessage = "Não é permitido o envio de arquivos com extensão " + extension + "."
                };

                return View("Upload", errorModel);
            }

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
                ErrorMessage = null,
                PreviewType = previewType
            };

            return View("UploadFinished", previewModel);
        }

        public class UploadModel
        {
            public enum PreviewTypeEnum
            {
                image,
                office,
                pdf,
                other,
                none
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