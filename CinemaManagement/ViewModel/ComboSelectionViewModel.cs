using System.Collections.Generic;

namespace CinemaManagement.ViewModels
{
    public class ComboSelectionViewModel
    {
        public string OrderId { get; set; }
        public decimal TicketTotal { get; set; }
        public List<ComboItem> AvailableCombos { get; set; } = new List<ComboItem>();
        public List<SelectedCombo> SelectedCombos { get; set; } = new List<SelectedCombo>();
        public decimal ComboTotal { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class ComboItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }

    public class SelectedCombo
    {
        public int ComboId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}