using NorthmedClinic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using static System.Data.Entity.Infrastructure.Design.Executor;
using QRCoder;
using System.Net.Mail;
using System.IO;



namespace NorthmedClinic.Controllers
{
    [Authorize(Roles = "Pharmacist")]
    public class PharmacistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PharmacistController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Pharmacist/Dashboard
        public ActionResult Dashboard()
        {
            var stockItems = _context.PharmacistStocks.ToList();

            // Pass data to the view through ViewBag for the graph
            ViewBag.ItemNames = stockItems.Select(s => s.ItemName).ToList();
            ViewBag.Quantities = stockItems.Select(s => s.QuantityAvailable).ToList();

            return View(stockItems); // Pass stock items to the view for the table and graph
        }

        // Controller
        public ActionResult ManageStock()
        {
            var pharmacistStocks = _context.PharmacistStocks.ToList();

            // Aggregate by ItemName and sum the quantities for the chart
            var groupedData = pharmacistStocks
                .GroupBy(item => item.ItemName)
                .Select(g => new
                {
                    ItemName = g.Key,
                    TotalQuantity = g.Sum(item => item.QuantityAvailable)
                })
                .ToList();

            // Pass the aggregated data to the view using ViewBag
            ViewBag.ItemNames = groupedData.Select(g => g.ItemName).ToList();
            ViewBag.Quantities = groupedData.Select(g => g.TotalQuantity).ToList();

            // Return the ungrouped pharmacist stocks for display in the table
            return View(pharmacistStocks);
        }




        // GET: Pharmacist/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Pharmacist/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ItemName,Dosage,QuantityAvailable,ExpirationDate")] PharmacistStock stock)
        {
            if (ModelState.IsValid)
            {
                // Generate a unique batch code specific to the item name
                stock.BatchCode = GenerateUniqueBatchCode(stock.ItemName);

                // Set the last updated date to the current date and time
                stock.LastUpdatedDate = DateTime.Now;

                // Add the new stock item as a unique batch to the database
                _context.PharmacistStocks.Add(stock);
                _context.SaveChanges();

                return RedirectToAction("ManageStock");
            }
            return View(stock);
        }





        // GET: Pharmacist/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var stock = _context.PharmacistStocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }

