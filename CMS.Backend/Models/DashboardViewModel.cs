using CMS.Data.Entities;
using System.Collections.Generic;

namespace CMS.Backend.Models
{
    public class DashboardViewModel
    {
        public int TotalPosts { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }

        public IEnumerable<Order> RecentOrders { get; set; }
        public IEnumerable<Post> RecentPosts { get; set; }
    }
}
