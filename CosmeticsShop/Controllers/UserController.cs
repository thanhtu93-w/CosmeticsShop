using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsShop.Controllers
{
    public class UserController : BaseController
    {
        ShoppingEntities db = new ShoppingEntities();
        public bool CheckRole(string type)
        {
            Models.User user = Session["User"] as Models.User;
            if (user.UserType.Name == type)
            {
                return true;
            }
            return false;
        }
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ConfirmEmail(int ID)
        {
            Models.User user = db.Users.SingleOrDefault(x => x.ID == ID);
            if (user.IsConfirm.Value)
            {
                ViewBag.Message = "EmailConfirmed";
                return View();
            }
            string urlBase = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");
            ViewBag.Email = "Truy cập vào Email để xác minh tài khoản: " + user.Email;
            SentMail("Mã xác minh tài khoản", user.Email, "taurenth@gmail.com", "gveywvjinnhifxfc", "Xác minh nhanh bằng cách click vào link: " + urlBase + "User/ConfirmEmailLink/" + ID + "?Captcha=" + user.Captcha + "</p>");
            return View();
        }
        [HttpGet]
        public ActionResult ConfirmEmailLink(int ID, string Captcha)
        {
            User user = db.Users.SingleOrDefault(x => x.ID == ID && x.Captcha == Captcha);
            if (user != null)
            {
                user.IsConfirm = true;
                db.SaveChanges();
                ViewBag.Message = "Xác minh tài khoản thành công";
                return View();
            }
            ViewBag.Message = "Mã xác minh tài khoản không đúng";
            return View();
        }

        public void SentMail(string title, string toEmail, string fromEmail, string password, string content)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Hệ thống xác minh tài khoản");
                mail.To.Add(toEmail);
                mail.Subject = title;
                mail.Body = content;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(fromEmail, password);
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Gửi mail thất bại: " + ex.Message);
            }
        }


        public ActionResult CheckoutOrder()
        {
            if (CheckRole("Client"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            Models.User user = Session["User"] as Models.User;
            List<Order> orders = db.Orders.Where(x => x.UserID == user.ID).ToList();
            return View(orders);
        }
        public ActionResult OrderDetails(int ID)
        {
            if (CheckRole("Client"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            List<OrderDetail> orderDetails = db.OrderDetails.Where(x => x.OrderID.Value == ID).ToList();
            return View(orderDetails);
        }
        public ActionResult Complete(int ID)
        {
            Order order = db.Orders.Find(ID);
            order.Status = "Complete";
            order.DateShip = DateTime.Now;
            db.SaveChanges();

            // Cập nhật sản phẩm
            List<OrderDetail> orderDetails = db.OrderDetails.Where(x => x.OrderID.Value == ID).ToList();
            foreach (var item in orderDetails)
            {
                Product product = db.Products.Find(item.ProductID);
                product.Quantity -= item.Quantity;
                product.PurchasedCount += item.Quantity;
                db.SaveChanges();
            }
            return RedirectToAction("CheckoutOrder");
        }
        [HttpPost]
        public JsonResult ToggleWishlist(int productId)
        {
            User user = Session["User"] as User;
            if (user == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var item = db.Wishlists.FirstOrDefault(w => w.UserID == user.ID && w.ProductID == productId);
            if (item != null)
            {
                db.Wishlists.Remove(item); 
                db.SaveChanges();
                return Json(new { success = true, status = "removed" });
            }
            else
            {
                db.Wishlists.Add(new Wishlist { UserID = user.ID, ProductID = productId, CreatedDate = DateTime.Now });
                db.SaveChanges();
                return Json(new { success = true, status = "added" });
            }
        }
        public ActionResult Wishlist()
        {
            var products = db.Products.Where(p => p.IsActive == true).ToList();

            var user = Session["User"] as User;
            if (user != null)
            {
                ViewBag.Wishlist = db.Wishlists.Where(w => w.UserID == user.ID).Select(w => w.ProductID).ToList();
            }

            return View(products);
        }
    }
}