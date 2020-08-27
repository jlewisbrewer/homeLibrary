using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeLibrary.API.Dtos;
using HomeLibrary.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeLibrary.API.Data
{
    public class HomeLibraryRepository : IHomeLibraryRepository
    {
        private readonly DataContext _context;
        public HomeLibraryRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }


        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Book> GetBook(int id)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);

            return book;
        }

        public async Task<IEnumerable<Book>> GetBooks()
        {
            var books = await _context.Books
                .ToListAsync();

            return books;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            user.UserBooks = await _context.UserBooks
                .Include(ub => ub.Book)
                .Where(x => x.UserId == id)
                .ToListAsync();

            return user;
        }

        public async Task<IEnumerable<Book>> GetUserBooks(int id)
        {
            var booksIds = await _context.UserBooks
                .Where(x => x.UserId == id)
                .Select(x => x.BookId)
                .ToListAsync();

            var booksToReturn = await _context.Books
                .Where(x => booksIds.Contains(x.Id))
                .ToListAsync();

            return booksToReturn;
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> SearchForExistingBook(BookForSearchDto bookForSearchDto)
        {
            var id = -1;
            if (await _context.Books.AnyAsync(x => x.Isbn10 == bookForSearchDto.Isbn))
                id = (await _context.Books.FirstOrDefaultAsync(x => x.Isbn10 == bookForSearchDto.Isbn)).Id;
            
            if (await _context.Books.AnyAsync(x => x.Isbn13 == bookForSearchDto.Isbn))
                id = (await _context.Books.FirstOrDefaultAsync(x => x.Isbn13 == bookForSearchDto.Isbn)).Id;

            if (await _context.Books.AnyAsync(x => x.Title.ToLower() == bookForSearchDto.Title.ToLower() && x.Author.ToLower() == bookForSearchDto.Author.ToLower()))
                id = (await _context.Books.FirstOrDefaultAsync(x => x.Title.ToLower() == bookForSearchDto.Title.ToLower() && x.Author.ToLower() == bookForSearchDto.Author.ToLower())).Id;

            return id;
        }
    }
}