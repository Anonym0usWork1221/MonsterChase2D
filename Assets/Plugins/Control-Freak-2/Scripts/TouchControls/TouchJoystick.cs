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
// ----------------------
//! Touch Joystick class.
// ---------------------
public class TouchJoystick : DynamicTouchControl
	{
//! \cond

	public JoystickConfig
		config;
	private JoystickState 
		state;


	public DigitalBinding
		pressBinding;
	public JoystickStateBinding	
		joyStateBinding;

	public bool
		emulateTouchPressure;
	public AxisBinding
		touchPressureBinding;


		
	// -----------------
	public TouchJoystick() : base()
		{
		this.joyStateBinding	= new JoystickStateBinding();
		this.pressBinding		= new DigitalBinding();
		this.emulateTouchPressure = true;
		this.touchPressureBinding = new AxisBinding();
	
		this.centerWhenFollowing = false;

		this.config				= new JoystickConfig();
		this.state				= new JoystickState(this.config);
		}

	// -----------------
	override protected void OnInitControl()
		{	
		base.OnInitControl();
			
		this.ResetControl();
		}

//! \endcond

	// ----------------
	//public bool Pressed()			{ return this.touchStateWorld.PressedRaw(); }
	//public bool JustPressed()		{ return this.touchStateWorld.JustPressedRaw(); }
	//public bool JustReleased()		{ return this.touchStateWorld.JustReleasedRaw(); }
		
	// ----------------
	public Vector2	GetVector()			{ return this.state.GetVector(); }		
	public Vector2	GetVectorRaw()		{ return this.state.GetVectorRaw();	}
	public Dir		GetDir()				{ return this.state.GetDir(); }		
	public JoystickState GetState()	{		return this.state;	}				

//! \cond


	// ------------------
	override public void ResetControl()
		{
		base.ResetControl();

		this.ReleaseAllTouches(); //true);
			
		this.touchStateWorld.Reset();
		this.touchStateScreen.Reset();
		this.touchStateOriented.Reset();

		this.state.Reset();
		
		}


		
	// ---------------------
	override protected void OnUpdateControl()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return;
#endif


	
		base.OnUpdateControl();
		


		if (this.touchStateWorld.PressedRaw()) 
			{
			this.state.ApplyUnclampedVec(WorldToNormalizedPos(this.touchStateWorld.GetCurPosSmooth(), this.GetOriginOffset())); 
			}

			
		// Process stick's state...

		this.state.Update();
			
		if (this.IsActive())
			this.SyncRigState();
		
		}


	// ---------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{
		return (
			this.pressBinding				.IsBoundToAxis(axisName, rig) ||
			this.touchPressureBinding	.IsBoundToAxis(axisName, rig) ||
			this.joyStateBinding			.IsBoundToAxis(axisName, rig) );
		}



	// ----------------------
	override protected bool OnIsBoundToKey(KeyCode key, InputRig rig)
		{
		return (
			this.pressBinding				.IsBoundToKey(key, rig) ||
			this.touchPressureBinding	.IsBoundToKey(key, rig) ||
			this.joyStateBinding			.IsBoundToKey(key, rig) );
		}
		

	// ----------------------
	override protected void OnGetSubBindingDescriptions(BindingDescriptionList descList, 
		Object undoObject, string parentMenuPath)
		{
		descList.Add(this.pressBinding, "Press", parentMenuPath, this); 
		descList.Add(this.touchPressureBinding, InputRig.InputSource.Analog, "Touch Pressure", parentMenuPath, this);
		descList.Add(this.joyStateBinding, "Joy State", parentMenuPath, this);
 
		}




	// ----------------------
	override public bool IsUsingKeyForEmulation(KeyCode key)
		{

		return false;
		}
		



	// ------------------------
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

	
		this.joyStateBinding.SyncJoyState(this.state, this.rig);
		}




#if UNITY_EDITOR

	// --------------------
	override protected void DrawCustomGizmos(bool selected)
		{
		if (this.state == null) 
			return;

		Matrix4x4 initialMatrix = Gizmos.matrix;
			

		Rect r = this.GetLocalRect();

		Color analogDeadZoneColor = Color.yellow * 0.33f;
		Color analogEndZoneColor = Color.yellow * 0.66f;
		Color analogEdgeColor = Color.yellow ;
		Color digitalDeadZoneColor = Color.green * 0.5f;
			


		// Draw shape and full rect...

		Gizmos.color = (selected ? Color.red : Color.white);
		this.DrawDefaultGizmo(true);
			


		Gizmos.matrix = this.transform.localToWorldMatrix * Matrix4x4.TRS(r.center, Quaternion.identity, new Vector3(r.width, r.height, 0.00001f));
			
			
	

		// Draw Analog dead zone...
	
		if (this.config.analogDeadZone > 0)
			{
			Gizmos.color = analogDeadZoneColor;

			if (this.config.clampMode == JoystickConfig.ClampMode.Circle) //.circularClamp)
				CFGizmos.DrawCircle(Vector3.zero, this.config.analogDeadZone, false);
			else 
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one * this.config.analogDeadZone);
			}

		if (this.config.analogEndZone < 1)
			{
			Gizmos.color = analogEndZoneColor;

			if (this.config.clampMode == JoystickConfig.ClampMode.Circle) //.circularClamp)
				CFGizmos.DrawCircle(Vector3.zero, this.config.analogEndZone, false);
			else 
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one * this.config.analogEndZone);
			}

		if (this.config.digitalEnterThresh > 0)
			{
			Gizmos.color = digitalDeadZoneColor;

			if (this.config.clampMode == JoystickConfig.ClampMode.Circle) //.circularClamp)
				CFGizmos.DrawCircle(Vector3.zero, this.config.digitalEnterThresh, false);
			else 
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one * this.config.digitalEnterThresh);
			}

		if ((this.config.clampMode == JoystickConfig.ClampMode.Circle) != ((this.shape == TouchControl.Shape.Circle) || (this.shape == TouchControl.Shape.Ellipse)))
			{
			Gizmos.color = analogEdgeColor;

			if (this.config.clampMode == JoystickConfig.ClampMode.Circle) //.circularClamp)
				CFGizmos.DrawCircle(Vector3.zero, Vector3.one, false);
			else 
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}




		Gizmos.matrix = initialMatrix;
		}

#endif


#if UNITY_EDITOR		
	[ContextMenu("Add Default Animator")]
	private void ContextMenuCreateAnimator()
		{
		ControlFreak2Editor.TouchControlWizardUtils.CreateTouchJoystickSimpleAnimator(this, "-Animator", 
			ControlFreak2Editor.TouchControlWizardUtils.GetDefaultAnalogJoyHatSprite(this.name), 1, 0.5f, "Create Touch Joystick Animator");
		}
#endif

//! \endcond

	}
}
