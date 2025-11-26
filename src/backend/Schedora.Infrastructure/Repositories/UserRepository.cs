using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Entities;
using Schedora.Domain.Interfaces;
using Schedora.Infrastructure.Persistence;

namespace Schedora.Infrastructure.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(DataContext context) : base(context)
    {
    }

    public async Task<List<User>> GetUsersNotActiveByEmail(string email)
    {
        return await _context.Users
            .Where(d => d.Email == email && !d.EmailConfirmed && !d.IsActive)
            .ToListAsync();
    }

    public async Task<User?> UserByEmail(string email)
    {
        return await _context.Users.SingleOrDefaultAsync(d => d.Email == email);
    }

    public async Task<bool> UserByEmailExists(string email)
    {
        return await _context.Users.AnyAsync(d => d.Email == email && d.EmailConfirmed);
    }
}