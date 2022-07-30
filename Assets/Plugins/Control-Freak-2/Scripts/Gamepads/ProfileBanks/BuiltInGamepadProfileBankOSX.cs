// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2
{

public class BuiltInGamepadProfileBankOSX : BuiltInGamepadProfileBank
	{
	// ------------------
	public BuiltInGamepadProfileBankOSX() : base()
		{
		this.profiles = new GamepadProfile[]
			{
			// MOGA Hero Power Mode B (Unity 5.2.2) -----------------

			new GamepadProfile(
				"MOGA", 
				"Unknown Moga 2 HID",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Mac,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// Right Stick
				GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad
	
				GamepadProfile.KeySource.Key(0),				// A
				GamepadProfile.KeySource.Key(1),				// B
				GamepadProfile.KeySource.Key(2),				// X
				GamepadProfile.KeySource.Key(3),				// Y
				
				GamepadProfile.KeySource.Key(6),				// Select
				GamepadProfile.KeySource.Key(7),				// Start
	
				GamepadProfile.KeySource.Key(4),				// L1
				GamepadProfile.KeySource.Key(5),				// R1
				GamepadProfile.KeySource.PlusAxis(6),			// L2
				GamepadProfile.KeySource.PlusAxis(7),			// R2
				GamepadProfile.KeySource.Key(8),				// L3
				GamepadProfile.KeySource.Key(9)					// R3
				),


			// MOGA Generic -----------------

			new GamepadProfile(
				"MOGA", 
				"Moga",
				GamepadProfile.ProfileMode.Regex,
				null, null,		
				//GamepadProfile.PlatformFlag.Mac,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// Right Stick
				GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad
	
				GamepadProfile.KeySource.Key(0),				// A
				GamepadProfile.KeySource.Key(1),				// B
				GamepadProfile.KeySource.Key(2),				// X
				GamepadProfile.KeySource.Key(3),				// Y
				
				GamepadProfile.KeySource.Key(6),				// Select
				GamepadProfile.KeySource.Key(7),				// Start
	
				GamepadProfile.KeySource.Key(4),				// L1
				GamepadProfile.KeySource.Key(5),				// R1
				GamepadProfile.KeySource.PlusAxis(6),			// L2
				GamepadProfile.KeySource.PlusAxis(7),			// R2
				GamepadProfile.KeySource.Key(8),				// L3
				GamepadProfile.KeySource.Key(9)					// R3
				),


			// DualShock 3 (Unity 5.2.2) -----------------

			new GamepadProfile(
				"DualShock 3", 
				"Sony PLAYSTATION(R)3 Controller",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Mac,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// Right Stick
				GamepadProfile.JoystickSource.Dpad(4, 5, 6, 7),			// Dpad
	
				GamepadProfile.KeySource.Key(14),				// Cross
				GamepadProfile.KeySource.Key(13),				// Circle
				GamepadProfile.KeySource.Key(12),				// Triangle
				GamepadProfile.KeySource.Key(15),				// Square
				
				GamepadProfile.KeySource.Key(0),				// Select
				GamepadProfile.KeySource.Key(3),				// Start
	
				GamepadProfile.KeySource.Key(10),				// L1
				GamepadProfile.KeySource.Key(11),				// R1
				GamepadProfile.KeySource.Key(8),				// L2
				GamepadProfile.KeySource.Key(9),				// R2
				GamepadProfile.KeySource.Key(1),				// L3
				GamepadProfile.KeySource.Key(2)					// R3
				),

			// Twin PSX-USB Converter -----------------

			new GamepadProfile(
				"PSX", 
				"Twin USB Joystick",
				GamepadProfile.ProfileMode.Regex,
				null, null,		
				//GamepadProfile.PlatformFlag.Mac,
	
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// Left Stick
				GamepadProfile.JoystickSource.Axes(1, true, 0, false),	// Right Stick
				GamepadProfile.JoystickSource.Axes(8, true, 9, false),	// Dpad
	
				GamepadProfile.KeySource.Key(2),				// Cross
				GamepadProfile.KeySource.Key(1),				// Circle
				GamepadProfile.KeySource.Key(0),				// Triangle
				GamepadProfile.KeySource.Key(3),				// Square
				
				GamepadProfile.KeySource.Key(8),				// Select
				GamepadProfile.KeySource.Key(9),				// Start
	
				GamepadProfile.KeySource.Key(6),				// L1
				GamepadProfile.KeySource.Key(7),				// R1
				GamepadProfile.KeySource.Key(4),				// L2
				GamepadProfile.KeySource.Key(5),				// R2
				GamepadProfile.KeySource.Key(10),			// L3
				GamepadProfile.KeySource.Key(11)				// R3
				),



	
			};
		}
	}
}

//! \endcond

