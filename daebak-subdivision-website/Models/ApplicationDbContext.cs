using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // ✅ Define DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Homeowner> Homeowners { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Announcement> ANNOUNCEMENTS { get; set; }
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
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<ForumCategory> ForumCategories { get; set; }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<ForumReport> ForumReports { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollResponse> PollResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ Explicit Table Mapping
            modelBuilder.Entity<User>().ToTable("USERS");
            modelBuilder.Entity<Homeowner>().ToTable("HOMEOWNERS");
            modelBuilder.Entity<Admin>().ToTable("ADMINS");
            modelBuilder.Entity<Staff>().ToTable("STAFF");
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
            modelBuilder.Entity<VehicleRegistration>().ToTable("VEHICLE_REGISTRATION");
            modelBuilder.Entity<Contact>().ToTable("CONTACTS");
            modelBuilder.Entity<Feedback>().ToTable("FEEDBACK");
            modelBuilder.Entity<ForumCategory>().ToTable("FORUM_CATEGORIES");
            modelBuilder.Entity<ForumThread>().ToTable("FORUM_THREADS");
            modelBuilder.Entity<ForumPost>().ToTable("FORUM_POSTS");
            modelBuilder.Entity<ForumReport>().ToTable("FORUM_REPORTS");
            modelBuilder.Entity<Poll>().ToTable("POLLS");
            modelBuilder.Entity<PollResponse>().ToTable("POLL_RESPONSES");

            // ✅ Explicit Column Mapping for Homeowner
            modelBuilder.Entity<Homeowner>()
                .Property(h => h.HomeownerId)
                .HasColumnName("HOMEOWNER_ID");

            modelBuilder.Entity<Homeowner>()
                .Property(h => h.UserId)
                .HasColumnName("USER_ID");

            modelBuilder.Entity<Homeowner>()
                .Property(h => h.HouseNumber)
                .HasColumnName("HOUSE_NUMBER");

            // ✅ Define Relationships
            modelBuilder.Entity<Homeowner>()
                .HasOne(h => h.User)
                .WithOne()
                .HasForeignKey<Homeowner>(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Admin>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Staff>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Staff>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(sr => sr.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Feedback>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Unique Constraints
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
