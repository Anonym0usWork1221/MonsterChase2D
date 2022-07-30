// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

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
using System.Text.RegularExpressions;

namespace ControlFreak2
{
	

[System.Serializable]
public class GamepadProfile
	{
	
	public const int 
		CAP_DPAD			= (1 << 0),
		CAP_LEFT_STICK		= (1 << 1),
		CAP_RIGHT_STICK		= (1 << 2),
		CAP_START			= (1 << 3), 
		CAP_SELECT			= (1 << 4), 
		CAP_SHOULDER_KEYS	= (1 << 5),		// L1/R1
		CAP_ANALOG_TRIGGERS	= (1 << 6),		// ANAOG_TRIGGERS (L2/R2)
		CAP_PRESSABLE_STICKS= (1 << 7);		// L3/R3 Buttons
		

	public enum DeviceType
		{
		Unknown,
		PS3,
		PS4,
		Xbox360,
		XboxOne,
		MOGA,
		OUYA
		}

	public enum ProfileMode
		{
		Normal,
		Regex
		}



	public string		
		name,
		joystickIdentifier;

	public ProfileMode 
		profileMode;


	public string		
		unityVerFrom,
		unityVerTo;


	public JoystickSource
		leftStick,
		rightStick,
		dpad;		

	public KeySource
		keyFaceU,
		keyFaceR,
		keyFaceD,
		keyFaceL,

		keyStart,
		keySelect,	
		keyL1,	
		keyR1,	
		keyL2,
		keyR2,
		keyL3,
		keyR3;

	


	// ------------------
	public GamepadProfile()
		{
		this.name = "New Profile";
		this.joystickIdentifier	= "Device Identifier";
		this.profileMode = ProfileMode.Normal;
		
		this.unityVerFrom		= "4.7";
		this.unityVerTo		= "9.9";

		

		this.dpad		= new JoystickSource();
		this.leftStick	= new JoystickSource();
		this.rightStick	= new JoystickSource();

		this.keyFaceU	= KeySource.Empty();
		this.keyFaceR	= KeySource.Empty();
		this.keyFaceD	= KeySource.Empty();
		this.keyFaceL	= KeySource.Empty();
		this.keyStart	= KeySource.Empty();
		this.keySelect	= KeySource.Empty();
		this.keyL1		= KeySource.Empty();
		this.keyR1		= KeySource.Empty();
		this.keyL2		= KeySource.Empty();
		this.keyR2		= KeySource.Empty();
		this.keyL3		= KeySource.Empty();
		this.keyR3		= KeySource.Empty();
		}
	
		
	// ------------------
	public bool IsCompatible(string deviceName)
		{
		//string unityVer = Application.unityVersion;
		//if ((this.unityVerFrom.CompareTo(unityVer) >= 0) || (this.unityVerTo.CompareTo(unityVer) <= 0))
		//	return false;
			
		if (this.profileMode == ProfileMode.Normal)
			{
			if (deviceName.IndexOf(this.joystickIdentifier, System.StringComparison.OrdinalIgnoreCase) < 0)
				return false;
			}

		else if (this.profileMode == ProfileMode.Regex)
			{
			if (!Regex.IsMatch(deviceName, this.joystickIdentifier, (RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)))
				return false;
			}

		return true;
		}


	// --------------------
	public void AddSupportedVersion(string unityVer)
		{
		if (string.IsNullOrEmpty(unityVer))
			return;

		if (this.unityVerTo.CompareTo(unityVer) < 0)
			this.unityVerTo = unityVer;

		}
		

	// -------------------
	public JoystickSource GetJoystickSource(int id)
		{
		JoystickSource joy = null;
	
		switch (id)
			{
			case (int)GamepadManager.GamepadStick.LeftAnalog	: joy = this.leftStick; break;
			case (int)GamepadManager.GamepadStick.RightAnalog	: joy = this.rightStick; break;
			case (int)GamepadManager.GamepadStick.Dpad			: joy = this.dpad; break;
			}

		return joy;
		}

