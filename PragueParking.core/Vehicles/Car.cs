using System;
using System.Collections.Generic;
using System.Text;


using PragueParking.Core;

namespace PragueParking.Core;

public class Car : Vehicle
{
    public Car(
        string registrationNumber,
        DateTimeOffset arrivalTime)
        : base(registrationNumber, arrivalTime)
    {
    }
}
