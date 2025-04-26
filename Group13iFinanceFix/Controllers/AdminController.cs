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
                    // Delete from Users
                    db.iFINANCEUser.Remove(user);

                    // Remove password
                    var login = db.UserPassword.FirstOrDefault(p => p.ID == id);
                    if (login != null) db.UserPassword.Remove(login);


                    // remove from Non admin table
                    var nonAdmin = db.NonAdminUser.FirstOrDefault(n => n.ID == id);
                    if (nonAdmin != null) {
                        db.NonAdminUser.Remove(nonAdmin);

                        // Remove all TransactionLines
                        var transactions = db.FinanceTransaction.Where(ft => ft.authorID == id).ToList();
                        foreach (var transaction in transactions)
                        {
                            var transactionLines = db.TransactionLine.Where(tl => tl.transactionID == transaction.ID).ToList();
                            db.TransactionLine.RemoveRange(transactionLines);
                        }

                        // Remove FinanceTransactions
                        db.FinanceTransaction.RemoveRange(transactions);

                        // Remove MasterAccount records
                        var groups = db.GroupTable.Where(gt => gt.userId == id).ToList();
                        var groupIds = groups.Select(g => g.ID).ToList();
                        var masterAccounts = db.MasterAccount.Where(ma => groupIds.Contains(ma.accountGroup)).ToList();
                        db.MasterAccount.RemoveRange(masterAccounts);

                        // Remove Group records
                        db.GroupTable.RemoveRange(groups);
                    }

                    // Remove from admin table
                    var admin = db.Administrator.FirstOrDefault(n => n.ID == id); 
                    if (admin != null) db.Administrator.Remove(admin);

                    db.SaveChanges();
                }
            }

            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("AdminDashboard");
        }
    }
}

    

