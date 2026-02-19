using Schedora.Domain.Dtos;

namespace Schedora.Application.Responses;

public class PostValidationResponse
{
     public long PostId { get; set; }
     public List<PostValidationDto> Validations { get; set; }
     public DateTime ValidatedAt  { get; set; }
}