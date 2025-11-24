# ICT1.2 - First Robot Project

This repository is a minimal starter project for students building a robot application using separate classes for parts of its functionality. The basic idea is to give each class a single
responsibility, for example detecting obstacles in front of the robot and determining
their distance, or managing the speed of the robot. The idea is also to hide implementation
details, such as what type of sensor is used for obstacle detection, in those separate classes.
At the highest level in the program, we mostly see abstractions so we don't have to worry
about the details.

## What this project contains

- Minimal .NET project targeting `net8.0`.
- Main source file `Program.cs` and two classes in `DriveSystem.cs` and `ObstacleDetectionSystem.cs`.

## Prerequisites

- .NET 8 SDK (to match `net8.0` target)
- Visual Studio Code (strongly recommended)
- Install the Avans-StatisticalRobot VS Code extension to connect to the robot

## How to build

From the project root run:

```
dotnet build RobotProject.csproj
```

## How to run

Use the VS Code extension to deploy to the robot. Make sure the **proper** robot is selected and press F5 to build and send to the robot.

Console output appears in the Debug Console in VS Code.

## Notes and tips for students

- Make sure you connect to the robot before attempting hardware-specific code. Use the Avans-StatisticalRobot extension for VS Code.
- Draw a class diagram of the classes in this program to help with understanding the structure.

## Improvements to try yourself
This code can certainly be improved:
- Put `ObstacleDetectionSystem` and `DriveSystem` in a `List<IUpdatable> updatables`.
Then use a `foreach` loop to `Update()` each one in de event loop in `Program.cs`.
- Pass the ultrasonic sensor pin number as a parameter to the constructor of
`ObstacleDetectionSystem`. This avoids hard coding this implementation detail in `ObstacleDetectionSystem`, making it more generic and reusable.
- Maybe DriveSystem should have a maximum forward speed limit that can be set
using a method `SetForwardSpeedLimit(double limit)`?
Then setting that limit to zero below a certain obstacle distance would
automatically avoid any collisions. Give it a try.
- Maybe DriveSystem should have a *target* speed and an *actual* speed so that
with each call to `Update()` the actual speed is adjusted a little towards the target speed.
That would make speed changes much more gradual and elegant. Then you no longer set the
actual immediate speed as done here. Instead you set the target speed,
and DriveSystem takes care of achieving that target speed in small steps.
Give it a try.

## More information

- Avans Robot Library: https://github.com/AvansICT/ICT1.2-Avans_Robot_Library
