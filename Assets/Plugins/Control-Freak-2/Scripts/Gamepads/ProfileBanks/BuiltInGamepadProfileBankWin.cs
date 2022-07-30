// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2
{

public class BuiltInGamepadProfileBankWin : BuiltInGamepadProfileBank
	{
	// ------------------
	public BuiltInGamepadProfileBankWin() : base()
		{

			
		this.profiles = new GamepadProfile[]
			{	

			// XBox One  ------------------

			new GamepadProfile(              
              "XBOX One", 
              "XBox One", 
              GamepadProfile.ProfileMode.Normal,
              null, null,     
              GamepadProfile.JoystickSource.Axes(0, true, 1, false),    // Left Stick
              GamepadProfile.JoystickSource.Axes(3, true, 4, false),    // Right Stick
              GamepadProfile.JoystickSource.Axes(5, true, 6, true),    // Dpad
              GamepadProfile.KeySource.Key(0),                // A
              GamepadProfile.KeySource.Key(1),                // B
              GamepadProfile.KeySource.Key(2),                // X
              GamepadProfile.KeySource.Key(3),                // Y
             
              GamepadProfile.KeySource.Key(7),               // Select (menu)
              GamepadProfile.KeySource.Key(6),                // Start (select)
              GamepadProfile.KeySource.Key(4),                // L1
              GamepadProfile.KeySource.Key(5),                // R1
              GamepadProfile.KeySource.PlusAxis(8),			  // L2	
              GamepadProfile.KeySource.PlusAxis(9),				// R2
              GamepadProfile.KeySource.Key(8),                // L3
              GamepadProfile.KeySource.Key(9)                 // R3
              ),





			// nVidia Shield (Unity 5.5.x) ------------------

			new GamepadProfile(              
              "nVidia Shield", 
              "nVidia", 
              GamepadProfile.ProfileMode.Normal,
              null, null,     
              GamepadProfile.JoystickSource.Axes(0, true, 1, false),    // Left Stick
              GamepadProfile.JoystickSource.Axes(2, true, 3, false),    // Right Stick
              GamepadProfile.JoystickSource.Axes(4, true, 5, true),    // Dpad
              GamepadProfile.KeySource.Key(9),                // A
              GamepadProfile.KeySource.Key(8),                // B
              GamepadProfile.KeySource.Key(7),                // X
              GamepadProfile.KeySource.Key(6),                // Y
             
              GamepadProfile.KeySource.Key(11),               // Select
              GamepadProfile.KeySource.Key(0),                // Start
              GamepadProfile.KeySource.Key(5),                // L1
              GamepadProfile.KeySource.Key(4),                // R1
              GamepadProfile.KeySource.PlusAxis(-1),			  // L2		// Triggers don't work.
              GamepadProfile.KeySource.MinusAxis(-1),				// R2
              GamepadProfile.KeySource.Key(3),                // L3
              GamepadProfile.KeySource.Key(2)                 // R3
              ),

			// Xbox 360 (Unity 5.2.2) -----------------

			new GamepadProfile(
				"XBOX 360", 
				"Controller (XBOX 360 For Windows)",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Win,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(3, true, 4, false),	// Right Stick
				GamepadProfile.JoystickSource.Axes(5, true, 6, true),	// Dpad
	
				GamepadProfile.KeySource.Key(0),				// A
				GamepadProfile.KeySource.Key(1),				// B
				GamepadProfile.KeySource.Key(2),				// X
				GamepadProfile.KeySource.Key(3),				// Y
				
				GamepadProfile.KeySource.Key(6),				// Select
				GamepadProfile.KeySource.Key(7),				// Start
	
				GamepadProfile.KeySource.Key(4),				// L1
				GamepadProfile.KeySource.Key(5),				// R1
				GamepadProfile.KeySource.PlusAxis(8),	// L2
				GamepadProfile.KeySource.PlusAxis(9),	// R2
				GamepadProfile.KeySource.Key(8),				// L3
				GamepadProfile.KeySource.Key(9)					// R3
				),


			// Xbox 360 catch-all -----------------

			new GamepadProfile(
				"XBOX 360", 
				"XBOX 360",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Win,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(3, true, 4, false),	// Right Stick
				GamepadProfile.JoystickSource.Axes(5, true, 6, true),	// Dpad
	
				GamepadProfile.KeySource.Key(0),				// A
				GamepadProfile.KeySource.Key(1),				// B
				GamepadProfile.KeySource.Key(2),				// X
				GamepadProfile.KeySource.Key(3),				// Y
				
				GamepadProfile.KeySource.Key(6),				// Select
				GamepadProfile.KeySource.Key(7),				// Start
	
				GamepadProfile.KeySource.Key(4),				// L1
				GamepadProfile.KeySource.Key(5),				// R1
				GamepadProfile.KeySource.PlusAxis(8),	// L2
				GamepadProfile.KeySource.PlusAxis(9),	// R2
				GamepadProfile.KeySource.Key(8),				// L3
				GamepadProfile.KeySource.Key(9)					// R3
				),


			// MOGA HERO Power "Mode B" (Unity 5.2.2) -----------------

			new GamepadProfile(
				"MOGA", 
				"Android Controller Gen-2(ACC)",
				GamepadProfile.ProfileMode.Normal,
				null, null,
				//GamepadProfile.PlatformFlag.Win,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(2, true, 4, false),	// Right Stick
				GamepadProfile.JoystickSource.Axes(4, true, 5, true),	// Dpad
	
				GamepadProfile.KeySource.Key(0),				// A
				GamepadProfile.KeySource.Key(1),				// B
				GamepadProfile.KeySource.Key(2),				// X
				GamepadProfile.KeySource.Key(3),				// Y
				
				GamepadProfile.KeySource.Key(6),				// Select
				GamepadProfile.KeySource.Key(7),				// Start
	
				GamepadProfile.KeySource.Key(4),				// L1
				GamepadProfile.KeySource.Key(5),				// R1
				GamepadProfile.KeySource.Key(-1),			// L2 
				GamepadProfile.KeySource.Key(-1),			// R2 
				GamepadProfile.KeySource.Key(8),				// L3
				GamepadProfile.KeySource.Key(9)					// R3
				),


			// MOGA Catch-all (Unity 5.2.2) -----------------

			new GamepadProfile(
				"MOGA", 
				"Android Controller",
				GamepadProfile.ProfileMode.Regex,
				null, null,
				//GamepadProfile.PlatformFlag.Win,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(2, true, 4, false),	// Right Stick
				GamepadProfile.JoystickSource.Axes(4, true, 5, true),	// Dpad
	
				GamepadProfile.KeySource.Key(0),				// A
				GamepadProfile.KeySource.Key(1),				// B
				GamepadProfile.KeySource.Key(2),				// X
				GamepadProfile.KeySource.Key(3),				// Y
				
				GamepadProfile.KeySource.Key(6),				// Select
				GamepadProfile.KeySource.Key(7),				// Start
	
				GamepadProfile.KeySource.Key(4),				// L1
				GamepadProfile.KeySource.Key(5),				// R1
				GamepadProfile.KeySource.Empty(),				// L2
				GamepadProfile.KeySource.Empty(),				// R2
				GamepadProfile.KeySource.Key(8),				// L3
				GamepadProfile.KeySource.Key(9)					// R3
				),


			// Playstation Twin USB Adapter (Unity 5.2.2) -----------------

			new GamepadProfile(
				"PSX", 
				"Twin USB Joystick",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Win,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick (or d-pad in digital mode)
				GamepadProfile.JoystickSource.Axes(3, true, 2, false),	// Right Stick (right-down)
				GamepadProfile.JoystickSource.Axes(4, true, 5, true),	// Dpad (right-UP!)
	
				GamepadProfile.KeySource.Key(2),				// Cross
				GamepadProfile.KeySource.Key(1),				// Circle
				GamepadProfile.KeySource.Key(3),				// Square
				GamepadProfile.KeySource.Key(0),				// Triangle
				
				GamepadProfile.KeySource.Key(8),				// Select
				GamepadProfile.KeySource.Key(9),				// Start
	
				GamepadProfile.KeySource.Key(6),				// L1
				GamepadProfile.KeySource.Key(7),				// R1
				GamepadProfile.KeySource.Key(4),				// L2
				GamepadProfile.KeySource.Key(5),				// R2
				GamepadProfile.KeySource.Key(10),				// L3
				GamepadProfile.KeySource.Key(11)				// R3
				),

			};


		// Generic profile based on XBOX 360...

		this.genericProfile =  new GamepadProfile.GenericProfile(
				leftStick	: GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				rightStick	: GamepadProfile.JoystickSource.Axes(3, true, 4, false),	// Right Stick
				dpad			: GamepadProfile.JoystickSource.Axes(5, true, 6, true),	// Dpad
	
			   faceBottom	: GamepadProfile.KeySource.Key(0),				// A
				faceRight	: GamepadProfile.KeySource.Key(1),				// B
				faceLeft		: GamepadProfile.KeySource.Key(2),				// X
				faceTop		: GamepadProfile.KeySource.Key(3),				// Y
				
				select		: GamepadProfile.KeySource.Key(6),				// Select
				start			: GamepadProfile.KeySource.Key(7),				// Start
	
				L1				: GamepadProfile.KeySource.Key(4),				// L1
				R1				: GamepadProfile.KeySource.Key(5),				// R1
				L2				: GamepadProfile.KeySource.PlusAxis(8),	// L2
				R2				: GamepadProfile.KeySource.PlusAxis(9),	// R2
				L3				: GamepadProfile.KeySource.Key(8),				// L3
				R3				: GamepadProfile.KeySource.Key(9)				// R3
				);


		}
	}
}



//! \endcond
