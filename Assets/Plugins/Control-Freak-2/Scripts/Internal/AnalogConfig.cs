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
public class AnalogConfig
	{
	public float analogDeadZone; 

	public float analogEndZone; 	

	public float analogRangeStartValue; 
	
	public float digitalEnterThresh; 

	public float digitalLeaveThresh; 
		
	
	public float 
		digitalToAnalogPressSpeed,
		digitalToAnalogReleaseSpeed;

	public const float
		DIGITAL_TO_ANALOG_SMOOTHING_TIME = 0.2f;


	public bool	
		useRamp;
	public AnimationCurve
		ramp;


	const float MIN_DIGITAL_DEADZONE	= 0.05f;
	const float MAX_DIGITAL_DEADZONE	= 0.9f;
	const float MIN_ANALOG_DEADZONE		= 0.0f;
	const float MAX_ANALOG_DEADZONE		= 0.9f;
	const float MIN_ANALOG_ENDZONE		= 0.1f;
	const float MAX_ANALOG_ENDZONE		= 1.0f;
	
	const float MAX_DIGI_LEAVE_MARGIN	= 0.1f;
	const float MIN_DIGI_LEAVE_THRESH	= 0.1f;




	// -----------------
	public AnalogConfig()
		{
		this.analogDeadZone					= 0.1f;
		this.analogEndZone					= 1.0f;
		this.analogRangeStartValue			= 0;
		this.digitalEnterThresh				= 0.5f;
		this.digitalLeaveThresh				= 0.2f;
		this.digitalToAnalogPressSpeed		= 0.5f;
		this.digitalToAnalogReleaseSpeed	= 0.5f;

		//this.digitalMagnetStrength	= 0;

		this.ramp = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.5f), new Keyframe(1, 1)); 
		}


	// -----------------
	public float GetAnalogVal(float rawVal)
		{
		float absRawVal = Mathf.Abs(rawVal);

		if (absRawVal <= this.analogDeadZone)
			return 0;
		if (absRawVal >= this.analogEndZone)
			return (rawVal >= 0) ? 1 : -1;

		float v = ((absRawVal - this.analogDeadZone) / (this.analogEndZone - this.analogDeadZone));

		if (this.useRamp && (this.ramp != null))
			v = Mathf.Clamp01(this.ramp.Evaluate(v));
		else
			v = Mathf.Lerp(this.analogRangeStartValue, 1, v);

		return ((rawVal >= 0) ? v : -v);	
		}


	// ----------------
	public int GetSignedDigitalVal(float rawVal, int prevDigiVal)
		{
		if (rawVal >= 0)
			{
			return ((rawVal > ((prevDigiVal == 1) ? this.digitalLeaveThresh : this.digitalEnterThresh)) ? 1 : 0);
			}
		else
			return ((rawVal < ((prevDigiVal == -1) ? -this.digitalLeaveThresh : -this.digitalEnterThresh)) ? -1 : 0);
		}
	

	// ----------------
	public bool GetDigitalVal(float rawVal, bool prevDigiVal)
		{
		return ((rawVal > ((prevDigiVal) ? this.digitalLeaveThresh : this.digitalEnterThresh)));
		}
		

	// ---------------
	public float AnimateDigitalToAnalog(float curVal, float targetVal, bool pressed)
		{
		return CFUtils.SmoothTowards(curVal, targetVal, 
			(pressed ? this.digitalToAnalogPressSpeed : this.digitalToAnalogReleaseSpeed) * DIGITAL_TO_ANALOG_SMOOTHING_TIME, CFUtils.realDeltaTime, 0.001f);
		}







	}
}

//! \endcond

