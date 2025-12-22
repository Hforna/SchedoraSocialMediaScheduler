using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Schedora.Application.Services;
using Stripe;

namespace Schedora.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    public WebhooksController(IStripeWebhookService stripeWebhookService, 
        ILogger<WebhooksController> logger, IConfiguration configuration)
    {
        _stripeWebhookService = stripeWebhookService;
        _logger = logger;
        _configuration = configuration;
    }

    private readonly IStripeWebhookService _stripeWebhookService;
    private readonly ILogger<WebhooksController> _logger;
    private readonly IConfiguration _configuration;
    
    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var request = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        var endpointSecret = _configuration.GetValue<string>("services:payment:stripe:webhook:endpointSecret");
        
        try
        {
            var signatureHeader = HttpContext.Request.Headers["Stripe-Signature"];
            
            var stripeEvent = EventUtility.ParseEvent(request, throwOnApiVersionMismatch: false);

            stripeEvent = EventUtility.ConstructEvent(request, signatureHeader, endpointSecret, throwOnApiVersionMismatch: false);

            switch (stripeEvent.Type)
            {
                case EventTypes.CustomerSubscriptionCreated:
                    await _stripeWebhookService.HandleSubscriptionCreatedEvent(stripeEvent.Object);
                    break;
            }

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "There was an error on stripe webhook endpoint: {message}", e.Message);
            
            return BadRequest();
        }
    }
}