	// ------------------
	public KeySource GetKeySource(int id)
		{
		KeySource key = null;

		switch (id)
			{
			case (int)GamepadManager.GamepadKey.FaceBottom :	key = this.keyFaceD; break; 
			case (int)GamepadManager.GamepadKey.FaceRight	:	key = this.keyFaceR; break; 
			case (int)GamepadManager.GamepadKey.FaceLeft 	:	key = this.keyFaceL; break; 
			case (int)GamepadManager.GamepadKey.FaceTop	:	key = this.keyFaceU; break; 
			case (int)GamepadManager.GamepadKey.Start 		:	key = this.keyStart; break; 
			case (int)GamepadManager.GamepadKey.Select 		:	key = this.keySelect; break; 
			case (int)GamepadManager.GamepadKey.L1			:	key = this.keyL1; break; 
			case (int)GamepadManager.GamepadKey.R1			:	key = this.keyR1; break; 
			case (int)GamepadManager.GamepadKey.L2			:	key = this.keyL2; break; 
			case (int)GamepadManager.GamepadKey.R2			:	key = this.keyR2; break; 
			case (int)GamepadManager.GamepadKey.L3			:	key = this.keyL3; break; 
			case (int)GamepadManager.GamepadKey.R3			:	key = this.keyR3; break;
			} 
			
		return key;
		}

		
	// ------------------
	public bool IsDuplicateOf(GamepadProfile profile)		
		{
		if (!this.joystickIdentifier.Equals(profile.joystickIdentifier, System.StringComparison.OrdinalIgnoreCase))
			return false;
			

		for (int i = 0; i < GamepadManager.GamepadKeyCount; ++i)
			{
			if (!this.GetKeySource(i).IsDuplicateOf(profile.GetKeySource(i)))
				return false;
			}
			
		for (int i = 0; i < GamepadManager.GamepadStickCount; ++i)
			{
			if (!this.GetJoystickSource(i).IsDuplicateOf(profile.GetJoystickSource(i)))
				return false;
			}

		return true;
		}
	

	// --------------------
	public GamepadProfile(
		string 	name,
		string 	deviceIdentifier,	
		ProfileMode	profileMode = ProfileMode.Normal,
		string		unityVerFrom = null,
		string 		unityVerTo = null,
		JoystickSource	leftStick = null,
		JoystickSource	rightStick = null,
		JoystickSource	dpad = null,
		KeySource		faceBottom = null,
		KeySource		faceRight = null,
		KeySource		faceLeft	= null,
		KeySource		faceTop = null,
		KeySource		select	= null,
		KeySource		start = null,
		KeySource		L1	= null,
		KeySource		R1 = null,
		KeySource		L2 = null,
		KeySource		R2 = null,
		KeySource		L3 = null,
		KeySource		R3 = null)
		{
		this.name 					= name;
		this.joystickIdentifier = deviceIdentifier;
		this.profileMode			= profileMode;
		//this.platformFlags 		= platformFlags;
		this.unityVerFrom			= (string.IsNullOrEmpty(unityVerFrom) 	? "4.3" : unityVerFrom);
		this.unityVerTo			= (string.IsNullOrEmpty(unityVerTo) 	? "9.9" : unityVerTo);
			



		//this.sticks	= new JoystickSource[GamepadManager.GamepadStickCount];
		//this.keys	= new KeySource[GamepadManager.GamepadKeyCount];
			
		this.leftStick	= (leftStick != null)	? leftStick 	: JoystickSource.Empty();
		this.rightStick	= (rightStick != null)	? rightStick 	: JoystickSource.Empty();
		this.dpad		= (dpad != null) 		? dpad 			: JoystickSource.Empty();

		this.keyFaceU	= (faceTop != null)	? faceTop 		: KeySource.Empty();			
		this.keyFaceR	= (faceRight != null)	? faceRight 		: KeySource.Empty();			
		this.keyFaceD	= (faceBottom != null)	? faceBottom 		: KeySource.Empty();			
		this.keyFaceL	= (faceLeft != null)	? faceLeft 		: KeySource.Empty();			

		this.keyStart	= (start != null)	? start		: KeySource.Empty();			
		this.keySelect	= (select != null)	? select		: KeySource.Empty();			
			
		this.keyL1		= (L1 != null)		? L1		: KeySource.Empty();			
		this.keyR1		= (R1 != null)		? R1		: KeySource.Empty();			
		this.keyL2		= (L2 != null)		? L2		: KeySource.Empty();			
		this.keyR2		= (R2 != null)		? R2		: KeySource.Empty();			
		this.keyL3		= (L3 != null)		? L3		: KeySource.Empty();			
		this.keyR3		= (R3 != null)		? R3		: KeySource.Empty();
		}


		
	// ------------------
	// Joystick Source
	// -------------------
	[System.Serializable]
	public class JoystickSource
		{
		public KeySource
			keyU,
			keyD,
			keyR,
			keyL;

