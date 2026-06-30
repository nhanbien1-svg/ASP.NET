using CMS.Data.Entities; // Chỉ giữ lại dòng chuẩn này
using Microsoft.EntityFrameworkCore;

namespace CMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ==========================================
        // 1. PHÂN HỆ TIN TỨC (CMS)
        // ==========================================
        public DbSet<CategoryPost> CategoryPosts { get; set; }
        public DbSet<Post> Posts { get; set; }

        // ==========================================
        // 2. PHÂN HỆ SẢN PHẨM (E-COMMERCE)
        // ==========================================
        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<Product> Products { get; set; }

        // ==========================================
        // 3. PHÂN HỆ KHÁCH HÀNG & ĐƠN HÀNG
        // ==========================================
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }

        // ==========================================
        // 4. PHÂN HỆ GIỎ HÀNG (CART)
        // ==========================================
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // ==========================================
        // 5. PHÂN HỆ HỆ THỐNG & TÀI KHOẢN
        // ==========================================
        public DbSet<User> Users { get; set; }

        // ==========================================
        // TỐI ƯU HÓA RÀNG BUỘC DATABASE (MỚI THÊM)
        // ==========================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc tính duy nhất: Không cho phép 2 tài khoản trùng Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // Ràng buộc tính duy nhất: Không cho phép 2 tài khoản trùng Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}