using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using Tea_Billing_System.Data;
using Tea_Billing_System.Models;
using Razorpay.Api;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;
using System.Net;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Reflection.Metadata;

namespace Tea_Billing_System.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext db;

        public OrderController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // 📌 Show Product Cards with "Buy Now" Button
        public IActionResult CreateOrder()
        {
            var products = db.Products.ToList();
            return View(products);
        }

        // 📌 Initiate Payment via Razorpay
        public IActionResult InitiatePayment(string teaName, int quantity)
        {
            var product = db.Products.FirstOrDefault(p => p.Name == teaName);
            if (product == null || quantity <= 0 || product.Stock < quantity)
            {
                TempData["Error"] = "Invalid order details.";
                return RedirectToAction("CreateOrder");
            }

            decimal amount = quantity * product.Price;
            string userEmail = HttpContext.Session.GetString("UserEmail");

            var order = new Models.Order
            {
                UserEmail = userEmail,
                TeaName = teaName,
                Quantity = quantity,
                TotalPrice = amount,
                Status = "Pending"
            };

            db.Orders.Add(order);
            db.SaveChanges();

            // Initialize Razorpay
            var client = new RazorpayClient("rzp_test_Kl7588Yie2yJTV", "6dN9Nqs7M6HPFMlL45AhaTgp");

            var options = new Dictionary<string, object>
            {
                { "amount", amount * 100 },
                { "currency", "INR" },
                { "receipt", order.Id.ToString() },
                { "payment_capture", 1 }
            };

            var razorpayOrder = client.Order.Create(options);

            ViewBag.OrderId = razorpayOrder["id"].ToString();
            ViewBag.Amount = amount;
            ViewBag.TeaName = teaName;
            ViewBag.Quantity = quantity;

            return View();
        }

        // 📌 Handle Payment Success
        [HttpPost]
        public async Task<IActionResult> PaymentSuccess(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
        {
            var client = new RazorpayClient("rzp_test_Kl7588Yie2yJTV", "6dN9Nqs7M6HPFMlL45AhaTgp");

            Dictionary<string, string> attributes = new Dictionary<string, string>
    {
        { "razorpay_payment_id", razorpay_payment_id },
        { "razorpay_order_id", razorpay_order_id },
        { "razorpay_signature", razorpay_signature }
    };

            try
            {
                // Verify Razorpay payment signature
                Utils.verifyPaymentSignature(attributes);

                // Find the order
                var order = db.Orders.FirstOrDefault(o => o.RazorpayPaymentId == null && o.PaymentStatus == "Pending");
                if (order == null)
                {
                    ModelState.AddModelError("", "Order not found.");
                    return View("PaymentFailure");
                }

                // ✅ Deduct stock AFTER payment is confirmed
                var product = db.Products.FirstOrDefault(p => p.Name == order.TeaName);
                if (product != null && product.Stock >= order.Quantity)
                {
                    product.Stock -= order.Quantity;
                    db.Products.Update(product);
                }
                else
                {
                    ModelState.AddModelError("", "Stock not available.");
                    return View("PaymentFailure");
                }

                // ✅ Only update PaymentStatus (DO NOT change Order Status)
                order.PaymentStatus = "Success";
                order.RazorpayPaymentId = razorpay_payment_id;

                db.Orders.Update(order);
                db.SaveChanges();

                // ✅ Generate PDF Invoice & send email
                string pdfPath = GeneratePdf(order);
                SendEmailWithPdf(order.UserEmail, pdfPath);

                return RedirectToAction("UserOrders");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Payment verification failed: " + ex.Message);
                return View("UserOrders");
            }
        }



        // 📌 Generate PDF Bill
        private string GeneratePdf(Models.Order order)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Invoices");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string fileName = $"{order.Id}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();
                doc.Add(new Paragraph($"Order ID: {order.Id}"));
                doc.Add(new Paragraph($"Customer Email: {order.UserEmail}"));
                doc.Add(new Paragraph($"Tea Name: {order.TeaName}"));
                doc.Add(new Paragraph($"Quantity: {order.Quantity}"));
                doc.Add(new Paragraph($"Total Price: ₹{order.TotalPrice}"));
                doc.Add(new Paragraph($"Payment Status: {order.PaymentStatus}"));
                doc.Add(new Paragraph($"Order Date: {order.OrderDate}"));
                doc.Close();
                writer.Close();
            }
            return $"/Invoices/{fileName}";
        }

        // 📌 Send Email with PDF
        private void SendEmailWithPdf(string customerEmail, string pdfPath)
        {
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Invoices", Path.GetFileName(pdfPath));

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("sakshimsawant162@gmail.com");
            mail.To.Add(customerEmail);
            mail.Subject = "Your Tea Order Invoice";
            mail.Body = "Thank you for your order. Please find the attached invoice.";
            mail.Attachments.Add(new Attachment(absolutePath));

            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("sakshimsawant162@gmail.com", "czscembogqficlkq");
            smtp.Send(mail);
        }
        public IActionResult UserOrders()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account"); // Redirect if user is not logged in
            }

            var orders = db.Orders.Where(o => o.UserEmail == userEmail).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

    }
}
