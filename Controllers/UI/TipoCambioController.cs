using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using proyecto_ecommerce_deportivo_net.Integrations;
using proyecto_ecommerce_deportivo_net.Models;
using proyecto_ecommerce_deportivo_net.DTO;
using proyecto_ecommerce_deportivo_net.Data;

namespace proyecto_ecommerce_deportivo_net.Controllers.UI
{

    public class TipoCambioController : Controller
    {
        private readonly ILogger<TipoCambioController> _logger;
        private readonly CurrencyExchangeApiIntegration _currency;

        public TipoCambioController(ILogger<TipoCambioController> logger,
        CurrencyExchangeApiIntegration currency)
        {
            _logger = logger;
            _currency = currency;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Exchange(TipoCambioDTO? tipoCambio)
        {
            double rate = await _currency.GetExchangeRate(tipoCambio.From, tipoCambio.To);
            var cambio = tipoCambio.Cantidad * rate;
            ViewData["From"] = tipoCambio.From;
            ViewData["To"] = tipoCambio.To;
            ViewData["rate"] = rate;
            ViewData["cambio"] = cambio;
            return View("Index");
        }

       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}