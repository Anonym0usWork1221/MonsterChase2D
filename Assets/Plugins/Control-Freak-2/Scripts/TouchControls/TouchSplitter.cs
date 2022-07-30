// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

using ControlFreak2.Internal;

namespace ControlFreak2
{
// ---------------------
//! Touch Splitter Class.
// --------------------
public class TouchSplitter : TouchControl 
	{
//! \cond
	public List<TouchControl>
		targetControlList;	

	// -----------------
	public TouchSplitter() : base()
		{
		this.ignoreFingerRadius = true;	
		this.targetControlList = new List<TouchControl>(4);
		}


		

	// ---------------------
	override protected void OnInitControl()
		{
		this.ResetControl();
		}
		
	// ---------------------
	override protected void OnUpdateControl() {} 
	override protected void OnDestroyControl() {} 
	

	// ------------------
	override public void ResetControl()
		{
	
		this.ReleaseAllTouches(); //true);
	
		}

	// ------------------
	override public void ReleaseAllTouches() 
		{
		for (int i = 0; i < this.targetControlList.Count; ++i)
			{
			TouchControl c = this.targetControlList[i];
			if (c != null)
				c.ReleaseAllTouches(); 
			}

		}





	// --------------
	override public bool OnTouchStart(TouchObject touch, TouchControl sender, TouchStartType touchStartType)
		{	
		bool someActivated = false;

		for (int i = 0; i < this.targetControlList.Count; ++i)
			{
			TouchControl c = this.targetControlList[i];
			if (c != null)
				{
				if (c.OnTouchStart(touch, this, TouchStartType.ProxyPress))
					someActivated = true;
				}
			}

		return someActivated;
		}

	// --------------
	override public bool OnTouchEnd(TouchObject touch, TouchEndType touchEndType)
		{
		return false;
		}

	// --------------
	override public bool OnTouchMove(TouchObject touch) 
		{
		return false;
		}
		
	// --------------
	override public bool OnTouchPressureChange(TouchObject touch) 
		{
		return false;
		}


	// -------------------	
	override public bool CanBeTouchedDirectly(TouchObject touchObj)
		{
		if (!base.CanBeTouchedDirectly(touchObj))
			return false;

		for (int i = 0; i < this.targetControlList.Count; ++i)
			{
			TouchControl c = this.targetControlList[i];
			if (c == null)	
				continue;

			if (c.CanBeActivatedByOtherControl(this, touchObj))
				return true;
			}

		return false;		
		}


	// --------------------
	public override bool CanSwipeOverOthers(TouchObject touchObj)
		{ return false; }

	public override bool CanBeSwipedOverFromNothing (TouchObject touchObj)
		{ return this.CanBeSwipedOverFromNothingDefault(touchObj); }

	public override bool CanBeSwipedOverFromRestrictedList(TouchObject touchObj)
		{ return this.CanBeSwipedOverFromRestrictedListDefault(touchObj); }



//! \endcond

	}

}
