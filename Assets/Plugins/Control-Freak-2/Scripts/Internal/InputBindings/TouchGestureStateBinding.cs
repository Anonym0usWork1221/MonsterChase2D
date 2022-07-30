// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond


using UnityEngine;
using System.Collections.Generic;

using ControlFreak2;

namespace ControlFreak2.Internal
{
// ----------------------
/// Touch state binding.
// ----------------------

[System.Serializable]
public class TouchGestureStateBinding : InputBindingBase
	{	
	public DigitalBinding
		rawPressBinding,
		normalPressBinding,
		longPressBinding,
		rawTapBinding,
		tapBinding,
		doubleTapBinding,
		longTapBinding;
	
	public DirectionBinding
		normalPressSwipeDirBinding,	
		longPressSwipeDirBinding;		
		
	public AxisBinding 
		normalPressSwipeHorzAxisBinding,
		normalPressSwipeVertAxisBinding,
		longPressSwipeHorzAxisBinding,
		longPressSwipeVertAxisBinding;

	public ScrollDeltaBinding
		normalPressScrollHorzBinding,
		normalPressScrollVertBinding,
		longPressScrollHorzBinding,
		longPressScrollVertBinding;
		
	public MousePositionBinding
		rawPressMousePosBinding,
		normalPressMousePosBinding,
		longPressMousePosBinding,
		tapMousePosBinding,
		doubleTapMousePosBinding,
		longTapMousePosBinding,
		normalPressSwipeMousePosBinding, 
		longPressSwipeMousePosBinding; 
		
	public EmuTouchBinding
		rawPressEmuTouchBinding,
		normalPressEmuTouchBinding,
		longPressEmuTouchBinding;

	public JoystickStateBinding
		normalPressSwipeJoyBinding,
		longPressSwipeJoyBinding;




	// ------------------
	public TouchGestureStateBinding(InputBindingBase parent = null) : base(parent)
		{
		this.enabled = false;	

		this.rawPressBinding	= new DigitalBinding(this);
		this.longPressBinding	= new DigitalBinding(this);
		this.normalPressBinding	= new DigitalBinding(this);
		//this.releasedBinding	= new DigitalBinding(this);
		this.rawTapBinding		= new DigitalBinding(this);
		this.tapBinding			= new DigitalBinding(this);
		this.doubleTapBinding	= new DigitalBinding(this);
		this.longTapBinding		= new DigitalBinding(this);
	

			
		this.normalPressSwipeHorzAxisBinding 	= new AxisBinding(this);
		this.normalPressSwipeVertAxisBinding 	= new AxisBinding(this);
		this.longPressSwipeHorzAxisBinding 		= new AxisBinding(this);
		this.longPressSwipeVertAxisBinding 		= new AxisBinding(this);

		this.normalPressScrollHorzBinding		= new ScrollDeltaBinding(this);
		this.normalPressScrollVertBinding		= new ScrollDeltaBinding(this);
		this.longPressScrollHorzBinding			= new ScrollDeltaBinding(this);
		this.longPressScrollVertBinding			= new ScrollDeltaBinding(this);

					
		this.rawPressEmuTouchBinding	= new EmuTouchBinding(this);
		this.normalPressEmuTouchBinding = new EmuTouchBinding(this);
		this.longPressEmuTouchBinding	= new EmuTouchBinding(this);
			
		this.rawPressMousePosBinding		= new MousePositionBinding(10, false, this);
		this.normalPressMousePosBinding		= new MousePositionBinding(20, false, this);
		this.longPressMousePosBinding		= new MousePositionBinding(20, false, this);
		this.tapMousePosBinding				= new MousePositionBinding(30, false, this);
		this.doubleTapMousePosBinding		= new MousePositionBinding(30, false, this);
		this.longTapMousePosBinding			= new MousePositionBinding(30, false, this);
		this.normalPressSwipeMousePosBinding= new MousePositionBinding(20, false, this);
		this.longPressSwipeMousePosBinding	= new MousePositionBinding(20, false, this);
		 

		this.normalPressSwipeDirBinding = new DirectionBinding(this);
		this.longPressSwipeDirBinding	= new DirectionBinding(this);

		this.normalPressSwipeJoyBinding	= new JoystickStateBinding(this);
		this.longPressSwipeJoyBinding	= new JoystickStateBinding(this);
		
		}

		

