namespace CinemaManagement.ViewModels
{
    public class RevenueStatsViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<RevenueByMovie> ByMovie { get; set; }
        public List<RevenueByTheater> ByTheater { get; set; }
        public List<RevenueByShowtime> ByShowtime { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalTickets { get; set; }

        public string PaymentMethod { get; set; } // "All", "Momo", "Cash"
    }

    public class RevenueByMovie
    {
        public string MovieTitle { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenueByTheater
    {
        public string TheaterName { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenueByShowtime
    {
        public string MovieTitle { get; set; }
        public string TheaterName { get; set; }
        public DateTime Showtime { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
    }
}