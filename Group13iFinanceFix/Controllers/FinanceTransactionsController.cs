using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
            var financeTransaction = db.FinanceTransaction.Include(f => f.NonAdminUser);
            return View(financeTransaction.ToList());
        }

        // GET: FinanceTransactions/Details/5
        public ActionResult Details(string id)
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

        // GET: FinanceTransactions/Create
        public ActionResult Create()
        {
            var model = new FinanceTransactionViewModel
            {
                Transaction = new FinanceTransaction(),
                TransactionLines = new List<TransactionLine>
            {
            new TransactionLine(), // One debit line
            new TransactionLine()  // One credit line
            }
            };

            ViewBag.authorID = new SelectList(db.NonAdminUser, "ID", "StreetAddress");
            return View(model);
        }

        // POST: FinanceTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FinanceTransactionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate firstMasterAccount values
                var validMasterAccountIds = db.MasterAccount.Select(ma => ma.ID).ToHashSet();
                foreach (var line in model.TransactionLines)
                {
                    if (!validMasterAccountIds.Contains(line.firstMasterAccount))
                    {
                    
                        ModelState.AddModelError("", $"Invalid Master Account ID: {line.firstMasterAccount}.");
                        ViewBag.authorID = new SelectList(db.NonAdminUser, "ID", "StreetAddress", model.Transaction.authorID);
                        return View(model);
                    }
                }
                double totalDebits = (double)model.TransactionLines.Sum(t => t.debitAmount);
                double totalCredits = (double)model.TransactionLines.Sum(t => t.creditedAmount);

                if (Math.Abs(totalDebits - totalCredits) > 0.01)
                {
                    ModelState.AddModelError("", "Debits and Credits must be equal.");
                    ViewBag.authorID = new SelectList(db.NonAdminUser, "ID", "StreetAddress", model.Transaction.authorID);
                    return View(model);
                }

                // Assign ID for transaction
                model.Transaction.ID = Guid.NewGuid().ToString();
                db.FinanceTransaction.Add(model.Transaction);

                // Assign IDs for lines and link to transaction
                foreach (var line in model.TransactionLines)
                {
                    line.ID = Guid.NewGuid().ToString();
                    line.transactionID = model.Transaction.ID;
                    db.TransactionLine.Add(line);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.authorID = new SelectList(db.NonAdminUser, "ID", "StreetAddress", model.Transaction.authorID);
            return View(model);
        }

        // GET: FinanceTransactions/Edit/5
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
            ViewBag.authorID = new SelectList(db.NonAdminUser, "ID", "StreetAddress", financeTransaction.authorID);
            return View(financeTransaction);
        }

        // POST: FinanceTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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
            ViewBag.authorID = new SelectList(db.NonAdminUser, "ID", "StreetAddress", financeTransaction.authorID);
            return View(financeTransaction);
        }

        // GET: FinanceTransactions/Delete/5
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

        // POST: FinanceTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            FinanceTransaction financeTransaction = db.FinanceTransaction.Find(id);
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