	// -------------------
	public void CopyFrom(TouchGestureStateBinding b)
		{
		if (this.enabled = b.enabled)
			{
			this.Enable();

				

			this.rawPressBinding					.CopyFrom(b.rawPressBinding);
			this.normalPressBinding					.CopyFrom(b.normalPressBinding);
			this.longPressBinding					.CopyFrom(b.longPressBinding);
			this.rawTapBinding						.CopyFrom(b.rawTapBinding);
			this.tapBinding							.CopyFrom(b.tapBinding);
			this.doubleTapBinding					.CopyFrom(b.doubleTapBinding);
			this.longTapBinding						.CopyFrom(b.longTapBinding);
	
			this.normalPressSwipeDirBinding			.CopyFrom(b.normalPressSwipeDirBinding);
			this.longPressSwipeDirBinding			.CopyFrom(b.longPressSwipeDirBinding);
	
			this.normalPressSwipeHorzAxisBinding	.CopyFrom(b.normalPressSwipeHorzAxisBinding);
			this.normalPressSwipeVertAxisBinding	.CopyFrom(b.normalPressSwipeVertAxisBinding);
			this.longPressSwipeHorzAxisBinding		.CopyFrom(b.longPressSwipeHorzAxisBinding);
			this.longPressSwipeVertAxisBinding		.CopyFrom(b.longPressSwipeVertAxisBinding);
	
			this.normalPressScrollHorzBinding		.CopyFrom(b.normalPressScrollHorzBinding);
			this.normalPressScrollVertBinding		.CopyFrom(b.normalPressScrollVertBinding);
			this.longPressScrollHorzBinding			.CopyFrom(b.longPressScrollHorzBinding);
			this.longPressScrollVertBinding			.CopyFrom(b.longPressScrollVertBinding);
	
			this.rawPressMousePosBinding			.CopyFrom(b.rawPressMousePosBinding);
			this.normalPressMousePosBinding			.CopyFrom(b.normalPressMousePosBinding);
			this.longPressMousePosBinding			.CopyFrom(b.longPressMousePosBinding);
			this.tapMousePosBinding					.CopyFrom(b.tapMousePosBinding);
			this.doubleTapMousePosBinding			.CopyFrom(b.doubleTapMousePosBinding);
			this.longTapMousePosBinding				.CopyFrom(b.longTapMousePosBinding);
			this.normalPressSwipeMousePosBinding	.CopyFrom(b.normalPressSwipeMousePosBinding);
			this.longPressSwipeMousePosBinding		.CopyFrom(b.longPressSwipeMousePosBinding);
	
			this.rawPressEmuTouchBinding			.CopyFrom(b.rawPressEmuTouchBinding);
			this.normalPressEmuTouchBinding			.CopyFrom(b.normalPressEmuTouchBinding);
			this.longPressEmuTouchBinding			.CopyFrom(b.longPressEmuTouchBinding);

			this.normalPressSwipeJoyBinding			.CopyFrom(b.normalPressSwipeJoyBinding);
			this.longPressSwipeJoyBinding			.CopyFrom(b.longPressSwipeJoyBinding);
		
			}
		}

