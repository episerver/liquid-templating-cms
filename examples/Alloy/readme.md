# Alloy MVC template - Liquid

This template should not be seen as best practices, but as a great way to learn and test Optimizely CMS. 

## Liquid
This instance has been modified to use the Optimizely.CMS.Labs.LiquidTemplating Nuget package, allowing views within the solution to be written in the Liquid Templating language.

The Optimizely.CMS.Labs.LiquidTemplating package can be swapped in or out with minor levels of C# code change. 

The following C# changes have been made to this instance

- Liquid files are added to the TemplateCoordinator rather than .cshtml
- The DefaultController references liquid views directly (rather than .cshtml)
- Any ViewModels or ContentModels with publically available methods, used within the views have been modified so that the functionality is available via a public getter.

All of the views are written in the Liquid templating language. There are some implementation differences within the views due to the below

- The Fluid ViewEngine does not support nested layouts
- Any functionality that depends public methods is not supported by Liquid. Liquid has no concept of calling functions and  method and computing data at render time (actually this isn’t strictly true - see ContentLoader Value, but it isn’t something you should rely on)

## How to run

Chose one of the following options to get started. 

### Windows

Prerequisities
- .NET SDK 6+
- SQL Server 2016 Express LocalDB (or later)

```bash
$ dotnet run
````

### Any OS with Docker

Prerequisities
- Docker
- Enable Docker support when applying the template

```bash
$ docker-compose up
````

> Note that this Docker setup is just configured for local development. Follow this [guide to enable HTTPS](https://github.com/dotnet/dotnet-docker/blob/main/samples/run-aspnetcore-https-development.md).

### Any OS with external database server

Prerequisities
- .NET SDK 6+
- SQL Server 2016 (or later) on a external server, e.g. Azure SQL

Create an empty database on the external database server and update the connection string accordingly.

```bash
$ dotnet run
````

### Default credentials
A .mdf database is provided. Default credentials have been set up as

    - Username: Admin
    - Password: Test123!