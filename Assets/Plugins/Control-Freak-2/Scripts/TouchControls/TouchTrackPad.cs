// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------


using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

using ControlFreak2.Internal;

namespace ControlFreak2
{
// -------------------
//! Touch Track Pad Class.
// -------------------
public class TouchTrackPad : TouchControl
	{
//! \cond
	public float 
		touchSmoothing;
		
	public DigitalBinding
		pressBinding;

	public AxisBinding	
		horzSwipeBinding,
		vertSwipeBinding;

	public bool
		emulateTouchPressure;
	public AxisBinding
		touchPressureBinding;


	
	private TouchObject	
		touchObj;
	private TouchControl.TouchStartType
		touchStartType;

	private TouchGestureBasicState
		touchState;
	

	// ---------------------
	public TouchTrackPad() : base()
		{
		this.touchSmoothing		= 0.5f;
		this.touchState			= new TouchGestureBasicState();

		this.pressBinding		= new DigitalBinding();
		this.horzSwipeBinding	= new AxisBinding("Mouse X", false);
		this.vertSwipeBinding	= new AxisBinding("Mouse Y", false);
		this.emulateTouchPressure = true;
		this.touchPressureBinding = new AxisBinding();
		}

//! \endcond

	
	// ----------------
	public bool Pressed()						{ return this.touchState.PressedRaw(); }
	public bool JustPressed()					{ return this.touchState.JustPressedRaw(); }
	public bool JustReleased()					{ return this.touchState.JustReleasedRaw(); }

	// -----------------
	public bool		IsTouchPressureSensitive() { return (this.touchState.PressedRaw() && this.touchState.IsPressureSensitive()); } 
	public float	GetTouchPressure()			{ return (this.touchState.PressedRaw() ? this.touchState.GetPressure() : 0); }


	// -----------------
	//! Get oriented swipe delta vector in pixels.
	// ------------------
	public Vector2 GetSwipeDelta()
		{
		return this.touchState.GetDeltaVecSmooth();
		}



	// ------------------
	//! Set touch smoothing time in seconds.
	// ------------------
	public void SetTouchSmoothing(float smTime)
		{	
		this.touchSmoothing = Mathf.Clamp01(smTime);
			
		this.touchState	.SetSmoothingTime(this.touchSmoothing * InputRig.TOUCH_SMOOTHING_MAX_TIME);
		}


//! \cond

	// -----------------
	override protected void OnInitControl()
		{
		this.ResetControl();
			
		this.SetTouchSmoothing(this.touchSmoothing);
		}
		
		
	// ------------------
	override protected void OnDestroyControl()
		{	
		this.ResetControl(); //.ReleaseAllTouches(true);
		}
		



	// ------------------
	override public void ResetControl()
		{
		
		this.ReleaseAllTouches(); //true);
			
		this.touchState.Reset();
		}




	// ----------------
	override protected void OnUpdateControl()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return;
#endif

		if ((this.touchObj != null) && (this.rig != null))
			this.rig.WakeTouchControlsUp();


		this.touchState.Update();
				
			
		if (this.IsActive())
			this.SyncRigState();		
		}
		
		


