// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2.Internal
{
	
[System.Serializable]
public class JoystickConfig : AnalogConfig
	{
	public enum ClampMode
		{
		Circle,		//!< Clamp inside a circle.
		Square	 	//!< Clamp inside a square.
		}	
		

	public enum StickMode
		{
		Analog,		//!< Analog Stick.
		Digital4,	//!< Digital 4-way.
		Digital8		//!< Digital 8-way.
		}

	public enum DigitalDetectionMode
		{
		Touch,
		Joystick
		}


	public ClampMode 
		clampMode;

	public bool
		perAxisDeadzones; //!< Use separate deadzones for each axis. (Only in Square Clamp Mode)

	public StickMode 
		stickMode;

	public DirectionState.OriginalDirResetMode
		originalDirResetMode;


	public float 
		angularMagnet;

	public DigitalDetectionMode
		digitalDetectionMode;


	


	public bool
		blockX,
		blockY;
	


	const float MIN_SAFE_TILT			= 0.01f;
	const float MIN_SAFE_TILT_SQ		= MIN_SAFE_TILT * MIN_SAFE_TILT;

	const float MIN_DIGITAL_DEADZONE	= 0.05f;
	const float MAX_DIGITAL_DEADZONE	= 0.9f;
	const float MIN_ANALOG_DEADZONE		= 0.0f;
	const float MAX_ANALOG_DEADZONE		= 0.9f;
	const float MIN_ANALOG_ENDZONE		= 0.1f;
	const float MAX_ANALOG_ENDZONE		= 1.0f;
	
	const float MAX_DIGI_LEAVE_MARGIN	= 0.4f;
	const float MIN_DIGI_LEAVE_THRESH	= 0.1f;

	const float DIGI_MAX_4WAY_ANGLE_MAGNET = 22.5f;
	const float DIGI_MAX_8WAY_ANGLE_MAGNET = 11.75f;



	// -----------------
	public JoystickConfig() : base()
		{
		this.clampMode			= ClampMode.Square;
		this.angularMagnet	= 0.5f;

		this.digitalLeaveThresh = 0.3f;
		this.digitalEnterThresh = 0.6f;
		}



	// --------------
	public Vector2 AnimateDirToAnalogVec(Vector2 curVec, Dir targetDir)
		{
		float digiToAnalogSpeed = ((targetDir == Dir.N) ? this.digitalToAnalogReleaseSpeed : this.digitalToAnalogPressSpeed);
		
		return CFUtils.SmoothTowardsVec2(curVec, CFUtils.DirToVector(targetDir, (this.clampMode == ClampMode.Circle)), 
			(digiToAnalogSpeed * DIGITAL_TO_ANALOG_SMOOTHING_TIME), Time.unscaledTime, 0.0001f);	
		}


	// ------------------
	public Vector2 ClampNormalizedPos(Vector2 np)
		{
		return ((this.clampMode == ClampMode.Circle) ? CFUtils.ClampInsideUnitCircle(np) : CFUtils.ClampPerAxisInsideUnitSquare(np)); //CFUtils.ClampInsideUnitSquare(np));
		}

	}
}

//! \endcond

