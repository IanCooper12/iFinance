using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Group13iFinanceFix.Models;

namespace Group13iFinanceFix.Controllers
{
    public class NonAdminController : Controller
    {
        private Group13_iFINANCEDBEntities1 db = new Group13_iFINANCEDBEntities1();

        // GET: NonAdmin/NonAdminDashboard
        [HttpGet]
        public ActionResult NonAdminDashboard()
        {
            var userId = Session["UserID"] as string;
            // Ensure the user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                return View("Error");
            }

            var user = db.NonAdminUser.FirstOrDefault(u => u.ID == userId); // change table to NonAdminUser
            var profile = db.iFINANCEUser.FirstOrDefault(u => u.ID == userId); // get name from iFINANCEUser

            if (user != null && profile != null)
            {
                ViewBag.UsersName = profile.UsersName; // set the user's name
                return View(user); // pass the NonAdminUser model
            }
            return RedirectToAction("Login", "Account"); // better fallback

        }

        // GET: NonAdmin/AccountGroups
        [HttpGet]
        public ActionResult AccountGroups()
        {
            var userId = Session["UserID"] as string;
            // Ensure the user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                return View("Error");
            }
            var currentUser = db.iFINANCEUser.FirstOrDefault(u => u.ID == userId);
            if (currentUser == null)
            {
                return View("Error");
            }
            var userGroups = db.GroupTable.Where(g => g.userId == userId).ToList();

            // Get the list of element names of each group from element Ids
            var elementNames = new Dictionary<string, string>();
            foreach (var group in userGroups)
            {
                var elementName = db.AccountCategory.First(c => c.ID == group.element).accountName;
                elementNames.Add(group.ID, elementName);
            }
            ViewBag.elementNames = elementNames;

            // Get the list of parent names of each group from group Ids
            var parentNames = new Dictionary<string, string>();
            foreach (var group in userGroups)
            {
                if (group.parent == null) continue;
                var parentName = db.GroupTable.First(c => c.ID == group.parent).groupName;
                parentNames.Add(group.ID, parentName);
            }
            ViewBag.parentNames = parentNames;


