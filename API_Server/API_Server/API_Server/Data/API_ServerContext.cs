using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API_Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace API_Server.Data
{
    public class API_ServerContext : IdentityDbContext<User>
    {
        public API_ServerContext (DbContextOptions<API_ServerContext> options)
            : base(options)
        {
        }

        public DbSet<API_Server.Models.Cart> Cart { get; set; } = default!;

        public DbSet<API_Server.Models.Comment> Comment { get; set; }

        public DbSet<API_Server.Models.Image> Image { get; set; }

        public DbSet<API_Server.Models.ImportInvoice> ImportInvoice { get; set; }

        public DbSet<API_Server.Models.ImportInvoiceDetail> ImportInvoiceDetail { get; set; }

        public DbSet<API_Server.Models.Invoice> Invoice { get; set; }

        public DbSet<API_Server.Models.InvoiceDetail> InvoiceDetail { get; set; }

        public DbSet<API_Server.Models.News> News { get; set; }

        public DbSet<API_Server.Models.Product> Product { get; set; }

        public DbSet<API_Server.Models.ProductType> ProductType { get; set; }

        public DbSet<API_Server.Models.ProductTypeDetail> ProductTypeDetail { get; set; }

        public DbSet<API_Server.Models.ProductVoucher> ProductVoucher { get; set; }

        public DbSet<API_Server.Models.Rating> Rating { get; set; }

        public DbSet<API_Server.Models.Slider> Slider { get; set; }

        public DbSet<API_Server.Models.Supplier> Supplier { get; set; }

        public DbSet<API_Server.Models.Voucher> Voucher { get; set; }

        public DbSet<API_Server.Models.User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImportInvoiceDetail>()
                .HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Bạn có thể thực hiện tương tự cho ImportInvoiceId nếu cần thiết


            modelBuilder.Entity<InvoiceDetail>()
               .HasOne(i => i.Product)
               .WithMany()
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.NoAction);

            // Bạn có thể thực hiện tương tự cho các quan hệ khóa ngoại khác nếu cần thiết

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<API_Server.Models.ProductVoucherDetail> ProductVoucherDetail { get; set; }

        public DbSet<API_Server.Models.UserVouchers> UserVouchers { get; set; }
    }
}
