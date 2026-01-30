namespace Schedora.Domain.Interfaces;

public interface IPostRepository
{
    public Task<Post?> GetPostById(long postId);
}