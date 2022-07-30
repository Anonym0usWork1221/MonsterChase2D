// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------


#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 
	#define UNITY_PRE_5_0
#endif

#if UNITY_PRE_5_0 || UNITY_5_0 
	#define UNITY_PRE_5_1
#endif

#if UNITY_PRE_5_1 || UNITY_5_1 
	#define UNITY_PRE_5_2
#endif

#if UNITY_PRE_5_2 || UNITY_5_2 
	#define UNITY_PRE_5_3
#endif

#if UNITY_PRE_5_3 || UNITY_5_3 
	#define UNITY_PRE_5_4
#endif


using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using UnityEngine.EventSystems;

using ControlFreak2.Internal;


#if UNITY_EDITOR
using ControlFreak2Editor;
#endif

namespace ControlFreak2
{
// --------------------
//! Input Rig Class.
// --------------------
public class InputRig : ComponentBase, IBindingContainer
	{
	public event System.Action
		onSwitchesChanged;		//!< Called every time after this rig's switches changed.

	public event System.Action
		onAddExtraInput;			//!< Can be used to add input data from external sources to this rig.

	static public event System.Action
		onAddExtraInputToActiveRig;	//!< Can be used to add input data from external sources to currently active rig.


//! \cond
	public const string
		CF_EMPTY_AXIS 			= "cfEmpty",

		CF_SCROLL_WHEEL_X_AXIS 	= "cfScroll0",
		CF_SCROLL_WHEEL_Y_AXIS 	= "cfScroll1",
		CF_MOUSE_DELTA_X_AXIS 	= "cfMouseX",
		CF_MOUSE_DELTA_Y_AXIS 	= "cfMouseY",

		DEFAULT_LEFT_STICK_NAME		= "LeftStick",
		DEFAULT_RIGHT_STICK_NAME	= "RightStick",

		DEFAULT_VERT_SCROLL_WHEEL_NAME = "Mouse ScrollWheel",
		DEFAULT_HORZ_SCROLL_WHEEL_NAME = "Mouse ScrollWheel Secondary";


	public bool		autoActivate			= true;
	public bool		overrideActiveRig		= true;
		
	public bool		hideWhenDisactivated = true;
	public bool		disableWhenDisactivated = false;

	public bool		hideWhenTouchScreenIsUnused = true; 
	public float	hideWhenTouchScreenIsUnusedDelay;		// Time of touch inactivity after which touch controls will become hidden
	
	public bool		hideWhenGamepadIsActivated = true; 
	public float	hideWhenGamepadIsActivatedDelay;		// Time of touch inactivity after which touch controls will become hidden
	

	public float	
		fingerRadiusInCm = 0.25f;
	public bool
		swipeOverFromNothing = false;

		
	public float
		controlBaseAlphaAnimDuration	= 0.5f,
		animatorMaxAnimDuration			= 0.2f;
	
	
		
	
	public float
		ninetyDegTurnMouseDelta	= 500;		///< Reference unscaled mouse delta that would turn the camera by 90 degrees in a typical FPP game.	
	public float
		ninetyDegTurnTouchSwipeInCm = 4.0f;	///< Swipe distance in centimeters that would result in 90 degree turn in a typical FPP game.
	public float
		ninetyDegTurnAnalogDuration	= 0.75f;	///< Amount of time that would take to turn the camera by 90 degrees with fully tilted analog stick. 
		
	public float
		virtualScreenDiameterInches = 4.0f;	///< Virtual screen diameter in Inches that will be used to calculate fake DPI when testing touch input in editor/webplayer.
		

	public int
		scrollStepsPerNinetyDegTurn = 10;



	public const float 
		TOUCH_SMOOTHING_MAX_TIME = 0.1f;


	[System.NonSerializedAttribute]
	private float	
		elapsedSinceLastTouch;
	[System.NonSerializedAttribute]
	private bool
		touchControlsSleeping;


	public VirtualJoystickConfigCollection		
		joysticks;

	public AxisConfigCollection
		axes;

	public List<KeyCode>
		keyboardBlockedCodes;

	[System.NonSerializedAttribute]
	private BitArray
		keysPrev,
		keysCur,
		keysNext,
		keysMuted,
		keysBlocked;


	[System.NonSerializedAttribute]
	private bool
		keysNextSomeDown,
		keysNextSomeOn,

		keysCurSomeOn,
		keysCurSomeDown;


	public JoystickConfig
		dpadConfig,
		leftStickConfig,
		rightStickConfig;

	public AnalogConfig
		leftTriggerAnalogConfig,
		rightTriggerAnalogConfig;


	public GamepadConfig[]				
		gamepads;
	public GamepadConfig
		anyGamepad;


	public RigSwitchCollection
		rigSwitches;
		
	public AutomaticInputConfigCollection
		autoInputList;


	[System.NonSerializedAttribute]
	private List<TouchControl> 			
		touchControls;
		
	public TiltConfig		
		tilt;
	public MouseConfig
		mouseConfig; 
	public ScrollWheelState
		scrollWheel;
		
			
	private FixedTimeStepController
		fixedTimeStep;
	
	private const float 	
		MAX_DELTA_TIME = 0.2f,
		DELTA_TIME_SMOOTHING_FACTOR = 0.1f,
		DELTA_TIME_SMOOTHING_TIME		= 1.0f;
		
	private const int
		FIXED_FPS		= 120;




	// --------------------
	public void OnActivateRig()
		{
		if (this.hideWhenDisactivated)
			this.ShowOrHideTouchControls(true, false);

		}
	
	// ------------------
	public void OnDisactivateRig()
		{
		if (this.disableWhenDisactivated)
			{
			this.gameObject.SetActive(false);
			}

		else if (this.hideWhenDisactivated)
			this.ShowOrHideTouchControls(false, false);

		}



	

		

	// ---------------------
	public InputRig() : base()
		{
		this.BasicConstructor();
		}

	// ------------------------
	private void BasicConstructor()
		{		
	
		this.fixedTimeStep	= new FixedTimeStepController(FIXED_FPS);


		this.ninetyDegTurnMouseDelta		= 500;
		this.ninetyDegTurnTouchSwipeInCm	= 4.0f;
		this.ninetyDegTurnAnalogDuration 	= 0.75f;
		this.virtualScreenDiameterInches 	= 4.0f;

		this.hideWhenDisactivated 				= true;
		this.hideWhenTouchScreenIsUnused		= true;
		this.hideWhenTouchScreenIsUnusedDelay	= 10;
		this.hideWhenGamepadIsActivated 		= true; 
		this.hideWhenGamepadIsActivatedDelay	= 5;



		this.rigSwitches = new RigSwitchCollection(this);
			

		// Allocate emu touches....
	
		this.InitEmuTouches();


		// Allocate touch control list...

		this.touchControls = new List<TouchControl>(32);


		// Allocate keycode bitsets...

		int keyCodeMaxVal = CFUtils.GetEnumMaxValue(typeof(KeyCode)) + 1;


		this.keysCur		= new BitArray(keyCodeMaxVal);
		this.keysPrev		= new BitArray(keyCodeMaxVal);
		this.keysNext		= new BitArray(keyCodeMaxVal);
		this.keysBlocked	= new BitArray(keyCodeMaxVal);
		this.keysMuted		= new BitArray(keyCodeMaxVal);


		// Reset joysticks...
		
		this.joysticks = new VirtualJoystickConfigCollection(this, 1);
			

		// Reset Analog Axes... 

		this.axes = new AxisConfigCollection(this, 16);

		this.axes.list.Clear();
		this.axes.list.Add(AxisConfig.CreateSignedAnalog("Horizontal", KeyCode.D, KeyCode.A));
		this.axes.list.Add(AxisConfig.CreateSignedAnalog("Vertical", KeyCode.W, KeyCode.S));
		this.axes.list.Add(AxisConfig.CreateDelta("Mouse X", KeyCode.None, KeyCode.None));
		this.axes.list.Add(AxisConfig.CreateDelta("Mouse Y", KeyCode.None, KeyCode.None));
		this.axes.list.Add(AxisConfig.CreateScrollWheel(DEFAULT_VERT_SCROLL_WHEEL_NAME, KeyCode.None, KeyCode.None));
		this.axes.list.Add(AxisConfig.CreateScrollWheel(DEFAULT_HORZ_SCROLL_WHEEL_NAME, KeyCode.None, KeyCode.None));
		this.axes.list.Add(AxisConfig.CreateDigital("Fire1", KeyCode.LeftShift));
		this.axes.list.Add(AxisConfig.CreateDigital("Fire2", KeyCode.LeftControl));
		this.axes.list.Add(AxisConfig.CreateDigital("Jump", KeyCode.Space));

		
		// Init keyboard blocked list...

		this.keyboardBlockedCodes = new List<KeyCode>(16);

			
		// Init gamepad configs...

		this.dpadConfig 						= new JoystickConfig();
		this.dpadConfig.stickMode			= JoystickConfig.StickMode.Digital8;
		this.dpadConfig.analogDeadZone	= 0.3f;
		this.dpadConfig.angularMagnet		= 0;
		this.dpadConfig.digitalDetectionMode = JoystickConfig.DigitalDetectionMode.Joystick;

		this.leftStickConfig						= new JoystickConfig();
		this.leftStickConfig.stickMode		= JoystickConfig.StickMode.Analog;
		this.leftStickConfig.analogDeadZone	= 0.3f;
		this.leftStickConfig.analogEndZone	= 0.7f;
		this.leftStickConfig.angularMagnet	= 0.5f;
		this.leftStickConfig.digitalDetectionMode = JoystickConfig.DigitalDetectionMode.Joystick;

		this.rightStickConfig					= new JoystickConfig();
		this.rightStickConfig.stickMode		= JoystickConfig.StickMode.Analog;
		this.rightStickConfig.analogDeadZone= 0.3f;
		this.rightStickConfig.analogEndZone	= 0.7f;
		this.rightStickConfig.angularMagnet	= 0.5f;
		this.rightStickConfig.digitalDetectionMode = JoystickConfig.DigitalDetectionMode.Joystick;

		this.leftTriggerAnalogConfig				= new AnalogConfig();
		this.leftTriggerAnalogConfig.analogDeadZone	= 0.2f;

		this.rightTriggerAnalogConfig 				= new AnalogConfig();
		this.rightTriggerAnalogConfig.analogDeadZone= 0.2f;


		this.anyGamepad = new GamepadConfig(this);

		this.gamepads = new GamepadConfig[GamepadManager.MAX_JOYSTICKS];

		for (int i = 0; i < this.gamepads.Length; ++i)
			this.gamepads[i] = new GamepadConfig(this);


		// Setup initial gamepad mappings...
		
		this.anyGamepad.enabled = true;


		this.anyGamepad.leftStickStateBinding.horzAxisBinding.AddTarget().SetSingleAxis("Horizontal", false);
		this.anyGamepad.leftStickStateBinding.vertAxisBinding.AddTarget().SetSingleAxis("Vertical", false);
		this.anyGamepad.leftStickStateBinding.enabled = true;
		this.anyGamepad.leftStickStateBinding.horzAxisBinding.enabled = true;
		this.anyGamepad.leftStickStateBinding.vertAxisBinding.enabled = true;

		this.anyGamepad.rightStickStateBinding.horzAxisBinding.AddTarget().SetSingleAxis("Mouse X", false);
		this.anyGamepad.rightStickStateBinding.vertAxisBinding.AddTarget().SetSingleAxis("Mouse Y", false);
		this.anyGamepad.rightStickStateBinding.enabled = true;
		this.anyGamepad.rightStickStateBinding.horzAxisBinding.enabled = true;
		this.anyGamepad.rightStickStateBinding.vertAxisBinding.enabled = true;

		this.anyGamepad.dpadStateBinding.horzAxisBinding.AddTarget().SetSingleAxis("Horizontal", false);
		this.anyGamepad.dpadStateBinding.vertAxisBinding.AddTarget().SetSingleAxis("Vertical", false);
		this.anyGamepad.dpadStateBinding.enabled = true;
		this.anyGamepad.dpadStateBinding.horzAxisBinding.enabled = true;
		this.anyGamepad.dpadStateBinding.vertAxisBinding.enabled = true;

		this.anyGamepad.digiFaceDownBinding.enabled = true;
		this.anyGamepad.digiFaceDownBinding.AddAxis().SetAxis("Fire1", true);

		this.anyGamepad.digiFaceRightBinding.enabled = true;
		this.anyGamepad.digiFaceRightBinding.AddAxis().SetAxis("Jump", true);

		this.anyGamepad.digiFaceLeftBinding.enabled = true;
		this.anyGamepad.digiFaceLeftBinding.AddAxis().SetAxis("Fire2", true);

		this.anyGamepad.digiFaceUpBinding.enabled = true;
		this.anyGamepad.digiFaceUpBinding.AddAxis().SetAxis("Fire3", true);



		this.anyGamepad.digiR1Binding.enabled = true;
		this.anyGamepad.digiR1Binding.AddAxis().SetAxis("Fire1", true);

		this.anyGamepad.digiR2Binding.enabled = true;
		this.anyGamepad.digiR2Binding.AddAxis().SetAxis("Fire1", true);

		this.anyGamepad.digiL1Binding.enabled = true;
		this.anyGamepad.digiL1Binding.AddAxis().SetAxis("Fire2", true);

		this.anyGamepad.digiL2Binding.enabled = true;
		this.anyGamepad.digiL2Binding.AddAxis().SetAxis("Fire2", true);



		// Init tilt...

		this.tilt = new TiltConfig(this);

		// Init Mouse config...

		this.mouseConfig = new MouseConfig(this);
		this.scrollWheel = new ScrollWheelState(this);

		this.autoInputList = new AutomaticInputConfigCollection(this);

		}





#if UNITY_EDITOR
	// --------------------
	[ContextMenu("Transfer axes from Unity Input Manager")]
	public void TransferAxesFromUnityInputManager()
		{
		UnityInputManagerToRigDialog.ShowDialog(this);
		}


	// --------------------
	[ContextMenu("Select Rig's Main Panel")]
	private void SelectMainPanel()
		{
		TouchControlPanel panel = ControlFreak2Editor.TouchControlWizardUtils.GetRigPanel(this);
		if (panel == null)
			UnityEditor.EditorUtility.DisplayDialog("Control Freak 2", "This rig doesn't have a Touch Control Panel!", "OK");
		else
			UnityEditor.Selection.activeTransform = panel.transform;
		}
#endif		


	// ---------------------
	override protected void OnInitComponent()
		{	

		this.autoInputList.SetRig(this);



		this.ResetState();

		this.ResetAllSwitches(true);

		this.InvalidateBlockedKeys();

		}		



	// ----------------------
	override protected void OnDestroyComponent()
		{
		if (this.touchControls != null)
			{
			int count = this.touchControls.Count;
			for (int i = 0; i < count; ++i)
				{
				TouchControl c = this.touchControls[i];
				if (c != null) 
					c.SetRig(null);
				}
			}
		}


	// --------------------
	override protected void OnEnableComponent()
		{
		if (this.autoActivate && ((CF2Input.activeRig == null) || this.overrideActiveRig))
			{
#if UNITY_EDITOR
			if ((CF2Input.activeRig != null) && (CF2Input.activeRig != this))
				{
				Debug.LogWarning("Active rig detected [" + CF2Input.activeRig.name + "], when trying to auto-activate [" + this.name + "]!! There should be only one active rig in the scene!"); 
				}
#endif
			CF2Input.activeRig = this;	
			}


		// Fix Event System input modules...	
		
		EventSystem eventSys = EventSystem.current;
		if (eventSys == null)
			eventSys = GameObject.FindObjectOfType<EventSystem>();

			if (eventSys != null)
				{
				//TouchInputModule touchInputModule = eventSys.GetComponent<TouchInputModule>();
				MonoBehaviour touchInputModule = eventSys.GetComponent("TouchInputModule") as MonoBehaviour;

#if UNITY_PRE_5_3

				// Make sure that TouchInputModule is there...

				if (touchInputModule == null)
					{
#if UNITY_EDITOR
					Debug.Log("Added TouchInputModule to [" + eventSys.name + "] Event System object! It's required in Unity 5.2 and eariler!"); 
#endif
					touchInputModule = eventSys.gameObject.AddComponent<TouchInputModule>();
					}
				else
					{
					touchInputModule.enabled = true;
					}

#else

				// Disable TouchInputModule if it's there...

				if ((touchInputModule != null) && (touchInputModule.enabled))
					{
#if UNITY_EDITOR
					Debug.Log("Disabling TouchInputModule on [" + eventSys.name + "] Event System object! It's no longer needed as of Unity 5.3."); 
#endif
					touchInputModule.enabled = false;
					}

#endif	
			}



		// Init rig...

		this.fixedTimeStep.Reset();


		this.ResetState();
		this.MuteUntilRelease();

		if (CFUtils.editorStopped)
			return;

		CF2Input.onMobileModeChange += this.OnMobileModeChange;
		CFCursor.onLockStateChange	+= this.OnCursorLockStateChange;
		}

