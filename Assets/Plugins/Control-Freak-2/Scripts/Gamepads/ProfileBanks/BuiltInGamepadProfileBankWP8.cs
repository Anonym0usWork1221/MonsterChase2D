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


//#if UNITY_PRE_5_4

using UnityEngine;

namespace ControlFreak2
{

public class BuiltInGamepadProfileBankWP8 : BuiltInGamepadProfileBank
	{
	// ------------------
	public BuiltInGamepadProfileBankWP8() : base()
		{
		this.profiles = new GamepadProfile[]
			{	
			};
		}
	}
}

//#endif

//! \endcond
