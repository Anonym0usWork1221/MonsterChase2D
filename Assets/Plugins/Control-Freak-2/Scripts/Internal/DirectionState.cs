// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;

namespace ControlFreak2.Internal
{

// -----------------------
//! Direction State Class.
// -----------------------

public class DirectionState 
	{
	//! When to reset the original direction.
	public enum OriginalDirResetMode
		{		
		OnNeutral,		//!< Reset on neutral.
		On180,			//!< Reset on neutral or when direction changes by 180 degrees.
		On135,			//!< Reset on neutral or when direction changes by 135 degrees or more.
		On90				//!< Reset on neutral or when direction changes by 90 degress or more.
		}

	private Dir
		dirCur,
		dirPrev,
		dirOriginalCur,
		dirOriginalPrev;


	// --------------------
	public DirectionState()
		{
		this.Reset();
		}


	// ------------------
	//! Get current direction.
	// -------------------
	public Dir GetCur()					{ return this.dirCur; }

	// ------------------
	//! Get previous frame's direction.
	// -------------------
	public Dir GetPrev()					{ return this.dirPrev; }

	// ------------------
	//! Get the original (first non-neutral) directon.
	// -------------------
	public Dir GetOriginal()			{ return this.dirOriginalCur; }

	// ------------------
	//! Get previous frame's original direction.
	// -------------------
	public Dir GetPrevOriginal()		{ return this.dirOriginalPrev; }


	// ------------------
	//! Returns true if direction just changed to given one.
	// -------------------
	public bool JustPressed(Dir dir)	{ return ((this.dirPrev != dir) && (this.dirCur == dir)); } 

	// ------------------
	//! Returns true if direction just changed to something than given one.
	// -------------------
	public bool JustReleased(Dir dir)	{ return ((this.dirPrev == dir) && (this.dirCur != dir)); } 



//! \cond

	// -------------------
	public void Reset()
		{
		this.dirCur				= Dir.N;
		this.dirPrev			= Dir.N;
		this.dirOriginalCur		= Dir.N;
		this.dirOriginalPrev	= Dir.N;
		}


	// ------------------
	public void BeginFrame()
		{
		this.dirPrev			= this.dirCur;
		this.dirOriginalPrev	= this.dirOriginalCur;
		}


	// ------------------
	public void SetDir(Dir dir, OriginalDirResetMode resetMode)
		{

		this.dirCur				= dir;
	
		// Detect original dir change...

		if (this.dirCur != this.dirPrev)
			{	
			if (this.dirCur == Dir.N)	
				this.dirOriginalCur = Dir.N;

			else if (this.dirPrev == Dir.N)
				this.dirOriginalCur = this.dirCur;

			else if ((resetMode != OriginalDirResetMode.OnNeutral) &&
					(Mathf.Abs(CFUtils.DirDeltaAngle(this.dirOriginalPrev, this.dirCur)) >= (
						(resetMode == OriginalDirResetMode.On90) ? 90 : 
						(resetMode == OriginalDirResetMode.On135) ? 135 : 180)))
				this.dirOriginalCur = this.dirCur;
			}
	
		}


//! \endcond

	}
}