	// --------------------
	override protected void OnDisableComponent()
		{
		if (CF2Input.activeRig == this)
			CF2Input.activeRig = null;

		this.ResetState();

		if (CFUtils.editorStopped)
			return;

		CF2Input.onMobileModeChange -= this.OnMobileModeChange;
		CFCursor.onLockStateChange	-= this.OnCursorLockStateChange;

		}
		


	// ------------------
	private void OnMobileModeChange()		{ this.SyncDisablingConditions(false); }
	private void OnCursorLockStateChange()	{ this.SyncDisablingConditions(false); }


	// ------------------
	void OnApplicationPause(bool paused)
		{
		int count = this.touchControls.Count;
		for (int i = 0; i < count; ++i)
			{
			TouchControl c = this.touchControls[i];
			if (c != null)
				c.ReleaseAllTouches(); //true);
			}
		}
		
		
	// --------------------
	void Update()
		{	
		this.fixedTimeStep.Update(CFUtils.realDeltaTimeClamped);


#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return;
#endif

		if (!this.IsInitialized)
			return;

		
		this.UpdateConfigs();	
		}


//! \endcond
		

	// -------------------
	//! Total reset - axes, mouse, tilt, touch controls. Everything except rig switches...
	// -------------------
	public void ResetState()	
		{
		if (!this.IsInitialized)
			return;


		//this.rigSwitches.Reset();
	

		// Reset tilt and mouse...

		this.tilt.Reset();
		this.mouseConfig.Reset();
		this.scrollWheel.Reset();
			


		this.ResetEmuTouches();


		this.joysticks.ResetAll();
		this.axes.ResetAll();

			
		this.MuteUntilRelease();


		// Reset keycodes...

		this.keysCur.SetAll(false);
		this.keysPrev.SetAll(false);
		this.keysNext.SetAll(false);

		this.keysMuted.SetAll(true);

		this.keysCurSomeDown	= false;
		this.keysCurSomeOn		= false;
		this.keysNextSomeDown	= false;
		this.keysNextSomeOn		= false;

//
		

		this.InvalidateBlockedKeys();


		// Reset touch controls...

		for (int i = 0; i < this.touchControls.Count; ++i)
			{
			TouchControl c = this.touchControls[i];
			if (c != null)
				c.ResetControl();
			}

		
 
		}



	// ---------------------
	public void MuteUntilRelease()
		{
		this.axes.MuteAllUntilRelease();

		this.keysPrev.SetAll(false);
		this.keysCur.SetAll(false);
		this.keysMuted.SetAll(true);
		}

		


	// -------------------
	private void UpdateConfigs()
		{
		this.CheckResolution();

		// Update touch sleeping...

		if (!this.touchControlsSleeping)
			{
			float sleepDelay = -1;

			if (this.hideWhenTouchScreenIsUnused)
				sleepDelay = this.hideWhenTouchScreenIsUnusedDelay;
			
			if (this.hideWhenGamepadIsActivated && (GamepadManager.activeManager != null) && (GamepadManager.activeManager.GetActiveGamepadCount() > 0))
				{
				if (sleepDelay < 0)
					sleepDelay = this.hideWhenGamepadIsActivatedDelay;
				else if (this.hideWhenGamepadIsActivatedDelay < sleepDelay)
					sleepDelay = this.hideWhenGamepadIsActivatedDelay;
				}
 
			if (sleepDelay > 0)
				{
				this.elapsedSinceLastTouch += CFUtils.realDeltaTimeClamped;
				if (this.elapsedSinceLastTouch > sleepDelay)
					this.PutTouchControlsToSleep();	
				}	
			} 
		else
			{
			// Wake them up if there's no active gamepads left...

			if (!this.hideWhenTouchScreenIsUnused && this.hideWhenGamepadIsActivated && 
				(GamepadManager.activeManager != null) && (GamepadManager.activeManager.GetActiveGamepadCount() == 0))
				this.WakeTouchControlsUp();
					
			}


		// If rig Switches changed their state...			

		this.ApplySwitches(false);
			


		// Update touch controls...
			
		for (int i = 0; i < this.touchControls.Count; ++i)
			this.touchControls[i].UpdateControl();


		// Update tilt and mouse...

		this.tilt.Update();
		this.mouseConfig.Update();
		this.scrollWheel.Update();

		// Update collected joys and axes...

		this.joysticks.Update(this);


		this.axes.ApplyKeyboardInput();

		// Apply custom input...

		if ((this == CF2Input.activeRig) && (onAddExtraInputToActiveRig != null))
			InputRig.onAddExtraInputToActiveRig();

		if (this.onAddExtraInput != null)
			this.onAddExtraInput();


		// Apply auto-input...

		this.autoInputList.Update(this);

		this.axes.Update(this);
		


		// Update keys by cycling state bitsets...

		this.UpdateKeyCodes();
				

		// Update emu-touches...

		this.UpdateEmuTouches();

		}



	// ---------------------
	private void UpdateKeyCodes()
		{
		BitArray oldPrevBuff = this.keysPrev;
		this.keysPrev	= this.keysCur; 
		this.keysCur	= this.keysNext;
		this.keysNext	= oldPrevBuff; 

		this.keysNext.SetAll(false);

		// Unmute keys if they are released...

		this.keysMuted.And(this.keysCur);

		// Mute keys...
		
		this.keysMuted.Not();
		this.keysCur.And(this.keysMuted);
		this.keysMuted.Not();

			
		this.keysCurSomeDown		= this.keysNextSomeDown;
		this.keysCurSomeOn			= this.keysNextSomeOn;

		this.keysNextSomeDown	= false;
		this.keysNextSomeOn		= false;

		}

	// --------------------
	private void InvalidateBlockedKeys()
		{
		this.keysBlocked.SetAll(false);

		for (int i = 0; i < this.keyboardBlockedCodes.Count; ++i)
			{
			int key = (int)this.keyboardBlockedCodes[i];
			if ((key >= 0) && (key < this.keysBlocked.Length))
				 this.keysBlocked[key] = true;
			}
		}
		


//! \cond


	static public bool GetSourceKeyState(KeyCode key)
		{
		if ((key == KeyCode.None) || 
			(CF2Input.IsInMobileMode() && ((key >= KeyCode.Mouse0) && (key <= KeyCode.Mouse6))))
			return false;

		return UnityEngine.Input.GetKey(key);
		}

	// --------------------------
	static public bool IsMouseKeyCode(KeyCode key)	
		{ return ((key >= KeyCode.Mouse0) && (key <= KeyCode.Mouse6)); }

	// ---------------------
	public void AddControl(TouchControl c)
		{
		if (!this.CanBeUsed())
			return;

#if UNITY_EDITOR
		if (this.touchControls.Contains(c))
			{
			Debug.LogError("Config(" + this.name + ") already contains " + ((c != null) ? c.name : "NULL"));
			return;  
			}
#endif
		this.touchControls.Add(c);
		}

	// --------------------
	public void RemoveControl(TouchControl c)
		{
		if (!this.CanBeUsed())
			return;

		if (this.touchControls != null)
			this.touchControls.Remove(c);
		}	

//! \endcond
		

	// ----------------------
	//! Get a list of all active Touch Controls attached to this rig.
	// ----------------------  
	public List<TouchControl> GetTouchControls()
		{
		return this.touchControls;
		}
		

	// --------------------
	//! Find a touch control by name.
	// --------------------
	public TouchControl FindTouchControl(string name)
		{
		return this.touchControls.Find(x => (x.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)));
		}


		

		
	private bool
		touchControlsHidden;



	// ---------------------------
	//! \name Touch Control visibility-related methods
	//! \{
	// ---------------------------

	// -----------------------
	//! Manually hide or show touch controls of this rig.
	// -----------------------
	public void ShowOrHideTouchControls(bool show, bool skipAnim)
		{	
		if (this.touchControlsHidden != !show)
			{
			this.touchControlsHidden = !show;
		
			this.SyncDisablingConditions(skipAnim);

			// Wake controls up if there's no activated gamepads...

			if ((GamepadManager.activeManager == null) || (GamepadManager.activeManager.GetActiveGamepadCount() == 0))
				{
	 			if (show && this.AreTouchControlsSleeping())	
					this.WakeTouchControlsUp();
				}
			}
		}

	// -------------------
	//! Check if touch controls are hidden manually.
	// ----------------------
	public bool AreTouchControlsHiddenManually()
		{
		return this.touchControlsHidden;
		}



	// -------------------
	//! Check if the touch controls are sleeping due to touch screen inactivity.
	// -------------------
	public bool AreTouchControlsSleeping()
		{
		return this.touchControlsSleeping;
		}

	// ----------------------
	//! Wake up sleeping touch controls.
	// ----------------------
	public void WakeTouchControlsUp()
		{
		this.elapsedSinceLastTouch = 0;

		if (this.touchControlsSleeping)
			{
			this.touchControlsSleeping = false;
			this.SyncDisablingConditions(false); //this.SetControlsHidingSwitch(TouchControl.HIDDEN_DUE_TO_INACTIVITY, false);
			}
		}

	// ---------------------
	//! Put touch controls to sleep. They will be awaken when the touch screen will be touched. 
	// ----------------------
	public void PutTouchControlsToSleep()
		{
		this.touchControlsSleeping = true;
		this.SyncDisablingConditions(false); 
		}
		




	// ------------------
	//! Returns true if the controls are not sleeping and are not manually hidden.
	// ------------------
	public bool AreTouchControlsVisible()
		{ 
		return (!this.touchControlsSleeping && !this.touchControlsHidden);
		}

	// ----------------------
	private void SyncDisablingConditions(bool noAnim)
		{
		int count = this.touchControls.Count; 
		for (int i = 0; i < count; ++i)
			{
			TouchControl c = this.touchControls[i];
			if (c != null) 
				c.SyncDisablingConditions(noAnim);
			}

		this.tilt.OnDisablingConditionsChange();

	 		
		this.autoInputList.OnDisablingConditionsChange();

		}

	//! \}


		

	// ---------------------------
	//! \name Tilt-related methods
	//! \{
	// ---------------------------

	// ---------------
	//! Is tilt/accelerometer available?
	// ---------------
	static public bool IsTiltAvailable()
		{
		return TiltState.IsAvailable();
		}

	// ------------------
	//! Return true if the accelerometer is calibrated. Calibration is required to unblock pitch axis, roll axis doesn't require calibration.
	// ------------------
	public bool IsTiltCalibrated()
		{ return this.tilt.tiltState.IsCalibrated(); }
		
	// --------------------
	//! Calibrate tilt by storing devices current orientation as neutral.
	// --------------------
	public void CalibrateTilt()
		{ this.tilt.tiltState.Calibate(); }

	// ------------------
	//! Reset tilt state. Calibration will be required afterwards.
	// ------------------
	public void ResetTilt()
		{ this.tilt.Reset(); }

	//! \}




	// ---------------------------
	//! \name Rig Switch-related methods
	//! \{
	// ---------------------------

	
	// -----------------
	public bool GetSwitchState(string name, ref int cachedId, bool fallbackVal)
		{ return this.rigSwitches.GetSwitchState(name, ref cachedId, fallbackVal); }
	public bool GetSwitchState(string name, bool fallbackVal)
		{ return this.rigSwitches.GetSwitchState(name, fallbackVal); }

	public void SetSwitchState(string name, ref int cachedId, bool state)
		{ this.rigSwitches.SetSwitchState(name, ref cachedId, state); }
	public void SetSwitchState(string name, bool state)
		{ this.rigSwitches.SetSwitchState(name, state); }

	public void SetAllSwitches(bool state)
		{ this.rigSwitches.SetAll(state); }
		
	public void ResetSwitch(string name, ref int cachedId)
		{ this.rigSwitches.ResetSwitch(name, ref cachedId); }
	public void ResetSwitch(string name)
		{ this.rigSwitches.ResetSwitch(name); }

	public void ResetAllSwitches(bool skipAnim)
		{ 
		this.rigSwitches.Reset(); 

		if (skipAnim)
			this.ApplySwitches(true);
		}

	// ---------------
	public bool IsSwitchDefined(string name, ref int cachedId)		{ return (this.rigSwitches.Get(name, ref cachedId) != null); }
	public bool IsSwitchDefined(string name)							{ return (this.rigSwitches.Get(name) != null); }


	// ----------------------
	//! Force switch application. Normally changed switches are applied at the begining of the next frame.
	// ----------------------
	public void ApplySwitches(bool skipAnim)
		{
		if (this.rigSwitches.changed)
			{	
			this.SyncDisablingConditions(skipAnim);
			this.rigSwitches.changed = false;

			if (this.onSwitchesChanged != null)
				this.onSwitchesChanged();

			}
		}

	
	//! \}




	// -------------------
	//! \name Input-compatibility methods section.
	//! \{
	// ----------------------

	// ---------------
	public void ResetInputAxes()
		{
		this.MuteUntilRelease();


		// TODO : reset touch controller, reset gamepads?
			
		}


	// ---------------
	//! Is Axis Defined?
	// ---------------
	public bool IsAxisDefined(string name, ref int cachedId)		{ return (this.axes.Get(name, ref cachedId) != null); }
	public bool IsAxisDefined(string name)							{ return (this.axes.Get(name) != null); }


	//! \}		

	
	// ---------------------------
	//! \name Named axis methods
	//! \{
	// ---------------------------

	// --------------
	public float GetAxis(string axisName)
		{
		AxisConfig s = this.axes.Get(axisName);
		return ((s != null) ? s.GetAnalog() : 0);
		}

	public float GetAxis(string axisName, ref int cachedId)
		{
		AxisConfig s = this.axes.Get(axisName, ref cachedId);
		return ((s != null) ? s.GetAnalog() : 0);
		}



	// --------------
	public float GetAxisRaw(string axisName)
		{
		AxisConfig s = this.axes.Get(axisName);
		return ((s != null) ? s.GetAnalogRaw() : 0);
		}

	public float GetAxisRaw(string axisName, ref int cachedId)
		{
		AxisConfig s = this.axes.Get(axisName, ref cachedId);
		return ((s != null) ? s.GetAnalogRaw() : 0);
		}

	
	//! \}


	// ---------------------------
	//! \name Named button methods
	//! \{
	// ---------------------------

	// --------------
	public bool GetButton(string axisName)
		{
		AxisConfig s = this.axes.Get(axisName);
		return ((s != null) ? s.GetButton() : false);
		}

	public bool GetButton(string axisName, ref int cachedId)
		{
		AxisConfig s = this.axes.Get(axisName, ref cachedId);
		return ((s != null) ? s.GetButton() : false);
		}

		
	// --------------
	public bool GetButtonDown(string axisName)
		{
		AxisConfig s = this.axes.Get(axisName);
		return ((s != null) ? s.GetButtonDown() : false);
		}

	public bool GetButtonDown(string axisName, ref int cachedId)
		{
		AxisConfig s = this.axes.Get(axisName, ref cachedId);
		return ((s != null) ? s.GetButtonDown() : false);
		}
	

	// --------------
	public bool GetButtonUp(string axisName)
		{
		AxisConfig s = this.axes.Get(axisName);
		return ((s != null) ? s.GetButtonUp() : false);
		}
	
	public bool GetButtonUp(string axisName, ref int cachedId)
		{
		AxisConfig s = this.axes.Get(axisName, ref cachedId);
		return ((s != null) ? s.GetButtonUp() : false);
		}



	
	//! \}

	
	// ---------------------------
	//! \name Key methods
	//! \{
	// ---------------------------