            return View(stock);
        }

        // POST: Pharmacist/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var stock = _context.PharmacistStocks.Find(id);
            if (stock != null)
            {
                _context.PharmacistStocks.Remove(stock);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageStock");
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }


        //restock

        //public ActionResult Restock()
        //{
        //    // Retrieve all stock items, marking items with QuantityAvailable ≤ 10 as auto-checked
        //    var stockItems = _context.PharmacistStocks.ToList();
        //    ViewBag.LowStockItems = stockItems.Where(s => s.QuantityAvailable <= 10).Select(s => s.Id).ToList();

        //    return View(stockItems); // Pass all stock items to the view
        //}

        public ActionResult Restock()
        {
            // Retrieve all stock items from the database first
            var stockItems = _context.PharmacistStocks.ToList();

            // Perform the grouping and aggregation in memory
            var groupedStockItems = stockItems
                .GroupBy(s => s.ItemName)
                .Select(g => new PharmacistStock
                {
                    ItemName = g.Key,
                    QuantityAvailable = g.Sum(i => i.QuantityAvailable),
                    Id = g.First().Id // Use the Id from the first item in the group for reference
                })
                .ToList();

            ViewBag.LowStockItems = groupedStockItems
                .Where(s => s.QuantityAvailable <= 10)
                .Select(s => s.Id)
                .ToList();

            return View(groupedStockItems); // Pass grouped items to the view
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Restock(List<int> selectedItems, List<int> quantities)
        //{
        //    // Validate form submission
        //    if (selectedItems == null || quantities == null || selectedItems.Count != quantities.Count)
        //    {
        //        ModelState.AddModelError("", "Please select at least one item and specify valid quantities for each selected item.");
        //        return View("Restock", _context.PharmacistStocks.ToList()); // Return to view with error
        //    }

        //    // Validate each quantity
        //    bool hasInvalidQuantities = false;
        //    for (int i = 0; i < quantities.Count; i++)
        //    {
        //        if (quantities[i] <= 0)
        //        {
        //            ModelState.AddModelError("", $"Quantity for selected item {selectedItems[i]} must be greater than zero.");
        //            hasInvalidQuantities = true;
        //        }
        //    }

        //    // Return to view if there are invalid quantities
        //    if (hasInvalidQuantities)
        //    {
        //        return View("Restock", _context.PharmacistStocks.ToList());
        //    }

        //    // Create a new order entry
        //    var newOrder = new Order
        //    {
        //        OrderDate = DateTime.Now,
        //        Status = "Pending",
        //        OrderItems = new List<OrderItem>() // Initialize the OrderItems collection
        //    };

        //    // Process valid selected items and quantities
        //    for (int i = 0; i < selectedItems.Count; i++)
        //    {
        //        int itemId = selectedItems[i];
        //        int quantity = quantities[i];

        //        var stockItem = _context.PharmacistStocks.Find(itemId);

        //        // Create a new OrderItem and add it to the OrderItems collection
        //        var orderItem = new OrderItem
        //        {
        //            ItemName = stockItem.ItemName,
        //            QuantityOrdered = quantity
        //        };

        //        newOrder.OrderItems.Add(orderItem); // Add item to the order's items list
        //    }

        //    // Save the order with items to the database
        //    _context.Orders.Add(newOrder);
        //    _context.SaveChanges();

        //    // Store the OrderId in TempData to access in CheckAvailability
        //    TempData["OrderId"] = newOrder.Id;
        //    // Replace TempData["OrderId"] with Session["OrderId"]
        //    Session["OrderId"] = newOrder.Id;


        //    // Redirect to CheckAvailability action
        //    return RedirectToAction("CheckAvailability");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restock(List<int> selectedItems, List<int> quantities, List<string> newItemNames, List<int> newQuantities)
        {
            // Validate form submission for existing items
            if ((selectedItems == null || quantities == null || selectedItems.Count != quantities.Count) && (newItemNames == null || newQuantities == null || newItemNames.Count != newQuantities.Count))
            {
                ModelState.AddModelError("", "Please select or add at least one item and specify valid quantities.");
                return View("Restock", _context.PharmacistStocks.ToList());
            }

            // Validate each quantity in existing items
            bool hasInvalidQuantities = false;
            for (int i = 0; i < quantities?.Count; i++)
            {
                if (quantities[i] <= 0)
                {
                    ModelState.AddModelError("", $"Quantity for selected item {selectedItems[i]} must be greater than zero.");
                    hasInvalidQuantities = true;
                }
            }

            // Validate each quantity in new items
            for (int i = 0; i < newQuantities?.Count; i++)
            {
                if (newQuantities[i] <= 0)
                {
                    ModelState.AddModelError("", $"Quantity for new item '{newItemNames[i]}' must be greater than zero.");
                    hasInvalidQuantities = true;
                }
            }

            // Return to view if there are invalid quantities
            if (hasInvalidQuantities)
            {
                return View("Restock", _context.PharmacistStocks.ToList());
            }

            // Create a new order entry
            var newOrder = new Order
            {
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderItems = new List<OrderItem>() // Initialize the OrderItems collection
            };

            // Process valid selected (existing) items and quantities
            for (int i = 0; i < selectedItems?.Count; i++)
            {
                int itemId = selectedItems[i];
                int quantity = quantities[i];

                var stockItem = _context.PharmacistStocks.Find(itemId);

                // Create a new OrderItem and add it to the OrderItems collection
                var orderItem = new OrderItem
                {
                    ItemName = stockItem.ItemName,
                    QuantityOrdered = quantity
                };

                newOrder.OrderItems.Add(orderItem); // Add item to the order's items list
            }

            // Process new items and their quantities
            for (int i = 0; i < newItemNames?.Count; i++)
            {
                string itemName = newItemNames[i];
                int quantity = newQuantities[i];

                // Create a new OrderItem for each new item
                var newOrderItem = new OrderItem
                {
                    ItemName = itemName,
                    QuantityOrdered = quantity
                };

                newOrder.OrderItems.Add(newOrderItem); // Add new item to the order's items list
            }

            // Save the order with items to the database
            _context.Orders.Add(newOrder);
            _context.SaveChanges();

            // Store the OrderId in TempData to access in CheckAvailability
            TempData["OrderId"] = newOrder.Id;
            Session["OrderId"] = newOrder.Id;

            // Redirect to CheckAvailability action
            return RedirectToAction("CheckAvailability");
        }



        //Confirm Availability

        public ActionResult CheckAvailability()
        {
            // Retrieve OrderId from TempData
            var orderId = TempData["OrderId"] as int?;

            if (orderId == null)
            {
                TempData["ErrorMessage"] = "No order found for availability check.";
                return RedirectToAction("Restock");
            }

            // Retrieve the order and its items
            var order = _context.Orders
                .Include(o => o.OrderItems) // Include related OrderItems
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null || !order.OrderItems.Any())
            {
                TempData["ErrorMessage"] = "Order items not found for availability check.";
                return RedirectToAction("Restock");
            }

            var supplierAvailability = new List<SupplierItemAvailability>();

            // Query supplier inventory for each ordered item
            foreach (var orderItem in order.OrderItems)
            {
                var suppliers = _context.Inventories
                    .Where(i => i.ItemName.Trim().ToLower() == orderItem.ItemName.Trim().ToLower() && i.Quantity > 0)
                    .Select(i => new SupplierItemAvailability
                    {
                        SupplierName = i.Supplier.SupplierName,
                        ItemName = i.ItemName,
                        QuantityAvailable = i.Quantity,
                        Price = i.Price,
                        QuantityNeeded = orderItem.QuantityOrdered
                    }).ToList();

                // If no suppliers found, add a placeholder entry with zero availability
                if (!suppliers.Any())
                {
                    supplierAvailability.Add(new SupplierItemAvailability
                    {
                        SupplierName = "No suppliers available",
                        ItemName = orderItem.ItemName,
                        QuantityAvailable = 0,
                        Price = 0, // No price available
                        QuantityNeeded = orderItem.QuantityOrdered
                    });
                }
                else
                {
                    supplierAvailability.AddRange(suppliers);
                }
            }

            return View(supplierAvailability);

        }






        //public ActionResult RecommendSuppliers()
        //{
        //    // Retrieve OrderId from Session
        //    var orderId = Session["OrderId"] as int?;

        //    if (orderId == null)
        //    {
        //        TempData["ErrorMessage"] = "No order found for supplier recommendation.";
        //        return RedirectToAction("Restock");
        //    }

        //    // Retrieve the order and its items
        //    var order = _context.Orders
        //        .Include(o => o.OrderItems)
        //        .FirstOrDefault(o => o.Id == orderId);

        //    if (order == null || !order.OrderItems.Any())
        //    {
        //        TempData["ErrorMessage"] = "Order items not found for supplier recommendation.";
        //        return RedirectToAction("Restock");
        //    }

        //    ViewData["OrderId"] = orderId;

        //    var recommendations = new List<SupplierRecommendation>();

        //    foreach (var orderItem in order.OrderItems)
        //    {
        //        var suppliersWithItem = _context.Inventories
        //            .Where(i => i.ItemName == orderItem.ItemName && i.Quantity > 0)
        //            .OrderBy(i => i.Price) // Sort by price ascending for recommendation
        //            .ToList();

        //        if (suppliersWithItem.Any())
        //        {
        //            var quantityNeeded = orderItem.QuantityOrdered;
        //            var supplierList = new List<SupplierItemAvailability>();
        //            int remainingQuantity = quantityNeeded;

        //            // Find the best combination for recommendation
        //            foreach (var supplierInventory in suppliersWithItem)
        //            {
        //                if (remainingQuantity <= 0) break;

        //                int quantityFromSupplier = Math.Min(supplierInventory.Quantity, remainingQuantity);
        //                remainingQuantity -= quantityFromSupplier;

        //                supplierList.Add(new SupplierItemAvailability
        //                {
        //                    SupplierName = supplierInventory.Supplier.SupplierName,
        //                    ItemName = orderItem.ItemName,
        //                    QuantityAvailable = quantityFromSupplier,
        //                    Price = supplierInventory.Price,
        //                    QuantityNeeded = orderItem.QuantityOrdered
        //                });
        //            }

        //            // Add all suppliers who have enough stock for this item as alternative options
        //            var alternativeSuppliers = suppliersWithItem
        //                .Where(s => s.Quantity >= quantityNeeded) // Suppliers who can fulfill the full quantity individually
        //                .Select(s => new SupplierItemAvailability
        //                {
        //                    SupplierId = s.SupplierId, // Map SupplierId here
        //                    SupplierName = s.Supplier.SupplierName,
        //                    ItemName = orderItem.ItemName,
        //                    QuantityAvailable = s.Quantity,
        //                    Price = s.Price,
        //                    QuantityNeeded = orderItem.QuantityOrdered
        //                }).ToList();

        //            recommendations.Add(new SupplierRecommendation
        //            {
        //                OrderItemId = orderItem.Id, // Set OrderItemId here
        //                ItemName = orderItem.ItemName,
        //                Suppliers = supplierList, // Recommended combination based on price
        //                FullyFulfilled = (remainingQuantity <= 0),
        //                AlternativeSuppliers = alternativeSuppliers // Add alternatives who can meet the quantity alone
        //            });
        //        }
        //        else
        //        {
        //            // No supplier has the item
        //            recommendations.Add(new SupplierRecommendation
        //            {
        //                ItemName = orderItem.ItemName,
        //                Suppliers = new List<SupplierItemAvailability>(),
        //                FullyFulfilled = false
        //            });
        //        }
        //    }

        //    return View(recommendations);
        //}

        public ActionResult RecommendSuppliers()
        {
            // Retrieve OrderId from Session
            var orderId = Session["OrderId"] as int?;

            if (orderId == null)
            {
                TempData["ErrorMessage"] = "No order found for supplier recommendation.";
                return RedirectToAction("Restock");
            }

            // Retrieve the order and its items
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null || !order.OrderItems.Any())
            {
                TempData["ErrorMessage"] = "Order items not found for supplier recommendation.";
                return RedirectToAction("Restock");
            }

            ViewData["OrderId"] = orderId;

            var recommendations = new List<SupplierRecommendation>();

            foreach (var orderItem in order.OrderItems)
            {
                var suppliersWithItem = _context.Inventories
                    .Where(i => i.ItemName == orderItem.ItemName && i.Quantity > 0)
                    .OrderBy(i => i.Price) // Sort by price ascending for recommendation
                    .ToList();

                if (suppliersWithItem.Any())
                {
                    var quantityNeeded = orderItem.QuantityOrdered;
                    var supplierList = new List<SupplierItemAvailability>();
                    int remainingQuantity = quantityNeeded;

                    // Find the best combination for recommendation
                    foreach (var supplierInventory in suppliersWithItem)
                    {
                        if (remainingQuantity <= 0) break;

                        int quantityFromSupplier = Math.Min(supplierInventory.Quantity, remainingQuantity);
                        remainingQuantity -= quantityFromSupplier;

                        var supplierId = supplierInventory.SupplierId; // Capture SupplierId here for logging
                        var supplierName = supplierInventory.Supplier.SupplierName;

                        Console.WriteLine($"SupplierId: {supplierId}, SupplierName: {supplierName}");

                        supplierList.Add(new SupplierItemAvailability
                        {
                            SupplierId = supplierId, // Set SupplierId correctly
                            SupplierName = supplierName,
                            ItemName = orderItem.ItemName,
                            QuantityAvailable = quantityFromSupplier,
                            Price = supplierInventory.Price,
                            QuantityNeeded = orderItem.QuantityOrdered
                        });
                    }


                    // Add all suppliers who have enough stock for this item as alternative options
                    var alternativeSuppliers = suppliersWithItem
                        .Where(s => s.Quantity >= quantityNeeded) // Suppliers who can fulfill the full quantity individually
                        .Select(s => new SupplierItemAvailability
                        {
                            SupplierId = s.SupplierId, // Ensure SupplierId is set here
                            SupplierName = s.Supplier.SupplierName,
                            ItemName = orderItem.ItemName,
                            QuantityAvailable = s.Quantity,
                            Price = s.Price,
                            QuantityNeeded = orderItem.QuantityOrdered
                        }).ToList();

                    recommendations.Add(new SupplierRecommendation
                    {
                        OrderItemId = orderItem.Id, // Set OrderItemId here
                        ItemName = orderItem.ItemName,
                        Suppliers = supplierList, // Recommended combination based on price
                        FullyFulfilled = (remainingQuantity <= 0),
                        AlternativeSuppliers = alternativeSuppliers // Add alternatives who can meet the quantity alone

                    });
                }
                else
                {
                    // No supplier has the item
                    recommendations.Add(new SupplierRecommendation
                    {
                        OrderItemId = orderItem.Id, // Set OrderItemId here
                        ItemName = orderItem.ItemName,
                        Suppliers = new List<SupplierItemAvailability>(),
                        FullyFulfilled = false,
                        QuantityNeeded = orderItem.QuantityOrdered // Include QuantityNeeded
                    });
                }
            }

            return View(recommendations);
        }





        //cancel order on Recommendation page
        [HttpPost]
        public ActionResult CancelOrder()
        {
            // Retrieve OrderId from Session
            var orderId = Session["OrderId"] as int?;

            if (orderId == null)
            {
                TempData["ErrorMessage"] = "No order found to cancel.";
                return RedirectToAction("Restock");
            }

            // Retrieve the order with associated items
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == orderId);

            if (order != null)
            {
                // Remove associated order items and the order itself
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Order canceled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Order could not be found for cancellation.";
            }

            // Clear the session OrderId to reset the order context
            Session["OrderId"] = null;

            return RedirectToAction("Restock");
        }


        //make order with suppliers




        [HttpPost]
        public ActionResult MakeOrderWithSelectedSuppliers(FormCollection form)
        {
            var selections = new List<SupplierSelectionViewModel>();

            // Process each unique OrderItemId based on form keys
            var orderItemIds = form.AllKeys
                .Where(key => key.StartsWith("SelectedSupplier_"))
                .Select(key => int.Parse(key.Split('_')[1]))
                .Distinct();

            foreach (var orderItemId in orderItemIds)
            {
                // Capture the selected option for the current OrderItemId
                var selectedOption = form[$"SelectedSupplier_{orderItemId}"];
                Console.WriteLine($"Processing OrderItemId {orderItemId}, Selected Option: {selectedOption}");

                // Skip if "Notify" was selected
                if (selectedOption == "Notify")
                {
                    Console.WriteLine($"OrderItemId {orderItemId} skipped due to Notify option.");
                    continue;
                }

                if (selectedOption == "Recommended")
                {
                    // Handle "Recommended" option by collecting multiple supplier entries
                    Console.WriteLine($"Processing Recommended suppliers for OrderItemId {orderItemId}");

                    var supplierKeys = form.AllKeys
                        .Where(k => k.StartsWith($"Suppliers[{orderItemId}][Recommended]"));

                    foreach (var supplierKey in supplierKeys)
                    {
                        var parts = supplierKey.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 4)
                            continue;

                        var supplierCount = parts[3];
                        var quantityKey = $"Suppliers[{orderItemId}][Recommended][{supplierCount}].Quantity";
                        var priceKey = $"Suppliers[{orderItemId}][Recommended][{supplierCount}].Price";
                        var supplierIdKey = $"Suppliers[{orderItemId}][Recommended][{supplierCount}].SupplierId";

                        if (form.AllKeys.Contains(quantityKey) && form.AllKeys.Contains(priceKey) && form.AllKeys.Contains(supplierIdKey))
                        {
                            int quantity;
                            decimal price;

                            if (!int.TryParse(form[quantityKey], out quantity) || !decimal.TryParse(form[priceKey], out price))
                                continue;

                            var supplierId = form[supplierIdKey];
                            Console.WriteLine($"Recommended SupplierId {supplierId} for OrderItemId {orderItemId}: Quantity {quantity}, Price {price}");

                            if (!selections.Any(s => s.OrderItemId == orderItemId && s.SupplierId == supplierId))
                            {
                                selections.Add(new SupplierSelectionViewModel
                                {
                                    OrderItemId = orderItemId,
                                    SupplierId = supplierId,
                                    Quantity = quantity,
                                    Price = price
                                });
                            }
                        }
                    }
                }
                else
                {
                    // Handle alternative supplier selection
                    Console.WriteLine($"Processing Alternative supplier for OrderItemId {orderItemId} with SupplierId {selectedOption}");

                    var quantityKey = $"Suppliers[{orderItemId}][Alternative].Quantity";
                    var priceKey = $"Suppliers[{orderItemId}][Alternative].Price";
                    var supplierIdKey = $"Suppliers[{orderItemId}][Alternative].SupplierId";

                    // Ensure all required keys are present
                    if (form.AllKeys.Contains(supplierIdKey) && selectedOption == form[supplierIdKey])
                    {
                        int quantity;
                        decimal price;

                        if (form.AllKeys.Contains(quantityKey) && form.AllKeys.Contains(priceKey) &&
                            int.TryParse(form[quantityKey], out quantity) && decimal.TryParse(form[priceKey], out price))
                        {
                            var supplierId = form[supplierIdKey];
                            Console.WriteLine($"Alternative SupplierId {supplierId} for OrderItemId {orderItemId}: Quantity {quantity}, Price {price}");

                            if (!selections.Any(s => s.OrderItemId == orderItemId && s.SupplierId == supplierId))
                            {
                                selections.Add(new SupplierSelectionViewModel
                                {
                                    OrderItemId = orderItemId,
                                    SupplierId = supplierId,
                                    Quantity = quantity,
                                    Price = price
                                });
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse quantity or price for Alternative SupplierId {selectedOption} on OrderItemId {orderItemId}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"SupplierId mismatch or missing keys for Alternative SupplierId {selectedOption} on OrderItemId {orderItemId}");
                    }
                }
            }

            // Save each selection to the database
            foreach (var selection in selections)
            {
                Console.WriteLine($"Saving OrderItemId {selection.OrderItemId}, SupplierId {selection.SupplierId}, Quantity {selection.Quantity}, Price {selection.Price}");

                var orderItemDetail = new OrderItemDetails
                {
                    OrderItemId = selection.OrderItemId,
                    SupplierId = selection.SupplierId,
                    Quantity = selection.Quantity,
                    Status = "New",
                    Price = selection.Price * selection.Quantity
                };

                _context.OrderItemDetails.Add(orderItemDetail);

            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Order successfully placed with selected suppliers.";
            // Redirect to UpdateInventory after saving
            return RedirectToAction("UpdateInventory");
        }

        public ActionResult UpdateInventory()
        {
            // Fetch all OrderItemDetails where status is "Pending" to ensure we only adjust newly created records
            var orderItemDetails = _context.OrderItemDetails
                .Include(oid => oid.Supplier)
                .Where(oid => oid.Status == "New")
                .ToList();

            foreach (var detail in orderItemDetails)
            {
                // Find inventory item based on OrderItemDetails
                var inventoryItem = _context.Inventories
                    .FirstOrDefault(i => i.ItemName == detail.OrderItem.ItemName && i.SupplierId == detail.SupplierId);

                if (inventoryItem != null)
                {
                    // Reduce inventory quantity based on the quantity in OrderItemDetails
                    inventoryItem.Quantity -= detail.Quantity;
                    detail.Status = "Pending"; // Mark the status as processed
                }
            }

            _context.SaveChanges();

            TempData["InventoryUpdateMessage"] = "Inventory successfully updated based on the latest orders.";
            return RedirectToAction("OrderSummary");
        }



        // GET: OrderSummary
        public ActionResult OrderSummary()
        {
            // Retrieve OrderId from Session
            var orderId = Session["OrderId"] as int?;

            if (!orderId.HasValue)
            {
                TempData["ErrorMessage"] = "Order ID not found in session. Please try again.";
                return RedirectToAction("Index"); // Redirect if no order ID is found in session
            }

            // Fetch the order along with its items and details
            var order = _context.Orders
                .Include(o => o.OrderItems.Select(oi => oi.OrderItemDetails.Select(oid => oid.Supplier)))
                .SingleOrDefault(o => o.Id == orderId.Value);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index"); // Redirect to a different page if the order is not found
            }

            // Clear OrderId from session after use
            Session.Remove("OrderId");

            return View(order); // Pass the order as the model to the OrderSummary view
        }





        //manage orders
        public ActionResult ManageOrders()
        {
            var orders = _context.Orders
                .SelectMany(o => o.OrderItems.SelectMany(oi => oi.OrderItemDetails.Select(oid => new ManageOrderViewModel
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    ItemName = oi.ItemName,
                    QuantityOrdered = oi.QuantityOrdered,
                    Status = oi.Status,
                    SupplierName = oid.Supplier.SupplierName ?? "N/A", // Explicitly select SupplierName
                    IsVerified = o.IsVerified
                })))
                .ToList();

            return View(orders);
        }

        //cancel order
        [HttpPost]
        public ActionResult MarkOrderAsCanceled(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .SingleOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return HttpNotFound("Order not found.");
            }

            // Update the status of the order itself to "Canceled"
            order.Status = "Canceled";
            // Update status of each item in the order to "Canceled"
            foreach (var item in order.OrderItems)
            {
                item.Status = "Canceled";
            }

            _context.SaveChanges();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        //verify order
        [HttpGet]
        public ActionResult VerifyOrder(int orderId)
        {
            var order = _context.Orders
    .Include(o => o.OrderItems.Select(oi => oi.OrderItemDetails)) // Include OrderItemDetails
    .Where(o => o.Id == orderId)
    .Select(o => new VerifyOrderViewModel
    {
        OrderId = o.Id,
        OrderDate = o.OrderDate,
        Status = o.Status,
        OrderItems = o.OrderItems.Select(oi => new VerifyOrderItemViewModel
        {
            OrderItemId = oi.Id,
            ItemName = oi.ItemName,
            QuantityOrdered = oi.QuantityOrdered,
            OrderItemDetails = oi.OrderItemDetails.Select(oid => new VerifyOrderItemDetailViewModel
            {
                Id = oid.Id, // Make sure the Id is being mapped correctly
                Quantity = oid.Quantity,
                SupplierName = oid.Supplier != null ? oid.Supplier.SupplierName : "N/A",
                Status = oid.Status,
                Condition = oid.Condition,
                ReturnReason = oid.ReturnReason
            }).ToList() // Ensure OrderItemDetails is properly converted to List
        }).ToList()
    })
    .SingleOrDefault();



            if (order == null)
            {
                return HttpNotFound("Order not found.");
            }

            return View(order);
        }



        //verify and add to stock



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ConfirmVerification(int orderId)
        //{
        //    var order = _context.Orders
        //        .Include(o => o.OrderItems.Select(oi => oi.OrderItemDetails))
        //        .SingleOrDefault(o => o.Id == orderId);

        //    if (order == null)
        //    {
        //        return HttpNotFound("Order not found.");
        //    }

        //    // Mark order as verified
        //    order.IsVerified = true;
        //    order.Status = "Verified";

        //    foreach (var orderItem in order.OrderItems)
        //    {
        //        foreach (var detail in orderItem.OrderItemDetails)
        //        {
        //            // Retrieve the corresponding inventory item
        //            var inventoryItem = _context.Inventories
        //                .FirstOrDefault(i => i.ItemName == orderItem.ItemName && i.SupplierId == detail.SupplierId);

        //            if (inventoryItem == null)
        //            {
        //                continue; // Skip if inventory details cannot be found
        //            }

        //            // Generate a unique batch code specific to the item name
        //            var uniqueBatchCode = GenerateUniqueBatchCode(orderItem.ItemName);

        //            // Add new item to stock as a unique entry with a unique batch code
        //            var newStockItem = new PharmacistStock
        //            {
        //                ItemName = orderItem.ItemName,
        //                Dosage = inventoryItem.Dosage,
        //                QuantityAvailable = detail.Quantity,
        //                ExpirationDate = inventoryItem.ExpirationDate,
        //                LastUpdatedDate = DateTime.Now,
        //                BatchCode = uniqueBatchCode
        //            };

        //            _context.PharmacistStocks.Add(newStockItem);
        //        }
        //    }

        //    _context.SaveChanges();

        //    TempData["SuccessMessage"] = "Order has been verified and stock has been updated successfully.";
        //    return RedirectToAction("ManageStock");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmVerification(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems.Select(oi => oi.OrderItemDetails))
                .SingleOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return HttpNotFound("Order not found.");
            }

            order.IsVerified = true;
            order.Status = "Verified";

            foreach (var orderItem in order.OrderItems)
            {
                foreach (var detail in orderItem.OrderItemDetails)
                {
                    string condition = Request.Form[$"Condition_{detail.Id}"];
                    string returnReason = condition == "Bad" ? Request.Form[$"ReturnReason_{detail.Id}"] : null;

                    if (condition == "Good")
                    {
                        detail.Condition = "Good";

                        var inventoryItem = _context.Inventories
                            .FirstOrDefault(i => i.ItemName == orderItem.ItemName && i.SupplierId == detail.SupplierId);

                        if (inventoryItem != null)
                        {
                            var uniqueBatchCode = GenerateUniqueBatchCode(orderItem.ItemName);

                            var newStockItem = new PharmacistStock
                            {
                                ItemName = orderItem.ItemName,
                                Dosage = inventoryItem.Dosage,
                                QuantityAvailable = detail.Quantity,
                                ExpirationDate = inventoryItem.ExpirationDate,
                                LastUpdatedDate = DateTime.Now,
                                BatchCode = uniqueBatchCode
                            };

                            _context.PharmacistStocks.Add(newStockItem);
                        }
                        
                    }
                    else if (condition == "Bad")
                    {
                        detail.Condition = "Bad";
                        detail.ReturnReason = returnReason;
                    }
                }
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Verification complete: Stock updated and bad items returned.";
            return RedirectToAction("ManageStock");
        }











        // Helper method to generate a unique batch code based on ItemName
        private string GenerateUniqueBatchCode(string itemName)
        {
            string batchCode;
            var random = new Random();

            // Use a prefix based on the first 3 characters of the item name
            string itemPrefix = itemName.Length >= 3 ? itemName.Substring(0, 3).ToUpper() : itemName.ToUpper();

            do
            {
                // Generate a unique 4-digit numeric code and append it to the item prefix
                batchCode = $"{itemPrefix}-{random.Next(1000, 9999)}";
            }
            while (_context.PharmacistStocks.Any(ps => ps.BatchCode == batchCode));

            return batchCode;
        }




        //making QR codes

        // Action to generate QR code and send email
        public ActionResult SendStockQRCodeEmail(int id)
        {
            var stockItem = _context.PharmacistStocks.Find(id);
            if (stockItem == null)
            {
                return HttpNotFound("Stock item not found.");
            }

            // Create QR code content with BatchCode and ItemName
            string qrContent = stockItem.BatchCode;


            // Generate QR code as image stream
            using (var qrCodeImageStream = GenerateQRCodeImageStream(qrContent))
            {
                if (qrCodeImageStream == null)
                {
                    TempData["ErrorMessage"] = "Failed to generate QR code.";
                    return RedirectToAction("ManageStock");
                }

                // Prepare email message
                string toEmail = "thebookbuffet4@gmail.com"; // Recipient's email
                string subject = $"QR Code for Stock Item: {stockItem.ItemName} (Batch Code: {stockItem.BatchCode})";
                string body = $"<p>Here is the QR code for the stock item:</p><p><strong>Item Name:</strong> {stockItem.ItemName}</p><p><strong>Batch Code:</strong> {stockItem.BatchCode}</p>";

                try
                {
                    using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtpClient.Credentials = new NetworkCredential("thebookbuffet4@gmail.com", "korkecyugcbhgncr");
                        smtpClient.EnableSsl = true;

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress("thebookbuffet4@gmail.com", "The Book Buffet"),
                            Subject = subject,
                            Body = body,
                            IsBodyHtml = true
                        };
                        mailMessage.To.Add(toEmail);

                        // Attach QR code image
                        qrCodeImageStream.Position = 0; // Reset stream position
                        mailMessage.Attachments.Add(new Attachment(qrCodeImageStream, "StockQRCode.png", "image/png"));

                        smtpClient.Send(mailMessage);
                    }

                    TempData["SuccessMessage"] = "QR code email sent successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Failed to send email. Error: " + ex.Message;
                }
            }

            return RedirectToAction("ManageStock");
        }

        // Helper method to generate QR code image as a memory stream
        private MemoryStream GenerateQRCodeImageStream(string content)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrCodeData))
            {
                var qrCodeImage = qrCode.GetGraphic(20); // Generate the QR code image
                var memoryStream = new MemoryStream();
                qrCodeImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                return memoryStream; // Return image as memory stream
            }
        }


        //track expiry
        public ActionResult TrackExpiration()
        {
            var today = DateTime.Today;

            // Fetch all stock items
            var allItems = _context.PharmacistStocks
                .OrderBy(item => item.ExpirationDate)
                .Select(item => new TrackExpirationViewModel
                {
                    Name = item.ItemName,
                    BatchCode = item.BatchCode,
                    ExpirationDate = item.ExpirationDate
                })
                .ToList();

            return View(allItems);
        }


        [HttpPost]
        public JsonResult GetStockInfoByBatch(string batchCode)
        {
            // Assuming your database context and models are set up properly
            var stockItem = _context.PharmacistStocks.FirstOrDefault(item => item.BatchCode == batchCode);

            if (stockItem == null)
            {
                return Json(new { success = false, message = "Stock item not found." });
            }

            return Json(new
            {
                success = true,
                name = stockItem.ItemName,
                batchCode = stockItem.BatchCode,
                expirationDate = stockItem.ExpirationDate.ToString("dd MMMM yyyy")
            });
        }


        // Action to fetch pharmacist stock information based on the scanned QR data (e.g., Batch Code)
        [HttpPost]
        public JsonResult ProcessQRCode(string qrCodeData)
        {
            var parsedData = ParseQRCodeData(qrCodeData);

            if (parsedData == null || !parsedData.ContainsKey("BatchCode"))
            {
                return Json(new { success = false, message = "Invalid QR Code format." });
            }


            var batchCode = parsedData["BatchCode"];

            // Search for the stock item in the database using only the batch code
            var stockItem = _context.PharmacistStocks.FirstOrDefault(s => s.BatchCode == batchCode);



            if (stockItem == null)
            {
                return Json(new { success = false, message = "Stock item not found." });
            }

            return Json(new
            {
                success = true,
                message = "Stock item found",
                details = new
                {
                    ItemName = stockItem.ItemName,
                    BatchCode = stockItem.BatchCode,
                    ExpiryDate = stockItem.ExpirationDate.ToString("yyyy-MM-dd")
                }
            });
        }

        // Helper to parse the QR code data
        private Dictionary<string, string> ParseQRCodeData(string qrCodeData)
        {
            var data = new Dictionary<string, string>();
            try
            {
                var parts = qrCodeData.Split(';');
                foreach (var part in parts)
                {
                    var keyValue = part.Split(':');
                    if (keyValue.Length == 2)
                    {
                        data[keyValue[0].Trim()] = keyValue[1].Trim();
                    }
                }
            }
            catch
            {
                return null;
            }
            return data;
        }
















    }
}