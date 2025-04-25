using Group13iFinanceFix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Group13iFinanceFix.Controllers
{
    public class AccountController : Controller
    {
        private Group13_iFINANCEDBEntities1 db = new Group13_iFINANCEDBEntities1();

        [HttpGet]
        public ActionResult Login() //display the login form
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.UserPassword //look for a user matching username and password
                .FirstOrDefault(u => u.userName == model.Username && u.encryptedPassword == model.Password);

            if (user != null) //If match found
            {
                var isAdmin = db.Administrator.Any(a => a.ID == user.ID); //Check if the user is an admin

                //Save their info
                Session["UserID"] = user.ID;
                Session["IsAdmin"] = isAdmin;
                //Redirect based on role
                return isAdmin
                    ? RedirectToAction("AdminDashboard", "Admin")
                    : RedirectToAction("NonAdminDashboard", "NonAdmin"); //Not tested yet
            }

            ViewBag.Message = "Invalid username or password.";
            return View(model);
        }
    }
}