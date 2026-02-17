using Microsoft.AspNetCore.Mvc;
using PaymentGatewayIntegration.Models;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

namespace PaymentGatewayIntegration.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _config;
        private readonly MvcdbContext _context;

        public PaymentController(IConfiguration config, MvcdbContext context)
        {
            _config = config;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // Step 1: Create Order
        public IActionResult Pay(int studentId, decimal amount)
        {
            string key = _config["Razorpay:Key"];
            string secret = _config["Razorpay:Secret"];

            RazorpayClient client = new RazorpayClient(key, secret);

            Dictionary<string, object> options = new Dictionary<string, object>
        {
            { "amount", amount * 100 },
            { "currency", "INR" },
            { "receipt", Guid.NewGuid().ToString() }
        };

            Order order = client.Order.Create(options);

            // Save to DB
            var payment = new Models.Payment
            {
                StudentId = studentId,
                OrderId = order["id"].ToString(),
                Amount = amount,
                Status = "Created"
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            ViewBag.OrderId = order["id"].ToString();
            ViewBag.Amount = amount;
            ViewBag.Key = key;

            return View();
        }

        // Step 2: Verify Payment
        [HttpPost]
        public IActionResult Verify(string razorpay_order_id,
                                    string razorpay_payment_id,
                                    string razorpay_signature)
        {
            string secret = _config["Razorpay:Secret"];
            string payload = razorpay_order_id + "|" + razorpay_payment_id;

            string generatedSignature = ComputeHmacSha256(payload, secret);

            if (generatedSignature == razorpay_signature)
            {
                var payment = _context.Payments
                    .FirstOrDefault(p => p.OrderId == razorpay_order_id);

                if (payment != null)
                {
                    payment.PaymentId = razorpay_payment_id;
                    payment.Status = "Paid";
                    _context.SaveChanges();
                }

                return RedirectToAction("Success");
            }

            return Content("Payment Failed");
        }

        public IActionResult Success()
        {
            return View();
        }

        private string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
