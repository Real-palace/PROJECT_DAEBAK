using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace daebak_subdivision_website.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<BillingItem> BillingItems { get; set; }
        public DbSet<UserBill> UserBills { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<FacilityReservation> FacilityReservations { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<VisitorPass> VisitorPasses { get; set; }
        public DbSet<VehicleRegistration> VehicleRegistrations { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; } // ✅ FIXED: Ensure correct plural naming
        public DbSet<ForumCategory> ForumCategories { get; set; }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<ForumReport> ForumReports { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollResponse> PollResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map Entities to Database Table Names
            modelBuilder.Entity<User>().ToTable("USERS");
            modelBuilder.Entity<Announcement>().ToTable("ANNOUNCEMENTS");
            modelBuilder.Entity<Event>().ToTable("EVENTS");
            modelBuilder.Entity<BillingItem>().ToTable("BILLING_ITEMS");
            modelBuilder.Entity<UserBill>().ToTable("USER_BILLS");
            modelBuilder.Entity<Payment>().ToTable("PAYMENTS");
            modelBuilder.Entity<Facility>().ToTable("FACILITIES");
            modelBuilder.Entity<FacilityReservation>().ToTable("FACILITY_RESERVATIONS");
            modelBuilder.Entity<ServiceRequest>().ToTable("SERVICE_REQUESTS");
            modelBuilder.Entity<Document>().ToTable("DOCUMENTS");
            modelBuilder.Entity<VisitorPass>().ToTable("VISITOR_PASSES");
            modelBuilder.Entity<VehicleRegistration>().ToTable("VEHICLE_REGISTRATIONS");
            modelBuilder.Entity<Contact>().ToTable("CONTACTS");
            modelBuilder.Entity<Feedback>().ToTable("FEEDBACK"); // ✅ FIXED: Correct table mapping
            modelBuilder.Entity<ForumCategory>().ToTable("FORUM_CATEGORIES");
            modelBuilder.Entity<ForumThread>().ToTable("FORUM_THREADS");
            modelBuilder.Entity<ForumPost>().ToTable("FORUM_POSTS");
            modelBuilder.Entity<ForumReport>().ToTable("FORUM_REPORTS");
            modelBuilder.Entity<Poll>().ToTable("POLLS");
            modelBuilder.Entity<PollResponse>().ToTable("POLL_RESPONSES");

            // Ensure Username and Email are Unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
