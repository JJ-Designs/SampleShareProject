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

        #region Blob constructor 
        /// <summary>
        /// BlobController constructor. New instance for each container.
        /// </summary>
        /// <param name="ContainerName"> The name you give the container</param>
        public BlobController(string ContainerName)
        {
            // Checks if Container name is null or empty  
            if (string.IsNullOrEmpty(ContainerName))
            {
                throw new ArgumentNullException("ContainerName", "Container Name can't be empty");
            }
            try
            {
                //Gets the connetion string from Web.config file
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureStorageConnectionString-1"));

                //Azure library for making containers
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = cloudBlobClient.GetContainerReference(ContainerName);

                // Create the container and set the permission if it doesn't already exist
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
        #endregion


        #region Blob Fields
        private CloudBlobContainer blobContainer;
        #endregion


        #region Upload File Blob
        /// <summary>
        /// Uploads a file to the definded container
        /// </summary>
        /// <param name="FileToUpload">The file to upload</param>
        /// <param name="FileName">The name it should be uploaded as</param>
        /// <returns>Returns the absolute URI as an string</returns>
        public string UploadFile(Web.HttpPostedFileBase FileToUpload, string FileName)
        {
            string absoluteUri;
            // Check HttpPostedFileBase is null  
            if (FileToUpload == null || FileToUpload.ContentLength == 0)
                return null;
            try
            {
                CloudBlockBlob blockBlob;
                // Create a block blob  
                blockBlob = blobContainer.GetBlockBlobReference(FileName);
                // Set the object's content type  
                blockBlob.Properties.ContentType = FileToUpload.ContentType;
                // Upload to blob  
                blockBlob.UploadFromStream(FileToUpload.InputStream);

                // Get's file URI
                absoluteUri = blockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
            //Returns URI
            return absoluteUri;
        }
        #endregion


        #region Download File Blob
        /// <summary>
        /// Download a file from URI
        /// </summary>
        /// <param name="SampleFileName"></param>
        /// <returns></returns>
        public string DownloadFile(string SampleFileName)
        {
            string AbsoluteUri;
            //Gets the blob in the defined container
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(SampleFileName);

            MemoryStream memStream = new MemoryStream();

            //Opens a stream to downloade file from URI
            blockBlob.DownloadToStream(memStream);
            AbsoluteUri = blockBlob.Uri.AbsoluteUri;

            //Sets the type of file in the stream
            Web.HttpContext.Current.Response.ContentType = blockBlob.Properties.ContentType.ToString();

            //Adds headers to set the file name and size.
            Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; filename=" + blockBlob.Name);
            Web.HttpContext.Current.Response.AddHeader("Content-Length", blockBlob.Properties.Length.ToString());

            //Puts the download to the browser
            Web.HttpContext.Current.Response.BinaryWrite(memStream.ToArray());
            Web.HttpContext.Current.Response.Flush();
            Web.HttpContext.Current.Response.Close();
            return AbsoluteUri;
        }
        #endregion


    }
}