            return View(userGroups);
        }

        // GET: NonAdmin/CreateAccountGroup
        [HttpGet]
        public ActionResult CreateAccountGroup()
        {
            var userId = Session["UserID"] as string;
            // Ensure the user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                return View("Error");
            }
            var currentUser = db.iFINANCEUser.FirstOrDefault(u => u.ID == userId);
            if (currentUser == null)
            {
                return View("Error");
            }

            // Fetch all categories from the AccountCategory table
            var categories = db.AccountCategory.ToList();

            // Pass the categories to the view using ViewBag
            ViewBag.Categories = categories;

            return View();
        }

        // Creating account group form submission
        [HttpPost]
        public ActionResult CreateAccountGroup(GroupTable group)
        {
            var userId = Session["UserID"] as string;

            // Ensure the user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                return View("Error");
            }

            // Fetch the current user
            var currentUser = db.iFINANCEUser.FirstOrDefault(u => u.ID == userId);
            if (currentUser == null)
            {
                return View("Error");
            }

            // Get the parent field from the form submission
            var parent = Request.Form["parent"];

            // Check if a GroupTable entry exists for the given parent and user
            var parentGroup = db.GroupTable.FirstOrDefault(g => g.groupName == parent && g.userId == userId);
            var parentId = parentGroup?.ID;

            var newGroup = new GroupTable
            {
                ID = Guid.NewGuid().ToString(), // Generate a unique ID
                groupName = group.groupName,   // Use the submitted group name
                parent = parentId,             // Set the parent ID
                element = group.element,       // Use the submitted element
                userId = userId                // Associate the group with the current user
            };

            // Add the new group to the database
            db.GroupTable.Add(newGroup);
            db.SaveChanges();

            return RedirectToAction("AccountGroups");
        }

        // Deleting an account group form submission
        [HttpPost]
        public ActionResult DeleteAccountGroup(string id)
        {
            // Fetch the group by ID
            var group = db.GroupTable.Find(id);
            if (group != null)
            {
                // Check if there are any associated Master accounts
                if (db.MasterAccount.Where(m => m.accountGroup == group.ID).Any())
                {
                    // Don't try to delete it if there are
                    TempData["ErrorMessage"] = "Cannot delete the group because it has associated MasterAccount records.";
                    return RedirectToAction("AccountGroups");
                }

                // Recursively delete all child groups
                if (!DeleteChildGroups(group.ID))
                {
                    return RedirectToAction("AccountGroups");
                }

                // Delete the parent group
                db.GroupTable.Remove(group);
                db.SaveChanges();
            }

            return RedirectToAction("AccountGroups");
        }
        private bool DeleteChildGroups(string parentId)
        {
            // Find all child groups where parent matches the given parentId
            var childGroups = db.GroupTable.Where(g => g.parent == parentId).ToList();

            foreach (var childGroup in childGroups)
            {
                // Check if there are any associated Master accounts
                if (db.MasterAccount.Where(m => m.accountGroup == childGroup.ID).Any())
                {
                    // Don't try to delete it if there are
                    TempData["ErrorMessage"] = "Cannot delete the group because it has associated MasterAccount records.";
                    return false;
                }

                // Recursively delete the children of the current child group
                if (!DeleteChildGroups(childGroup.ID))
                {
                    return false;
                }

                // Delete the current child group
                db.GroupTable.Remove(childGroup);
            }

            // Save changes after removing all child groups
            db.SaveChanges();

            return true;
        }

        // GET: NonAdmin/ChartOfAccounts
        [HttpGet]
        public ActionResult ChartOfAccounts()
        {
            var userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                return View("Error");
            }

            var userGroupIds = db.GroupTable
                .Where(g => g.userId == userId)
                .Select(g => g.ID)
                .ToList();

            var masterAccounts = db.MasterAccount
                .Where(ma => userGroupIds.Contains(ma.accountGroup))
                .ToList();

            return View(masterAccounts);
        }

        // GET: NonAdmin/AddMasterAccount
        [HttpGet]
        public ActionResult AddMasterAccount()
        {
            var userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                return View("Error");
            }

            // Fetch groups for the dropdown
            var userGroups = db.GroupTable
                .Where(g => g.userId == userId)
                .Select(g => new { g.ID, g.groupName })
                .ToList();

            ViewBag.Groups = new SelectList(userGroups, "ID", "groupName");
            return View();
        }

        // POST: NonAdmin/AddMasterAccount
        [HttpPost]
        public ActionResult AddMasterAccount(MasterAccount masterAccount)
        {
            if (ModelState.IsValid)
            {
                masterAccount.ID = Guid.NewGuid().ToString();
                masterAccount.closingAmount = masterAccount.openingAmount;
                db.MasterAccount.Add(masterAccount);
                db.SaveChanges();
                return RedirectToAction("ChartOfAccounts");
            }

            var userId = Session["UserID"] as string;
            var userGroups = db.GroupTable
                .Where(g => g.userId == userId)
                .Select(g => new { g.ID, g.groupName })
                .ToList();

            ViewBag.Groups = new SelectList(userGroups, "ID", "groupName");
            return View(masterAccount);
        }

        // GET: NonAdmin/EditMasterAccount/{id}
        [HttpGet]
        public ActionResult EditMasterAccount(string id)
        {
            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount == null)
            {
                return HttpNotFound();
            }

            var userId = Session["UserID"] as string;
            var userGroups = db.GroupTable
                .Where(g => g.userId == userId)
                .Select(g => new { g.ID, g.groupName })
                .ToList();

            ViewBag.Groups = new SelectList(userGroups, "ID", "groupName", masterAccount.accountGroup);
            return View(masterAccount);
        }

        // POST: NonAdmin/EditMasterAccount
        [HttpPost]
        public ActionResult EditMasterAccount(MasterAccount masterAccount)
        {
            if (ModelState.IsValid)
            {
                // Calculate the total debits for the account
                var totalDebits = db.TransactionLine
                    .Where(t => t.firstMasterAccount == masterAccount.ID)
                    .Sum(t => (double?)t.debitAmount) ?? 0;

                // Calculate the total debits where the account is the secondMasterAccount
                var totalCredits = db.TransactionLine
                    .Where(t => t.firstMasterAccount == masterAccount.ID)
                    .Sum(t => (double?)t.creditedAmount) ?? 0;

                // Update the closing amount
                masterAccount.closingAmount = (masterAccount.openingAmount ?? 0)
                                              + totalDebits
                                              - totalCredits;

                // Mark the entity as modified and save changes
                var entry = db.Entry(masterAccount);
                entry.State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ChartOfAccounts");
            }

            // If the model is invalid, reload the groups for the dropdown and return the view
            var userId = Session["UserID"] as string;
            var userGroups = db.GroupTable
                .Where(g => g.userId == userId)
                .Select(g => new { g.ID, g.groupName })
                .ToList();

            ViewBag.Groups = new SelectList(userGroups, "ID", "groupName", masterAccount.accountGroup);
            return View(masterAccount);
        }



        // POST: NonAdmin/DeleteMasterAccount/{id}
        [HttpPost]
        public ActionResult DeleteMasterAccount(string id)
        {
            var masterAccount = db.MasterAccount.Find(id);
            if (masterAccount != null)
            {
                db.MasterAccount.Remove(masterAccount);
                db.SaveChanges();
            }

            return RedirectToAction("ChartOfAccounts");
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            var userId = Session["UserID"]?.ToString();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            return View(); // this is required for the GET route to load the page
        }



        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var userId = Session["UserID"]?.ToString(); //grab current user
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            if (model.NewPassword != model.ConfirmPassword)
            {
                ViewBag.Message = "New passwords do not match."; //mismatch
                return View(model);
            }

            using (var db = new Group13_iFINANCEDBEntities1())
            {
                var user = db.UserPassword.FirstOrDefault(u => u.ID == userId);

                if (user == null || user.encryptedPassword != model.CurrentPassword)
                {
                    ViewBag.Message = "Current password is incorrect."; //wrong current
                    return View(model);
                }

                user.encryptedPassword = model.NewPassword;
                db.SaveChanges(); //save new pass

                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("NonAdminDashboard", "NonAdmin");
            }
        }

        public ActionResult BalanceSheet()
        {
            var userId = Session["UserID"]?.ToString(); //Grab user
            var isAdmin = Session["IsAdmin"] as bool?; //Check admin

            if (userId == null || isAdmin == true)
                return RedirectToAction("Login", "Account"); // block if not logged in or is admin

            using (var db = new Group13_iFINANCEDBEntities1())
            {   //Grab all accounts with type from category
                var data = (
                    from ma in db.MasterAccount
                    join grp in db.GroupTable on ma.accountGroup equals grp.ID
                    join cat in db.AccountCategory on grp.element equals cat.ID
                    where grp.userId == userId
                    select new ReportEntry
                    {
                        AccountName = ma.name,
                        Amount = ma.closingAmount ?? 0, //handles nulls
                        AccountType = cat.accountName
                    }
                ).ToList();

                //Filter between assest liability and equity
                ViewBag.Assets = data.Where(d => d.AccountType == "Assets").ToList();
                ViewBag.Liabilities = data.Where(d => d.AccountType == "Liabilities").ToList();
                ViewBag.Equity = data.Where(d => d.AccountType == "Equity").ToList();

                return View();
            }
        }

        public ActionResult ProfitAndLoss()
        {
            var userId = Session["UserID"]?.ToString(); // get current user
            var isAdmin = Session["IsAdmin"] as bool?;  // check if admin
            if (userId == null || isAdmin == true)
                return RedirectToAction("Login", "Account"); // block admins or not logged in

            using (var db = new Group13_iFINANCEDBEntities1())
            {
                // grab accounts and join with category for types
                var data = (
                    from ma in db.MasterAccount
                    join grp in db.GroupTable on ma.accountGroup equals grp.ID
                    join cat in db.AccountCategory on grp.element equals cat.ID
                    where grp.userId == userId
                    select new ReportEntry
                    {
                        AccountName = ma.name,
                        Amount = ma.closingAmount ?? 0, // fallback if null
                        AccountType = cat.accountName
                    }
                ).ToList();

                // filter types
                ViewBag.Income = data.Where(d => d.AccountType == "Income").ToList();
                ViewBag.Expenses = data.Where(d => d.AccountType == "Expenses").ToList();

                // cast back so we can sum it (lambda issue workaround)
                var incomeList = ViewBag.Income as List<ReportEntry>;
                var expenseList = ViewBag.Expenses as List<ReportEntry>;

                ViewBag.TotalIncome = incomeList?.Sum(i => i.Amount) ?? 0; // total income
                ViewBag.TotalExpenses = expenseList?.Sum(e => e.Amount) ?? 0; // total expenses

                return View(); // push to view
            }
        }
    }
}
   