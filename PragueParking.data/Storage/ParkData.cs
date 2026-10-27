using System;
using System.Collections.Generic;
using System.Text;
using PragueParking.Core;

namespace PragueParking.Data;

public class ParkData
{
    public List<SavedVehicle> Vehicles { get; set; } = new();
    public List<SavedTicket> Tickets { get; set; } = new();
}

public class SavedVehicle
{
    public string VehicleType { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTimeOffset ArrivalTime { get; set; }
    public int SpotNumber { get; set; }
}

public class SavedTicket
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public int SpotNumber { get; set; }
    public DateTimeOffset ArrivalTime { get; set; }
    public DateTimeOffset? DepartureTime { get; set; }
}

