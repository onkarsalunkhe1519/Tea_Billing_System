using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Tea_Billing_System.Data;
using Tea_Billing_System.Models;
using System;

namespace Tea_Billing_System.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext db;

        public ReviewController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // 📌 View Reviews for a Tea Product
        public IActionResult ProductReviews(string teaName)
        {
            var reviews = db.Reviews.Where(r => r.TeaName == teaName).OrderByDescending(r => r.ReviewDate).ToList();
            ViewBag.TeaName = teaName;
            return View(reviews);
        }

        // 📌 Add a New Review (GET)
        public IActionResult AddReview(string teaName)
        {
            ViewBag.TeaName = teaName;
            return View();
        }

        // 📌 Add a New Review (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(Review review)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToAction("Login", "Account"); // Redirect if not logged in
            }

            review.UserEmail = HttpContext.Session.GetString("UserEmail");

            
                db.Reviews.Add(review);
                db.SaveChanges();
                return RedirectToAction("ProductReviews", new { teaName = review.TeaName });
            
        }

        // 📌 Delete Review (Admin Only)
        public IActionResult DeleteReview(int id)
        {
            var review = db.Reviews.Find(id);
            if (review != null)
            {
                db.Reviews.Remove(review);
                db.SaveChanges();
                TempData["Success"] = "Review deleted successfully!";
            }
            return RedirectToAction("ProductReviews", new { teaName = review.TeaName });
        }
    }
}
