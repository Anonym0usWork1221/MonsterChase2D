// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;

namespace ControlFreak2.Internal
{
	
// -----------------------------
//! Basic Touch Gesture State Class.
// ------------------------

public class TouchGestureBasicState
	{
//! \cond
	protected bool
		controlledByMouse,
		pollStartedByMouse;
	
	protected bool	
		isPressureSensitive,
		pollStartPressureSensitive;
	protected float
		pressureCur,
		pollStartPressure;

	protected bool
		pressCur,
		pressPrev;

	protected float 
		releasedDur,
		elapsedSincePress,
		elapsedSinceRelease,		
		curDist,
		releasedDist;

	protected Vector2 
		posCurRaw,
		posPrevRaw,
		posCurSmooth,
		posPrevSmooth,
		posNext,
		posStart;

		
	protected float 
		elapsedSinceLastMove;

	// Released touch's state...

	protected Vector2
		relStartPos,
		relEndPos,
		relExtremeDistPerAxis;
		//relVelPerAxis;

	protected float
		relDur,
		relExtremeDistSq,
		relDistSq;
		//relVel;

	protected float
		extremeDistCurSq,
		extremeDistPrevSq;

	protected Vector2 
		extremeDistPerAxisCur,
		extremeDistPerAxisPrev;

	protected float 
		smoothingTime;
		


	protected bool
		prepollState,		
		//pollCurState,
		pollStartedPress,	
		pollReleased,
		pollReleasedAsCancel;

	protected float
		pollCurPressure;

	protected Vector2 
		pollCurPos,
		pollPressStartPos,
		pollReleasePos;

	protected float
		pollPressStartDelay;


//! \endcond
	
		
	// -----------------
	public TouchGestureBasicState()
		{
		}


	// -------------------
	//! Reset state. 
	// ----------------
	virtual public void Reset()
		{	
		this.pressCur			= false;
		this.pressPrev			= false;

		this.isPressureSensitive = false;
		this.pressureCur = 1;
			
		this.prepollState		= false;
		//this.pollCurState		= false;
		this.pollReleased		= false;		
		this.pollReleasedAsCancel = false;
		this.pollStartedPress	= false;
		this.pollStartPressureSensitive = false;
		this.pollStartPressure	= 1;
		this.pollCurPressure		= 1;

		this.pollCurPos		= Vector2.zero;
		this.pollPressStartPos = Vector2.zero;
		this.pollReleasePos = Vector2.zero;
		this.pollPressStartDelay = 0;



		this.curDist			= 0;
		this.extremeDistCurSq	= 0;
		this.extremeDistPrevSq	= 0;
		this.releasedDist 		= 0;
		this.extremeDistPerAxisCur	= Vector2.zero;
		this.extremeDistPerAxisPrev	= Vector2.zero;
		this.posCurRaw				= Vector2.zero;
		this.posCurSmooth			= Vector2.zero;
		this.posPrevRaw				= Vector2.zero;
		this.posPrevSmooth			= Vector2.zero;
		this.relEndPos				= Vector2.zero;
		this.relDur				= 0;
		this.releasedDist		= 0;
		this.relStartPos		= Vector2.zero;
		}
	
			
	// --------------------
	//! Set smoothing time in seconds.
	// --------------------
	public void SetSmoothingTime(float smt)	{ this.smoothingTime = smt; }
		

	// ----------------------
	public bool PressedRaw()			{ return this.pressCur; }									//!< Is pressed? (raw)
	public bool JustPressedRaw()		{ return (this.pressCur && !this.pressPrev); }		//!< Just pressed? (raw)
	public bool JustReleasedRaw()		{ return (!this.pressCur && this.pressPrev); }		//!< Just released? (raw)

