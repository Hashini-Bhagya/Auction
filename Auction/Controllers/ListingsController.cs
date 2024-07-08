using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Auction.Data;
using Auction.Models;
using Auction.Data.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;


namespace Auction.Controllers
{
    public class ListingsController : Controller
    {
        private readonly IListingsService _listingsService;
        private readonly IBidsService _bidsService;
        private readonly ICommentService _commentService;
        private readonly IWebHostEnvironment _webHostEnvironment;
       

        public ListingsController(IListingsService listingsService, IWebHostEnvironment webHostEnvironment, IBidsService bidsService, ICommentService commentService)
        {
            _listingsService = listingsService;
            _webHostEnvironment = webHostEnvironment;
            _bidsService = bidsService;
            _commentService = commentService;
        }



        // GET: Listings
        public async Task<IActionResult> Index(int? pageNumber, string searchString)
        {
            var applicationDbContext = _listingsService.GetAll();

            int pageSize = 3;
            if (!string.IsNullOrEmpty(searchString))
            {
                applicationDbContext = applicationDbContext.Where(a => a.Title.Contains(searchString));
                return View(await PaginatedList<Listing>.CreateAsync(applicationDbContext.Where(l => l.IsSold == false).AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<Listing>.CreateAsync(applicationDbContext/*.Where(l => l.IsSold == false)*/.AsNoTracking(), pageNumber ?? 1, pageSize));
        }



        public async Task<IActionResult> MyListings(int? pageNumber)
        {
            var applicationDbContext = _listingsService.GetAll();

            int pageSize = 3;
           

            return View("Index", await PaginatedList<Listing>.CreateAsync(applicationDbContext.Where(l => l.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> MyBids(int? pageNumber)
        {
            var applicationDbContext = _bidsService.GetAll();

            int pageSize = 3;


            return View(await PaginatedList<Bid>.CreateAsync(applicationDbContext.Where(l => l.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).AsNoTracking(), pageNumber ?? 1, pageSize));
        }





        // GET: Listings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _listingsService.GetById(id);
            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }



        // GET: Listings/Create
        public IActionResult Create()
        {
            return View();
        }

        //POST: Listings/Create
        //To protect from overposting attacks, enable the specific properties you want to bind to.
        //For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.


        //aluth eka

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ListingVM listing)
        {
            if (ModelState.IsValid)
            {
                string relativePath = null;

                if (listing.Image != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                    string fileName = Path.GetFileName(listing.Image.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await listing.Image.CopyToAsync(fileStream);
                    }

                    // Store the relative path to be used in the view
                    relativePath = Path.Combine("Images", fileName);
                }

                var listObj = new Listing
                {
                    Title = listing.Title,
                    Description = listing.Description,
                    Price = listing.Price,
                    ImagePath = relativePath,
                    IdentityUserId = listing.IdentityUserId
                };

                await _listingsService.Add(listObj);
                return RedirectToAction(nameof(Index));
            }
            return View(listing);
        }

        [HttpPost]
        public async Task<ActionResult> AddBid([Bind("Id,Price,ListingId,IdentityUserId")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                await _bidsService.Add(bid);
            }
            var listing = await _listingsService.GetById(bid.ListingId);
            listing.Price = bid.Price;
            await _listingsService.SaveChanges();

            return View("Details", listing);
        }
        public async Task<ActionResult> CloseBidding(int id)
        {
            var listing = await _listingsService.GetById(id);
            listing.IsSold = true;
            await _listingsService.SaveChanges();
            return View("Details", listing);
        }

        //[HttpPost]
        //public async Task<IActionResult> AddBid([Bind("Price,ListingId")] Bid bid)
        //{
        //    // Validate the bid price
        //    if (bid.Price <= 0)
        //    {
        //        ModelState.AddModelError("Price", "Bid price must be greater than zero.");
        //    }

        //    var listing = await _listingsService.GetById(bid.ListingId);

        //    if (listing == null)
        //    {
        //        return NotFound();
        //    }

        //    // Validate bid price against the current price
        //    if (bid.Price <= listing.Price)
        //    {
        //        ModelState.AddModelError("Price", "Bid price must be higher than the current price.");
        //    }

        //    // Add user identity to bid
        //    bid.IdentityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (ModelState.IsValid)
        //    {
        //        await _bidsService.Add(bid);

        //        listing.Price = bid.Price; // Update the listing's current price
        //        await _listingsService.SaveChanges();

        //        return RedirectToAction("Details", new { id = listing.Id });
        //    }

        //    // If the model state is invalid, reload the listing details with validation errors
        //    return View("Details", listing);
        //}

        [HttpPost]
        public async Task<ActionResult> AddComment([Bind("Id, Content, ListingId, IdentityUserId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                await _commentService.Add(comment);
            }
            var listing = await _listingsService.GetById(comment.ListingId);
            return View("Details", listing);
        }

        //[HttpPost]
        //public async Task<ActionResult> AddBid([Bind("Id, Price, ListingId, IdentityUserId")] Bid bid)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //await _bidsService.Add(bid);

        //        return BadRequest(ModelState);

        //    }
        //    var listing = await _listingsService.GetById(bid.ListingId);

        //    if (listing == null)
        //    {
        //        return NotFound(); // Or handle accordingly
        //    }

        //    //new
        //    await _bidsService.Add(bid);

        //    listing.Price = bid.Price;
        //    await _listingsService.SaveChanges();

        //    //   return View("Details", listing);

        //    return RedirectToAction("Details", new { id = listing.Id });
        //}
        //public async Task<ActionResult> CloseBidding(int id)
        //{
        //    var listing = await _listingsService.GetById(id);

        //    if (listing == null)
        //    {
        //        return NotFound();
        //    }

        //    listing.IsSold = true;
        //    await _listingsService.SaveChanges();
        //    return View("Details", listing);
        //}


    }


}



//// GET: Listings/Edit/5
//public async Task<IActionResult> Edit(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var listing = await _context.Listings.FindAsync(id);
//    if (listing == null)
//    {
//        return NotFound();
//    }
//    ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", listing.IdentityUserId);
//    return View(listing);
//}

//// POST: Listings/Edit/5
//// To protect from overposting attacks, enable the specific properties you want to bind to.
//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//[HttpPost]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,ImagePath,IsSold,IdentityUserId")] Listing listing)
//{
//    if (id != listing.Id)
//    {
//        return NotFound();
//    }

//    if (ModelState.IsValid)
//    {
//        try
//        {
//            _context.Update(listing);
//            await _context.SaveChangesAsync();
//        }
//        catch (DbUpdateConcurrencyException)
//        {
//            if (!ListingExists(listing.Id))
//            {
//                return NotFound();
//            }
//            else
//            {
//                throw;
//            }
//        }
//        return RedirectToAction(nameof(Index));
//    }
//    ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", listing.IdentityUserId);
//    return View(listing);
//}

//// GET: Listings/Delete/5
//public async Task<IActionResult> Delete(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var listing = await _context.Listings
//        .Include(l => l.User)
//        .FirstOrDefaultAsync(m => m.Id == id);
//    if (listing == null)
//    {
//        return NotFound();
//    }

//    return View(listing);
//}

//// POST: Listings/Delete/5
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var listing = await _context.Listings.FindAsync(id);
//    if (listing != null)
//    {
//        _context.Listings.Remove(listing);
//    }

//    await _context.SaveChangesAsync();
//    return RedirectToAction(nameof(Index));
//}

//private bool ListingExists(int id)
//{
//    return _context.Listings.Any(e => e.Id == id);
//}

//}
//}