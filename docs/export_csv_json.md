# Export data from the Data lake

This document has the goal to explain how to use the export functionality.

## Why?

For instance, the data can be used in different fields on your application, a brief example is to use for data analysis. Exporting data into a CSV or JSON file, make it easier for a Data scientists to use and customize it with a python script.
It’s also a nice way to see your data without relying on third-party tools, which means, you will have locally your data in a human-readable format.

Keep in mind that we are considering that you are under Andromeda.ConsoleApp folder and using a local project. If you are using a compiled Andromeda the `run` command will be replaced to `Andromeda.ConsoleApp.dll`

## How to Use:
To export your data we have the following commands:

### `dotnet run -- export -h`
        It will show the possible options.
### `dotnet run -- export`
        It will export the data (YouTube, Facebook, AdWords) with JSON and limited by 100 as default.

### `dotnet run export -t csv`
        It Will export the data on a CSV file. It is also possible to select JSON.

### `dotnet run export -s facebook`
        It Will export data filter by Facebook. It also can be Youtube or Adwords.

### `dotnet run export -l 10`
        It Will export the first 10 rows of the tables.

### `dotnet run export -t csv -s youtube -l 20`
        It will export 20 rows of YouTube on the CSV file
