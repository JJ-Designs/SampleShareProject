using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Web = System.Web;

namespace SampleShareV1.Controllers
{
    public class BlobController : Controller
    {
        //Eksempel 2
        private CloudBlobContainer blobContainer;

        /// <summary>
        /// BlobController constroctor. new instance for each container.
        /// </summary>
        /// <param name="ContainerName"> The name you give the container</param>
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

        /// <summary>
        /// Create new blob container
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileToUpload"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public string UploadFile(Web.HttpPostedFileBase FileToUpload, string FileName)
        {
            string absoluteUri;
            // Check HttpPostedFileBase is null or not  
            if (FileToUpload == null || FileToUpload.ContentLength == 0)
                return null;
            try
            {
                CloudBlockBlob blockBlob;
                // Create a block blob  
                blockBlob = blobContainer.GetBlockBlobReference(FileName);
                // Set the object's content type  
                blockBlob.Properties.ContentType = FileToUpload.ContentType;
                // upload to blob  
                blockBlob.UploadFromStream(FileToUpload.InputStream);

                // get file uri  
                absoluteUri = blockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
            return absoluteUri;
        }
        
        /// <summary>
        /// download a File from URI
        /// </summary>
        /// <param name="SampleFileName"></param>
        /// <returns></returns>
        public string DownloadFile(string SampleFileName)
        {
            string AbsoluteUri;
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(SampleFileName);

            MemoryStream memStream = new MemoryStream();

            // opens a stream to downloade file from URI
            blockBlob.DownloadToStream(memStream);
            AbsoluteUri = blockBlob.Uri.AbsoluteUri;

            //sets the type of file in the stream
            Web.HttpContext.Current.Response.ContentType = blockBlob.Properties.ContentType.ToString();

            //adds headers to set the file name and size.
            Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; filename=" + blockBlob.Name);
            Web.HttpContext.Current.Response.AddHeader("Content-Length", blockBlob.Properties.Length.ToString());

            //puts the download to the browser
            Web.HttpContext.Current.Response.BinaryWrite(memStream.ToArray());
            Web.HttpContext.Current.Response.Flush();
            Web.HttpContext.Current.Response.Close();
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