using PragueParking.Core;
using PragueParking.Core.Pricing;
using PragueParking.Data.Pricing;
using PragueParking.Data.Storage;
using Spectre.Console;

var garage = new ParkingGarage();
var priceList = new PriceList();
var fileRepo = new FileRepo();
fileRepo.Load(garage);

while (true)
{
    AnsiConsole.Clear();

    AnsiConsole.Write(
        new FigletText("Prague Parking")
            .Centered()
            .Color(Color.DeepSkyBlue1));

    AnsiConsole.Write(
        new Align(
            new Markup("[grey]Parking Garage Management System[/]"),
            HorizontalAlignment.Center,
            VerticalAlignment.Top));

    ShowParkingSpots(garage, out var parkingTable, out var legend);

    var menu = new SelectionPrompt<string>()
        .Title("[bold cyan]Main Menu[/]")
        .AddChoices(
            "View parking status",
            "Park a vehicle",
            "Remove a vehicle",
            "Find a vehicle",
            "Exit");


    var menuDisplay = new Panel(
        new Markup(
            "[bold cyan]1.[/] View parking status\n\n" +
            "[bold cyan]2.[/] Park a vehicle\n\n" +
            "[bold cyan]3.[/] Remove a vehicle\n\n" +
            "[bold cyan]4.[/] Find a vehicle\n\n" +
            "[bold cyan]5.[/] Exit"))
        .Header("[bold cyan]Main Menu[/]")
        .Expand();

    AnsiConsole.Write(
        new Columns(
            menuDisplay,
            new Panel(parkingTable)
                .Header("[bold cyan]Parking Garage[/]")
                .Expand()));

    AnsiConsole.Write(
        new Align(legend, HorizontalAlignment.Center));

    var choiceNumber = AnsiConsole.Prompt(
        new TextPrompt<int>("[bold cyan]Select option (1-5):[/]")
            .Validate(number => number >= 1 && number <= 5
                ? ValidationResult.Success()
                : ValidationResult.Error("[red]Enter a number from 1 to 5.[/]")));

    var choice = choiceNumber switch
    {
        1 => "View parking status",
        2 => "Park a vehicle",
        3 => "Remove a vehicle",
        4 => "Find a vehicle",
        5 => "Exit",
        _ => ""
    };


    switch (choice)
    {
        case "View parking status":
            AnsiConsole.MarkupLine(
                $"[green]Total spots:[/] {garage.ParkingSpots.Count}");

            AnsiConsole.MarkupLine(
                $"[green]Occupied spots:[/] " +
                $"{garage.ParkingSpots.Count(s => s.ParkedVehicles.Count > 0)}");

            AnsiConsole.MarkupLine(
                $"[green]Available spots:[/] " +
                $"{garage.ParkingSpots.Count(s => s.ParkedVehicles.Count == 0)}");

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
            Console.ReadKey(true);
            break;


        case "Park a vehicle":
            {
                var vehicleType = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select vehicle type:")
                        .AddChoices("Car", "Motorcycle"));

                var registrationNumber = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter registration number:")
                        .Validate(registration => !string.IsNullOrWhiteSpace(registration)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Registration number is required.")));

                Vehicle vehicle = vehicleType switch
                {
                    "Car" => new Car(
                        registrationNumber.Trim(),
                        DateTimeOffset.Now),

                    "Motorcycle" => new Motorcycle(
                        registrationNumber.Trim(),
                        DateTimeOffset.Now),

                    _ => throw new InvalidOperationException(
                        $"Unknown vehicle type: {vehicleType}")
                };

                if (garage.ParkVehicle(vehicle))
                {
                    fileRepo.Save(garage);
                    var spot = garage.FindParkingSpot(vehicle.RegistrationNumber);
                    AnsiConsole.MarkupLine(
                        $"[green]Vehicle parked successfully in spot {spot?.SpotNumber}.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        "[red]Could not park vehicle. Check the registration number or available spaces.[/]");
                }

                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                break;
            }



        case "Remove a vehicle":
            {
                var registrationNumber = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter registration number to remove:")
                        .Validate(registration => !string.IsNullOrWhiteSpace(registration)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Registration number is required.")));

                registrationNumber = registrationNumber.Trim();

                var vehicle = garage.FindVehicle(registrationNumber);
                var ticket = garage.ParkingTickets.LastOrDefault(t =>
                    t.RegistrationNumber.Equals(
                        registrationNumber,
                        StringComparison.OrdinalIgnoreCase)
                    && t.DepartureTime is null);

                if (vehicle is null || ticket is null)
                {
                    AnsiConsole.MarkupLine(
                        "[red]Vehicle or active parking ticket not found.[/]");
                }
                else if (garage.RemoveVehicle(registrationNumber))
                {
                    decimal fee = Pricing.CalculateFee(
                                        vehicle,
                                        ticket,
                                        priceList.CarHourlyRate,
                                        priceList.MotorcycleHourlyRate,
                                        priceList.FreeMinutes);

                    AnsiConsole.MarkupLine(
                        "[green]Vehicle removed successfully.[/]");

                    AnsiConsole.MarkupLine(
                        $"Registration: {vehicle.RegistrationNumber}");

                    AnsiConsole.MarkupLine(
                        $"Parking duration: {ticket.DepartureTime!.Value - ticket.ArrivalTime}");

                    AnsiConsole.MarkupLine(
                        $"[bold yellow]Parking fee: {fee:0.##} CZK[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        "[red]Could not remove vehicle.[/]");
                }

                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                break;
            }



        case "Find a vehicle":
            {
                var registrationNumber = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter registration number to search:")
                        .Validate(registration => !string.IsNullOrWhiteSpace(registration)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Registration number is required.")));

                var vehicle = garage.FindVehicle(registrationNumber.Trim());
                var spot = garage.FindParkingSpot(registrationNumber.Trim());

                if (vehicle is not null && spot is not null)
                {
                    string vehicleType = vehicle is Car ? "Car" : "Motorcycle";

                    AnsiConsole.MarkupLine("[green]Vehicle found![/]");
                    AnsiConsole.MarkupLine($"Type: {vehicleType}");
                    AnsiConsole.MarkupLine($"Registration: {vehicle.RegistrationNumber}");
                    AnsiConsole.MarkupLine($"Parking spot: {spot.SpotNumber}");
                    AnsiConsole.MarkupLine(
                        $"Arrival time: {vehicle.ArrivalTime:yyyy-MM-dd HH:mm}");
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        "[red]Vehicle not found.[/]");
                }

                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                break;
            }


        case "Exit":
            return;
    }
}

static void ShowParkingSpots(
    ParkingGarage garage,
    out Table parkingTable,
    out Markup legend)
{
    parkingTable = new Table()
        .Border(TableBorder.Rounded);

    for (int column = 1; column <= 10; column++)
    {
        parkingTable.AddColumn(
            new TableColumn(column.ToString("D2")).Centered());
    }

    for (int row = 0; row < 10; row++)
    {
        var cells = new string[10];

        for (int column = 0; column < 10; column++)
        {
            int index = row * 10 + column;
            var spot = garage.ParkingSpots[index];

            string number = spot.SpotNumber.ToString("D3");

            if (spot.ParkedVehicles.Count == 0)
            {
                cells[column] = $"[green]{number}[/]";
            }
            else if (
                spot.ParkedVehicles.Any(v => v is Car) ||
                spot.ParkedVehicles.Count >= 2)
            {
                cells[column] = $"[red]{number}[/]";
            }
            else
            {
                cells[column] = $"[yellow]{number}[/]";
            }
        }

        parkingTable.AddRow(cells);
    }

    legend = new Markup(
        "[green]■[/] Available   " +
        "[yellow]■[/] One Motorcycle   " +
        "[red]■[/] Full");
}
