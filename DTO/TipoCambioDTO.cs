using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_ecommerce_deportivo_net.DTO
{
    public class TipoCambioDTO
    {
        public int Cantidad { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
    }
}