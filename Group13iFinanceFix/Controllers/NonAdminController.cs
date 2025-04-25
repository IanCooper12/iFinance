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

            // Fetch GroupTable data from the database
            var currentUser = db.iFINANCEUser.FirstOrDefault(u => u.ID == userId);

            if (currentUser != null)
            {
                return View(currentUser);
            }
            return View("Error");
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
                // Recursively delete all child groups
                DeleteChildGroups(group.ID);

                // Delete the parent group
                db.GroupTable.Remove(group);
                db.SaveChanges();
            }

            return RedirectToAction("AccountGroups");
        }
        private void DeleteChildGroups(string parentId)
        {
            // Find all child groups where parent matches the given parentId
            var childGroups = db.GroupTable.Where(g => g.parent == parentId).ToList();

            foreach (var childGroup in childGroups)
            {
                // Recursively delete the children of the current child group
                DeleteChildGroups(childGroup.ID);

                // Delete the current child group
                db.GroupTable.Remove(childGroup);
            }

            // Save changes after removing all child groups
            db.SaveChanges();
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
                db.Entry(masterAccount).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ChartOfAccounts");
            }

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




    }
}
   