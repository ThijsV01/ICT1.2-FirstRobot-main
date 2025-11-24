using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
/* 
    Dit is een voorbeeld van robotcode waarin de basisfuncties
    zijn verdeeld over enkele klassen, in plaats van alles in
    Program.cs te zetten. Bestudeer klasse DriveSystem en kijk
    welke public methoden die heeft om de robot te laten rijden.
    Bekijk ook lasse ObstacleDetectionSystem om te zie hoe die
    de afstand tot een obstakel meet en beschikbaar stelt.
    Je ziet dat deze twee klassen volkomen onafhankelijk zijn van
    wat voor gedrag de robot zal vertonen. Elke klasse heeft slechts
    één verantwoordelijkheid. De logica die bepaalt wat de robot
    doet zit in de event loop hier in Program.cs.
*/

// Show that our programming is running with a message on the console
Console.WriteLine("Hello First Robot!");
// Play a sound on the built-in robot buzzer to signal that our program is running
Robot.PlayNotes("g>g");

Led led = new Led(5); // Use the correct pin number here
bool ledIsOn = false;

// DriveSystem handles everything to do with movement of the robot
DriveSystem driveSystem = new DriveSystem();
// ObstacleDetectionSystem handles everything to do with detecting obstacles
// in front of our robot
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();

// An improvement to consider...
// It would be nice to put these objects in a List of IUpdatable objects
// and use a foreach loop here to Update() each of them, such as in the code below
// List<IUpdatable> updatables = new List<IUpdatable>();
// updatables.Add(obstacleDetectionSystem);
// updatables.Add(driveSystem);

// This is our event loop that runs indefinitely until we manually stop the program
while (true)
{
    // Use a short delay between updates to prevent overloading the data transfer
    // mechanism that sends data to the Romi microcontroller that controls the Romi
    // robot components such as the motors and the buzzer
    Robot.Wait(200);

    // Allow the separate classes to do their job by calling their Update() methods
    obstacleDetectionSystem.Update();
    driveSystem.Update();
    // An improvement to consider... (use a foreach loop)
    // foreach (IUpdatable updatable in updatables)
    // {
    //     updatable.Update();
    // }

    // Now for the main robot application logic (below)

    // Reduce speed if an obstacle is nearby
    // or speed up if obstacles are far away
    int obstacleDistance = obstacleDetectionSystem.ObstacleDistance;
    Console.WriteLine($"Obstacle distance = {obstacleDistance} cm");

    if (obstacleDistance < 5)
    {
        // If the obstacle comes closer,
        // then reverse while turning left
        driveSystem.SetForwardSpeed(-0.15);
        driveSystem.SetTurnSpeed(-0.3);
    }
    else if (obstacleDistance < 8)
    {
        // Avoid a collision
        driveSystem.Stop();
    }
    else if (obstacleDistance < 12)
    {
        // Slow down more
        driveSystem.SetForwardSpeed(0.10);
        driveSystem.SetTurnSpeed(0.0);
    }
    else if (obstacleDistance < 30)
    {
        // Slow down
        driveSystem.SetForwardSpeed(0.19);
        driveSystem.SetTurnSpeed(0.0);
    }
    else
    {
        // Run forward in a left turn
        driveSystem.SetForwardSpeed(0.25);
        driveSystem.SetTurnSpeed(-0.10);
    }

    // Blink the LED each time we pass through this event loop
    ledIsOn = !ledIsOn;
    if (ledIsOn)
    {
        led.SetOn();
    }
    else
    {
        led.SetOff();
    }
}