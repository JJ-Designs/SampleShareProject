using SampleShareV1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace SampleShareV1.Controllers
{
    public class MainController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        [ActionName("CatalogSampleDetails")]
        public ActionResult CatalogSampleDetails(int SampleID)
        {
            SampleShareDBEntities entities = new SampleShareDBEntities();
            AudioSamples audioSample = entities.AudioSamples.Single(a => a.SampleID == SampleID);
            return View(audioSample);
        }

        [HttpGet]
        [ActionName("PortfolioSampleDetails")]
        public ActionResult PortfolioSampleDetails(int SampleID)
        {
            SampleShareDBEntities entities = new SampleShareDBEntities();
            AudioSamples audioSample = entities.AudioSamples.Single(a => a.SampleID == SampleID);
            return View(audioSample);
        }

        [HttpGet]
        [ActionName("Catalog")]
        public ActionResult Catalog(int CategoryID)
        {
            SampleShareDBEntities entities = new SampleShareDBEntities();
            List<AudioSamples> audioSamples;
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
            SampleShareDBEntities entities = new SampleShareDBEntities();
            Users user = entities.Users.Single(u => u.UserID == UserIDFromURL);
            if (Session["UserID"] != null)
            {
                return View(user);
            }
            else
                return RedirectToAction("Index", "Main");
        }

        [HttpPost]
        [ActionName("EditMyProfile")]
        public ActionResult EditMyProfile(Users userprofile)
        {
            SampleShareDBEntities entities = new SampleShareDBEntities();
            Users user = entities.Users.Single(u => u.UserName.Equals(userprofile.UserName));

            user.UserName = userprofile.UserName;
            user.FullName = userprofile.FullName;
            user.Email = userprofile.Email;
            user.Profession = userprofile.Profession;
            user.Discriptions = userprofile.Discriptions;
            user.Pass = userprofile.Pass;

            entities.Entry(user).State = EntityState.Modified;
            entities.SaveChanges();
            return RedirectToAction("MyProfile", new { UserIDFromURL = Session["UserID"] });
        }




        [HttpGet]
        [ActionName("MyProfile")]
        public ActionResult MyProfile(int UserIDFromURL)
        {
            SampleShareDBEntities entities = new SampleShareDBEntities();
            Users user = entities.Users.Single(s => s.UserID == UserIDFromURL);
            return View(user);
        }
		
        [HttpGet]
        [ActionName("login")]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ActionName("login")]
        [ValidateAntiForgeryToken]
        public ActionResult login(Users objUser)
        {
            if (ModelState.IsValid)
            {
                using (SampleShareDBEntities entities = new SampleShareDBEntities())
                {
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

        //Sign up controller. Post (When you submit information from the page)
        [HttpPost]
        [ActionName("SignUp")]
        public ActionResult SignUp(Users user)
        {
            if (user.FullName != "" && user.FullName != null && user.Email != "" && user.Email != null && user.Pass != "" && user.Pass != null && user.UserName != "" && user.UserName != null)
            {
                if (user.Pass.Length >= 4)
                {
                    SampleShareDBEntities entities = new SampleShareDBEntities();
                    if (!entities.Users.Any(x => x.UserName == user.UserName))
                    {
                        if (!entities.Users.Any(x => x.Email == user.Email))
                        {
                            user.userrightid = 2;
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

        //Logout controller 
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
                SampleShareDBEntities entities = new SampleShareDBEntities();
                ViewBag.Categories = entities.Categories.ToList();
                return View();
            }
            else
                return RedirectToAction("Index", "Main");
        }

        [HttpPost]
        public ActionResult UploadSample(HttpPostedFileBase uploadFile, AudioSamples audioSample)
        {
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
    }
}