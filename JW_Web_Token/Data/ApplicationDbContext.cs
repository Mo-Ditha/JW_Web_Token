using JW_Web_Token.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace JW_Web_Token.Data
{
    // Represents the database context for the application
    public class ApplicationDbContext : DbContext
    {
        // Constructor accepting DbContextOptions for configuring the context
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Represents a DbSet for the User model, allowing interaction with the 'Users' table in the database
        public DbSet<User> Users { get; set; }
    }
}
