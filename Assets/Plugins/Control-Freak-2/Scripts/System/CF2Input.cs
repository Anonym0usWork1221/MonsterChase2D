// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------


#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 
	#define UNITY_PRE_5_0
#endif

#if UNITY_PRE_5_0 || UNITY_5_0 
	#define UNITY_PRE_5_1
#endif

#if UNITY_PRE_5_1 || UNITY_5_1 
	#define UNITY_PRE_5_2
#endif

#if UNITY_PRE_5_2 || UNITY_5_2 
	#define UNITY_PRE_5_3
#endif

#if UNITY_PRE_5_3 || UNITY_5_3 
	#define UNITY_PRE_5_4
#endif

using UnityEngine;
using System.Collections.Generic;
using ControlFreak2.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace ControlFreak2
{
 
// ---------------------
//! Unity Input class replacement.
// ---------------------
static public class CF2Input 
	{
#if UNITY_EDITOR && UNITY_PRE_5_3
	private const bool 
		simulateMouseWithTouchesWhenRigIsActive = true;	// This the only way to make Unity Remote usable in Unity 5.2 and below.
#else
	private const bool 
		simulateMouseWithTouchesWhenRigIsActive = false;
#endif

	// ----------------
	//! Mobile Mode.
	// ----------------
	public enum MobileMode
		{	
		Auto,			//!< Auto-enable on handheld devices with a touchscreen. 
		Enabled,		//!< Forced mobile controls.
		Disabled		//!< Always disabled.
		}	
		
	static private MobileMode 
		mMobileMode =
#if (CF_FORCE_MOBILE_MODE || (UNITY_EDITOR && !CF_DONT_FORCE_MOBILE_MODE_IN_EDITOR))
			MobileMode.Enabled;	
#else
			MobileMode.Auto;
#endif

	static private bool
		mSimulateMouseWithTouches;


	// -------------------
	static public event System.Action
		onMobileModeChange;			//!< Called whenever mobile mode changes.

	static public event System.Action
		onActiveRigChange;			//!< Called whenever active Input Rig changes.

	

	// -----------------
	//! Get current mobile mode.
	// -----------------
	static public MobileMode GetMobileMode()
		{ return mMobileMode; }
		

	// ------------------
	//! Set mobile mode.
	// ------------------
	static public void SetMobileMode(MobileMode mode)	
		{ 
		mMobileMode = mode;

		if (onMobileModeChange != null)
			onMobileModeChange();

		CFCursor.InternalRefresh();
		}
		

	// --------------------
	//! Returns true when actually in mobile mode (automatic or forced). 
	// --------------------
	static public bool IsInMobileMode()					
		{ 
		return ((mMobileMode == MobileMode.Enabled) || ((mMobileMode == MobileMode.Auto) && IsMobilePlatform()));		
		}

			
	// -------------------
	//! Is running on mobile platform?
	// --------------------
	static public bool IsMobilePlatform()
		{		
#if UNITY_IPHONE || UNITY_WP8
		return true;
#else		
		return (Application.isMobilePlatform); 	
#endif
		}


//! \cond

	// --------------------
	[System.ObsoleteAttribute("Use .IsInMobileMode()")]
	static public bool ControllerActive()
		{
		return IsInMobileMode();
		}

//! \endcond
		
	
	// -------------------		
	static private InputRig 
		mRig;		

	//! Active Input Rig.
	static public InputRig activeRig
		{
		get 
			{ return mRig; }
	
		// --------------
		set 
			{
			if (value == mRig)
				return;

			if (mRig != null)
				mRig.OnDisactivateRig();
			else
				mSimulateMouseWithTouches = UnityEngine.Input.simulateMouseWithTouches;
			
			mRig = value;
			if (mRig != null)	
				{
				mRig.OnActivateRig();
	
				UnityEngine.Input.simulateMouseWithTouches = simulateMouseWithTouchesWhenRigIsActive;
				}
			else
				{
				UnityEngine.Input.simulateMouseWithTouches = mSimulateMouseWithTouches;
				}
	
			if (onActiveRigChange != null)
				onActiveRigChange();

			CFCursor.InternalRefresh();
			}
		}
		

		

	// ---------------
	//! Reset active Input Rig.
	// ---------------
	static public void ResetInputAxes()
		{
		if (mRig != null) 
			mRig.ResetInputAxes();

		UnityEngine.Input.ResetInputAxes();

		}
	
	// ---------------------------
	/// \name Named axis methods
	/// \{
	// ---------------------------

	// --------------
	static public float GetAxis(string axisName)						
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureAxis(axisName);
#endif 
		return ((mRig != null) ? mRig.GetAxis(axisName)				: Input.GetAxis(axisName)); 
		}


	static public float GetAxis(string axisName, ref int cachedId)		
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureAxis(axisName);
#endif 
 
		return ((mRig != null) ? mRig.GetAxis(axisName, ref cachedId)	: Input.GetAxis(axisName)); 
		}

	// --------------
	static public float GetAxisRaw(string axisName)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureAxis(axisName);