		// ----------------------
		public JoystickSource()
			{
			this.keyD = new KeySource();
			this.keyU = new KeySource();
			this.keyL = new KeySource();
			this.keyR = new KeySource();

			}
			
		// --------------------
		public static JoystickSource Dpad(int keyU, int keyR, int keyD, int keyL)	
			{
			JoystickSource s = new JoystickSource();
			s.keyD.SetKey(keyD);
			s.keyU.SetKey(keyU);
			s.keyL.SetKey(keyL);
			s.keyR.SetKey(keyR);


			return s;
			}

		// ----------------------
		public static JoystickSource Axes(int horzAxisId, bool horzPositiveRight, int vertAxisId, bool vertPositiveUp)	
			{	
			JoystickSource s = new JoystickSource();
	
			s.keyR.SetAxis(horzAxisId, 	horzPositiveRight);		
			s.keyL.SetAxis(horzAxisId, 	horzPositiveRight);		

			s.keyU.SetAxis(vertAxisId, 	vertPositiveUp);		
			s.keyD.SetAxis(vertAxisId, 	vertPositiveUp);		

			return s;
			}
			

		// ----------------
		public bool IsDuplicateOf(JoystickSource a)
			{ 
			return (
				this.keyD.IsDuplicateOf(a.keyD) &&
				this.keyU.IsDuplicateOf(a.keyU) &&
				this.keyL.IsDuplicateOf(a.keyL) &&
				this.keyR.IsDuplicateOf(a.keyR) );

			}



		// ------------------
		static public JoystickSource Empty() { return (new JoystickSource()); }
		}
	

	// ----------------------
	// Digital Source
	// ----------------------
	[System.Serializable]
	public class KeySource
		{
		public int	keyId;			// Key Id
		public int	axisId;			// Axis Id
		public bool	axisSign;		// true = positive potion, false = negative portion.
						
	
		// -------------
		public KeySource()
			{
			this.keyId		= -1;
			this.axisId		= -1;
			this.axisSign	= true;
			}
		// ---------------
		public bool IsEmpty()	
			{ return ((this.keyId < 0) && (this.axisId < 0));	}
			
		// ----------------
		public bool IsDuplicateOf(KeySource a)
			{ return ((this.keyId == a.keyId) && (this.axisId == a.axisId) && (this.axisSign == a.axisSign)); }			
			
	
		// ----------------
		public void Clear()
			{
			this.axisId = -1;
			this.keyId = -1;
			this.axisSign = true;
			}

		// --------------
		public void SetKey(int keyId)
			{
			this.keyId = keyId;
			this.axisId = -1;
			this.axisSign = true;
			}

		// ------------------
		public void SetAxis(int axisId, bool axisSign)
			{
			this.keyId		= -1;
			this.axisId		= axisId;
			this.axisSign	= axisSign;
			}


		// Constructors -----

		private KeySource(int keyId, int axisId, bool axisSign)	{ this.axisId = axisId; this.keyId = keyId; this.axisSign = axisSign; }

