using System;
using System.Collections.Generic;
using System.Text;



using PragueParking.Core;

namespace PragueParking.Core;

public class Motorcycle : Vehicle
{
    public Motorcycle(
        string registrationNumber,
        DateTimeOffset arrivalTime)
        : base(registrationNumber, arrivalTime)
    {
    }
}