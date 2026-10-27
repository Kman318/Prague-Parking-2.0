
using PragueParking.Core;
using PragueParking.Core.Pricing;
using Xunit;

namespace PragueParking.Tests;

public class PricingTests
{
    [Theory]
    [InlineData(8, 0)]
    [InlineData(10, 0)]
    [InlineData(11, 20)]
    [InlineData(60, 20)]
    [InlineData(70, 20)]
    [InlineData(71, 40)]
    public void Car_ShouldHaveCorrectParkingFee(
        int parkingMinutes,
        decimal expectedFee)
    {
        decimal fee = CalculateFee(
            new Car("ABC123", DateTimeOffset.Now),
            parkingMinutes,
            20m);

        Assert.Equal(expectedFee, fee);
    }

    [Theory]
    [InlineData(8, 0)]
    [InlineData(10, 0)]
    [InlineData(11, 10)]
    [InlineData(60, 10)]
    [InlineData(70, 10)]
    [InlineData(71, 20)]
    public void Motorcycle_ShouldHaveCorrectParkingFee(
        int parkingMinutes,
        decimal expectedFee)
    {
        decimal fee = CalculateFee(
            new Motorcycle("MC123", DateTimeOffset.Now),
            parkingMinutes,
            10m);

        Assert.Equal(expectedFee, fee);
    }

    private static decimal CalculateFee(Vehicle vehicle, int parkingMinutes, decimal hourlyRate)
    {
        DateTimeOffset arrival = DateTimeOffset.Now;

        var ticket = new ParkingTicket(vehicle.RegistrationNumber, 1, arrival);

        ticket.RegisterDeparture(arrival.AddMinutes(parkingMinutes));

        return Pricing.CalculateFee(
            vehicle,
            ticket,
            20m,
            10m,
            10);
    }
}
