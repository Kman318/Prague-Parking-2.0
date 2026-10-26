using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using PragueParking.Core;

namespace PragueParking.Data.Storage;

public class FileRepo
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public FileRepo()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PragueParking");

        Directory.CreateDirectory(directory);

        _filePath = Path.Combine(directory, "parking-data.json");
    }

    public void Save(ParkingGarage garage)
    {
        var data = new ParkData();

        foreach (var spot in garage.ParkingSpots)
        {
            foreach (var vehicle in spot.ParkedVehicles)
            {
                data.Vehicles.Add(new SavedVehicle
                {
                    VehicleType = vehicle switch
                    {
                        Car => "Car",
                        Motorcycle => "Motorcycle",
                        _ => throw new NotSupportedException(
                            $"Unsupported vehicle type: {vehicle.GetType().Name}")
                    },
                    RegistrationNumber = vehicle.RegistrationNumber,
                    ArrivalTime = vehicle.ArrivalTime,
                    SpotNumber = spot.SpotNumber
                });
            }
        }

        foreach (var ticket in garage.ParkingTickets)
        {
            data.Tickets.Add(new SavedTicket
            {
                RegistrationNumber = ticket.RegistrationNumber,
                SpotNumber = ticket.SpotNumber,
                ArrivalTime = ticket.ArrivalTime,
                DepartureTime = ticket.DepartureTime
            });
        }

        string json = JsonSerializer.Serialize(data, _jsonOptions);

        File.WriteAllText(_filePath, json);
    }

    public void Load(ParkingGarage garage)
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        string json = File.ReadAllText(_filePath);

        var data = JsonSerializer.Deserialize<ParkData>(json, _jsonOptions);

        if (data is null)
        {
            throw new InvalidDataException(
                "The saved parking data could not be read.");
        }

        foreach (var spot in garage.ParkingSpots)
        {
            spot.ParkedVehicles.Clear();
        }

        garage.ParkingTickets.Clear();

        foreach (var savedVehicle in data.Vehicles)
        {
            Vehicle vehicle = savedVehicle.VehicleType switch
            {
                "Car" => new Car(
                    savedVehicle.RegistrationNumber,
                    savedVehicle.ArrivalTime),

                "Motorcycle" => new Motorcycle(
                    savedVehicle.RegistrationNumber,
                    savedVehicle.ArrivalTime),

                _ => throw new InvalidDataException(
                    $"Unknown vehicle type: {savedVehicle.VehicleType}")
            };

            var spot = garage.ParkingSpots.FirstOrDefault(
                s => s.SpotNumber == savedVehicle.SpotNumber);

            if (spot is null)
            {
                throw new InvalidDataException(
                    $"Parking spot {savedVehicle.SpotNumber} does not exist.");
            }

            spot.ParkedVehicles.Add(vehicle);
        }

        foreach (var savedTicket in data.Tickets)
        {
            var ticket = new ParkingTicket(
                savedTicket.RegistrationNumber,
                savedTicket.SpotNumber,
                savedTicket.ArrivalTime);

            if (savedTicket.DepartureTime is DateTimeOffset departureTime)
            {
                ticket.RegisterDeparture(departureTime);
            }

            garage.ParkingTickets.Add(ticket);
        }
    }
}

