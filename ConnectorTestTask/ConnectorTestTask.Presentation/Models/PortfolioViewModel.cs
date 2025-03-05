using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConnectorTestTask.Presentation.Models
{
    public class PortfolioViewModel
    {
        [Required]
        public List<PortfolioItem> Portfolio { get; set; } = new();

        [Required]
        public string TargetCurrency { get; set; }

        public Dictionary<string, decimal>? ConversionResults { get; set; }

        [Required]
        public List<string> AvailableCurrencies { get; set; } = new();
    }

  
}