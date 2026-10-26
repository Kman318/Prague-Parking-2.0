using System;
using System.Collections.Generic;
using System.Text;


namespace PragueParking.Core;

public abstract class Vehicle
{
    public string RegistrationNumber { get; }
    public DateTimeOffset ArrivalTime { get; }

    protected Vehicle(
        string registrationNumber,
        DateTimeOffset arrivalTime)
    {
        RegistrationNumber = registrationNumber;
        ArrivalTime = arrivalTime;
    }
}