	// --------------
	public bool GetKey(KeyCode keyCode)
		{
		if (((int)keyCode < 0) || ((int)keyCode >= this.keysCur.Length))
			{
#if UNITY_EDITOR
			Debug.LogError("Out of range KeyCode : " + keyCode + " (" + (int)keyCode + ")");
#endif
			return false;
			}				

		if (this.keysCur.Get((int)keyCode))
			return true;
		if (this.keysBlocked[(int)keyCode])
			return false;		
		return ((CF2Input.IsInMobileMode() && IsMouseKeyCode(keyCode)) ?  false : UnityEngine.Input.GetKey(keyCode));
		}
		
	// --------------
	public bool GetKeyDown(KeyCode keyCode)
		{
		if (((int)keyCode < 0) || ((int)keyCode >= this.keysCur.Length))
			{
#if UNITY_EDITOR
			Debug.LogError("Out of range KeyCode : " + keyCode + " (" + (int)keyCode + ")");
#endif
			return false;
			}				

		int i = (int)keyCode;
		if (!this.keysPrev.Get(i) && this.keysCur.Get(i))
			return true;
		if (this.keysBlocked[i])
			return false;		
		return ((CF2Input.IsInMobileMode() && IsMouseKeyCode(keyCode)) ?  false : UnityEngine.Input.GetKeyDown(keyCode));
		}
		
	// --------------
	public bool GetKeyUp(KeyCode keyCode)
		{
		if (((int)keyCode < 0) || ((int)keyCode >= this.keysCur.Length))
			{
#if UNITY_EDITOR
			Debug.LogError("Out of range KeyCode : " + keyCode + " (" + (int)keyCode + ")");
#endif
			return false;
			}				

		int i = (int)keyCode;
		if (this.keysPrev.Get(i) && !this.keysCur.Get(i))
			return true;
		if (this.keysBlocked[i])
			return false;		
		return ((CF2Input.IsInMobileMode() && IsMouseKeyCode(keyCode)) ?  false : UnityEngine.Input.GetKeyUp(keyCode));
		}
		


	// --------------
	public bool GetKey		(string keyName)	{ return this.GetKey(NameToKeyCode(keyName)); }
	public bool GetKeyDown	(string keyName)	{ return this.GetKeyDown(NameToKeyCode(keyName)); }
	public bool GetKeyUp	(string keyName)	{ return this.GetKeyUp(NameToKeyCode(keyName)); }
				
		

	// ----------------------
	public bool AnyKey()		{ return (this.keysCurSomeOn || UnityEngine.Input.anyKey); }
	public bool AnyKeyDown()	{ return (this.keysCurSomeDown || UnityEngine.Input.anyKeyDown); }


	
	//! \}


	// ---------------------------
	//! \name Mouse button methods
	//! \{
	// ---------------------------

	// --------------
	public bool GetMouseButton(int mouseButton)
		{
		return (((mouseButton >= 0) && (mouseButton <= 6)) ? 
			GetKey(KeyCode.Mouse0 + mouseButton) : false);
		}
		
	// --------------
	public bool GetMouseButtonDown(int mouseButton)
		{
		return (((mouseButton >= 0) && (mouseButton <= 6)) ? 
			GetKeyDown(KeyCode.Mouse0 + mouseButton) : false);
		}

	// --------------
	public bool GetMouseButtonUp(int mouseButton)
		{
		return (((mouseButton >= 0) && (mouseButton <= 6)) ? 
			GetKeyUp(KeyCode.Mouse0 + mouseButton) : false);
		}
		
	//! \} 
	


	


//! \cond

	// ---------------	
	public bool IsAxisAvailableOnMobile(string axisName)
		{
		for (int i = 0; i < this.touchControls.Count; ++i)
			{
			if (this.touchControls[i].IsBoundToAxis(axisName, this))
				return true;
			}
		return false;
		}


	// ---------------
	public bool IsKeyAvailableOnMobile(KeyCode keyCode)
		{
		for (int i = 0; i < this.touchControls.Count; ++i)
			{
			if (this.touchControls[i].IsBoundToKey(keyCode, this))
				return true;
			}

		for (int i = 0; i < this.axes.list.Count; ++i)
			{
			InputRig.AxisConfig axis = this.axes.list[i];
			if (axis.DoesAffectKeyCode(keyCode) && IsAxisAvailableOnMobile(axis.name))
				return true;				
			}

		return false;
		}

	// --------------
	public bool IsTouchEmulatedOnMobile()
		{
		for (int i = 0; i < this.touchControls.Count; ++i)
			{
			if (this.touchControls[i].IsEmulatingTouches())
				return true;
			}
		return false;
		}

	// --------------
	public bool IsMousePositionEmulatedOnMobile()
		{
		for (int i = 0; i < this.touchControls.Count; ++i)
			{
			if (this.touchControls[i].IsEmulatingMousePosition())
				return true;
			}
		return false;
		}

	
	// ---------------
	public bool IsScrollWheelEmulatedOnMobile()
		{
		if (!this.scrollWheel.vertScrollDeltaBinding.deltaBinding.enabled)
			return false;
		AxisBinding.TargetElem bindElem = this.scrollWheel.vertScrollDeltaBinding.deltaBinding.GetTarget(0);
		if (bindElem == null)
			return false;

		return (!bindElem.separateAxes && this.IsAxisAvailableOnMobile(bindElem.singleAxis));
		}



	// ---------------
	//! Is Virtual Joystick Defined?
	// ---------------
	public bool IsJoystickDefined(string name, ref int cachedId)	{ return (this.joysticks.Get(name, ref cachedId) != null); }
	public bool IsJoystickDefined(string name)						{ return (this.joysticks.Get(name) != null); }


	// -----------------
	public JoystickState GetJoystickState(string name, ref int cachedId)
		{ 
		VirtualJoystickConfig joy = this.joysticks.Get(name, ref cachedId);
		return ((joy == null) ? null : joy.joystickState); 
		}
		

	// ----------------
	static public KeyCode MouseButtonToKey(int mouseButton)
		{
		return (((mouseButton >= 0) && (mouseButton <= 6)) ? (KeyCode.Mouse0 + mouseButton) : KeyCode.Mouse0);
		}






	// ------------------	
	public int axisConfigCount 
		{ get { return this.axes.list.Count; } }

	// ------------------
	public AxisConfig GetAxisConfig(int id)
		{
		if ((id < 0) || (id >= this.axes.list.Count))
			return null;

		return this.axes.list[id];
		}


	// ------------------
	public AxisConfig GetAxisConfig(string name, ref int cachedId)
		{
		return this.axes.Get(name, ref cachedId);
		}

	public AxisConfig GetAxisConfig(string name)
		{
		return this.axes.Get(name);
		}

		
	// -----------------
	public void SetAxis(string name, ref int cachedId, float v, InputSource source)
		{	AxisConfig axis = this.axes.Get(name, ref cachedId);	if (axis != null) axis.Set(v, source); } 
		
	public void SetAxisScroll(string name, ref int cachedId, int v)
		{	AxisConfig axis = this.axes.Get(name, ref cachedId);	if (axis != null) axis.SetScrollDelta(v); } 


	// -----------
	public void SetAxisDigital(string name, ref int cachedId, /*bool v,*/ bool negSide)
		{	AxisConfig axis = this.axes.Get(name, ref cachedId);	if (axis != null) axis.SetSignedDigital(/*v,*/ !negSide); } 

 
	public void SetJoystickState(string name, ref int joyId, JoystickState state)
		{ VirtualJoystickConfig joy = this.joysticks.Get(name, ref joyId); if (joy != null) joy.SetState(state); }




	// ------------
	public void SetKeyCode(KeyCode keyCode) //, bool state)
		{
		if (keyCode == KeyCode.None)
			return;

		//if (!state)
		//	return;

 
		int keyId = (int)keyCode;
		this.keysNext.Set(keyId, true);

		if (!this.keysCur[keyId])
			this.keysNextSomeDown = true;
			
		this.keysNextSomeOn = true;

		}


	// ------------------
	public bool GetNextFrameKeyState(KeyCode key)
		{
		return this.keysNext[(int)key];
		}


		
	// ----------------------
	private float 
		analogToEmuMouseScale,
		analogToUniversalScale,
		analogToRawDeltaScale,
		scrollToEmuMouseScale,
		scrollToUniversalScale,
		mousePointsToUniversalScale,
		touchPixelsToEmuMouseScale,
		touchPixelsToUniversalScale;

	private int 
		storedHorzRes,
		storedVertRes;	
	private float 
		storedDPCM;
		

	// ----------------------
	public void RecalcPixelConversionParams()
		{
		this.CheckResolution(true);
		}
	

	// ----------------------
	private void CheckResolution(bool forceRecalc = false)
		{
		float dpcm = CFScreen.dpcm;

		if (forceRecalc || ((this.storedHorzRes == 0) || (this.storedHorzRes != Screen.width) || (this.storedVertRes != Screen.height) ||
			(this.storedDPCM < 1) || (this.storedDPCM != dpcm)))
			{
			this.storedHorzRes	= Screen.width;
			this.storedVertRes	= Screen.height;
			this.storedDPCM 	= dpcm;
				
			// When not on mobile device, use virtual screen DPI...
			
			if (!Application.isMobilePlatform || (dpcm <= 1))
				dpcm = Mathf.Sqrt((this.storedHorzRes * this.storedHorzRes) + (this.storedVertRes * this.storedVertRes)) / (this.virtualScreenDiameterInches * 2.54f);
				

			//this.mousePixelsToEmuMouseScale = (this.storedHorzRes == 0) ? 0 : ((float)this.mouseTargetHorzRes / (float)this.storedHorzRes);
			this.touchPixelsToEmuMouseScale = (dpcm < 1) ? 0 : ((float)this.ninetyDegTurnMouseDelta / (this.ninetyDegTurnTouchSwipeInCm * dpcm)); 
	
			this.mousePointsToUniversalScale = (this.ninetyDegTurnMouseDelta == 0) ? 0 : (1.0f / this.ninetyDegTurnMouseDelta);
			this.touchPixelsToUniversalScale = (dpcm < 1) ? 0 : ((float)1.0f / (this.ninetyDegTurnTouchSwipeInCm * dpcm));

			this.analogToEmuMouseScale	= (float)this.ninetyDegTurnMouseDelta / this.ninetyDegTurnAnalogDuration;
			this.analogToUniversalScale	= 1.0f / this.ninetyDegTurnAnalogDuration;		 
			this.analogToRawDeltaScale	= (float)this.ninetyDegTurnMouseDelta / this.ninetyDegTurnAnalogDuration;

			this.scrollToEmuMouseScale	= (float)this.ninetyDegTurnMouseDelta / (float)this.scrollStepsPerNinetyDegTurn;
			this.scrollToUniversalScale	= 1.0f / (float)this.scrollStepsPerNinetyDegTurn;		 

		

			}
		}


	// ----------------------
	public float TransformMousePointDelta(float mousePoints, DeltaTransformMode mode)
		{
		switch (mode)
			{
			case DeltaTransformMode.EmulateMouse :	return mousePoints; //(mousePix * this.mousePixelsToEmuMouseScale);
			case DeltaTransformMode.Universal :		return (mousePoints * this.mousePointsToUniversalScale); //(mousePix * this.mousePixelsToUniversalScale);
			}

		return mousePoints;
		}


	// ----------------------
	public float TransformTouchPixelDelta(float touchPix, DeltaTransformMode mode)
		{
		switch (mode)
			{
			case DeltaTransformMode.EmulateMouse :	return (touchPix * this.touchPixelsToEmuMouseScale);
			case DeltaTransformMode.Universal :		return (touchPix * this.touchPixelsToUniversalScale);
			}

		return touchPix;
		}
		

	// ----------------------
	public float TransformNormalizedDelta(float normDelta, DeltaTransformMode mode)
		{
		switch (mode)
			{
			case DeltaTransformMode.EmulateMouse	: return (normDelta * this.ninetyDegTurnMouseDelta); //(float)this.mouseTargetHorzRes);
			case DeltaTransformMode.Universal		: return (normDelta);
			case DeltaTransformMode.Raw				: return (normDelta * this.ninetyDegTurnMouseDelta); //(float)this.storedHorzRes);
			}

		return normDelta;
		}

	// ----------------------
	public float TransformScrollDelta(float scrollDelta, DeltaTransformMode mode)
		{
		switch (mode)
			{
			case DeltaTransformMode.EmulateMouse	: return (scrollDelta * this.scrollToEmuMouseScale);
			case DeltaTransformMode.Raw 			: return (scrollDelta * this.scrollToEmuMouseScale);
			case DeltaTransformMode.Universal		: return (scrollDelta * this.scrollToUniversalScale);
			}

		return scrollDelta;
		}


	// ----------------------
	public float TransformAnalogDelta(float analogVal, DeltaTransformMode mode)
		{
		switch (mode)
			{
			case DeltaTransformMode.EmulateMouse :	return (analogVal * this.analogToEmuMouseScale);
			case DeltaTransformMode.Universal :		return (analogVal * this.analogToUniversalScale);
			case DeltaTransformMode.Raw :			return (analogVal * this.analogToRawDeltaScale);
			}

		return analogVal;
		}




	// ---------------------
	public void GetSubBindingDescriptions(
		BindingDescriptionList			descList, 
		Object 							undoObject,
		string 							parentMenuPath)
		{
		this.mouseConfig.GetSubBindingDescriptions(descList, this, parentMenuPath + "Mouse/");
		this.tilt.GetSubBindingDescriptions(descList, this, parentMenuPath + "Tilt/");
		this.scrollWheel.GetSubBindingDescriptions(descList, this, parentMenuPath + "Mouse Scroll Wheel/");
			
		this.anyGamepad.GetSubBindingDescriptions(descList, this, parentMenuPath + "Gamepads/Combined Gamepad/");
		for (int i = 0; i < this.gamepads.Length; ++i)
			this.gamepads[i].GetSubBindingDescriptions(descList, this, parentMenuPath + "Gamepads/Gamepad " + (i + 1).ToString() + "/");
			
		for (int i = 0; i < this.joysticks.list.Count; ++i)
			{
			this.joysticks.list[i].GetSubBindingDescriptions(descList, this, parentMenuPath + "Virtual Joysticks/Joystick [" + this.joysticks.list[i].name + "]/");
			}
		}
		

	// -----------------------
	public bool IsBoundToKey(KeyCode key, InputRig rig)
		{
		if (this.mouseConfig.IsBoundToKey(key, rig) ||
			this.tilt		.IsBoundToKey(key, rig) ||
			this.scrollWheel.IsBoundToKey(key, rig) ||
			this.anyGamepad	.IsBoundToKey(key, rig))
			return true;

		for (int i = 0; i < this.gamepads.Length; ++i)
			{ if (this.gamepads[i].IsBoundToKey(key, rig)) return true; }
			
		for (int i = 0; i < this.joysticks.list.Count; ++i)
			{ if (this.joysticks.list[i].IsBoundToKey(key, rig)) return true; }

		return false;
		}

	// ---------------------
	public bool IsBoundToAxis			(string axisName, InputRig rig)
		{
		if (this.mouseConfig.IsBoundToAxis(axisName, rig) ||
			this.tilt		.IsBoundToAxis(axisName, rig) ||
			this.scrollWheel.IsBoundToAxis(axisName, rig) ||
			this.anyGamepad	.IsBoundToAxis(axisName, rig))
			return true;

		for (int i = 0; i < this.gamepads.Length; ++i)
			{ if (this.gamepads[i].IsBoundToAxis(axisName, rig)) return true; }
			
		for (int i = 0; i < this.joysticks.list.Count; ++i)
			{ if (this.joysticks.list[i].IsBoundToAxis(axisName, rig)) return true; }

		return false;
		}


	// ---------------------
	public bool IsEmulatingTouches		()
		{
		if (this.mouseConfig.IsEmulatingTouches() ||
			this.tilt		.IsEmulatingTouches() ||
			this.scrollWheel.IsEmulatingTouches() ||
			this.anyGamepad	.IsEmulatingTouches())
			return true;

		for (int i = 0; i < this.gamepads.Length; ++i)
			{ if (this.gamepads[i].IsEmulatingTouches()) return true; }
			
		for (int i = 0; i < this.joysticks.list.Count; ++i)
			{ if (this.joysticks.list[i].IsEmulatingTouches()) return true; }

		return false;
		}

