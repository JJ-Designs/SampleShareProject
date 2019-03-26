using SampleShareV1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public ActionResult Catalog()
        {
            return View();
        }

        public ActionResult MyContent()
        {
            return View();
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
                return RedirectToAction("index", "Home");
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
                        ViewBag.Message = "Brugernavn eller password er forkert.";
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
    }
}