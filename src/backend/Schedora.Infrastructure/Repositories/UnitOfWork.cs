using Schedora.Domain.Interfaces;
using Schedora.Infrastructure.Persistence;

namespace Schedora.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    public UnitOfWork(DataContext context, IGenericRepository genericRepository, IUserRepository userRepository)
    {
        _context = context;
        GenericRepository = genericRepository;
        UserRepository = userRepository;
    }

    public IGenericRepository GenericRepository { get; set; }
    public IUserRepository UserRepository { get; set; }


    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}