using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BevoBnB.ViewModels
{
  
        public class EarningsViewModel
        {
            public decimal TotalEarnings { get; set; }
            public decimal MonthlyEarnings { get; set; }
            public decimal LastMonthEarnings { get; set; }
            public decimal EarningsChange { get; set; }
            public int TotalBookings { get; set; }
            public int CompletedBookings { get; set; }
            public IEnumerable<PropertyEarningsViewModel> PropertiesEarnings { get; set; } = new List<PropertyEarningsViewModel>();
            public decimal StayRevenue => TotalEarnings * 0.9m;
            public decimal CleaningFees => TotalEarnings * 0.1m;
        }

        public class PropertyEarningsViewModel
        {
            public int PropertyID { get; set; }
            public string PropertyNumber { get; set; }
            public decimal TotalEarnings { get; set; }
            public int CompletedBookings { get; set; }
        }


}