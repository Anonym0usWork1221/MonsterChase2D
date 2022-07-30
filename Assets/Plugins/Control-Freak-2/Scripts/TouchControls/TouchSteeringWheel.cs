// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

using ControlFreak2.Internal;


namespace ControlFreak2
{
// ------------------
//! Touch Steering Wheel Class.
// -------------------
public class TouchSteeringWheel : DynamicTouchControl
	{
	
	// -------------------	
	public enum WheelMode
		{
		Swipe,	//!< Swipe horizontally to turn
		Turn		//!< Turn like a real steering wheel
		}
	
	public WheelMode
		wheelMode;			//!< Wheel mode.

	public bool 
		limitTurnSpeed = false;		//!< Limit wheel turn speed?
	public float 
		minTurnTime = 0.05f;			//!< Quickest time possible in seconds to turn a wheel from neutral to max. angle.
	public float 
		maxReturnTime = 0.25f;		//!< Time needed to return from max. turn angle to neutral.

	public float 
		maxTurnAngle = 60;			//!< Max turn angle in degrees (in Turn wheel mode)

	public float
		turnModeDeadZone = 0.05f;	//!< Dead-zone for the 'Turn' wheel mode.  

	public bool 
		physicalMode = false;		//!< Physical Swipe mode. (used in 'Swipe' wheel mode). 
	public float 
		physicalMoveRangeCm = 2.0f;	//!< Physical swipe range in centimeters.

	public bool		
		sendInputWhileReturning = false;

//! \cond

	public AnalogConfig
		analogConfig;
		
	public DigitalBinding
		pressBinding;
		
	public AxisBinding			
		analogTurnBinding;
	
	public DigitalBinding
		turnRightBinding,
		turnLeftBinding;

	public bool
		emulateTouchPressure;
	public AxisBinding
		touchPressureBinding;


		

	private Vector2 
		pressOrigin;


	private float
		
		rawValCur,

		valCur,
		valPrev;

	private Vector2 
		startVec;
	private float
		startRawVal;

	private float 
		startAngle,
		curAngle,
		angleDelta;
	private bool
		angleIsSafe;



//! \endcond

		
	// -------------------
	public TouchSteeringWheel() : base()
		{
		this.analogConfig					= new AnalogConfig();
		this.analogConfig.analogDeadZone	= 0.0f;

		this.touchSmoothing		 =	 0.1f;
		this.centerOnDirectTouch = false;
		this.centerWhenFollowing = false;

		this.wheelMode = WheelMode.Swipe;

		this.pressBinding = new DigitalBinding();
		this.analogTurnBinding = new AxisBinding("Horizontal", false);
		this.turnLeftBinding	= new DigitalBinding(KeyCode.None, true, "Horizontal", true, false);
		this.turnRightBinding	= new DigitalBinding(KeyCode.None, true, "Horizontal", false, false);
		this.emulateTouchPressure = true;
		this.touchPressureBinding = new AxisBinding();
		}
		


	
	// --------------
	//! Get wheel's analog value.
	// --------------
	public float GetValue()			{ return this.valCur; }
		
	// --------------
	//! Get wheel's analog value delta.
	// --------------
	public float GetValueDelta()	{ return (this.valCur - this.valPrev); }


	// -------------------
	//public bool Pressed()			{ return this.touchStateWorld.PressedRaw(); } 
		
	

//! \cond

	// -------------------
	override protected void OnInitControl ()
		{
		base.OnInitControl();

		this.ResetControl();
		}

	// ------------------
	override public void ResetControl()
		{
		base.ResetControl();

		this.ReleaseAllTouches(); //rue);
			
		this.touchStateWorld.Reset();
		this.touchStateScreen.Reset();
		this.touchStateOriented.Reset();

		this.valCur = 0;
		this.valPrev = 0;
		this.rawValCur = 0;
		//this.rawValPrev = 0;
		this.startRawVal = 0;
		this.startVec = Vector2.zero;

		this.angleIsSafe = false;
		this.startAngle = 0;
		this.curAngle = 0;
		this.angleDelta = 0;
		
		}
		