	// ---------------------
	public bool IsEmulatingMousePosition()
		{
		if (this.mouseConfig.IsEmulatingMousePosition() ||
			this.tilt		.IsEmulatingMousePosition() ||
			this.scrollWheel.IsEmulatingMousePosition() ||
			this.anyGamepad	.IsEmulatingMousePosition())
			return true;

		for (int i = 0; i < this.gamepads.Length; ++i)
			{ if (this.gamepads[i].IsEmulatingMousePosition()) return true; }
			
		for (int i = 0; i < this.joysticks.list.Count; ++i)
			{ if (this.joysticks.list[i].IsEmulatingMousePosition()) return true; }

		return false;
		}


		
	// --------------------
	public struct Touch
		{
		public TouchPhase	phase;
		public float		deltaTime;
		public int			fingerId;
		public int			tapCount;
		public Vector2		position;
		public Vector2		rawPosition;
		public Vector2		deltaPosition;
#if !UNITY_PRE_5_3
		public float
			altitudeAngle,
			azimuthAngle,
			maximumPossiblePressure,
			pressure,
			radius,
			radiusVariance ;
		public UnityEngine.TouchType
			type;			
#endif			


		
		static public Touch Dummy;
		static public Touch[] EmptyArray;
			
		// ---------------
		static Touch()
			{
			Dummy = new Touch();
			Dummy.phase = TouchPhase.Canceled;

			EmptyArray = new Touch[0];
			}

		// -------------------
		public Touch(UnityEngine.Touch t)
			{
			this.phase			= t.phase;
			this.deltaTime		= t.deltaTime;
			this.fingerId		= t.fingerId;
			this.tapCount		= t.tapCount;
			this.position		= t.position;
			this.rawPosition	= t.rawPosition;
			this.deltaPosition = t.deltaPosition;

#if !UNITY_PRE_5_3
			this.altitudeAngle= 0;
			this.azimuthAngle	= 0;
			this.maximumPossiblePressure = 1.0f;
			this.pressure		= 1;
			this.radius 		= 1;
			this.radiusVariance = 0;
			this.type			= UnityEngine.TouchType.Direct;
#endif			

			}
			

		static private Touch[] mTranslatedArray;

		// ------------------
		static public Touch[] TranslateUnityTouches(UnityEngine.Touch[] tarr)
			{
			if ((tarr == null) || (tarr.Length == 0))
				return Touch.EmptyArray;

			if ((mTranslatedArray == null) || (tarr.Length != mTranslatedArray.Length))
				mTranslatedArray = new Touch[tarr.Length];
				
			for (int i = 0; i < tarr.Length; ++i)
				mTranslatedArray[i] = new Touch(tarr[i]);

			return mTranslatedArray;		
			}
		}

	
		
	// ------------------
	private const int MAX_EMU_TOUCHES = 8;
	private List<EmulatedTouchState> emuTouches;
	private List<EmulatedTouchState> emuTouchesOrdered;
	private Touch[] emuOutputTouches;
	private bool	emuOutputTouchesDirty;
	

	// ------------------
	private void InitEmuTouches()
		{
		this.emuTouches = new List<EmulatedTouchState>(MAX_EMU_TOUCHES);
		this.emuTouchesOrdered = new List<EmulatedTouchState>(MAX_EMU_TOUCHES);

		for (int i = 0; i < MAX_EMU_TOUCHES; ++i)
			{
			this.emuTouches.Add(new EmulatedTouchState(i));
			}
			
		}
		

	// ------------------
	public int InternalStartEmuTouch(Vector2 pos)	
		{
		for (int i = 0; i < this.emuTouches.Count; ++i)
			{
			EmulatedTouchState touch = this.emuTouches[i];
			if (!touch.IsUsed())
				{
				touch.Start(pos);
				return i;
				}
			}

		return -1;
		}
		
	// --------------
	public void InternalEndEmuTouch(int emuTouchId, bool cancel)
		{
		if ((emuTouchId < 0) || (emuTouchId >= this.emuTouches.Count))
			return;
		
		this.emuTouches[emuTouchId].EndTouch(cancel);
		}
		
	// ------------
	public void InternalUpdateEmuTouch(int emuTouchId, Vector2 pos)
		{
		if ((emuTouchId < 0) || (emuTouchId >= this.emuTouches.Count))
			return;
		
		this.emuTouches[emuTouchId].UpdatePos(pos);
		}


	// -----------------
	public Touch[] GetEmuTouchArray()	
		{
		if (this.emuOutputTouchesDirty || (this.emuOutputTouches == null))
			{
			if ((this.emuOutputTouches == null) || (this.emuTouchesOrdered.Count != this.emuOutputTouches.Length))
				this.emuOutputTouches = new Touch[this.emuTouchesOrdered.Count];

			for (int i = 0; i < this.emuTouchesOrdered.Count; ++i)
				this.emuOutputTouches[i] = this.emuTouchesOrdered[i].outputTouch;

			this.emuOutputTouchesDirty = false;
			}

		return this.emuOutputTouches;		
		}
	

	// -----------------
	public int GetEmuTouchCount()
		{	
		return this.emuTouchesOrdered.Count;
		}


	// ----------------
	public Touch GetEmuTouch(int i)
		{	
		if ((i < 0) || (i >= this.emuTouchesOrdered.Count))
			return Touch.Dummy;

		return this.emuTouchesOrdered[i].outputTouch;
		}

		
	// -----------------
	private void ResetEmuTouches()
		{
		for (int i = 0; i < this.emuTouches.Count; ++i)	
			this.emuTouches[i].Reset();
		}

	// -------------------
	private void UpdateEmuTouches()
		{
		this.emuTouchesOrdered.Clear();

		for (int i = 0; i < this.emuTouches.Count; ++i)	
			{
			EmulatedTouchState t = this.emuTouches[i];
			t.Update();

			if (t.IsActive())
				this.emuTouchesOrdered.Add(t);
			}	


		this.emuOutputTouchesDirty = true;
		}



	// --------------------
	public void SyncEmuTouch(TouchGestureBasicState touch, ref int emuTouchId)
		{
		if (touch == null)
			return;

		if (touch.JustPressedRaw())
			{
			emuTouchId = this.InternalStartEmuTouch(touch.GetStartPos());
			}
		else if (touch.JustReleasedRaw())	
			{

			this.InternalEndEmuTouch(emuTouchId, false);
			emuTouchId = -1;
			}

		if (touch.PressedRaw())
			this.InternalUpdateEmuTouch(emuTouchId, touch.GetCurPosRaw());

		}





	// ------------------
	private class EmulatedTouchState
		{
		private int fingerId;
		public		TouchGestureBasicState touch;
		public Touch outputTouch;
		private bool isUsed;
		private bool	updatedThisFrame;
			
		// ---------------
		public EmulatedTouchState()
			{
			this.Reset();
			}

		// -----------------
		public bool IsUsed()
			{
			return this.isUsed;
			}

		// -----------------
		public bool IsActive()
			{
			return (this.touch.PressedRaw() || this.touch.JustReleasedRaw());
			}
			

		// ----------------
		public void Start(Vector2 pos)
			{	
			this.isUsed = true;
			this.updatedThisFrame = true;
			this.touch.OnTouchStart(pos, pos, 0, false, false, 1);
			}
			
		// -----------------
		public void UpdatePos(Vector2 pos)
			{
			this.updatedThisFrame = true;
			this.touch.OnTouchMove(pos);
			}

		// -----------------
		public void EndTouch(bool cancel)
			{
			this.updatedThisFrame = true;
			this.isUsed = false;
			this.touch.OnTouchEnd(cancel);
			}

		// ---------------
		public EmulatedTouchState(int fingerId)
			{
			this.fingerId = fingerId;
			this.touch = new TouchGestureBasicState();
			this.outputTouch = new Touch();
			}
			


		// ----------------
		public void Reset()
			{
			this.isUsed = false;
			this.touch.Reset();
			this.SyncOutputTouch();
			}
			

		// ---------------
		private void SyncOutputTouch()
			{
			this.outputTouch.fingerId = this.fingerId;
			this.outputTouch.deltaTime = Time.unscaledDeltaTime;
			this.outputTouch.tapCount = 0;
	
			if (this.touch.JustPressedRaw())
				{
				this.outputTouch.phase = TouchPhase.Began;
				}
			else if (this.touch.JustReleasedRaw())
				{
				this.outputTouch.phase = TouchPhase.Ended;
				}
			else if (this.touch.PressedRaw())
				{
				this.outputTouch.phase = (this.touch.GetDeltaVecRaw().sqrMagnitude > 0.001f) ? TouchPhase.Moved : TouchPhase.Stationary;
				}
			else 
				{
				this.outputTouch.phase = TouchPhase.Canceled;	// ??
				}

			this.outputTouch.position =
			this.outputTouch.rawPosition = this.touch.GetCurPosRaw();
			this.outputTouch.deltaPosition = this.touch.GetDeltaVecRaw();
			}



		// ------------------
		public void Update()
			{

			// End touch if	it hasn't been updated this frame...
		
			if (this.isUsed && !this.updatedThisFrame)
				{
#if UNITY_EDITOR
				Debug.LogError("Ending inactive emu-touch #" + this.fingerId);
#endif
				this.EndTouch(false);	// TODO : cancel or not?
				}

			// Update...

			this.touch.Update();
			this.SyncOutputTouch();

			this.updatedThisFrame = false;
			}

		}



#if UNITY_EDITOR
	// -------------------
	
#endif

	// -----------------
	public static KeyCode NameToKeyCode(string keyName) 
		{
		if (keyName.Length == 1)
			{
			char c = keyName[0];

			if ((c >= 'a') && (c <= 'z'))
				return (KeyCode.A + (int)(c - 'a'));

			else if ((c >= 'A') && (c <= 'Z'))
				return (KeyCode.A + (int)(c - 'A'));

			else if ((c >= '0') && (c <= '9'))
				return (KeyCode.Alpha0 + (int)(c - '0'));
			}

		System.StringComparison cmp = System.StringComparison.OrdinalIgnoreCase;

		if (keyName.Equals("enter",		cmp)) return KeyCode.Return;
		if (keyName.Equals("return",		cmp)) return KeyCode.Return;

		if (keyName.Equals("space",		cmp)) return KeyCode.Space;
		if (keyName.Equals("spacebar",	cmp)) return KeyCode.Space;
		if (keyName.Equals(" ",				cmp)) return KeyCode.Space;

		if (keyName.Equals("esc",			cmp)) return KeyCode.Escape;
		if (keyName.Equals("escape",		cmp)) return KeyCode.Escape;

		if (keyName.Equals("left",			cmp)) return KeyCode.LeftArrow;
		if (keyName.Equals("right",		cmp)) return KeyCode.RightArrow;
		if (keyName.Equals("up",			cmp)) return KeyCode.UpArrow;
		if (keyName.Equals("down",			cmp)) return KeyCode.DownArrow;

		if (keyName.Equals("Left Arrow",			cmp)) return KeyCode.LeftArrow;
		if (keyName.Equals("Right Arrow",		cmp)) return KeyCode.RightArrow;
		if (keyName.Equals("Up Arrow",			cmp)) return KeyCode.UpArrow;
		if (keyName.Equals("Down Arrow",			cmp)) return KeyCode.DownArrow;

		if (keyName.Equals("Arrow Left",			cmp)) return KeyCode.LeftArrow;
		if (keyName.Equals("Arrow Right",		cmp)) return KeyCode.RightArrow;
		if (keyName.Equals("Arrow Up",			cmp)) return KeyCode.UpArrow;
		if (keyName.Equals("Arrow Down",			cmp)) return KeyCode.DownArrow;

		if (keyName.Equals("Page Down",	cmp)) return KeyCode.PageDown;
		if (keyName.Equals("PageDown",	cmp)) return KeyCode.PageDown;
		if (keyName.Equals("PgDwn",		cmp)) return KeyCode.PageDown;

		if (keyName.Equals("Page Up",		cmp)) return KeyCode.PageUp;
		if (keyName.Equals("PageUp",		cmp)) return KeyCode.PageUp;
		if (keyName.Equals("PgUp",			cmp)) return KeyCode.PageUp;
 

		if (keyName.Equals("alt",				cmp)) return KeyCode.LeftAlt;
		if (keyName.Equals("L alt",			cmp)) return KeyCode.LeftAlt;
		if (keyName.Equals("Left alt",		cmp)) return KeyCode.LeftAlt;
		if (keyName.Equals("R alt",			cmp)) return KeyCode.RightAlt;
		if (keyName.Equals("Right alt",		cmp)) return KeyCode.RightAlt;

		if (keyName.Equals("control",		cmp)) return KeyCode.LeftControl;
		if (keyName.Equals("L control",		cmp)) return KeyCode.LeftControl;
		if (keyName.Equals("Left control",	cmp)) return KeyCode.LeftControl;
		if (keyName.Equals("R control",		cmp)) return KeyCode.RightControl;
		if (keyName.Equals("Right control",	cmp)) return KeyCode.RightControl;

		if (keyName.Equals("ctrl",		cmp)) return KeyCode.LeftControl;
		if (keyName.Equals("L ctrl",		cmp)) return KeyCode.LeftControl;
		if (keyName.Equals("Left ctrl",	cmp)) return KeyCode.LeftControl;
		if (keyName.Equals("R ctrl",		cmp)) return KeyCode.RightControl;
		if (keyName.Equals("Right ctrl",	cmp)) return KeyCode.RightControl;

			
		if (keyName.Equals("shift",			cmp)) return KeyCode.LeftShift;
		if (keyName.Equals("L shift",		cmp)) return KeyCode.LeftShift;
		if (keyName.Equals("Left shift",	cmp)) return KeyCode.LeftShift;
		if (keyName.Equals("R shift",		cmp)) return KeyCode.RightShift;
		if (keyName.Equals("Right shift",	cmp)) return KeyCode.RightShift;

		if (keyName.Equals("Caps Lock",	cmp)) return KeyCode.CapsLock;
		if (keyName.Equals("CapsLock",	cmp)) return KeyCode.CapsLock;
		if (keyName.Equals("Caps",			cmp)) return KeyCode.CapsLock;

		if (keyName.Equals("tab",			cmp)) return KeyCode.Tab;

		if (keyName.Equals("/",				cmp)) return KeyCode.Backslash;
		if (keyName.Equals("backslash",	cmp)) return KeyCode.Backslash;
		if (keyName.Equals("\\",			cmp)) return KeyCode.Slash;
		if (keyName.Equals("slash",		cmp)) return KeyCode.Slash;
		
		if (keyName.Equals("[",		cmp)) return KeyCode.LeftBracket;
		if (keyName.Equals("]",		cmp)) return KeyCode.RightBracket;

		if (keyName.Equals(".",		cmp)) return KeyCode.Comma;
		if (keyName.Equals(",",		cmp)) return KeyCode.Colon;
		if (keyName.Equals("'",		cmp)) return KeyCode.Quote;
		if (keyName.Equals(";",		cmp)) return KeyCode.Semicolon;
			
				
		if (keyName.Equals("mouse 0",		cmp)) return KeyCode.Mouse0;
		if (keyName.Equals("mouse 1",		cmp)) return KeyCode.Mouse1;
		if (keyName.Equals("mouse 2",		cmp)) return KeyCode.Mouse2;
		if (keyName.Equals("mouse 3",		cmp)) return KeyCode.Mouse3;
		if (keyName.Equals("mouse 4",		cmp)) return KeyCode.Mouse4;
		if (keyName.Equals("left mouse",	cmp)) return KeyCode.Mouse0;
		if (keyName.Equals("right mouse",	cmp)) return KeyCode.Mouse1;
		if (keyName.Equals("LMB",			cmp)) return KeyCode.Mouse0;
		if (keyName.Equals("RMB",			cmp)) return KeyCode.Mouse1;
		if (keyName.Equals("MMB",			cmp)) return KeyCode.Mouse2;

		if (keyName.Equals("F1",			cmp)) return KeyCode.F1;
		if (keyName.Equals("F2",			cmp)) return KeyCode.F2;
		if (keyName.Equals("F3",			cmp)) return KeyCode.F3;
		if (keyName.Equals("F4",			cmp)) return KeyCode.F4;
		if (keyName.Equals("F5",			cmp)) return KeyCode.F5;
		if (keyName.Equals("F6",			cmp)) return KeyCode.F6;
		if (keyName.Equals("F7",			cmp)) return KeyCode.F7;
		if (keyName.Equals("F8",			cmp)) return KeyCode.F8;
		if (keyName.Equals("F9",			cmp)) return KeyCode.F9;
		if (keyName.Equals("F10",			cmp)) return KeyCode.F10;
		if (keyName.Equals("F11",			cmp)) return KeyCode.F11;
		if (keyName.Equals("F12",			cmp)) return KeyCode.F12;
	
		

		return KeyCode.None;
		}