	// -------------------
	public void SyncTouchState(TouchGestureState screenState, TouchGestureState orientedState, JoystickState swipeJoyState, InputRig rig)
		{
		if ((screenState == null) || !this.enabled || (rig == null))
			return;
			
		if (orientedState == null)
			orientedState = screenState;

		// Sync taps...

		if (screenState.JustTapped(1))
			{
			this.tapBinding.Sync(true, rig);
			this.tapMousePosBinding.SyncPos(screenState.GetTapPos(), rig);
			}


		if (screenState.JustTapped(2))
			{
			this.doubleTapBinding.Sync(true, rig);
			this.doubleTapMousePosBinding.SyncPos(screenState.GetTapPos(), rig);
			}

		if (screenState.JustLongTapped())
			{
			this.longTapBinding.Sync(true, rig);
			this.longTapMousePosBinding.SyncPos(screenState.GetTapPos(), rig);
			}			

		// Sync swipe direction...

		if (orientedState.PressedLong())
			this.longPressSwipeDirBinding.SyncDirectionState(orientedState.GetSwipeDirState(), rig);
		else if (orientedState.PressedNormal())
			this.normalPressSwipeDirBinding.SyncDirectionState(orientedState.GetSwipeDirState(), rig); 


		if (orientedState.PressedRaw())
			{
			// Press binding...

			this.rawPressBinding.Sync(true, rig);
			this.rawPressMousePosBinding.SyncPos(screenState.GetCurPosSmooth(), rig);

			if (screenState.PressedNormal())
				{
				this.normalPressBinding.Sync(true, rig);
				this.normalPressMousePosBinding.SyncPos(screenState.GetCurPosSmooth(), rig);
				}

			if (screenState.PressedLong())
				{
				this.longPressBinding.Sync(true, rig);
				this.longPressMousePosBinding.SyncPos(screenState.GetCurPosSmooth(), rig);
				}

			// Bind swipe and scroll deltas..

			Vector2 swipeDelta = orientedState.GetConstrainedDeltaVec();	//.GetDeltaVecSmooth();
			Vector2 scrollDelta = orientedState.GetScrollDelta();

			if (orientedState.PressedLong())
				{
				
				this.longPressScrollHorzBinding.SyncScrollDelta(Mathf.RoundToInt(scrollDelta.x), rig);
				this.longPressScrollVertBinding.SyncScrollDelta(Mathf.RoundToInt(scrollDelta.y), rig);

				if (screenState.Swiped())
					{
					this.longPressSwipeMousePosBinding.SyncPos(screenState.GetCurPosSmooth(), rig);
					this.longPressSwipeHorzAxisBinding.SyncFloat(swipeDelta.x, InputRig.InputSource.TouchDelta, rig);
					this.longPressSwipeVertAxisBinding.SyncFloat(swipeDelta.y, InputRig.InputSource.TouchDelta, rig);
					}

				if (swipeJoyState != null)
					this.longPressSwipeJoyBinding.SyncJoyState(swipeJoyState, rig);

				}
			else if (orientedState.PressedNormal())
				{
				
				this.normalPressScrollHorzBinding.SyncScrollDelta(Mathf.RoundToInt(scrollDelta.x), rig);
				this.normalPressScrollVertBinding.SyncScrollDelta(Mathf.RoundToInt(scrollDelta.y), rig);

				if (screenState.Swiped())
					{
					this.normalPressSwipeMousePosBinding.SyncPos(screenState.GetCurPosSmooth(), rig);
					this.normalPressSwipeHorzAxisBinding.SyncFloat(swipeDelta.x, InputRig.InputSource.TouchDelta, rig);
					this.normalPressSwipeVertAxisBinding.SyncFloat(swipeDelta.y, InputRig.InputSource.TouchDelta, rig);
					}

				if (swipeJoyState != null)
					this.normalPressSwipeJoyBinding.SyncJoyState(swipeJoyState, rig);

				}
		
			}
	


		// Sync emu-tocuch position...

		this.rawPressEmuTouchBinding.SyncState(screenState, rig);

		if (orientedState.PressedLong())
			this.longPressEmuTouchBinding.SyncState(screenState, rig);
		else if(orientedState.PressedNormal())
			this.normalPressEmuTouchBinding.SyncState(screenState, rig);


		}


	// --------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{
		if (!this.enabled)
			return false;
		
		return (

			this.rawPressBinding				.IsBoundToAxis(axisName, rig) ||
			this.normalPressBinding				.IsBoundToAxis(axisName, rig) ||
			this.longPressBinding				.IsBoundToAxis(axisName, rig) ||
			this.tapBinding						.IsBoundToAxis(axisName, rig) ||
			this.doubleTapBinding				.IsBoundToAxis(axisName, rig) ||
			this.longTapBinding					.IsBoundToAxis(axisName, rig) ||
	
			this.normalPressSwipeDirBinding		.IsBoundToAxis(axisName, rig) ||
			this.longPressSwipeDirBinding		.IsBoundToAxis(axisName, rig) ||
	
			this.normalPressSwipeHorzAxisBinding.IsBoundToAxis(axisName, rig) ||
			this.normalPressSwipeVertAxisBinding.IsBoundToAxis(axisName, rig) ||
			this.longPressSwipeHorzAxisBinding	.IsBoundToAxis(axisName, rig) ||
			this.longPressSwipeVertAxisBinding	.IsBoundToAxis(axisName, rig) ||
	
			this.normalPressScrollHorzBinding	.IsBoundToAxis(axisName, rig) ||
			this.normalPressScrollVertBinding	.IsBoundToAxis(axisName, rig) ||
			this.longPressScrollHorzBinding		.IsBoundToAxis(axisName, rig) ||
			this.longPressScrollVertBinding		.IsBoundToAxis(axisName, rig) ||
	
			this.rawPressMousePosBinding		.IsBoundToAxis(axisName, rig) ||
			this.normalPressMousePosBinding		.IsBoundToAxis(axisName, rig) ||
			this.longPressMousePosBinding		.IsBoundToAxis(axisName, rig) ||
			this.tapMousePosBinding				.IsBoundToAxis(axisName, rig) ||
			this.doubleTapMousePosBinding		.IsBoundToAxis(axisName, rig) ||
			this.longTapMousePosBinding			.IsBoundToAxis(axisName, rig) ||
			this.normalPressSwipeMousePosBinding.IsBoundToAxis(axisName, rig) ||
			this.longPressSwipeMousePosBinding	.IsBoundToAxis(axisName, rig) ||
	
			this.longPressSwipeJoyBinding		.IsBoundToAxis(axisName, rig) ||
			this.normalPressSwipeJoyBinding		.IsBoundToAxis(axisName, rig) ||

			this.rawPressEmuTouchBinding		.IsBoundToAxis(axisName, rig) ||
			this.normalPressEmuTouchBinding		.IsBoundToAxis(axisName, rig) ||
			this.longPressEmuTouchBinding		.IsBoundToAxis(axisName, rig) );
		}