	public Vector2 GetCurPosRaw()		{ return this.posCurRaw; }					//!< Get current position (raw, unsmoothed)
	public Vector2 GetCurPosSmooth()	{ return this.posCurSmooth; }				//!< Get current position (smoothed)
	public Vector2 GetStartPos()		{ return this.posStart; }					//!< Get start position
	public Vector2 GetReleasedStartPos(){ return this.relStartPos; } 			//!< Get released touch's start position
	public Vector2 GetReleasedEndPos()	{ return this.relEndPos; }				//!< Get released touch's end position
	public Vector2 GetSwipeVecRaw()		{ return (this.posCurRaw - this.posStart); } 	//!< Get current swipe vector (raw)
	public Vector2 GetSwipeVecSmooth()	{ return (this.posCurSmooth - this.posStart); } //!< Get current swipe vector (smoothed)
	public Vector2 GetDeltaVecRaw()		{ return (this.posCurRaw - this.posPrevRaw); } 	//!< Get current swipe delta (raw)
	public Vector2 GetDeltaVecSmooth()	{ return (this.posCurSmooth - this.posPrevSmooth); }//!< Get current swipe delta (smoothed) 


	//public Vector2 GetLastUsedPos()		{ return this.posCurRaw; }
		
	public bool		Moved(float threshSquared)		{ return (this.extremeDistCurSq > threshSquared); }	//!< Returns true if current touch's movement exceeded given (squared) threshold.
	public bool		JustMoved(float threshSquared)	{ return ((this.extremeDistCurSq > threshSquared) && (this.extremeDistPrevSq <= threshSquared)); } //!< Returns true if current touch's movement just exceeded given (squared) threshold.

	public bool		IsControlledByMouse()		{ return this.controlledByMouse; }	//!< Returns true if current touch is controlled by mouse.

	public bool		IsPressureSensitive()	{ return this.PressedRaw() && this.isPressureSensitive; }	//!< Is this touch pressure sensitive?
	public float	GetPressure()				{ return (this.PressedRaw() ? this.pressureCur : 0); }		//!< Get current touch pressure.
	
	
//! \cond

	// -------------
	virtual public void Update()
		{
		this.InternalUpdate();
		this.InternalPostUpdate();
		}
		

	// --------------------
	protected void InternalUpdate()
		{
		this.posPrevRaw				= this.posCurRaw;
		this.posPrevSmooth			= this.posCurSmooth;
		this.pressPrev				= this.pressCur;
		this.extremeDistPrevSq		= this.extremeDistCurSq;
		this.extremeDistPerAxisPrev = this.extremeDistPerAxisCur;
			

		// Process collected 'events'...


	
		// Release...

		if ((this.pollReleased && (this.prepollState == true))) 
			{
			this.posCurRaw = this.pollReleasePos;
			this.posCurSmooth = this.posCurRaw;

			this.OnRelease(this.pollReleasePos, this.pollReleasedAsCancel);
			
			this.pollReleased	= false;
			this.prepollState	= this.pressCur;
				
			if (this.pollStartedPress)
				this.pollPressStartDelay += CFUtils.realDeltaTime;	// add this frame's deltaTime to cached press event (?)
			}

		// Press...

		else if ((this.pollStartedPress && (this.prepollState == false))) // || (!this.pressCur && this.pollCurState))
			{
			this.OnPress(this.pollPressStartPos, this.pollCurPos, this.pollPressStartDelay, 
				this.pollStartedByMouse, this.pollStartPressureSensitive, this.pollStartPressure);
	
			this.pollStartedPress = false;
			this.prepollState	= this.pressCur;
			}

		else
			{
			this.posCurRaw = this.pollCurPos;
			this.posCurSmooth = CFUtils.SmoothTowardsVec2(this.posCurSmooth, this.posCurRaw, this.smoothingTime, CFUtils.realDeltaTime, 0);

			this.pressureCur = this.pollCurPressure;
			}
			
			


		// Check movement...
			
		if (this.pressCur)
			this.CheckMovement(false);
	
		}


			
	// --------------------
	protected void InternalPostUpdate()
		{
		// Update timers...

		this.elapsedSincePress		+= CFUtils.realDeltaTime;
		this.elapsedSinceRelease	+= CFUtils.realDeltaTime;
		this.elapsedSinceLastMove	+= CFUtils.realDeltaTime;

		}

		


	


	// ----------------------
	// Events 
	// ----------------------
		

