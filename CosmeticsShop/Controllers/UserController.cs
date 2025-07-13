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
            if (user == null || user.UserType == null || string.IsNullOrEmpty(user.UserType.Name))
            {
                return false;
            }
            return user.UserType.Name == type;
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
            if (!CheckRole("Client"))
            {
                return RedirectToAction("Index", "Home");
            }

            Models.User user = Session["User"] as Models.User;
            List<Order> orders = db.Orders.Where(x => x.UserID == user.ID).ToList();

            var completedOrderIds = orders.Where(o => o.Status == "Complete").Select(o => o.ID).ToList();
            var reviewedOrderMap = new Dictionary<int, bool>();

            foreach (var orderId in completedOrderIds)
            {
                var productIds = db.OrderDetails
                   .Where(od => od.OrderID == orderId)
                   .Select(od => od.ProductID.Value) 
                   .ToList();

                var reviewedProductIds = db.ProductReviews
                                           .Where(r => r.UserID == user.ID && productIds.Contains(r.ProductID))
                                           .Select(r => r.ProductID)
                                           .Distinct()
                                           .ToList();

                bool allReviewed = productIds.All(pid => reviewedProductIds.Contains(pid));
                reviewedOrderMap[orderId] = allReviewed;
            }

            ViewBag.ReviewedOrderMap = reviewedOrderMap;

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
            var user = Session["User"] as User;
            if (user == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var item = db.Wishlists.FirstOrDefault(w => w.UserID == user.ID && w.ProductID == productId);
            if (item != null)
            {
                db.Wishlists.Remove(item);
                var result = db.SaveChanges();
                return Json(new { success = result > 0, status = "removed" });
            }
            else
            {
                db.Wishlists.Add(new Wishlist
                {
                    UserID = user.ID,
                    ProductID = productId,
                    CreatedDate = DateTime.Now
                });
                var result = db.SaveChanges();
                return Json(new { success = result > 0, status = "added" });
            }
        }

        public ActionResult Favorites()
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            var wishlistProductIDs = db.Wishlists
                .Where(w => w.UserID == user.ID)
                .Select(w => w.ProductID)
                .ToList();

            var products = db.Products
                .Where(p => wishlistProductIDs.Contains(p.ID))
                .ToList();

            ViewBag.WishlistProductIDs = wishlistProductIDs;
            ViewBag.ListProduct = products;
            ViewBag.NamePage = "Sản phẩm yêu thích";

            return View(products); 
        }
        public ActionResult Profile()
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            var userInfo = db.Users.SingleOrDefault(x => x.ID == user.ID);
            return View(userInfo);
        }

        [HttpPost]
        public ActionResult Profile(User updatedUser, HttpPostedFileBase AvatarFile)
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            var userInDb = db.Users.SingleOrDefault(x => x.ID == user.ID);
            if (userInDb != null)
            {
                userInDb.Name = updatedUser.Name;
                userInDb.Phone = updatedUser.Phone;
                userInDb.Address = updatedUser.Address;

                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(AvatarFile.FileName);
                    string path = Server.MapPath("~/Content/images/" + fileName);
                    AvatarFile.SaveAs(path);
                    userInDb.Avatar = fileName;
                }

                db.SaveChanges();
                Session["User"] = userInDb;
                ViewBag.Message = "Cập nhật thành công";
            }

            return View(userInDb);
        }
        public ActionResult ReviewOrder(int? orderId)
        {
            if (!CheckRole("Client"))
            {
                return RedirectToAction("SignIn", "Home");
            }

            if (orderId == null)
            {
                return RedirectToAction("CheckoutOrder", "User");
            }

            var user = Session["User"] as User;
            if (user == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            var order = db.Orders.FirstOrDefault(o => o.ID == orderId.Value && o.UserID == user.ID && o.Status == "Complete");
            if (order == null)
            {
                return HttpNotFound();
            }

            var orderDetails = db.OrderDetails
                                 .Include("Product")
                                 .Where(od => od.OrderID == orderId.Value)
                                 .GroupBy(od => od.ProductID)
                                 .Select(g => g.FirstOrDefault())
                                 .ToList();

            var productReviews = db.ProductReviews
                                   .Where(r => r.OrderID == orderId.Value && r.UserID == user.ID)
                                   .ToList();

            var reviewDict = productReviews.ToDictionary(r => r.ProductID, r => r);

            ViewBag.OrderID = orderId.Value;
            ViewBag.ProductReviews = reviewDict;

            return View(orderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReviewSingle(int OrderID, int ProductID, int Rating, string Comment)
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            var existingReview = db.ProductReviews.FirstOrDefault(r =>
     r.UserID == user.ID && r.ProductID == ProductID && r.OrderID == OrderID);
            if (existingReview == null)
            {
                var review = new ProductReview
                {
                    ProductID = ProductID,
                    UserID = user.ID,
                    OrderID = OrderID, // thêm nếu bạn đang dùng OrderID để truy xuất
                    Rating = Rating,
                    Comment = Comment,
                    CreatedDate = DateTime.Now,
                };
                db.ProductReviews.Add(review);
            }
            else
            {
                existingReview.Rating = Rating;
                existingReview.Comment = Comment;
                existingReview.CreatedDate = DateTime.Now;
            }
            db.SaveChanges();
            TempData["Message"] = "Đánh giá sản phẩm thành công!";

            return RedirectToAction("ReviewOrder", new { orderId = OrderID });
        }
        [HttpPost]
        public ActionResult ResendVerification()
        {
            var user = Session["User"] as User;
            if (user == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            var dbUser = db.Users.SingleOrDefault(u => u.ID == user.ID);
            if (dbUser == null || dbUser.IsConfirm == true)
            {
                TempData["Message"] = "Tài khoản đã xác minh hoặc không tồn tại.";
                return RedirectToAction("Profile");
            }

            string urlBase = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");
            string confirmUrl = urlBase + "User/ConfirmEmailLink/" + dbUser.ID + "?Captcha=" + dbUser.Captcha;

            string content = $"<p>Vui lòng xác minh tài khoản của bạn bằng cách nhấn vào link sau:<br/><a href='{confirmUrl}'>{confirmUrl}</a></p>";

            SentMail("Mã xác minh tài khoản", dbUser.Email, "taurenth@gmail.com", "gveywvjinnhifxfc", content);
            TempData["Message"] = "Email xác minh đã được gửi lại. Vui lòng kiểm tra hộp thư.";

            return RedirectToAction("Profile");
        }


    }
}