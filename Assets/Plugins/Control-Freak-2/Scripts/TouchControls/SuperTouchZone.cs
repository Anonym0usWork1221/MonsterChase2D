// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

#define CF2_TOUCH_ZONE_EMULATE_FINGERS
//#define CF2_DISABLE_TOUCH_MARKERS


#if UNITY_EDITOR
using ControlFreak2Editor;
#endif

using UnityEngine;
using UnityEngine.EventSystems;
using ControlFreak2.Internal;

namespace ControlFreak2
{

// -------------------
//! Super Touch Zone class.
// -------------------
[ExecuteInEditMode()]

public class SuperTouchZone : TouchControl
	{
//! \cond



	public MultiTouchGestureThresholds
		customThresh;
		
	[Tooltip("How many fingers can be used on this zone at the same time?"), Range(1, 3)]
	public int maxFingers = 3;
		
	public float touchSmoothing	= 0.5f;

	public bool			disableTouchMarkers = false;


	public bool			strictMultiTouch;				//!< Two fingers need to start at the same time.
	public float		strictMultiTouchTime;
	
	public bool			freezeTwistWhenTooClose;	//!< When fingers distance drops below safe distance, twist angle will not be updated.  
	
	public bool			noPinchAfterDrag;				//!< Pinching will be ignored after multi-finger drag.
	public bool			noPinchAfterTwist;			//!< Pinching will be ignored after twisting.
					
	public bool			noTwistAfterDrag;				//!< Twisting will be ignored after multi-finger dragging.
	public bool			noTwistAfterPinch;			//!< Twisting will be ignored after pinching.
			
	public bool			noDragAfterPinch;				//!< Multi-finger dragging will be ignored after pinching.
	public bool			noDragAfterTwist;				//!< Multi-finger dragging will be ignored after twisting.

	public bool			startPinchWhenTwisting;		//!< Pinch will be marked as moving, when twist starts. Use this to eliminate possible pinch jump later on.
	public bool			startPinchWhenDragging;		//!< Pinch will be marked as moving, when multi-finger drag starts. Use this to eliminate possible pinch jump later on.
						
	public bool			startDragWhenPinching;		//!< Multi-finger drag will be marked as moving, when pinch starts. Use this to eliminate possible drag jump later on.
	public bool			startDragWhenTwisting;		//!< Multi-finger drag will be marked as moving, when twist starts. Use this to eliminate possible drag jump later on.
			
	public bool			startTwistWhenDragging;		//!< Twist will be marked as moving, when multi-finger drag starts. Use this to eliminate possible twist jump later on.
	public bool			startTwistWhenPinching;		//!< Twist will be marked as moving, when pinch starts. Use this to eliminate possible twist jump later on.
		

	// ---------------------
	/// Gesture Detection Order
	// ---------------------
	public enum GestureDetectionOrder
		{
		TwistPinchSwipe,
		TwistSwipePinch,
		
		PinchTwistSwipe,
		PinchSwipeTwist,

		SwipeTwistPinch,
		SwipePinchTwist		
		}

	public GestureDetectionOrder gestureDetectionOrder;


	private MultiFingerTouch[]
		multiFingerTouch;
			
	public MultiFingerTouchConfig[] 
		multiFingerConfigs;
		

	private FingerState[] 
		fingers,
		fingersOrdered;

	private int			
		rawFingersPressedCur,
		rawFingersPressedPrev;

		 
	private bool
		dontAllowNewFingers,
		waitingForMoreFingers;
	private float	
		elapsedSinceFirstTouch;


	private bool 
		pinchMoved,
		pinchJustMoved,
		twistMoved,
		twistJustMoved;

	private float 
		pinchDistQuietInitial,
		pinchDistInitial,
		pinchDistCur,
		pinchDistPrev;

	private float 	
		twistAngleQuietInitial,
		twistAngleInitial,
		twistAngleCur,
		twistAnglePrev;

	private int 
		twistScrollPrev,
		twistScrollCur,
	
		pinchScrollPrev,
		pinchScrollCur;


		
	public const int 
		MAX_FINGERS = 3;

	const float 
		MIN_TWIST_FINGER_DIST_SQ	= 0.1f,
		MIN_PINCH_DIST				= 0.1f;


	public EmuTouchBinding 
		separateFingersAsEmuTouchesBinding;


	public AnalogConfig
		twistAnalogConfig,
		pinchAnalogConfig;

	public AxisBinding 
		twistAnalogBinding,
		twistDeltaBinding;

	public DigitalBinding 
		twistRightDigitalBinding,
		twistLeftDigitalBinding;

	public ScrollDeltaBinding
		pinchScrollDeltaBinding,
		twistScrollDeltaBinding;
	

	public AxisBinding 
		pinchAnalogBinding,
		pinchDeltaBinding;
		
	public DigitalBinding 
		pinchDigitalBinding,
		spreadDigitalBinding;
	


	public bool
		emuWithKeys;
	public KeyCode
		emuKeyOneFinger,
		emuKeyTwoFingers,
		emuKeyThreeFingers,

		emuKeySwipeU,
		emuKeySwipeR,
		emuKeySwipeD,
		emuKeySwipeL,
	
		emuKeyTwistR,
		emuKeyTwistL,
		emuKeyPinch,			
		emuKeySpread,
			
		emuMouseTwoFingersKey	= KeyCode.LeftControl,
		emuMouseTwistKey		= KeyCode.LeftShift,
		emuMousePinchKey		= KeyCode.LeftShift,
		emuMouseThreeFingersKey;

	public enum EmuMouseAxis
		{
		X, Y		
		}

	public EmuMouseAxis
		emuMousePinchAxis = EmuMouseAxis.X,
		emuMouseTwistAxis = EmuMouseAxis.Y;


		
	public float 
		emuKeySwipeSpeed = 0.25f,	// Screen heights per second
		emuKeyPinchSpeed = 0.25f,	// Screen heights per second
		emuKeyTwistSpeed = 45.0f;	// Degrees per second
		
	public float
		mouseEmuTwistScreenFactor = 0.3f;	// Swipe distance to twist 90 degrees (in fraction of screen height). 





	// ----------------------
	public SuperTouchZone() : base()
		{
		this.strictMultiTouchTime = 0.5f;

		this.customThresh = new MultiTouchGestureThresholds();

			
		this.fingers	= new FingerState[MAX_FINGERS];
		this.fingersOrdered = new FingerState[MAX_FINGERS];
 

		for (int i = 0; i < MAX_FINGERS; ++i)
			{
			this.fingers[i] = new FingerState(this);
			this.fingersOrdered[i] = this.fingers[i];
			}

		this.multiFingerTouch = new MultiFingerTouch[MAX_FINGERS];
		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			{
			this.multiFingerTouch[i] = new MultiFingerTouch(i + 1, this); //, this.GetThresholds());
			}
	
		

		// Allocate bindings....

		this.multiFingerConfigs = new MultiFingerTouchConfig[MAX_FINGERS];

		for (int i = 0; i < this.multiFingerConfigs.Length; ++i)	
			{
			this.multiFingerConfigs[i] = new MultiFingerTouchConfig();
			}
			
		this.separateFingersAsEmuTouchesBinding	= new EmuTouchBinding();
	
		//this.twistAxisBinding			= new AxisBinding();
		this.twistAnalogBinding			= new AxisBinding();
		this.twistDeltaBinding			= new AxisBinding();
		//this.twistKeyBinding			= new DigitalBinding();
		this.twistLeftDigitalBinding	= new DigitalBinding();
		this.twistRightDigitalBinding	= new DigitalBinding();

		//this.pinchAxisBinding			= new AxisBinding();
		this.pinchAnalogBinding			= new AxisBinding();
		this.pinchDeltaBinding			= new AxisBinding();
		//this.pinchKeyBinding			= new DigitalBinding();
		this.pinchDigitalBinding		= new DigitalBinding();
		this.spreadDigitalBinding		= new DigitalBinding();

		this.pinchScrollDeltaBinding = new ScrollDeltaBinding();
		this.twistScrollDeltaBinding = new ScrollDeltaBinding();

		this.pinchAnalogConfig 			= new AnalogConfig();
		this.twistAnalogConfig			= new AnalogConfig();

			
		


		this.pinchDistCur		= MIN_PINCH_DIST;
		this.pinchDistPrev		= MIN_PINCH_DIST;
		this.pinchDistInitial	= MIN_PINCH_DIST;	
		this.pinchDistQuietInitial = MIN_PINCH_DIST;

		

#if CF2_TOUCH_ZONE_EMULATE_FINGERS
		this.InitEmulatedFingers();
#endif
		}	




	// ---------------------
	override protected void OnDestroyControl()
		{		
		this.ResetControl(); 
		}
		

	// ----------------
	override protected void OnInitControl()
		{
					
		// Connect multi-finger config to multi-finger state

		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			{
			this.multiFingerTouch[i].Init((i < this.multiFingerConfigs.Length) ? this.multiFingerConfigs[i] : null);
			}



		
			
		this.SetTouchSmoothing(this.touchSmoothing);

		this.ResetControl();
		}

