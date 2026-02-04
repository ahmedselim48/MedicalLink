using MedLink.Application.Interfaces.Services;
using MedLink.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Medical_Team_B.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IStripeWebhookService _webhookService;

        public WebhookController(
            ILogger<WebhookController> logger,
            IStripeWebhookService webhookService)
        {
            _logger = logger;
            _webhookService = webhookService;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var signature = Request.Headers["Stripe-Signature"];
                
                 if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Missing Stripe-Signature header.");
                    return BadRequest();
                }

                await _webhookService.HandleStripeEventAsync(json, signature!);
                return Ok();
            }
            catch (StripeException ex)
            {
                 // Stripe specific errors (signature verification failed, etc)
                 // We return 400 so Stripe doesn't retry invalid requests
                _logger.LogError(ex, "Stripe Webhook processing failed.");
                return BadRequest(); 
            }
            catch (Exception ex)
            {
                // General errors (DB connection, logic error)
                // We return 500 so Stripe RETRIES later
                _logger.LogError(ex, "General Webhook processing error.");
                return StatusCode(500); 
            }
        }
    }
}
