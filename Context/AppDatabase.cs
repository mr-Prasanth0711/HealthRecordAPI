using HealthRecordAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace HealthRecordAPI.Context
{
    public class AppDatabase : DbContext
    {
        public AppDatabase(DbContextOptions<AppDatabase> options) : base(options)
        {

        }

        public DbSet<RegisterModel> Users { get; set; } 
       public DbSet<BasicDetailModel> Details { get; set; }
        public DbSet<GetDetailsModel> getDetails { get; set; }

        public DbSet<RecordModel> Record { get; set; }
        public object PostedFile { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            

        } 
    }
}
