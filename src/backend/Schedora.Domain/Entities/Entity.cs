using System.ComponentModel.DataAnnotations.Schema;

namespace Schedora.Domain.Entities;

public abstract class Entity : IEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public interface IEntity
{
    public long Id { get; set; }
}