	// ---------------------
	[System.Serializable]
	public class RigSwitchCollection : NamedConfigCollection<RigSwitch>
		{	
		private bool _changed;
		public bool changed { get { return this._changed; }  set { this._changed = value; }  }
			
		//private int mask;


		// ----------------
		public RigSwitchCollection(InputRig rig) : base(rig, 4) 
			{
			}

	
		// ------------------------
		public RigSwitch Add(string name, bool undoable)
			{
			RigSwitch rigFlag = this.Get(name);
			if (rigFlag != null)
				return rigFlag;
					
#if UNITY_EDITOR
			if (undoable)
				CFGUI.CreateUndo("Create \"" + name + "\" rig switch", this.rig);
#endif
			rigFlag = new RigSwitch();
			rigFlag.name = name;
	
			this.list.Add(rigFlag);
	
#if UNITY_EDITOR
			if (undoable)
				CFGUI.EndUndo(this.rig);
#endif
			return rigFlag;
			}



		// ---------------------
		public void SetSwitchState(string name, ref int cachedId, bool state)
			{
			RigSwitch s = this.Get(name, ref cachedId);
			if (s == null)
				return;

			if (s.GetState() != state)
				this.changed = true;

			s.SetState(state);
			}

		public void SetSwitchState(string name, bool state)
			{ int id = 0; this.SetSwitchState(name, ref id, state); }



		// --------------------
		public bool GetSwitchState(string name, ref int cachedId, bool fallbackValue)
			{
			RigSwitch s = this.Get(name, ref cachedId);
			return ((s == null) ? fallbackValue : s.GetState());
			}

		public bool GetSwitchState(string name, bool fallbackValue)
			{
			RigSwitch s = this.Get(name);
			return ((s == null) ? fallbackValue : s.GetState());
			}

			

		// --------------------
		public bool ToggleSwitchState(string name, ref int cachedId, bool fallbackValue)
			{
			RigSwitch s = this.Get(name, ref cachedId);
			if (s == null) 
				return fallbackValue;
				
			this.changed = true;

			return (s.ToggleState());
			}
			
		public bool ToggleSwitchState(string name, bool fallbackValue)
			{ int id = 0; return (this.ToggleSwitchState(name, ref id, fallbackValue)); }
			


				
		// -----------------
		public void SetAll(bool state)
			{
			for (int i = 0; i < this.list.Count; ++i)
				{
				RigSwitch s = this.list[i];
				if (s.GetState() != state)
					{
					this.changed = true;
					s.SetState(state);
					}
				}
			}

			
		// -------------------
		public void ResetSwitch(string name, ref int cachedId)
			{
			RigSwitch s = this.Get(name, ref cachedId);
			if (s == null)
				return;

			if (s.GetState() != s.defaultState)
				{
				this.changed = true;
				s.SetState(s.defaultState);
				}		
			}
	
		public void ResetSwitch(string name)
			{ int id = 0; this.ResetSwitch(name, ref id); }
	
	
		// ------------------
		public void Reset()
			{
			for (int i = 0; i < this.list.Count; ++i)
				{
				RigSwitch s = this.list[i];
				if (s.GetState() != s.defaultState)
					{
					this.changed = true;
					s.SetState(s.defaultState);
					}	
				}
			}




		// -----------------
		public int GetSwitchId(string name)
			{
			int id = -1;
			RigSwitch s = this.Get(name, ref id);
			return ((s == null) ? -1 : id);
			}
			

		}


	// ----------------------
	[System.Serializable]
	public class RigSwitch : NamedConfigElement
		{
		public bool defaultState;		
		private bool state;
			
			
		// -----------------
		public RigSwitch() : base()		
			{
			}

		// ----------------
		public RigSwitch(string name) : base()
			{
			this.name = name;
			}
		
			
		// ------------------
		public void SetState(bool state)	{ this.state = state; }

		// ---------------
		public bool GetState()				{ return this.state; }

		// ---------------
		public bool ToggleState()			{ return (this.state = !this.state); }

		// -----------------
		override public void Reset()
			{
			this.state = this.defaultState;
			}
		}	





		

	// ------------------------
	[System.Serializable]
	public class GamepadConfig : IBindingContainer
		{
		public bool enabled;

		public DigitalBinding
			digiFaceDownBinding,
			digiFaceRightBinding,
			digiFaceLeftBinding,
			digiFaceUpBinding,
			digiStartBinding,	
			digiSelectBinding,
			digiL1Binding,
			digiR1Binding,
			digiL2Binding,
			digiR2Binding,
			digiL3Binding,
			digiR3Binding;

		public AxisBinding
			analogL2Binding,
			analogR2Binding;

		public JoystickStateBinding
			leftStickStateBinding,
			rightStickStateBinding,
			dpadStateBinding;
					
		[System.NonSerialized]
		private  JoystickState
			leftStickState,
			rigthStickState,
			dpadState;



		// --------------------
		private void BasicConstructor(InputRig rig)
			{
			this.enabled =  false;

			this.digiFaceDownBinding	= new DigitalBinding();
			this.digiFaceRightBinding	= new DigitalBinding();
			this.digiFaceLeftBinding	= new DigitalBinding();
			this.digiFaceUpBinding		= new DigitalBinding();
			this.digiStartBinding		= new DigitalBinding();
			this.digiSelectBinding		= new DigitalBinding();
			this.digiL1Binding			= new DigitalBinding();
			this.digiR1Binding			= new DigitalBinding();
			this.digiL2Binding			= new DigitalBinding();
			this.digiR2Binding			= new DigitalBinding();
			this.digiL3Binding			= new DigitalBinding();
			this.digiR3Binding			= new DigitalBinding();

			this.analogL2Binding		= new AxisBinding();
			this.analogR2Binding		= new AxisBinding();
		
				
			this.leftStickStateBinding	= new JoystickStateBinding();
			this.rightStickStateBinding	= new JoystickStateBinding();
			this.dpadStateBinding		= new JoystickStateBinding();
			
			}

		// -------------------
		public GamepadConfig(InputRig rig) : base()
			{
			this.BasicConstructor(rig);
			}
			

		// -------------------
		public void SyncGamepad(GamepadManager.Gamepad gamepad, InputRig rig)
			{
			if (!this.enabled || (gamepad == null)) // || !gamepad.IsConnected())
				return;
	
			gamepad.leftStick	.SyncJoyState(this.leftStickStateBinding, rig);
			gamepad.rightStick.SyncJoyState(this.rightStickStateBinding, rig);
			gamepad.dpad		.SyncJoyState(this.dpadStateBinding, rig);

			gamepad.keys[(int)GamepadManager.GamepadKey.FaceBottom].SyncDigital(this.digiFaceDownBinding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.FaceRight]	.SyncDigital(this.digiFaceRightBinding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.FaceLeft]	.SyncDigital(this.digiFaceLeftBinding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.FaceTop]	.SyncDigital(this.digiFaceUpBinding, rig);
				
			gamepad.keys[(int)GamepadManager.GamepadKey.Start]		.SyncDigital(this.digiStartBinding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.Select]		.SyncDigital(this.digiSelectBinding, rig);
			
			gamepad.keys[(int)GamepadManager.GamepadKey.L1]			.SyncDigital(this.digiL1Binding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.R1]			.SyncDigital(this.digiR1Binding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.L2]			.SyncDigital(this.digiL2Binding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.R2]			.SyncDigital(this.digiR2Binding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.L3]			.SyncDigital(this.digiL3Binding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.R3]			.SyncDigital(this.digiR3Binding, rig);
				
			gamepad.keys[(int)GamepadManager.GamepadKey.L2]			.SyncAnalog(this.analogL2Binding, rig);
			gamepad.keys[(int)GamepadManager.GamepadKey.R2]			.SyncAnalog(this.analogR2Binding, rig);
			}


		// ---------------------
		public void GetSubBindingDescriptions(
			BindingDescriptionList			descList, 
			Object 							undoObject,
			string 							parentMenuPath)
			{
			descList.Add(this.digiFaceDownBinding, 	"Bottom Face Button", parentMenuPath, undoObject);
			descList.Add(this.digiFaceRightBinding, "Right Face Button", parentMenuPath, undoObject);
			descList.Add(this.digiFaceUpBinding, 	"Top Face Button", parentMenuPath, undoObject);
			descList.Add(this.digiFaceLeftBinding, 	"Left Face Button", parentMenuPath, undoObject);

			descList.Add(this.digiL1Binding, 		"L1", parentMenuPath, undoObject);
			descList.Add(this.digiR1Binding, 		"R1", parentMenuPath, undoObject);
			descList.Add(this.digiL2Binding, 		"L2 (Digital)", parentMenuPath, undoObject);
			descList.Add(this.digiR2Binding, 		"R2 (Digital)", parentMenuPath, undoObject);
			descList.Add(this.analogL2Binding, 		"L2 (Analog)", parentMenuPath, undoObject);
			descList.Add(this.analogR2Binding, 		"R2 (Analog)", parentMenuPath, undoObject);
			descList.Add(this.digiL3Binding, 		"L3", parentMenuPath, undoObject);
			descList.Add(this.digiR3Binding, 		"R3", parentMenuPath, undoObject);
				
			descList.Add(this.digiStartBinding,		"Start", parentMenuPath, undoObject);
			descList.Add(this.digiSelectBinding,	"Select (Back)", parentMenuPath, undoObject);

			descList.Add(this.dpadStateBinding,			"D-Pad State", parentMenuPath, undoObject);
			descList.Add(this.leftStickStateBinding,	"Left Stick State", parentMenuPath, undoObject);
			descList.Add(this.rightStickStateBinding,	"Right Stick State", parentMenuPath, undoObject);
			}
			
	
		// -----------------------
		public bool IsBoundToKey(KeyCode key, InputRig rig)
			{
			return (
				this.digiFaceDownBinding	.IsBoundToKey(key, rig) ||
				this.digiFaceRightBinding	.IsBoundToKey(key, rig) ||
				this.digiFaceUpBinding		.IsBoundToKey(key, rig) ||
				this.digiFaceLeftBinding	.IsBoundToKey(key, rig) ||
				this.digiL1Binding			.IsBoundToKey(key, rig) ||
				this.digiR1Binding			.IsBoundToKey(key, rig) ||
				this.digiL2Binding			.IsBoundToKey(key, rig) ||
				this.digiR2Binding			.IsBoundToKey(key, rig) ||
				this.analogL2Binding			.IsBoundToKey(key, rig) ||
				this.analogR2Binding			.IsBoundToKey(key, rig) ||
				this.digiL3Binding			.IsBoundToKey(key, rig) ||
				this.digiR3Binding			.IsBoundToKey(key, rig) ||
				this.digiStartBinding		.IsBoundToKey(key, rig) ||
				this.digiSelectBinding		.IsBoundToKey(key, rig) ||
				this.dpadStateBinding		.IsBoundToKey(key, rig) ||
				this.leftStickStateBinding	.IsBoundToKey(key, rig) ||
				this.rightStickStateBinding.IsBoundToKey(key, rig) );
			}
	
		// ---------------------
		public bool IsBoundToAxis			(string axisName, InputRig rig)
			{
			return (
				this.digiFaceDownBinding	.IsBoundToAxis(axisName, rig) ||
				this.digiFaceRightBinding	.IsBoundToAxis(axisName, rig) ||
				this.digiFaceUpBinding		.IsBoundToAxis(axisName, rig) ||
				this.digiFaceLeftBinding	.IsBoundToAxis(axisName, rig) ||
				this.digiL1Binding			.IsBoundToAxis(axisName, rig) ||
				this.digiR1Binding			.IsBoundToAxis(axisName, rig) ||
				this.digiL2Binding			.IsBoundToAxis(axisName, rig) ||
				this.digiR2Binding			.IsBoundToAxis(axisName, rig) ||
				this.analogL2Binding			.IsBoundToAxis(axisName, rig) ||
				this.analogR2Binding			.IsBoundToAxis(axisName, rig) ||
				this.digiL3Binding			.IsBoundToAxis(axisName, rig) ||
				this.digiR3Binding			.IsBoundToAxis(axisName, rig) ||
				this.digiStartBinding		.IsBoundToAxis(axisName, rig) ||
				this.digiSelectBinding		.IsBoundToAxis(axisName, rig) ||
				this.dpadStateBinding		.IsBoundToAxis(axisName, rig) ||
				this.leftStickStateBinding	.IsBoundToAxis(axisName, rig) ||
				this.rightStickStateBinding.IsBoundToAxis(axisName, rig) );
			}
	
		// ---------------------
		public bool IsEmulatingTouches		()
			{
			return (
				this.digiFaceDownBinding	.IsEmulatingTouches() ||
				this.digiFaceRightBinding	.IsEmulatingTouches() ||
				this.digiFaceUpBinding		.IsEmulatingTouches() ||
				this.digiFaceLeftBinding	.IsEmulatingTouches() ||
				this.digiL1Binding			.IsEmulatingTouches() ||
				this.digiR1Binding			.IsEmulatingTouches() ||
				this.digiL2Binding			.IsEmulatingTouches() ||
				this.digiR2Binding			.IsEmulatingTouches() ||
				this.analogL2Binding			.IsEmulatingTouches() ||
				this.analogR2Binding			.IsEmulatingTouches() ||
				this.digiL3Binding			.IsEmulatingTouches() ||
				this.digiR3Binding			.IsEmulatingTouches() ||
				this.digiStartBinding		.IsEmulatingTouches() ||
				this.digiSelectBinding		.IsEmulatingTouches() ||
				this.dpadStateBinding		.IsEmulatingTouches() ||
				this.leftStickStateBinding	.IsEmulatingTouches() ||
				this.rightStickStateBinding.IsEmulatingTouches() );
			}
	
		// ---------------------
		public bool IsEmulatingMousePosition()
			{
			return (
				this.digiFaceDownBinding	.IsEmulatingMousePosition() ||
				this.digiFaceRightBinding	.IsEmulatingMousePosition() ||
				this.digiFaceUpBinding		.IsEmulatingMousePosition() ||
				this.digiFaceLeftBinding	.IsEmulatingMousePosition() ||
				this.digiL1Binding			.IsEmulatingMousePosition() ||
				this.digiR1Binding			.IsEmulatingMousePosition() ||
				this.digiL2Binding			.IsEmulatingMousePosition() ||
				this.digiR2Binding			.IsEmulatingMousePosition() ||
				this.analogL2Binding			.IsEmulatingMousePosition() ||
				this.analogR2Binding			.IsEmulatingMousePosition() ||
				this.digiL3Binding			.IsEmulatingMousePosition() ||
				this.digiR3Binding			.IsEmulatingMousePosition() ||
				this.digiStartBinding		.IsEmulatingMousePosition() ||
				this.digiSelectBinding		.IsEmulatingMousePosition() ||
				this.dpadStateBinding		.IsEmulatingMousePosition() ||
				this.leftStickStateBinding	.IsEmulatingMousePosition() ||
				this.rightStickStateBinding.IsEmulatingMousePosition() );
			}
	

		}




	// ------------------------
	// Virtual Joystick Config class.		
	// ------------------------
	[System.Serializable]
	public class VirtualJoystickConfig : NamedConfigElement, IBindingContainer
		{
		public bool
			disableOnMobile;

		public JoystickConfig	
			joystickConfig;
			
		public JoystickState
			joystickState;

		public KeyCode 
			keyboardUp,
			keyboardRight,
			keyboardDown,
			keyboardLeft;
	

		public JoystickStateBinding
			joyStateBinding;

		// --------------------
		private void BasicConstructor()
			{
			this.name = "Joystick";
			
			this.keyboardUp		= KeyCode.W;
			this.keyboardRight	= KeyCode.D;
			this.keyboardDown	= KeyCode.S;
			this.keyboardLeft	= KeyCode.A;

			if (this.joystickConfig == null)
				this.joystickConfig = new JoystickConfig();
				
			this.joystickState = new JoystickState(this.joystickConfig);


			this.joyStateBinding = new JoystickStateBinding();

		
			}

		// -------------------
		public VirtualJoystickConfig() : base()
			{
			this.BasicConstructor();
			}


