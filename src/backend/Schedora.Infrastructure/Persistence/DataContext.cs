using Schedora.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Entities;

namespace Schedora.Infrastructure.Persistence;

public class DataContext : IdentityDbContext<User, Role, long>
{
    public DataContext(DbContextOptions<DataContext> dbContext) : base(dbContext) {}
    
    
}