	// ---------------------
	override protected void OnUpdateControl()
		{
		base.OnUpdateControl();

		
		this.valPrev = this.valCur;
			
			

		// Touch started...

		if (this.touchStateWorld.JustPressedRaw())
			{
			this.startRawVal = this.rawValCur;

			// Swipe mode...

			if (this.wheelMode == WheelMode.Swipe)
				{
				if (this.physicalMode)
					{
					this.startVec = this.touchStateOriented.GetCurPosSmooth();
					}
				else
					{
					this.startVec = this.WorldToNormalizedPos(this.touchStateWorld.GetStartPos(), this.GetOriginOffset());
					}
				}
	
			// Real mode...
			else
				{
				this.startVec = this.WorldToNormalizedPos(this.touchStateWorld.GetStartPos(), this.GetOriginOffset());

				this.angleIsSafe	= false;
				this.curAngle		= 0;
				this.startAngle	= 0;
				this.angleDelta	= 0;
//(this.startVec.sqrMagnitude > (this.realModeDeadZone * this.realModeDeadZone));
//				this.startAngle	= this.angleIsSafe ? this.GetWheelAngle(this.startVec, 0) : 0;
//				this.curAngle		= this.startAngle;
				}
			}
			
			
		if (this.touchStateWorld.PressedRaw())
			{
			float v = 0;

			// Swipe mode...

			if (this.wheelMode == WheelMode.Swipe)
				{
				if (this.physicalMode)
					{	
					v = (this.touchStateOriented.GetCurPosSmooth().x - this.startVec.x) / (this.physicalMoveRangeCm * CFScreen.dpcm * 0.5f);
					}
				else
					{
					Vector3 np = this.WorldToNormalizedPos(this.touchStateWorld.GetCurPosSmooth(), this.GetOriginOffset());
					v = (np.x - this.startVec.x);
					}
				}

			// Real mode...

			else
				{
				Vector3 np = this.WorldToNormalizedPos(this.touchStateWorld.GetCurPosSmooth(), this.GetOriginOffset());
				if (np.sqrMagnitude < (this.turnModeDeadZone * this.turnModeDeadZone))
					{
					this.angleIsSafe = false;
					}
				else 
					{
					this.curAngle = this.GetWheelAngle(np, this.curAngle);

					if (!this.angleIsSafe)
						{	
						this.startAngle = this.curAngle;
						this.startRawVal = this.rawValCur;
						this.angleIsSafe = true;
						}						
					}		

				this.angleDelta = CFUtils.SmartDeltaAngle(this.startAngle, this.curAngle, this.angleDelta);

				//if (!this.angleIsSafe)
				//	this.angleDelta = Mathf.Clamp(this.angleDelta, -this.maxTurnAngle, this.maxTurnAngle);
				
				this.angleDelta = Mathf.Clamp(this.angleDelta, -this.maxTurnAngle - 360, this.maxTurnAngle + 360);

				v =  this.angleDelta / this.maxTurnAngle;		
				}

			float targetRawVal = this.startRawVal + v; // * this.maxTurnAngle;

			this.rawValCur = CFUtils.MoveTowards(this.rawValCur, targetRawVal, (this.limitTurnSpeed ? this.minTurnTime : 0), CFUtils.realDeltaTime, 0.001f);

			}	

		else
			{
			this.rawValCur = CFUtils.MoveTowards(this.rawValCur, 0, (this.limitTurnSpeed ? this.maxReturnTime : 0), CFUtils.realDeltaTime, 0.001f);

			}

		this.rawValCur = Mathf.Clamp(this.rawValCur, -1, 1);
			
		this.valCur = this.analogConfig.GetAnalogVal(this.rawValCur);

			
		if (this.IsActive())
			this.SyncRigState();



		}


