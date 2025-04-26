using Group13iFinanceFix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Group13FinanceFix.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult AdminDashboard()
        {
            using (var db = new Group13_iFINANCEDBEntities1()) //accessing the database
            {
                var users = db.iFINANCEUser.ToList();//Store everything into a list
                return View(users); //pass list to the view
            }
        }

        [HttpGet]
        public ActionResult EditUser(string id)
        {
            using (var db = new Group13_iFINANCEDBEntities1())
            {
                var user = db.iFINANCEUser
                             .Include("NonAdminUser") // Load related NonAdminUser
                             .FirstOrDefault(u => u.ID == id);

                if (user == null)
                    return HttpNotFound();

                return View(user);
            }
        }

        [HttpPost]
        public ActionResult EditUser(iFINANCEUser updatedUser, string Email, string Address)
        {
            using (var db = new Group13_iFINANCEDBEntities1())
            {
                var user = db.iFINANCEUser.FirstOrDefault(u => u.ID == updatedUser.ID); //find the user
                if (user != null)
                {
                    user.UsersName = updatedUser.UsersName; //update their name

                    var nonAdmin = db.NonAdminUser.FirstOrDefault(n => n.ID == updatedUser.ID);
                    if (nonAdmin != null)
                    {
                        nonAdmin.Email = Email; //update email
                        nonAdmin.StreetAddress = Address; //update address
                    }

                    db.SaveChanges();
                }
            }

            TempData["Success"] = "User updated successfully!";
            return RedirectToAction("AdminDashboard");
        }

        public ActionResult DeleteUser(string id) //Delete a user
        {
            using (var db = new Group13_iFINANCEDBEntities1())
            {
                var user = db.iFINANCEUser.FirstOrDefault(u => u.ID == id); //find user
                if (user != null)
                {
                    db.iFINANCEUser.Remove(user);

                    var login = db.UserPassword.FirstOrDefault(p => p.ID == id); //remove from table
                    if (login != null) db.UserPassword.Remove(login);

                    var nonAdmin = db.NonAdminUser.FirstOrDefault(n => n.ID == id); //remove from non admin table
                    if (nonAdmin != null) db.NonAdminUser.Remove(nonAdmin);

                    db.SaveChanges();
                }
            }

            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("AdminDashboard");
        }
    }
}

    

