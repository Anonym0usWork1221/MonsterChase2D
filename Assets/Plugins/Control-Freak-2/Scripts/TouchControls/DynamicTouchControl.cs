// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

using ControlFreak2.Internal;

namespace ControlFreak2 //.Internal
{
// -------------------------------
//! Base class for dynamic touch controls.
// -------------------------------
public abstract class DynamicTouchControl : TouchControl
	{
//! \cond
	public bool fadeOutWhenReleased	= true;
	public bool	startFadedOut		= false;
	public float fadeOutTargetAlpha = 0;
	public float fadeInDuration		= 0.2f;
	public float fadeOutDelay		= 0.5f;
	public float fadeOutDuration	= 0.5f;

		
	public bool centerOnDirectTouch		= true;
	public bool centerOnIndirectTouch	= true;
	public bool	centerWhenFollowing		= false;

	public bool stickyMode;
	public bool clampInsideRegion = true;
	public bool clampInsideCanvas = true;
	public bool returnToStartingPosition = false;

	public Vector2 	
		directInitialVector,
		indirectInitialVector;

		
	public float touchSmoothing;


	[Tooltip("Stick's origin smoothing - the higher the value, the slower the movement."), Range(0, 1.0f)]
	public float originSmoothTime = 0.5f;

	const float ORIGIN_ANIM_MAX_TIME = 0.2f;


		
	public 		DynamicRegion	targetDynamicRegion;
	protected 	bool			linkedToDynamicRegion;
	protected	bool			touchStartedByRegion;
	protected TouchStartType 	touchStartType;
	
	protected TouchGestureBasicState
		touchStateOriented,
		touchStateScreen,
		touchStateWorld;

	public TouchGestureBasicState
		TouchStateScreen		{ get { return this.touchStateScreen; } }
	public TouchGestureBasicState
		TouchStateOriented	{ get { return this.touchStateOriented; } }
	public TouchGestureBasicState
		TouchStateWorld		{ get { return this.touchStateWorld; } }



	protected TouchObject		touchObj;	

	private RectTransform		initialRectCopy;

	private Vector3 
		originPos,
		originStartPos;

	private bool originAnimOn;
	private float originAnimElapsed;
		




		

	// -----------------
	public DynamicTouchControl() : base()
		{
		this.touchStateScreen		= new TouchGestureBasicState();
		this.touchStateWorld		= new TouchGestureBasicState();
		this.touchStateOriented		= new TouchGestureBasicState();
		this.directInitialVector	= Vector2.zero;
		this.indirectInitialVector	= Vector2.zero;
		this.startFadedOut			= true;
		this.fadeOutWhenReleased	= true;
		this.touchSmoothing			= 0.1f;

		}



	// ------------------
	override protected void OnInitControl()
		{	
		this.SetTargetDynamicRegion(this.targetDynamicRegion);		
			
		this.SetTouchSmoothing(this.touchSmoothing);


		// Create a copy of this rectTransform...
			
		this.StoreDefaultPos();
			
		}


	// ------------------
	public override void ResetControl ()
		{
		if (this.CanFadeOut() && this.startFadedOut && !CFUtils.editorStopped)
			this.DynamicFadeOut(false);	
		else
			this.DynamicWakeUp(false);
		}
	

	// --------------
	override public void InvalidateHierarchy()
		{
		base.InvalidateHierarchy();
	
		this.StoreDefaultPos();	
		}

//! \endcond


	// ----------------
	public bool Pressed()			{ return this.touchStateWorld.PressedRaw(); }
	public bool JustPressed()		{ return this.touchStateWorld.JustPressedRaw(); }
	public bool JustReleased()		{ return this.touchStateWorld.JustReleasedRaw(); }

	//// ----------------
	//private bool IsPhysicallyTouched() { return (this.touchStateWorld.IsPressureSensitive().PressedRaw() && (this.touchObj != null)); }

	// -----------------
	public bool		IsTouchPressureSensitive() { return (this.touchStateWorld.PressedRaw() && this.touchStateWorld.IsPressureSensitive()); } 