	// ------------------
	private void SyncRigState()
		{
		if (this.Pressed() || this.sendInputWhileReturning)
			this.analogTurnBinding.SyncFloat(this.GetValue(), InputRig.InputSource.Analog, this.rig);

		if (this.Pressed())
			{
			this.pressBinding.Sync(this.Pressed(), this.rig);			
				
			if (this.GetValue() <= -this.analogConfig.digitalEnterThresh)
				this.turnLeftBinding.Sync(true, this.rig);

			else if (this.GetValue() >= this.analogConfig.digitalEnterThresh)
				this.turnRightBinding.Sync(true, this.rig);


			if (this.IsTouchPressureSensitive())
				this.touchPressureBinding.SyncFloat(this.GetTouchPressure(), InputRig.InputSource.Analog, this.rig);

			else if (this.emulateTouchPressure)	
				this.touchPressureBinding.SyncFloat(1, InputRig.InputSource.Digital, this.rig);			

			}
		}


//void OnGUI()
//	{
//	GUILayout.Box("Angle : " + this.curAngle + "\nstart:"  + this.startAngle + "\nSafe:" + this.angleIsSafe + 
//		"\nDelta: " + this.angleDelta + "\nval: " + this.valCur);
//	}


	// -----------------
	private float GetWheelAngle(Vector2 np, float fallbackAngle)
		{
		if (np.sqrMagnitude < (this.turnModeDeadZone * this.turnModeDeadZone))
			return fallbackAngle;

		np.Normalize();

		float angle = Mathf.Atan2(np.x, np.y) * Mathf.Rad2Deg;

		//if (Mathf.Abs(angle - fallbackAngle) > 180.0f)
		//	angle = Mathf.Sign(angle) * this.maxTurnAngle;
		
		return angle;
		}



#if UNITY_EDITOR

	// --------------------
	override protected void DrawCustomGizmos(bool selected)
		{
		Matrix4x4 initialMatrix = Gizmos.matrix;

		Rect r = this.GetLocalRect();



		// Draw shape and full rect...

		Gizmos.color = (selected ? Color.red : Color.white);
		this.DrawDefaultGizmo(true);
			

		if (this.wheelMode == WheelMode.Turn)
			{
			Gizmos.matrix = this.transform.localToWorldMatrix * Matrix4x4.TRS(r.center, Quaternion.identity, new Vector3(r.width, r.height, 0.00001f));
			
			Gizmos.color = Color.yellow;

			CFGizmos.DrawCircle(Vector3.zero, this.turnModeDeadZone, false);

			Gizmos.matrix = initialMatrix;
			}		

		}

#endif


	// ---------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{ 
		return (
			this.analogTurnBinding.IsBoundToAxis(axisName, rig) ||
			this.pressBinding.IsBoundToAxis(axisName, rig) ||
			this.touchPressureBinding.IsBoundToAxis(axisName, rig) ||
			this.turnLeftBinding.IsBoundToAxis(axisName, rig) ||
			this.turnRightBinding.IsBoundToAxis(axisName, rig) );
		}

	// ----------------------
	override protected bool OnIsBoundToKey(KeyCode key, InputRig rig)
		{ 
		return (
			this.analogTurnBinding.IsBoundToKey(key, rig) ||
			this.pressBinding.IsBoundToKey(key, rig) ||
			this.touchPressureBinding.IsBoundToKey(key, rig) ||
			this.turnLeftBinding.IsBoundToKey(key, rig) ||
			this.turnRightBinding.IsBoundToKey(key, rig));
		}


	// ----------------------
	override protected void OnGetSubBindingDescriptions(BindingDescriptionList descList, //BindingDescription.BindingType typeMask, 
		Object undoObject, string parentMenuPath) //, bool addUnusedBindings, int axisSourceTypeMask)
		{
		descList.Add(this.pressBinding, "Press", parentMenuPath, this);
		descList.Add(this.touchPressureBinding, InputRig.InputSource.Analog, "Touch Pressure", parentMenuPath, this);

		descList.Add(this.analogTurnBinding, InputRig.InputSource.Analog, "Analog Turn", parentMenuPath, this);
		descList.Add(this.turnLeftBinding, "Turn Left", parentMenuPath, this);
		descList.Add(this.turnRightBinding, "Turn Right", parentMenuPath, this);
 
		}

#if UNITY_EDITOR		
	[ContextMenu("Add Default Animator")]
	private void ContextMenuCreateAnimator()
		{
		ControlFreak2Editor.TouchControlWizardUtils.CreateWheelAnimator(this, "-Animator", 
				ControlFreak2Editor.TouchControlWizardUtils.GetDefaultWheelSprite(this.name), 1, "Create Touch Steering Wheel Animator");
		}
#endif

		

//! \endcond

	}
}