	// --------------------
	public void OnTouchStart(Vector2 startPos, Vector2 curPos, float timeElapsed, TouchObject touchObj)
		{
		this.OnTouchStart(startPos, curPos, timeElapsed, touchObj.IsMouse(), touchObj.IsPressureSensitive(), touchObj.GetPressure());
		}

	// ----------------------
	public void OnTouchStart(Vector2 startPos, Vector2 curPos, float timeElapsed, bool controlledByMouse, 
		bool isPressureSensitive /*= false*/, float pressure /*= 1*/)
		{
		if (this.prepollState == false)
			{
			this.pollReleased = false;	// cancel mid frame press
			}
		else
			{
			if (!this.pollReleased)	// if there was no mid-frame release, this shouldn't happen - IGNORE!!
				return;
			}
	

		this.pollStartedPress		= true;
		this.pollPressStartPos		= startPos;
		this.pollPressStartDelay	= timeElapsed;
		this.pollStartedByMouse		= controlledByMouse;
		this.pollStartPressureSensitive = isPressureSensitive;
		this.pollStartPressure		= pressure;
		this.pollCurPos				= curPos;	
		}
		


	// ----------------------
	public void OnTouchPressureChange(float pressure)
		{
		this.isPressureSensitive = true;
		this.pollCurPressure	= pressure;
		}



	// ------------
	public void OnTouchMove(Vector2 pos)
		{
		this.pollCurPos = pos;
		}
		
	// ---------------
	public void OnTouchMoveByDelta(Vector2 delta)
		{
		this.pollCurPos += delta;
		}
		

	// ------------
	public void OnTouchEnd(Vector2 pos, bool cancel)
		{
		if (this.prepollState == true)
			{
			if (this.pollReleased)	// if there's a release pending, ignore this one...
				return;
			}
		else
			{
			if (!this.pollStartedPress)	// releasing a touch that didn't even started?! Ignoring!
				return;
			}

		this.pollReleased = true;	
		this.pollReleasedAsCancel = cancel;	
		this.pollReleasePos = pos;

		}

	public void OnTouchEnd(bool cancel)
		{
		OnTouchEnd(this.pollCurPos, cancel);
		}
		



	// -----------------
	virtual protected void OnPress(Vector2 startPos, Vector2 pos, float delay, 
		bool startedByMouse, bool isPressureSensitive = false, float pressure = 1)
		{
		this.elapsedSincePress 		= delay;
		this.posStart				= startPos;
		this.posPrevRaw				= startPos;	// TODO : ??!
		this.posPrevSmooth			= this.posPrevRaw;
		this.posCurRaw				= pos;		
		this.posCurSmooth			= pos;	// TODO ? 	
		this.pressCur 				= true;
		this.controlledByMouse		= startedByMouse;		
		this.isPressureSensitive	= isPressureSensitive;
		this.pressureCur				= pressure;	


		this.extremeDistCurSq		= 0;
		this.extremeDistPrevSq		= 0;
		this.extremeDistPerAxisCur	= 
		this.extremeDistPerAxisPrev = 
			Vector2.zero;

			
		this.elapsedSinceLastMove 	= 0;

		}


	// --------------------
	virtual protected void OnRelease(Vector2 pos, bool cancel)
		{
		this.elapsedSinceRelease 	= 0;
		this.posCurRaw				= pos;
		this.posCurSmooth			= pos;
		this.pressCur 				= false;
	
		this.CheckMovement(true);

		this.relStartPos 			= this.posStart;
		this.relEndPos				= pos;			
		this.relDur					= this.elapsedSincePress;
		this.relDistSq				= (this.relEndPos - this.relStartPos).sqrMagnitude;

		this.relExtremeDistSq		= this.extremeDistCurSq;		
		this.relExtremeDistPerAxis	= this.extremeDistPerAxisCur;
		}

		
	// --------------------
	virtual protected void CheckMovement(bool itsFinalUpdate)
		{
		Vector2 delta = (this.posCurRaw - this.posStart);
	
		this.extremeDistCurSq	= Mathf.Max(this.extremeDistCurSq, delta.sqrMagnitude);
		this.extremeDistPerAxisCur = Vector2.Max(this.extremeDistPerAxisCur, new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y)));
		}		