	public float	GetTouchPressure()			{ return (this.touchStateWorld.PressedRaw() ? this.touchStateWorld.GetPressure() : 0); }
	//public float	GetAbsTouchPressure()		{ return (this.IsPhysicallyTouched() ? this.touchObj.GetAbsPressure() : 0); }
	//public float	GetMaxTouchPressure()		{ return (this.IsPhysicallyTouched() ? this.touchObj.GetMaxPressure() : 0); }


	// ------------------
	//! Set this control's touch smoothing time as a fraction of max touch smoothing time.
	// ------------------
	public void SetTouchSmoothing(float smTime)
		{	
		this.touchSmoothing = Mathf.Clamp01(smTime);
			
		this.touchStateWorld	.SetSmoothingTime(this.touchSmoothing * InputRig.TOUCH_SMOOTHING_MAX_TIME);
		this.touchStateOriented	.SetSmoothingTime(this.touchSmoothing * InputRig.TOUCH_SMOOTHING_MAX_TIME);
		this.touchStateScreen	.SetSmoothingTime(this.touchSmoothing * InputRig.TOUCH_SMOOTHING_MAX_TIME);
		}

		

	// -------------------
	//! Set this control's dynamic region.
	// ----------------
	public void SetTargetDynamicRegion(DynamicRegion targetDynamicRegion)
		{
		this.targetDynamicRegion = targetDynamicRegion;
			
		if ((targetDynamicRegion != null) && targetDynamicRegion.CanBeUsed())
			{	
			targetDynamicRegion.SetTargetControl(this);
			}				
		}

//! \cond
	// ---------------
	public void OnLinkToDynamicRegion(DynamicRegion dynRegion)
		{
		this.linkedToDynamicRegion = ((dynRegion != null) && (dynRegion == this.targetDynamicRegion));
		}
//! \endcond


	// --------------
	//! Get this control's dynamic region.
	// --------------
	public DynamicRegion GetDynamicRegion()
		{
		return (this.linkedToDynamicRegion ? this.targetDynamicRegion : null);
		//return this.dynamicRegion;		
		}



	// ----------------
	//! Is this control in dynamic mode?
	// ----------------
	public bool IsInDynamicMode()
		{
		return (this.GetDynamicRegion() != null);
		}


	// ----------------
	//! Get dynamic alpha (not multiplied by base alpha).		
	// ------------------
	public float GetDynamicAlpha()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return 1.0f;
#endif
		return this.dynamicAlphaCur;
		}

//! \cond

	// -----------------
	override public float GetAlpha()
		{
		return this.GetDynamicAlpha() * base.GetAlpha(); 
		}
		


	
	
	private bool
		dynamicIsFadingOut,
		dynamicAlphaAnimOn,
		dynamicWaitingToFadeOut;
	private float		
		dynamicAlphaAnimDur,
		dynamicAlphaAnimElapsed,
		dynamicFadeOutDelayElapsed,	
		dynamicAlphaCur,
		dynamicAlphaStart,
		dynamicAlphaTarget;
		


	// --------------------
	private void SetDynamicAlpha(float alpha, float animDur)
		{	
		if (animDur > 0.001f)
			{
			this.dynamicAlphaAnimDur = animDur;
			this.dynamicAlphaStart = this.dynamicAlphaCur;
			this.dynamicAlphaTarget = alpha;
			this.dynamicAlphaAnimElapsed = 0;
			this.dynamicAlphaAnimOn = true;
			}
		else
			{
			this.dynamicAlphaAnimOn = false;
			this.dynamicAlphaAnimElapsed = 0;
			this.dynamicAlphaCur = this.dynamicAlphaTarget = this.dynamicAlphaStart = alpha;
			}
		}
				
	// --------------------
	private void DynamicWakeUp(bool animate)
		{
		this.dynamicIsFadingOut = false;
		this.dynamicWaitingToFadeOut = false;
		this.SetDynamicAlpha(1.0f, (animate ? this.fadeInDuration : 0));
		}