#endif 
		return ((mRig != null) ? mRig.GetAxisRaw(axisName)			: Input.GetAxisRaw(axisName)); 
		}


	static public float GetAxisRaw(string axisName, ref int cachedId)	
		{ 
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureAxis(axisName);
#endif 
		return ((mRig != null) ? mRig.GetAxisRaw(axisName, ref cachedId) : Input.GetAxisRaw(axisName)); 
		}

	/// \}


	// ---------------------------
	/// \name Named button methods
	/// \{
	// ---------------------------

	// --------------
	static public bool	GetButton(string axisName)				
		{ 
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureButton(axisName);
#endif 
		return ((mRig != null) ? mRig.GetButton(axisName)					: Input.GetButton(axisName)); 
		}


	static public bool	GetButton(string axisName, ref int cachedId)
		{ 
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureButton(axisName);
#endif 
		return ((mRig != null) ? mRig.GetButton(axisName, ref cachedId)	: Input.GetButton(axisName)); 
		}


	// --------------!
	static public bool	GetButtonDown(string axisName)					
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureButton(axisName);
#endif 
		return ((mRig != null) ? mRig.GetButtonDown(axisName)				: Input.GetButtonDown(axisName)); 
		}


	static public bool	GetButtonDown(string axisName, ref int cachedId)
		{ 
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureButton(axisName);
#endif 
		return ((mRig != null) ? mRig.GetButtonDown(axisName, ref cachedId): Input.GetButtonDown(axisName)); 
		}

	// --------------
	static public bool	GetButtonUp(string axisName)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureButton(axisName);
#endif 
		return ((mRig != null) ? mRig.GetButtonUp(axisName)				: Input.GetButtonUp(axisName));
		}

	static public bool	GetButtonUp(string axisName, ref int cachedId)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureButton(axisName);
#endif 
		return ((mRig != null) ? mRig.GetButtonUp(axisName, ref cachedId)	: Input.GetButtonUp(axisName)); 
		}
			
	/// \}

	
	// ---------------------------
	/// \name Key methods
	/// \{
	// ---------------------------
		
	// --------------
	static public bool	GetKey(KeyCode keyCode)			
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(keyCode);
#endif 
		return ((mRig != null) ? mRig.GetKey(keyCode)		: Input.GetKey(keyCode));
		}

	// --------------
	static public bool	GetKeyDown(KeyCode keyCode)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(keyCode);
#endif 
		return ((mRig != null) ? mRig.GetKeyDown(keyCode)	: Input.GetKeyDown(keyCode));
		}

	// --------------
	static public bool	GetKeyUp(KeyCode keyCode)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(keyCode);
#endif 
		return ((mRig != null) ? mRig.GetKeyUp(keyCode)	: Input.GetKeyUp(keyCode));
		}


	// --------------
	[System.Obsolete("Please, use GetKey(KeyCode) version instead!")]
	static public bool	GetKey(string keyName)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(InputRig.NameToKeyCode(keyName));
#endif 
		return ((mRig != null) ? mRig.GetKey(keyName)		: Input.GetKey(keyName));
		}

	// --------------
	[System.Obsolete("Please, use GetKeyDown(KeyCode) version instead!")]
	static public bool	GetKeyDown(string keyName)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(InputRig.NameToKeyCode(keyName));
#endif 
		return ((mRig != null) ? mRig.GetKeyDown(keyName)	: Input.GetKeyDown(keyName)); 
		}

	// --------------
	[System.Obsolete("Please, use GetKeyUp(KeyCode) version instead!")]
	static public bool	GetKeyUp(string keyName)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(InputRig.NameToKeyCode(keyName));
#endif 
		return ((mRig != null) ? mRig.GetKeyUp(keyName)	: Input.GetKeyUp(keyName));
		}


	// --------------------------
	static public bool anyKey		{ get { return ((mRig != null) ? mRig.AnyKey() : UnityEngine.Input.anyKey); } }


	// --------------------------
	static public bool anyKeyDown	{ get { return ((mRig != null) ? mRig.AnyKeyDown() : UnityEngine.Input.anyKeyDown); } }

		
	/// \}
		

	// ---------------------------
	/// \name Emulated touch methods
	/// \{
	// ---------------------------

	// -----------------
	static public int touchCount				
		{ 
		get { 
#if UNITY_EDITOR
			ControlFreak2Editor.Assistant.CaptureTouch();
#endif 
			return ((mRig != null) ? mRig.GetEmuTouchCount()	: Input.touchCount); 
			} 
		}

	// -----------------
	static public InputRig.Touch[] touches
		{
		get { 
#if UNITY_EDITOR
			ControlFreak2Editor.Assistant.CaptureTouch();
#endif 
			return ((mRig != null) ? mRig.GetEmuTouchArray()	: InputRig.Touch.TranslateUnityTouches(Input.touches)); 
			}
		}
	
	// -----------------
	static public InputRig.Touch	GetTouch(int i)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureTouch();
