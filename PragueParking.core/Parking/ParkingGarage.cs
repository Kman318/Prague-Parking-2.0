using System;
using System.Collections.Generic;
using System.Text;

namespace PragueParking.Core;

public class ParkingGarage
{
    public List<ParkingSpot> ParkingSpots { get; } = new();
    public List<ParkingTicket> ParkingTickets { get; } = new();
    public int MotorcyclesPerSpot { get; }

    public ParkingGarage(
    int numberOfSpots = 100,
    int motorcyclesPerSpot = 2)
    {
        if (numberOfSpots <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfSpots),
                "The number of parking spots must be greater than zero.");
        }

        if (motorcyclesPerSpot <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(motorcyclesPerSpot),
                "Motorcycles per spot must be greater than zero.");
        }

        MotorcyclesPerSpot = motorcyclesPerSpot;

        for (int i = 1; i <= numberOfSpots; i++)
        {
            ParkingSpots.Add(new ParkingSpot(i));
        }
    }

    public bool ParkVehicle(Vehicle vehicle)
    {
        if (string.IsNullOrWhiteSpace(vehicle.RegistrationNumber))
        {
            return false;
        }
        if (FindVehicle(vehicle.RegistrationNumber) is not null)
        {
            return false;
        }
        foreach (ParkingSpot spot in ParkingSpots)
        {
            if (CanParkVehicle(spot, vehicle))
            {
                spot.ParkedVehicles.Add(vehicle);

                ParkingTickets.Add(new ParkingTicket(
                        vehicle.RegistrationNumber,
                        spot.SpotNumber,
                        vehicle.ArrivalTime));

                return true;
            }
        }

        return false;
    }

    private bool CanParkVehicle(ParkingSpot spot, Vehicle vehicle)
    {
        if (spot.ParkedVehicles.Count == 0)
        {
            return true;
        }

        if (spot.ParkedVehicles.Any(v => v is Car))
        {
            return false;
        }

        if (vehicle is Car)
        {
            return false;
        }

        if (vehicle is Motorcycle && spot.ParkedVehicles.Count < MotorcyclesPerSpot)
        {
            return true;
        }

        return false;
    }
    public bool RemoveVehicle(string registrationNumber)
    {
        foreach (ParkingSpot spot in ParkingSpots)
        {
            Vehicle? vehicle = spot.ParkedVehicles.FirstOrDefault(v => v.RegistrationNumber.Equals(
                        registrationNumber, StringComparison.OrdinalIgnoreCase));

            if (vehicle is not null)
            {
                ParkingTicket? ticket = ParkingTickets.LastOrDefault(t =>
                        t.RegistrationNumber.Equals(vehicle.RegistrationNumber,
                            StringComparison.OrdinalIgnoreCase) && t.DepartureTime is null);

                ticket?.RegisterDeparture(DateTimeOffset.Now);

                spot.ParkedVehicles.Remove(vehicle);
                return true;
            }
        }

        return false;
    
    }

    public void ClearGarage()
    {
        foreach (ParkingSpot spot in ParkingSpots)
        {
            spot.ParkedVehicles.Clear();
        }

        ParkingTickets.Clear();
    }


    public bool MoveVehicle(string registrationNumber, int targetSpotNumber)
    {
        ParkingSpot? sourceSpot = FindParkingSpot(registrationNumber);

        if (sourceSpot is null)
        {
            return false;
        }

        ParkingSpot? targetSpot = ParkingSpots.FirstOrDefault(s => s.SpotNumber == targetSpotNumber);

        if (targetSpot is null || sourceSpot == targetSpot)
        {
            return false;
        }

        Vehicle? vehicle = sourceSpot.ParkedVehicles
            .FirstOrDefault(v => v.RegistrationNumber.Equals(
                registrationNumber,
                StringComparison.OrdinalIgnoreCase));

        if (vehicle is null || !CanParkVehicle(targetSpot, vehicle))
        {
            return false;
        }

        sourceSpot.ParkedVehicles.Remove(vehicle);
        targetSpot.ParkedVehicles.Add(vehicle);

        ParkingTicket? ticket = ParkingTickets.LastOrDefault(t =>
            t.RegistrationNumber.Equals(
                vehicle.RegistrationNumber,
                StringComparison.OrdinalIgnoreCase) && t.DepartureTime is null);

        ticket?.UpdateSpotNumber(targetSpot.SpotNumber);

        return true;
    }


    public Vehicle? FindVehicle(string registrationNumber)
    {
        foreach (ParkingSpot spot in ParkingSpots)
        {
            Vehicle? vehicle = spot.ParkedVehicles.FirstOrDefault(v =>
                    v.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase));

            if (vehicle is not null)
            {
                return vehicle;
            }
        }

        return null;
    }
    public ParkingSpot? FindParkingSpot(string registrationNumber)
    {
        foreach (ParkingSpot spot in ParkingSpots)
        {
            bool found = spot.ParkedVehicles.Any(v =>
                v.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase));

            if (found)
            {
                return spot;
            }
        }

        return null;
    }
}
