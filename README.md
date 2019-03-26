# iMatch Xamarin Sample

This sample project demonstrates how to use the iMatch Xamarin plugin.

# How to install
1. Clone this repository and open the project with Visual Studio.
2. Build and run the application on an Android or iOS device.

## How to add the plugin to your project
The iMatch Xamarin plugin is available on [nuget](https://www.nuget.org/packages/Gridler.iMatch).
### Package Manager
    Install-Package Gridler.iMatch
### .NET CLI
    dotnet add package Gridler.iMatch

# How to use the SDK
The iMatch plugin exposes it's functions through the iMatch interface and several event listeners.

    iMatch = new iMatch(adapter);
    iMatch.MessageReceived += Imatch_MessageReceived;
    iMatch.ConnectionStatusChanged += Imatch_ConnectionStatusChanged;

    iMatch.ScanForDevices();

## Basic steps
1. Create an instance of the iMatch class and pass in a CrossBluetoothLE adapter.
2. Call the ScanForDevices() function to scan for and connect to an iMatch device. 
3. Implement the desired listeners to enable your app to handle the events sent from the iMatch device.
4. Send commands to the iMatch by using the sendCommand() function and handle the results.

# More information
* See the sample code for the exact implementation.
* See the [iMatch wiki](https://github.com/Gridler/cordova-plugin-imatch/wiki) to learn about the commands you can send to the iMatch. 