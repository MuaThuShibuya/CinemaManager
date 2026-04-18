using CinemaManagement.Models;
using System.Collections.Generic;

namespace CinemaManagement.ViewModels
{
    public class TheaterCityGroupViewModel
    {
        public string City { get; set; }
        public List<Theater> Theaters { get; set; }
    }
}