	// -----------------
	override protected bool OnIsBoundToKey(KeyCode keyCode, InputRig rig)
		{
		if (!this.enabled)
			return false;

		return (

			this.rawPressBinding				.IsBoundToKey(keyCode, rig) ||
			this.normalPressBinding				.IsBoundToKey(keyCode, rig) ||
			this.longPressBinding				.IsBoundToKey(keyCode, rig) ||
			this.tapBinding						.IsBoundToKey(keyCode, rig) ||
			this.doubleTapBinding				.IsBoundToKey(keyCode, rig) ||
			this.longTapBinding					.IsBoundToKey(keyCode, rig) ||
	
			this.normalPressSwipeDirBinding		.IsBoundToKey(keyCode, rig) ||
			this.longPressSwipeDirBinding		.IsBoundToKey(keyCode, rig) ||
	
			this.normalPressSwipeHorzAxisBinding.IsBoundToKey(keyCode, rig) ||
			this.normalPressSwipeVertAxisBinding.IsBoundToKey(keyCode, rig) ||
			this.longPressSwipeHorzAxisBinding	.IsBoundToKey(keyCode, rig) ||
			this.longPressSwipeVertAxisBinding	.IsBoundToKey(keyCode, rig) ||
	
			this.longPressSwipeJoyBinding		.IsBoundToKey(keyCode, rig) ||
			this.normalPressSwipeJoyBinding		.IsBoundToKey(keyCode, rig) ||

			this.normalPressScrollHorzBinding	.IsBoundToKey(keyCode, rig) ||
			this.normalPressScrollVertBinding	.IsBoundToKey(keyCode, rig) ||
			this.longPressScrollHorzBinding		.IsBoundToKey(keyCode, rig) ||
			this.longPressScrollVertBinding		.IsBoundToKey(keyCode, rig) );

		}

//	// ---------------
//	override protected bool OnIsBoundToJoystick(string joyName, InputRig rig)
//		{
//		return false;
//		}

	// ----------------
	override protected bool OnIsEmulatingMousePosition()
		{
		return (
			this.rawPressMousePosBinding		.IsEmulatingMousePosition() ||
			this.normalPressMousePosBinding		.IsEmulatingMousePosition() ||
			this.longPressMousePosBinding		.IsEmulatingMousePosition() ||
			this.tapMousePosBinding				.IsEmulatingMousePosition() ||
			this.doubleTapMousePosBinding		.IsEmulatingMousePosition() ||
			this.longTapMousePosBinding			.IsEmulatingMousePosition() ||
			this.normalPressSwipeMousePosBinding.IsEmulatingMousePosition() ||
			this.longPressSwipeMousePosBinding	.IsEmulatingMousePosition() ||
	
			this.longPressSwipeJoyBinding		.IsEmulatingMousePosition() ||
			this.normalPressSwipeJoyBinding		.IsEmulatingMousePosition() ||

			this.rawPressEmuTouchBinding		.IsEmulatingMousePosition() ||
			this.normalPressEmuTouchBinding		.IsEmulatingMousePosition() ||
			this.longPressEmuTouchBinding		.IsEmulatingMousePosition() );
		}

