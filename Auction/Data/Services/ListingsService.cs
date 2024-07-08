using Auction.Models;
using Microsoft.EntityFrameworkCore;

namespace Auction.Data.Services
{
    public class ListingsService : IListingsService
    {
        private readonly ApplicationDbContext _context;

        public ListingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Add(Listing listing)
        {
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Listing> GetAll()
        {
            var applicationDbContext = _context.Listings.Include(l => l.User);
            return applicationDbContext;
        }

        public async Task<Listing> GetById(int? id)
        {
            // var listing = await _context.Listings
            return await _context.Listings
                 .Include(l => l.User)
                 .Include(l => l.Comments)
                  .Include(l => l.Bids)
                 .ThenInclude(b => b.User)
               // .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);

           
        }

        public async Task SaveChanges()
        {
           await _context.SaveChangesAsync();
        }
    }
}


//aluthen denm eka

//using Auction.Models;
//using Microsoft.EntityFrameworkCore;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Auction.Data.Services
//{
//    public class ListingsService : IListingsService
//    {
//        private readonly ApplicationDbContext _context;

//        public ListingsService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task Add(Listing listing)
//        {
//            _context.Listings.Add(listing);
//            await _context.SaveChangesAsync();
//        }

//        public IQueryable<Listing> GetAll()
//        {
//            return _context.Listings.Include(l => l.User);
//        }

//        public async Task<Listing> GetById(int? id)
//        {
//            return await _context.Listings
//                .Include(l => l.User)
//                .Include(l => l.Comments)
//                 .Include(l => l.BIds)
//                .FirstOrDefaultAsync(m => m.Id == id);
//        }
//    }
//}
