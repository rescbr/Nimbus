using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Nimbus.Web.Utils
{
    public class AzureBlob
    {
        CloudStorageAccount _storageAccount = CloudStorageAccount.Parse(NimbusConfig.StorageAccount);
        CloudBlobClient _blobClient;
        CloudBlobContainer _container;

        public CloudBlobContainer Container
        {
            get { return _container; }
        }
        CloudBlockBlob _blockBlob;

        public CloudBlockBlob BlockBlob
        {
            get { return _blockBlob; }
        }
        
        public AzureBlob(string container, string filename)
        {
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _container = _blobClient.GetContainerReference(container);
            _blockBlob = _container.GetBlockBlobReference(filename);
        }

        public void UploadStreamToAzure(Stream stream)
        {
            _blockBlob.UploadFromStream(stream);
            _blockBlob.Properties.CacheControl = "max-age=75, must-revalidate";
            _blockBlob.SetProperties();
        }

        public Stream DownloadToMemoryStream()
        {
            Stream outStream = new MemoryStream();
            _blockBlob.DownloadToStream(outStream);
            outStream.Seek(0, SeekOrigin.Begin);
            return outStream;
        }

        public void Delete()
        {
            _blockBlob.DeleteIfExists();
        }

        /// <summary>
        /// Will transform "some $ugly ###url wit[]h spaces" into "some-ugly-url-with-spaces"
        /// </summary>
        public static string RenameToValidName(string phrase, int maxLength = 50)
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
    }
}