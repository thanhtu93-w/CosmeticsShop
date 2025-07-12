using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsShop.Controllers
{
    public class AdminController : Controller
    {
        ShoppingEntities db = new ShoppingEntities();
        // GET: Admin
        public ActionResult Index()
        {
            if (CheckRole("Admin"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            var completedOrders = db.Orders.Where(o => o.Status == "Complete").ToList();

            ViewBag.TotalOrder = db.Orders.Count();
            ViewBag.PendingOrder = db.Orders.Count(o => o.Status != "Complete");
            ViewBag.CompletedOrder = db.Orders.Count(o => o.Status == "Complete");

            ViewBag.TotalMoney = completedOrders
                .Join(db.OrderDetails,
                      o => o.ID,
                      d => d.OrderID,
                      (o, d) => d.ProductPrice * d.Quantity)
                .Sum();

            ViewBag.TotalSoldProducts = completedOrders
                .Join(db.OrderDetails,
                      o => o.ID,
                      d => d.OrderID,
                      (o, d) => d.Quantity)
                .Sum();

            ViewBag.TotalClient = db.Users.Count(u => u.UserTypeID == 2);
            ViewBag.VerifiedClient = db.Users.Count(u => u.UserTypeID == 2 && u.IsConfirm == true);
            ViewBag.UnverifiedClient = db.Users.Count(u => u.UserTypeID == 2 && u.IsConfirm == false);

            ViewBag.TotalProduct = db.Products.Count();

            return View();
        }
        public bool CheckRole(string type)
        {
            Models.User user = Session["User"] as Models.User;
            if (user != null && user.UserType.Name == type)
            {
                return true;
            }
            return false;
        }
    }
}