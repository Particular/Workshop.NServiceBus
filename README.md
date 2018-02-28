# NServiceBus Workshop

**Please ensure you have prepared your machine well in advance of the workshop. Your time during the workshop is valuable, and we want to use it for learning, rather than setting up machines.**

If you have any difficulty preparing your machine, or following this document, please raise an issue in this repository ASAP so that we can resolve the problem before the workshop begins.

- [Preparing your machine for the workshop](#preparing-your-machine-for-the-workshop)
- [Running the exercise solutions](#running-the-exercise-solutions)
- [Preparing your machine for the advanced exercises](#preparing-your-machine-for-the-advanced-exercises)
- [Demos](#demos)
- [FAQ](#faq)

## Preparing your machine for the workshop

- [Install the pre-requisites](#install-the-pre-requisites)
- [Get a copy of this repository](#get-a-copy-of-this-repository)
- [Build the exercise solutions](#build-the-exercise-solutions)

### Install the pre-requisites

To complete the exercises, you require a Windows machine and Visual Studio. You must be using a Windows client edition, such as Windows 10, rather than a server edition, such as Windows Server 2016. The Particular Platform Installer does not support server editions of Windows.

#### Visual Studio

Install [Visual Studio 2017](https://www.visualstudio.com) (Community, Professional, or Enterprise) with the following workloads:
  - .NET desktop development
  - ASP.NET and web development

#### MSMQ and DTC

To ensure MSMQ and DTC are correctly installed, use the [Particular Platform Installer](https://particular.net/start-platform-download) in the following step.

NOTE: If you are using Microsoft Edge, see [the FAQ](#how-do-i-download-the-particular-platform-installer-with-microsoft-edge).

In the installation screen, select a minimum of:

- "Configure MSDTC for NServiceBus"
- "Configure Microsoft Message Queuing"

#### Particular Software Platform

While installing the Particular Software Platform, we will also make sure MSMQ and MSDTC (Microsoft Distributed Transaction Coordinator) are correctly installed.

1. Download the [Particular Service Platform](https://particular.net/start-platform-download) and start it.
2. Select every single item that is not installed yet. This includes ServiceControl, ServicePulse and ServiceInsight. But also MSMQ and MSDTC if it has not been installed yet.
3. When installation has completed, click "Start ServiceControl Management". You may now close the Particular Service Platform Installation window.

In the ServiceControl Management window, click "Add new instance".

In the settings for the new instance, there are two choices you must make:

- "Transport": choose "MSMQ".
- "Audit forwarding": choose "Off".

All other settings should be left with their default values.

Click "Add".

After the instance has been added, make a note of the name of the instance. This will usually be "Particular.ServiceControl".

You may now close the ServiceControl Management window.

### Get a copy of this repository

Clone or download this repo. If you're downloading a zip copy of the repo, ensure the zip file is unblocked before decompressing it:

* Right-click on the downloaded copy
* Click "Properties"
* On the "General" properties page, check the "Unblock" checkbox
* Click "OK"

### Build the exercise solutions

TODO: This still needs to be added to the repository

The exercises are contained in Visual Studio solutions under [exercises](exercises). All the solutions require NuGet package restore. This may be possible at the workshop venue (you can verify with the workshop organizers if internet access is available at the venue) but to ensure you can build the solutions during the workshop, we recommend you restore all NuGet packages and build all the solutions before the workshop starts. The simplest way to do this is to open a command prompt, navigate to your copy of this repo, and run `.\build.cmd exercises`. (For a full list of build targets, run `.\build.cmd -T`, or `.\build.cmd -h` for help.) You can safely ignore any compiler warnings.

## Running the exercise solutions

Before opening any exercise solutions, set the startup projects by navigating to your copy of this repo and running `set-startup-projects.cmd`. Note that if you `git clean` your clone, you will have to run this command again.

The startup projects are also listed in the instructions for each exercise. If you need to, you can configure them manually:

  - In Visual Studio, right click the solution in the Solution Explorer
  - Click "Properties"
  - Ensure that, in the left hand pane, "Common Properties", "Start Project" is selected.
  - Select the "Multiple startup projects" radio button
  - Set the "Action" for each project listed in the instructions for the exercise to "Start".

To run an exercise solution, simply press <kbd>F5</kbd> in Visual Studio. The exercise solution will now be running and fully functional.

## Demos

### ServicePulse monitoring & recovery

The [self contained platform demo](https://docs.particular.net/tutorials/monitoring-demo/) includes ServicePulse, ServiceControl, ServiceControl Monitoring and a few endpoints to demonstrate monitoring and recovery.

## FAQ

If the answer to your question is not listed here, consult your on-site trainer.

### Can I use Windows 7?

Yes. However, Visual Studio 2017 comes with SQL Server 2016 LocalDB, which is not compatible with Windows 7. Instead, you will need to install [SQL Server 2014 LocalDB](https://www.microsoft.com/en-us/download/details.aspx?id=42299). When prompted to choose which file to download, choose either the 64-bit or 32-bit version of `SqlLocalDB.msi`, depending on [your Windows installation](https://support.microsoft.com/en-gb/help/15056/windows-7-32-64-bit-faq).

### How do I download the Particular Platform Installer with Microsoft Edge?

When you attempt to download the installer, you may be presented with this message:

![download prompt](https://github.com/Particular/Workshop/raw/master/img/platform-installer-on-edge/download-prompt.png?raw=true)

If so, click on "View Downloads" to show the "Downloads" window:

![download list](https://github.com/Particular/Workshop/raw/master/img/platform-installer-on-edge/downloads-list.png?raw=true)

**Right-click** the red text "This unsafe download was blocked by SmartScreen Filter." to show the context menu:

![download unsafe file](https://github.com/Particular/Workshop/raw/master/img/platform-installer-on-edge/download-unsafe-file-option.png?raw=true)

 Click "Download unsafe file".
