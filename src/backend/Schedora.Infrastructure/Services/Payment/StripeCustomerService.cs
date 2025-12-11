using Microsoft.Extensions.Logging;
using Schedora.Domain.Dtos;
using Schedora.Domain.Services;
using Stripe;

namespace Schedora.Infrastructure.Services.Payment;

public class StripeCustomerService : ICustomerPaymentService
{

    public async Task<string> CreateCustomer(long userId, string fullName, string email, string? phoneNumber, UserAddressDto userAddress)
    {
        var options = new CustomerCreateOptions()
        {
            Address = new AddressOptions()
            {
                City = userAddress.City,
                Country = userAddress.Country,
                Line1 = userAddress.Line1,
                Line2 = userAddress.Line2,
                PostalCode = userAddress.PostalCode,
                State = userAddress.State,
            },
            Email = email,
            Name =  fullName,
            Metadata = new Dictionary<string, string> { { "user_id", userId.ToString() } }
        };
        if(!string.IsNullOrEmpty(phoneNumber))
            options.Phone = phoneNumber;

        var service = new CustomerService();
        var customer = await service.CreateAsync(options);
        
        return customer.Id;
    }
}