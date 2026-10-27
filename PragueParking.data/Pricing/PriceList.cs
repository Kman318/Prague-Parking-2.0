
using System;
using System.IO;

namespace PragueParking.Data.Pricing;

public class PriceList
{
    public decimal CarHourlyRate { get; set; }
    public decimal MotorcycleHourlyRate { get; set; }
    public int FreeMinutes { get; set; }

    public PriceList()
    {
        CarHourlyRate = 20m;
        MotorcycleHourlyRate = 10m;
        FreeMinutes = 10;
    }

    public PriceList(decimal carHourlyRate, decimal motorcycleHourlyRate, int freeMinutes)
    {
        CarHourlyRate = carHourlyRate;
        MotorcycleHourlyRate = motorcycleHourlyRate;
        FreeMinutes = freeMinutes;
    }

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Price list file was not found.", filePath);
        }

        foreach (string line in File.ReadAllLines(filePath))
        {
            string trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            string[] parts = trimmedLine.Split('=', 2);

            if (parts.Length != 2)
            {
                continue;
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            switch (key)
            {
                case "CarHourlyRate":
                    CarHourlyRate = decimal.Parse(value);
                    break;

                case "MotorcycleHourlyRate":
                    MotorcycleHourlyRate = decimal.Parse(value);
                    break;

                case "FreeMinutes":
                    FreeMinutes = int.Parse(value);
                    break;
            }
        }
    }
}