	// --------------------
	private void DynamicFadeOut(bool animate)
		{
		if (!animate)
			{
			this.dynamicIsFadingOut = true;
			this.dynamicWaitingToFadeOut = false;	
			this.dynamicFadeOutDelayElapsed = 0;
			this.SetDynamicAlpha(this.fadeOutTargetAlpha, 0);
			}
		else
			{
			if (this.dynamicIsFadingOut)
				return;

			this.dynamicIsFadingOut = true;
			this.dynamicWaitingToFadeOut = true;
			this.dynamicFadeOutDelayElapsed = 0;
			}
		
		}

		
	// ----------------------	
	private bool CanFadeOut()
		{
		return (this.fadeOutWhenReleased && this.IsInDynamicMode());
		}

	// --------------------
	private void UpdateDynamicAlpha()
		{

		// Update anim...

		if (this.dynamicAlphaAnimOn)
			{
			this.dynamicAlphaAnimElapsed += CFUtils.realDeltaTime;
			if (this.dynamicAlphaAnimElapsed > this.dynamicAlphaAnimDur)
				{
				this.dynamicAlphaAnimOn = false;
				this.dynamicAlphaCur = this.dynamicAlphaTarget;
				}
			else
				{
				this.dynamicAlphaCur = Mathf.Lerp(this.dynamicAlphaStart, this.dynamicAlphaTarget, (this.dynamicAlphaAnimElapsed / this.dynamicAlphaAnimDur));
				}
			}

		

		// Control fadeout...

		if (this.dynamicIsFadingOut)
			{
			if (this.dynamicWaitingToFadeOut)
				{
				this.dynamicFadeOutDelayElapsed += CFUtils.realDeltaTime;
				if (this.dynamicFadeOutDelayElapsed >= this.fadeOutDelay)
					{
					this.dynamicWaitingToFadeOut = false;	
					this.SetDynamicAlpha(this.fadeOutTargetAlpha, this.fadeOutDuration);
					} 
				}

			}
		else
			{
			}
		

		
		}



	// -----------------
	public override void SetWorldPos(Vector2 pos2D)
		{
		this.SetOriginPos(pos2D, false);
		this.StoreDefaultPos();
		}

	// -----------------
	protected void SetOriginPos(Vector3 pos, bool animate)
		{
		this.originPos = pos;

		if (animate)
			{
			this.originStartPos = this.GetWorldPos();	
			this.originAnimOn = true;
			this.originAnimElapsed = 0;
			}
		else
			{	
			this.SetWorldPosRaw(this.originPos); //this.transform.position = this.originPos;
			this.originStartPos = this.originPos;
			this.originAnimOn = false;
			}
		}

	protected void SetOriginPos(Vector3 pos) { SetOriginPos(pos, true); }



	// ---------------------
	protected Vector2 GetOriginOffset()
		{
		return (this.transform.position - this.originPos);
		}

	// -------------------
	protected void UpdateOriginAnimation()
		{			
		if (this.originAnimOn)
			{
			this.originAnimElapsed += CFUtils.realDeltaTime;
			if (this.originAnimElapsed >= (this.originSmoothTime * ORIGIN_ANIM_MAX_TIME))
				{
				this.originAnimOn = false;
				this.SetWorldPosRaw(this.originPos);	
				}
			else
				{
				this.SetWorldPosRaw(Vector3.Lerp(this.originStartPos, this.originPos, 
					(this.originAnimElapsed / (this.originSmoothTime * ORIGIN_ANIM_MAX_TIME))));
				}
			}

		}

		


		

