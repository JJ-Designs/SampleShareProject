using SampleShareV1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleShareV1.Controllers
{
    public class MainController : Controller
    {
        #region Index
        /// <summary>
        /// Routes to the index view.
        /// </summary>
        /// <returns>
        /// Returns the index view.
        /// </returns>
        public ActionResult Index()
        {
            //Instantiate entity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();

            //Takes the 5 most downloaded public samples
            List<AudioSamples> mostDownloaded = entities.AudioSamples
                .Where(a => a.isPublic == true)
                .OrderByDescending(a => a.Downloads)
                .Take(5).ToList();

            //Takes the last 5 public uploaded samples
            List<AudioSamples> lastUploaded = entities.AudioSamples
                .Where(a => a.isPublic == true )
                .OrderByDescending(a => a.CreationDate)
                .Take(5).ToList();

            //Get's the most downloaded for each category
            List<AudioSamples> bestOfCategory = entities.AudioSamples
                .Where(a => a.isPublic == true)
                .Where(a => a.CategoryID == 1)
                .OrderByDescending(a => a.Downloads)
                .Take(1).ToList();

            bestOfCategory.Add(entities.AudioSamples
                .Where(a => a.isPublic == true)
                .Where(a => a.CategoryID == 2)
                .OrderByDescending(a => a.Downloads)
                .Take(1).Single());

            bestOfCategory.Add(entities.AudioSamples
               .Where(a => a.isPublic == true)
               .Where(a => a.CategoryID == 3)
               .OrderByDescending(a => a.Downloads)
               .Take(1).Single());

            bestOfCategory.Add(entities.AudioSamples
               .Where(a => a.isPublic == true)
               .Where(a => a.CategoryID == 4)
               .OrderByDescending(a => a.Downloads)
               .Take(1).Single());

            bestOfCategory.Add(entities.AudioSamples
               .Where(a => a.isPublic == true)
               .Where(a => a.CategoryID == 5)
               .OrderByDescending(a => a.Downloads)
               .Take(1).Single());

            //put's the data in viewbags for use in view
            ViewBag.MostDownloaded = mostDownloaded;
            ViewBag.Recent = lastUploaded;
            ViewBag.BestOfCategory = bestOfCategory;
            return View();
        }
        #endregion


        #region Sample Details
        /// <summary>
        /// Get audio sample data from database and returns the details view.
        /// </summary>
        /// <param name="SampleID"> the chosen ID in Catalog.</param>
        /// <returns>
        /// Returns the Catalog details view.
        /// </returns>
        [HttpGet]
        [ActionName("SampleDetails")]
        public ActionResult SampleDetails(int SampleID)
        {
            //Instantiate entity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            //Selects a audio sample based on its id
            AudioSamples audioSample = entities.AudioSamples.Single(a => a.SampleID == SampleID);
            return View(audioSample);
        }
        #endregion


        #region Catalog
        /// <summary>
        /// Gets all public audio samples and checks for category.
        /// then returns the catalog view.
        /// </summary>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("Catalog")]
        public ActionResult Catalog(int CategoryID)
        {
            //Instantiate entity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            List<AudioSamples> audioSamples;
            //If no category is chosen show all public audios amples
            if (CategoryID != 0)
                audioSamples = entities.AudioSamples
                    .Where(a => a.isPublic == true)
                    .Where(a => a.CategoryID == CategoryID).ToList();
            else
                audioSamples = entities.AudioSamples.Where(a => a.isPublic == true).ToList();
            ViewBag.Categories = entities.Categories.ToList();
            return View(audioSamples);
        }
        #endregion


        #region Portfolio
        /// <summary>
        /// Routes to the profile view if a user is logged in. Else route to index
        /// </summary>
        /// <param name="UserIDFromURL"></param>
        /// <param name="CategoryID">Category to filter by. No filter if id is 0</param>
        /// <returns>Returns the profile view if a user is logged in. Else route to index.</returns>
        [HttpGet]
        [ActionName("MyPortfolio")]
        public ActionResult MyPortfolio(int UserIDFromURL, int CategoryID)
        {
            //Instantiate entity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();

            //checks if user is logged in
            if (Session["UserID"] != null)
            {
                //Gets the user from URL ID
                Users user = entities.Users.Single(s => s.UserID == UserIDFromURL);
                List<AudioSamples> audioSamples;
                if (CategoryID != 0) // Gets all audio samples for the user if category id is 0
                    audioSamples = entities.AudioSamples
                    .Where(a => a.UserID == UserIDFromURL)
                    .Where(a => a.CategoryID == CategoryID).ToList();
                else //Only gets the audio samples where the category id equals the selected category
                    audioSamples = entities.AudioSamples.Where(a => a.UserID == UserIDFromURL).ToList();
                ViewBag.Categories = entities.Categories.ToList();
                return View(audioSamples);
            }
            else //Redirect to index, if there is no user in session
                return RedirectToAction("Index", "Main");
        }
        #endregion


        #region Download
        /// <summary>
        /// Downloads the audiosample chosen from its id.
        /// Adds the download count to the database.
        /// </summary>
        /// <param name="AudioSampleIDFromURL">The id of the audio sample to download.</param>
        /// <returns>If there are no user in session, rediret to Index. Else after download, go to catalog. </returns>
        [HttpGet]
        [ActionName("DownLoad")]
        public ActionResult Downlaod(int AudioSampleIDFromURL)
        {
            if (Session["UserID"] != null)
            {
                //Instantiate entity framework database
                SampleShareDBEntities entities = new SampleShareDBEntities();
                //Get's the audio sample to download from the database
                AudioSamples audioSample = entities.AudioSamples.SingleOrDefault(a => a.SampleID == AudioSampleIDFromURL);

                // Finds the Container with the name samples 
                BlobController BlobManagerObj = new BlobController("samples");
                //Downloads the file in the container
                string FileAbsoluteUri = BlobManagerObj.DownloadFile(audioSample.FilePath);
                //Adds the download count to the database
                audioSample.Downloads++;
                entities.SaveChanges();

                return RedirectToAction("Catalog");
            }
            else //Redirect to login, if there is no user in session
                return RedirectToAction("Login");
        }
        #endregion


        #region Edit Profile
        /// <summary>
        /// Get's information of the user user from the database, and returns the Edit view. 
        /// </summary>
        /// <param name="UserIDFromURL">The id of the user to show in the edit view</param>
        /// <returns>If there are no user in session, rediret to Index. else returns the Edit view.</returns>
        [HttpGet]
        [ActionName("EditMyProfile")]
        public ActionResult EditMyProfile(int UserIDFromURL)
        {
            if (Session["UserID"] != null)
            {
                //Instantiate entity framework database
                SampleShareDBEntities entities = new SampleShareDBEntities();
                //Gets the logged in user from url
                Users user = entities.Users.Single(u => u.UserID == UserIDFromURL);

                //Takes the last 3 uploaded samples for the user
                List<AudioSamples> lastAudioSamples = entities.AudioSamples
                    .Where(a => a.UserID == UserIDFromURL)
                    .OrderByDescending(a => a.CreationDate)
                    .Take(3).ToList();

                //Takes the 5 most downloaded samples for the user
                List<AudioSamples> mostDownloadedAudioSamples = entities.AudioSamples
                    .Where(a => a.UserID == UserIDFromURL)
                    .OrderByDescending(a => a.Downloads)
                    .Take(5).ToList();

                //Saves list in viewbags for use in view
                ViewBag.LatestAudioSamples = lastAudioSamples;
                ViewBag.MostDownloadedAudioSamples = mostDownloadedAudioSamples;
                //Go to view
                return View(user);
            }
            else //Redirect to login, if there is no user in session
                return RedirectToAction("Login");
        }

        /// <summary>
        /// Updates the user in the database based on input in edit view.
        /// </summary>
        /// <param name="uploadFile">File to upload for profile picture</param>
        /// <param name="userprofile">User information from view</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("EditMyProfile")]
        public ActionResult EditMyProfile(HttpPostedFileBase uploadFile, Users userprofile)
        {
            //Checks if user is logged in
            if (Session["UserID"] != null)
            {
                //Instantiate entity framework database
                SampleShareDBEntities entities = new SampleShareDBEntities();
                foreach (string file in Request.Files)
                {
                    uploadFile = Request.Files[file];
                }
                //Gets the user from database
                Users user = entities.Users.Single(u => u.UserID.Equals(userprofile.UserID));
                //Updates the user from url info
                user.UserName = userprofile.UserName;
                user.FullName = userprofile.FullName;
                user.Email = userprofile.Email;
                user.Profession = userprofile.Profession;
                user.Discriptions = userprofile.Discriptions;
                //Encrupts the password if it was changed
                user.Pass = userprofile.Pass != null ? Security.Encrypt(userprofile.Pass) : user.Pass;
                user.ProfileImgPath = uploadFile.FileName != "" ? uploadFile.FileName : user.ProfileImgPath;

                //Saves profile picture in blob container
                BlobController BlobManagerObj = new BlobController("pictures");
                string FileAbsoluteUri = BlobManagerObj.UploadFile(uploadFile, user.ProfileImgPath);

                //Saves changes in database
                entities.Entry(user).State = EntityState.Modified;
                entities.SaveChanges();
                return RedirectToAction("MyProfile", new { UserIDFromURL = userprofile.UserID });
            }
            else // Redirect to login, if there is no user in session
                return RedirectToAction("Login");
        }
        #endregion


        #region Profile
        /// <summary>
        /// Get's information of the user user from the database, and returns the profile view.   
        /// </summary>
        /// <param name="UserIDFromURL">The id of the user to show in the profile view</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("MyProfile")]
        public ActionResult MyProfile(int UserIDFromURL)
        {
            //Instantiate entity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            //Gets the user from URL ID
            Users user = entities.Users.Single(u => u.UserID == UserIDFromURL);
            
            //Gets the latest 3 samples for the user 
            List<AudioSamples> lastAudioSamples = entities.AudioSamples
                .Where(a => a.UserID == UserIDFromURL)
                .OrderByDescending(a => a.CreationDate)
                .Take(3).ToList();

            //Gets the 5 most downloaded samples for the user 
            List<AudioSamples> mostDownloadedAudioSamples = entities.AudioSamples
                .Where(a => a.UserID == UserIDFromURL)
                .OrderByDescending(a => a.Downloads)
                .Take(5).ToList();

            //Gets the total count of audio samples for the user
            int totalAudioSamples = entities.AudioSamples.Where(a => a.UserID == UserIDFromURL).Count();
            
            //Gets the total count of downloads of all audio samples for the user
            int? totalDownloads = entities.AudioSamples
                .Where(a => a.UserID == UserIDFromURL)
                .Select(a => a.Downloads).Sum();

            //Gets the total count of public audio samples for the user
            int totalPublicAudiosamples = entities.AudioSamples
                .Where(a => a.UserID == UserIDFromURL)
                .Where(a => a.isPublic == true).Count();

            //Gets the total count of private audio samples for the user
            int totalPrivateAudiosamples = entities.AudioSamples
                .Where(a => a.UserID == UserIDFromURL)
                .Where(a => a.isPublic == false).Count();


            //Put's the data in viewbags for use in view
            ViewBag.LatestAudioSamples = lastAudioSamples;
            ViewBag.MostDownloadedAudioSamples = mostDownloadedAudioSamples;
            ViewBag.TotalAudioSamples = totalAudioSamples;
            ViewBag.TotalDownloads = totalDownloads != null ? totalDownloads : 0;
            ViewBag.TotalPublicAudiosamples = totalPublicAudiosamples;
            ViewBag.TotalPrivateAudiosamples = totalPrivateAudiosamples;
            return View(user);
        }
        #endregion


        #region Login
        /// <summary>
        /// Routes to login view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("login")]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// If username and password matches, then it logs the user it and opens a session.
        /// </summary>
        /// <param name="objUser">Get's user information from view.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("login")]
        [ValidateAntiForgeryToken]
        public ActionResult login(Users objUser)
        {
            //Checks for correct login information
            if (ModelState.IsValid)
            {
                if(objUser.Pass != null && objUser.UserName != null)
                {
                    //Instantiate entity framework database
                    using (SampleShareDBEntities entities = new SampleShareDBEntities())
                    {
                        objUser.Pass = Security.Encrypt(objUser.Pass);
                        //Get's the user from the database
                        var obj = entities.Users.Where(a => a.UserName.Equals(objUser.UserName) && a.Pass.Equals(objUser.Pass)).FirstOrDefault();
                        if (obj != null)
                        {
                            //Set's session information on the user
                            Session["UserID"] = obj.UserID.ToString();
                            Session["UserName"] = obj.UserName.ToString();
                            Session["FullName"] = obj.FullName.ToString();
                            Session["UserRightID"] = obj.userrightid.ToString();
                            return RedirectToAction("MyProfile", new { UserIDFromURL = Session["UserID"] });
                        }
                        else
                        {
                            ViewBag.Message = "USERNAME OR PASSWORD IS WRONG!";
                        }
                    }
                }
                else
                    ViewBag.Message = "YOU MUST ENTER SOMETHING IN ALL FEILDS!";
            }
            return View(objUser);
        }
        #endregion


        #region Sign Up
        /// <summary>
        /// Routes to the Sign up view
        /// </summary>
        /// <returns>Returns the Sign up view</returns>
        [HttpGet]
        [ActionName("SignUp")]
        public ActionResult SignUp()
        {
            return View();
        }

        /// <summary>
        /// Checks for valid data, and then enters a new user to the database.
        /// </summary>
        /// <param name="user">Get's User information from the view.</param>
        /// <returns> If succesfull sign up, redirect to login. Else returns the Sign up viw with error message. </returns>
        [HttpPost]
        [ActionName("SignUp")]
        public ActionResult SignUp(Users user)
        {
            //Checks for no empty textboxes
            if (user.FullName != "" && user.FullName != null && user.Email != "" && user.Email != null && user.Pass != "" && user.Pass != null && user.UserName != "" && user.UserName != null)
            {
                if (user.Pass.Length >= 4)
                {
                    //Instantiate entity framework database
                    SampleShareDBEntities entities = new SampleShareDBEntities();
                    //Checks if user already exists
                    if (!entities.Users.Any(x => x.UserName == user.UserName))
                    {
                        if (!entities.Users.Any(x => x.Email == user.Email))
                        {
                            //Enters user information to the database
                            user.Pass = Security.Encrypt(user.Pass);
                            user.userrightid = 2;
                            user.ProfileImgPath = "default-profile-picture.png";
                            entities.Users.Add(user);
                            entities.SaveChanges();
                            return RedirectToAction("login");
                        }
                        else
                            ViewBag.Message = "ACCOUNT ALREADY LINKED TO E-MAIL";
                    }
                    else
                        ViewBag.Message = "USERNAME ALREADY EXSIST";
                }
                else
                    ViewBag.Message = "PASSWORD MUST BE LONGER THEN 4 CHARACTERS";
            }
            else
                ViewBag.Message = "YOU MUST ENTER SOMETHING IN ALL FEILDS!";
            return View(user);
        }
        #endregion


        #region Logout
        /// <summary>
        /// The logout controller, closes the session.
        /// </summary>
        /// <returns>Redirects to index</returns>
        public ActionResult logout()
        {
            Session.Abandon();
            return RedirectToAction("index");
        }
        #endregion


        #region Upload Sample
        /// <summary>
        /// Routes to the upload view if a user is logged in. Else route to index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("UploadSample")]
        public ActionResult UploadSample()
        {
            if(Session["UserID"] != null)
            {
                //Instantiate entity framework database
                SampleShareDBEntities entities = new SampleShareDBEntities();
                ViewBag.Categories = entities.Categories.ToList();
                return View();
            }
            else //Redirect to index, if there is no user in session
                return RedirectToAction("Index", "Main");
        }

        /// <summary>
        /// Uploads the file to the storage, and enters the information to the database.
        /// </summary>
        /// <param name="uploadFile">File to upload</param>
        /// <param name="audioSample">Information on the audio sample to uploade to the database</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadSample(HttpPostedFileBase uploadFile, AudioSamples audioSample)
        {
            //Instantiate entity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            foreach (string file in Request.Files)
            {
                //Gets the file uploaded from the browser
                uploadFile = Request.Files[file];
            }
            ViewBag.Categories = entities.Categories.ToList();
            //Checks for empty inputs
            if (uploadFile.FileName != null && !uploadFile.FileName.Equals(""))
            {
                if (audioSample.SampleTitel != null && !audioSample.SampleTitel.Equals(""))
                {
                    if (audioSample.CategoryID != null)
                    {
                        if (uploadFile.ContentLength < 102400000)
                        {
                            //Only wav, mp3 or flac file type is accepted
                            string fileExt = uploadFile.FileName.Substring(uploadFile.FileName.IndexOf("."));
                            if (fileExt == ".wav" || fileExt == ".mp3" || fileExt == ".flac")
                            {
                                //Enters the new audio sample to the database
                                audioSample.FilePath = audioSample.SampleTitel + fileExt;
                                audioSample.CreationDate = DateTime.Now;
                                audioSample.Downloads = 0;
                                audioSample.Categories = entities.Categories.Single(c => c.CategoryID == audioSample.CategoryID);
                                string id = (string)Session["UserID"];
                                audioSample.UserID = Int32.Parse(id);
                                audioSample.Users = entities.Users.Single(u => u.UserID == audioSample.UserID);

                                entities.AudioSamples.Add(audioSample);

                                //Finds the Container with the name samples
                                BlobController BlobManagerObj = new BlobController("samples");
                                string FileAbsoluteUri = BlobManagerObj.UploadFile(uploadFile, audioSample.FilePath);
                                entities.SaveChanges();

                                return RedirectToAction("index");
                            }
                            else
                                ViewBag.Message = "* NOT A VALID FILE TYPE!";
                        }
                        else
                            ViewBag.Message = "* FILE IS TOO BIG!";
                    }
                    else
                        ViewBag.Message = "* YOU MUST CHOOSE A CATEGORY";
                }
                else
                    ViewBag.Message = "* YOU MUST ENTER A TITLE";
            }
            else
                ViewBag.Message = "* YOU MUST CHOOSE A FILE";

            return View();
        }
        #endregion


        #region Delete User & Audio-Samples
        /// <summary>
        /// Deletes the user from the database with all of it's audio samples. 
        /// </summary>
        /// <param name="UserIDFromURL">The ID of the user to delete.</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("DeleteUserAndFiles")]
        public ActionResult DeleteUserAndFiles(int UserIDFromURL)
        {
            //Instantiate entity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            //Findes user from URL
            Users user = entities.Users.Single(a => a.UserID == UserIDFromURL);
            //Get's all audio samples made by that user
            List<AudioSamples> audioSamplesToDel = entities.AudioSamples.Where(a => a.UserID == user.UserID).ToList();
            //Deletes all the audio samples in the list
            foreach (AudioSamples audioSample in audioSamplesToDel)
                entities.AudioSamples.Remove(audioSample);
            //Deletes the user
            entities.Users.Remove(user);
            entities.SaveChanges();
            Session.Abandon();
            return RedirectToAction("index");
        }
        #endregion


        #region Delete Audio-Sample
        /// <summary>
        /// Deletes the audio samples from the database.
        /// </summary>
        /// <param name="SampleIDFromURL">The id of the audio sample to delete</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("DeleteAudioSample")]
        public ActionResult DeleteAudioSample(int SampleIDFromURL)
        {
            //Instantiate entity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            //Get's the audio sample to delete
            AudioSamples audioSampleToDel = entities.AudioSamples.SingleOrDefault(a => a.SampleID == SampleIDFromURL);
            //Deletes the audio sample
            entities.AudioSamples.Remove(audioSampleToDel);
            entities.SaveChanges();

            return RedirectToAction("Index");
        }
        #endregion
    }
}