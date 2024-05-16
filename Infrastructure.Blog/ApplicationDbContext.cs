

 
using Domain.Blog.entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
 


namespace Infrastructure.Blog
{
    public class ApplicationDBContext :IdentityDbContext<User>
    {
        public ApplicationDBContext() : base() { }

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }
        //defining dbs

        public DbSet<Blogs> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<React> Reacts { get; set; }
          public DbSet<Notification> Notifications { get; set; }
        


        //db configuration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(ConnectionString);
            }
        } 
        
        
    /*    protected override void OnModelCreating( ModelBuilder builder )
        {
            base.OnModelCreating( builder );
            seedRoles(builder);
        }*/



        //Seed roles into the database
     /*   private static void seedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData
                (
         new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
         new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" }
         );
        }*/


        //connection strings
        private string ConnectionString
        {
            get
            {
                return "Host=localhost; Database=Blogging; Port=5432; User Id=postgres; Password=admin";
            }
        }
    }
}
