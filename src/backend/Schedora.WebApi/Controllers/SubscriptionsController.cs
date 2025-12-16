using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedora.Application.Requests;
using Schedora.Application.Services;
using Schedora.WebApi.RequestExamples;
using Swashbuckle.AspNetCore.Filters;

namespace Schedora.WebApi.Controllers;

/// <summary>
/// Subscriptions API controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionsController"/> class.
    /// </summary>
    /// <param name="subscriptionService">
    /// Service responsible for handling subscription business logic.
    /// </param>
    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }
    
    /// <summary>
    /// Gets all available subscription plans.
    /// </summary>
    /// <returns>
    /// Returns a list of available subscription plans.
    /// </returns>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailablePlans()
    {
        var result = await _subscriptionService.GetSubscriptionPlans();
        
        return Ok(result);
    }

    /// <summary>
    /// Gets the current subscription of the authenticated user.
    /// </summary>
    /// <returns>
    /// Returns the authenticated user's current subscription details.
    /// </returns>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUserSubscription()
    {
        var result = await _subscriptionService.GetCurrentUserSubscriptionPlan();
        
        return Ok(result);
    }

    /// <summary>
    /// Creates a checkout session to start a new subscription.
    /// </summary>
    /// <param name="request">
    /// Request containing the information required to create the subscription checkout session.
    /// </param>
    /// <returns>
    /// Returns data required to complete the subscription checkout process.
    /// </returns>
    [HttpPost]
    [SwaggerRequestExample(typeof(CreateSubscriptionCheckoutRequest), typeof(CreateSubscriptionCheckoutRequestExample))]
    public async Task<IActionResult> CreateSubscriptionCheckoutSession([FromBody]CreateSubscriptionCheckoutRequest request)
    {
        var result = await _subscriptionService.CreateSubscriptionCheckout(request);
        
        return Ok(result);
    }
}