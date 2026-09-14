namespace ECommerce.WebUI.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; } // الأرباح للطلبات التي تم توصيلها
        public int PendingOrdersCount { get; set; } // الطلبات المعلقة (المطلوبة في Task 10)
    
}
}
