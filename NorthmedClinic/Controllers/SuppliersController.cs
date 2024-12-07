using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using NorthmedClinic.Models;
using System.Net;

namespace NorthmedClinic.Controllers
{
    [Authorize(Roles = "Supplier")]
    public class SuppliersController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Suppliers
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var inventory = _context.Inventories.Where(i => i.SupplierId == userId).ToList();
            return View(inventory);
        }

        // GET: Suppliers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            Inventory inventory = _context.Inventories.FirstOrDefault(i => i.Id == id && i.SupplierId == userId);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // GET: Suppliers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ItemName,Dosage,Quantity,ExpirationDate,Price")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                inventory.SupplierId = User.Identity.GetUserId();
                _context.Inventories.Add(inventory);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(inventory);
        }


        // GET: Suppliers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            Inventory inventory = _context.Inventories.FirstOrDefault(i => i.Id == id && i.SupplierId == userId);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ItemName,Dosage,Quantity,ExpirationDate,Price")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                inventory.SupplierId = userId; // Ensure the SupplierId is set for the logged-in user
                _context.Entry(inventory).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(inventory);
        }

        // GET: Suppliers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userId = User.Identity.GetUserId();
            Inventory inventory = _context.Inventories.FirstOrDefault(i => i.Id == id && i.SupplierId == userId);
            if (inventory == null)
            {
                return HttpNotFound();
            }
            return View(inventory);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            Inventory inventory = _context.Inventories.FirstOrDefault(i => i.Id == id && i.SupplierId == userId);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var inventory = _context.Inventories.Where(i => i.SupplierId == userId).ToList();

            // Retrieve supplier name from the AspNetUsers table
            var supplierName = _context.Users.Where(u => u.Id == userId).Select(u => u.SupplierName).FirstOrDefault(); // Replace UserName with the appropriate column for the supplier's name if needed

            // Pass data to the view through ViewBag
            ViewBag.SupplierName = supplierName;
            ViewBag.ItemNames = inventory.Select(i => i.ItemName).ToList();
            ViewBag.Quantities = inventory.Select(i => i.Quantity).ToList();

            return View(inventory);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }


        //see orders
        // GET: Supplier/SeeOrders
        public ActionResult SeeOrders()
        {
            // Get the logged-in supplier’s ID
            var supplierId = User.Identity.GetUserId();

            // Fetch all OrderItemDetails assigned to this supplier, excluding canceled orders
            var orders = _context.OrderItemDetails
                .Include(o => o.OrderItem.Order) // Ensure you include related Order for filtering
                .Where(o => o.SupplierId == supplierId &&
                            o.OrderItem.Order.Status != "Canceled") // Exclude canceled orders
                .ToList();

            return View(orders);
        }



        // POST: Supplier/UpdateStatus
        [HttpPost]
        public ActionResult UpdateStatus(int[] itemIds, string status)
        {
            // Step 1: Update the status of each OrderItemDetail
            var itemsToUpdate = _context.OrderItemDetails
                .Where(o => itemIds.Contains(o.Id))
                .ToList();

            foreach (var item in itemsToUpdate)
            {
                item.Status = status;
            }

            _context.SaveChanges();

            // Step 2: Check and update the status of each OrderItem if all details are ready
            var orderItemIds = itemsToUpdate.Select(i => i.OrderItemId).Distinct().ToList();

            var orderItems = _context.OrderItems
                .Where(oi => orderItemIds.Contains(oi.Id))
                .ToList();

            foreach (var orderItem in orderItems)
            {
                // If all OrderItemDetails for this OrderItem are "Ready", set OrderItem status to "Ready"
                if (orderItem.OrderItemDetails.All(detail => detail.Status == "Ready"))
                {
                    orderItem.Status = "Ready";
                }
            }

            _context.SaveChanges();

            // Step 3: Check and update the status of each Order if all OrderItems are ready, 
            // but only if the order has not been verified yet
            var orderIds = orderItems.Select(oi => oi.OrderId).Distinct().ToList();

            var orders = _context.Orders
                .Where(o => orderIds.Contains(o.Id) && !o.IsVerified) // Only update non-verified orders
                .ToList();

            foreach (var order in orders)
            {
                // If all OrderItems for this Order are "Ready", set Order status to "Ready"
                if (order.OrderItems.All(oi => oi.Status == "Ready"))
                {
                    order.Status = "Ready";
                }
            }

            _context.SaveChanges();

            return RedirectToAction("SeeOrders");
        }




    }
}