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
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Web.UI;

namespace IEP_Projekat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuctionsController : Controller
    {
        public class MyHttpPostedFileBase : HttpPostedFileBase
        {

        }

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

        public Model3 db = new Model3();

        [HttpPost]
        [AllowAnonymous]
        public string GetTimeLeft(List<int> auctionIds)
        {
            string retStr = null;
            lock (Startup.objlock) {
                if (auctionIds != null) { 
                foreach (var id in auctionIds)
                {
                    //var auction = db.Auctions.Find(id);
                    /*if (auction != null)
                    {
                        retStr+= ""+ id + " " + auction.Duration+";";
                    }*/
                    AuctionMemoryModel auction = null;
                    try
                    {
                        auction = Startup.auctionTable[id];
                    }
                    catch (KeyNotFoundException) { }
                    if (auction != null && auction.Duration>=0)
                    {
                        retStr += "" + id + " " + auction.Duration+","+auction.Price;
                        if (auction.LastBidderUserName != null) retStr += "," + auction.LastBidderUserName;
                        retStr += ";";
                    }
                }
                foreach (var auction in Startup.toSend)
                {
                    if (auction != null)
                    {
                        retStr += "" + auction.Id + " " + auction.Duration + "," + auction.Price;
                        if (auction.LastBidderUserName != null) retStr += "," + auction.LastBidderUserName;
                        retStr += ";";
                    }
                }
                Startup.toSend = new List<AuctionMemoryModel>();
            }
            return retStr;
        }
        }

        [HttpPost]
        [AllowAnonymous]
        public string RefreshBidderListPartial(int? AuctionId)
        {
            //return PartialView("_PartialBidderList", db.Bids.Where(x => x.AuctionId == AuctionId).OrderByDescending(x => x.BidTime).Take(10)).ToString();
            string html = "";
            using (var sw = new StringWriter())
            {
                PartialViewResult result = PartialView("_PartialBidderList", db.Bids.Where(x => x.AuctionId == AuctionId).OrderByDescending(x => x.BidTime).Take(10).ToList());

                result.View = ViewEngines.Engines.FindPartialView(ControllerContext, "_PartialBidderList").View;

                ViewContext vc = new ViewContext(ControllerContext, result.View, result.ViewData, result.TempData, sw);

                result.View.Render(vc, sw);

                html = sw.GetStringBuilder().ToString();
            }

            return html;
        }

        // GET: Auctions/Open/1
        public ActionResult Open(int? id)
        {
             
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Auction auction = db.Auctions.Find(id);
                if (auction == null || auction.State != "READY")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            lock (Startup.writelock)
            {
                auction.State = "OPEN";
                auction.OpenTime = DateTime.Now;
                //auction.CloseTime = auction.OpenTime.Value.AddSeconds(auction.Duration);
                auction.ImageToUpload = new MyHttpPostedFileBase();
                db.Entry(auction).State = EntityState.Modified;
                string username = null;
                if (auction.LastBidderId != null)
                {
                    var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(usr => usr.Id == auction.LastBidderId).First();
                    username = user.UserName;
                }
                AuctionMemoryModel amm = new AuctionMemoryModel()
                {
                    CloseTime = auction.CloseTime,
                    CreateTime = auction.CreateTime,
                    Duration = auction.Duration,
                    Id = auction.Id,
                    Name = auction.Name,
                    OpenTime = auction.OpenTime,
                    Price = auction.StartPrice,
                    State = auction.State,
                    LastBidderUserName = username
                };
                Startup.auctionTable.Add(amm.Id, amm);
                try { 
                db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                                    validationError.PropertyName,
                                                    validationError.ErrorMessage);
                        }
                    }
                }
            }
            return RedirectToAction("Index");
            
        }

        // GET: Auctions
        public ActionResult Index()
        {
            List<AuctionViewModel> auctions = new List<AuctionViewModel>();
            foreach(Auction a in db.Auctions.ToList())
            {
                string username = null;
                if (a.LastBidderId != null)
                {
                    var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(usr => usr.Id == a.LastBidderId).First();
                    username = user.UserName;
                }
                auctions.Add(new AuctionViewModel()
                {
                    Id=a.Id,
                    Duration = a.Duration,
                    ImageContent = a.ImageContent,
                    Name = a.Name,
                    Price = a.StartPrice,
                    State = a.State,
                    LastBidderUserName = username,
                    CloseTime = a.CloseTime,
                    CreateTime = a.CreateTime,
                    OpenTime = a.OpenTime
                });
            }
            auctions = auctions.Where(a=>a.State!="DELETED").OrderByDescending(a=>a.State=="OPEN").ThenByDescending(a => a.OpenTime).ToList();
            return View(auctions);
        }

        // GET: Auctions/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null || auction.State=="DELETED")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string username = null;
            if (auction.LastBidderId != null)
            {
                var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(usr => usr.Id == auction.LastBidderId).First();
                username = user.UserName;
            }
            AuctionViewModel auctionView = new AuctionViewModel
            {
                Id = auction.Id,
                Duration = auction.Duration,
                ImageContent = auction.ImageContent,
                Name = auction.Name,
                Price = auction.StartPrice,
                State = auction.State,
                LastBidderUserName = username,
                CloseTime = auction.CloseTime,
                CreateTime = auction.CreateTime,
                OpenTime = auction.OpenTime
            };
            return View(auctionView);
        }

        // GET: Auctions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Duration,ImageToUpload,StartPrice")] AuctionCreateModel auctionModel)
        {
            if (ModelState.IsValid)
            {
                Auction auction = new Auction()
                {
                    Name=auctionModel.Name,
                    Duration = auctionModel.Duration,
                    ImageToUpload = auctionModel.ImageToUpload,
                    StartPrice = auctionModel.StartPrice,
                };
                auction.ImageContent = new Byte[auction.ImageToUpload.ContentLength];
                auction.ImageMimeType = auction.ImageToUpload.ContentType;
                auction.ImageToUpload.InputStream.Read(auction.ImageContent, 0, auction.ImageToUpload.ContentLength);
                auction.State = "READY";
                auction.CreateTime = DateTime.Now;
                db.Auctions.Add(auction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(auctionModel);
        }

        // GET: Auctions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            AuctionCreateModel auctionModel = new AuctionCreateModel()
            {
                Id = auction.Id,
                Name = auction.Name,
                Duration = auction.Duration,
                ImageToUpload = auction.ImageToUpload,
                StartPrice = auction.StartPrice,
                ImageMimeType = auction.ImageMimeType,
                State = auction.State,
                CreateTime = auction.CreateTime,
                ImageContent = auction.ImageContent
            };
            if (auction == null || auction.State!="READY")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(auctionModel);
        }

        // POST: Auctions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Duration,StartPrice,ImageMimeType,State,CreateTime,ImageToUpload")] AuctionCreateModel auctionModel)
        {
            if (ModelState.IsValid)
            {
                Auction auction = new Auction()
                {
                    Id = auctionModel.Id,
                    Name = auctionModel.Name,
                    Duration = auctionModel.Duration,
                    ImageToUpload = auctionModel.ImageToUpload,
                    StartPrice = auctionModel.StartPrice,
                    ImageMimeType=auctionModel.ImageMimeType,
                    State = auctionModel.State,
                    CreateTime = auctionModel.CreateTime
                };
                auction.ImageContent = new Byte[auction.ImageToUpload.ContentLength];
                auction.ImageMimeType = auction.ImageToUpload.ContentType;
                auction.ImageToUpload.InputStream.Read(auction.ImageContent, 0, auction.ImageToUpload.ContentLength);
                auction.State = "READY";
                db.Entry(auction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(auctionModel);
        }

        // GET: Auctions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auctions.Find(id);
            if (auction == null || auction.State!="READY")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string username = null;
            if (auction.LastBidderId != null)
            {
                var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(usr => usr.Id == auction.LastBidderId).First();
                username = user.UserName;
            }
            AuctionViewModel auctionView = new AuctionViewModel
            {
                Id = auction.Id,
                Duration = auction.Duration,
                ImageContent = auction.ImageContent,
                Name = auction.Name,
                Price = auction.StartPrice,
                State = auction.State,
                LastBidderUserName = username,
                CloseTime = auction.CloseTime,
                CreateTime = auction.CreateTime,
                OpenTime = auction.OpenTime
            };
            return View(auctionView);
        }

        // POST: Auctions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Auction auction = db.Auctions.Find(id);
            auction.State = "DELETED";
            //db.Auctions.Remove(auction);
            auction.ImageToUpload = new MyHttpPostedFileBase();
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
