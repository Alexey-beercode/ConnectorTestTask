using System.ComponentModel.DataAnnotations;

namespace ConnectorTestTask.Presentation.Models
{
    public class PortfolioViewModel
    {
        [Required]
        public Dictionary<string, decimal> Portfolio { get; set; } = new();

        [Required]
        public string TargetCurrency { get; set; }

        public Dictionary<string, decimal>? ConversionResults { get; set; }

        public HashSet<string> AvailableCurrencies { get; set; } = new();
    }
}