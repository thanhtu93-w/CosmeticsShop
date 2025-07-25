﻿using CosmeticsShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsShop.Controllers
{
    public class HomeController : BaseController
    {
        ShoppingEntities db = new ShoppingEntities();

        public ActionResult Index()
        {
            if (Session["Cart"] == null)
            {
                Session["Cart"] = new List<ItemCart>();
            }

            ViewBag.ListProduct = db.Products
                .Where(x => x.IsActive == true && x.PurchasedCount > 0)
                .OrderByDescending(x => x.PurchasedCount)
                .ToList();

          
            SetWishlistToViewBag();
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(Models.User user)
        {
            Models.User check = db.Users.SingleOrDefault(x => x.Email == user.Email);
            if (check != null)
            {
                ViewBag.Message = "Email đã tồn tại";
                return View();
            }

            Models.User userAdded = new Models.User();
            try
            {
                user.Captcha = new Random().Next(100000, 999999).ToString();
                user.IsConfirm = false;
                user.UserTypeID = 2;
                user.Address = "pr.jpg";
                userAdded = db.Users.Add(user);
                db.SaveChanges();
            }
            catch (Exception)
            {
                ViewBag.Message = "Đăng ký thất bại";
                return View();
            }
            return RedirectToAction("ConfirmEmail", "User", new { ID = userAdded.ID });
        }
        public ActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignIn(string Email, string Password)
        {
            var check = db.Users.Include("UserType")
                                .SingleOrDefault(x => x.Email == Email && x.Password == Password);

            if (check != null)
            {
                Session["User"] = check;

                if (check.UserType != null && check.UserType.Name.ToLower() == "admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Message = "Email hoặc mật khẩu không đúng";
            return View();
        }

        public ActionResult SignOut()
        {
            Session.Remove("User");
            return RedirectToAction("Index");
        }
    }
}