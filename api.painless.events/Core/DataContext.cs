using api.painless.events.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.painless.events.Core
{
    public class WriteContext : DbContext
    {

        protected readonly IConfiguration _configuration;

        public WriteContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                var connectionString = _configuration.GetSection("ConnectionStrings:WriteConnection").Value;
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), null);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }


        public DbSet<Account> Accounts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Entities.File> Files { get; set; }
        public DbSet<FileDownload> FileDownloads { get; set; }

    }




    public class ReadContext : DbContext
    {

        protected readonly IConfiguration _configuration;

        public ReadContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                var connectionString = _configuration.GetSection("ConnectionStrings:WriteConnection").Value;
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), null);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }


        public DbSet<Account> Accounts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Entities.File> Files { get; set; }
        public DbSet<FileDownload> FileDownloads { get; set; }



    }







}
