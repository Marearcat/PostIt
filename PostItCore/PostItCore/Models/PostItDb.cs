using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace PostItCore.Models
{
    public class PostItDb : IdentityDbContext<User>
    {
        public PostItDb(DbContextOptions<PostItDb> options)
            : base(options)
        {
        }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Mail> Messages { get; set; }
        public DbSet<Subscribe> Subscribes { get; set; }
        public DbSet<Favor> Favors { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