	// -------------------
	override public void ResetControl()
		{
		this.ReleaseAllTouches(); //true);
			
		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			this.multiFingerTouch[i].Reset();

		for (int i = 0; i < this.fingers.Length; ++i)
			this.fingers[i].Reset();



		this.twistMoved = false;
		this.twistJustMoved = false;
		this.twistAngleCur = 0;
		this.twistAngleInitial = 0;
		this.twistAnglePrev = 0;
		this.twistAngleQuietInitial = 0;

		this.pinchMoved = false;
		this.pinchJustMoved = false;
		this.pinchDistCur		= MIN_PINCH_DIST;
		this.pinchDistPrev		= MIN_PINCH_DIST;
		this.pinchDistInitial	= MIN_PINCH_DIST;	
		this.pinchDistQuietInitial = MIN_PINCH_DIST;	
	
		this.pinchScrollCur = 0;
		this.pinchScrollPrev = 0;
		this.twistScrollCur = 0;
		this.twistScrollPrev = 0;
		}


//! \endcond


	// -------------------
	public bool	PressedRaw			(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.PressedRaw()); }
	public bool	JustPressedRaw		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.JustPressedRaw()); }
	public bool	JustReleasedRaw		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.JustReleasedRaw()); }
	
	public bool	PressedNormal		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.PressedNormal()); }
	public bool	JustPressedNormal	(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.JustPressedNormal()); }
	public bool	JustReleasedNormal	(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.JustReleasedNormal()); }

	public bool	PressedLong			(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.PressedLong()); }
	public bool	JustPressedLong		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.JustPressedLong()); }
	public bool	JustReleasedLong	(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.JustReleasedLong()); }


	public bool JustTapped			(int fingerCount, int howManyTimes) { MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.JustTapped(howManyTimes)); }
	public bool JustLongTapped		(int fingerCount) { MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchScreen.JustLongTapped()); }

	public bool SwipeActive			(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchOriented.Swiped()); }
	public bool SwipeJustActivated	(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? false : m.touchOriented.JustSwiped()); }

	public Vector2	GetCurPos		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchScreen.GetCurPosRaw()); }
	public Vector2	GetStartPos		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchScreen.GetStartPos()); }
	public Vector2	GetTapPos		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchScreen.GetTapPos()); }

	public Vector2	GetRawSwipeVector		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchOriented.GetSwipeVecRaw()); }
	public Vector2	GetRawSwipeDelta		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchOriented.GetDeltaVecRaw()); }
	public Vector2	GetUnconstrainedSwipeVector	(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchOriented.GetSwipeVecSmooth()); }
	public Vector2	GetUnconstrainedSwipeDelta		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchOriented.GetDeltaVecSmooth()); }
	public Vector2	GetSwipeVector	(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchOriented.GetConstrainedSwipeVec()); }
	public Vector2	GetSwipeDelta	(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchOriented.GetConstrainedDeltaVec()); }
	public Dir		GetSwipeDir		(int fingerCount) 	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Dir.N : m.touchOriented.GetSwipeDirState().GetCur()); }	

	public Vector2	GetScroll		(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchOriented.GetScroll()); }
	public Vector2	GetScrollDelta	(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? Vector2.zero : m.touchOriented.GetScrollDelta()); }

	public bool		PinchActive			()		{ return this.pinchMoved; }
	public bool		PinchJustActivated	()		{ return this.pinchJustMoved; }
	public float 	GetPinchScale		()		{ return (this.pinchDistCur / this.pinchDistInitial); }
	public float 	GetPinchScaleDelta	()		{ return (this.pinchDistCur / this.pinchDistPrev); }
	public float 	GetPinchDist		()		{ return (this.pinchDistCur - this.pinchDistInitial); }
	public float 	GetPinchDistDelta	()		{ return (this.pinchDistCur - this.pinchDistPrev); }

	public bool		TwistActive			()		{ return this.twistMoved; }
	public bool		TwistJustActivated	()		{ return this.twistJustMoved; }
	public float 	GetTwist			()		{ return this.twistAngleCur; }
	public float 	GetTwistDelta		()		{ return (this.twistAngleCur - this.twistAnglePrev); }
	
	public int 		GetTwistScrollDelta	()		{ return (this.twistScrollCur - this.twistScrollPrev); }
	public int 		GetPinchScrollDelta	()		{ return (this.pinchScrollCur - this.pinchScrollPrev); }
	
	public float	GetReleasedDuration(int fingerCount)	{ MultiFingerTouch m = this.GetMultiFingerTouch(fingerCount); return ((m == null) ? 0 : m.touchScreen.GetReleasedDurationRaw()); }
	

	// ------------------
	public void SetTouchSmoothing(float smPow)
		{	
		this.touchSmoothing = Mathf.Clamp01(smPow);
	
		for (int i = 0; i < this.fingers.Length; ++i)
			{
			this.fingers[i].touchScreen.SetSmoothingTime(this.touchSmoothing * InputRig.TOUCH_SMOOTHING_MAX_TIME);
			this.fingers[i].touchOriented.SetSmoothingTime(this.touchSmoothing * InputRig.TOUCH_SMOOTHING_MAX_TIME);
			}
		
		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			{
			this.multiFingerTouch[i].touchScreen.SetSmoothingTime(this.touchSmoothing * InputRig.TOUCH_SMOOTHING_MAX_TIME);
			this.multiFingerTouch[i].touchOriented.SetSmoothingTime(this.touchSmoothing * InputRig.TOUCH_SMOOTHING_MAX_TIME);
			}
		}


		

