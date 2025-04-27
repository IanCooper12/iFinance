using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group13iFinanceFix.Models;

namespace Group13iFinanceFix.Controllers
{
    public class FinanceTransactionsController : Controller
    {
        private Group13_iFINANCEDBEntities1 db = new Group13_iFINANCEDBEntities1();

        // GET: FinanceTransactions
        public ActionResult Index()
        {
            // Ensure the user is logged in
            var userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return View("Error");
            if (db.NonAdminUser.FirstOrDefault(u => u.ID == userId) == null) return View("Error"); // Must be logged in to access

            var financeTransactions = db.FinanceTransaction
                .Include(f => f.NonAdminUser)
                .Where(f => f.authorID == userId);
            return View(financeTransactions);
        }

        // GET: FinanceTransactions/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return View("Error");
            }

            // Ensure the user is logged in
            var userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return View("Error");
            if (db.NonAdminUser.FirstOrDefault(u => u.ID == userId) == null) return View("Error"); // Must be logged in to access

            // Get the FinanceTransaction along with its TransactionLines
            var financeTransaction = db.FinanceTransaction
                .Include(f => f.TransactionLine)
                .FirstOrDefault(f => f.ID == id && f.authorID == userId);

            // Get the names of the master accounts
            var masterAccountNames = new Dictionary<string, string>();
            foreach (var line in financeTransaction.TransactionLine)
            {
                var firstMasterAccount = db.MasterAccount.FirstOrDefault(m => m.ID == line.firstMasterAccount);
                var secondMasterAccount = db.MasterAccount.FirstOrDefault(m => m.ID == line.secondMasterAccount);
                masterAccountNames[line.firstMasterAccount] = firstMasterAccount.name;
                masterAccountNames[line.secondMasterAccount] = secondMasterAccount.name;
            }

            ViewBag.MasterAccountNames = masterAccountNames;


            return View(financeTransaction);
        }

        // GET: FinanceTransactions/Create
        public ActionResult Create()
        {
            // Ensure the user is logged in
            var userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return View("Error");
            if (db.NonAdminUser.FirstOrDefault(u => u.ID == userId) == null) return View("Error"); // Must be logged in to access

            var model = new FinanceTransactionViewModel {
                Transaction = new FinanceTransaction(),
                TransactionLines = new List<TransactionLine>() {
                    new TransactionLine(), // One debit line
                    new TransactionLine()  // One credit line
                },
            };

            ViewBag.Accounts = new SelectList(db.MasterAccount, "ID", "name");
            return View(model);
        }

        // POST: FinanceTransactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FinanceTransactionViewModel model)
        {
            // Ensure the user is logged in
            var userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return View("Error");
            if (db.NonAdminUser.FirstOrDefault(u => u.ID == userId) == null) return View("Error"); // Must be logged in to access

            // Validate credit/debit amounts match
            double totalDebits = (double)model.TransactionLines.Sum(t => t.debitAmount);
            double totalCredits = (double)model.TransactionLines.Sum(t => t.creditedAmount);
            if (Math.Abs(totalDebits - totalCredits) > 0.01)
            {
                ModelState.AddModelError("", "Debits and Credits must be equal.");
                return View(model);
            }

            // Generate data for Transaction
            model.Transaction.ID = Guid.NewGuid().ToString(); // Generate random ID
            model.Transaction.TransactionDate = DateTime.Now; // transaction date is the current date
            model.Transaction.authorID = userId; // Use currently logged in user as author

            // Assign IDs for lines and link to transaction
            foreach (var line in model.TransactionLines)
            {
                line.ID = Guid.NewGuid().ToString();
                line.transactionID = model.Transaction.ID;
                db.TransactionLine.Add(line);

                // Update closing amounts for affected accounts
                var firstAccount = db.MasterAccount.FirstOrDefault(m => m.ID == line.firstMasterAccount);

                if (firstAccount != null)
                {
                    firstAccount.closingAmount = (firstAccount.closingAmount ?? 0) + (line.debitAmount ?? 0) - (line.creditedAmount ?? 0);
                }
            }

            // Assume there are 2 lines and set the first/second master accounts
            var firstLine = model.TransactionLines.First();
            var secondLine = model.TransactionLines.Last();
            firstLine.secondMasterAccount = secondLine.firstMasterAccount;
            secondLine.secondMasterAccount = firstLine.firstMasterAccount;

            db.FinanceTransaction.Add(model.Transaction);
            db.SaveChanges();

            return RedirectToAction("Index");
        }



        // GET: FinanceTransactions/Edit/{id}
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinanceTransaction financeTransaction = db.FinanceTransaction.Find(id);
            if (financeTransaction == null)
            {
                return HttpNotFound();
            }
            return View(financeTransaction);
        }

        // POST: FinanceTransactions/Edit/{ID,TransactionDate,TransactionDescription,authorID}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,TransactionDate,TransactionDescription,authorID")] FinanceTransaction financeTransaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(financeTransaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(financeTransaction);
        }

        // GET: FinanceTransactions/Delete/{id}
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinanceTransaction financeTransaction = db.FinanceTransaction.Find(id);
            if (financeTransaction == null)
            {
                return HttpNotFound();
            }
            return View(financeTransaction);
        }

        // POST: FinanceTransactions/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var financeTransaction = db.FinanceTransaction.Find(id);
            var transactionLines = db.TransactionLine.Where(t => t.transactionID == id).ToList();

            // Reverse the changes to closing amounts for affected 
            foreach (var line in transactionLines)
            {
                var firstAccount = db.MasterAccount.FirstOrDefault(m => m.ID == line.firstMasterAccount);

                if (firstAccount != null)
                {
                    firstAccount.closingAmount = (firstAccount.closingAmount ?? 0) - (line.debitAmount ?? 0) + (line.creditedAmount ?? 0);
                    db.Entry(firstAccount).State = EntityState.Modified; // Mark as modified
                }
            }

            db.TransactionLine.RemoveRange(transactionLines);
            db.FinanceTransaction.Remove(financeTransaction);
            db.SaveChanges();

            return RedirectToAction("Index");
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