		static public KeySource Key				(int keyId)				{	return new KeySource(keyId, -1, 	true); }
		static public KeySource PlusAxis		(int axisId)			{	return new KeySource(-1, 	axisId, true); } 
		static public KeySource MinusAxis		(int axisId)			{	return new KeySource(-1,	axisId, false); } 
		static public KeySource KeyAndPlusAxis	(int keyId, int axisId)	{	return new KeySource(keyId, axisId, true); } 
		static public KeySource KeyAndMinusAxis	(int keyId, int axisId)	{	return new KeySource(keyId, axisId, false); } 

		// -----------
		static public KeySource Empty() { return (new KeySource(-1, -1, true)); } 

		
		}
		
	// ---------------------
	// Geteric Profile Class
	// ---------------------
	public class GenericProfile : GamepadProfile
		{
		//// -------------------
		//public GenericProfile() : base(
		//	"Generic Gamepad", 
		//	"",
		//	ProfileMode.Normal,
		//	null,
		//	null,
		//	//GamepadProfile.PlatformFlag.All,

		//	GamepadProfile.JoystickSource.Axes(0, true, 1, false),	// LS
		//	GamepadProfile.JoystickSource.Empty(),
		//	GamepadProfile.JoystickSource.Empty(),
	
		//	GamepadProfile.KeySource.Key(0),		// A
		//	GamepadProfile.KeySource.Key(1),		// B
		//	GamepadProfile.KeySource.Empty(),		// X
		//	GamepadProfile.KeySource.Empty(),		// Y
			
		//	GamepadProfile.KeySource.Empty(),		// Select	// ESCAPE
		//	GamepadProfile.KeySource.Empty(),		// Start

		//	GamepadProfile.KeySource.Empty(),		// L1
		//	GamepadProfile.KeySource.Empty(),		// R1
		//	GamepadProfile.KeySource.Empty(),		// L2
		//	GamepadProfile.KeySource.Empty(),		// R2
		//	GamepadProfile.KeySource.Empty(),		// L3
		//	GamepadProfile.KeySource.Empty()		// R3			
		//	)
		//	{			
		//	}


		// ----------------------
		public GenericProfile(
			JoystickSource	leftStick = null,
			JoystickSource	rightStick = null,
			JoystickSource	dpad = null,
			KeySource		faceBottom = null,
			KeySource		faceRight = null,
			KeySource		faceLeft	= null,
			KeySource		faceTop = null,
			KeySource		select	= null,
			KeySource		start = null,
			KeySource		L1	= null,
			KeySource		R1 = null,
			KeySource		L2 = null,
			KeySource		R2 = null,
			KeySource		L3 = null,
			KeySource		R3 = null)

			//GamepadProfile.JoystickSource	leftStick,
			//GamepadProfile.JoystickSource	rightStick,
			//GamepadProfile.JoystickSource	dpad,
			//GamepadProfile.KeySource		keyFaceD,
			//GamepadProfile.KeySource		keyFaceR,
			//GamepadProfile.KeySource		keyFaceL,
			//GamepadProfile.KeySource		keyFaceU,
			//GamepadProfile.KeySource		keySelect,
			//GamepadProfile.KeySource		keyStart,
			//GamepadProfile.KeySource		keyL1,
			//GamepadProfile.KeySource		keyR1,
			//GamepadProfile.KeySource		keyL2,
			//GamepadProfile.KeySource		keyR2,
			//GamepadProfile.KeySource		keyL3,
			//GamepadProfile.KeySource		keyR3) 
: 
		base (
			"Generic Gamepad", 
			"",
			ProfileMode.Normal,
			null,
			null,
			//GamepadProfile.PlatformFlag.All,	
			leftStick : leftStick,
			rightStick : rightStick,
			dpad : dpad,
			faceBottom : faceBottom,
			faceRight : faceRight,
			faceLeft	: faceLeft,
			faceTop : faceTop,
			select	: select,
			start : start,
			L1	: L1,
			R1 : R1,
			L2 : L2,
			R2 : R2,
			L3 : L3,
			R3 : R3)
			{ 
			}
			





		}
	}

}

//! \endcond
