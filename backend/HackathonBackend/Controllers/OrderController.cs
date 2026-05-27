using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HackathonBackend.Data;
using HackathonBackend.Models;
using HackathonBackend.Services;

namespace HackathonBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _email;

        private const decimal LoyaltyAmountPerPoint = 10m;

        public OrderController(
            AppDbContext context,
            IWebHostEnvironment env,
            IEmailService email)
        {
            _context = context;
            _env = env;
            _email = email;
        }

        public class ApproveOrderDto
        {
            public DateTime? EstimatedDeliveryDate { get; set; }
        }

        public class RejectOrderDto
        {
            public string Reason { get; set; } = "";
        }

        private int GetUserId()
        {
            var idStr =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            return int.Parse(idStr ?? "0");
        }

        [HttpPost("place")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PlaceOrder(
            [FromForm] PlaceOrderRequest request)
        {
            int userId = GetUserId();

            var cart = _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Medicine)
                .ToList();

            if (!cart.Any())
            {
                return BadRequest(new
                {
                    message = "Cart is empty"
                });
            }

            foreach (var item in cart)
            {
                if (item.Medicine == null)
                {
                    return BadRequest(new
                    {
                        message = "Invalid medicine"
                    });
                }

                if (
                    item.Medicine.StockQuantity
                    < item.Quantity
                )
                {
                    return BadRequest(new
                    {
                        message =
                        $"Not enough stock for {item.Medicine.Name}"
                    });
                }
            }

            bool needsPrescription =
                cart.Any(
                    c => c.Medicine!.RequiresPrescription
                );

            string? savedFileName = null;

            if (needsPrescription)
            {
                if (
                    request.Prescription == null ||
                    request.Prescription.Length == 0
                )
                {
                    return BadRequest(new
                    {
                        message =
                        "Prescription is required"
                    });
                }

                // Validate file: JPG, PNG or PDF, max 5 MB.
                const long maxBytes = 5 * 1024 * 1024;
                if (request.Prescription.Length > maxBytes)
                {
                    return BadRequest(new
                    {
                        message = "Prescription file must be under 5 MB."
                    });
                }

                var extension = Path.GetExtension(request.Prescription.FileName)
                    ?.ToLowerInvariant() ?? "";
                var allowed = new HashSet<string> { ".jpg", ".jpeg", ".png", ".pdf" };
                if (!allowed.Contains(extension))
                {
                    return BadRequest(new
                    {
                        message = "Prescription must be a JPG, PNG or PDF file."
                    });
                }

                var folder = Path.Combine(
                    _env.ContentRootPath,
                    "wwwroot",
                    "prescriptions"
                );

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                savedFileName =
                    $"{Guid.NewGuid()}_{Path.GetFileName(request.Prescription.FileName)}";

                var fullPath =
                    Path.Combine(folder, savedFileName);

                using var stream =
                    new FileStream(
                        fullPath,
                        FileMode.Create
                    );

                await request.Prescription.CopyToAsync(stream);
            }

            decimal totalAmount =
                cart.Sum(
                    c => c.Medicine!.Price * c.Quantity
                );

            decimal discountAmount = 0m;

            int? promotionId = null;

            if (!string.IsNullOrWhiteSpace(request.PromoCode))
            {
                var now = DateTime.UtcNow;

                var promo = _context.Promotions.FirstOrDefault(p =>
                    p.PromotionCode == request.PromoCode &&
                    p.IsActive &&
                    p.StartDate <= now &&
                    p.EndDate >= now
                );

                if (promo != null)
                {
                    discountAmount =
                        promo.DiscountType == "Percentage"
                        ? Math.Round(
                            totalAmount *
                            promo.DiscountValue / 100m,
                            2
                        )
                        : promo.DiscountValue;

                    if (
                        promo.MaxDiscountAmount.HasValue &&
                        discountAmount >
                        promo.MaxDiscountAmount.Value
                    )
                    {
                        discountAmount =
                            promo.MaxDiscountAmount.Value;
                    }

                    if (discountAmount > totalAmount)
                    {
                        discountAmount = totalAmount;
                    }

                    promo.UsedCount += 1;

                    promotionId = promo.Id;
                }
            }

            decimal finalAmount =
                totalAmount - discountAmount;

            var orderDate = DateTime.UtcNow;

            var order = new Order
            {
                UserId = userId,

                OrderNumber =
                    $"ORD-{orderDate:yyyyMMddHHmmss}-{userId}",

                OrderDate = orderDate,

                EstimatedDeliveryDate =
                    orderDate.AddDays(3),

                Status = "Pending Validation",

                PrescriptionFile = savedFileName,

                PromotionId = promotionId,

                TotalAmount = totalAmount,

                DiscountAmount = discountAmount,

                FinalAmount = finalAmount,

                DeliveryAddress = request.Address ?? "",
                DeliveryPhone = request.Phone ?? "",
                DeliveryNotes = request.Notes ?? "",

                Items = cart.Select(c =>
                    new OrderItem
                    {
                        MedicineId = c.MedicineId,

                        Quantity = c.Quantity,

                        UnitPrice = c.Medicine!.Price,

                        TotalPrice =
                            c.Medicine!.Price
                            * c.Quantity
                    }).ToList()
            };

            _context.Orders.Add(order);

            foreach (var item in cart)
            {
                item.Medicine!.StockQuantity
                    -= item.Quantity;
            }

            _context.CartItems.RemoveRange(cart);

            await _context.SaveChangesAsync();

            int pointsEarned =
                (int)Math.Floor(
                    finalAmount /
                    LoyaltyAmountPerPoint
                );

            if (pointsEarned > 0)
            {
                var user =
                    _context.Users.Find(userId);

                if (user != null)
                {
                    user.LoyaltyPoints += pointsEarned;

                    _context.LoyaltyTransactions.Add(
                        new LoyaltyTransaction
                        {
                            UserId = userId,
                            OrderId = order.Id,
                            PointsEarned = pointsEarned,
                            PointsRedeemed = 0,
                            TransactionDate = DateTime.UtcNow,
                            Description =
                                $"Earned for order {order.OrderNumber}"
                        });

                    await _context.SaveChangesAsync();
                }
            }

            // ===== Confirmation email =====
            var customer = await _context.Users.FindAsync(userId);
            if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
            {
                var lines = string.Join("\n",
                    cart.Select(c =>
                        $"  - {c.Medicine!.Name} x {c.Quantity} @ ₹{c.Medicine.Price} = ₹{c.Medicine.Price * c.Quantity}"));

                var body =
$@"Hi {customer.FirstName},

Thanks for your order at ByteBrigade Pharmacy!

Order number: {order.OrderNumber}
Placed at:    {order.OrderDate:dddd, dd MMM yyyy HH:mm} UTC

Items:
{lines}

Subtotal:     ₹{order.TotalAmount:F2}
Discount:     -₹{order.DiscountAmount:F2}
Final amount: ₹{order.FinalAmount:F2}

Status: Pending Validation. We will email you again once our team
validates your order{(needsPrescription ? " and prescription" : "")}.

— ByteBrigade Pharmacy";

                await _email.SendAsync(
                    customer.Email,
                    $"Order {order.OrderNumber} received - awaiting validation",
                    body);
            }

            return Ok(new
            {
                message =
                    "Order placed successfully",

                orderId = order.Id,

                orderNumber =
                    order.OrderNumber,

                totalAmount =
                    order.TotalAmount,

                finalAmount =
                    order.FinalAmount,

                estimatedDeliveryDate =
                    order.EstimatedDeliveryDate,

                pointsEarned
            });
        }

        [HttpGet("my")]
        public IActionResult MyOrders()
        {
            int userId = GetUserId();

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Medicine)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return Ok(orders);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult AllOrders()
        {
            var orders = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Medicine)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return Ok(orders);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateStatus(
            int id,
            [FromBody] string status)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();

            order.Status = status;
            _context.SaveChanges();
            return Ok(order);
        }

        // ===== Approve order (Admin) =====
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveOrder(int id, [FromBody] ApproveOrderDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            order.Status = "Approved";
            order.RejectionReason = null;
            order.EstimatedDeliveryDate =
                dto?.EstimatedDeliveryDate ?? DateTime.UtcNow.AddDays(3);

            await _context.SaveChangesAsync();

            // Fire-and-forget style email (kept awaited to surface logging errors)
            var user = await _context.Users.FindAsync(order.UserId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var body =
$@"Hi {user.FirstName},

Great news — your ByteBrigade Pharmacy order {order.OrderNumber} has been APPROVED.

Estimated delivery: {order.EstimatedDeliveryDate:dddd, dd MMM yyyy}
Final amount: ₹{order.FinalAmount:F2}

Thank you for choosing ByteBrigade.
— ByteBrigade Pharmacy";

                await _email.SendAsync(user.Email, $"Order {order.OrderNumber} approved", body);
            }

            return Ok(new
            {
                message = "Order approved",
                orderId = order.Id,
                orderNumber = order.OrderNumber,
                status = order.Status,
                estimatedDeliveryDate = order.EstimatedDeliveryDate
            });
        }

        // ===== Reject order (Admin) =====
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectOrder(int id, [FromBody] RejectOrderDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Reason))
                return BadRequest(new { message = "A rejection reason is required." });

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Rejected";
            order.RejectionReason = dto.Reason.Trim();

            // Restore stock for items in this order (best-effort)
            var orderItems = _context.OrderItems
                .Where(i => i.OrderId == order.Id)
                .ToList();

            foreach (var oi in orderItems)
            {
                var med = await _context.Medicines.FindAsync(oi.MedicineId);
                if (med != null) med.StockQuantity += oi.Quantity;
            }

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(order.UserId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var body =
$@"Hi {user.FirstName},

Your ByteBrigade Pharmacy order {order.OrderNumber} has been REJECTED.

Reason: {order.RejectionReason}

If you believe this was a mistake, please contact our support team.
— ByteBrigade Pharmacy";

                await _email.SendAsync(user.Email, $"Order {order.OrderNumber} rejected", body);
            }

            return Ok(new
            {
                message = "Order rejected",
                orderId = order.Id,
                orderNumber = order.OrderNumber,
                status = order.Status,
                rejectionReason = order.RejectionReason
            });
        }

        // ===== Mark delivered (Admin) =====
        [HttpPut("{id}/deliver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkDelivered(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Delivered";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order marked as delivered",
                orderId = order.Id,
                status = order.Status
            });
        }
    }
}