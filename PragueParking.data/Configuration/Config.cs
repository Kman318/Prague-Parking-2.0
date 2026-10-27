using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PragueParking.data.Configuration;

public class Config
{
    public ParkingSettings Parking { get; set; } = new();
    public PricingSettings Pricing { get; set; } = new();
    public DisplaySettings Display { get; set; } = new();

    public static Config Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Configuration file was not found: {filePath}");
        }

        string json = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<Config>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException(
                "Could not load the configuration file.");
    }
}

public class ParkingSettings
{
    public int TotalSpots { get; set; } = 100;
    public int MotorcyclesPerSpot { get; set; } = 2;
}

public class PricingSettings
{
    public decimal CarHourlyRate { get; set; } = 20;
    public decimal MotorcycleHourlyRate { get; set; } = 10;
    public int FreeMinutes { get; set; } = 10;
}

public class DisplaySettings
{
    public int MapColumns { get; set; } = 10;
    public bool ShowParkingMap { get; set; } = true;
}

