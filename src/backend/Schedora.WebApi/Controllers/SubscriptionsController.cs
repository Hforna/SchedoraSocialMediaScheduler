using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedora.Application.Requests;
using Schedora.Application.Services;

namespace Schedora.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    
    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }
    
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailablePlans()
    {
        var result = await _subscriptionService.GetSubscriptionPlans();
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscriptionCheckoutSession([FromBody] CreateSubscriptionCheckoutRequest request)
    {
        var result = await _subscriptionService.CreateSubscriptionCheckout(request);
        
        return Ok(result);
    }
}