	// ---------------------
	private void SyncRigState()
		{
		if (this.Pressed())
			{
			this.pressBinding.Sync(true, this.rig);
			
			if (this.IsTouchPressureSensitive())
				this.touchPressureBinding.SyncFloat(this.GetTouchPressure(), InputRig.InputSource.Analog, this.rig);

			else if (this.emulateTouchPressure)	
				this.touchPressureBinding.SyncFloat(1, InputRig.InputSource.Digital, this.rig);			
			}

		Vector2 swipeDelta = this.GetSwipeDelta();

		this.horzSwipeBinding.SyncFloat(swipeDelta.x, InputRig.InputSource.TouchDelta, this.rig);
		this.vertSwipeBinding.SyncFloat(swipeDelta.y, InputRig.InputSource.TouchDelta, this.rig);
		}


	
	// ---------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{ 
		return (
			this.pressBinding.IsBoundToAxis(axisName, rig) ||
			this.touchPressureBinding.IsBoundToAxis(axisName, rig) ||
			this.horzSwipeBinding.IsBoundToAxis(axisName, rig) ||
			this.vertSwipeBinding.IsBoundToAxis(axisName, rig) );
		}


	// ----------------------
	override protected bool OnIsBoundToKey(KeyCode key, InputRig rig)
		{ 
		return (
			this.pressBinding.IsBoundToKey(key, rig) ||
			this.touchPressureBinding.IsBoundToKey(key, rig) ||
			this.horzSwipeBinding.IsBoundToKey(key, rig) ||
			this.vertSwipeBinding.IsBoundToKey(key, rig) );
		}


	// ----------------------
	override protected void OnGetSubBindingDescriptions(BindingDescriptionList descList, 
		Object undoObject, string parentMenuPath) 
		{
		descList.Add(this.pressBinding, "Press", parentMenuPath, this);
		descList.Add(this.touchPressureBinding, InputRig.InputSource.Analog, "Touch Pressure", parentMenuPath, this);
		descList.Add(this.horzSwipeBinding, InputRig.InputSource.TouchDelta, "Horz. Swipe Delta", parentMenuPath, this);
		descList.Add(this.vertSwipeBinding, InputRig.InputSource.TouchDelta, "Vert. Swipe Delta", parentMenuPath, this);

		}


	// ----------------
	override public bool CanBeTouchedDirectly(TouchObject touchObj)
		{
		return (base.CanBeTouchedDirectly(touchObj) && (this.touchObj == null));
		}
	
	// -------------------
	override public bool CanBeActivatedByOtherControl(TouchControl c, TouchObject touchObj)
		{
		return (base.CanBeActivatedByOtherControl(c, touchObj) && (this.touchObj == null));
		}

	

	// ---------------------
	public override bool CanBeSwipedOverFromNothing(TouchObject touchObj)
		{
		return (base.CanBeSwipedOverFromNothingDefault(touchObj) && (this.touchObj == null));
		}

	// ---------------------
	public override bool CanBeSwipedOverFromRestrictedList(TouchObject touchObj)
		{
		return (base.CanBeSwipedOverFromRestrictedListDefault(touchObj) && (this.touchObj == null));
		}

	// -----------------------
	public override bool CanSwipeOverOthers(TouchObject touchObj)
		{
		return this.CanSwipeOverOthersDefault(touchObj, this.touchObj, this.touchStartType);
		}


	// ------------------
	override public void ReleaseAllTouches() 
		{
		if (this.touchObj != null)
			{
			this.touchObj.ReleaseControl(this, TouchEndType.Cancel); 
			this.touchObj = null;
			}	

		}
		

	// ------------------
	override public bool OnTouchStart(TouchObject touchObj, TouchControl sender, TouchStartType touchStartType)
		{ 
		if (this.touchObj != null)
			return false;
			
//Debug.LogFormat("----------------Track pad start : {0} : active:{1}", Time.frameCount, this.IsActive());

		this.touchObj		= touchObj;
		this.touchStartType = touchStartType;
		this.touchObj.AddControl(this);

		Vector2 localPos = this.ScreenToOrientedPos(touchObj.screenPosStart, touchObj.cam); 

		this.touchState.OnTouchStart(localPos, localPos, 0, touchObj);

		return true;
		}

	// ------------------
	override public bool OnTouchEnd(TouchObject touchObj, TouchEndType touchEndType) 
		{
		if ((this.touchObj == null) || (this.touchObj != touchObj))
			return false;
			
//Debug.LogFormat("----------------Track pad END!!!! : {0} : active:{1}", Time.frameCount, this.IsActive());

		this.touchObj = null;
			
		this.touchState.OnTouchEnd(touchEndType != TouchEndType.Release);

		return true;
		}


	// -------------------
	override public bool OnTouchMove(TouchObject touchObj) 
		{
		if ((this.touchObj == null) || (this.touchObj != touchObj))
			return false;
			
		Vector2 localPos = this.ScreenToOrientedPos(touchObj.screenPosCur, touchObj.cam); //data.pos, data.cam);

		this.touchState.OnTouchMove(localPos);

		this.CheckSwipeOff(touchObj, this.touchStartType);
	
		return true;
		}


	// -------------------
	override public bool OnTouchPressureChange(TouchObject touchObj) 
		{
		if ((this.touchObj == null) || (this.touchObj != touchObj))
			return false;
			
		this.touchState.OnTouchPressureChange(touchObj.GetPressure());
	
		return true;
		}



#if UNITY_EDITOR		
	[ContextMenu("Add Default Animator")]
	private void ContextMenuCreateAnimator()
		{
		ControlFreak2Editor.TouchControlWizardUtils.CreateTouchTrackPadAnimator(this, "-Animator", 
				ControlFreak2Editor.TouchControlWizardUtils.GetDefaultTrackPadSprite(this.name), 1, "Create Touch TrackPad Animator");
		}
#endif


//! \endcond
	}
}
