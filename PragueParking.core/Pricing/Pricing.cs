
using PragueParking.Core;

namespace PragueParking.Core.Pricing;

public static class Pricing
{
    public static decimal CalculateFee(
        Vehicle vehicle,
        ParkingTicket ticket,
        decimal carHourlyRate,
        decimal motorcycleHourlyRate,
        int freeMinutes)
    {
        if (ticket.DepartureTime is not DateTimeOffset departureTime)
        {
            throw new InvalidOperationException(
                "The parking ticket has no departure time.");
        }

        TimeSpan parkingDuration =
            departureTime - ticket.ArrivalTime;

        TimeSpan freePeriod =
            TimeSpan.FromMinutes(freeMinutes);

        if (parkingDuration <= freePeriod)
        {
            return 0m;
        }

        decimal hourlyRate = vehicle switch
        {
            Car => carHourlyRate,
            Motorcycle => motorcycleHourlyRate,
            _ => throw new NotSupportedException(
                $"Unsupported vehicle type: {vehicle.GetType().Name}")
        };

        double billableHours = Math.Ceiling(
            (parkingDuration - freePeriod).TotalHours);

        return (decimal)billableHours * hourlyRate;
    }
}
