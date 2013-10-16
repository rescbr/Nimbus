using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Nimbus.Web.API;
using Nimbus.Web.Website.Models;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;



namespace Nimbus.Web.Website.Controllers
{
    public class UserProfileController : NimbusWebController
    {
        [Authorize]
        public ActionResult Index()
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var userprofile = new UserProfileModel()
            {
                CurrentUser = NimbusUser,
                ChannelPaid = channelApi.UserChannelPaid(NimbusUser.UserId),
                User = userApi.showProfile(),
                ChannelFollow = channelApi.FollowsChannel(NimbusOrganization.Id),
                MyChannels = channelApi.MyChannel(),
                ReadLater = channelApi.showReadLaterChannel(NimbusOrganization.Id),
                Messages = msgApi.ReceivedMessages()
            };
            return View("UserProfile", userprofile);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Upload()
        {
            if (Request.Files.Count != 1)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Bad Request" });
            }

            if (Request.Files[0].ContentLength > 10 * 1024 * 1024) //10 MB
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.RequestEntityTooLarge;
                return Json(new { error = "Too Large" });
            }


            var nomeOriginal = Request.Files[0].FileName.Trim(new char[]{' ', '"' });
            var nomeFinal = "fullsize-" + NimbusUser.UserId + "-" +
                RenameToValidName(Path.GetFileNameWithoutExtension(nomeOriginal)) +
                Path.GetExtension(nomeOriginal);

            var imageStream = ResizeImage(Request.Files[0].InputStream, 786, 786);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(NimbusConfig.StorageAccount);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("avatarupload");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(nomeFinal);
            blockBlob.UploadFromStream(imageStream);
            
            var pathFinal = blockBlob.Uri.AbsoluteUri.Replace("https://", "http://");

            //pega o nome do arquivo
            //nome final = onde vai ser armazendo
            //pega o caminho da pasta que vai ser gravado o arquivo e sava
            //retorna a img JA salva  para o json colocar na tela 
            return Json(new { url = nomeFinal });            
        }

        [Authorize]
        [HttpPost]
        public ActionResult Crop()
        { //262 x 100
            var imagem_url = Request.Form["url"];
            var x1 = int.Parse(Request.Form["x1"]);
            var x2 = int.Parse(Request.Form["x2"]);
            var y1 = int.Parse(Request.Form["y1"]);
            var y2 = int.Parse(Request.Form["y2"]);

            //caso o cara corte a imagem, mudar o nome do arquivo a ser salvo
            var nomeFinal = "../uploads/imagem_crop" + Path.GetExtension(imagem_url);
            
            using (var response = new StreamReader(Server.MapPath(imagem_url)))
            {
                Bitmap imagem = new Bitmap(response.BaseStream);

                int largura = x2 - x1;
                int altura = y2 - y1;

                Bitmap target = new Bitmap(largura, altura);
                Rectangle cropRect = new Rectangle(x1, y1, largura, altura);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(imagem, new Rectangle(0, 0, largura, altura), cropRect, GraphicsUnit.Pixel);
                    //aqui deve chamar a função que 'compacta' a imagem corretamente
                    //aqui deve salvar no jeito certo -> no azure
                    using (var fileStream = new FileStream(Server.MapPath(nomeFinal), FileMode.OpenOrCreate))
                    {
                        target.Save(fileStream, imagem.RawFormat);
                        fileStream.Flush();
                    }

                }
            }
            //depois que salvar no azure retorna por json p mostrar na tela a imagem final
            return Json(new { imagem_recortada = nomeFinal });
        }

        /// <summary>
        /// Will transform "some $ugly ###url wit[]h spaces" into "some-ugly-url-with-spaces"
        /// </summary>
        string RenameToValidName(string phrase, int maxLength = 50)
        {
            string str = phrase.ToLower();
            // invalid chars, make into spaces
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces/hyphens into one space
            str = Regex.Replace(str, @"[\s-]+", " ").Trim();
            // cut and trim it
            str = str.Substring(0, str.Length <= maxLength ? str.Length : maxLength).Trim();
            // hyphens
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }

        Stream ResizeImage(Stream input, int cropWidth, int cropHeight)
        {
            // Declare variable for the conversion
            float ratio;

            // Create variable to hold the image
            Image thisImage = Image.FromStream(input);

            // Get height and width of current image
            int width = (int)thisImage.Width;
            int height = (int)thisImage.Height;

            // Ratio and conversion for new size
            if (width > cropWidth)
            {
                ratio = (float)width / (float)cropWidth;
                width = (int)(width / ratio);
                height = (int)(height / ratio);
            }

            // Ratio and conversion for new size
            if (height > cropHeight)
            {
                ratio = (float)height / (float)cropHeight;
                height = (int)(height / ratio);
                width = (int)(width / ratio);
            }



            // Create "blank" image for drawing new image
            Bitmap outImage = new Bitmap(width, height);
            Graphics outGraphics = Graphics.FromImage(outImage);
            //SolidBrush sb = new SolidBrush(System.Drawing.Color.White);

            outGraphics.CompositingQuality = CompositingQuality.HighQuality;
            outGraphics.SmoothingMode = SmoothingMode.HighQuality;
            outGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;


            // Fill "blank" with new sized image
            //outGraphics.FillRectangle(sb, 0, 0, outImage.Width, outImage.Height);
            outGraphics.DrawImage(thisImage, 0, 0, outImage.Width, outImage.Height);
            //sb.Dispose();

            thisImage.Dispose();

            ImageCodecInfo iciJpeg = GetEncoderInfo("image/jpeg");

            Encoder encQuality = Encoder.Quality;
            EncoderParameters encParams = new EncoderParameters(1);
            EncoderParameter paramQuality = new EncoderParameter(encQuality, 90L);
            encParams.Param[0] = paramQuality;

            Stream outputStream = new MemoryStream();
            // Save new image as jpg
            outImage.Save(outputStream, iciJpeg, encParams);

            outputStream.Seek(0, SeekOrigin.Begin);

            return outputStream;

        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}