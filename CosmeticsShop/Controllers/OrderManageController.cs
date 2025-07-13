using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace CosmeticsShop.Controllers
{
    public class OrderManageController : Controller
    {
        ShoppingEntities db = new ShoppingEntities();
        public bool CheckRole(string type)
        {
            Models.User user = Session["User"] as Models.User;
            if (user != null && user.UserType.Name == type)
            {
                return true;
            }
            return false;
        }
        // GET: OrderManage
        public ActionResult Index()
        {
            if (CheckRole("Admin"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }

            List<Order> orders = db.Orders.Include("User").ToList();
            return View(orders);
        }
        public ActionResult Details(int? ID)
        {
            if (!CheckRole("Admin"))
                return RedirectToAction("Index", "Admin");

            if (ID == null)
                return RedirectToAction("SignIn", "Home"); // Chuyển về trang đăng nhập nếu ID null

            var order = db.Orders.Find(ID.Value);
            if (order == null)
                return HttpNotFound(); // Trường hợp không tìm thấy đơn hàng

            ViewBag.IsProcessed = order.Status != "Processing";

            List<OrderDetail> orderDetails = db.OrderDetails
                .Where(x => x.OrderID == ID.Value)
                .ToList();

            return View(orderDetails);
        }

        public ActionResult Processed(int ID)
        {
            Order order = db.Orders.Find(ID);
            order.Status = "Processed";
            order.DateShip = DateTime.Now.AddDays(3);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Delivering(int ID)
        {
            Order order = db.Orders.Find(ID);
            order.Status = "Delivering";
            order.DateShip = DateTime.Now.AddDays(3);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Delete(int ID)
        {
            if (!CheckRole("Admin")) return RedirectToAction("Index", "Admin");

            var order = db.Orders.Find(ID);
            if (order != null)
            {
                db.OrderDetails.RemoveRange(db.OrderDetails.Where(x => x.OrderID == ID));
                db.Orders.Remove(order);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public ActionResult Complete(int ID)
        {
            if (!CheckRole("Admin")) return RedirectToAction("Index", "Admin");

            var order = db.Orders.Find(ID);
            if (order != null)
            {
                order.Status = "Complete";
                order.DateShip = DateTime.Now;

                if (order.IsPaid == false || order.IsPaid == null)
                {
                    order.IsPaid = true;
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

    }
}