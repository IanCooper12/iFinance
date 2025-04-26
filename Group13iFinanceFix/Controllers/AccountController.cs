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


        [HttpGet]
        public ActionResult Register() //Registration form
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var db = new Group13_iFINANCEDBEntities1())
            {
                var newUser = new iFINANCEUser
                {
                    ID = model.ID,
                    UsersName = model.Username
                };
                db.iFINANCEUser.Add(newUser);

                var userPassword = new UserPassword //Set all user values
                {
                    ID = model.ID,
                    userName = model.Username,
                    encryptedPassword = model.Password,
                    passwordExpiryTime = 90,
                    userAccountExpiryDate = DateTime.Now.AddYears(1)
                };
                db.UserPassword.Add(userPassword);

                if (model.IsAdmin) //if adding an admin theres different logic
                {
                    var admin = new Administrator
                    {
                        ID = model.ID,
                        dateHired = DateTime.Now
                    };
                    db.Administrator.Add(admin);
                }
                else
                {
                    var nonAdmin = new NonAdminUser
                    {
                        ID = model.ID,
                        StreetAddress = model.StreetAddress,
                        Email = model.Email
                    };
                    db.NonAdminUser.Add(nonAdmin);
                }

                db.SaveChanges();
            }

            TempData["Success"] = "User registered successfully!";
            return RedirectToAction("AdminDashboard", "Admin"); //Go back to the dashboard after registered
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {//Forgot password logic. and check to make sure passwords match
            if (!ModelState.IsValid)
                return View(model);

            if (model.NewPassword != model.ConfirmPassword)
            {
                ViewBag.Message = "New passwords do not match.";
                return View(model);
            }

            using (var db = new Group13_iFINANCEDBEntities1())
            {
                // Only allow password reset for non-admin users
                var user = db.NonAdminUser.FirstOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    ViewBag.Message = "Email not found or not a non-admin account.";
                    return View(model);
                }

                var password = db.UserPassword.FirstOrDefault(p => p.ID == user.ID);
                if (password == null)
                {
                    ViewBag.Message = "Password record not found.";
                    return View(model);
                }

                password.encryptedPassword = model.NewPassword;
                db.SaveChanges();

                TempData["Success"] = "Password reset successfully.";
                return RedirectToAction("Login");
            }
        }
    }
}