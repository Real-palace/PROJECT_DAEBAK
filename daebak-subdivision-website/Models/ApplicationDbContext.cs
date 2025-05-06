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
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<FeedbackResponse> FeedbackResponses { get; set; }
        public DbSet<SecurityAlert> SecurityAlerts { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<UserEmergencyContact> UserEmergencyContacts { get; set; }

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
            modelBuilder.Entity<Feedback>().ToTable("FEEDBACK");
            modelBuilder.Entity<FeedbackResponse>().ToTable("FEEDBACK_RESPONSES");
            modelBuilder.Entity<SecurityAlert>().ToTable("SECURITY_ALERTS");
            modelBuilder.Entity<EmergencyContact>().ToTable("EMERGENCY_CONTACTS");
            modelBuilder.Entity<UserEmergencyContact>().ToTable("USER_EMERGENCY_CONTACTS");

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
                .WithOne(u => u.Homeowner)
                .HasForeignKey<Homeowner>(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Admin>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Fix Staff-User relationship by specifying both navigation properties
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne(u => u.Staff)
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
            
            // Add relationship for FeedbackResponse
            modelBuilder.Entity<FeedbackResponse>()
                .HasOne(fr => fr.Feedback)
                .WithMany()
                .HasForeignKey(fr => fr.FeedbackId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Security related relationships
            modelBuilder.Entity<VisitorPass>()
                .HasOne(v => v.RequestedBy)
                .WithMany()
                .HasForeignKey(v => v.RequestedById)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<VehicleRegistration>()
                .HasOne(v => v.Owner)
                .WithMany()
                .HasForeignKey(v => v.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<SecurityAlert>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<UserEmergencyContact>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
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
