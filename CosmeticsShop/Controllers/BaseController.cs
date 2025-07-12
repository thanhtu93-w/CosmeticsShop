using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsShop.Controllers
{
    public class BaseController : Controller
    {
        // GET: Default

        protected ShoppingEntities db = new ShoppingEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                ViewBag.ListCategory = db.Categories
                    .Where(x => x.IsActive == true)
                    .OrderBy(x => x.Name)
                    .ToList();
            }
            catch
            {
                ViewBag.ListCategory = new List<Category>();
            }

            base.OnActionExecuting(filterContext);
        }
    }
}