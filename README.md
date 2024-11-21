# NServiceBus Workshop

**Please ensure you have prepared your machine well in advance of the workshop. Your time during the workshop is valuable, and we want to use it for learning, rather than setting up machines.**

If you have any difficulty preparing your machine, or following this document, please raise an issue in this repository ASAP so that we can resolve the problem before the workshop begins.

- [Preparing your machine for the workshop](#preparing-your-machine-for-the-workshop)
- [Running the exercise solutions](#running-the-exercise-solutions)
- [Demos](#demos)
- [FAQ](#faq)

## Preparing your machine for the workshop

- [Install the pre-requisites](#install-the-pre-requisites)
- [Get a copy of this repository](#get-a-copy-of-this-repository)

### Install the pre-requisites

To complete the exercises, you require a Windows machine and Visual Studio/Rider.

#### Visual Studio / Rider

Install JetBrains Rider 2021.3 or later, or [Visual Studio 2022](https://www.visualstudio.com) or later (Community, Professional, or Enterprise) with the following workloads:

- .NET desktop development
- ASP.NET and web development

The following frameworks need to be installed:

- .NET 8
- Docker

#### ServiceInsight

The Particular Service Platform includes ServiceControl, ServicePulse and ServiceInsight.

The samples include a platform connection package that will fire up both ServiceControl and ServicePulse, without requiring any installation.

In order to try out ServiceInsight, an installation is required. The latest version can be downloaded [from GitHub](https://github.com/Particular/ServiceInsight/releases/latest). Once installed, ServiceInsight needs to be connected to ServiceControl. 
To do so, click the "Connect" button at the top left of the screen, and enter the ServiceControl URL. You can find the correct URL in ServicePulse, which shows a status bar with the ServiceControl information. Hovering over that, will expose the ServiceControl API URL. 

### Get a copy of this repository

Clone or download this repo. If you're downloading a zip copy of the repo, ensure the zip file is unblocked before decompressing it:

* Right-click on the downloaded copy
* Click "Properties"
* On the "General" properties page, check the "Unblock" checkbox
* Click "OK"

## Troubleshooting

### No valid license

Install the license in `/exercises/License.xml` as an [environment variable](https://docs.particular.net/nservicebus/licensing/#license-management-environment-variable).

### Access denied exception with learning transport

If you are getting access denied exceptions on the file system while running the exercises the cause is likely virusscanners, backup or file synchronisation software like dropbox or onedrive opening these files. Exclude the `.learningtransport` folder or temporarily disable these tools.

## Demos

### ServicePulse monitoring & recovery

The [self contained platform demo](https://docs.particular.net/tutorials/monitoring-demo/) includes ServicePulse, ServiceControl, ServiceControl Monitoring and a few endpoints to demonstrate monitoring and recovery.

## FAQ

If the answer to your question is not listed here, consult your on-site trainer.
