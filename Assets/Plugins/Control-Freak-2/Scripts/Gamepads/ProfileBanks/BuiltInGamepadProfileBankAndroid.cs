// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------


//! \cond

using UnityEngine;

namespace ControlFreak2
{

public class BuiltInGamepadProfileBankAndroid : BuiltInGamepadProfileBank
	{
	// ------------------
	public BuiltInGamepadProfileBankAndroid() : base()
		{
		// Android Generic Profile Based on MOGA / Xbox360 / DS3 ...

		this.genericProfile = new GamepadProfile.GenericProfile(

			GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// LS
			GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// RS
			GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad

			GamepadProfile.KeySource.Key(0),		// A
			GamepadProfile.KeySource.Key(1),		// B
			GamepadProfile.KeySource.Key(2),		// X
			GamepadProfile.KeySource.Key(3),		// Y
			
			GamepadProfile.KeySource.Key(11),		// Select	
			GamepadProfile.KeySource.Key(10),		// Start	// ENTER

			GamepadProfile.KeySource.Key(4),			// L1	// Left Shift
			GamepadProfile.KeySource.Key(5),			// R1
			GamepadProfile.KeySource.KeyAndPlusAxis(6, 6),	// L2
			GamepadProfile.KeySource.KeyAndPlusAxis(7, 7),	// R2
			GamepadProfile.KeySource.Key(8),			// L3
			GamepadProfile.KeySource.Key(9)			// R3
			);


		this.profiles = new GamepadProfile[]
			{
			
			// 8Bitdo FC30 PRO ------------------------

			new GamepadProfile(
				"8Bitdo FC30 PRO", 
				"8Bitdo",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// LS
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// RS
				GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad
	
				GamepadProfile.KeySource.Key(0),		// A
				GamepadProfile.KeySource.Key(1),		// B
				GamepadProfile.KeySource.Key(2),		// X
				GamepadProfile.KeySource.Key(3),		// Y
				
				GamepadProfile.KeySource.Key(11),		// Select	
				GamepadProfile.KeySource.Key(10),		// Start
	
				GamepadProfile.KeySource.Key(4),		// L1
				GamepadProfile.KeySource.Key(5),		// R1
				GamepadProfile.KeySource.Key(6),	// L2
				GamepadProfile.KeySource.Key(7),	// R2
				GamepadProfile.KeySource.Key(8),		// L3
				GamepadProfile.KeySource.Key(9)		// R3
				),	
		

			// MOGA (Hero Power) ------------------------

			new GamepadProfile(
				"MOGA", 
				"Broadcom Bluetooth HID",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Android,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// LS
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// RS
				GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad
	
				GamepadProfile.KeySource.Key(0),		// A
				GamepadProfile.KeySource.Key(1),		// B
				GamepadProfile.KeySource.Key(2),		// X
				GamepadProfile.KeySource.Key(3),		// Y
				
				GamepadProfile.KeySource.Empty(),		// Select	// ESCAPE
				GamepadProfile.KeySource.Key(10),		// Start
	
				GamepadProfile.KeySource.Key(4),		// L1
				GamepadProfile.KeySource.Key(5),		// R1
				GamepadProfile.KeySource.Empty(), 	//PlusAxis(6),	// L2
				GamepadProfile.KeySource.Empty(),	//PlusAxis(7),	// R2
				GamepadProfile.KeySource.Key(8),		// L3
				GamepadProfile.KeySource.Key(9)		// R3
				),	
		


		

			// Xbox 360 -----------------------------
			new GamepadProfile(
				"XBOX 360", 
				"Microsoft X-Box 360 pad",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Android,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// LS
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// RS
				GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad
	
				GamepadProfile.KeySource.Key(0),		// A
				GamepadProfile.KeySource.Key(1),		// B
				GamepadProfile.KeySource.Key(2),		// X
				GamepadProfile.KeySource.Key(3),		// Y
				
				GamepadProfile.KeySource.Empty(),		// Select	// ESCAPE
				GamepadProfile.KeySource.Key(10),		// Start	// ENTER
	
				GamepadProfile.KeySource.Key(4),		// L1
				GamepadProfile.KeySource.Key(5),		// R1
				GamepadProfile.KeySource.PlusAxis(6),	// L2
				GamepadProfile.KeySource.PlusAxis(7),	// R2
				GamepadProfile.KeySource.Key(8),		// L3
				GamepadProfile.KeySource.Key(9)			// R3
				),	


			// Xbox 360 catch-all -----------------------------
			new GamepadProfile(
				"XBOX 360", 
				"(X-Box)|(Xbox)",
				GamepadProfile.ProfileMode.Regex,
				null, null,		
				//GamepadProfile.PlatformFlag.Android,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// LS
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// RS
				GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad
	
				GamepadProfile.KeySource.Key(0),		// A
				GamepadProfile.KeySource.Key(1),		// B
				GamepadProfile.KeySource.Key(2),		// X
				GamepadProfile.KeySource.Key(3),		// Y
				
				GamepadProfile.KeySource.Empty(),		// Select	// ESCAPE
				GamepadProfile.KeySource.Key(10),		// Start	// ENTER
	
				GamepadProfile.KeySource.Key(4),		// L1
				GamepadProfile.KeySource.Key(5),		// R1
				GamepadProfile.KeySource.PlusAxis(6),	// L2
				GamepadProfile.KeySource.PlusAxis(7),	// R2
				GamepadProfile.KeySource.Key(8),		// L3
				GamepadProfile.KeySource.Key(9)			// R3
				),	


			// DualShock 3 Controller ------------------------
			new GamepadProfile(
				"DualShock3", 
				"Sony PLAYSTATION(R)3 Controller",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Android,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// LS
				GamepadProfile.JoystickSource.Axes(2, true, 3, false),	// RS
				GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad
	
				GamepadProfile.KeySource.Key(0),		// A
				GamepadProfile.KeySource.Key(1),		// B
				GamepadProfile.KeySource.Key(2),		// X
				GamepadProfile.KeySource.Key(3),		// Y
				
				GamepadProfile.KeySource.Key(11),		// Select	
				GamepadProfile.KeySource.Key(10),		// Start	// ENTER
	
				GamepadProfile.KeySource.Key(4),		// L1	// Left Shift
				GamepadProfile.KeySource.Key(5),		// R1
				GamepadProfile.KeySource.PlusAxis(6),	// L2
				GamepadProfile.KeySource.PlusAxis(7),	// R2
				GamepadProfile.KeySource.Key(8),		// L3
				GamepadProfile.KeySource.Key(9)			// R3
				),			

			// Playstation Twin USB Adapter (Unity 5.2.2) -----------------

			new GamepadProfile(
				"PSX", 
				"Twin USB Joystick",
				GamepadProfile.ProfileMode.Normal,
				null, null,		
				//GamepadProfile.PlatformFlag.Android,
	
				GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// Left Stick (or d-pad in digital mode) (right-down)
				GamepadProfile.JoystickSource.Axes(3, true, 2, false),	// Right Stick (right-down)
				GamepadProfile.JoystickSource.Axes(4, true, 5, false),	// Dpad (right-down!)
	
				GamepadProfile.KeySource.Key(17),				// Cross
				GamepadProfile.KeySource.Key(16),				// Circle
				GamepadProfile.KeySource.Key(18),				// Square
				GamepadProfile.KeySource.Key(15),				// Triangle
				
				GamepadProfile.KeySource.Key(-1),				// Select
				GamepadProfile.KeySource.Key(-1),				// Start
	
				GamepadProfile.KeySource.Key(-1),				// L1
				GamepadProfile.KeySource.Key(-1),				// R1
				GamepadProfile.KeySource.Key(19),				// L2
				GamepadProfile.KeySource.Key(-1),				// R2
				GamepadProfile.KeySource.Key(-1),				// L3
				GamepadProfile.KeySource.Key(-1)				// R3
				),	
			};
		}
	}
}

//! \endcond

