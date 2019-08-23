using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using belt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace belt.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet("")]        
        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }
        // [HttpGet("/user/new")]
        // public IActionResult NewUser()
        // {
        //     return View("NewUser");
        // }
        [HttpPost("user/create")]
        public IActionResult CreateUser(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(!dbContext.Users.Any(u => u.Username == newUser.Username))
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                    newUser.Wallet = 1000;
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();
                    User userInDb = dbContext.Users.FirstOrDefault(u => u.Username == newUser.Username);
                    HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("Username", "Password is already in use!");
                }
            }
            return View("Index");
        }
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Username == userSubmission.LoginUsername);
                if( userInDb != null)
                {
                    var hasher = new PasswordHasher<LoginUser>();
                    var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.LoginPassword);
                    if(result != 0)
                    {
                        //log in user
                        HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                        return RedirectToAction("Dashboard");
                    }
                }
                ModelState.AddModelError("LoginUsername", "Invalid Username/Password");
            }
            return View("Index");
        }
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Name = dbContext.Users.FirstOrDefault(u => u.UserId == userId).FirstName;
            ViewBag.UserId = dbContext.Users.FirstOrDefault(u => u.UserId == userId).UserId;
            ViewBag.Wallet = dbContext.Users.FirstOrDefault(u => u.UserId == userId).Wallet;
            List<Auction> AllAuctions = dbContext.Auctions.Include(a => a.Creator).Include( a => a.HighBidder).OrderBy(a => a.Date).ToList();
            foreach(Auction auction in AllAuctions)
            {
                if(auction.Date < DateTime.Now)
                {
                    //Auction has expired and money needs to be transferred and auction removed
                    User seller = auction.Creator;
                    User buyer = auction.HighBidder;
                    seller.Wallet += auction.HighBid;
                    buyer.Wallet -= auction.HighBid;
                    dbContext.Auctions.Remove(auction);
                }
            }
            return View("Dashboard", AllAuctions);
        }
        [HttpGet("auction/new")]
        public IActionResult NewAuction()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Name = dbContext.Users.FirstOrDefault(u => u.UserId == userId).FirstName;
            ViewBag.UserId = dbContext.Users.FirstOrDefault(u => u.UserId == userId).UserId;
            return View("NewAuction");
        }
        [HttpPost("auction/create")]
        public IActionResult CreateAuction(Auction newAuction)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            if(ModelState.IsValid)
            {
                dbContext.Auctions.Add(newAuction);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            ViewBag.Name = dbContext.Users.FirstOrDefault(u => u.UserId == userId).FirstName;
            ViewBag.UserId = dbContext.Users.FirstOrDefault(u => u.UserId == userId).UserId;

            return View("NewAuction");
        }

        [HttpGet("auction/details/{AuctionId}")]
        public IActionResult details(int AuctionId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            Auction auctionToView = dbContext.Auctions.Include(a => a.Creator)
            .Include(a => a.HighBidder).FirstOrDefault(a => a.AuctionId == AuctionId);
            ViewBag.Error = "";
            return View("Details", auctionToView);
        }
        [HttpPost("auction/bid")]
        public IActionResult Bid(double Amount, int AuctionId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            
            double highBid = dbContext.Auctions.Include(a => a.Creator).Include(a => a.HighBidder).FirstOrDefault(a => a.AuctionId == AuctionId).HighBid;
            double wallet = dbContext.Users.FirstOrDefault(u => u.UserId == userId).Wallet;
            if( Amount > wallet || Amount < highBid)
            {
                Auction auctionToView = dbContext.Auctions.Include(a => a.Creator)
                .Include(a => a.HighBidder).FirstOrDefault(a => a.AuctionId == AuctionId);
                ViewBag.Error = "Please enter an amount greater than the minium bid but less than the amount in your wallet";
                return View("Details", auctionToView);
            }
            else
            {
                // update highest bid
                // Add to DB
                // RSVP myRSVP = new RSVP();
                // myRSVP.UserId = (int)HttpContext.Session.GetInt32("UserId");
                // myRSVP.WeddingId = weddingId;
                // dbContext.RSVPs.Add(myRSVP);
                // dbContext.SaveChanges();
                User highestBidder = dbContext.Users.FirstOrDefault(u => u.UserId == (int)userId);
                highestBidder.Wallet -= Amount;
                Auction auctionToUpdate = dbContext.Auctions.FirstOrDefault(a => a.AuctionId == AuctionId);
                auctionToUpdate.HighBidderId = (int)userId;
                auctionToUpdate.HighBid = Amount;
                dbContext.SaveChanges();

            }
            return RedirectToAction("Dashboard");
        }
        [HttpGet("auction/delete/{auctionId}")]
        public IActionResult Delete(int auctionId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            Auction auctionToDelete = dbContext.Auctions.FirstOrDefault(w => w.AuctionId == auctionId);
            if(auctionToDelete.CreatorId == userId)
            {
                dbContext.Auctions.Remove(auctionToDelete);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
