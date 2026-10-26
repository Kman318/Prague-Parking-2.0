using System;
using System.Collections.Generic;
using System.Text;


namespace PragueParking.Data.Pricing;

public class PriceList
{
    public decimal CarHourlyRate { get; set; } = 20m;
    public decimal MotorcycleHourlyRate { get; set; } = 10m;
    public int FreeMinutes { get; set; } = 10;
}

