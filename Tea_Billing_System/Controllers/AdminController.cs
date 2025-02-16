using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Tea_Billing_System.Data;
using Tea_Billing_System.Models;
using System;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text;

namespace Tea_Billing_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;

        public AdminController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // 📌 View All Orders
        public IActionResult Orders(string filterStatus, string filterDate)
        {
            var orders = db.Orders.OrderByDescending(o => o.OrderDate).ToList();

            // Filter by Status
            if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "All")
            {
                orders = orders.Where(o => o.Status == filterStatus).ToList();
            }

            // Filter by Date
            if (!string.IsNullOrEmpty(filterDate))
            {
                if (DateTime.TryParse(filterDate, out DateTime selectedDate))
                {
                    orders = orders.Where(o => o.OrderDate.Date == selectedDate.Date).ToList();
                }
            }

            return View(orders);
        }

        // 📌 Update Order Status (Admin can update status)
        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = status;
                db.SaveChanges();
                TempData["Success"] = "Order status updated successfully!";
            }
            else
            {
                TempData["Error"] = "Order not found.";
            }
            return RedirectToAction("Orders");
        }
        public IActionResult Dashboard()
        {
            var totalRevenue = db.Orders.Where(o => o.PaymentStatus == "Success").Sum(o => o.TotalPrice);
            var totalOrders = db.Orders.Count();
            var pendingOrders = db.Orders.Count(o => o.Status == "Pending");
            var deliveredOrders = db.Orders.Count(o => o.Status == "Delivered");

            var topTeas = db.Orders
                .GroupBy(o => o.TeaName)
                .Select(g => new { TeaName = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.DeliveredOrders = deliveredOrders;
            ViewBag.TopTeas = topTeas;

            return View();
        }
        public IActionResult ExportOrdersToCSV()
        {
            var orders = db.Orders.ToList();

            var builder = new StringBuilder();
            builder.AppendLine("Order ID,Customer Email,Tea Name,Quantity,Total Price,Payment Status,Order Status,Order Date");

            foreach (var order in orders)
            {
                builder.AppendLine($"{order.Id},{order.UserEmail},{order.TeaName},{order.Quantity},₹{order.TotalPrice},{order.PaymentStatus},{order.Status},{order.OrderDate}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "OrderReport.csv");
        }

        // 📌 Export Orders to PDF
        public IActionResult ExportOrdersToPDF()
        {
            var orders = db.Orders.ToList();
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, "OrderReport.pdf");

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                doc.Add(new Paragraph("Tea Billing System - Order Report"));
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(7);
                table.AddCell("Order ID");
                table.AddCell("Customer Email");
                table.AddCell("Tea Name");
                table.AddCell("Quantity");
                table.AddCell("Total Price");
                table.AddCell("Payment Status");
                table.AddCell("Order Status");

                foreach (var order in orders)
                {
                    table.AddCell(order.Id.ToString());
                    table.AddCell(order.UserEmail);
                    table.AddCell(order.TeaName);
                    table.AddCell(order.Quantity.ToString());
                    table.AddCell("₹" + order.TotalPrice);
                    table.AddCell(order.PaymentStatus);
                    table.AddCell(order.Status);
                }

                doc.Add(table);
                doc.Close();
                writer.Close();
            }

            return File(System.IO.File.ReadAllBytes(filePath), "application/pdf", "OrderReport.pdf");
        }
        public IActionResult AllReviews(string teaName)
        {
            var reviews = db.Reviews.OrderByDescending(r => r.ReviewDate).ToList();

            if (!string.IsNullOrEmpty(teaName))
            {
                reviews = reviews.Where(r => r.TeaName == teaName).ToList();
            }

            ViewBag.TeaList = db.Products.Select(p => p.Name).Distinct().ToList();
            return View(reviews);
        }

        

    }
}
