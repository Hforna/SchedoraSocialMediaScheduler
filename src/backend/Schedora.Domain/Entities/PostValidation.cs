using System;
using System.Collections.Generic;
using System.Text.Json;
using Schedora.Domain.Dtos;

namespace Schedora.Domain.Entities;

public class PostValidation : Entity
{

    public string Validations { get; set; } = "";
    public PostValidationEnum Status { get; set; } = PostValidationEnum.PENDING;

    public long PostId { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public void SucceedValidation()
    {
        Status = PostValidationEnum.SUCCEEDED;
        ValidatedAt = DateTime.UtcNow;
    }

    public void SetValidations(List<PostValidationDto> errors)
    {
        Validations = JsonSerializer.Serialize(errors);
    }

    public List<PostValidationDto>? GetValidations()
    {
        return JsonSerializer.Deserialize<List<PostValidationDto>>(Validations);
    }
}