//! \cond


	// ------------------
	protected MultiFingerTouch GetMultiFingerTouch(int fingerCount)
		{
		if ((fingerCount <= 0) || (fingerCount > this.multiFingerTouch.Length))
			return null;

		return this.multiFingerTouch[fingerCount - 1]; 
		}



	// ---------------
	public MultiTouchGestureThresholds GetThresholds()
		{	
		return this.customThresh; 
		}
		


	// ---------------------------
	// Events --------------------
	// ---------------------------

	


	// -------------------
	private Vector2 GetAveragedFingerPos(FingerState[] fingers, int count, bool orientedSpace, bool useSmoothPos)
		{
		count = Mathf.Clamp(count, 0, fingers.Length);
	
		Vector2 
			boxMin	= Vector2.zero,	
			boxMax	= Vector2.zero;
			
		for (int i = 0; i < count; ++i)
			{
			TouchGestureBasicState t = (orientedSpace ? fingers[i].touchOriented : fingers[i].touchScreen); 
			Vector2 pos = (useSmoothPos ? t.GetCurPosSmooth() : t.GetCurPosRaw());
				
			if (i == 0)
				{
				boxMin = boxMax = pos;
				}	
			else
				{
				boxMin = Vector2.Min(boxMin, pos);
				boxMax = Vector2.Max(boxMax, pos);
				}
			}

		return (boxMin + ((boxMax - boxMin) * 0.5f));
		}
		
		

	// ----------------
	private int 
		multiFingerCountCur,
		multiFingerCountPrev;
	
	// ----------------
	private void StartMultiFingerTouch(int fingerNum, FingerState[] fingers, Vector2 startScreenPos, Vector2 startOrientedPos, bool quietMode)
		{
		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			{
			MultiFingerTouch mf = this.multiFingerTouch[i];
			if (mf.GetFingerCount() == fingerNum)	
				{
				mf.Start(fingers, startScreenPos, startOrientedPos, quietMode, this.elapsedSinceFirstTouch);
					
				// Store initial twist angle and pinch dist...

				if (mf.GetFingerCount() == 2)
					{
					this.pinchDistQuietInitial	= this.GetTwoFingerDist();
					this.twistAngleQuietInitial = this.GetTwoFingerAngle(0);			
					}
				}
			else
				{
				if (fingerNum > mf.GetFingerCount())
					mf.End(true);
				else
					mf.EndAndReport();	// ??
	
				if ((fingerNum > 0) && (mf.GetFingerCount() < fingerNum)) 
					{
					mf.touchScreen.CancelTap();
					mf.touchOriented.CancelTap();
					}

				}
			}		
		}
		

	// ----------------------
	private void CancelTapsOnMultiFingersOtherThan(int howManyFingers)
		{
		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			{
			MultiFingerTouch mf = this.multiFingerTouch[i];
			if (mf.GetFingerCount() != howManyFingers)
				{
				mf.touchOriented.CancelTap();
				mf.touchScreen.CancelTap();
				}	
			}
		}


	// 



	// ------------------------
	override protected void OnUpdateControl()
		{	
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return;
#endif


#if CF2_TOUCH_ZONE_EMULATE_FINGERS
		this.UpdateEmulatedFingers();
#endif

		this.rawFingersPressedPrev = this.rawFingersPressedCur;
		this.rawFingersPressedCur = 0; //fingersPressed;
			
		bool someFingersReleased = false;	// Use this to detect when to reset uni-touch origin...
			


		// Update fingers...
			

		for (int i = 0; i < this.fingers.Length; ++i)
			{
			FingerState finger = this.fingers[i];

			finger.Update();
			
			if (finger.touchScreen.PressedRaw())
				{

				this.fingersOrdered[this.rawFingersPressedCur] = finger;
				this.rawFingersPressedCur++;
				}

			if (finger.touchScreen.JustReleasedRaw())
				someFingersReleased = true;
			}
			
			
		Vector2 
			centerScreenPos		= this.GetAveragedFingerPos(this.fingersOrdered, this.rawFingersPressedCur, false, false),
			centerOrientedPos	= this.GetAveragedFingerPos(this.fingersOrdered, this.rawFingersPressedCur, true, false);



		// Send a heatbeat to the rig...

		if ((this.rawFingersPressedCur > 0) && (this.rig != null))
			this.rig.WakeTouchControlsUp();

			
			
		if (this.rawFingersPressedCur == 0)
			{
			this.elapsedSinceFirstTouch = 0;
			this.dontAllowNewFingers = false;
			}


		// Loose multi-finger touch...

		if (!this.strictMultiTouch || (this.maxFingers <= 1))
			{
			
			if (this.rawFingersPressedCur != this.rawFingersPressedPrev)
				{
				this.elapsedSinceFirstTouch	= 0;		
				this.StartMultiFingerTouch(this.rawFingersPressedCur, this.fingersOrdered, centerScreenPos, centerOrientedPos, false);
				this.dontAllowNewFingers		= false;
				this.waitingForMoreFingers		= false;

				// Cancel potential taps when this touch was started by releasing other...

				if (this.rawFingersPressedCur < this.rawFingersPressedPrev)
					{
					MultiFingerTouch mf = this.GetMultiFingerTouch(this.rawFingersPressedCur);
					if (mf != null)
						mf.DisableTapUntilRelease();
					}

				}
			}

		// Strict multi-finger touch...
		else
			{

			if ((this.rawFingersPressedCur != this.rawFingersPressedPrev) || someFingersReleased)
				{
				if ((this.rawFingersPressedCur < this.rawFingersPressedPrev) || someFingersReleased)
					{
					this.dontAllowNewFingers = (this.rawFingersPressedCur > 0);
					this.StartMultiFingerTouch(0, this.fingersOrdered, centerScreenPos, centerOrientedPos, false);
					}
				else
					{
					if ((this.rawFingersPressedPrev == 0))
						{
						this.waitingForMoreFingers = true;
						this.elapsedSinceFirstTouch = 0;		
						}

					StartMultiFingerTouch(this.rawFingersPressedCur, this.fingersOrdered, centerScreenPos, centerOrientedPos, this.strictMultiTouch);
					}
				}
			else if (this.strictMultiTouch && this.waitingForMoreFingers && (this.rawFingersPressedCur > 0))
				{
				this.elapsedSinceFirstTouch += CFUtils.realDeltaTime;
				if (this.elapsedSinceFirstTouch > this.strictMultiTouchTime)
					{
					this.waitingForMoreFingers = false;
					this.dontAllowNewFingers = true;
					}
				}
			}					

		

		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			{
			MultiFingerTouch mf = this.multiFingerTouch[i];

			if (mf.GetFingerCount() == this.rawFingersPressedCur)
				mf.SetPos(centerScreenPos, centerOrientedPos );

			mf.Update();
	
			if (mf.touchScreen.JustPressedRaw())
				{
				this.CancelTapsOnMultiFingersOtherThan(mf.GetFingerCount());
				}					
			}




		// Update two-finger gestures...

		this.UpdateTwistAndPinch(false);

		// Sync input rig state....

		if (this.separateFingersAsEmuTouchesBinding.enabled)
			{
			for (int i = 0; i < this.fingers.Length; ++i)
				{
				this.rig.SyncEmuTouch(this.fingers[i].touchScreen, ref this.fingers[i].emuTouchId);
				}
			}


		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			{
			this.multiFingerTouch[i].SyncToRig();
			}
			


		MultiTouchGestureThresholds thresh = this.GetThresholds();
		if (thresh != null)
			{
			// Bind twist and pinch scroll values...

			this.pinchScrollDeltaBinding.SyncScrollDelta(this.GetPinchScrollDelta(), this.rig);
			this.twistScrollDeltaBinding.SyncScrollDelta(this.GetTwistScrollDelta(), this.rig);


			// Bind pinch...
	
			if (this.PinchActive())
				{
				if (this.pinchAnalogBinding.enabled)
					this.pinchAnalogBinding.SyncFloat(this.pinchAnalogConfig.GetAnalogVal(this.GetPinchDist() / thresh.pinchAnalogRangePx), InputRig.InputSource.Analog, this.rig);

				if (this.pinchDeltaBinding.enabled)
					this.pinchDeltaBinding.SyncFloat((this.GetPinchDistDelta() / thresh.pinchDeltaRangePx), InputRig.InputSource.NormalizedDelta, this.rig);
			
				// Digital pinch state...
					
				if (this.GetPinchDist() > thresh.pinchDigitalThreshPx)
					this.spreadDigitalBinding.Sync(true, this.rig);

				else if (this.GetPinchDist() < thresh.pinchDigitalThreshPx)
					this.pinchDigitalBinding.Sync(true, this.rig);

				}

			// Bind twist...
	
			if (this.TwistActive())
				{
				if (this.twistAnalogBinding.enabled)
					this.twistAnalogBinding.SyncFloat(this.twistAnalogConfig.GetAnalogVal(this.GetTwist() / thresh.twistAnalogRange), InputRig.InputSource.Analog, this.rig);

				if (this.twistDeltaBinding.enabled)
					this.twistDeltaBinding.SyncFloat((this.GetTwistDelta() / thresh.twistDeltaRange), InputRig.InputSource.NormalizedDelta, this.rig);
					
				// Digital twist state...
					
				if (this.GetTwist() > thresh.twistDigitalThresh)
					this.twistRightDigitalBinding.Sync(true, this.rig);

				if (this.GetTwist() < thresh.twistDigitalThresh)
					this.twistLeftDigitalBinding.Sync(true, this.rig);
				}
			}

		}



	// -----------------
	private float GetTwoFingerDist()
		{		
		MultiFingerTouch twoFingers = this.GetMultiFingerTouch(2);
		FingerState fingerA, fingerB;
		if ((twoFingers == null) || ((fingerA = twoFingers.GetFinger(0)) == null) || ((fingerB = twoFingers.GetFinger(1)) == null))
			return 0;

		return Mathf.Max(MIN_PINCH_DIST, (fingerA.touchScreen.GetCurPosSmooth() - fingerB.touchScreen.GetCurPosSmooth()).magnitude);
		}
		
	// ------------------	
	private float GetTwoFingerAngle(float defaultAngle)
		{
		MultiFingerTouch twoFingers = this.GetMultiFingerTouch(2);
		FingerState fingerA, fingerB;
		if ((twoFingers == null) || ((fingerA = twoFingers.GetFinger(0)) == null) || ((fingerB = twoFingers.GetFinger(1)) == null))
			return defaultAngle;

		Vector2 fingerVec = fingerA.touchScreen.GetCurPosSmooth() - fingerB.touchScreen.GetCurPosSmooth();
		float fingerDistSq = fingerVec.sqrMagnitude;
		if (fingerDistSq <= MIN_TWIST_FINGER_DIST_SQ)
			return defaultAngle;
			
		float angle = CFUtils.VecToAngle(fingerVec.normalized);

		return (defaultAngle + Mathf.DeltaAngle(defaultAngle, angle));
		}



	// ------------------
	private void UpdateTwistAndPinch() { UpdateTwistAndPinch(false); }
	private void UpdateTwistAndPinch(bool lastUpdateMode /*= false*/)
		{
		if (lastUpdateMode)
			return;		// nothing to do at the end...
			

		this.pinchDistPrev = this.pinchDistCur;
		this.twistAnglePrev = this.twistAngleCur;
		this.twistScrollPrev = this.twistScrollCur;
		this.pinchScrollPrev = this.pinchScrollCur;

		this.pinchJustMoved	= false;
		this.twistJustMoved = false;

			
		MultiTouchGestureThresholds thresh = this.GetThresholds();


		MultiFingerTouch dblTouch = this.GetMultiFingerTouch(2);
		if (dblTouch == null)
			return;

		if (dblTouch.touchScreen.JustPressedRaw())
			{
			this.pinchDistInitial	= this.pinchDistQuietInitial; //.GetTwoFingerDist();
			this.pinchDistPrev		= this.pinchDistCur = this.pinchDistInitial;
			this.pinchJustMoved		= false;
			this.pinchMoved			= false;

			this.twistAngleInitial	= this.twistAngleQuietInitial; //GetFingerAngle(0);
			this.twistAngleCur		= 0;	
			this.twistAnglePrev		= 0;
			this.twistJustMoved		= false;
			this.twistMoved			= false;
	
			this.twistScrollCur = this.twistScrollPrev = 0;
			this.pinchScrollCur = this.pinchScrollPrev = 0;

			}
		
		// Reset twist and pinch state on release...

		else if (!dblTouch.touchScreen.PressedRaw())
			{			
			this.pinchDistInitial = this.pinchDistCur;
			this.twistAngleInitial = this.twistAngleCur;
			this.twistJustMoved = false;
			this.twistMoved = false;
			this.pinchMoved = false;
			this.pinchJustMoved = false;
			}


		else if (dblTouch.touchScreen.PressedRaw())
			{
			bool justTwisted = false;
			bool justPinched = false;
			bool justDragged = dblTouch.touchScreen.JustSwiped();
	

				
			// Update pinch...

			this.pinchDistCur = this.GetTwoFingerDist();
			if (!this.pinchMoved && (Mathf.Abs(this.pinchDistInitial - this.pinchDistCur) > thresh.pinchDistThreshPx))
				justPinched = true;


			// Update twist...

			this.twistAngleCur = (this.GetTwoFingerAngle(this.twistAngleCur) - this.twistAngleInitial);
			if (!this.twistMoved && (pinchDistCur > thresh.twistMinDistPx) && (Mathf.Abs(this.twistAngleCur) > thresh.twistAngleThresh))
				justTwisted = true;


			// Update pinch/twist scroll values....

			this.pinchScrollCur = CFUtils.GetScrollValue(this.pinchDistInitial - this.pinchDistCur, this.pinchScrollCur, 
				thresh.pinchScrollStepPx, thresh.pinchScrollMagnet);

			this.twistScrollCur = CFUtils.GetScrollValue(this.twistAngleCur, this.twistScrollCur, 
				thresh.twistScrollStep, thresh.twistScrollMagnet);
				
		

			// Resolve detection order...
		
			bool cancelDrag = false;

			int orderCode = 0;
			switch (this.gestureDetectionOrder)
				{
				case GestureDetectionOrder.TwistPinchSwipe :
					orderCode = ((0 	<< 0) | (1 		<< 3) | (2 	<< 6)); break;
				case GestureDetectionOrder.TwistSwipePinch :
					orderCode = ((0		<< 0) | (2 		<< 3) | (1 	<< 6)); break;
				case GestureDetectionOrder.PinchTwistSwipe :
					orderCode = ((1		<< 0) | (0 		<< 3) | (2 	<< 6)); break;
				case GestureDetectionOrder.PinchSwipeTwist :
					orderCode = ((1		<< 0) | (2 		<< 3) | (0 	<< 6)); break;
				case GestureDetectionOrder.SwipeTwistPinch :
					orderCode = ((2		<< 0) | (0 		<< 3) | (1 	<< 6)); break;
				case GestureDetectionOrder.SwipePinchTwist :
					orderCode = ((2		<< 0) | (1 		<< 3) | (0 	<< 6)); break;
				}

			
			for (int i = 0; i < 3; ++i)
				{
				int gestureCode = (orderCode >> (i * 3)) & ((1 << 3) - 1);
				switch (gestureCode)
					{
					// Twist...
					case 0 : 
						if (this.twistMoved || justTwisted)
							{
							if (this.noDragAfterTwist)
								cancelDrag = true;
							if (this.noPinchAfterTwist)
								justPinched = false;
							}
						break;

					// Pinch...
					case 1 : 
						if (this.pinchMoved || justPinched)
							{
							if (this.noDragAfterPinch)
								cancelDrag = true;
							if (this.noTwistAfterPinch)
								justTwisted = false;
							}
						break;

					// Multi-drag...
					case 2 : 
						if (dblTouch.touchScreen.Swiped() || justDragged)
							{
							if (this.noTwistAfterDrag)
								justTwisted = false;
							if (this.noPinchAfterDrag)
								justPinched = false;
							}
						break;
					}
				}
			


			// Officially start drag, pinch and/or twist...
			
			if (cancelDrag)
				{	
				dblTouch.touchScreen.BlockSwipe();
				dblTouch.touchOriented.BlockSwipe();
				justDragged = false;
				}

			if (justDragged)
				this.OnTwoFingerDragStart();

			if (justPinched)
				this.OnPinchStart();

			if (justTwisted)
				this.OnTwistStart();

			}
		

		}

		
	// ---------------
	private void OnPinchStart()
		{
		if (!this.pinchMoved)
			{
			this.pinchMoved = true;
			this.pinchJustMoved = true;

			if (this.startDragWhenPinching)
				this.OnTwoFingerDragStart();

			if (this.startTwistWhenPinching)
				this.OnTwistStart();
			}
		}
	
	// ---------------
	private void OnTwistStart()	
		{
		if (!this.twistMoved)
			{
			this.twistMoved 	= true;
			this.twistJustMoved = true;

			if (this.startDragWhenTwisting)
				this.OnTwoFingerDragStart();

			if (this.startPinchWhenTwisting)
				this.OnPinchStart();
			}
		}

	// --------------
	private void OnTwoFingerDragStart()
		{
		MultiFingerTouch twoFingers = this.GetMultiFingerTouch(2);
		if (twoFingers == null)
			return;

		if (!twoFingers.touchScreen.Swiped())
			{
			twoFingers.touchScreen.ForceSwipe();
			twoFingers.touchOriented.ForceSwipe();

			if (this.startTwistWhenDragging)
				this.OnTwistStart();

			if (this.startPinchWhenDragging)
				this.OnPinchStart();
			}
		}
		



	// ----------------------
	private FingerState GetFirstUnusedFinger(TouchObject newTouchObj)
		{
		if (this.strictMultiTouch && this.dontAllowNewFingers)	
			return null;

		//FingerState bestFinger = null;

		int count = Mathf.Min(this.maxFingers, this.fingers.Length);
		for (int i = 0; i < count; ++i)
			{
			FingerState finger = this.fingers[i];
		
			if (finger.touchObj == null)
				return finger;

			//if (finger.touchObj == newTouchObj) && (newTouchObj != null))
			//	return null;

			//if ((bestFinger == null) && (finger.touchObj == null))	
			//	bestFinger = finger;
			}

		return null;

		//return bestFinger;
		}
		

	// -----------------------
	private FingerState GetFingerByTouch(TouchObject touchObj)
		{
		if (touchObj == null)
			return null;

		for (int i = 0; i < this.fingers.Length; ++i)
			{
			if (touchObj == this.fingers[i].touchObj)
				return this.fingers[i];
			}

		return null;
		}

	// -------------------
	override public bool CanBeActivatedByOtherControl(TouchControl c, TouchObject touchObj)
		{
		return (base.CanBeActivatedByOtherControl(c, touchObj) && (this.GetFirstUnusedFinger(touchObj) != null));
		}

	// ----------------------
	override public bool CanBeTouchedDirectly(TouchObject touchObj)
		{
		return (base.CanBeTouchedDirectly(touchObj) && (this.GetFirstUnusedFinger(touchObj) != null));
		}
		

	


	// ----------------------------
	public override bool CanBeSwipedOverFromNothing(TouchObject touchObj)
		{
		return (base.CanBeSwipedOverFromNothingDefault(touchObj) && (this.GetFirstUnusedFinger(touchObj) != null));
		}

	// ----------------------------
	public override bool CanBeSwipedOverFromRestrictedList(TouchObject touchObj)
		{
		return (base.CanBeSwipedOverFromRestrictedListDefault(touchObj) && (this.GetFirstUnusedFinger(touchObj) != null));
		}


	// -----------------------
	public override bool CanSwipeOverOthers(TouchObject touchObj)
		{
		if (this.swipeOverOthersMode == SwipeOverOthersMode.Disabled)
			return false;

		FingerState finger = this.GetFingerByTouch(touchObj);
		return ((finger != null) && this.CanSwipeOverOthersDefault(touchObj, touchObj, finger.touchStartType));
		}


	// ------------------------			
	override public bool OnTouchStart(TouchObject touch, TouchControl sender, TouchStartType touchStartType)
		{
		if (!this.IsInitialized)
			return false;
	
		FingerState finger = this.GetFirstUnusedFinger(touch);

		if (finger == null)
			{
			return false;
			}
			

#if CF2_TOUCH_ZONE_EMULATE_FINGERS

		if (this.emulatedFingers.OnSystemTouchStart(touch, sender, touchStartType))
			{
			return true;
			}	
#endif

		if (!finger.OnTouchStart(touch, 0, touchStartType))
			return false;
				
		touch.AddControl(this);

		return true;				
		}


	// ----------------------
	override public bool OnTouchEnd(TouchObject touch, TouchEndType touchEndType)
		{
		if (!this.IsInitialized)
			return false;
			
#if CF2_TOUCH_ZONE_EMULATE_FINGERS
		if (this.emulatedFingers.OnSystemTouchEnd(touch, touchEndType))
			{
			return true;
			}	
#endif

		for (int i = 0; i < this.fingers.Length; ++i)
			{
			if (this.fingers[i].OnTouchEnd(touch, touchEndType)) 
				{
				return true;
				}
			}

		return false;
		}


	// ----------------------
	override public bool OnTouchMove(TouchObject touch) 
		{
		if (!this.IsInitialized)
			return false;

#if CF2_TOUCH_ZONE_EMULATE_FINGERS
		if (this.emulatedFingers.OnSystemTouchMove(touch))
			{
			return true;
			}	
#endif



		for (int i = 0; i < this.fingers.Length; ++i)
			{
			if (this.fingers[i].OnTouchMove(touch))
				{
				return true;
				}
			}


		return false;
		}


	// --------------
	override public bool OnTouchPressureChange(TouchObject touch) 
		{
		for (int i = 0; i < this.fingers.Length; ++i)
			{
			if (this.fingers[i].OnTouchPressureChange(touch))
				{
				return true;
				}
			}

		return false;
		}


	// ----------------------
	override public void ReleaseAllTouches()
		{
		if (!this.IsInitialized)
			return;

		for (int i = 0; i < this.fingers.Length; ++i)
			this.fingers[i].ReleaseTouch(true);
			
		for (int i = 0; i < this.multiFingerTouch.Length; ++i)
			{
			this.multiFingerTouch[i].End(true); 
			this.multiFingerTouch[i].EndDrivingTouch(true);
			}
			
#if CF2_TOUCH_ZONE_EMULATE_FINGERS
		this.emulatedFingers.EndMouseAndTouches(true); 
#endif

		this.rawFingersPressedCur = 0;
		}




	// ---------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{
		for (int i = 0; i < Mathf.Min(this.maxFingers, SuperTouchZone.MAX_FINGERS); ++i)
			{
			if (this.multiFingerConfigs[i].binding.IsBoundToAxis(axisName, rig))	
				return true;
			}

		return (
			this.twistAnalogBinding			.IsBoundToAxis(axisName, rig) ||
			this.twistDeltaBinding			.IsBoundToAxis(axisName, rig) ||
			this.twistLeftDigitalBinding	.IsBoundToAxis(axisName, rig) ||
			this.twistRightDigitalBinding	.IsBoundToAxis(axisName, rig) ||
			this.pinchAnalogBinding			.IsBoundToAxis(axisName, rig) ||
			this.pinchDeltaBinding			.IsBoundToAxis(axisName, rig) ||
			this.pinchDigitalBinding		.IsBoundToAxis(axisName, rig) ||
			this.spreadDigitalBinding		.IsBoundToAxis(axisName, rig) ||
			this.pinchScrollDeltaBinding	.IsBoundToAxis(axisName, rig) ||
			this.twistScrollDeltaBinding	.IsBoundToAxis(axisName, rig) 
			);
		}



	// ----------------------
	override protected bool OnIsBoundToKey(KeyCode key, InputRig rig)
		{
		for (int i = 0; i < Mathf.Min(this.maxFingers, SuperTouchZone.MAX_FINGERS); ++i)
			{
			if (this.multiFingerConfigs[i].binding.IsBoundToKey(key, rig))	
				return true;
			}

		return (
			this.twistAnalogBinding			.IsBoundToKey(key, rig) ||
			this.twistDeltaBinding			.IsBoundToKey(key, rig) ||
			this.twistLeftDigitalBinding	.IsBoundToKey(key, rig) ||
			this.twistRightDigitalBinding	.IsBoundToKey(key, rig) ||
			this.pinchAnalogBinding			.IsBoundToKey(key, rig) ||
			this.pinchDigitalBinding		.IsBoundToKey(key, rig) ||
			this.spreadDigitalBinding		.IsBoundToKey(key, rig) ||
			this.pinchScrollDeltaBinding	.IsBoundToKey(key, rig) ||
			this.twistScrollDeltaBinding	.IsBoundToKey(key, rig) 
			);

		}


	// ----------------------
	override public bool IsUsingKeyForEmulation(KeyCode key)
		{
		if (!this.emuWithKeys)
			return false;

		if ((key == this.emuKeyOneFinger) 	||
			(key == this.emuKeyTwoFingers) 	||
			(key == this.emuKeyThreeFingers)||
			(key == this.emuKeyPinch)		||
			(key == this.emuKeySpread) 		||
			(key == this.emuKeyTwistL)	 	||
			(key == this.emuKeyTwistR)	 	||
			(key == this.emuKeySwipeU)	 	||
			(key == this.emuKeySwipeR)	 	||
			(key == this.emuKeySwipeD)	 	||
			(key == this.emuKeySwipeL)	 	||
			(key == this.emuMouseTwoFingersKey) ||
			(key == this.emuMouseThreeFingersKey) ||
			(key == this.emuMousePinchKey) ||
			(key == this.emuMouseTwistKey) 
		 	)
			return true;

		return false;
		}

	
	// -------------------
	override protected bool OnIsEmulatingTouches()
		{
		if (this.separateFingersAsEmuTouchesBinding.IsEmulatingTouches())
			return true;

		for (int i = 0; i < Mathf.Min(this.maxFingers, SuperTouchZone.MAX_FINGERS); ++i)
			{	
			if (this.multiFingerConfigs[i].binding.IsEmulatingTouches())
				return true;
			}

		return false;
		}

	// -------------------
	override protected bool OnIsEmulatingMousePosition()
		{
		for (int i = 0; i < Mathf.Min(this.maxFingers, SuperTouchZone.MAX_FINGERS); ++i)
			{	
			if (this.multiFingerConfigs[i].binding.IsEmulatingMousePosition())
				return true;
			}

		return false;
		}

		
	// ----------------------
	override protected void OnGetSubBindingDescriptions(BindingDescriptionList descList, 
		Object undoObject, string parentMenuPath)
		{
		for (int i = 0; i < this.multiFingerConfigs.Length; ++i)
			{
			if (descList.addUnusedBindings || ((i + 1) <= this.maxFingers))
				{
				descList.Add(this.multiFingerConfigs[i].binding, 		((i + 1).ToString() + "-Finger Touch"), parentMenuPath, this);
				}
			}		
			

		descList.Add(this.separateFingersAsEmuTouchesBinding, "Separate Fingers as Emu Touches", parentMenuPath, this);
			
		if (descList.addUnusedBindings || (this.maxFingers >= 2))
			{
			descList.Add(this.pinchAnalogBinding, InputRig.InputSource.Analog, "Pinch (Analog)", parentMenuPath, this);
			descList.Add(this.pinchDeltaBinding,  InputRig.InputSource.NormalizedDelta, "Pinch (Analog)", parentMenuPath, this);

			descList.Add(this.pinchDigitalBinding, "Pinch (Digital)", parentMenuPath, this);
			descList.Add(this.spreadDigitalBinding, "Spread (Digital)", parentMenuPath, this);
				
			descList.Add(this.twistAnalogBinding, InputRig.InputSource.Analog, "Twist (Analog)", parentMenuPath, this);
			descList.Add(this.twistDeltaBinding,  InputRig.InputSource.NormalizedDelta, "Twist (Analog)", parentMenuPath, this);

			descList.Add(this.twistLeftDigitalBinding, "Twist Left (Digital)", parentMenuPath, this);
			descList.Add(this.twistRightDigitalBinding, "Twist Right (Digital)", parentMenuPath, this);


			descList.Add(this.pinchScrollDeltaBinding, "Pinch Scroll Delta", parentMenuPath, this);
			descList.Add(this.twistScrollDeltaBinding, "Twist Scroll Delta", parentMenuPath, this);
			} 
		}





	// ------------------
	// Finger State class
	// -----------------

	protected class FingerState
		{
		public SuperTouchZone 
			zone;

		public TouchObject 
			touchObj;
		public TouchControl.TouchStartType
			touchStartType;
			
		public int emuTouchId;

		public TouchGestureBasicState 
			touchScreen,
			touchOriented;

		// -----------------
		public FingerState(SuperTouchZone zone)
			{
			this.zone 			= zone;
			this.touchObj		= null;
			this.touchScreen 	= new TouchGestureBasicState(); 
			this.touchOriented= new TouchGestureBasicState(); 
			this.emuTouchId	= -1;
			} 


		// --------------------
		public bool IsActive { get { return (this.touchObj != null); } } 
			
			
		// -------------------
		public bool IsControlledByMouse()	{ return ((this.touchObj != null) && this.touchObj.IsMouse()); }
	


		// --------------------
		public void Reset()
			{
			this.touchScreen.Reset();
			this.touchOriented.Reset();
			}

		// --------------------
		public void Update()
			{
			this.touchScreen.Update();
			this.touchOriented.Update();
			}
			
		// --------------------
		public bool OnTouchStart(TouchObject touchObj, float delay, TouchStartType touchStartType)
			{	
			if (this.touchObj != null) 	
				return false;
		
			this.touchObj = touchObj;				
			this.touchStartType = touchStartType;	

			Vector2 startPos = ((touchStartType == TouchStartType.DirectPress) ? touchObj.screenPosStart : touchObj.screenPosCur);

			this.touchScreen.OnTouchStart(startPos, touchObj.screenPosCur, delay, this.touchObj);
			this.touchOriented.OnTouchStart(
				this.zone.ScreenToOrientedPos(startPos, touchObj.cam), 
				this.zone.ScreenToOrientedPos(touchObj.screenPosCur, touchObj.cam), delay, this.touchObj);

			return true;
			}

		// --------------------
		public bool OnTouchEnd(TouchObject touchObj, TouchEndType touchEndType) 
			{
			if ((this.touchObj != touchObj) || (this.touchObj == null))	
				return false;

			this.touchScreen.OnTouchEnd(touchObj.screenPosCur, (touchEndType != TouchEndType.Release)); 
			this.touchOriented.OnTouchEnd(this.zone.ScreenToOrientedPos(touchObj.screenPosCur, touchObj.cam), (touchEndType != TouchEndType.Release));

			this.touchObj = null;				

			return true;
			}
			
			

		// -----------------
		public bool OnTouchMove(TouchObject touchObj)
			{
			if ((this.touchObj != touchObj) || (this.touchObj == null)) 
				return false;

			this.touchScreen.OnTouchMove(touchObj.screenPosCur);
			this.touchOriented.OnTouchMove(this.zone.ScreenToOrientedPos(touchObj.screenPosCur, touchObj.cam));

			this.zone.CheckSwipeOff(touchObj, this.touchStartType);

			return true;
			}	


		// -----------------
		public bool OnTouchPressureChange(TouchObject touchObj)
			{
			if ((this.touchObj != touchObj) || (this.touchObj == null)) 
				return false;

			this.touchScreen.OnTouchPressureChange(touchObj.GetPressure());
			this.touchOriented.OnTouchPressureChange(touchObj.GetPressure());
	
			return true;
			}	


		// --------------
		public void ReleaseTouch(bool cancel)
			{
			if (this.touchObj != null)
				{
				this.touchObj.ReleaseControl(this.zone, (cancel ? TouchControl.TouchEndType.Cancel : TouchControl.TouchEndType.Release));
				this.touchObj = null;
				}

			this.touchScreen.OnTouchEnd(cancel);
			this.touchOriented.OnTouchEnd(cancel);
			}
		}
		
		

	// --------------------
	[System.Serializable]
	public class MultiFingerTouchConfig
		{
		public TouchGestureStateBinding 
			binding;

		public bool 
			driveOtherControl;
		public TouchControl 
			controlToDriveOnRawPress,
			controlToDriveOnNormalPress,
			controlToDriveOnLongPress,

			controlToDriveOnNormalPressSwipe,
			controlToDriveOnNormalPressSwipeU,
			controlToDriveOnNormalPressSwipeR,
			controlToDriveOnNormalPressSwipeD,
			controlToDriveOnNormalPressSwipeL,

			controlToDriveOnLongPressSwipe,
			controlToDriveOnLongPressSwipeU,
			controlToDriveOnLongPressSwipeR,
			controlToDriveOnLongPressSwipeD,
			controlToDriveOnLongPressSwipeL;

		public TouchGestureConfig
			touchConfig;
			
		public JoystickConfig
			swipeJoyConfig;

		
		// -------------------
		public MultiFingerTouchConfig()
			{
			this.binding				= new TouchGestureStateBinding();
			this.driveOtherControl	= false;
	
			this.touchConfig			= new TouchGestureConfig();
			this.swipeJoyConfig		= new JoystickConfig();
			}
		}



	// ----------------------
	protected class MultiFingerTouch 
		{
		private SuperTouchZone			
			zone;
		public TouchGestureState	
			touchScreen,
			touchOriented;
		public float				
			quietModeElapsed;
		private FingerState[]	
			fingers;		
		public bool					
			active,
			quietMode;
		private Vector2
			posStartScreen,
			posStartOriented,
			posCurScreen,
			posCurOriented;
		private bool
			controlledByMouse;

		private JoystickState
			swipeJoyState;
	 	
		private MultiFingerTouchConfig config;
		
		private TouchObject drivingTouch;


		// ---------------------
		public MultiFingerTouch(int fingerCount, SuperTouchZone zone)
			{
			this.zone				= zone;
			this.touchScreen		= new TouchGestureState();
			this.touchOriented	= new TouchGestureState();
			this.fingers			= new FingerState[fingerCount];	
			this.drivingTouch		= new TouchObject();
			this.swipeJoyState	= new JoystickState(null);

			this.Reset();
			}
			

		// -------------------
		public void Init(MultiFingerTouchConfig config)	
			{
			this.config = config;				
			
			this.touchScreen	.SetConfig((config != null) ? config.touchConfig : null);
			this.touchOriented	.SetConfig((config != null) ? config.touchConfig : null);

			this.touchScreen	.SetThresholds(this.zone.GetThresholds());
			this.touchOriented	.SetThresholds(this.zone.GetThresholds());
					
			this.swipeJoyState.config = config.swipeJoyConfig;
			}



		// -----------------
		public int				GetFingerCount()	{ return this.fingers.Length; }		
		public FingerState	GetFinger(int i)	{ return this.fingers[i]; }

		public bool				IsActive()			{ return this.active; }
		public bool				IsQuiet()			{ return this.quietMode; }

			

		// -------------------
		public void Reset()
			{
			this.touchScreen.Reset();
			this.touchOriented.Reset();
			this.swipeJoyState.Reset();

			this.active		= false;
			this.quietMode	= false;
			}


		// ------------------
		public void EndDrivingTouch(bool cancel)
			{
			this.drivingTouch.End(cancel);
			}

			

			
		// -----------------
		public void SyncToRig()
			{
			if (this.config == null)
				return;
				
			this.config.binding.SyncTouchState(this.touchScreen, this.touchOriented, this.swipeJoyState, this.zone.rig);
			}


		// -----------------
		public void Start(FingerState[] fingers, Vector2 startScreenPos, Vector2 startOrientedPos, bool quietMode, float quietAlreadyElapsed)
			{
			this.controlledByMouse = true;

			for (int i = 0; i < this.fingers.Length; ++i)
				{
				this.fingers[i] = fingers[i];

				if (!this.fingers[i].IsControlledByMouse())
					this.controlledByMouse = false;
				}			

			this.active				= true;
			this.quietMode			= quietMode;
			this.quietModeElapsed	= quietAlreadyElapsed;

			this.posStartScreen		= startScreenPos;
			this.posStartOriented	= startOrientedPos;
			this.posCurScreen		= startScreenPos;
			this.posCurOriented		= startOrientedPos;
				
			if (!quietMode)
				{
				this.touchScreen.OnTouchStart(startScreenPos, startScreenPos, 0, this.controlledByMouse, false, 1);
				this.touchOriented.OnTouchStart(startOrientedPos, startOrientedPos, 0, this.controlledByMouse, false, 1);
				}
			}

		// ---------------------
		public void End(bool cancel)
			{
			if (!this.active)
				return;

			this.active = false;
			
			this.touchScreen.OnTouchEnd(cancel || this.quietMode);			
			this.touchOriented.OnTouchEnd(cancel || this.quietMode);			
			}
			
			
		// -------------------
		public void EndAndReport()
			{
			if (!this.active)
				return;

			if (this.quietMode)	
				this.StartOfficially();

			this.active = false;			
			this.touchScreen.OnTouchEnd(false);			
			this.touchOriented.OnTouchEnd(false);			
		
			}

		// --------------------
		public void SetPos(Vector2 screenPos, Vector2 orientedPos)
			{
			this.posCurScreen = screenPos;
			this.posCurOriented = orientedPos;
			}
			

		// ------------------------
		private bool IsPotentialTapPossible()
			{
			return ((this.active && this.quietMode)); 
			}

		// ----------------
		public void CancelTap()
			{
			this.touchOriented.CancelTap();
			this.touchScreen.CancelTap();
			}	

		// -------------------
		public void DisableTapUntilRelease()
			{
			this.touchOriented.DisableTapUntilRelease();
			this.touchScreen.DisableTapUntilRelease();
			}
	
		// --------------------
		public void StartOfficially()
			{
			if (!this.active || !this.quietMode)
				return;

			this.quietMode = false;
			this.touchScreen.OnTouchStart(this.posStartScreen, this.posCurScreen, this.quietModeElapsed, this.controlledByMouse, false, 1);
			this.touchOriented.OnTouchStart(this.posStartOriented, this.posCurOriented, this.quietModeElapsed, this.controlledByMouse, false, 1);
			}

		// --------------------
		public void Update()
			{
			this.touchScreen.HoldDelayedEvents(this.IsPotentialTapPossible());
			this.touchOriented.HoldDelayedEvents(this.IsPotentialTapPossible());

			if (this.active)
				{
				for (int i = 0; i < this.fingers.Length; ++i)
					{
					if (!this.fingers[i].touchScreen.PressedRaw())
						{
						this.EndAndReport();
						break;
						}
						
					// If any of the fingers moved cancel tap and officially start this multi-touch...

					else if (this.fingers[i].touchScreen.Moved(this.zone.GetThresholds().tapMoveThreshPxSq))
						{
						this.touchScreen.MarkAsNonStatic(); 
						this.touchOriented.MarkAsNonStatic(); 
	
						if (this.quietMode)
							this.StartOfficially();
						}
					}

				if (this.active)
					{
					if (this.quietMode)
						{ 
						this.quietModeElapsed += CFUtils.realDeltaTime;
						if (this.quietModeElapsed > this.zone.strictMultiTouchTime)
							{
							this.StartOfficially();
							}
						}
					else
						{
						this.touchScreen.OnTouchMove(this.posCurScreen);
						this.touchOriented.OnTouchMove(this.posCurOriented);
						}
					}
				}

			this.touchScreen.Update();
			this.touchOriented.Update();

	

			if (this.config != null)
				{
				// Update swipe joystick...

				if (this.touchOriented.PressedRaw() && this.touchScreen.Swiped())
					this.swipeJoyState.ApplyUnclampedVec(this.touchOriented.GetSwipeVecSmooth() / this.zone.GetThresholds().swipeJoystickRadPx);

				this.swipeJoyState.Update();


				// Drive other control...
	

				if (this.drivingTouch.IsOn())
					{
					if (!this.touchScreen.PressedRaw())
						this.drivingTouch.End(false);
					else
						this.drivingTouch.MoveIfNeeded(this.touchScreen.GetCurPosRaw(), this.zone.GetCamera());
					}

				else if (this.config.driveOtherControl)
					{
					// On Raw Press...

					if (this.config.controlToDriveOnRawPress != null)
						{
						if (this.touchScreen.JustPressedRaw())
							this.StartDrivingControl(this.config.controlToDriveOnRawPress);
						} 
					else
						{


						// On Long Press...

						if (this.touchOriented.PressedLong())
							{
							// On Swipe In Direction during Long Press...
	
							if ((this.config.controlToDriveOnLongPressSwipeU != null) ||
								 (this.config.controlToDriveOnLongPressSwipeD != null) ||
								 (this.config.controlToDriveOnLongPressSwipeR != null) ||
								 (this.config.controlToDriveOnLongPressSwipeL != null))
								{
								if (this.touchOriented.GetSwipeDirState4().GetCur() != Dir.N)
									{
									switch (this.touchOriented.GetSwipeDirState4().GetCur())
										{
										case Dir.U : this.StartDrivingControl(this.config.controlToDriveOnLongPressSwipeU); break;
										case Dir.R : this.StartDrivingControl(this.config.controlToDriveOnLongPressSwipeR); break;
										case Dir.D : this.StartDrivingControl(this.config.controlToDriveOnLongPressSwipeD); break;
										case Dir.L : this.StartDrivingControl(this.config.controlToDriveOnLongPressSwipeL); break;
										}
									}
								}
	
							// On Swipe in any direction during Long Press...
	
							else if (this.config.controlToDriveOnLongPressSwipe != null)
								{
								if (this.touchOriented.Swiped())
									this.StartDrivingControl(this.config.controlToDriveOnLongPressSwipe);
								}
	
							// On Long Press...
	
							if (this.config.controlToDriveOnLongPress != null)
								{
								this.StartDrivingControl(this.config.controlToDriveOnLongPress);
								}

							}


						// On Normal Press...
	
						else if (this.touchOriented.PressedNormal())
							{

							// On Swipe In Direction during Normal Press...
	
							if ((this.config.controlToDriveOnNormalPressSwipeU != null) ||
								 (this.config.controlToDriveOnNormalPressSwipeD != null) ||
								 (this.config.controlToDriveOnNormalPressSwipeR != null) ||
								 (this.config.controlToDriveOnNormalPressSwipeL != null))
								{
								if (this.touchOriented.PressedNormal() && this.touchOriented.GetSwipeDirState4().GetCur() != Dir.N)
									{
									switch (this.touchOriented.GetSwipeDirState4().GetCur())
										{
										case Dir.U : this.StartDrivingControl(this.config.controlToDriveOnNormalPressSwipeU); break;
										case Dir.R : this.StartDrivingControl(this.config.controlToDriveOnNormalPressSwipeR); break;
										case Dir.D : this.StartDrivingControl(this.config.controlToDriveOnNormalPressSwipeD); break;
										case Dir.L : this.StartDrivingControl(this.config.controlToDriveOnNormalPressSwipeL); break;
										}
									}
								}
	
							// On Swipe in any direction...
	
							else if (this.config.controlToDriveOnNormalPressSwipe != null)
								{
								if (this.touchOriented.PressedNormal() && this.touchOriented.Swiped())
									this.StartDrivingControl(this.config.controlToDriveOnNormalPressSwipe);
								}	
	
							// On Normal Press...
	
							if (this.config.controlToDriveOnNormalPress != null)
								{
								if (this.touchScreen.PressedNormal())
									this.StartDrivingControl(this.config.controlToDriveOnNormalPress);
								}

							}	

						}
					}

				}
			}

		// --------------------
		private void StartDrivingControl(TouchControl c)
			{
			if ((c == null) || !c.IsActive() || this.drivingTouch.IsOn())
				return;
	
			this.drivingTouch.Start(this.touchScreen.GetStartPos(), this.touchScreen.GetCurPosRaw(), this.zone.GetCamera(), false, false, 1);
			if (!c.OnTouchStart(this.drivingTouch, this.zone, TouchStartType.ProxyPress))
				{
				this.drivingTouch.End(true);
				}
			}
	
		}





	// --------------------
	public void DrawMarkerGUI()
		{
#if CF2_TOUCH_ZONE_EMULATE_FINGERS
		this.emulatedFingers.DrawMarkers();
#endif
		}


		

