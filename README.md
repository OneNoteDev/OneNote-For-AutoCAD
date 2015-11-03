README: OneNote-For-AutoCAD
OneNote add-in for AutoCAD that uses OneNote APIs
===================
1 - Overview
===================
OneNote for AutoCAD® is an add-in for AutoCAD that allows users to take notes alongside their drawings from within AutoCAD. These notes are backed up to the cloud and can be accessed anytime. Users can see these notes the next time they open the drawing in AutoCAD.

OneNote-For-AutoCAD is being shared as sample code to demonstrate a real world example of how to leverage the [OneNote REST APIs](http://dev.onenote.com) across various integration points. Developers can use this code sample to bootstrap their own OneNote related projects.

===================
2 - Shipped Product
===================
OneNote for AutoCAD is now shipped to the AutoDesk app store!

You can download the add-in OneNote for AutoCAD [here] (https://apps.autodesk.com/ACD/en/Detail/Index?id=appstore.exchange.autodesk.com%3aonenote_windows32and64%3aen)


===================
3 - Dependencies
===================
The project is written in C# and requires .NET 4.5 or above. 
A Visual Studio 2013 or above compatible solution is provided and the project has the following dependencies:
 - Nuget packages:
    - LiveSDK
    - Newtonsoft.Json
    - Microsoft.AspNet.WebApi.Client
 - AutoCAD .NET APIs. Learn more [here](http://help.autodesk.com/view/ACD/2015/ENU/?guid=GUID-C3F3C736-40CF-44A0-9210-55F6A939B6F2)  

==================
4 - Building
==================
Open Visual Studio as an administrator. 
Provided that you have all of the indicated dependencies, then you should be able to build this project directly
from the VS interface.

To run the app you need to have following pre-requisites:
 - 1. Trial or Full version of AutoCAD installed
 - 2. You will need to obtain a Microsoft Live Application ClientId as described [here] (https://msdn.microsoft.com/en-us/library/office/dn575426.aspx)
 - 3. In AuthManager.cs set the ClientId as your Microsoft Live Application ClientId.
 - 4. In the Project properties, ensure that the option to Start External Program on Debug is selected and supply it the path to the acad.exe executable on your machine. Generally this is present in Program Files where AutoCAD is installed. Then on running the project, the AutoCAD IDE will start up with the add-in installed. 
 - 5. Once you make any changes to the project, build it and load the new dll using netload in the AutoCAD IDE.

==================
5 - Demo
==================
[![Demo video](https://raw.githubusercontent.com/OneNoteDev/OneNote-For-AutoCAD/master/Screenshot.png?token=AGIr-FeRzNDWiuTKglCnptRwu3invStVks5WO6bwwA%3D%3D)](https://screencast.autodesk.com/Embed/4126e149-e5f2-4597-a9e8-6fa9ccfb6d75)