		// ----------------------
		public VirtualJoystickConfig(string targetName, KeyCode keyUp, KeyCode keyRight, KeyCode keyDown, KeyCode keyLeft) : base()
			{
			this.BasicConstructor();

			this.name = targetName;
				
			this.keyboardUp		= keyUp;
			this.keyboardRight	= keyRight;
			this.keyboardDown	= keyDown;
			this.keyboardLeft	= keyLeft;

			}
			


		// -----------------------
		override public void Reset()
			{
			this.joystickState.Reset();
			}


		// ----------------------
		override public void Update(InputRig rig)
			{
			this.joystickState.ApplyDigital(
				GetSourceKeyState(this.keyboardUp),
				GetSourceKeyState(this.keyboardRight),
				GetSourceKeyState(this.keyboardDown),
				GetSourceKeyState(this.keyboardLeft));

			this.joystickState.Update();

			this.joyStateBinding.SyncJoyState(this.joystickState, rig);
			}
		
		
		// --------------------
		public void SetState(JoystickState state)
			{
			if (this.joystickState != null)
				this.joystickState.ApplyState(state);
			}
			


		// -------------------
		public bool IsBoundToAxis(string axisName, InputRig rig)
			{ 
			return (this.joyStateBinding.IsBoundToAxis(axisName, rig) );
			}
	

		// ----------------------
		public bool IsBoundToKey(KeyCode key, InputRig rig)
			{ 
			return (
				this.joyStateBinding.IsBoundToKey(key, rig) );
			}
	
		// ------------
		public bool IsEmulatingTouches()		{ return false; }
		public bool IsEmulatingMousePosition()	{ return false; }

	

		// ----------------------
		public void GetSubBindingDescriptions(BindingDescriptionList descList,
			Object undoObject, string parentMenuPath) 
			{
			descList.Add(this.joyStateBinding, "Joystick State", parentMenuPath, undoObject);
	
			}


		}





	// ------------------
	[System.Serializable]
	public class VirtualJoystickConfigCollection : NamedConfigCollection<VirtualJoystickConfig>
		{
		public VirtualJoystickConfigCollection(InputRig rig, int capacity) : base(rig, capacity) { }


			
		// ------------------------
		public VirtualJoystickConfig Add(string name, bool undoable)
			{
			VirtualJoystickConfig joy = this.Get(name);
			if (joy != null)
				return joy;
					
#if UNITY_EDITOR
			if (undoable)
				CFGUI.CreateUndo("Create \"" + name + "\" joy config", this.rig);
#endif
			joy = new VirtualJoystickConfig();
			joy.name = name;
				
			this.list.Add(joy);
	
#if UNITY_EDITOR
			if (undoable)
				CFGUI.EndUndo(this.rig);
#endif
			return joy;
			}
	
	
	
		
		}
		


		
	// ----------------------	
	public enum AxisType
		{
		UnsignedAnalog,//!< Clamped analog axis (0..1). 
		SignedAnalog,	//!< Clamped signed analog axis (-1..1).
		Digital,			//!< Digital button.
		ScrollWheel,	//!< Scroll Wheel-like axis (integer values).
		Delta				//!< Delta axis (eg. mouse).
		}
		


	public enum InputSource
		{
		Digital,
		Analog,
		MouseDelta,
		TouchDelta,
		NormalizedDelta,
		Scroll
		}

	public enum DeltaTransformMode	
		{
		Universal,		//!< Universal mode for consistent delta values across different input sources - mouse, touchscreen, joystick, keyboard.
		EmulateMouse,	//!< Emulates mouse delta on virtual screen of specified resolution.	
		Raw				//!< No transform. (Not recommended!)
		}

	// --------------------------
	[System.Serializable]
	public class AxisConfig : NamedConfigElement
		{	
		
		public AxisType
			axisType;

		public DeltaTransformMode
			deltaMode;
		
		public bool
			snap;

		public float 	
			scale;
			
		public bool
			affectSourceKeys;

		public KeyCode
			affectedKeyPositive,
			affectedKeyNegative;

		public KeyCode 
			keyboardPositive,
			keyboardPositiveAlt0,
			keyboardPositiveAlt1,
			keyboardPositiveAlt2,
			keyboardNegative,
			keyboardNegativeAlt0,
			keyboardNegativeAlt1,
			keyboardNegativeAlt2;
			
		public float
			analogToDigitalThresh,
			rawSmoothingTime,
			smoothingTime;
		
		public float 
			digitalToAnalogAccelTime,
			digitalToAnalogDecelTime;

		public bool 
			digitalToScrollAutoRepeat;
		public float
			digitalToScrollRepeatInterval,
			digitalToScrollDelay;

		public float
			scrollToAnalogSmoothingTime,
			scrollToAnalogStepDuration,

			scrollToDeltaSmoothingTime;

		private float 
			valRaw,
			val;

		private int 
			scrollPrev,
			scrollCur;
			
		private bool  
			digiCur,
			digiPrev;
	
		// Helper vars...

		private bool
			muteUntilRelease;
	
		private float 
			digitalToAnalogVal;

		private bool
			analogToDigitalNegCur,
			analogToDigitalPosCur;

		private float
			valSmoothVel,
			valRawSmoothVel;

		private float
			scrollToAnalogTimeRemaining,
			scrollToAnalogValue;

		private int 
			scrollToDigitalClicksRemaining;
		private bool
			scrollToDigitalClickOn;

		
		private float 
			deltaAccumTarget,
			
			deltaAccumSmoothCur,
			deltaAccumSmoothPrev,

			deltaAccumRawCur,	
			deltaAccumRawPrev;


		private float
			scrollElapsedSinceChange;	
		private bool	
			digiToScrollPositivePrev,	
			digiToScrollNegativePrev,
			digiToScrollDelayPhase;	
	
		// Input collected this frame...

		//private bool
		//	frDigitalPos,
		//	frDigitalNeg;
		
		//private float
		//	frAnalogPos,
		//	frAnalogNeg,
		//	frMouseDeltaPos,
		//	frMouseDeltaNeg,
		//	frTouchDeltaPos,
		//	frTouchDeltaNeg,
		//	frNormalizedDeltaPos,
		//	frNormalizedDeltaNeg;
		
		//private int	
		//	frScrollDelta;

		public bool		frDigitalPos		{ get; protected set; }
		public bool		frDigitalNeg		{ get; protected set; }
		
		public float	frAnalogPos			{ get; protected set; }
		public float	frAnalogNeg			{ get; protected set; }
		public float	frMouseDeltaPos	{ get; protected set; }
		public float	frMouseDeltaNeg	{ get; protected set; }
		public float	frTouchDeltaPos	{ get; protected set; }
		public float	frTouchDeltaNeg	{ get; protected set; }
		public float	frNormalizedDeltaPos { get; protected set; }
		public float	frNormalizedDeltaNeg { get; protected set; }

		public int		frScrollDelta		{ get; protected set; }


		

			
		// ---------------------
		public AxisConfig() : base()
			{
			this.name					= "Axis";
			this.axisType				= InputRig.AxisType.Digital;
			this.deltaMode				= InputRig.DeltaTransformMode.EmulateMouse;
			this.keyboardPositive	= KeyCode.None;
			this.keyboardNegative	= KeyCode.None;
			this.rawSmoothingTime	= 0.0f;
			this.smoothingTime		= 0.0f;
			this.analogToDigitalThresh			= 0.125f;
			this.digitalToAnalogAccelTime		= 0.25f;
			this.digitalToAnalogDecelTime		= 0.25f;
			this.scrollToAnalogSmoothingTime	= 0.1f;
			this.scale 								= 1;				
			this.affectSourceKeys				= true;

			this.digitalToScrollRepeatInterval	= 0.5f;
			this.digitalToScrollDelay			= 1.0f;
			this.digitalToScrollAutoRepeat		= true;

			this.scrollToAnalogStepDuration		= 0.1f;
			this.scrollToAnalogSmoothingTime 	= 0.05f;

			this.scrollToDeltaSmoothingTime	= 0.1f;

			this.Reset();
			}

		// ---------------------
		static public AxisConfig CreateDigital(string name, KeyCode key)
			{
			AxisConfig o = new AxisConfig();

			o.name				= name;
			o.axisType 			= InputRig.AxisType.Digital;
			o.keyboardPositive	= key;
			o.keyboardNegative	= KeyCode.None;

			return o;
			}
			
		// ---------------------
		static public AxisConfig CreateScrollWheel(string name, KeyCode keyPositive, KeyCode keyNegative)
			{
			AxisConfig o = new AxisConfig();

			o.name				= name;
			o.axisType 			= InputRig.AxisType.ScrollWheel;
			o.keyboardPositive	= keyPositive;
			o.keyboardNegative	= keyNegative;

			return o;
			}
			
		// ---------------------
		static public AxisConfig CreateAnalog(string name, KeyCode keyPositive)
			{
			AxisConfig o = new AxisConfig();

			o.name				= name;
			o.axisType 			= InputRig.AxisType.UnsignedAnalog;
			o.keyboardPositive	= keyPositive;
			o.keyboardNegative	= KeyCode.None;

			return o;
			}
			
		// ---------------------
		static public AxisConfig CreateSignedAnalog(string name, KeyCode keyPositive, KeyCode keyNegative)
			{
			AxisConfig o = new AxisConfig();

			o.name				= name;
			o.axisType 			= InputRig.AxisType.SignedAnalog;
			o.keyboardPositive	= keyPositive;
			o.keyboardNegative	= keyNegative;

			return o;
			}
			
		// ---------------------
		static public AxisConfig CreateDelta(string name, KeyCode keyPositive, KeyCode keyNegative)
			{
			AxisConfig o = new AxisConfig();

			o.name				= name;
			o.axisType 			= InputRig.AxisType.Delta;
			o.keyboardPositive	= keyPositive;
			o.keyboardNegative	= keyNegative;

			return o;
			}
			


		// -----------------------
		override public void Reset()
			{
			
			this.val = 0;
			this.valRaw = 0;
			this.valSmoothVel = 0;
			this.valRawSmoothVel = 0;
				
			this.frAnalogPos		= 0;
			this.frAnalogNeg		= 0;
			this.frDigitalNeg		= false;
			this.frDigitalPos		= false;
			this.frMouseDeltaPos	= 0;
			this.frMouseDeltaNeg	= 0;
			this.frTouchDeltaPos	= 0;
			this.frTouchDeltaNeg	= 0;
			this.frNormalizedDeltaPos = 0;
			this.frNormalizedDeltaNeg = 0;
				
			this.deltaAccumTarget		= 0;
			this.deltaAccumSmoothCur	= 0;
			this.deltaAccumSmoothPrev	= 0;
			this.deltaAccumRawCur		= 0;
			this.deltaAccumRawPrev		= 0;

			this.digiToScrollPositivePrev	= false;
			this.digiToScrollNegativePrev	= false;
			this.digiToScrollDelayPhase		= true;
			this.scrollElapsedSinceChange	= 0;
			}

			

		// ------------------
		public void MuteUntilRelease()
			{
			this.muteUntilRelease = true;
			}


		// -------------
		public bool IsMuted()
			{ return this.muteUntilRelease; }
	
	

		// ----------------------
		public void ApplyKeyboardInput()
			{
			if (GetSourceKeyState(this.keyboardPositive) || 
				 GetSourceKeyState(this.keyboardPositiveAlt0) ||
				 GetSourceKeyState(this.keyboardPositiveAlt1) ||
				 GetSourceKeyState(this.keyboardPositiveAlt2) )
				this.SetSignedDigital(true);

			if (GetSourceKeyState(this.keyboardNegative) || 
				 GetSourceKeyState(this.keyboardNegativeAlt0) ||
				 GetSourceKeyState(this.keyboardNegativeAlt1) ||
				 GetSourceKeyState(this.keyboardNegativeAlt2) )
				this.SetSignedDigital(false);
			}