	// ----------------------
	//! Store this dynamic control's default position. 
	// ------------------
	protected void StoreDefaultPos()
		{
		if (CFUtils.editorStopped)
			return;

		//if (this.initialRectCopy == null)
		//	{
		//	GameObject go = new GameObject(this.name + "_INITIAL_POS", typeof(RectTransform));
		//	this.initialRectCopy = go.GetComponent<RectTransform>();
		//	}

		//RectTransform rectTr = this.GetComponent<RectTransform>();

		//this.initialRectCopy.hideFlags = HideFlags.DontSave; //.HideAndDontSave;

		//this.initialRectCopy.SetParent(rectTr.parent, false);
		//this.initialRectCopy.anchoredPosition3D = rectTr.anchoredPosition3D;
		//this.initialRectCopy.anchorMax = rectTr.anchorMax;
		//this.initialRectCopy.anchorMin = rectTr.anchorMin;
		//this.initialRectCopy.offsetMin = rectTr.offsetMin;
		//this.initialRectCopy.offsetMax = rectTr.offsetMax;
		//this.initialRectCopy.pivot = rectTr.pivot;


		if (this.initialRectCopyGo == null)
			{
			this.initialRectCopyGo = new GameObject(this.name + "_INITIAL_POS", typeof(InitialPosPlaceholder));
			}

		RectTransform rectTr = this.GetComponent<RectTransform>();

		this.initialRectCopyGo.transform.SetParent(rectTr.parent, false);	 		


		this.initialRectCopy = this.initialRectCopyGo.GetComponent<RectTransform>();


		this.initialRectCopyGo.hideFlags = HideFlags.HideAndDontSave;

		this.initialAnchorMin				= rectTr.anchorMin;
		this.initialAnchorMax				= rectTr.anchorMax;
		this.initialOffsetMin				= rectTr.offsetMin;
		this.initialOffsetMax				= rectTr.offsetMax;
		this.initialAnchoredPosition3D	= rectTr.anchoredPosition3D;
		this.initialPivot						= rectTr.pivot;

		this.SetupInitialRectPosition();
		}

	private GameObject 
		initialRectCopyGo;
	private Vector2 
		initialAnchorMax,
		initialAnchorMin,	
		initialOffsetMin,
		initialOffsetMax,
		initialPivot;
	private Vector3 
		initialAnchoredPosition3D;


	// -----------------
	private void SetupInitialRectPosition()
		{
		if (this.initialRectCopy == null)
			return;

		this.initialRectCopy.anchoredPosition3D	= this.initialAnchoredPosition3D;
		this.initialRectCopy.anchorMin				= this.initialAnchorMin;
		this.initialRectCopy.anchorMax				= this.initialAnchorMax;
		this.initialRectCopy.offsetMin				= this.initialOffsetMin;
		this.initialRectCopy.offsetMax				= this.initialOffsetMax;
		this.initialRectCopy.pivot						= this.initialPivot;
		}


	//
	// -------------------
	protected Vector3 GetDefaultPos()
		{	
		//Vector3 defaultPos = this.transform.position;

		//if (this.initialRectCopyGo != null)
		//	{
		//	if (this.initialRectCopy != null)
		//		return this.initialRectCopy.position;

		//	// If the rectTransform was added by Unity after initialRectCopyGO creation, set it's layout and wait to the next frame...

		//	this.initialRectCopy = this.initialRectCopyGo.GetComponent<RectTransform>();
		//	if (this.initialRectCopy != null)
		//		this.SetupInitialRectPosition();
		//	}

		//return defaultPos;

		return ((this.initialRectCopy == null) ? this.transform.position : this.initialRectCopy.position);
		}


	// -------------------
	override protected void OnDestroyControl()
		{
		this.ResetControl(); 
			
		// Destroy initial rect copy...

		if (this.initialRectCopyGo != null)
			GameObject.Destroy(this.initialRectCopyGo);

		}

	// ----------------------
	override protected void OnUpdateControl()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return;
#endif

		if ((this.touchObj != null) && (this.rig != null))
			this.rig.WakeTouchControlsUp();

	
		this.UpdateDynamicAlpha();


		this.touchStateWorld.Update();
		this.touchStateScreen.Update();
		this.touchStateOriented.Update();