#if CF2_TOUCH_ZONE_EMULATE_FINGERS
		
	const float
		MIN_EMU_FINGER_DIST_FACTOR = 0.1f,
		MAX_EMU_FINGER_DIST_FACTOR = 0.9f;

		
	private EmulatedFingerSystem
		emulatedFingers;

	// ----------------------
	private void InitEmulatedFingers()
		{
		this.emulatedFingers = new EmulatedFingerSystem(this);
		}


	// --------------------
	private void ReleaseEmulatedFingers(bool cancel)
		{
		this.emulatedFingers.EndTouches(cancel);
		}

	// ---------------------
	private void UpdateEmulatedFingers()
		{
		
		this.emulatedFingers.Update();
 
		}



#if UNITY_EDITOR && !CF2_DISABLE_TOUCH_MARKERS
	// ----------------------
	void OnGUI()
		{
		if (this.disableTouchMarkers)
			return;

		// Draw the markers even if there's no TouchMarkerGUI in the scene...
			
		if (TouchMarkerGUI.mInst == null)
			this.DrawMarkerGUI();

		}
#endif



	// -------------------------
	private class EmulatedFingerObject : TouchObject
		{
		public Vector2		emuPos;
		public SuperTouchZone	parentZone;

		// ---------------
		public EmulatedFingerObject(SuperTouchZone	parentZone) : base()
			{	
			this.parentZone = parentZone;
			}

		
		}
		

	// ---------------------
	private class EmulatedFingerSystem
		{
		public EmulatedFingerObject[] fingers;
		private ControlFreak2.SuperTouchZone zone;			
			
		private TouchObject mouseTouchObj;

		private Vector2 lastMousePos;
			

		private int
			curFingerNum;
		private Vector2 
			centerPos;
		private float 
			twistCur,
			pinchDistCur;



		// ---------------------	
		public EmulatedFingerSystem(ControlFreak2.SuperTouchZone zone)
			{
			this.zone		= zone;
			this.fingers	= new EmulatedFingerObject[SuperTouchZone.MAX_FINGERS];

			for (int i = 0; i < this.fingers.Length; ++i)
				{
				this.fingers[i] = new EmulatedFingerObject(zone);
				}	
			}


		public void EndMouseAndTouches(bool cancel)
			{
			if (this.mouseTouchObj != null)
				{
				this.mouseTouchObj.ReleaseControl(this.zone, (cancel ? TouchControl.TouchEndType.Cancel : TouchControl.TouchEndType.Release));
				this.mouseTouchObj = null;
				}
	
			this.EndTouches(cancel);
			}

		// ------------------
		public void EndTouches(bool cancel)
			{				
			this.curFingerNum = 0;

			for (int i = 0; i < this.fingers.Length; ++i)
				this.fingers[i].End(cancel);
			}


		// -------------------
		public bool OnSystemTouchStart(TouchObject touchObj, TouchControl sender, TouchStartType touchStartType)
			{
			if ((sender != null) && (sender != this.zone))
				return false;
			if (this.mouseTouchObj != null)
				return false;

			EmulatedFingerObject emuTouchObj = (touchObj as EmulatedFingerObject);
			if (emuTouchObj != null)
				{
				if (emuTouchObj.parentZone == this.zone)
					{
//Debug.Log("[" + Time.frameCount + "] Emulated finger OnTouchStart() LOOP detected!! (Zone: " + this.zone.name + ")"); 
					return false;
					}
				}
				
				
			this.EndTouches(false);
				
			this.mouseTouchObj = touchObj;
			this.mouseTouchObj.AddControl(this.zone);

			int fingerNum =  
				(Input.GetKey(this.zone.emuMouseTwoFingersKey) || Input.GetKey(this.zone.emuMouseTwistKey) || (Input.GetKey(this.zone.emuMousePinchKey)) ? 2 :
				Input.GetKey(this.zone.emuMouseThreeFingersKey) ? 3 : 1);

			this.lastMousePos = touchObj.screenPosCur;
				
			this.twistCur = 0;
			this.pinchDistCur = (0.25f * Mathf.Min(Screen.width, Screen.height));
				
			this.StartTouches(fingerNum, this.lastMousePos, touchObj.IsMouse());

			
			return true;
			}
			

		// -------------------
		public bool OnSystemTouchEnd(TouchObject touchObj, TouchEndType touchEndType) //bool cancel)
			{
			if ((this.mouseTouchObj == null) || (this.mouseTouchObj != touchObj))
				return false;
				
			this.mouseTouchObj = null;

			this.EndTouches((touchEndType != TouchEndType.Release)); 

			return true;
			}

			

		// -------------------
		public bool OnSystemTouchMove(TouchObject touchObj)
			{
			if ((this.mouseTouchObj == null) || (this.mouseTouchObj != touchObj))	
				return false;
				
			return true;
			}


		// ---------------------
		public void StartTouches(int num, Vector2 center, bool isMouse)
			{
			this.EndTouches(false);
				
			if (num > this.fingers.Length)
				num = this.fingers.Length;
				
			this.curFingerNum	= num;
			this.centerPos		= center;

			for (int i = 0; i < num; ++i)
				{
				EmulatedFingerObject finger = this.fingers[i];


				finger.emuPos = this.GetFingerPos(i);
				finger.Start(finger.emuPos, finger.emuPos, this.zone.GetCamera(), isMouse, false, 1);

				this.zone.OnTouchStart(finger, null, TouchStartType.DirectPress);				
				}
			}


		// -----------------
		public void Update()
			{
			float screenDim = Mathf.Min(Screen.width, Screen.height);				

			// Update mouse...
				
			if (this.mouseTouchObj != null)
				{
				Vector2 mouseDelta = (this.mouseTouchObj.screenPosCur - this.lastMousePos);
				this.lastMousePos = this.mouseTouchObj.screenPosCur;
					
				bool twistMode = Input.GetKey(this.zone.emuMouseTwistKey);
				bool pinchMode = Input.GetKey(this.zone.emuMousePinchKey);

				if (this.zone.emuMousePinchAxis == this.zone.emuMouseTwistAxis)
					pinchMode = false;

				bool swipeMode = Input.GetKey(this.zone.emuMouseTwoFingersKey) || Input.GetKey(this.zone.emuMouseThreeFingersKey) || (!twistMode && !pinchMode);

				if (twistMode)
					this.twistCur += (mouseDelta[(int)this.zone.emuMouseTwistAxis] * (90.0f / Mathf.Max(30, this.zone.mouseEmuTwistScreenFactor * screenDim)));
				if (pinchMode)
					this.pinchDistCur += mouseDelta[(int)this.zone.emuMousePinchAxis];
				if (swipeMode)
					this.centerPos += mouseDelta;
					
				
				}

			/// ...or update keyboard...

			else
				{
				Vector2 swipeSpeed = new Vector2(
					(Input.GetKey(this.zone.emuKeySwipeR) ? 1 : 0) + (Input.GetKey(this.zone.emuKeySwipeL) ? -1 : 0), 
					(Input.GetKey(this.zone.emuKeySwipeU) ? 1 : 0) + (Input.GetKey(this.zone.emuKeySwipeD) ? -1 : 0));
		
				float pinchSpeed = (Input.GetKey(this.zone.emuKeyPinch) ? -1 : 0) + (Input.GetKey(this.zone.emuKeySpread) ? 1 : 0);
				float twistSpeed = (Input.GetKey(this.zone.emuKeyTwistL) ? -1 : 0) + (Input.GetKey(this.zone.emuKeyTwistR) ? 1 : 0);


				// Start new emulated gesture...
					
				if (this.curFingerNum == 0)	
					{
					int fingerNum = 0;

					if ((swipeSpeed.sqrMagnitude > 0.0001f) || Input.GetKey(this.zone.emuKeyOneFinger))
						fingerNum = 1;
					else if ((Mathf.Abs(pinchSpeed) > 0.0001f) || (Mathf.Abs(twistSpeed) > 0.001f) || Input.GetKey(this.zone.emuKeyTwoFingers))
						fingerNum = 2;
					else if (Input.GetKey(this.zone.emuKeyThreeFingers))
						fingerNum = 3;
		
		 		
					this.StartTouches(fingerNum, this.zone.GetScreenSpaceCenter(this.zone.GetCamera()), false);
			
					}
				else
					{
					if ((swipeSpeed.sqrMagnitude < 0.0001f) && (Mathf.Abs(pinchSpeed) < 0.0001f) && (Mathf.Abs(twistSpeed) < 0.001f) && 
						((this.curFingerNum == 1) ? !Input.GetKey(this.zone.emuKeyOneFinger) :
						 (this.curFingerNum == 2) ? !Input.GetKey(this.zone.emuKeyTwoFingers) :
						 (this.curFingerNum == 3) ? !Input.GetKey(this.zone.emuKeyThreeFingers) : true))
						{
						this.EndTouches(false);		
						}
					else
						{
						this.centerPos += swipeSpeed * (screenDim * this.zone.emuKeySwipeSpeed * CFUtils.realDeltaTime);
							
						this.pinchDistCur += pinchSpeed * (screenDim * this.zone.emuKeyPinchSpeed * CFUtils.realDeltaTime); 
						this.pinchDistCur = Mathf.Clamp(this.pinchDistCur, (screenDim * MIN_EMU_FINGER_DIST_FACTOR), (screenDim * MAX_EMU_FINGER_DIST_FACTOR));
		
						this.twistCur += twistSpeed * this.zone.emuKeyTwistSpeed * CFUtils.realDeltaTime;
						}							
					}	
				}
				
 
			// Clamp pinch distance...

			this.pinchDistCur = Mathf.Clamp(this.pinchDistCur, (screenDim * MIN_EMU_FINGER_DIST_FACTOR), (screenDim * MAX_EMU_FINGER_DIST_FACTOR));



			// Move virtual touches...

			for (int i = 0; i < this.fingers.Length; ++i)
				{
				EmulatedFingerObject finger = this.fingers[i];
				finger.emuPos = this.GetFingerPos(i);

				if (finger.IsOn())
					finger.MoveIfNeeded(finger.emuPos, this.zone.GetCamera());
				}
			}
					

		// ---------------------
		public void DrawMarkers()
			{
			Texture2D 
				markerTex = null,
				pinchHintTex = null,
				twistHintTex = null;

			if (TouchMarkerGUI.mInst != null)
				{
				markerTex = TouchMarkerGUI.mInst.fingerMarker;
				pinchHintTex = TouchMarkerGUI.mInst.pinchHintMarker;
				twistHintTex = TouchMarkerGUI.mInst.twistHintMarker;
				}
#if UNITY_EDITOR
			
			if (markerTex == null)
				markerTex = CFEditorStyles.Inst.texFinger;
			if (pinchHintTex == null)
				pinchHintTex = CFEditorStyles.Inst.texPinchHint;
			if (twistHintTex == null)
				twistHintTex = CFEditorStyles.Inst.texTwistHint;
#endif
							

			if ((markerTex == null) || (pinchHintTex == null) || (twistHintTex == null))
				return;


			float markerSize = 32;
	
			Matrix4x4 initialMatrix = GUI.matrix;
			Color initialColor = GUI.color;
				
			// Draw twist/pinch hints...

			if ((this.curFingerNum > 1)) 
				{				
				bool twistMode = Input.GetKey(this.zone.emuMouseTwistKey);
				bool pinchMode = Input.GetKey(this.zone.emuMousePinchKey);
	
				if (this.zone.emuMousePinchAxis == this.zone.emuMouseTwistAxis)
					pinchMode = false;


				float twistAlpha = 0.6f;
				float pinchAlpha = 0.6f;

				
				if (twistMode && pinchMode)
					{
					float t = (Time.unscaledTime / 2.0f);
						
					twistAlpha *= Mathf.Clamp01(Mathf.Sin((t) * 2.0f * Mathf.PI));
					pinchAlpha *= Mathf.Clamp01(Mathf.Sin((t + 0.5f) * 2.0f * Mathf.PI));
					}

				float hintSize = 128; //Mathf.Clamp((this.pinchDistCur - 8), 32, 128);

				Rect hintRect = new Rect(-hintSize * 0.5f, -hintSize * 0.5f, hintSize, hintSize);
					
				Vector2 hintCenter = this.centerPos;
				hintCenter.y = Screen.height - hintCenter.y;

				if (twistMode)
					{
					GUI.color = new Color(1, 1, 1, twistAlpha);
					GUI.matrix = Matrix4x4.TRS(hintCenter, Quaternion.Euler(0,0, ((int)this.zone.emuMouseTwistAxis == 1) ? -90 : 0), Vector3.one);
					GUI.DrawTexture(hintRect, twistHintTex, ScaleMode.ScaleToFit);
					}		

				if (pinchMode)
					{
					GUI.color = new Color(1, 1, 1, pinchAlpha);
					GUI.matrix = Matrix4x4.TRS(hintCenter, Quaternion.Euler(0,0, ((int)this.zone.emuMousePinchAxis == 1) ? -90 : 0), Vector3.one);
					GUI.DrawTexture(hintRect, pinchHintTex, ScaleMode.ScaleToFit);
					}		

				}			
			


			// Draw Finger markers...


			GUI.color = new Color(1,1,1, 0.5f);

			for (int i = 0; i < this.fingers.Length; ++i)
				{		
				if (!this.fingers[i].IsOn())
					continue;

				Vector2 pos = this.fingers[i].emuPos;
				pos.y = Screen.height - pos.y;
					
				GUI.matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0,0, this.GetFingerAngle(i)), Vector3.one);

				GUI.DrawTexture(new Rect(-(markerSize * 0.5f), -markerSize, markerSize, markerSize), markerTex);  
				}
				
			GUI.color = initialColor;
			GUI.matrix = initialMatrix;
			}

	
		// ---------------------
		private Vector2 GetFingerPos(int fingerId)
			{
			if (this.curFingerNum <= 1)
				return this.centerPos;
	
			float rad = (this.pinchDistCur * 0.5f);
	
			float angle = this.GetFingerAngle(fingerId) * Mathf.Deg2Rad;
			
			return (this.centerPos + ((new Vector2(Mathf.Sin(angle), Mathf.Cos(angle))) * rad)); 	
			}

		// --------------------	
		private float GetFingerAngle(int fingerId)
			{
			if (this.curFingerNum <= 1)
				return 0;

			return ((360 * ((float)fingerId / (float)(this.curFingerNum))) + this.twistCur);
			}
			
		}

#endif



#if UNITY_EDITOR		
	[ContextMenu("Add Default Animator")]
	private void ContextMenuCreateAnimator()
		{
		ControlFreak2Editor.TouchControlWizardUtils.CreateSuperTouchZoneAnimator(this, "-Animator", 
			ControlFreak2Editor.TouchControlWizardUtils.GetDefaultSuperTouchZoneSprite(this.name), 1, "Create Super Touch Zone Animator");
		}
#endif


//! \endcond


	}
}
