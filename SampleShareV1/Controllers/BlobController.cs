using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SampleShareV1.Controllers
{
    public class BlobController : Controller
    {

        private CloudBlobContainer blobContainer;


        public BlobController(string ContainerName)
        {
            // Check if Container Name is null or empty  
            if (string.IsNullOrEmpty(ContainerName))
            {
                throw new ArgumentNullException("ContainerName", "Container Name can't be empty");
            }
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureStorageConnectionString-1"));

                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = cloudBlobClient.GetContainerReference(ContainerName);

                // Create the container and set the permission  
                if (blobContainer.CreateIfNotExists())
                {
                    blobContainer.SetPermissions(
                        new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Blob
                        }
                    );
                }
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }


        // GET: File
        public ActionResult Index()
        {
            return View();
        }
        private CloudBlobContainer GetCloudBlobContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureStorageConnectionString-1"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("test-blob-container");
            return container;
        }

        public ActionResult CreateBlobContainer()
        {
            CloudBlobContainer container = GetCloudBlobContainer();
            ViewBag.Success = container.CreateIfNotExists();
            ViewBag.BlobContainerName = container.Name;

            return View();
        }

        public string UploadFile(HttpPostedFileBase FileToUpload)
        {
            string AbsoluteUri;
            // Check HttpPostedFileBase is null or not  
            if (FileToUpload == null || FileToUpload.ContentLength == 0)
                return null;
            try
            {
                string FileName = Path.GetFileName(FileToUpload.FileName);
                CloudBlockBlob blockBlob;
                // Create a block blob  
                blockBlob = blobContainer.GetBlockBlobReference(FileName);
                // Set the object's content type  
                blockBlob.Properties.ContentType = FileToUpload.ContentType;
                // upload to blob  
                blockBlob.UploadFromStream(FileToUpload.InputStream);

                // get file uri  
                AbsoluteUri = blockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
            return AbsoluteUri;
        }

        //downloadFile
        public string DownloadFile(string SampleFileName)
        {
            string AbsoluteUri;
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(SampleFileName);

            MemoryStream memStream = new MemoryStream();

            blockBlob.DownloadToStream(memStream);
            AbsoluteUri = blockBlob.Uri.AbsoluteUri;

            HttpContext.Response.ContentType = blockBlob.Properties.ContentType.ToString();
            HttpContext.Response.AddHeader("Content-Disposition", "Attachment; filename=" + blockBlob.ToString());

            HttpContext.Response.AddHeader("Content-Length", blockBlob.Properties.Length.ToString());
            HttpContext.Response.BinaryWrite(memStream.ToArray());
            HttpContext.Response.Flush();
            HttpContext.Response.Close();
            return AbsoluteUri;
        }

        public List<string> BlobList()
        {
            List<string> _blobList = new List<string>();
            foreach (IListBlobItem item in blobContainer.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob _blobpage = (CloudBlockBlob)item;
                    _blobList.Add(_blobpage.Uri.AbsoluteUri.ToString());
                }
            }
            return _blobList;
        }

        public bool DeleteBlob(string AbsoluteUri)
        {
            try
            {
                Uri uriObj = new Uri(AbsoluteUri);
                string BlobName = Path.GetFileName(uriObj.LocalPath);

                // get block blob refarence  
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(BlobName);

                // delete blob from container      
                blockBlob.Delete();
                return true;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }
    }
}