#endif 
		return ((mRig != null) ? mRig.GetEmuTouch(i) 			: (new InputRig.Touch(Input.GetTouch(i)))); 
		}

	/// \}



	// ---------------------------
	/// \name Mouse button methods
	/// \{
	// ---------------------------

	// --------------
	static public bool	GetMouseButton(int mouseButton)		
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(InputRig.MouseButtonToKey(mouseButton));
#endif 
		return ((mRig != null) ? mRig.GetMouseButton(mouseButton)		: Input.GetMouseButton(mouseButton));
		}

	// --------------
	static public bool	GetMouseButtonDown(int mouseButton)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(InputRig.MouseButtonToKey(mouseButton));
#endif 
		return ((mRig != null) ? mRig.GetMouseButtonDown(mouseButton)	: Input.GetMouseButtonDown(mouseButton)); 
		}

	// --------------
	static public bool	GetMouseButtonUp(int mouseButton)
		{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureKey(InputRig.MouseButtonToKey(mouseButton));
#endif 
		return ((mRig != null) ? mRig.GetMouseButtonUp(mouseButton)	: Input.GetMouseButtonUp(mouseButton));
		}

		
	/// \} 
	

	// ---------------------------
	/// \name Mouse properties.
	/// \{
	// ---------------------------

	// --------------
	static public Vector3	mousePosition 
		{
		get {
#if UNITY_EDITOR
			ControlFreak2Editor.Assistant.CaptureMousePos();
#endif 
			return ((mRig == null) ? UnityEngine.Input.mousePosition : mRig.mouseConfig.GetPosition()); 
			}
		}


	// -------------------
	static public Vector2 mouseScrollDelta
		{
		get {
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureScrollWheel();
#endif 
			return ((mRig == null) ? new Vector2(UnityEngine.Input.mouseScrollDelta.x, UnityEngine.Input.mouseScrollDelta.y) : mRig.scrollWheel.GetDelta()); 
			}
		}

	// ---------------------
	static public bool simulateMouseWithTouches
		{
		get 
			{ return ((mRig == null) ? UnityEngine.Input.simulateMouseWithTouches : mSimulateMouseWithTouches); }

		set 
			{
			mSimulateMouseWithTouches = value;	
			if (mRig == null)
				UnityEngine.Input.simulateMouseWithTouches = mSimulateMouseWithTouches;
			else
				UnityEngine.Input.simulateMouseWithTouches = simulateMouseWithTouchesWhenRigIsActive;
				
			} 
		
		}

	//! \}




	// --------------------
	//! Calibrate active rig's tilt.
	// --------------------
	static public void CalibrateTilt()
		{
		if (CF2Input.activeRig != null)
			CF2Input.activeRig.CalibrateTilt();
		}

		
	

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Control Freak 2/Tools/Mobile Mode/Automatic", true, 100)]
	static private bool SetMoblieModeToAutoValidator() { return (GetMobileMode() != MobileMode.Auto); }
	[UnityEditor.MenuItem("Control Freak 2/Tools/Mobile Mode/Automatic", false, 100)]
	static private void SetMoblieModeToAuto() { SetMobileMode(MobileMode.Auto); }

	[UnityEditor.MenuItem("Control Freak 2/Tools/Mobile Mode/On", true, 100)]
	static private bool SetMoblieModeToOnValidator() { return (GetMobileMode() != MobileMode.Enabled); }
	[UnityEditor.MenuItem("Control Freak 2/Tools/Mobile Mode/On", false, 100)]
	static private void SetMoblieModeToOn() { SetMobileMode(MobileMode.Enabled); }

	[UnityEditor.MenuItem("Control Freak 2/Tools/Mobile Mode/Off", true, 100)]
	static private bool SetMoblieModeToOffValidator() { return (GetMobileMode() != MobileMode.Disabled); }
	[UnityEditor.MenuItem("Control Freak 2/Tools/Mobile Mode/Off", false, 100)]
	static private void SetMoblieModeToOff() { SetMobileMode(MobileMode.Disabled); }

#endif

	
	}
}
