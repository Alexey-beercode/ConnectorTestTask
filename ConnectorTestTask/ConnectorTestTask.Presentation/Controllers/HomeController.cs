using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ConnectorTestTask.Presentation.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            // Получение исключения из HttpContext
            var exception = HttpContext.Items["Exception"] as Exception;

            // Передача данных об ошибке в представление
            ViewBag.ErrorMessage = exception?.Message;
            ViewBag.StackTrace = exception?.StackTrace;

            return View();
        }
    }
}