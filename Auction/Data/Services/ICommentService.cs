using Auction.Models;

namespace Auction.Data.Services
{
    public interface ICommentService
    {
        Task Add(Comment comment);
    }
}
