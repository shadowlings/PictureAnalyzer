using Microsoft.EntityFrameworkCore;

namespace PictureAnalyzer.Data
{
    public partial class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        public DbSet<Folder> Folders { get; set; }
        public DbSet<AuthorizedUser> AuthorizedUsers { get; set; }
        public DbSet<FolderItem> FolderItems { get; set; }
        public DbSet<ImageRatings> ImageRatings { get; set; }
        public DbSet<ImageTag> ImageTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
