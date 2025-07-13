using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsShop.Controllers
{
    public class ProductController : Controller
    {
        ShoppingEntities db = new ShoppingEntities();
        // GET: Product
        public ActionResult Index(int CategoryID = 0, string keyword = "")
        {
            ViewBag.ListCategory = db.Categories.Where(x => x.IsActive == true).ToList();
            if (keyword != "")
            {
                ViewBag.NamePage = "Tìm kiếm sản phẩm";
                ViewBag.ListProduct = db.Products.Where(x => x.IsActive == true && x.Name.Contains(keyword)).ToList();
                return View();
            }
            if (CategoryID != 0)
            {
                ViewBag.NamePage = "Danh mục " + db.Categories.Find(CategoryID).Name;
                ViewBag.ListProduct = db.Products.Where(x => x.IsActive == true && x.CategoryID == CategoryID).ToList();
            }
            else
            {
                ViewBag.NamePage = "Tất cả sản phẩm";
                ViewBag.ListProduct = db.Products.Where(x => x.IsActive == true).ToList();
            }
            return View();
        }
        public ActionResult Details(int ID)
        {
            var product = db.Products.Find(ID);
            if (product == null) return HttpNotFound();

            var reviews = db.ProductReviews.Where(r => r.ProductID == ID).ToList();
            ViewBag.Reviews = reviews;
            ViewBag.ListCategory = db.Categories.Where(x => x.IsActive == true).ToList();

            double avgRating = reviews.Count > 0 ? reviews.Average(r => r.Rating ?? 0) : 0;
            ViewBag.AvgRating = avgRating;
            ViewBag.TotalReview = reviews.Count;

            return View(product);
        }

    }
}