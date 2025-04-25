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
    }
}