		// ----------------------
		override public void Update(InputRig rig)
			{
			// Read keyboard state...


				
			// Update...

			this.scrollPrev				= this.scrollCur;
			this.digiPrev				= this.digiCur;
		

			// Convert analog to digital...
			
			this.analogToDigitalNegCur = ((this.frAnalogPos + this.frAnalogNeg) < -this.analogToDigitalThresh);
			this.analogToDigitalPosCur = ((this.frAnalogPos + this.frAnalogNeg) > this.analogToDigitalThresh);
				
			// Convert digital to analog...

			float digiToAnalogTarget = (this.frDigitalNeg ? -1 : 0) + (this.frDigitalPos ? 1 : 0);
			float digiToAnalogTime = ((this.frDigitalNeg || this.frDigitalPos) ? this.digitalToAnalogAccelTime : this.digitalToAnalogDecelTime); // * DIGITAL_ACCEL_MAX_TIME;
				

			// Snap...

			if (this.snap && (Mathf.Abs(digiToAnalogTarget) > 0.1f) && ((digiToAnalogTarget >= 0) != (this.digitalToAnalogVal >= 0)))
				this.digitalToAnalogVal = 0;

			this.digitalToAnalogVal = CFUtils.MoveTowards(this.digitalToAnalogVal, digiToAnalogTarget, digiToAnalogTime, CFUtils.realDeltaTime, 0.01f);
				

			float rawAnalogVal = CFUtils.ApplyDeltaInput((this.frAnalogNeg + this.frAnalogPos), this.digitalToAnalogVal);
			rawAnalogVal = Mathf.Clamp(rawAnalogVal, -1, 1);

			
				




			// Compute output analog and digital state according to axis type...

			switch (this.axisType)
				{
				// Digital Axis -------------------

				case AxisType.Digital :

					this.scrollToDigitalClicksRemaining += this.frScrollDelta;

					if (this.scrollToDigitalClickOn)
						this.scrollToDigitalClickOn = false;

					else if (this.scrollToDigitalClicksRemaining != 0)
						{
						this.scrollToDigitalClickOn = true;
						if (this.scrollToDigitalClicksRemaining > 0)
							{
							--this.scrollToDigitalClicksRemaining;
							this.frDigitalPos = true;
							}
						else
							{
							++this.scrollToDigitalClicksRemaining;
							this.frDigitalNeg = true;
							}
						}

					this.digiCur = (this.frDigitalNeg || this.frDigitalPos || this.analogToDigitalNegCur || this.analogToDigitalPosCur || this.scrollToDigitalClickOn);
					this.val = this.valRaw = (this.digiCur ? 1 : 0); //((this.frDigitalPos || this.analogToDigitalPosCur) ? 1 : 0) + ((this.frDigitalNeg || this.analogToDigitalNegCur) ? -1 : 0);
					break;



				// Analog axis ------------------------

				case AxisType.UnsignedAnalog :	
				case AxisType.SignedAnalog :
					
					if (this.scrollToAnalogTimeRemaining != 0)
						this.scrollToAnalogTimeRemaining = Mathf.MoveTowards(this.scrollToAnalogTimeRemaining, 0, CFUtils.realDeltaTimeClamped);

					if (this.frScrollDelta != 0)
						this.scrollToAnalogTimeRemaining += ((float)this.frScrollDelta * this.scrollToAnalogStepDuration);

					if ((this.scrollToAnalogTimeRemaining != 0) || (this.scrollToAnalogValue != 0))
						{
						this.scrollToAnalogValue = CFUtils.SmoothTowards(this.scrollToAnalogValue,  
							((this.scrollToAnalogTimeRemaining == 0) ? 0 : (this.scrollToAnalogTimeRemaining < 0) ? -1 : 1), 
							this.scrollToAnalogSmoothingTime, CFUtils.realDeltaTimeClamped, 0.001f);

						rawAnalogVal = CFUtils.ApplyDeltaInput(rawAnalogVal, this.scrollToAnalogValue);
						}

					this.digiCur = (this.frDigitalNeg || this.frDigitalPos || this.analogToDigitalNegCur || this.analogToDigitalPosCur || 
						(this.scrollToAnalogTimeRemaining != 0));
					
					if ((this.axisType == InputRig.AxisType.UnsignedAnalog) && (rawAnalogVal < 0))
						rawAnalogVal = 0;
					
					this.valRaw = CFUtils.SmoothDamp(this.valRaw, rawAnalogVal, ref this.valRawSmoothVel, 
						this.rawSmoothingTime , CFUtils.realDeltaTime, 0.001f);
		
					this.val = CFUtils.SmoothDamp(this.val, rawAnalogVal, ref this.valSmoothVel, 
						this.smoothingTime , CFUtils.realDeltaTime, 0.001f);

					break;
	

				// Scroll Axis -------------------

				case AxisType.ScrollWheel :

					// Convert digital to scroll...

					bool 
						digiPositive = (this.frDigitalPos || this.analogToDigitalPosCur),
						digiNegative = (this.frDigitalNeg || this.analogToDigitalNegCur);
	
					int digiToScrollVal = ((digiPositive ? 1 : 0) + (digiNegative ? -1 : 0));

					if ((this.frScrollDelta != 0) || ((digiPositive != this.digiToScrollPositivePrev) || (digiNegative != this.digiToScrollNegativePrev)))
						{
						this.digiToScrollDelayPhase = true;
						this.scrollElapsedSinceChange = 0;

						if ((digiToScrollVal != 0) && (!this.digiToScrollNegativePrev && !this.digiToScrollPositivePrev))
							this.frScrollDelta = CFUtils.ApplyDeltaInputInt(this.frScrollDelta, digiToScrollVal);
						} 
					else if (this.digitalToScrollAutoRepeat)
						{
						if (digiToScrollVal != 0)
							{
							this.scrollElapsedSinceChange += CFUtils.realDeltaTimeClamped;
							if (this.scrollElapsedSinceChange > (this.digiToScrollDelayPhase ? this.digitalToScrollDelay : this.digitalToScrollRepeatInterval))
								{
								this.digiToScrollDelayPhase = false;
								this.scrollElapsedSinceChange = 0;

								this.frScrollDelta = CFUtils.ApplyDeltaInputInt(this.frScrollDelta, digiToScrollVal);
								}
							}		
						}
					
					this.digiToScrollPositivePrev = digiPositive;
					this.digiToScrollNegativePrev = digiNegative;

					// Add this frames delta...

					this.scrollCur += this.frScrollDelta;
				
					this.val		= this.valRaw = (float)(this.scrollCur - this.scrollPrev);
					this.digiCur	= (this.frScrollDelta != 0);
					break;



				// Delta Axis ------------------------

				case AxisType.Delta :

					// Combine mouse and touch deltas...

					float rawDelta = CFUtils.ApplyDeltaInput(
						rig.TransformTouchPixelDelta(this.frTouchDeltaPos + this.frTouchDeltaNeg, this.deltaMode), 
						rig.TransformMousePointDelta(this.frMouseDeltaPos + this.frMouseDeltaNeg, this.deltaMode));
					
					rawDelta = CFUtils.ApplyDeltaInput(rawDelta, 
						rig.TransformNormalizedDelta(this.frNormalizedDeltaPos + this.frNormalizedDeltaNeg, this.deltaMode));

					rawDelta = CFUtils.ApplyDeltaInput(rawDelta, 
						rig.TransformScrollDelta(this.frScrollDelta, this.deltaMode));


					// Combine with analog delta...
					
					for (int fi = 0; fi < rig.fixedTimeStep.GetFrameSteps(); ++fi)
						{
						rawDelta = CFUtils.ApplyDeltaInput(rawDelta, 	
							rig.TransformAnalogDelta(rawAnalogVal * rig.fixedTimeStep.GetDeltaTime(), this.deltaMode));
						}

					// Smooth...

					this.deltaAccumRawPrev		= this.deltaAccumRawCur;

					this.deltaAccumTarget += rawDelta;

					this.deltaAccumSmoothPrev	= this.deltaAccumSmoothCur;
					this.deltaAccumSmoothCur	= CFUtils.SmoothDamp(this.deltaAccumSmoothCur, this.deltaAccumTarget, ref this.valSmoothVel, 
						this.smoothingTime, CFUtils.realDeltaTime, 0.00001f);  

					this.deltaAccumRawPrev		= this.deltaAccumRawCur;
					this.deltaAccumRawCur		= CFUtils.SmoothDamp(this.deltaAccumRawCur, this.deltaAccumTarget, ref this.valRawSmoothVel, 
						this.rawSmoothingTime, CFUtils.realDeltaTime, 0.00001f);  
					
					this.val	= (this.deltaAccumSmoothCur - this.deltaAccumSmoothPrev);
					this.valRaw = (this.deltaAccumRawCur 	- this.deltaAccumRawPrev);
					
					break;
				}

				

			if (this.muteUntilRelease)
				{
				this.val					= 0;
				this.valRaw				= 0;
				this.valRawSmoothVel	= 0;
				this.valSmoothVel		= 0;
				this.digiCur			= false;
				this.digiPrev			= false;
				this.digitalToAnalogVal = 0;

				if (!this.frDigitalNeg && !this.frDigitalPos && !this.analogToDigitalNegCur && !this.analogToDigitalPosCur && (this.frScrollDelta == 0))
					{
					this.muteUntilRelease = false;
					}
				else
					{
					}


				if (this.axisType == InputRig.AxisType.Delta)
					this.muteUntilRelease = false;
				}

			// Set affected keys...

			if ((this.analogToDigitalNegCur || frDigitalNeg))
				{
				if (this.affectSourceKeys)
					{
					rig.SetKeyCode(this.affectedKeyNegative);
					}
				else
					{
					rig.SetKeyCode(this.keyboardNegative);
					rig.SetKeyCode(this.keyboardNegativeAlt0);
					rig.SetKeyCode(this.keyboardNegativeAlt1);
					rig.SetKeyCode(this.keyboardNegativeAlt2);
					} 
				}

			if ((this.analogToDigitalPosCur || frDigitalPos))
				{
				if (this.affectSourceKeys)
					{
					rig.SetKeyCode(this.affectedKeyPositive);
					}
				else
					{
					rig.SetKeyCode(this.keyboardPositive);
					rig.SetKeyCode(this.keyboardPositiveAlt0);
					rig.SetKeyCode(this.keyboardPositiveAlt1);
					rig.SetKeyCode(this.keyboardPositiveAlt2);
					} 
				}

				

			// Reset accumulators...

			this.frAnalogPos = 0;		
			this.frAnalogNeg = 0;		
			this.frMouseDeltaPos = 0;
			this.frMouseDeltaNeg = 0;
			this.frTouchDeltaPos = 0;
			this.frTouchDeltaNeg = 0;
			this.frNormalizedDeltaPos = 0;
			this.frNormalizedDeltaNeg = 0;
			this.frScrollDelta = 0;

			this.frDigitalPos = false;
			this.frDigitalNeg = false;	
			}
		

	

	
		// --------------
		public void Set(float v, InputSource source)
			{
			switch (source)
				{
				case InputSource.Digital : 
					if (Mathf.Abs(v) > 0.5f)
						this.SetSignedDigital((v >= 0)); 
					return;

				case InputSource.Analog :
					this.SetAnalog(v);
					return;
		
				case InputSource.MouseDelta :
					this.SetMouseDelta(v);
					return;

				case InputSource.TouchDelta :
					this.SetTouchDelta(v);
					return;

				case InputSource.NormalizedDelta :
					this.SetNormalizedDelta(v);
					return;

				case InputSource.Scroll :
					this.SetScrollDelta(Mathf.RoundToInt(v));
					return;
				}
			}
			

		// ------------
		public void SetAnalog(float v)
			{
			this.frAnalogPos = CFUtils.ApplyPositveDeltaInput(this.frAnalogPos, v);
			this.frAnalogNeg = CFUtils.ApplyNegativeDeltaInput(this.frAnalogNeg, v);
			//CFUtils.ApplySignedDeltaInput(v, ref this.frAnalogPos, ref this.frAnalogNeg);
			}

		// --------------
		public void SetDigital()
			{
			this.frDigitalPos = true;

			}

		// --------------
		public void SetSignedDigital(bool vpositive)
			{
			if (vpositive)
				this.frDigitalPos = true;	
			else
				this.frDigitalNeg = true;
			}

		// ---------------
		public void SetScrollDelta(int scrollDelta)
			{
			this.frScrollDelta = CFUtils.ApplyDeltaInputInt(this.frScrollDelta, scrollDelta);
			}		

		public void SetTouchDelta(float touchDelta)
			{
			this.frTouchDeltaPos = CFUtils.ApplyPositveDeltaInput(this.frTouchDeltaPos, touchDelta);
			this.frTouchDeltaNeg = CFUtils.ApplyNegativeDeltaInput(this.frTouchDeltaNeg, touchDelta);
			//CFUtils.ApplySignedDeltaInput(touchDelta, ref this.frTouchDeltaPos, ref this.frTouchDeltaNeg);
			}

		public void SetMouseDelta(float mouseDelta)
			{
			this.frMouseDeltaPos = CFUtils.ApplyPositveDeltaInput(this.frMouseDeltaPos, mouseDelta);
			this.frMouseDeltaNeg = CFUtils.ApplyNegativeDeltaInput(this.frMouseDeltaNeg, mouseDelta);
			//CFUtils.ApplySignedDeltaInput(mouseDelta, ref this.frMouseDeltaPos, ref this.frMouseDeltaNeg);
			}

		public void SetNormalizedDelta(float normalizedDelta)
			{
			this.frNormalizedDeltaPos = CFUtils.ApplyPositveDeltaInput(this.frNormalizedDeltaPos, normalizedDelta);
			this.frNormalizedDeltaNeg = CFUtils.ApplyNegativeDeltaInput(this.frNormalizedDeltaNeg, normalizedDelta);
			//CFUtils.ApplySignedDeltaInput(normalizedDelta, ref this.frNormalizedDeltaPos, ref this.frNormalizedDeltaNeg);
			}





		// ---------------	
		public float GetAnalog()
			{
			if (this.axisType != InputRig.AxisType.ScrollWheel)
				return (this.val * this.scale);
			else
				return this.val;
			}

		// --------------
		public float GetAnalogRaw()
			{
			if (this.axisType != InputRig.AxisType.ScrollWheel)
				return (this.valRaw * this.scale);
			else
				return this.valRaw;
			}

		// -------------------
		public bool IsControlledByInput()
			{

			return (
				this.frDigitalNeg || 
				this.frDigitalPos || 
				(Mathf.Abs(this.frAnalogNeg) > 0.001f) || 
				(Mathf.Abs(this.frAnalogPos) > 0.001f) ||
				(Mathf.Abs(this.frMouseDeltaNeg) > 0.001f) ||
				(Mathf.Abs(this.frMouseDeltaPos) > 0.001f) ||
				(Mathf.Abs(this.frNormalizedDeltaNeg) > 0.001f) ||
				(Mathf.Abs(this.frNormalizedDeltaPos) > 0.001f) ||
				(Mathf.Abs(this.frTouchDeltaNeg) > 0.001f) ||
				(Mathf.Abs(this.frTouchDeltaPos) > 0.001f) ||
				(this.frScrollDelta != 0));
			}


		// ----------------
		public bool GetButton()
			{
			return (this.digiCur);
			}

		// -------------------
		public bool GetButtonDown()
			{
			return ((!this.digiPrev && this.digiCur));
			}
	
		// ---------------
		public bool GetButtonUp()
			{
			return ((this.digiPrev && !this.digiCur));
			}
			
			
		// ------------------
		public int GetSupportedInputSourceMask()
			{
			int mask = 0;

			switch (this.axisType)
				{
				case AxisType.Digital :
					mask = ((1 << (int)InputSource.Digital) | (1 << (int)InputSource.Analog) | (1 << (int)InputSource.Scroll));
					break;

				case AxisType.UnsignedAnalog :
				case AxisType.SignedAnalog :
					mask = ((1 << (int)InputSource.Digital) | (1 << (int)InputSource.Analog) | (1 << (int)InputSource.Scroll));
					break;

				case AxisType.ScrollWheel :
					mask = ((1 << (int)InputSource.Digital) | (1 << (int)InputSource.Analog) | (1 << (int)InputSource.Scroll)) ;
					break;

				case AxisType.Delta :
					mask = ((1 << (int)InputSource.Digital) | (1 << (int)InputSource.Analog) | (1 << (int)InputSource.Scroll) |
						(1 << (int)InputSource.MouseDelta) | (1 << (int)InputSource.TouchDelta) | (1 << (int)InputSource.NormalizedDelta));
					break;					
				}


			return mask;
			}

		// ----------------
		public bool DoesSupportInputSource(InputSource source)
			{
			return ((this.GetSupportedInputSourceMask() & (1 << (int)source)) != 0);
			}


		// -----------------
		private bool IsSignedAxis()
			{
			return ((this.axisType == AxisType.Delta) || (this.axisType == AxisType.ScrollWheel) || (this.axisType == AxisType.SignedAnalog));
			}


		// ----------------
		public bool DoesAffectKeyCode(KeyCode key)
			{
			if (!this.affectSourceKeys)
				{	
				if ((this.affectedKeyPositive == key) ||
					(this.IsSignedAxis() && (this.affectedKeyNegative == key)))
					return true;
				}
			else
				{
				if ((key == this.keyboardPositive) ||
					(key == this.keyboardPositiveAlt0) ||
					(key == this.keyboardPositiveAlt1) ||
					(key == this.keyboardPositiveAlt2))
					return true;

				if (this.IsSignedAxis() && 
					((key == this.keyboardNegative) ||
					(key == this.keyboardNegativeAlt0) ||
					(key == this.keyboardNegativeAlt1) ||
					(key == this.keyboardNegativeAlt2)
					))
					return true;
				}	

			return false;
			}

		}

		
		

	// ------------------
	[System.Serializable]
	public class AxisConfigCollection : NamedConfigCollection<AxisConfig>
		{
		public AxisConfigCollection(InputRig rig, int capacity) : base(rig, capacity) 	{ }



	
		// ------------------------
		public AxisConfig Add(string name, AxisType axisType, bool undoable)
			{
			AxisConfig axis = this.Get(name);
			if (axis != null)
				return axis;
					
#if UNITY_EDITOR
			if (undoable)
				CFGUI.CreateUndo("Create \"" + name + "\" axis.", this.rig);
#endif
			axis = new AxisConfig();
	
			axis.name = name;
			axis.axisType = axisType;
	
			this.list.Add(axis);
	
#if UNITY_EDITOR
			if (undoable)
				CFGUI.EndUndo(this.rig);
#endif
			return axis;
			}


		// ------------------
		public void ApplyKeyboardInput()
			{
			for (int i = 0; i < this.list.Count; ++i)
				this.list[i].ApplyKeyboardInput();
			}


		// ---------------------
		public void MuteAllUntilRelease()
			{
			for (int i = 0; i < this.list.Count; ++i)
				{
				this.list[i].MuteUntilRelease();
				}
			}
			
		}








	// ------------------------
	// Automatic Input Config Class.		
	// ------------------------
	[System.Serializable]
	public class AutomaticInputConfig : NamedConfigElement
		{
		public DigitalBinding
			targetBinding;

		public DisablingConditionSet
			disablingConditions;

		[System.NonSerialized]
		private bool		
			disabledByConditions;
	

		public List<RelatedAxis>
			relatedAxisList;
		public List<RelatedKey>
			relatedKeyList;


		// -------------------
		public AutomaticInputConfig()
			{
			this.targetBinding			= new DigitalBinding();
			this.disabledByConditions	= false;
			this.relatedAxisList			= new List<RelatedAxis>();
			this.relatedKeyList			= new List<RelatedKey>();

			this.disablingConditions	= new DisablingConditionSet(null);
			this.disablingConditions.mobileModeRelation = DisablingConditionSet.MobileModeRelation.EnabledOnlyInMobileMode;

			}



		// -----------------
		public void SetRig(InputRig rig)
			{
			this.disablingConditions.SetRig(rig);
			this.OnDisablingConditionsChange();
			}


		// -----------------
		public void OnDisablingConditionsChange()
			{
			this.disabledByConditions = this.disablingConditions.IsInEffect();
			}


		// ----------------------
		override public void Update(InputRig rig)
			{	
			if (this.disabledByConditions)
				return;

			for (int i = 0; i < this.relatedKeyList.Count; ++i)
				{
				if (!this.relatedKeyList[i].IsEnabling(rig))
					return; 
				}

			for (int i = 0; i < this.relatedAxisList.Count; ++i)
				{
				if (!this.relatedAxisList[i].IsEnabling(rig))
					return;
				}


			this.targetBinding.Sync(true, rig, true);
			}



		// ---------------------
		// Related Axis Class. 
		// --------------------
		[System.Serializable]	
		public class RelatedAxis
			{
			public string 
				axisName;
			public bool
				mustBeControlledByInput;

			private int
				axisId;


			// ----------------
			public RelatedAxis()
				{
				this.axisName = string.Empty;
				}


