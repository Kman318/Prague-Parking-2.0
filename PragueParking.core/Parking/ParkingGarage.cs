using System;
using System.Collections.Generic;
using System.Text;




namespace PragueParking.Core;

public class ParkingGarage
{
    public List<ParkingSpot> ParkingSpots { get; } = new();
    public List<ParkingTicket> ParkingTickets { get; } = new();

    public ParkingGarage(int numberOfSpots = 100)
    {
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

                ParkingTickets.Add(
                    new ParkingTicket(
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

        if (vehicle is Motorcycle && spot.ParkedVehicles.Count < 2)
        {
            return true;
        }

        return false;
    }
    public bool RemoveVehicle(string registrationNumber)
    {
        foreach (ParkingSpot spot in ParkingSpots)
        {
            Vehicle? vehicle = spot.ParkedVehicles
                .FirstOrDefault(v =>
                    v.RegistrationNumber.Equals(
                        registrationNumber,
                        StringComparison.OrdinalIgnoreCase));

            if (vehicle is not null)
            {
                ParkingTicket? ticket = ParkingTickets
                    .LastOrDefault(t =>
                        t.RegistrationNumber.Equals(
                            vehicle.RegistrationNumber,
                            StringComparison.OrdinalIgnoreCase)
                        && t.DepartureTime is null);

                ticket?.RegisterDeparture(DateTimeOffset.Now);

                spot.ParkedVehicles.Remove(vehicle);
                return true;
            }
        }

        return false;
    }
    public Vehicle? FindVehicle(string registrationNumber)
    {
        foreach (ParkingSpot spot in ParkingSpots)
        {
            Vehicle? vehicle = spot.ParkedVehicles
                .FirstOrDefault(v =>
                    v.RegistrationNumber.Equals(
                        registrationNumber,
                        StringComparison.OrdinalIgnoreCase));

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
                v.RegistrationNumber.Equals(
                    registrationNumber,
                    StringComparison.OrdinalIgnoreCase));

            if (found)
            {
                return spot;
            }
        }

        return null;
    }
}
