# Prague Parking 2.0

## About

Prague Parking 2.0 is a console application for managing a parking garage. 
It supports cars and motorcycles and uses a configuration file to control parking and pricing settings.

## Features

- View parking garage status.
- Park and remove vehicles.
- Find vehicles by registration number.
- Move vehicles between parking spots.
- Calculate parking fees using the configured prices.
- Reload the price list.
- Clear the garage.
- Save parked vehicles and parking tickets to a JSON file.
- Load saved parking data when the application starts.

## Requirements

- .NET 10 SDK.

## How to run

1. Clone or download the repository.
2. Open `Prague Parking 2.0.slnx` in Visual Studio.
3. Make sure the `AppSettings.json` configuration file is available to the application.
4. Build the solution.
5. Run the PragueParking.App project using F5 or Ctrl+F5.

## Data storage

Parking data is stored as JSON in the following location:

`%LOCALAPPDATA%\PragueParking\parking-data.json`

The application stores parked vehicles and parking tickets in this file.

## Configuration

The `AppSettings.json` file contains settings for the number of parking spots, 
motorcycle capacity, parking prices, free parking minutes, and parking map display.