			// -----------------
			public bool IsEnabling(InputRig rig)
				{
				AxisConfig axis = rig.GetAxisConfig(this.axisName, ref this.axisId);	
				if (axis == null)
					return true;


				return (axis.IsControlledByInput() == this.mustBeControlledByInput);
				}
			
			}

		// --------------------
		// Related Key Class.
		// --------------------
		[System.Serializable]	
		public class RelatedKey
			{
			public KeyCode 
				key;
			public bool 
				mustBeControlledByInput;

			// -----------------
			public bool IsEnabling(InputRig rig)
				{
				return ((this.key == KeyCode.None) || (rig.GetKey(this.key) == this.mustBeControlledByInput));
				}

			}

		

		}


	// ------------------
	[System.Serializable]
	public class AutomaticInputConfigCollection : NamedConfigCollection<AutomaticInputConfig>
		{
		public AutomaticInputConfigCollection(InputRig rig) : base(rig, 0) 	{ }




		// -------------------
		public void SetRig(InputRig rig)
			{
			for (int i = 0; i < this.list.Count; ++i)
				this.list[i].SetRig(rig);
			}


		// ---------------------
		public AutomaticInputConfig Add(string name, bool createUndo)
			{
#if UNITY_EDITOR		
			if (createUndo)
				CFGUI.CreateUndo("Add auto-input config", this.rig);
#endif

			AutomaticInputConfig c = new AutomaticInputConfig();
			c.name = name;
			c.SetRig(this.rig);

			this.list.Add(c);


#if UNITY_EDITOR		
			if (createUndo)
				CFGUI.EndUndo(this.rig);
#endif

			return c;
			}


		// --------------------
		public void OnDisablingConditionsChange()
			{	
			for (int i = 0; i < this.list.Count; ++i)
				{
				this.list[i].OnDisablingConditionsChange();
				}
			}
		}



	



	// ----------------
	public class NamedConfigElement 
		{
		public string name;
		
		
		virtual public void Reset() {}
		virtual public void Update(InputRig rig) {}
		//virtual public void Sync() {}
			
		// ----------------
		public NamedConfigElement()
			{
			}
		}
		


	// ----------------------
	[System.Serializable]
	public class NamedConfigCollection<T>  where T : NamedConfigElement, new()
		{
		public List<T> list;

		[System.NonSerialized]
		protected InputRig
			rig;

		// -------------------
		public NamedConfigCollection(InputRig rig, int capacity) // : base(capacity) 
			{
			this.rig	= rig;
			this.list	= new List<T>(capacity); 
			}



		
	

		// ------------------
		public void ResetAll()
			{
			int count = this.list.Count;
			for (int i = 0; i < count; ++i)
				this.list[i].Reset();
			}

		// --------------------
		public void Update(InputRig rig)
			{
			int count = this.list.Count;
			for (int i = 0; i < count; ++i)
				this.list[i].Update(rig);
			}


		// --------------------
		public T Get(string name, ref int cachedId)
			{
			if ((cachedId >= 0) && (cachedId < this.list.Count))
				{
				if (this.list[cachedId].name == name)
					return this.list[cachedId];
				}

			int count = this.list.Count;
			for (int i = 0; i < count; ++i)
				{
				if (this.list[i].name == name)
					{
					cachedId = i;
					return this.list[i];
					}				
				}
			
			return null;	
			}
	
	
		// --------------
		public T Get(string name)
			{		
			int count = this.list.Count;
			for (int i = 0; i < count; ++i)
				{
				if (this.list[i].name == name)
					return this.list[i];
				}
		
			return null;	
			}
		
		
		}



	// ----------------------
	// Tilit Config Class
	// ----------------------
		
	[System.Serializable]
	public class TiltConfig : IBindingContainer
		{	
		[System.NonSerialized]
		private InputRig 
			rig;

		public TiltState tiltState;
			
			
		private int
			digitalRollDirection,
			digitalPitchDirection;

		private bool 
			disabledByRigSwitches;

		public DisablingConditionSet
			disablingConditions;
			
		public AnalogConfig
			rollAnalogConfig,
			pitchAnalogConfig;

		public AxisBinding 
			rollBinding,
			pitchBinding;
			

		public DigitalBinding
			rollLeftBinding,
			rollRightBinding,
			pitchForwardBinding,
			pitchBackwardBinding;

 

		// ---------------
		public TiltConfig(InputRig rig)	
			{
			this.rig 		= rig;
			this.tiltState	= new TiltState();

			this.rollAnalogConfig = new AnalogConfig();
			this.pitchAnalogConfig = new AnalogConfig();

			this.rollAnalogConfig.analogDeadZone = 0.3f;
			this.pitchAnalogConfig.analogDeadZone = 0.3f;

			this.disablingConditions = new DisablingConditionSet(rig);

			this.disablingConditions.disableWhenCursorIsUnlocked = false;
			this.disablingConditions.disableWhenTouchScreenInactive = false;
			this.disablingConditions.mobileModeRelation = DisablingConditionSet.MobileModeRelation.EnabledOnlyInMobileMode;

			this.rollBinding = new AxisBinding("Horizontal", false);
			this.pitchBinding = new AxisBinding("Vertical", false);
		
			this.rollLeftBinding = new DigitalBinding();
			this.rollRightBinding = new DigitalBinding();

			this.pitchForwardBinding = new DigitalBinding();
			this.pitchBackwardBinding = new DigitalBinding();

			}

			

		// -----------------
		public void Reset()
			{	
			this.digitalPitchDirection = 0;
			this.digitalRollDirection = 0;

			this.tiltState.Reset();
			this.OnDisablingConditionsChange();
			}

	
		// -----------------
		public void OnDisablingConditionsChange()
			{
			this.disabledByRigSwitches = this.disablingConditions.IsInEffect();
			}
			

		// -------------------
		public bool IsEnabled()
			{
			return (!this.disabledByRigSwitches);
			}


		// ---------------
		public void Update()
			{
			this.tiltState.InternalApplyVector(UnityEngine.Input.acceleration);

			this.tiltState.Update();

			Vector2 rawAnalogVal = this.tiltState.GetAnalog();
 
			this.digitalRollDirection = this.rollAnalogConfig.GetSignedDigitalVal(rawAnalogVal.x, this.digitalRollDirection);
			this.digitalPitchDirection = this.pitchAnalogConfig.GetSignedDigitalVal(rawAnalogVal.y, this.digitalPitchDirection);

			// Sync axes...
				
			if (this.IsEnabled())
				{
				if (this.rollBinding.enabled)
					this.rollBinding.SyncFloat(this.rollAnalogConfig.GetAnalogVal(rawAnalogVal.x), InputRig.InputSource.Analog, this.rig);
					
				if (this.pitchBinding.enabled)
					this.pitchBinding.SyncFloat(this.pitchAnalogConfig.GetAnalogVal(rawAnalogVal.y), InputRig.InputSource.Analog, this.rig);
					

				if (this.digitalRollDirection != 0)
					((this.digitalRollDirection < 0) ? this.rollLeftBinding : this.rollRightBinding).Sync(true, this.rig); 

				if (this.digitalPitchDirection != 0)
					((this.digitalPitchDirection < 0) ? this.pitchBackwardBinding : this.pitchForwardBinding).Sync(true, this.rig); 
				
				}
			}


		// -------------------
		public bool IsBoundToAxis(string axisName, InputRig rig)
			{ 
			return (
				this.rollBinding.IsBoundToAxis(axisName, rig) ||
				this.pitchBinding.IsBoundToAxis(axisName, rig) ||
				this.rollLeftBinding.IsBoundToAxis(axisName, rig) ||
				this.rollRightBinding.IsBoundToAxis(axisName, rig) ||
				this.pitchForwardBinding.IsBoundToAxis(axisName, rig) ||
				this.pitchBackwardBinding.IsBoundToAxis(axisName, rig) );
			}
		
		// ----------------------
		public bool IsBoundToKey(KeyCode key, InputRig rig)
			{ 
			return (
				this.rollBinding.IsBoundToKey(key, rig) ||
				this.pitchBinding.IsBoundToKey(key, rig)  ||
				this.rollLeftBinding.IsBoundToKey(key, rig) ||
				this.rollRightBinding.IsBoundToKey(key, rig) ||
				this.pitchForwardBinding.IsBoundToKey(key, rig) ||
				this.pitchBackwardBinding.IsBoundToKey(key, rig) );
			}
	
		// ------------
		public bool IsEmulatingTouches()		{ return false; }
		public bool IsEmulatingMousePosition()	{ return false; }

	

		// ----------------------
		public void GetSubBindingDescriptions(BindingDescriptionList descList, 
			Object undoObject, string parentMenuPath) 
			{
			descList.Add(this.rollBinding, InputRig.InputSource.Analog, "Roll Angle (Analog)", parentMenuPath, undoObject);
			descList.Add(this.pitchBinding, InputRig.InputSource.Analog, "Pitch Angle (Analog)", parentMenuPath, undoObject);
			descList.Add(this.rollLeftBinding, "Roll Left (Digital)", parentMenuPath, undoObject);
			descList.Add(this.rollRightBinding, "Roll Right (Digital)", parentMenuPath, undoObject);
			descList.Add(this.pitchForwardBinding, "Pitch Forward (Digital)", parentMenuPath, undoObject);
			descList.Add(this.pitchBackwardBinding, "Pitch Backward (Digital)", parentMenuPath, undoObject);
	
			}

					
		}


	// ----------------------
	[System.Serializable]
	public class ScrollWheelState : IBindingContainer
		{
		private InputRig	rig;
		
		
	
		public ScrollDeltaBinding
			horzScrollDeltaBinding,
			vertScrollDeltaBinding;

		
		// -------------------
		public ScrollWheelState(InputRig rig)
			{
			this.rig = rig;

			this.vertScrollDeltaBinding = new ScrollDeltaBinding(DEFAULT_VERT_SCROLL_WHEEL_NAME, true);
			this.horzScrollDeltaBinding = new ScrollDeltaBinding(DEFAULT_HORZ_SCROLL_WHEEL_NAME, true);
			}

			

		// ------------------
		public void Reset()
			{
			}			

		// -------------------
		public void Update()
			{
			
			// Bind to axis...
		
			Vector2 mouseScroll = UnityEngine.Input.mouseScrollDelta;

			this.horzScrollDeltaBinding.SyncScrollDelta(Mathf.RoundToInt(mouseScroll.x), this.rig); //.Sync(delta.x);
			this.vertScrollDeltaBinding.SyncScrollDelta(Mathf.RoundToInt(mouseScroll.y), this.rig);
			}
			
		// ----------------
		public Vector2 GetDelta()
			{
			Vector2 v = UnityEngine.Input.mouseScrollDelta; 

			if ((this.rig == null)) 
				return v;
				
			if (this.horzScrollDeltaBinding.deltaBinding.enabled)
				v.x = this.horzScrollDeltaBinding.deltaBinding.GetAxis(this.rig);

			if (this.vertScrollDeltaBinding.deltaBinding.enabled)
				v.y = this.vertScrollDeltaBinding.deltaBinding.GetAxis(this.rig);

	
			return v;
			}



		// -------------------
		public bool IsBoundToAxis(string axisName, InputRig rig)
			{ 
			return (
				this.horzScrollDeltaBinding.IsBoundToAxis(axisName, rig) ||
				this.vertScrollDeltaBinding.IsBoundToAxis(axisName, rig) );
			}
	
	
		// ----------------------
		public bool IsBoundToKey(KeyCode key, InputRig rig)
			{ 
			return (
				this.horzScrollDeltaBinding.IsBoundToKey(key, rig) ||
				this.vertScrollDeltaBinding.IsBoundToKey(key, rig) );
			}
	
		// ------------
		public bool IsEmulatingTouches()		{ return false; }
		public bool IsEmulatingMousePosition()	{ return false; }

	

		// ----------------------
		public void GetSubBindingDescriptions(BindingDescriptionList descList, 
			Object undoObject, string parentMenuPath) 
			{
			descList.Add(this.vertScrollDeltaBinding, "Vertical/Primary Scroll Wheel Delta", parentMenuPath, undoObject);
			descList.Add(this.horzScrollDeltaBinding, "Horizontal/Secondary Scroll Wheel Delta", parentMenuPath, undoObject);
			}

			
		}



	// ----------------------
	[System.Serializable]
	public class MouseConfig : IBindingContainer
		{
		private InputRig rig;
		public AxisBinding horzDeltaBinding;
		public AxisBinding vertDeltaBinding;
			

		private Vector2 mousePos;
		private Vector2 frMousePos;
		private int		frMousePosPrio;
 			


		// --------------------
		public MouseConfig(InputRig rig)
			{
			this.rig = rig;
			
			this.horzDeltaBinding = new AxisBinding("Mouse X", false);
			this.vertDeltaBinding = new AxisBinding("Mouse Y", false);

			}

			
	
		// ----------------------
		public void Reset()
			{
			}


		// --------------------
		public void SetPosition(Vector2 pos, int prio)
			{
			if (prio > this.frMousePosPrio)	
				{
				this.frMousePos = pos;
				this.frMousePosPrio = prio;
				}
			}

	
		// --------------------
		public Vector3 GetPosition()
			{
			return this.mousePos;
			}


		// -------------------
		private bool IsMouseDeltaUsable()
			{
			return (!CF2Input.IsInMobileMode() ||  
				(UnityEngine.Input.touchSupported && UnityEngine.Input.mousePresent && !UnityEngine.Input.simulateMouseWithTouches &&
				(CFCursor.lockState == CursorLockMode.Locked)));
//#if !UNITY_PRE_5_0
//				(Cursor.lockState == CursorLockMode.Locked)));			
//#else
//				Screen.lockCursor));
//#endif	
			}

		// ------------------
		private bool IsMousePositionUsable()
			{
			return (!CF2Input.IsInMobileMode() ||  
				(UnityEngine.Input.touchSupported && UnityEngine.Input.mousePresent && !UnityEngine.Input.simulateMouseWithTouches && 
				(CFCursor.lockState != CursorLockMode.Locked)));

//#if !UNITY_PRE_5_0
//				(UnityEngine.Cursor.lockState != CursorLockMode.Locked)));				
//#else
//				(!UnityEngine.Screen.lockCursor)));				
//#endif
			}

		// ----------------------
		public void Update()
			{


			// Apply acutal mouse delta...

			if (this.IsMouseDeltaUsable()) //!CF2Input.IsInMobileMode() && UnityEngine.Input.mousePresent)
				{
				this.horzDeltaBinding.SyncFloat(UnityEngine.Input.GetAxisRaw(InputRig.CF_MOUSE_DELTA_X_AXIS), InputRig.InputSource.MouseDelta, this.rig);
				this.vertDeltaBinding.SyncFloat(UnityEngine.Input.GetAxisRaw(InputRig.CF_MOUSE_DELTA_Y_AXIS), InputRig.InputSource.MouseDelta, this.rig);
				}

			if (this.IsMousePositionUsable())
				{				

				// Set mouse pos...

				this.SetPosition(Input.mousePosition, -1);
				}


				
			// Set collected mouse pos...

			this.mousePos = this.frMousePos;
			this.frMousePosPrio = -10;


			}
			

		// ---------------
		public bool IsBoundToAxis(string axisName, InputRig rig)
			{ 
			return (
				this.horzDeltaBinding.IsBoundToAxis(axisName, rig) ||
				this.vertDeltaBinding.IsBoundToAxis(axisName, rig) );
			}
	

	
		// ----------------------
		public bool IsBoundToKey(KeyCode key, InputRig rig)
			{ 
			return (
				this.horzDeltaBinding.IsBoundToKey(key, rig) ||
				this.vertDeltaBinding.IsBoundToKey(key, rig) );
			}
	
		// ------------
		public bool IsEmulatingTouches()		{ return false; }
		public bool IsEmulatingMousePosition()	{ return false; }

	

		// ----------------------
		public void GetSubBindingDescriptions(BindingDescriptionList descList,  
			Object undoObject, string parentMenuPath)
			{
			descList.Add(this.horzDeltaBinding, InputRig.InputSource.MouseDelta, "Horz. Mouse Delta", parentMenuPath, undoObject);
			descList.Add(this.vertDeltaBinding, InputRig.InputSource.MouseDelta, "Vert. Mouse Delta", parentMenuPath, undoObject);
	
			}

		}


//! \endcond


	}
}
