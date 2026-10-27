using PragueParking.Core;
using PragueParking.Core.Pricing;
using PragueParking.Data.Pricing;
using PragueParking.Data.Storage;
using PragueParking.data.Configuration;
using Spectre.Console;

var config = Config.Load("AppSettings.json");
var garage = new ParkingGarage(
    config.Parking.TotalSpots,
    config.Parking.MotorcyclesPerSpot);
var priceList = new PriceList(
    config.Pricing.CarHourlyRate,
    config.Pricing.MotorcycleHourlyRate,
    config.Pricing.FreeMinutes);
priceList.LoadFromFile("pricelist.txt");

var fileRepo = new FileRepo();
fileRepo.Load(garage);

while (true)
{
    AnsiConsole.Clear();

    AnsiConsole.Write(
        new FigletText("Prague Parking").Centered().Color(Color.DeepSkyBlue3));

    AnsiConsole.WriteLine("\n\n");
    ShowParkingSpots(garage, config.Display.MapColumns, out var parkingTable, out var legend);

    var menu = new SelectionPrompt<string>()
        .Title("[bold cyan]Main Menu[/]")
        .AddChoices(
            "View Parking Status",
            "Park a Vehicle",
            "Remove a Vehicle",
            "Find a Vehicle",
            "Move a Vehicle",
            "Reload Price List",
            "Exit");


    var menuDisplay = new Panel(new Markup(
        "[bold cyan]1.[/] View Parking Status\n\n" +
        "[bold cyan]2.[/] Park a Vehicle\n\n" +
        "[bold cyan]3.[/] Remove a Vehicle\n\n" +
        "[bold cyan]4.[/] Find a Vehicle\n\n" +
        "[bold cyan]5.[/] Move a Vehicle\n\n" +
        "[bold cyan]6.[/] Reload Price List\n\n" +
        "[bold cyan]7.[/] Reset Garage\n\n" +
        "[bold cyan]8.[/] Exit"))
    .Header("[bold cyan]Main Menu[/]").Expand();

    if (config.Display.ShowParkingMap)
    {
        AnsiConsole.Write(
            new Columns(menuDisplay,
                new Panel(parkingTable).Header("[bold cyan]Parking Garage[/]").Expand()));

    }
    else
    {
        AnsiConsole.Write(menuDisplay);
    }

    AnsiConsole.Write(new Align(legend, HorizontalAlignment.Center));

    var choiceNumber = AnsiConsole.Prompt(
        new TextPrompt<int>("[bold cyan]Select option (1-8):[/]")
            .Validate(number => number >= 1 && number <= 8
                ? ValidationResult.Success()
                : ValidationResult.Error("[red]Enter a number from 1 to 8.[/]")));

    var choice = choiceNumber switch
    {
        1 => "View Parking Status",
        2 => "Park a Vehicle",
        3 => "Remove a Vehicle",
        4 => "Find a Vehicle",
        5 => "Move a Vehicle",
        6 => "Reload Price List",
        7 => "Reset Garage",
        8 => "Exit",
        _ => ""
    };


    switch (choice)
    {
        case "View Parking Status": AnsiConsole.MarkupLine($"[green]Total spots:[/] {garage.ParkingSpots.Count}");

            AnsiConsole.MarkupLine($"[green]Occupied spots:[/] " +
                $"{garage.ParkingSpots.Count(s => s.ParkedVehicles.Count > 0)}");

            AnsiConsole.MarkupLine($"[green]Available spots:[/] " +
                $"{garage.ParkingSpots.Count(s => s.ParkedVehicles.Count == 0)}");

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
            Console.ReadKey(true);
            break;


        case "Park a Vehicle":
            {
                var vehicleType = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .Title("Select vehicle type:")
                        .AddChoices("Car", "Motorcycle"));

                var registrationNumber = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter registration number:")
                        .Validate(registration => !string.IsNullOrWhiteSpace(registration)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Registration number is required.")));

                Vehicle vehicle = vehicleType switch
                {
                    "Car" => new Car(registrationNumber.Trim(), DateTimeOffset.Now),
                    "Motorcycle" => new Motorcycle(registrationNumber.Trim(), DateTimeOffset.Now),
                    _ => throw new InvalidOperationException($"Unknown vehicle type: {vehicleType}")
                };

                if (garage.ParkVehicle(vehicle))
                {
                    fileRepo.Save(garage);
                    var spot = garage.FindParkingSpot(vehicle.RegistrationNumber);
                    AnsiConsole.MarkupLine($"[green]Vehicle parked successfully in spot {spot?.SpotNumber}.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Could not park vehicle. Check the registration number or available spaces.[/]");
                }

                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                break;
            }



        case "Remove a Vehicle":
            {
                var registrationNumber = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter registration number to remove:")
                        .Validate(registration => !string.IsNullOrWhiteSpace(registration)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Registration number is required.")));

                registrationNumber = registrationNumber.Trim();

                var vehicle = garage.FindVehicle(registrationNumber);
                var ticket = garage.ParkingTickets.LastOrDefault(t =>
                    t.RegistrationNumber.Equals(registrationNumber, StringComparison.OrdinalIgnoreCase)
                    && t.DepartureTime is null);

                if (vehicle is null || ticket is null)
                {
                    AnsiConsole.MarkupLine("[red]Vehicle or active parking ticket not found.[/]");
                }
                else if (garage.RemoveVehicle(registrationNumber))
                {
                    fileRepo.Save(garage);
                    decimal fee = Pricing.CalculateFee(
                                        vehicle,
                                        ticket,
                                        priceList.CarHourlyRate,
                                        priceList.MotorcycleHourlyRate,
                                        priceList.FreeMinutes);

                    AnsiConsole.MarkupLine("[green]Vehicle removed successfully.[/]");
                    AnsiConsole.MarkupLine($"Registration: {vehicle.RegistrationNumber}");
                    AnsiConsole.MarkupLine($"Parking duration: {ticket.DepartureTime!.Value - ticket.ArrivalTime}");
                    AnsiConsole.MarkupLine($"[bold yellow]Parking fee: {fee:0.##} CZK[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Could not remove vehicle.[/]");
                }

                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                break;
            }



        case "Find a Vehicle":
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
                    AnsiConsole.MarkupLine($"Arrival time: {vehicle.ArrivalTime:yyyy-MM-dd HH:mm}");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Vehicle not found.[/]");
                }

                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                break;
            }

        case "Move a Vehicle":
            {
                var registrationNumber = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter registration number to move:")
                        .Validate(registration => !string.IsNullOrWhiteSpace(registration)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Registration number is required.")));

                registrationNumber = registrationNumber.Trim();

                var vehicle = garage.FindVehicle(registrationNumber);
                var currentSpot = garage.FindParkingSpot(registrationNumber);

                if (vehicle is null || currentSpot is null)
                {
                    AnsiConsole.MarkupLine("[red]Vehicle not found.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        $"Vehicle {vehicle.RegistrationNumber} is currently in spot {currentSpot.SpotNumber}.");

                    int targetSpotNumber = AnsiConsole.Prompt(
                        new TextPrompt<int>("Enter target parking spot number:")
                            .Validate(number => garage.ParkingSpots.Any(s => s.SpotNumber == number)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red]That parking spot does not exist.[/]")));

                    if (garage.MoveVehicle(registrationNumber, targetSpotNumber))
                    {
                        fileRepo.Save(garage);

                        AnsiConsole.MarkupLine(
                            $"[green]Vehicle moved successfully to spot {targetSpotNumber}.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine(
                            "[red]Could not move vehicle. The target spot may be occupied or invalid.[/]");
                    }
                }

                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                break;
            }



        case "Reload Price List":
            try
            {
                priceList.LoadFromFile("pricelist.txt");
                AnsiConsole.MarkupLine("[green]Price list reloaded successfully.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Could not reload price list: {Markup.Escape(ex.Message)}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");

            Console.ReadKey(true);
            break;


        case "Reset Garage":
            {
                bool confirm = AnsiConsole.Confirm(
                    "[bold red]Are you sure you want to reset the garage and delete all parking history?[/]",
                    false);

                if (confirm)
                {
                    garage.ClearGarage();
                    fileRepo.Save(garage);

                    AnsiConsole.MarkupLine(
                        "[green]Garage cleared. All vehicles and parking history have been reset.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Garage reset cancelled.[/]");
                }

                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                break;
            }


        case "Exit":
            return;
    }
}


static void ShowParkingSpots(ParkingGarage garage, int mapColumns, out Table parkingTable, out Markup legend)
{
    int totalSpots = garage.ParkingSpots.Count;
    int columns = Math.Max(1, Math.Min(mapColumns, Math.Max(1, totalSpots)));

    parkingTable = new Table()
    {
        Border = TableBorder.Rounded,
        ShowHeaders = false
    };

    for (int column = 1; column <= columns; column++)
    {
        parkingTable.AddColumn(new TableColumn("").Centered());
    }

    for (int start = 0; start < totalSpots; start += columns)
    {
        var cells = new string[columns];

        for (int column = 0; column < columns; column++)
        {
            int index = start + column;

            if (index >= totalSpots)
            {
                cells[column] = "";
                continue;
            }

            var spot = garage.ParkingSpots[index];

            string number = spot.SpotNumber.ToString("D3");

            if (spot.ParkedVehicles.Count == 0)
            {
                cells[column] = $"[green]{number}[/]";
            }
            else if (spot.ParkedVehicles.Any(v => v is Car))
            {
                cells[column] = $"[red]{number}[/]";
            }
            else if (spot.ParkedVehicles.Count >= 2)
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