		if (this.touchStateScreen.JustPressedRaw())
			{
			this.DynamicWakeUp(true);

			// Static mode...
	
			if (!this.IsInDynamicMode())
				{
				this.SetOriginPos(this.GetWorldPos(), false);
				}
	
			// Dynamic mode...
	
			else
				{		
				bool isFadedOut = (this.GetDynamicAlpha() < 0.001f);
		
				if (!this.centerOnDirectTouch && !this.touchStartedByRegion) 
					{
					this.SetOriginPos(this.GetWorldPos(), false);
					}
				else
					{

					Vector2 originPos = this.touchStateWorld.GetStartPos();


					// Add indirect initial offset...

					if (this.touchStartedByRegion)
						{
						if (this.indirectInitialVector != Vector2.zero)
							originPos -= this.NormalizedToWorldOffset(this.indirectInitialVector);
						}
					else
						{
						if (this.directInitialVector != Vector2.zero)
							originPos -= this.NormalizedToWorldOffset(this.directInitialVector);
						}

					if (!isFadedOut && !this.centerOnIndirectTouch) 
						{
						originPos = this.GetFollowPos(originPos, Vector2.zero); 
						}
	
					if (this.clampInsideRegion && (this.GetDynamicRegion() != null))
						originPos = this.ClampInsideOther(originPos, this.GetDynamicRegion());
			
					if (this.clampInsideCanvas && (this.canvas != null))
						originPos = this.ClampInsideCanvas(originPos, this.canvas);
						
					
					this.SetOriginPos(originPos, !isFadedOut);
					}
				}		
			}
			
			

		if (this.touchStateWorld.JustReleasedRaw())
			{
			// Return to initial pos in dynamic mode...

			if (!this.IsInDynamicMode() || this.returnToStartingPosition)
				{
				this.SetOriginPos(this.GetDefaultPos(), true);
				}				

			}


		// Fade-out in dynamic mode...
	
		if (this.IsInDynamicMode() && this.fadeOutWhenReleased && !this.touchStateWorld.PressedRaw())
			this.DynamicFadeOut(true);

			

		// Reset 'started by region' flag...

		this.touchStartedByRegion = false;



		// Update following...

		if (this.touchStateWorld.PressedRaw()) 
			{
			if (this.stickyMode && ((this.swipeOffMode == TouchControl.SwipeOffMode.Disabled) || 
				(this.swipeOffMode == TouchControl.SwipeOffMode.OnlyIfSwipedOver) && (this.touchStartType != TouchStartType.SwipeOver))) // && this.IsInDynamicMode())
				{
				bool pressWasOutside = true;
				Vector3 followPos = this.touchStateWorld.GetCurPosSmooth();
					
				if (!this.centerWhenFollowing)
					{
					pressWasOutside = false;
					followPos = this.GetFollowPos(followPos, this.GetOriginOffset(), out pressWasOutside);
					}


				if (pressWasOutside)
					{
					if (this.clampInsideRegion && (this.GetDynamicRegion() != null))
						followPos = this.ClampInsideOther(followPos, this.GetDynamicRegion()); //this.joyRegion);
			
					if (this.clampInsideCanvas && (this.canvas != null))
						followPos = this.ClampInsideCanvas(followPos, this.canvas);
						
					
					this.SetOriginPos(followPos);
					}
				}
			}
			
