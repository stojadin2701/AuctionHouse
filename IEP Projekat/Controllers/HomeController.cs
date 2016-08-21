using IEP_Projekat.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace IEP_Projekat.Controllers
{
    public class HomeController : Controller
    {
        private Model2 db = new Model2();
        public ActionResult Index(string searchString, string currentName, string stateList, string currentState, int? minPrice, int? currentMin, int? maxPrice, int? currentMax, int? page)
        {
            List<AuctionViewModel> auctions = new List<AuctionViewModel>();
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentName;
            }
            if (stateList != null)
            {
                page = 1;
            }
            else
            {
                stateList = currentState;
            }
            if (minPrice != null)
            {
                page = 1;
            }
            else
            {
                minPrice = currentMin;
            }
            if (maxPrice != null)
            {
                page = 1;
            }
            else
            {
                maxPrice = currentMax;
            }

            ViewBag.CurrentName = searchString;
            ViewBag.CurrentState = stateList;
            ViewBag.CurrentMin = minPrice;
            ViewBag.CurrentMax = maxPrice;

            
            foreach (Auction a in db.Auctions.ToList())
            {
                string username = null;
                if (a.LastBidderId != null)
                {
                    var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(usr => usr.Id == a.LastBidderId).First();
                    username = user.UserName;
                }
                if((a.State=="OPEN") || (a.State == "SOLD") || (a.State == "EXPIRED"))
                    auctions.Add(new AuctionViewModel()
                    {
                        Id = a.Id,
                        Duration = a.Duration,
                        ImageContent = a.ImageContent,
                        Name = a.Name,
                        Price = a.StartPrice,
                        State = a.State,
                        LastBidderUserName = username,
                        CloseTime = a.CloseTime,
                        CreateTime = a.CreateTime,
                        OpenTime = a.OpenTime,
                        
                    });
            }
            List<AuctionViewModel> auctionsRet = new List<AuctionViewModel>();
            if (!String.IsNullOrEmpty(searchString))
            {
                string[] srcStrings = searchString.Split(' ');
                foreach (string src in srcStrings)
                {
                    if (!String.IsNullOrEmpty(searchString))
                    {
                        auctionsRet = auctionsRet.Union(auctions.Where(a => a.Name.Contains(src))).ToList();
                    }
                }

            }
            else auctionsRet = auctions;
            if (!String.IsNullOrEmpty(stateList))
            {
                auctionsRet = auctionsRet.Where(a => a.State == stateList.ToUpper()).ToList();
            }
            if (minPrice != null)
            {
                auctionsRet = auctionsRet.Where(a => a.Price >= minPrice).ToList();
            }
            if (maxPrice != null && maxPrice>=minPrice)
            {
                auctionsRet = auctionsRet.Where(a => a.Price <= maxPrice).ToList();
            }
            
            auctionsRet = auctionsRet.OrderByDescending(a=> a.State=="OPEN").ThenByDescending(a => a.OpenTime).ToList();

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(auctionsRet.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}