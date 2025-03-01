using ConnectorTestTask.Core.Interfaces;
using ConnectorTestTask.Presentation.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConnectorTestTask.Presentation.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ITestConnector _connector;

        public PortfolioController(ITestConnector connector)
        {
            _connector = connector;
        }

        public async Task<IActionResult> Index()
        {
            var availableCurrencies = await _connector.GetAvailableCurrenciesAsync();
            return View(new PortfolioViewModel { AvailableCurrencies = availableCurrencies });
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(PortfolioViewModel model)
        {
            if (!model.Portfolio.Any())
            {
                return Json(new { success = false, message = "Добавьте хотя бы одну валюту." });
            }

            if (!model.AvailableCurrencies.Contains(model.TargetCurrency))
            {
                return Json(new { success = false, message = "Выбрана неподдерживаемая валюта." });
            }

            var result = await _connector.CalculatePortfolioValueAsync(model.Portfolio, model.TargetCurrency);
            return Json(new { success = true, data = result, targetCurrency = model.TargetCurrency });
        }
    }
}