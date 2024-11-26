using Microsoft.EntityFrameworkCore;
using WebServer.Models;
using API_Library;

namespace WebServer.Data
{
    public class DBManager : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=clients.db");
            //ClearDatabase();
        }

        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        private void ClearDatabase()
        {
            // Ensure the database is created
            Database.EnsureCreated();

            // Clear the Clients table
            Clients.RemoveRange(Clients);
            SaveChanges();
        }
    }
}
