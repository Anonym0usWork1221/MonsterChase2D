// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2
{

public class BuiltInGamepadProfileBankWebGL : BuiltInGamepadProfileBank
	{
	// ------------------
	public BuiltInGamepadProfileBankWebGL() : base()
		{
		this.profiles = new GamepadProfile[]
			{	
			// Xbox 360 (Unity 5.4.2b) WEBGL Windows -----------------

			new GamepadProfile(
				"XBOX 360", 
				"xinput",
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


			// Playstation Twin USB Adapter (Unity 5.4.1b WebGL Windows) -----------------

			new GamepadProfile(
				"PSX", 
				"Twin USB Joystick", //"0810-0001-Twin USB Joystick",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick (or d-pad in digital mode)
				GamepadProfile.JoystickSource.Axes(3, true, 2, false),	// Right Stick (right-down)
				GamepadProfile.JoystickSource.Dpad(13, 16, 14, 15),		//int keyU, int keyR, int keyD, int keyL)	.Keys(.Axes(4, true, 5, true),	// Dpad (right-UP!)
	
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

		// MOGA Catch-all (Unity 5.4.1b) Windows -----------------

			new GamepadProfile(
				"MOGA", 
				"Android Controller", //"2046-89e5-Android Controller Gen-2(ACC)",
				GamepadProfile.ProfileMode.Regex,
				null, null,
				//GamepadProfile.PlatformFlag.Win,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// Right Stick
				GamepadProfile.JoystickSource.Dpad(10, 13, 11, 12),	// Dpad (URDL)
	
				GamepadProfile.KeySource.Key(0),				// A
				GamepadProfile.KeySource.Key(1),				// B
				GamepadProfile.KeySource.Key(3),				// X
				GamepadProfile.KeySource.Key(4),				// Y
				
				GamepadProfile.KeySource.Key(-1),				// Select
				GamepadProfile.KeySource.Key(11),				// Start
	
				GamepadProfile.KeySource.Key(6),				// L1
				GamepadProfile.KeySource.Key(7),				// R1
				GamepadProfile.KeySource.PlusAxis(5),		// L2 (-1..1 !!)
				GamepadProfile.KeySource.PlusAxis(4),		// R2 (-1..1 !!)
				GamepadProfile.KeySource.Key(13),			// L3
				GamepadProfile.KeySource.Key(-1)				// R3
				),



			};
		}
	}
}

//! \endcond
