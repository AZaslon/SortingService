# Sorting Service

This is solution implementing service for convenient sorting of big missives of different types  (!*!Add link to page with supported types), Solution can also be extended with Custom types and custom processing logic.

## Getting started
To use the service use next REST API 

    Add C# code how to make REST request
    enter code here

## Architecture 
Architecture of the solution addressing next concerns 

 - Supporting of asynchronous processing of data
 - Scalability 
	 - Supporting of variable load  with possibility to address potential spikes 
	 - Easy support of potential changes in SLA (Service Level Agreements) for resiliency and availability 
 - Resiliency of service  

https://github.com/AZaslon/SortingService/blob/main/Docs/Sorting%20service.pptx

## Installation
The solution can be built and shipped as set of  ready to execute Linux container, all necessary settings can be set via environment variables.

### Building/running solution

#### Development environment

Prerequisites

 - Running Docker desktop (Latest version)
 - Visual Studio or IDE of your choice which  supporting of Docker-compose project type  and  .NET 5.0 SDK or Later (Software Development Kit)
 - Checkout repository
 
##### Run/Debugging locally
It should require minimal necessary actions to deploy/test locally, all necessary settings are predefined in appsettings and in docker-compose files.

 - Make sure Infrastructure services are running, see **Infrastructure** 
 - Open solution in Visual Studio
 - Choose  Docker as a run project 
 - Start debugging
 - VS will open Landing page, explore possible actions there.
 
##### Infrastructure
 To run/debug solution and tests on developers machines, install necessary infrastructure.
   - Execute ./buildscript/deploy.dependencies.locally.ps1   this will start Kafka Events queue and Redis Cache  services on local environment. Make sure services are started without issues. Check for critical exceptions on console.

##### Executing tests
- Make sure Infrastructure services are running, see **Infrastructure**
- Open solution in Visual Studio
- Execute tests with tests runner (*** !*!  note to myself Verify  if no issues wit MSTests !*! ***  ), alternatively build solution and execute tests with any tests runner supporting of NUnit3 tests type.
