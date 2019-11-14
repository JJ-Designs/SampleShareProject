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

        /// <summary>
        /// Uploads a file to a blob.
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
        /// downloads a File from blob based on the URI.
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

        /// <summary>
        /// Deletes a file from the blob.
        /// </summary>
        /// <param name="AbsoluteUri"></param>
        /// <returns>Returns true if successfull</returns>
        public bool DeleteBlob(string AbsoluteUri)
        {
            try
            {
                //Uri uriObj = new Uri(AbsoluteUri);
                //string BlobName = Path.GetFileName(uriObj.LocalPath);
                string BlobName = Path.GetFileName(AbsoluteUri);

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