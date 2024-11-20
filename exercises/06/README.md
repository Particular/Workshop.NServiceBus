# Exercise 6: Hosting in docker

Important: Before attempting the exercise, please ensure you have followed the instructions for preparing your machine and that you have read the instructions for running the exercise solutions.

## Overview

Now that we understand the fundamentals of NServiceBus it is time to host our solution and make it ready for production!

In this exercise you'll learn:

- How to host an NServiceBus endpoint in production
- Make the process compatible with automated deployments
- Host endpoints in Docker

In this execise we will deploy Sales, Billing and Shipping to docker but keep the ClientUI running locally so that we can easier interact with it via the console.

## Step 1

Enable [dotnet publish to generate docker images](https://learn.microsoft.com/en-us/dotnet/core/docker/publish-as-container) for us by adding:

`<EnableSdkContainerSupport>true</EnableSdkContainerSupport>`

to the .csproj files of Sales, Billing and Shipping.

## Step 2

Copy the `docker-compose.yml` file from `exercises/06` into the same folder as your `RetailDemo.sln` is located.

## Step 3

Since the learning transport uses the local filesystem for messages we need to change to a production transport to enable our endpoints to communicate with each other. For this exercise we will use the RabbitMQ transport.

For ClientUI, Sales, Billing and Shipping

1. Reference `NServiceBus.RabbitMQ`
1. `endpointConfiguration.UseTransport(new LearningTransport());`with `endpointConfiguration.UseTransport(new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), "host=rabbitmq"));`
  - For the ClientUI use "host=localhost" as the connection string, we will talk more about why later.
1. Add `endpointConfiguration.EnableInstallers()` to enable queues to be created automatically. NOTE: This is not recommended for production


## Step 4

Since containers by default have no storage accecible to them we need to write the NServiceBus diagnostics file to a different location. For now add the following line to Sales, Billing and Shipping:


`endpointConfiguration.CustomDiagnosticsWriter((_, __) => Task.CompletedTask);`

This tells NServiceBus to skip writing any diagnostics on startup.

## Step 5

Make sure that you are not throwing any exceptions in Sales->PlaceOrderHandler from previous excercises.

## Step 6

Now we are ready to build and publish Sales, Billng and Shipping to our local docker registry. 

Execute the following command in the same folder as `RetailDemo.sln`

`dotnet publish RetailDemo.sln /p:Configuration=Debug /p:Platform="Any CPU" --os linux --arch x64 /t:PublishContainer`

once done our endpoints are now built pushed to the local docker registry and ready to be used.

## Step 7

Looking at the docker-compose.yml file you can see how we define the 3 endpoints, a RabbitMQ broker and wired the endpoints up to wait for the broker to become available before staring.

To start running use the following command:

`docker compose up -d`

You should now see the containers starting up.

## Step 8

Start the ClientUI in debug mode to send a few orders into the system.

Use the following commands to see what Sales, Billing and Shipping is doing.

`docker compose logs sales`
`docker compose logs billing`
`docker compose logs shipping`


## FAQ

Use: `docker compose rm -sf` to stop and remove all images to start fresh. 