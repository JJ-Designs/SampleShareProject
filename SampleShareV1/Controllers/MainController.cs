﻿using SampleShareV1.Models;
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
        [ActionName("Catalog")]
        public ActionResult Catalog()
        {
            SampleShareDBEntities entities = new SampleShareDBEntities();
            List<AudioSamples> audioSamples = entities.AudioSamples.Where(a => a.isPublic == true).ToList();
            ViewBag.Categories = entities.Categories.ToList();

            // Container Name - Sample  
            BlobController BlobManagerObj = new BlobController("samples");
            //ViewBag.FilePath = BlobManagerObj.
            return View(audioSamples);
        }

        [HttpGet]
        [ActionName("MyPortfolio")]
        public ActionResult MyPortfolio()
        {
            SampleShareDBEntities entities = new SampleShareDBEntities();
            List<AudioSamples> audioSamples = entities.AudioSamples.Where(a => a.isPublic == true).ToList();
            ViewBag.Categories = entities.Categories.ToList();
            return View(audioSamples);
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
            {
                return RedirectToAction("Index", "Main");
            }
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
            if (Session["UserID"] != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Main");
            }
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
        public ActionResult SignUp(Users users)
        {
            SampleShareDBEntities entities = new SampleShareDBEntities();

            users.userrightid = 2;

            entities.Users.Add(users);
            entities.SaveChanges();
            return RedirectToAction("login");
            
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
            {
                return RedirectToAction("Index", "Main");
            }
        }

        [HttpPost]
        public ActionResult UploadSample(HttpPostedFileBase uploadFile, AudioSamples audioSample)
        {
            foreach (string file in Request.Files)
            {
                uploadFile = Request.Files[file];
            }
            SampleShareDBEntities entities = new SampleShareDBEntities();
            string fileExt = uploadFile.FileName.Substring(uploadFile.FileName.IndexOf("."));
            audioSample.FilePath = audioSample.SampleTitel + fileExt;
            audioSample.CreationDate = DateTime.Now;
            audioSample.Downloads = 0;
            string id = (string)Session["UserID"];
            audioSample.UserID = Int32.Parse(id);

            entities.AudioSamples.Add(audioSample);

            // Container Name - Sample  
            BlobController BlobManagerObj = new BlobController("samples"); //constrktor ses i Billede eksempel 2
            string FileAbsoluteUri = BlobManagerObj.UploadFile(uploadFile, audioSample.FilePath); //metode ses i Billede eksempel 3
            entities.SaveChanges();

            return RedirectToAction("index");
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
            return RedirectToAction("index");
        }
    }
}