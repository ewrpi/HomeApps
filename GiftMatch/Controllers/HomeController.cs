using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using GiftMatch.Helpers;
using GiftMatch.Models;

namespace GiftMatch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("Profile");

            return View(new IndexModel());
        }

        public ActionResult Profile()
        {
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index");

            UserProfile model = new UserProfile();
            model.UserName = User.Identity.Name;
            model.LoginModel = new LoginModel();

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
