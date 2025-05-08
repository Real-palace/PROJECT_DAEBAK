using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet definitions
        public DbSet<User> Users { get; set; }
        public DbSet<Homeowner> Homeowners { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Staff> Staff { get; set; }
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
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<FeedbackResponse> FeedbackResponses { get; set; }
        public DbSet<SecurityAlert> SecurityAlerts { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<UserEmergencyContact> UserEmergencyContacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Homeowner entity
            modelBuilder.Entity<Homeowner>(entity =>
            {
                entity.ToTable("HOMEOWNERS");
                entity.HasKey(e => e.HomeownerId);
                entity.Property(e => e.HomeownerId).HasColumnName("HOMEOWNER_ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.HouseNumber).HasColumnName("HOUSE_NUMBER").HasMaxLength(10).IsRequired(false);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Homeowner)
                    .HasForeignKey<Homeowner>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Feedback entity
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("FEEDBACK");
                entity.HasKey(e => e.FeedbackId);
                entity.Property(e => e.FeedbackId).HasColumnName("FEEDBACK_ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.HouseNumber).HasColumnName("HOUSE_NUMBER").HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.FeedbackType).HasColumnName("FEEDBACK_TYPE").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION").IsRequired();
                entity.Property(e => e.Status).HasColumnName("STATUS").HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Table Mappings
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
            modelBuilder.Entity<Feedback>().ToTable("FEEDBACK");
            modelBuilder.Entity<FeedbackResponse>().ToTable("FEEDBACK_RESPONSES");
            modelBuilder.Entity<SecurityAlert>().ToTable("SECURITY_ALERTS");
            modelBuilder.Entity<EmergencyContact>().ToTable("EMERGENCY_CONTACTS");
            modelBuilder.Entity<UserEmergencyContact>().ToTable("USER_EMERGENCY_CONTACTS");
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
