using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Interfaces;
using Schedora.Infrastructure.Persistence;

namespace Schedora.Infrastructure.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(DataContext context) : base(context)
    {
    }

    public async Task<bool> UserByEmailExists(string email)
    {
        return await _context.Users.AnyAsync(d => d.Email == email && d.EmailConfirmed);
    }
}