// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------


using UnityEngine;



namespace ControlFreak2
{

// -----------------
//! Unity's Screen class replacement.
// -----------------

public static class CFScreen
	{
		
	const float DEFAULT_FALLBACK_SCREEN_DIAMETER = 7.0f; // 7 inches	

	// --------------------

	static private bool
		mForceFallbackDpi	= false;

	static private Resolution
		mLastScreenResolution,
		mNativeScreenResolution;

	static private float 
		mFallbackDpi		= 96,
		mFallbackDiameter	= DEFAULT_FALLBACK_SCREEN_DIAMETER,

		mLastScreenWidth	= -1,
		mLastScreenHeight = -1,
		mLastScreenDpi		= -1,
		mNativeScreenDpi	= -1,

		mDpi		= 100,
		mDpcm		= 100,
		mInvDpi		= 1,
		mInvDpcm	= 1;
		
		
// ----------------

// ----------------
		

	// ----------------
	static public float dpi		{ get { UpdateDpiIfNeeded(); return mDpi; } }
	static public float dpcm	{ get { UpdateDpiIfNeeded(); return mDpcm; } }
	static public float invDpi	{ get { UpdateDpiIfNeeded(); return mInvDpi; } }
	static public float invDpcm{ get { UpdateDpiIfNeeded(); return mInvDpcm; } }
	static public float width	{ get { return Screen.width; } }
	static public float height	{ get { return Screen.height; } }
	
	// ---------------------
	//! Lock cursor. Use \ref CFCursor.lockState instead. 
	// ---------------------
	static public bool	lockCursor	
		{
		get { return (CFCursor.lockState == CursorLockMode.Locked); } 
		set { CFCursor.lockState = (value ? CursorLockMode.Locked : CursorLockMode.None); } 
		}

	// ---------------------
	//! Show cursor. Use \ref CFCursor.visible instead. 
	// ---------------------
	static public bool	showCursor	
		{
		get { return CFCursor.visible; } 
		set { CFCursor.visible = value; }
		}
		


	// ------------------------
	//! Get/Set full screen state.
	// ------------------------
	static public bool	fullScreen
		{
		get { return Screen.fullScreen; }
		set { UpdateDpi(); Screen.fullScreen = value; }
		}


	// ----------------
	//! Attempt to change screen resolution.	
	// ----------------
	static public void SetResolution(int width, int height, bool fullScreen, int refreshRate = 0)
		{
		UpdateDpi();

		Screen.SetResolution(width, height, fullScreen, refreshRate);
		}


	// ------------------------
	static public void ForceFallbackDpi(bool enableFallbackDpi)
		{
		mForceFallbackDpi = enableFallbackDpi;
		UpdateDpi();
		}

	// -----------------------
	static public void SetFallbackScreenDiameter(float diameterInInches)
		{
		mLastScreenWidth	= -1;
		mFallbackDiameter	= Mathf.Max(1.0f, diameterInInches);

		UpdateDpi();
		}


	// ------------------------
	static private void UpdateDpiIfNeeded()
		{
		Resolution curResolution = Screen.currentResolution;

		if ((mLastScreenWidth != Screen.width) || (mLastScreenHeight != Screen.height) || (mLastScreenDpi != Screen.dpi) ||
			(curResolution.width != mLastScreenResolution.width) || (curResolution.height != mLastScreenResolution.height))
			UpdateDpi();
		}


	// ----------------------
	//! Update internal DPI state. Call this before changing screen resolution via normal Screen.SetResolution(). 
	// ----------------------
	static public void UpdateDpi()
		{
		Resolution curResolution = Screen.currentResolution;

		if ((mLastScreenDpi != Screen.dpi) || (mNativeScreenResolution.width <= 0) || (mNativeScreenResolution.height <= 0))
			{
			mNativeScreenResolution = curResolution;
			mNativeScreenDpi = Screen.dpi;
			}

		mLastScreenWidth	= Screen.width;
		mLastScreenHeight	= Screen.height;
		mLastScreenDpi		= Screen.dpi;
	
		mFallbackDpi		= Mathf.Sqrt((mLastScreenWidth * mLastScreenWidth) + (mLastScreenHeight * mLastScreenHeight)) / mFallbackDiameter;
			
		mLastScreenResolution = curResolution;




#if UNITY_EDITOR || UNITY_WEBPLAYER	 

		mDpi = mNativeScreenDpi;  // to remove compiled warning...

		if (mForceFallbackDpi)
			mDpi = mFallbackDpi;
		else
			mDpi = mFallbackDpi;
#else

		// Fix DPI if needed...

		float fixedDpi = mLastScreenDpi;

		if ((mNativeScreenDpi > 0) && (mLastScreenDpi > 0) && (mNativeScreenResolution.width > 0) && (mNativeScreenResolution.height > 0) && 
			(curResolution.width > 0) && (curResolution.height > 0))
			{
			fixedDpi = mNativeScreenDpi * (
				Mathf.Sqrt((curResolution.width * curResolution.width) + (curResolution.height * curResolution.height)) /
				Mathf.Sqrt((mNativeScreenResolution.width * mNativeScreenResolution.width) + (mNativeScreenResolution.height * mNativeScreenResolution.height)) );
			}

		if ((fixedDpi < 1.0f) || mForceFallbackDpi)
			{
			mDpi = mFallbackDpi;
			}
		else
			{

			mDpi = fixedDpi;
			}
#endif

		mDpcm		= (mDpi / 2.54f);
		mInvDpi		= (1.0f / mDpi);
		mInvDpcm	= (1.0f / mDpcm);
		}



	}
}
