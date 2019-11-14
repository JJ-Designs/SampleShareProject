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
        /// <summary>
        /// Returns the index view.
        /// </summary>
        /// <returns>
        /// Returns the index view.
        /// </returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get audio sample data from database and returns the details view.
        /// </summary>
        /// <param name="SampleID"> the chosen ID in Catalog.</param>
        /// <returns>
        /// Returns the Catalog details view.
        /// </returns>
        [HttpGet]
        [ActionName("CatalogSampleDetails")]
        public ActionResult CatalogSampleDetails(int SampleID)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            // selects a adiosaple based on its ID 
            AudioSamples audioSample = entities.AudioSamples.Single(a => a.SampleID == SampleID);
            return View(audioSample);
        }

        
        /// <summary>
        /// Get audio sample data from database and returns the details view.
        /// </summary>
        /// <param name="SampleID">  the chosen ID in Portfolio.</param>
        /// <returns>
        /// Returns the Portfolio details view.
        /// </returns>
        [HttpGet]
        [ActionName("PortfolioSampleDetails")]
        public ActionResult PortfolioSampleDetails(int SampleID)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            AudioSamples audioSample = entities.AudioSamples.Single(a => a.SampleID == SampleID);
            return View(audioSample);
        }

        /// <summary>
        /// gets all public audio samples and checks for category.
        /// then returns the catalog view.
        /// </summary>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("Catalog")]
        public ActionResult Catalog(int CategoryID)
        {
            //Instantiate Enitity framework database
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

        [HttpGet]
        [ActionName("MyPortfolio")]
        public ActionResult MyPortfolio(int UserIDFromURL, int CategoryID)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();

            if (Session["UserID"] != null)
            {
                Users user = entities.Users.Single(s => s.UserID == UserIDFromURL);
                List<AudioSamples> audioSamples;
                if (CategoryID != 0)
                    audioSamples = entities.AudioSamples
                    .Where(a => a.UserID == UserIDFromURL)
                    .Where(a => a.CategoryID == CategoryID).ToList();
                else
                    audioSamples = entities.AudioSamples.Where(a => a.UserID == UserIDFromURL).ToList();
                ViewBag.Categories = entities.Categories.ToList();
                return View(audioSamples);
            }
            else
                return RedirectToAction("Index", "Main");
        }

        [HttpGet]
        [ActionName("SortByCategoryPortfolio")]
        public ActionResult SortByCategoryPortfolio(int UserIDFromURL, int CategoryID)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();

            if (Session["UserID"] != null)
            {
                Users user = entities.Users.Single(s => s.UserID == UserIDFromURL);
                List<AudioSamples> audioSamples = entities.AudioSamples
                .Where(a => a.UserID == UserIDFromURL)
                .Where(a => a.CategoryID == CategoryID).ToList();

                return RedirectToAction("","", new { UserIDFromURL = Session["UserID"], audioSamplesFromURL = audioSamples });
            }
            else
                return RedirectToAction("Index", "Main");

        }

        [HttpGet]
        [ActionName("DownLoad")]
        public ActionResult Downlaod(int AudioSampleIDFromURL)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            List<AudioSamples> audioSamples = entities.AudioSamples.Where(a => a.isPublic == true).ToList();
            AudioSamples audioSample = entities.AudioSamples.SingleOrDefault(a => a.SampleID == AudioSampleIDFromURL);

            // Container Name - Sample  
            BlobController BlobManagerObj = new BlobController("samples");
            string FileAbsoluteUri = BlobManagerObj.DownloadFile(audioSample.FilePath);
            audioSample.Downloads++;
            entities.SaveChanges();

            return RedirectToAction("Catalog");
        }
        
        public ActionResult MyContent()
        {
            return View();
        }
        [HttpGet]
        [ActionName("EditMyProfile")]
        public ActionResult EditMyProfile(int UserIDFromURL)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            Users user = entities.Users.Single(u => u.UserID == UserIDFromURL);
            return View(user);
        }

        [HttpPost]
        [ActionName("EditMyProfile")]
        public ActionResult EditMyProfile(HttpPostedFileBase uploadFile, Users userprofile)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            foreach (string file in Request.Files)
            {
                uploadFile = Request.Files[file];
            }
            Users user = entities.Users.Single(u => u.UserName.Equals(userprofile.UserName));
            user.UserName = userprofile.UserName;
            user.FullName = userprofile.FullName;
            user.Email = userprofile.Email;
            user.Profession = userprofile.Profession;
            user.Discriptions = userprofile.Discriptions;
            user.Pass = Security.Encrypt(userprofile.Pass);
            user.ProfileImgPath = uploadFile.FileName;

            BlobController BlobManagerObj = new BlobController("pictures");
            string FileAbsoluteUri = BlobManagerObj.UploadFile(uploadFile, user.ProfileImgPath);

            entities.Entry(user).State = EntityState.Modified;
            entities.SaveChanges();
            return RedirectToAction("MyProfile", new { UserIDFromURL = Session["UserID"] });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserIDFromURL"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("MyProfile")]
        public ActionResult MyProfile(int UserIDFromURL)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            Users user = entities.Users.Single(s => s.UserID == UserIDFromURL);
            return View(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("login")]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("login")]
        [ValidateAntiForgeryToken]
        public ActionResult login(Users objUser)
        {
            if (ModelState.IsValid)
            {
                //Instantiate Enitity framework database
                using (SampleShareDBEntities entities = new SampleShareDBEntities())
                {
                    objUser.Pass = Security.Encrypt(objUser.Pass);
                    var obj = entities.Users.Where(a => a.UserName.Equals(objUser.UserName) && a.Pass.Equals(objUser.Pass)).FirstOrDefault();
                    if (obj != null)
                    {
                        Session["UserID"] = obj.UserID.ToString();
                        Session["UserName"] = obj.UserName.ToString();
                        Session["FullName"] = obj.FullName.ToString();
                        return RedirectToAction("MyProfile", new { UserIDFromURL = Session["UserID"] });
                    }
                    else
                    {
                        ViewBag.Message = "USERNAME OR PASSWORD IS WRONG!";
                    }
                }
            }
            return View(objUser);
        }
        
        //Sign up controller. Get (When you firt open the page)
        [HttpGet]
        [ActionName("SignUp")]
        public ActionResult SignUp()
        {
            return View();
        }

        /// <summary>
        /// Sign up controller. Post (When you submit information from the page)
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SignUp")]
        public ActionResult SignUp(Users user)
        {
            if (user.FullName != "" && user.FullName != null && user.Email != "" && user.Email != null && user.Pass != "" && user.Pass != null && user.UserName != "" && user.UserName != null)
            {
                if (user.Pass.Length >= 4)
                {
                    //Instantiate Enitity framework database
                    SampleShareDBEntities entities = new SampleShareDBEntities();
                    if (!entities.Users.Any(x => x.UserName == user.UserName))
                    {
                        if (!entities.Users.Any(x => x.Email == user.Email))
                        {
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
                        ViewBag.Message = "USERNAME ALLREADY EXSIST";
                }
                else
                    ViewBag.Message = "PASSWORD MUST BE LONGER THEN 4 CHARACTERS";
            }
            else
                ViewBag.Message = "YOU MUST ENTER SOMETHING IN ALL FEILDS!";
            return View(user);
        }
        /// <summary>
        /// The logout controller 
        /// </summary>
        /// <returns></returns>
        public ActionResult logout()
        {
            Session.Abandon();
            return RedirectToAction("index");
        }

        // Upload Controller
        [HttpGet]
        [ActionName("UploadSample")]
        public ActionResult UploadSample()
        {
            if(Session["UserID"] != null)
            {
                //Instantiate Enitity framework database
                SampleShareDBEntities entities = new SampleShareDBEntities();
                ViewBag.Categories = entities.Categories.ToList();
                return View();
            }
            else
                return RedirectToAction("Index", "Main");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="audioSample"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadSample(HttpPostedFileBase uploadFile, AudioSamples audioSample)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            foreach (string file in Request.Files)
            {
                uploadFile = Request.Files[file];
            }
            ViewBag.Categories = entities.Categories.ToList();
            if (uploadFile.ContentLength < 102400000)
            {
                string fileExt = uploadFile.FileName.Substring(uploadFile.FileName.IndexOf("."));
                if (fileExt == ".wav" || fileExt == ".mp3" || fileExt == ".flac")
                {
                    audioSample.FilePath = audioSample.SampleTitel + fileExt;
                    audioSample.CreationDate = DateTime.Now;
                    audioSample.Downloads = 0;
                    audioSample.Categories = entities.Categories.Single(c => c.CategoryID == audioSample.CategoryID);
                    string id = (string)Session["UserID"];
                    audioSample.UserID = Int32.Parse(id);
                    audioSample.Users = entities.Users.Single(u => u.UserID == audioSample.UserID);

                    entities.AudioSamples.Add(audioSample);

                    // Container Name - Sample  
                    BlobController BlobManagerObj = new BlobController("samples"); //constrktor ses i Billede eksempel 2
                    string FileAbsoluteUri = BlobManagerObj.UploadFile(uploadFile, audioSample.FilePath); //metode ses i Billede eksempel 3
                    entities.SaveChanges();

                    return RedirectToAction("index");
                }
                else
                    ViewBag.Message = "NOT A VALID FILE TYPE!";
            }
            else
                ViewBag.Message = "FILE IS TOO BIG!";

            return View();
        }

        [HttpGet]
        [ActionName("DeleteUserAndFiles")]
        public ActionResult DeleteUserAndFiles(int UserIDFromURL)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            Users user = entities.Users.Single(a => a.UserID == UserIDFromURL);
            List<AudioSamples> audioSamplesToDel = entities.AudioSamples.Where(a => a.UserID == user.UserID).ToList();
            foreach (AudioSamples audioSample in audioSamplesToDel)
                entities.AudioSamples.Remove(audioSample);
            entities.Users.Remove(user);
            entities.SaveChanges();
            Session.Abandon();
            return RedirectToAction("index");
        }

        [HttpGet]
        [ActionName("DeleteAudioSample")]
        public ActionResult DeleteAudioSample(int SampleIDFromURL)
        {
            //Instantiate Enitity framework database
            SampleShareDBEntities entities = new SampleShareDBEntities();
            AudioSamples audioSampleToDel = entities.AudioSamples.SingleOrDefault(a => a.SampleID == SampleIDFromURL);
            entities.AudioSamples.Remove(audioSampleToDel);
            entities.SaveChanges();

            return RedirectToAction("MyPortefolio");
        }
    }
}