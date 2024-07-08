using Auction.Models;

namespace Auction.Data.Services
{
    public interface IBidsService
    {
        Task Add(Bid bid);
        IQueryable<Bid> GetAll();
    }
}


//using Auction.Models;


//namespace Auctions.Data.Services
//{
//    public interface IBidsService
//    {
//        Task Add(Bid bid);
//    }
//}