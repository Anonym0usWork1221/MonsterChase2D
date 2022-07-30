// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2
{

public class BuiltInGamepadProfileBankPS4 : BuiltInGamepadProfileBank
	{
	// ------------------
	public BuiltInGamepadProfileBankPS4() : base()
		{

			
		this.profiles = new GamepadProfile[]
			{	

			// Official controller (EXPERIMENTAL)  ------------------

			new GamepadProfile(              
              "PS4", 
              "PS4", 
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
