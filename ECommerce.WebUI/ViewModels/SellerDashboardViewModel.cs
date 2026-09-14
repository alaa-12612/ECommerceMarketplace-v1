namespace ECommerce.WebUI.ViewModels
{
    public class SellerDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }     // 👈 تم إضافة عدد الطلبات لتغطية المتطلب
        public int TotalItemsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
