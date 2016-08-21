using IEP_Projekat.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System;
using IEP_Projekat.Controllers;

[assembly: OwinStartupAttribute(typeof(IEP_Projekat.Startup))]
namespace IEP_Projekat
{
    
    public partial class Startup
    {
        public static Dictionary<int,AuctionMemoryModel> auctionTable = new Dictionary<int, AuctionMemoryModel>();
        
        public static List<int> toDelete = new List<int>();

        public static List<AuctionMemoryModel> toSend = new List<AuctionMemoryModel>();

        public static readonly object objlock = new object();
        public static readonly object writelock = new object();
        public static Model3 db = new Model3();

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateAdmin();
            fillAuctionTable();
            /*Task.Factory.StartNew(() =>
            {
                RefreshAuctionTime();
            });*/
            new Thread(RefreshAuctionTime).Start();
    }

        private void CreateAdmin()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Admin"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                var user = new ApplicationUser();
                user.UserName = "adminadmin";
                user.FullName = "Perica";
                user.Email = "admin@admin.com";

                string userPass = "RootBeer!123";

                var chkUser = UserManager.Create(user, userPass);
   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");
                }
            }
        }

        private static void fillAuctionTable()
        {
            foreach (Auction a in db.Auctions.Where(a => a.State=="OPEN").ToList()) {
                //if (a.State == "OPEN") { 
                string username = null;
                if (a.LastBidderId != null)
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                    var user = context.Users.Where(usr => usr.Id == a.LastBidderId).First();
                    //HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(usr => usr.Id == a.LastBidderId).First();
                    if(user!=null) username = user.UserName;
                }
                auctionTable.Add(a.Id, new AuctionMemoryModel()
                {
                    Id = a.Id,
                    CloseTime = a.CloseTime,
                    CreateTime = a.CreateTime,
                    Duration = a.Duration,
                    Name = a.Name,
                    OpenTime = a.OpenTime,
                    Price = a.StartPrice,
                    State = a.State,
                    LastBidderUserName = username
                });
                //}
            }
        }

        private static void RefreshAuctionTime()
        {
            while (true)
            {
                lock (writelock) { 
                        foreach(KeyValuePair<int, AuctionMemoryModel> pair in auctionTable)
                            {
                            lock (objlock)
                            {
                            Debug.WriteLine(pair.Key + " - " + pair.Value.Name + " - " + pair.Value.Duration);
                            var auction = db.Auctions.Find(pair.Key);
                            if (auction != null)
                            {
                                auction.ImageToUpload = new AuctionsController.MyHttpPostedFileBase();
                                if(auction.Duration>0 || pair.Value.Duration > 0)
                                {
                                    auction.Duration--;
                                    pair.Value.Duration--;
                                    if (pair.Value.Duration == 0 || auction.Duration==0)
                                    {
                                        pair.Value.Duration = 0;
                                        auction.Duration = 0;
                                        ApplicationDbContext context = new ApplicationDbContext();
                                        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                                        var user = context.Users.Where(usr => usr.Id == auction.LastBidderId).First();
                                        if (pair.Value.LastBidderUserName != null && user.TokenNum >= pair.Value.Price)
                                        {
                                            user.TokenNum -= pair.Value.Price;
                                            auction.State = "SOLD";
                                            UserManager.Update(user);
                                        }
                                        else auction.State = "EXPIRED";
                                        auction.CloseTime = DateTime.Now;
                                        toDelete.Add(pair.Key);
                                        toSend.Add(pair.Value);
                                        //db.SaveChanges();
                                    }
                                }
                                
                            }        
                        }
                    }
                    foreach (int key in toDelete) auctionTable.Remove(key);
                    db.SaveChanges();
                        
                }
                Thread.Sleep(1000);
            }
        }
    }
}
