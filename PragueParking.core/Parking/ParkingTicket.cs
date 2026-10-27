using System;
using System.Collections.Generic;
using System.Text;


namespace PragueParking.Core;

public class ParkingTicket
{
    public string RegistrationNumber { get; }
    public int SpotNumber { get; private set; }
    public DateTimeOffset ArrivalTime { get; }
    public DateTimeOffset? DepartureTime { get; private set; }

    public ParkingTicket(string registrationNumber, int spotNumber, DateTimeOffset arrivalTime)
    {

        RegistrationNumber = registrationNumber;
        SpotNumber = spotNumber;
        ArrivalTime = arrivalTime;
    }

    public void UpdateSpotNumber(int spotNumber)
    {
        if (spotNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(spotNumber),
                "Spot number must be greater than zero.");
        }

        SpotNumber = spotNumber;
    }

    public void RegisterDeparture(DateTimeOffset departureTime)
    {
        if (departureTime < ArrivalTime)
        {
            throw new ArgumentException("Departure time cannot be earlier than arrival time.");
        }

        DepartureTime = departureTime;
    }
}

