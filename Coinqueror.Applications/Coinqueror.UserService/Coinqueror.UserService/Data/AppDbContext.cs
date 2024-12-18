using Coinqueror.UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace Coinqueror.UserService.Data
{

    public class AppDbContext : DbContext
    {
        //Eklediğimiz AppDbContext sınıfı DbContext sınıfından kalıtım alıyor
        //Ayrıca kendini başlatıyor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Define the Users DbSet
        public DbSet<UserModel> Users { get; set; }

        // Override OnModelCreating to specify table name and any configurations
        //OnModelCreating'i override ederek tablo adını ve herhangi bir konfigürasyonu belirtebiliriz
        //OnModelCreating otomatik olarak (Start Methodu gibi) çağırılır ve yürütülür.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToTable("Users");
            // Additional configurations for UserModel (if any) can go here
        }
    }
}
