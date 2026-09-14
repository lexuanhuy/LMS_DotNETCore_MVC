using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LMS_DotNETCore_MVC.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMoMoService _moMoService;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMoMoService moMoService)
        {
            _context = context;
            _userManager = userManager;
            _moMoService = moMoService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDemoPayment()
        {
            var order = await CreatePendingOrderAsync();
            if (order == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            return RedirectToAction(nameof(QrCheckout), new { orderId = order.Id });
        }

        [HttpGet]
        public async Task<IActionResult> QrCheckout(Guid orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Course)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null || order.OrderStatus != "Pending")
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDemoPayment(Guid orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
            {
                return RedirectToAction("PaymentFailed", new { message = "Không tìm thấy đơn hàng" });
            }

            if (order.OrderStatus == "Completed")
            {
                return RedirectToAction("PaymentSuccess", new { orderId = order.Id });
            }

            if (order.OrderStatus != "Pending")
            {
                return RedirectToAction("PaymentFailed", new { message = "Đơn hàng không còn hiệu lực" });
            }

            await CompletePaidOrderAsync(order, $"DEMO-{DateTime.UtcNow:yyyyMMddHHmmss}");
            return RedirectToAction("PaymentSuccess", new { orderId = order.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelDemoPayment(Guid orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order != null && order.OrderStatus == "Pending")
            {
                order.OrderStatus = "Failed";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("PaymentFailed", new { message = "Bạn đã hủy thanh toán QR demo" });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentMoMo()
        {
            var order = await CreatePendingOrderAsync();
            if (order == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var payUrl = await _moMoService.CreatePaymentAsync(order, HttpContext);

            if (string.IsNullOrEmpty(payUrl))
            {
                order.OrderStatus = "Failed";
                await _context.SaveChangesAsync();
                return RedirectToAction("PaymentFailed");
            }

            return Redirect(payUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> MoMoReturn()
        {
            var queryParams = Request.Query;
            string resultCode = queryParams["resultCode"];
            string message = queryParams["message"];
            string orderIdStr = queryParams["orderId"];
            string signature = queryParams["signature"];

            string accessKey = queryParams["accessKey"];
            string amount = queryParams["amount"];
            string extraData = queryParams["extraData"];
            string ipnUrl = queryParams["ipnUrl"];
            string orderInfo = queryParams["orderInfo"];
            string partnerCode = queryParams["partnerCode"];
            string redirectUrl = queryParams["redirectUrl"];
            string requestId = queryParams["requestId"];
            string requestType = queryParams["requestType"];

            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderIdStr +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType;

            bool isValid = _moMoService.VerifySignature(signature, rawHash);

            if (!isValid)
            {
                return RedirectToAction("PaymentFailed", new { message = "Invalid Signature" });
            }

            if (Guid.TryParse(orderIdStr, out Guid orderId))
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null)
                {
                    if (resultCode == "0") // Success
                    {
                        await CompletePaidOrderAsync(order, queryParams["transId"]);
                        return RedirectToAction("PaymentSuccess", new { orderId = order.Id });
                    }
                    else
                    {
                        order.OrderStatus = "Failed";
                        await _context.SaveChangesAsync();
                        return RedirectToAction("PaymentFailed", new { message });
                    }
                }
            }

            return RedirectToAction("PaymentFailed");
        }

        [HttpGet]
        public IActionResult PaymentSuccess(Guid orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpGet]
        public IActionResult PaymentFailed(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        private async Task<Order?> CreatePendingOrderAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                return null;
            }

            var order = new Order
            {
                UserId = user.Id,
                TotalAmount = cart.CartItems.Sum(ci => ci.Price),
                OrderInfo = $"Thanh toán cho {cart.CartItems.Count} khóa học",
                OrderStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            foreach (var item in cart.CartItems)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    CourseId = item.CourseId,
                    Price = item.Price
                });
            }

            await _context.SaveChangesAsync();
            return order;
        }

        private async Task CompletePaidOrderAsync(Order order, string? transactionId)
        {
            order.OrderStatus = "Completed";
            order.TransactionId = transactionId;

            foreach (var detail in order.OrderDetails)
            {
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseId == detail.CourseId && e.StudentId == order.UserId);

                if (existingEnrollment == null)
                {
                    _context.Enrollments.Add(new Enrollment
                    {
                        CourseId = detail.CourseId,
                        StudentId = order.UserId,
                        EnrollDate = DateTime.UtcNow
                    });
                }
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == order.UserId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
        }
    }
}
