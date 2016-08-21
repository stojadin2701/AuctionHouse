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

namespace IEP_Projekat.Controllers
{
    [Authorize]
    public class BidsController : Controller
    {
        private Model3 db = new Model3();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BidNow([Bind(Include = "Id,AuctionId,UserId")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                lock (Startup.objlock) { 
                    var user = UserManager.FindById(bid.UserId);
                    if (user.TokenNum > 0) {
                        user.TokenNum--;
                        var auction = db.Auctions.Where(a => a.Id == bid.AuctionId).First();
                        if (auction != null && auction.Duration>0 && auction.State == "OPEN") { 
                            auction.LastBidderId = user.Id;
                            auction.StartPrice++;
                            auction.ImageToUpload = new AuctionsController.MyHttpPostedFileBase();
                            bid.BidTime = DateTime.Now;
                            if (auction.Duration>0 && auction.Duration <= 10) {
                                auction.Duration = 10;
                                Startup.auctionTable[auction.Id].Duration = 10;
                            };
                            db.Bids.Add(bid);
                            db.SaveChanges();
                            UserManager.Update(user);
                            Startup.auctionTable[auction.Id].LastBidderUserName = user.UserName;
                            Startup.auctionTable[auction.Id].Price = auction.StartPrice;
                        } else if(auction!=null && auction.State == "OPEN")
                        {
                            if (auction.LastBidderId != null && user.TokenNum >= auction.StartPrice)
                            {
                                user.TokenNum -= auction.StartPrice;
                                auction.State = "SOLD";
                                UserManager.Update(user);
                            }
                            else auction.State = "EXPIRED";
                            auction.CloseTime = DateTime.Now;
                            UserManager.Update(user);
                            db.SaveChanges();
                        }
                    }
                }
                return RedirectToAction("Index","Home", null);
            }
            return View(bid);
        }

        // GET: Bids
        public ActionResult Index()
        {
            return View(db.Bids.ToList());
        }

        // GET: Bids/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bid bid = db.Bids.Find(id);
            if (bid == null)
            {
                return HttpNotFound();
            }
            return View(bid);
        }

        // GET: Bids/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Bids/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AuctionId,UserId,BidTime")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                db.Bids.Add(bid);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(bid);
        }

        // GET: Bids/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bid bid = db.Bids.Find(id);
            if (bid == null)
            {
                return HttpNotFound();
            }
            return View(bid);
        }

        // POST: Bids/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AuctionId,UserId,BidTime")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bid).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bid);
        }

        // GET: Bids/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bid bid = db.Bids.Find(id);
            if (bid == null)
            {
                return HttpNotFound();
            }
            return View(bid);
        }

        // POST: Bids/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Bid bid = db.Bids.Find(id);
            db.Bids.Remove(bid);
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