	// ------------------
	override protected bool OnIsEmulatingTouches()
		{
		return (
			this.rawPressMousePosBinding		.IsEmulatingTouches() ||
			this.normalPressMousePosBinding		.IsEmulatingTouches() ||
			this.longPressMousePosBinding		.IsEmulatingTouches() ||
			this.tapMousePosBinding				.IsEmulatingTouches() ||
			this.doubleTapMousePosBinding		.IsEmulatingTouches() ||
			this.longTapMousePosBinding			.IsEmulatingTouches() ||
			this.normalPressSwipeMousePosBinding.IsEmulatingTouches() ||
			this.longPressSwipeMousePosBinding	.IsEmulatingTouches() ||
	
			this.longPressSwipeJoyBinding		.IsEmulatingTouches() ||
			this.normalPressSwipeJoyBinding		.IsEmulatingTouches() ||

			this.rawPressEmuTouchBinding		.IsEmulatingTouches() ||
			this.normalPressEmuTouchBinding		.IsEmulatingTouches() ||
			this.longPressEmuTouchBinding		.IsEmulatingTouches() );
		}

	

		

	// ---------------
	override protected void OnGetSubBindingDescriptions(BindingDescriptionList descList, //BindingDescription.BindingType typeMask, 
		Object undoObject, string parentMenuPath) //, bool addUnusedBindings, int axisSourceTypeMask)
		{
		descList.Add(this.rawPressBinding, 		"Press (Raw)", parentMenuPath, undoObject);
		descList.Add(this.normalPressBinding,	"Press (Normal)", parentMenuPath, undoObject);
		descList.Add(this.longPressBinding, 	"Long Press", 	parentMenuPath, undoObject);
		descList.Add(this.tapBinding, 			"Tap", parentMenuPath, undoObject);
		descList.Add(this.doubleTapBinding,	 	"Double Tap", parentMenuPath, undoObject);
		descList.Add(this.longTapBinding, 		"Long Tap", parentMenuPath, undoObject);

		descList.Add(this.normalPressScrollHorzBinding, "Horizontal Scroll (Normal Press)", parentMenuPath, undoObject);
		descList.Add(this.normalPressScrollVertBinding, "Vertical Scroll (Normal Press)", parentMenuPath, undoObject);
		descList.Add(this.longPressScrollHorzBinding, 	"Horizontal Scroll (Long Press)", parentMenuPath, undoObject);
		descList.Add(this.longPressScrollVertBinding, 	"Vertical Scroll (Long Press)", parentMenuPath, undoObject);

		descList.Add(this.normalPressSwipeHorzAxisBinding, InputRig.InputSource.TouchDelta, "Horizontal Swipe Delta (Normal Press)", parentMenuPath, undoObject);
		descList.Add(this.normalPressSwipeVertAxisBinding, InputRig.InputSource.TouchDelta, "Vertical Swipe Delta (Normal Press)", parentMenuPath, undoObject);
		descList.Add(this.longPressSwipeHorzAxisBinding, InputRig.InputSource.TouchDelta, "Horizontal Swipe Delta (Long Press)", parentMenuPath, undoObject);
		descList.Add(this.longPressSwipeVertAxisBinding, InputRig.InputSource.TouchDelta, "Vertical Swipe Delta (Long Press)", parentMenuPath, undoObject);

		descList.Add(this.normalPressSwipeDirBinding, "Swipe Direction (Normal Press)", parentMenuPath, undoObject);
		descList.Add(this.longPressSwipeDirBinding, "Swipe Direction (Long Press)", parentMenuPath, undoObject);

		descList.Add(this.normalPressSwipeJoyBinding,	"Swipe Joystick (Normal Press)", parentMenuPath, undoObject);
		descList.Add(this.longPressSwipeJoyBinding,		"Swipe Joystick (Long Press)", parentMenuPath, undoObject);

		descList.Add(this.rawPressMousePosBinding,		"Raw Press Position", parentMenuPath, undoObject);
		descList.Add(this.normalPressMousePosBinding,	"Normal Press Position", parentMenuPath, undoObject);
		descList.Add(this.longPressMousePosBinding,		"Long Press Position", parentMenuPath, undoObject);
		descList.Add(this.tapMousePosBinding,			"Tap Position", parentMenuPath, undoObject);
		descList.Add(this.doubleTapMousePosBinding,		"Double Tap Position", parentMenuPath, undoObject);
		descList.Add(this.longTapMousePosBinding,		"Long Tap Position", parentMenuPath, undoObject);
		descList.Add(this.normalPressSwipeMousePosBinding,"Swipe Position (Normal Press)", parentMenuPath, undoObject);
		descList.Add(this.longPressSwipeMousePosBinding,"Swipe Position (Long Press)", parentMenuPath, undoObject);

		descList.Add(this.normalPressEmuTouchBinding, 	"Emulated Touch (Normal Press)", parentMenuPath, undoObject);
		descList.Add(this.longPressEmuTouchBinding, 	"Emulated Touch (Long Press)", parentMenuPath, undoObject);

			

		}


	}



}

//! \endcond
