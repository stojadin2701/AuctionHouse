using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IEP_Projekat.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using PagedList;


namespace IEP_Projekat.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {


        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private Model1 db = new Model1();

        // GET: Orders
        public ActionResult Index(int? page)
        {
            if(page==null) page = 1;
            var userId = User.Identity.GetUserId();
            
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(db.Orders.Where(x => x.UserId == userId).OrderByDescending(x=>x.OrderTime).ToPagedList(pageNumber, pageSize));
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(order);
        }

        // GET: Orders/GetTokens
        public ActionResult GetTokens()
        {
            var model = new OrderViewModel()
            {
                BundleList = new List<SelectListItem>()
                {
                    new SelectListItem {Text="3 (1€)", Value="0" },
                    new SelectListItem {Text="5 (1.5€)", Value="1" },
                    new SelectListItem {Text="10 (2.5€)", Value="2" },
                }
        };
            return View(model);
        }

        // POST: Orders/GetTokens
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetTokens([Bind(Include = "BundleId,PhoneNum")] OrderViewModel order)
        {
             if (ModelState.IsValid)
             {
                var dbOrder = new Order {
                    OrderTime = DateTime.Now,
                    Status = "Waiting for confirmation",
                    UserId = User.Identity.GetUserId(),
                };

                switch (order.BundleId)
                {
                    case 0:
                        dbOrder.Price = 1;
                        dbOrder.TokenNum = 3;
                        break;
                    case 1:
                        dbOrder.Price = 1.5m;
                        dbOrder.TokenNum = 5;
                        break;
                    case 2:
                        dbOrder.Price = 2.5m;
                        dbOrder.TokenNum = 10;
                        break;
                }
                db.Orders.Add(dbOrder);
                db.SaveChanges();
                Response.BufferOutput = true;
                string redirect = "https://stage.centili.com/widget/WidgetModule?api=1fc5e477ba56bb3029b4e607f2377adb&package=" + order.BundleId + "&packagelock=true&clientid=" + dbOrder.Id + "&phone=" + order.PhoneNum + "&phonelock=true&autonext=true";
                Response.Redirect(redirect);
                return View("Index", "Home", null);
            }
            var model = new OrderViewModel()
            {
                BundleList = new List<SelectListItem>()
                {
                    new SelectListItem {Text="3 (1€)", Value="0" },
                    new SelectListItem {Text="5 (1.5€)", Value="1" },
                    new SelectListItem {Text="10 (2.5€)", Value="2" },
                }
            };
            return View(model);
        }

        //
        //GET: /Orders/ProcessPayment
        [AllowAnonymous]
        public HttpStatusCodeResult ProcessPayment(string clientid, string errormessage, string status, int? amount)
        {
            var dbOrder = db.Orders.Find(Convert.ToInt32(clientid));

            if (dbOrder != null)
            {
                var user = UserManager.FindById(dbOrder.UserId);
                if (user != null) { 
                    if (status.Equals("success"))
                    {
                        dbOrder.Status = "Completed";
                        db.SaveChanges();
                        user.TokenNum += dbOrder.TokenNum;
                        UserManager.Update(user);
                        return new HttpStatusCodeResult(200);
                    }
                    else if (status.Equals("failed") || status.Equals("canceled"))
                    {
                        dbOrder.Status = "Failed";
                        db.SaveChanges();
                        return new HttpStatusCodeResult(406);
                    }
                }
            }
            return new HttpStatusCodeResult(406);
        }

        /*// GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,TokenNum,Price,Status,OrderTime")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index");
        }*/

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
