using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface ICustomerPaymentService
{
    public Task<string> CreateCustomer(string fullName, string email, string phoneNumber, UserAddressDto userAddress);
}