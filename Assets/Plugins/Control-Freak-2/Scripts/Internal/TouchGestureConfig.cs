// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2.Internal
{
	
// ----------------------
[System.Serializable]
public class TouchGestureConfig
	{
	public int 
		maxTapCount;
	public bool		
		cleanTapsOnly,	
		detectLongPress,
		detectLongTap,
		//reportAllTaps,
		//dontReportPressDuringTap,
		//dontReportPressDuringLongTap,
		endLongPressWhenMoved,
		endLongPressWhenSwiped;
		
	public DirMode	
		dirMode;
		
	public DirectionState.OriginalDirResetMode
		swipeOriginalDirResetMode;

	public DirConstraint
		swipeConstraint,
		swipeDirConstraint,
		scrollConstraint;



	public enum DirMode
		{	
		FourWay,
		EightWay
		}

	public enum DirConstraint
		{
		None,
		Horizontal,
		Vertical,
		Auto
		}

	// ------------------
	public TouchGestureConfig()
		{
		this.maxTapCount = 1;
		this.cleanTapsOnly = true;

		this.swipeOriginalDirResetMode = DirectionState.OriginalDirResetMode.On180;
		}
	}

}

//! \endcond
