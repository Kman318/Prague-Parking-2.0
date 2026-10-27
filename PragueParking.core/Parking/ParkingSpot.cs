using System;
using System.Collections.Generic;
using System.Text;



namespace PragueParking.Core;

public class ParkingSpot
{
    public int SpotNumber { get; }
    public List<Vehicle> ParkedVehicles { get; } = new();
    public ParkingSpot(int spotNumber)
    {
        SpotNumber = spotNumber;
    }
}
