using System.Collections.Generic;
using CinemaManagement.Models;

namespace CinemaManagement.ViewModels
{
    public class PaymentSuccessViewModel
    {
        public List<Ticket> Tickets { get; set; }
        public string QrCodeBase64 { get; set; }
        public bool IsPaid { get; set; }
        public string OrderId { get; set; }
        public long TotalAmount { get; set; }
    }
}
