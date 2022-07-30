// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond


using UnityEngine;

namespace ControlFreak2
{
public class GamepadProfileBank : ScriptableObject
	{
#if UNITY_EDITOR
	private const string 
		DEFAULT_PROFILE_BANK_ASSET_PATH = "Assets/Plugins/Control-Freak-2/Data/CF2-Gamepad-Profiles.asset";
#endif
	
	public GamepadProfile[] profiles;

	

	// ----------------------
	public GamepadProfile GetProfile(string deviceIdentifier)
		{
		return null;
		}



#if UNITY_EDITOR	

	// ----------------
	static public GamepadProfileBank LoadDefaultBank()
		{
		GamepadProfileBank defaultBank = 
			(GamepadProfileBank)UnityEditor.AssetDatabase.LoadAssetAtPath(DEFAULT_PROFILE_BANK_ASSET_PATH, typeof(GamepadProfileBank));

		return defaultBank;
		}


	// --------------------
	static public void CreateDefaultProfileBank()
		{
		
		}

#endif
		

	}
}

//! \endcond

