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


//! \cond

using UnityEngine;

namespace ControlFreak2
{

abstract public class BuiltInGamepadProfileBank
	{
	static private BuiltInGamepadProfileBank 
		mInst;


	protected GamepadProfile[]
		profiles;

	protected GamepadProfile
		genericProfile;


	// ----------------------
	protected BuiltInGamepadProfileBank()
		{
		}


	// ---------------------
	static public GamepadProfile GetProfile(string deviceName)
		{
		BuiltInGamepadProfileBank bank = Inst();
		return ((bank == null) ? null : bank.FindProfile(deviceName));
		}


	// -------------------------
	static public GamepadProfile GetGenericProfile()
		{
		BuiltInGamepadProfileBank bank = Inst();
		return ((bank == null) ? null : bank.GetInternalGenericProfile());
		}


	// ---------------------
	virtual protected GamepadProfile GetInternalGenericProfile()
		{
		if (this.genericProfile == null)
			this.genericProfile = new GamepadProfile.GenericProfile();

		return this.genericProfile;
		}


	// ---------------------
	virtual protected GamepadProfile FindProfile(string deviceName)
		{
		return this.FindInternalProfile(deviceName);
		}


	// ---------------------
	protected GamepadProfile FindInternalProfile(string deviceName)
		{
		if ((this.profiles == null) || (this.profiles.Length == 0))	
			return null;
	

		for (int i = 0; i < this.profiles.Length; ++i)
			{
			if (this.profiles[i] == null)
				continue;

			if (this.profiles[i].IsCompatible(deviceName))
				return this.profiles[i];
			}
		
		return null;
		}




	// -----------------------
	static private BuiltInGamepadProfileBank Inst()
		{ 
		if (mInst != null)
			return mInst;
			
		string platformName = Application.platform.ToString().ToLower();

		switch (platformName)
			{
			case "android" :
				mInst = new BuiltInGamepadProfileBankAndroid();
				break;
	
			case "osxeditor" :
			case "osxplayer" :
			case "osxdashboardplayer" :
			case "osxwebplayer" :
				mInst = new BuiltInGamepadProfileBankOSX();
				break;

			case "windowseditor" :
			case "windowsplayer" :
			case "metroplayerarm" :
			case "metroplayerx64" :
			case "metroplayerx86" :
			case "wsaplayerarm" :
			case "wsaplayerx64" :
			case "wsaplayerx86" :
			case "windowswebplayer" :
				mInst = new BuiltInGamepadProfileBankWin();
				break;

			case "tvos" :
			case "iphoneplayer" :
				mInst = new BuiltInGamepadProfileBankIOS();
				break;

			case "wp8player" :
				mInst = new BuiltInGamepadProfileBankWP8();
				break;

			case "linuxeditor" :
			case "linuxplayer" :	
				mInst = new BuiltInGamepadProfileBankLinux();
				break;

			case "webglplayer" :
				mInst = new BuiltInGamepadProfileBankWebGL();
				break;		

			case "stadia" :
				mInst = new BuiltInGamepadProfileBankStadia();
				break;		

			case "xboxone" :
				mInst = new BuiltInGamepadProfileBankXboxOne();
				break;		

			case "ps4" :
				mInst = new BuiltInGamepadProfileBankPS4();
				break;		

			}




/*
		switch (Application.platform)
			{
			case RuntimePlatform.Android :
				mInst = new BuiltInGamepadProfileBankAndroid();
				break;
	
			case RuntimePlatform.OSXEditor :
			case RuntimePlatform.OSXPlayer :
#if UNITY_PRE_5_4
			case RuntimePlatform.OSXDashboardPlayer :
			case RuntimePlatform.OSXWebPlayer :
#endif
				mInst = new BuiltInGamepadProfileBankOSX();
				break;

			case RuntimePlatform.WindowsEditor :
			case RuntimePlatform.WindowsPlayer :
#if UNITY_PRE_5_0 
			case RuntimePlatform.MetroPlayerARM :
			case RuntimePlatform.MetroPlayerX64 :
			case RuntimePlatform.MetroPlayerX86 :
#else
			case RuntimePlatform.WSAPlayerARM :
			case RuntimePlatform.WSAPlayerX64 :
			case RuntimePlatform.WSAPlayerX86 :
#endif

#if UNITY_PRE_5_4
			case RuntimePlatform.WindowsWebPlayer :
#endif

				mInst = new BuiltInGamepadProfileBankWin();
				break;

#if !UNITY_PRE_5_3
			case RuntimePlatform.tvOS :
#endif	
			case RuntimePlatform.IPhonePlayer :
				mInst = new BuiltInGamepadProfileBankIOS();
				break;

#if UNITY_PRE_5_4
			case RuntimePlatform.WP8Player :
				mInst = new BuiltInGamepadProfileBankWP8();
				break;
#endif

			case RuntimePlatform.LinuxPlayer :	
				mInst = new BuiltInGamepadProfileBankLinux();
				break;
#if !UNITY_PRE_5_0

				case RuntimePlatform.WebGLPlayer :
				mInst = new BuiltInGamepadProfileBankWebGL();
				break;		
#endif
			}	
*/
		return mInst;
		}

	
	}
}

//! \endcond