		this.UpdateOriginAnimation();

		}
	


	// -------------------
	override public bool OnTouchStart(TouchObject touch, TouchControl sender, TouchStartType touchStartType)
		{
		if (this.touchObj != null)
			return false;
	
		this.touchObj = touch;		
		this.touchObj.AddControl(this);

		Vector3 
			startPosScreen		= ((touchStartType == TouchStartType.DirectPress) ? touch.screenPosStart : touch.screenPosCur),
			curPosScreen		= touch.screenPosCur,

			startPosOriented	= this.ScreenToOrientedPos(startPosScreen, touch.cam),
			curPosOriented		= this.ScreenToOrientedPos(curPosScreen, touch.cam),

			startPosWorld		= this.ScreenToWorldPos(startPosScreen, touch.cam),
			curPosWorld			= this.ScreenToWorldPos(curPosScreen, touch.cam);


		this.touchStateWorld.OnTouchStart	(startPosWorld,		curPosWorld, 0, this.touchObj);
		this.touchStateScreen.OnTouchStart	(startPosScreen,		curPosScreen, 0, this.touchObj);
		this.touchStateOriented.OnTouchStart(startPosOriented,	curPosOriented, 0, this.touchObj);
 
		this.touchStartedByRegion = ((sender != null) && (sender != this)); //== this.GetDynamicRegion()));
		this.touchStartType = touchStartType;
	
		return true;
		}

		 
	// -------------------
	override public bool OnTouchEnd(TouchObject touch, TouchEndType touchEndType) 
		{
		if ((touch != this.touchObj) || (this.touchObj == null))
			return false;

		this.touchObj = null;
		this.touchStateWorld.OnTouchEnd(touchEndType != TouchEndType.Release); //cancel);
		this.touchStateScreen.OnTouchEnd(touchEndType != TouchEndType.Release); //cancel);
		this.touchStateOriented.OnTouchEnd(touchEndType != TouchEndType.Release); //cancel);

		return true;
		}
		
		

	// -------------------
	override public bool OnTouchMove(TouchObject touch) //, TouchControl sender)
		{
		if ((touch != this.touchObj) || (this.touchObj == null))
			return false;
			
		Vector3
			posScreen	= touch.screenPosCur,
			posWorld	= this.ScreenToWorldPos(touch.screenPosCur, touch.cam),
			posOriented	= this.ScreenToOrientedPos(touch.screenPosCur, touch.cam);

		this.touchStateWorld.OnTouchMove(posWorld); 
		this.touchStateScreen.OnTouchMove(posScreen); 
		this.touchStateOriented.OnTouchMove(posOriented); 

		this.CheckSwipeOff(touch, this.touchStartType);

		return true;
		}



	// -------------------
	override public bool OnTouchPressureChange(TouchObject touch) 
		{
		if ((touch != this.touchObj) || (this.touchObj == null))
			return false;
			

		this.touchStateWorld.OnTouchPressureChange(touch.GetPressure());
		this.touchStateScreen.OnTouchPressureChange(touch.GetPressure());
		this.touchStateOriented.OnTouchPressureChange(touch.GetPressure());
	
		return true;
		}
		

		

	// ----------------------
	override public void ReleaseAllTouches() 
		{
		if (this.touchObj != null)
			{
			this.touchObj.ReleaseControl(this, TouchEndType.Cancel); 
			this.touchObj = null;
			}

		this.touchStateWorld.OnTouchEnd(true); //(touchEndType != TouchEndType.RELEASE)); //cancel);
		this.touchStateOriented.OnTouchEnd(true); //(touchEndType != TouchEndType.RELEASE)); //cancel);
		this.touchStateScreen.OnTouchEnd(true); //(touchEndType != TouchEndType.RELEASE)); //cancel);
		}
		


	// -------------------	
	override public bool CanBeTouchedDirectly(TouchObject touchObj)
		{
		return (base.CanBeTouchedDirectly(touchObj) && (this.touchObj == null) );
		}

	// ---------------------
	public override bool CanBeSwipedOverFromNothing(TouchObject touchObj)
		{
		return (base.CanBeSwipedOverFromNothingDefault(touchObj) && (this.touchObj == null) && this.IsActiveAndVisible());
		}


	// ---------------------
	public override bool CanBeSwipedOverFromRestrictedList(TouchObject touchObj)
		{
		return (base.CanBeSwipedOverFromRestrictedListDefault(touchObj) && (this.touchObj == null) && this.IsActiveAndVisible());
		}


	// -----------------------
	public override bool CanSwipeOverOthers(TouchObject touchObj)
		{
		return this.CanSwipeOverOthersDefault(touchObj, this.touchObj, this.touchStartType);
		}

	// -------------------
	virtual public bool CanBeActivatedByDynamicRegion()
		{
		return ((this.touchObj == null) && this.IsActive());
		}

	// -------------------
	override public bool CanBeActivatedByOtherControl(TouchControl c, TouchObject touchObj)
		{
		return (base.CanBeActivatedByOtherControl(c, touchObj) && (this.touchObj == null));
		}
//! \endcond

	}
}

