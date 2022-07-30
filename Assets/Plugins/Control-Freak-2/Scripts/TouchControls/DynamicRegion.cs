// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using ControlFreak2.Internal;

namespace ControlFreak2
{
// -----------------
//! Dynamic Region Class.
// -------------------
public class DynamicRegion : TouchControl 
	{

//! \cond

	[System.NonSerialized]
	private DynamicTouchControl targetControl;
		 

	// -----------------
	public DynamicRegion() : base()
		{
		this.ignoreFingerRadius = true;
		}

//! \endcond


	// -------------------
	//! Get target dynamic touch control.
	// ------------------	
	public DynamicTouchControl GetTargetControl()
		{
		return this.targetControl;
		}

		
	// --------------------
	//! Set target control.
	// --------------------
	public void SetTargetControl(DynamicTouchControl targetControl)
		{
		if (this.targetControl == targetControl)
			return;			
			
	

		if (this.targetControl != null)
			{
			if (this.targetControl.GetDynamicRegion() == this)
				{
#if UNITY_EDITOR
				if ((targetControl != null) && (targetControl != this.targetControl))
					Debug.LogError("Trying to link [" + targetControl.name + "] to Dynamic Region [" + this.name + "], but it's already linked to [" + this.targetControl.name + "].");
#endif
				return;
				}

			this.targetControl.OnLinkToDynamicRegion(null);
			}		

		this.targetControl = targetControl;
		this.targetControl.OnLinkToDynamicRegion(this);
		}


//! \cond
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
		this.ReleaseAllTouches(); 
	
		}

	// ------------------
	override public void ReleaseAllTouches() 
		{
		
		}




	// --------------
	override public bool OnTouchStart(TouchObject touch, TouchControl sender, TouchStartType touchStartType)
		{
		if (this.targetControl != null)
			{
			return this.targetControl.OnTouchStart(touch, this, TouchStartType.ProxyPress);
			}

		return false;
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
		return (base.CanBeTouchedDirectly(touchObj) && (this.targetControl != null) && this.targetControl.CanBeActivatedByDynamicRegion());
		}

	// ----------------
	public override bool CanBeSwipedOverFromNothing(TouchObject touchObj)
		{
		return false;
		}

	public override bool CanBeSwipedOverFromRestrictedList(TouchObject touchObj)
		{
		return (this.CanBeSwipedOverFromRestrictedListDefault(touchObj) && 
			(this.targetControl != null) && this.targetControl.CanBeActivatedByDynamicRegion());
		}


	// -----------------------
	public override bool CanSwipeOverOthers(TouchObject touchObj)
		{
		return false;
		}


	// -------------------
	override public bool CanBeActivatedByOtherControl(TouchControl c, TouchObject touchObj)
		{
		return (base.CanBeActivatedByOtherControl(c, touchObj) && (this.targetControl != null) && this.targetControl.CanBeActivatedByDynamicRegion());
		}
	
//! \endcond

	}

}