//! \endcond


	}









// -----------------------------------
//! Advanced Touch Gesture State Class.
// -----------------------------------
public class TouchGestureState : TouchGestureBasicState
	{
		
//! \cond

	private float 
		elapsedSinceLastTapCandidate;


	private Vector2 
		segmentOrigin,
		
		constrainedVecCur,
		constrainedVecPrev,

		scrollVecCur,
		scrollVecPrev;

		
	private TouchGestureConfig.DirConstraint
		swipeConstraint,
		swipeDirConstraint,
		scrollConstraint;


	private bool
		cleanPressCur,
		cleanPressPrev,

		holdPressCur,
		holdPressPrev;
		
		

	private Vector2 
		lastDirOrigin;
		
	public TouchGestureThresholds
		thresh;

	public TouchGestureConfig
		config;



	private bool 
		moved, 
		justMoved;
		//justCharged;
		
	private bool	
		nonStaticFlag,
		cancelTapFlag,
		disableTapUntilReleaseFlag,
		flushTapsFlag,
		holdDelayedEvents;		// when true, wait with fireing delayed events such as taps, etc.

	private bool
		justTapped,
		justLongTapped;

	private Vector2
		tapPos;

	private int 
		tapCount;


	private DirectionState
		swipeDirState,	
		swipeDirState4,	
		swipeDirState8;	


	private bool
		blockDrag;




	// -----------------
	private void BasicConstructor(TouchGestureThresholds thresholds, TouchGestureConfig config)
		{
		this.config = config;
		this.thresh = thresholds;


		this.swipeDirState = new DirectionState();
		this.swipeDirState4 = new DirectionState();
		this.swipeDirState8 = new DirectionState();

	
		this.Reset();
		}

	// -----------------
	public TouchGestureState(TouchGestureThresholds thresholds, TouchGestureConfig config) : base()
		{
		this.BasicConstructor(thresholds, config);
		}
		

	// -------------------
	public TouchGestureState() : base()
		{	
		this.BasicConstructor(null, null);
		}


	// -------------------
	public void SetThresholds(TouchGestureThresholds thresh)	{	this.thresh = thresh;	}

	// -------------------
	public void SetConfig(TouchGestureConfig config)			{	this.config = config; }


	// ----------------
	override public void Reset() 
		{
		base.Reset();

		this.swipeConstraint		= TouchGestureConfig.DirConstraint.None;
		this.scrollConstraint	= TouchGestureConfig.DirConstraint.None;
		this.swipeDirConstraint	= TouchGestureConfig.DirConstraint.None;

	
		this.constrainedVecCur	= Vector2.zero;
		this.constrainedVecPrev =  Vector2.zero;

		this.tapCount = 0;
		this.tapCandidateCount = 0;
		this.elapsedSincePress = 0;
		this.elapsedSinceRelease = 0;
		this.tapCandidateReleased =false;

		this.moved = false;
		this.justMoved = false;
		this.justTapped = false;
		this.justLongTapped = false;
			
		this.cleanPressCur = false;
		this.cleanPressPrev = false;
		
		this.holdPressCur	= false;
		this.holdPressPrev	= false;		
	
		this.flushTapsFlag = false;
		this.nonStaticFlag = false;
		this.cancelTapFlag = false;
		this.disableTapUntilReleaseFlag = false;

		this.swipeDirState.Reset();
		this.swipeDirState4.Reset();
		this.swipeDirState8.Reset();

		}

//! \endcond



	// ------------------
	//! \name Normal Press methods.
	//! \{
	// -----------------
	public bool PressedNormal()			{ return this.cleanPressCur; }
	public bool JustPressedNormal()		{ return (this.cleanPressCur && !this.cleanPressPrev); }
	public bool JustReleasedNormal()		{ return (!this.cleanPressCur && this.cleanPressPrev); }
	//! \}
		
	// ------------------
	//! \name Long Press methods.
	//! \{
	// -----------------
	public bool PressedLong()				{ return this.holdPressCur; }
	public bool JustPressedLong()			{ return (this.holdPressCur && !this.holdPressPrev); }
	public bool JustReleasedLong()		{ return (!this.holdPressCur && this.holdPressPrev); }
	//! \}


	// ----------------------
	public bool JustTapped(int howManyTimes)	{ return (this.justTapped && (this.tapCount == howManyTimes)); }
	public Vector2 GetTapPos()					{ return this.tapPos; }
		

	public bool JustLongTapped()				{ return this.justLongTapped; }
	
	public DirectionState	GetSwipeDirState()	{ return this.swipeDirState; }
	public DirectionState	GetSwipeDirState4()	{ return this.swipeDirState4; }
	public DirectionState	GetSwipeDirState8()	{ return this.swipeDirState8; }
	
	public Vector2	GetSegmentOrigin()	{ return this.segmentOrigin; }
		
	public bool		JustSwiped()		{ return this.justMoved; }
	public bool		Swiped()			{ return this.moved; }

	public Vector2	GetConstrainedSwipeVec()	{ return this.constrainedVecCur; }
	public Vector2	GetConstrainedDeltaVec()	{ return (this.constrainedVecCur - this.constrainedVecPrev); }

	public Vector2	GetScroll()			{ return this.scrollVecCur; }
	public Vector2	GetScrollDelta()	{ return (this.scrollVecCur - this.scrollVecPrev); }
	


	public float	GetReleasedDurationRaw()	{ return this.relDur; }		//!< Get released touch's total duration.


//! \cond

	public void		ForceSwipe()			{ if (!this.moved) this.OnSwipeStart(); }
	
	// -----------------
	public void BlockSwipe()				
		{ 
		this.blockDrag = true; 
		
		if (this.moved)
			{		
			this.moved = false;
			this.justMoved = false; 
			}
		}
		

	// ------------------	
	public void HoldDelayedEvents(bool holdThemForNow)
		{
		this.holdDelayedEvents = holdThemForNow;
		}
		

	// -----------
	public void CancelTap()
		{
		if (this.PressedRaw())
			this.cancelTapFlag = true;

		this.ResetTapState();
		}
		
	// -------------------
	public void DisableTapUntilRelease()
		{
		this.disableTapUntilReleaseFlag = true;
			
		this.ResetTapState();
		}
		
	// --------------
	public void FlushRegisteredTaps()
		{
		this.flushTapsFlag = true;
		}
		
	public void MarkAsNonStatic()
		{
		this.nonStaticFlag = true;
		}


	// -------------
	override public void Update()
		{
		if (this.thresh != null)
			this.thresh.Recalc(CFScreen.dpi);
			
		this.justTapped			= false;
		this.justMoved			= false;
		this.justLongTapped 	= false;

		this.cleanPressPrev 	= this.cleanPressCur;
		this.holdPressPrev 		= this.holdPressCur;
	
		this.scrollVecPrev 		= this.scrollVecCur;
		this.constrainedVecPrev	= this.constrainedVecCur;

		this.swipeDirState.BeginFrame();
		this.swipeDirState4.BeginFrame();
		this.swipeDirState8.BeginFrame();



		// Update base...

		base.InternalUpdate();


		// Update tap state...

		this.CheckTap(false);


		// Update timers...
			
		this.InternalPostUpdate();


		if (this.tapCandidateReleased)
			this.elapsedSinceLastTapCandidate 	+= CFUtils.realDeltaTime;

		}

		


	// --------------------
	override protected void OnRelease(Vector2 pos, bool cancel)
		{
		if (cancel)
			this.CancelTap();

		base.OnRelease(pos, cancel);


		}

	// -----------------
	override protected void OnPress(Vector2 startPos, Vector2 pos, float delay, bool startedByMouse, bool isPressureSensitive, float pressure)
		{
		base.OnPress(startPos, pos, delay, startedByMouse, isPressureSensitive, pressure);

		this.blockDrag				= false;
		this.moved					= false;
		this.justMoved 				= false;
		this.nonStaticFlag			= false;
			
			
		this.constrainedVecCur = this.constrainedVecPrev = Vector2.zero;

		this.scrollVecCur = this.scrollVecPrev = Vector2.zero;
		
		this.segmentOrigin = this.posCurSmooth;
	
		this.scrollConstraint	= TouchGestureConfig.DirConstraint.None;
		this.swipeConstraint		= TouchGestureConfig.DirConstraint.None;
		this.swipeDirConstraint	= TouchGestureConfig.DirConstraint.None;

		if (this.config != null)
			{
			this.scrollConstraint = this.config.scrollConstraint;

			this.swipeConstraint = this.config.swipeConstraint;

			this.swipeDirConstraint = this.config.swipeDirConstraint;
			}


		this.swipeDirState.Reset();
		this.swipeDirState4.Reset();
		this.swipeDirState8.Reset();
		}

		
	// -----------------
	private void OnSwipeStart()
		{
		if (!this.moved)
			{
			this.justMoved = true;
			this.moved = true;	
			}
		}

	// -----------------
	override protected void CheckMovement(bool itsFinalUpdate)
		{
		base.CheckMovement(itsFinalUpdate);
		
		if ((this.thresh == null) || (this.config == null))
			return;



		if (!this.PressedRaw())
			{
			this.cleanPressCur = false;
			this.holdPressCur = false;
			}
		else
			{
			// Detect long-press...

			bool longPressStillPossible = false;

			if (this.holdPressCur)
				{
				// End long-press on move?

				if (this.config.endLongPressWhenMoved && (this.nonStaticFlag || this.Moved(this.thresh.tapMoveThreshPxSq))) 	
					this.holdPressCur = false;
		
				// End long-press on swipe?

				else if (this.config.endLongPressWhenSwiped && this.Swiped())
					this.holdPressCur = false;

				else 
					{	
					// TODO check for long tap 
					}

				}
			else if (this.config.detectLongPress)
				{
				if (!this.nonStaticFlag && !this.Moved(this.thresh.tapMoveThreshPxSq))	
					{

					if (this.elapsedSincePress > this.thresh.longPressMinTime)
						{
						this.holdPressCur = true;
						}
					else
						{
						longPressStillPossible = true;
						}
					}
				}


			// Detect normal press...
			
			if (this.cleanPressCur)
				{
				}
			else if (!this.holdPressCur && !longPressStillPossible && !this.IsPotentialTap())
				{
				this.cleanPressCur = true;
				}
			}
			

		// Calculate scroll vector...

		Vector2 dragVecCur = this.GetSwipeVecRaw();

		if (this.scrollConstraint != TouchGestureConfig.DirConstraint.None)
			{
			if (this.scrollConstraint == TouchGestureConfig.DirConstraint.Auto)
				{
				if (Mathf.Abs(dragVecCur.x) >  this.thresh.scrollThreshPx)
					this.scrollConstraint = TouchGestureConfig.DirConstraint.Horizontal;

				if ((Mathf.Abs(dragVecCur.y) >  this.thresh.scrollThreshPx) && (Mathf.Abs(dragVecCur.y) > Mathf.Abs(dragVecCur.x)))
					this.scrollConstraint = TouchGestureConfig.DirConstraint.Vertical;
				}

			if (this.scrollConstraint == TouchGestureConfig.DirConstraint.Horizontal)
				dragVecCur.y = 0;
			else if (this.scrollConstraint == TouchGestureConfig.DirConstraint.Vertical)
				dragVecCur.x = 0;
			}

		for (int axisi = 0; axisi < 2; ++axisi)
			{
			this.scrollVecCur[axisi] = 
				CFUtils.GetScrollValue(dragVecCur[axisi], (int)this.scrollVecCur[axisi], this.thresh.scrollThreshPx, this.thresh.scrollMagnetFactor);
			}


		// Determinate swipe constraint...

		this.constrainedVecCur = this.GetSwipeVecSmooth();

		if (this.swipeConstraint != TouchGestureConfig.DirConstraint.None)
			{
			if (this.swipeConstraint == TouchGestureConfig.DirConstraint.Auto)
				{
				if (Mathf.Abs(this.extremeDistPerAxisCur.x) > this.thresh.dragThreshPx)
					this.swipeConstraint = TouchGestureConfig.DirConstraint.Horizontal;

				if ((Mathf.Abs(this.extremeDistPerAxisCur.y) > this.thresh.dragThreshPx) && 
					(Mathf.Abs(this.extremeDistPerAxisCur.y) > Mathf.Abs(this.extremeDistPerAxisCur.x)))
					this.swipeConstraint = TouchGestureConfig.DirConstraint.Vertical;
				}

			if (this.swipeConstraint == TouchGestureConfig.DirConstraint.Horizontal)
				this.constrainedVecCur.y = 0;

			else if (this.swipeConstraint == TouchGestureConfig.DirConstraint.Vertical)
				this.constrainedVecCur.x = 0;

			else 
				this.constrainedVecCur = Vector2.zero;
			}

		// Swipe check...

		if (!this.moved && !this.blockDrag)
			{
			if ((this.constrainedVecCur.sqrMagnitude > this.thresh.dragThreshPxSq))
				{
				this.OnSwipeStart();
				}
			}
	
			
		// Swipe direction...

		Vector2 swipeSegVec = (this.posCurSmooth - this.segmentOrigin);

		if (this.swipeDirConstraint != TouchGestureConfig.DirConstraint.None)
			{
			if (this.swipeDirConstraint == TouchGestureConfig.DirConstraint.Auto)
				{
				if (Mathf.Abs(swipeSegVec.x) > this.thresh.swipeSegLenPx)
					this.swipeDirConstraint = TouchGestureConfig.DirConstraint.Horizontal;

				if ((Mathf.Abs(swipeSegVec.y) > this.thresh.swipeSegLenPx) && (Mathf.Abs(swipeSegVec.y) > Mathf.Abs(swipeSegVec.x)))
					this.swipeDirConstraint = TouchGestureConfig.DirConstraint.Vertical;
				}

			if (this.swipeDirConstraint == TouchGestureConfig.DirConstraint.Horizontal)
				swipeSegVec.y = 0;

			else if (this.swipeDirConstraint == TouchGestureConfig.DirConstraint.Vertical)
				swipeSegVec.x = 0;

			else 
				swipeSegVec = Vector2.zero;
			}


		float swipeSegVecLenSq = swipeSegVec.sqrMagnitude;


		if (swipeSegVecLenSq > this.thresh.swipeSegLenPxSq)
			{
			swipeSegVec.Normalize();


			this.swipeDirState4.SetDir(CFUtils.VecToDir(swipeSegVec, this.swipeDirState4.GetCur(), 0.1f, false),	config.swipeOriginalDirResetMode);
			this.swipeDirState8.SetDir(CFUtils.VecToDir(swipeSegVec, this.swipeDirState8.GetCur(), 0.1f, true),		config.swipeOriginalDirResetMode);

			this.swipeDirState.SetDir(((config.dirMode == TouchGestureConfig.DirMode.EightWay) ? this.swipeDirState8 : this.swipeDirState4).GetCur(),
				config.swipeOriginalDirResetMode);


			this.segmentOrigin = this.posCurSmooth;			
			}

		
		}



private Vector2 
	tapFirstPos;

private int tapCandidateCount;
private int lastReportedTapCount;
	
private bool tapCandidateReleased;
		

	// -------------------
	private void ResetTapState()
		{
		this.tapCandidateCount				= 0;
		this.tapCandidateReleased			= false;
		this.elapsedSinceLastTapCandidate	= 0;	
		this.lastReportedTapCount			= 0;
		}

		
	// -----------------
	public bool IsPotentialTap()	
		{
		if ((this.thresh == null) || (this.config == null))
			return false;

		return (this.PressedRaw() && !this.cancelTapFlag && !this.disableTapUntilReleaseFlag &&
			(this.config.maxTapCount > 0) && !this.nonStaticFlag &&
			(this.elapsedSincePress < this.thresh.tapMaxDur) && (this.extremeDistCurSq < this.thresh.tapMoveThreshPxSq)); 
		}

	// -----------------
	public bool IsPotentialLongPress()	
		{
		if ((this.thresh == null) || (this.config == null))
			return false;

		return (this.PressedRaw() && !this.nonStaticFlag && this.config.detectLongPress && 
			(this.elapsedSincePress < this.thresh.longPressMinTime) && (this.extremeDistCurSq < this.thresh.tapMoveThreshPxSq)); 
		}



	// -----------------
	private void CheckTap(bool lastUpdate)
		{
		if ((this.thresh == null) || (this.config == null))
			return;


		if (this.config.maxTapCount <= 0)	
			return;

		if (this.flushTapsFlag)
			{
			this.ReportTap(true);
			this.flushTapsFlag = false;
			}

			
		if (!this.PressedRaw())
			{
			// If the time space between potential multi-taps got too long, report confirmed taps...

			if (this.tapCandidateReleased && !this.holdDelayedEvents && (this.elapsedSinceLastTapCandidate > this.thresh.multiTapMaxTimeGap))
				this.ReportTap(true);
			}
		else 
			{
			// If this press moved past tap threshold or press is too long, forget about this one and report confirmed...

			if (this.tapCandidateCount > 0)
				{
				if ((!this.holdDelayedEvents && (this.elapsedSincePress > this.thresh.tapMaxDur)) || this.nonStaticFlag ||
					(this.extremeDistCurSq > this.thresh.tapMoveThreshPxSq))
					{
					if (this.config.cleanTapsOnly)
						this.ResetTapState();	
					else
						this.ReportTap(true);
					}
				}
			}

		if (this.JustPressedRaw())
			{
			if (this.tapCandidateCount > 0)
				{
				// If a new press is too far from the first tap, report only the confirmed taps...

				if ((this.posCurRaw - this.tapFirstPos).sqrMagnitude > this.thresh.tapPosThreshPxSq)
					{
					if (this.config.cleanTapsOnly)
						this.ResetTapState();	
					else
						this.ReportTap(true);
					}			

				}	
				
			// If this is the first tap, store it's start pos...

			if (this.tapCandidateCount == 0)
				this.tapFirstPos = this.posStart;
				
			}

		if (this.JustReleasedRaw())
			{
			
			// If this is a tap candidate...
				
			if (this.cancelTapFlag || this.disableTapUntilReleaseFlag)
				{
				this.ResetTapState();
				}
			else
				{
				// Detect long tap...

				if (this.config.detectLongTap && (this.relDur > this.thresh.longPressMinTime) && !this.nonStaticFlag &&
					(this.relDur <= (this.thresh.longPressMinTime + this.thresh.longTapMaxDuration)) && 
					(this.relExtremeDistSq <= this.thresh.tapMoveThreshPxSq)) 
					{
					this.justLongTapped = true;	
					this.tapPos = this.relStartPos;
					}


				// Detect normal taps...

				if ((this.relDur <= this.thresh.tapMaxDur) && !this.nonStaticFlag && (this.relExtremeDistSq <= this.thresh.tapMoveThreshPxSq))
					{
					++this.tapCandidateCount;
	
					if (this.tapCandidateCount >= this.config.maxTapCount)
						{
						this.ReportTap(true);
						}
					else
						{							
						if (!this.config.cleanTapsOnly)
							this.ReportTap(false);

						this.tapCandidateReleased = true;
						this.elapsedSinceLastTapCandidate = 0;
						}
					}
	
				// ... if not, report confirmed taps (if any) and reset tap state...
				else 
					{
					if (this.config.cleanTapsOnly)
						this.ResetTapState();	
					else
						this.ReportTap(true);
					}		
				}		
		
			// Clear tap-cancel flag...

			this.disableTapUntilReleaseFlag	= false;
			this.cancelTapFlag					= false;
			} 

		

		}

		
	// -------------------
	private void ReportTap()
		{ ReportTap(true); }

	private void ReportTap(bool reset /* = true*/)
		{
		if ((this.tapCandidateCount > 0) && (this.tapCandidateCount > this.lastReportedTapCount))
			{
			this.lastReportedTapCount	= this.tapCandidateCount;
			this.justTapped 			= true;
			this.tapPos					= this.tapFirstPos;
			this.tapCount				= this.tapCandidateCount;
			}
			
		if (reset)
			this.ResetTapState();
		}

		

//! \endcond

	}




}
