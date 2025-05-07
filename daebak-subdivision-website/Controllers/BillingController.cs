using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace daebak_subdivision_website.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BillingController> _logger;

        public BillingController(ApplicationDbContext dbContext, ILogger<BillingController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region Admin Billing Views and Actions

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AdminBilling()
        {
            // Get counts for statistics
            int paidBillsCount = await _dbContext.UserBills.CountAsync(b => b.Status == "Paid");
            int pendingBillsCount = await _dbContext.UserBills.CountAsync(b => b.Status == "Pending");
            int overdueBillsCount = await _dbContext.UserBills.CountAsync(b => b.Status == "Overdue");
            decimal paymentsReceived = await _dbContext.Payments.Where(p => p.Status == "Completed").SumAsync(p => p.Amount);

            // Get billing items for the table
            var billingItems = await _dbContext.BillingItems.ToListAsync();

            // Get user bills with related data
            var userBills = await _dbContext.UserBills
                .Include(b => b.User)
                .Include(b => b.BillingItem)
                .OrderByDescending(b => b.DueDate)
                .ToListAsync();

            // Get payments with related data
            var payments = await _dbContext.Payments
                .Include(p => p.User)
                .Include(p => p.Bill)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Get all homeowners for bill assignment
            var homeowners = await _dbContext.Homeowners
                .Include(h => h.User)
                .ToListAsync();

            // Set ViewBag data for the view
            ViewBag.PaidBillsCount = paidBillsCount;
            ViewBag.PendingBillsCount = pendingBillsCount;
            ViewBag.OverdueBillsCount = overdueBillsCount;
            ViewBag.BillingItems = billingItems;
            ViewBag.UserBills = userBills;
            ViewBag.Payments = payments;
            ViewBag.Homeowners = homeowners;
            
            // For the charts/widgets
            ViewBag.TotalOutstanding = userBills.Where(b => b.Status != "Paid").Sum(b => b.Amount);
            ViewBag.TotalCollected = payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            ViewBag.OverdueBills = overdueBillsCount;

            // Create model and populate the properties used by the view
            var model = new AdminPageModel
            {
                PaymentsReceived = paymentsReceived,
                
                // Map UserBills to BillViewModel for the "All Bills" tab
                Bills = userBills.Select(b => new BillViewModel
                {
                    BillId = b.BillId,
                    BillNumber = b.BillNumber,
                    HomeownerName = $"{b.User.FirstName} {b.User.LastName}",
                    HouseNumber = b.User.HouseNumber ?? "N/A",
                    Description = b.BillingItem?.ItemName ?? "N/A",
                    Amount = b.Amount,
                    DueDate = b.DueDate,
                    Status = b.Status
                }).ToList(),
                
                // Map only pending UserBills for the "Pending Bills" tab
                PendingBills = userBills.Where(b => b.Status == "Pending").Select(b => new BillViewModel
                {
                    BillId = b.BillId,
                    BillNumber = b.BillNumber,
                    HomeownerName = $"{b.User.FirstName} {b.User.LastName}",
                    HouseNumber = b.User.HouseNumber ?? "N/A",
                    Description = b.BillingItem?.ItemName ?? "N/A",
                    Amount = b.Amount,
                    DueDate = b.DueDate,
                    Status = b.Status
                }).ToList(),
                
                // Map Payments to PaymentViewModel for the "Payment Records" tab
                PaymentRecords = payments.Select(p => new PaymentViewModel
                {
                    PaymentId = p.PaymentId,
                    ReceiptNumber = p.ReceiptNumber,
                    BillNumber = p.Bill?.BillNumber ?? "N/A",
                    HomeownerName = $"{p.User.FirstName} {p.User.LastName}",
                    HouseNumber = p.User.HouseNumber ?? "N/A",
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod
                }).ToList()
            };

            _logger.LogInformation("Admin billing page accessed");
            return View("~/Views/Admin/Billing.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBillingItem(BillingItem billingItem)
        {
            if (ModelState.IsValid)
            {
                billingItem.CreatedAt = DateTime.Now;
                billingItem.UpdatedAt = DateTime.Now;

                _dbContext.Add(billingItem);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Billing Item created: {ItemName}", billingItem.ItemName);
                return Json(new { success = true, message = "Billing item created successfully" });
            }

            return Json(new { success = false, message = "Failed to create billing item", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetBillingItem(int id)
        {
            var billingItem = await _dbContext.BillingItems.FindAsync(id);
            if (billingItem == null)
            {
                return NotFound();
            }

            return Json(billingItem);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBillingItem(BillingItem billingItem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _dbContext.BillingItems.AsNoTracking()
                        .FirstOrDefaultAsync(b => b.ItemId == billingItem.ItemId);
                    
                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    billingItem.CreatedAt = existingItem.CreatedAt;
                    billingItem.UpdatedAt = DateTime.Now;

                    _dbContext.Update(billingItem);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Billing Item updated: {ItemName}", billingItem.ItemName);
                    return Json(new { success = true, message = "Billing item updated successfully" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillingItemExists(billingItem.ItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return Json(new { success = false, message = "Failed to update billing item", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBillingItem(int id)
        {
            var billingItem = await _dbContext.BillingItems.FindAsync(id);
            if (billingItem == null)
            {
                return NotFound();
            }

            _dbContext.BillingItems.Remove(billingItem);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Billing Item deleted: {ItemName}", billingItem.ItemName);
            return Json(new { success = true, message = "Billing item deleted successfully" });
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignBillToUsers(int billingItemId, List<int> userIds, DateTime? customDueDate)
        {
            var billingItem = await _dbContext.BillingItems.FindAsync(billingItemId);
            if (billingItem == null)
            {
                return NotFound("Billing item not found");
            }

            int year = DateTime.Now.Year;
            int assignedCount = 0;
            var dueDate = customDueDate ?? billingItem.DueDate;

            foreach (var userId in userIds)
            {
                // Check if user exists
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) continue;

                // Generate a unique bill number
                var billCount = await _dbContext.UserBills.CountAsync();
                string billNumber = $"BILL-{year}-{(billCount + assignedCount + 1):D4}";

                // Create new user bill
                var userBill = new UserBill
                {
                    UserId = userId,
                    ItemId = billingItemId,
                    BillNumber = billNumber,
                    Amount = billingItem.Amount,
                    DueDate = dueDate,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbContext.UserBills.Add(userBill);
                assignedCount++;

                // Note: Notification creation removed
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Bills assigned to {Count} users for {ItemName}", assignedCount, billingItem.ItemName);
            return Json(new { success = true, message = $"Bills assigned to {assignedCount} users successfully" });
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPayment(int paymentId)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.Bill)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound("Payment not found");
            }

            payment.Status = "Completed";
            payment.UpdatedAt = DateTime.Now;

            // Update the bill status if applicable
            if (payment.Bill != null)
            {
                payment.Bill.Status = "Paid";
                payment.Bill.UpdatedAt = DateTime.Now;

                // Note: Notification creation removed
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Payment verified: Receipt #{ReceiptNumber}", payment.ReceiptNumber);
            return Json(new { success = true, message = "Payment verified successfully" });
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int paymentId, string reason)
        {
            var payment = await _dbContext.Payments
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound("Payment not found");
            }

            payment.Status = "Failed";
            payment.PaymentDetails = $"{payment.PaymentDetails}\nRejection reason: {reason}";
            payment.UpdatedAt = DateTime.Now;

            // Note: Notification creation removed
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Payment rejected: Receipt #{ReceiptNumber}", payment.ReceiptNumber);
            return Json(new { success = true, message = "Payment rejected successfully" });
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPaymentReminder(int billId)
        {
            var bill = await _dbContext.UserBills
                .Include(b => b.User)
                .Include(b => b.BillingItem)
                .FirstOrDefaultAsync(b => b.BillId == billId);

            if (bill == null)
            {
                return NotFound("Bill not found");
            }

            // Note: Notification creation removed
            // Instead, we'll just log that a reminder would be sent
            _logger.LogInformation("Payment reminder for Bill #{BillNumber} would be sent", bill.BillNumber);
            
            return Json(new { success = true, message = "Payment reminder logged successfully" });
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetBillingReport(string timeRange, DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime reportStartDate;
            DateTime reportEndDate = DateTime.Now;

            // Set date range based on selection
            switch (timeRange)
            {
                case "this_month":
                    reportStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
                case "last_month":
                    reportStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                    reportEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                    break;
                case "last_3_months":
                    reportStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-3);
                    break;
                case "this_year":
                    reportStartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    break;
                case "custom":
                    if (startDate == null || endDate == null)
                    {
                        return BadRequest("Start date and end date are required for custom range");
                    }
                    reportStartDate = startDate.Value;
                    reportEndDate = endDate.Value;
                    break;
                default:
                    reportStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
            }

            // Get payments data
            var completedPayments = await _dbContext.Payments
                .Include(p => p.Bill)
                .Where(p => p.Status == "Completed" && p.PaymentDate >= reportStartDate && p.PaymentDate <= reportEndDate)
                .ToListAsync();

            // Get bills data for the period
            var billsDue = await _dbContext.UserBills
                .Include(b => b.BillingItem)
                .Where(b => b.DueDate >= reportStartDate && b.DueDate <= reportEndDate)
                .ToListAsync();

            // Group by category
            var reportData = new List<object>();
            var categories = await _dbContext.BillingItems.Select(b => b.Category).Distinct().ToListAsync();

            decimal totalExpected = 0;
            decimal totalCollected = 0;
            decimal totalPending = 0;
            decimal totalOverdue = 0;

            foreach (var category in categories)
            {
                // Skip null category
                if (string.IsNullOrEmpty(category)) continue;

                // Get bills for this category
                var categoryBills = billsDue
                    .Where(b => b.BillingItem.Category == category)
                    .ToList();

                decimal expectedAmount = categoryBills.Sum(b => b.Amount);
                
                // Get payments for this category (via bill reference)
                var categoryPayments = completedPayments
                    .Where(p => p.Bill != null && p.Bill.BillingItem.Category == category)
                    .ToList();
                
                decimal collectedAmount = categoryPayments.Sum(p => p.Amount);
                
                decimal pendingAmount = categoryBills
                    .Where(b => b.Status == "Pending")
                    .Sum(b => b.Amount);
                
                decimal overdueAmount = categoryBills
                    .Where(b => b.Status == "Overdue")
                    .Sum(b => b.Amount);

                // Calculate collection rate
                decimal collectionRate = expectedAmount > 0 
                    ? Math.Round((collectedAmount / expectedAmount) * 100, 2) 
                    : 0;

                reportData.Add(new
                {
                    Category = category,
                    ExpectedAmount = expectedAmount,
                    CollectedAmount = collectedAmount,
                    CollectionRate = collectionRate,
                    PendingAmount = pendingAmount,
                    OverdueAmount = overdueAmount
                });

                totalExpected += expectedAmount;
                totalCollected += collectedAmount;
                totalPending += pendingAmount;
                totalOverdue += overdueAmount;
            }

            // Add total row
            decimal totalCollectionRate = totalExpected > 0 
                ? Math.Round((totalCollected / totalExpected) * 100, 2) 
                : 0;

            var result = new
            {
                ReportData = reportData,
                Totals = new
                {
                    ExpectedAmount = totalExpected,
                    CollectedAmount = totalCollected,
                    CollectionRate = totalCollectionRate,
                    PendingAmount = totalPending,
                    OverdueAmount = totalOverdue
                },
                TimeRange = new
                {
                    StartDate = reportStartDate.ToString("yyyy-MM-dd"),
                    EndDate = reportEndDate.ToString("yyyy-MM-dd")
                }
            };

            return Json(result);
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetMonthlyRevenueData(int year)
        {
            // If no year specified, use current year
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            var monthlyData = new decimal[12];

            // Get all completed payments for the year
            var payments = await _dbContext.Payments
                .Where(p => p.Status == "Completed" && p.PaymentDate.Year == year)
                .ToListAsync();

            // Group by month and sum amounts
            for (int i = 1; i <= 12; i++)
            {
                monthlyData[i - 1] = payments
                    .Where(p => p.PaymentDate.Month == i)
                    .Sum(p => p.Amount);
            }

            return Json(new
            {
                Year = year,
                MonthlyRevenue = monthlyData
            });
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetPaymentMethodsData()
        {
            var paymentMethods = await _dbContext.Payments
                .Where(p => p.Status == "Completed")
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(p => p.Amount)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return Json(paymentMethods);
        }

        #endregion

        #region Homeowner Billing Views and Actions

        [Authorize(Roles = "HOMEOWNER")]
        public async Task<IActionResult> Index()
        {
            // Get current user ID
            var username = User.Identity?.Name;
            var user = await _dbContext.Users
                .Include(u => u.Homeowner) // Make sure to include the Homeowner navigation property
                .FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user's bills and payments
            var userBills = await _dbContext.UserBills
                .Include(b => b.BillingItem)
                .Where(b => b.UserId == user.UserId)
                .OrderByDescending(b => b.DueDate)
                .ToListAsync();

            var userPayments = await _dbContext.Payments
                .Include(p => p.Bill)
                .Where(p => p.UserId == user.UserId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Calculate current balance (sum of pending and overdue bills)
            decimal currentBalance = userBills
                .Where(b => b.Status == "Pending" || b.Status == "Overdue")
                .Sum(b => b.Amount);

            // Get next due date (nearest upcoming due date)
            var upcomingBill = userBills
                .Where(b => b.Status == "Pending" && b.DueDate > DateTime.Now)
                .OrderBy(b => b.DueDate)
                .FirstOrDefault();

            DateTime? nextDueDate = upcomingBill?.DueDate;

            // Get last payment info
            var lastPayment = userPayments
                .Where(p => p.Status == "Completed")
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            // Create model or use ViewBag
            ViewBag.FirstName = user.FirstName;
            ViewBag.HouseNumber = user.Homeowner?.HouseNumber ?? "N/A"; // Use null conditional operator to safely access HouseNumber
            ViewBag.CurrentBalance = currentBalance;
            ViewBag.DueDate = nextDueDate;
            ViewBag.LastPayment = lastPayment?.Amount;
            ViewBag.LastPaymentDate = lastPayment?.PaymentDate;
            ViewBag.UserBills = userBills;
            ViewBag.PaymentHistory = userPayments;

            return View("~/Views/Homeowner/Billing.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "HOMEOWNER")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment(int billId, decimal amount, string paymentMethod, string referenceNumber = null)
        {
            // Get current user ID
            var username = User.Identity?.Name;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Check if bill exists and belongs to user
            var bill = await _dbContext.UserBills.FindAsync(billId);
            if (bill == null || bill.UserId != user.UserId)
            {
                return Json(new { success = false, message = "Bill not found or does not belong to user" });
            }

            // Generate receipt number
            int year = DateTime.Now.Year;
            int paymentCount = await _dbContext.Payments.CountAsync();
            string receiptNumber = $"RCPT-{year}-{(paymentCount + 1):D4}";

            // Create new payment
            var payment = new Payment
            {
                BillId = billId,
                UserId = user.UserId,
                ReceiptNumber = receiptNumber,
                PaymentDate = DateTime.Now,
                Amount = amount,
                PaymentMethod = paymentMethod,
                ReferenceNumber = referenceNumber,
                Status = "Pending", // Pending until verified by admin
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _dbContext.Payments.Add(payment);

            // Update bill status if full payment
            if (amount >= bill.Amount)
            {
                bill.Status = "Pending"; // Still pending until admin verifies payment
                bill.UpdatedAt = DateTime.Now;
            }
            else if (amount > 0)
            {
                bill.Status = "Partially Paid";
                bill.UpdatedAt = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Payment made: Receipt #{ReceiptNumber} by {Username}", receiptNumber, username);
            return Json(new { success = true, message = "Payment recorded successfully", receiptNumber });
        }

        [HttpPost]
        [Authorize(Roles = "HOMEOWNER")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPaymentProof(int paymentId, string proofOfPaymentUrl)
        {
            // Get current user ID
            var username = User.Identity?.Name;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Check if payment exists and belongs to user
            var payment = await _dbContext.Payments.FindAsync(paymentId);
            if (payment == null || payment.UserId != user.UserId)
            {
                return Json(new { success = false, message = "Payment not found or does not belong to user" });
            }

            // Update payment with proof URL
            payment.ProofOfPaymentUrl = proofOfPaymentUrl;
            payment.UpdatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Payment proof uploaded: Receipt #{ReceiptNumber} by {Username}", payment.ReceiptNumber, username);
            return Json(new { success = true, message = "Payment proof uploaded successfully" });
        }

        #endregion

        private bool BillingItemExists(int id)
        {
            return _dbContext.BillingItems.Any(b => b.ItemId == id);
        }
    }
}