namespace CinemaManagement.ViewModels
{
    public class PaymentViewModel
    {
        public string OrderId { get; set; }
        public decimal TicketTotal { get; set; }
        public decimal ComboTotal { get; set; }
        public decimal GrandTotal { get; set; }
    }
}