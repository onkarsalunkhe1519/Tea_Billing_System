using Microsoft.AspNetCore.Mvc;
using Tea_Billing_System.Data;
using Tea_Billing_System.Models;

namespace Tea_Billing_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext db;
        public AccountController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Account()
        {
            return View();
        }




        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User u)
        {
            if (ModelState.IsValid)
            {
                db.User.Add(u);
                db.SaveChanges();
                return RedirectToAction("Account");
            }
            return View(u);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User u)
        {
            // Remove validation errors for properties not used in login
            ModelState.Remove("Username");
            ModelState.Remove("Role");

            if (ModelState.IsValid)
            {
                var user = db.User.FirstOrDefault(x => x.Email == u.Email && x.Password == u.Password);
                if (user != null)
                {
                    // Set session variables
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    // Redirect based on role
                   
                    if (user.Role == "User")
                    {
                        return RedirectToAction("CreateOrder", "Order");
                    }
                    else if (user.Role == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }

                    return RedirectToAction("Index", "Home");  // Default fallback
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }
            return View(u);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();  // Clear all session data
            return RedirectToAction("Index", "Account");  // Redirect to login page
        }

    }
}
