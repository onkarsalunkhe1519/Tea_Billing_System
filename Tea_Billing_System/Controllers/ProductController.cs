using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Tea_Billing_System.Data;
using Tea_Billing_System.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Tea_Billing_System.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            this.db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // 📌 Show all products
        public IActionResult Index()
        {
            var products = db.Products.ToList();
            return View(products);
        }

        // 📌 Add New Product (GET)
        public IActionResult AddProduct()
        {
            return View();
        }

        // 📌 Add New Product (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProduct(Product p, IFormFile imageFile)
        {
            
                if (imageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    string uniqueFileName = Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(fileStream);
                    }

                    p.ImageUrl = "/uploads/" + uniqueFileName;
                }

                db.Products.Add(p);
                db.SaveChanges();
                return RedirectToAction("Index");
            

           
        }

        // 📌 Delete Product
        public IActionResult Delete(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
