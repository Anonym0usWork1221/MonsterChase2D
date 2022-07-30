// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9  
#define UNITY_PRE_5
#endif



using UnityEngine;

namespace ControlFreak2
{

#if UNITY_PRE_5
	public enum CursorLockMode
		{
		None,
		Locked,
		Confined
		}
#endif

// -----------------
//! Unity's Cursor class replacement.
// -----------------

public static class CFCursor
	{
	static public event System.Action
		onLockStateChange;

	// --------------
	static CFCursor()
		{
		mLockState	= CursorLockMode.None;	
		mVisible	= true;
		}






	// ------------------
	//! Get/set cursor lock state. This will not lock the cursor when running in the editor and the Mobile Mode is on to allow touch emulation with mouse. 
	// -----------------
	static public CursorLockMode	lockState
		{
		// --------------
		set
			{
#if UNITY_EDITOR
		ControlFreak2Editor.Assistant.CaptureCursorLock();
#endif 
			CursorLockMode initialLockState = mLockState;

			mLockState = value;
			
			if (IsCursorLockingAllowed())
				{
				InternalSetLockMode(value);
				mLockState = InternalGetLockMode();
				}
			else
				{
				InternalSetLockMode(CursorLockMode.None);				
				}

			if ((initialLockState != mLockState) && (onLockStateChange != null))
				onLockStateChange();
			}
	
		// --------------------
		get 
			{	
			if (IsCursorLockingAllowed())
				{
				if (mLockState != InternalGetLockMode())
					{
					mLockState = InternalGetLockMode(); //UnityEngine.Cursor.lockState;
		
					if (onLockStateChange != null)
						onLockStateChange();
					}		
				}		

			return mLockState;
			}
		}
		static private CursorLockMode mLockState;
		

	// ------------------
	//! Show/hide the cursor. This will not actually hide the cursor when running in the editor and the Mobile Mode is on to allow touch emulation with mouse. 
	// -----------------
	static public bool visible 
		{
		set
			{
			mVisible = value;
			
			if (IsCursorLockingAllowed())
				InternalSetVisible(value); 
			else
				InternalSetVisible(true);
			}

		get 
			{
			return (!IsCursorLockingAllowed() ? mVisible : (mVisible = InternalIsVisible())); //UnityEngine.Cursor.visible));
			}
		}
		static private bool mVisible;

	
	
		
	// ----------------
	static private bool IsCursorLockingAllowed()
		{
		return !((CF2Input.activeRig != null) && CF2Input.IsInMobileMode()); 
		}
		
//! \cond

	// ------------------
	static public void InternalRefresh()
		{
		if (!IsCursorLockingAllowed())
			{
			InternalSetLockMode(CursorLockMode.None); 
			InternalSetVisible(true); 
			}
		else
			{	
			CursorLockMode 
				initialLockState = InternalGetLockMode();
			bool
				initialVisibility	= InternalIsVisible();

			InternalSetLockMode(mLockState);
			InternalSetVisible(mVisible); 

			mLockState	= InternalGetLockMode();
			mVisible		= InternalIsVisible();

			if (((mLockState != initialLockState) || (mVisible != initialVisibility)) && (CFCursor.onLockStateChange != null))
				CFCursor.onLockStateChange();
			}
		}

//! \endcond

	

#if !UNITY_PRE_5
	// ------------------
	static public void SetCursor(Texture2D tex, Vector2 hotSpot, CursorMode mode)	{ UnityEngine.Cursor.SetCursor(tex, hotSpot, mode); }
#endif



	// --------------
	static private CursorLockMode InternalGetLockMode()
		{
#if UNITY_PRE_5
		return (UnityEngine.Screen.lockCursor ? CursorLockMode.Locked : CursorLockMode.None);
#else
		return UnityEngine.Cursor.lockState;
#endif
		}

	// -----------------
	static private void InternalSetLockMode(CursorLockMode mode)
		{
#if UNITY_PRE_5
		UnityEngine.Screen.lockCursor = (mode == CursorLockMode.Locked);
#else
		UnityEngine.Cursor.lockState = mode;
#endif
		}



	// --------------
	static private bool InternalIsVisible()
		{
#if UNITY_PRE_5
		return (UnityEngine.Screen.showCursor);
#else
		return UnityEngine.Cursor.visible;
#endif
		}

	// -----------------
	static private void InternalSetVisible(bool visible)
		{
#if UNITY_PRE_5
		UnityEngine.Screen.showCursor = visible;
#else
		UnityEngine.Cursor.visible = visible;
#endif
		}


	}
}
