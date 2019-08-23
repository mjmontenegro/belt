using Microsoft.EntityFrameworkCore;
 
namespace belt.Models
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options) { }
        // users table represented below
        public DbSet<User> Users {get;set;}
        public DbSet<Auction> Auctions {get;set;}
        // public DbSet<RSVP> RSVPs {get;set;}
    }
}