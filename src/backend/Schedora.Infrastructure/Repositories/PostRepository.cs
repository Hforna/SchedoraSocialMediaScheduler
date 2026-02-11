using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Entities;

namespace Schedora.Infrastructure.Repositories;

public class PostRepository : BaseRepository, IPostRepository
{
    public PostRepository(DataContext context) : base(context)
    {
    }

    public async Task<Post?> GetPostById(long postId)
    {
        return await _context.Posts.Include(d => d.Platforms)
            .SingleOrDefaultAsync(d => d.Id == postId);
    }

    public async Task<PostValidation?> GetPostValidationByPost(long postId)
    {
        return await _context.PostValidations.SingleOrDefaultAsync(d => d.PostId == postId);
    }
}