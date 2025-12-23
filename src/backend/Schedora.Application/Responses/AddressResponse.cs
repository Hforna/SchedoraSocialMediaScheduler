namespace Schedora.Application.Responses;

public class AddressResponse : BaseResponse
{
    public string City { get; set; }
    public string Country { get; set; }
    public string Line1 { get; set; }
    public string? Line2 { get; set; }
    public string PostalCode { get; set; }
    public string State { get; set; }
}