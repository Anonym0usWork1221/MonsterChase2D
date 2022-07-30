// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using ControlFreak2.Internal;



namespace ControlFreak2
{
// -------------------
//! Gamepad Manager class.
// -------------------
public class GamepadManager : ControlFreak2.Internal.ComponentBase
	{

	public delegate void OnConnectionChnageCallback(Gamepad gamepad);				//!< Gamepad connection delegate.
	public delegate void OnDisconnectionCallback(Gamepad gamepad, DisconnectionReason reason);		//!< Gamepad disconnection/disactivation delegate.

	static public GamepadManager activeManager		
		{ 	get; private set; }								//!< Active Gamepad Manager.

	static public event OnDisconnectionCallback
		onGamepadDisconnected;								//!< Called when a gamepad is disconnected.
	static public event OnDisconnectionCallback
		onGamepadDisactivated;								//!< Called when a gamepad is disactivated.
		
	static public event OnConnectionChnageCallback
		onGamepadConnected;									//!< Called when a gamepad is connected.
	static public event OnConnectionChnageCallback
		onGamepadActivated;									//!< Called when a gamepad is activated.

	static public event System.Action
		onChange;												//!< Called when anything releated to connection or activation changes.
	

	public const int 
		MAX_JOYSTICKS		= 4;							//!< Maximal number of connected gamepads.
	public const int 
		MAX_INTERNAL_AXES	= 10;							//!< Maximal number of internal gamepad axes.
	public const int 
		MAX_INTERNAL_KEYS	= 20;							//!< Maximal number of internal gamepad keys.

		
	// --------------------
	//! Gamepad stick.
	// --------------------
	public enum GamepadStick : int
		{
		LeftAnalog,		//!< Primary (usually the left analog stick).
		RightAnalog,	//!< Right analog stick.
		Dpad				//!< D-pad.
		}

	public const int 
		GamepadStickCount = ((int)GamepadStick.Dpad + 1);


	// -------------------
	//! Gamepad key.
	// -------------------
	public enum GamepadKey : int
		{
		FaceBottom,		//!< 'Cross' in PS terms.
		FaceRight,		//!< 'Square' in PS terms.
		FaceTop,			//!< 'Triangle' in PS terms.
		FaceLeft,		//!< 'Circle' in PS terms.

		Start,			//!< 'Start' button
		Select,			//!< 'Back' in XBox terms.

		L1,				//!< Left Shoulder Button 
		R1,				//!< Right Shoulder Button 
		L2,				//!< Left Trigger 
		R2,				//!< Right Trigger
		L3,				//!< Left Thumb Button
		R3					//!< Right Thumb Button
		}

	public const int 
		GamepadKeyCount = ((int)GamepadKey.R3 + 1);
		

	// ------------------
	//! Gamepad Disconnection Reason.
	// ------------------
	public enum DisconnectionReason
		{
		MassDisactivation,		//!< Mass disactivation.
		Disactivation,				//!< Single gamepad disactivation. 
		Disconnection,				//!< Single gamepad disconnection.
		ManagerDisabled			//!< Gamepad Manager has been disabled/destroyed.
		}		
		

//! \cond

	// ----------------------
	static public string GetJoyAxisName(int joyId, int axisId)
		{
		if ((joyId < 0) || (joyId >= MAX_JOYSTICKS) || (axisId < 0) || (axisId >= MAX_INTERNAL_AXES))
			return string.Empty;

		return ("cfJ" + joyId + "" + axisId);
		}
		
		
	// --------------------
	static public KeyCode GetJoyKeyCode(int joyId, int keyId)
		{
		KeyCode keyCode = KeyCode.Joystick1Button0;

		switch (joyId)
			{
			case 0 : keyCode = KeyCode.Joystick1Button0; break; 
			case 1 : keyCode = KeyCode.Joystick2Button0; break; 
			case 2 : keyCode = KeyCode.Joystick3Button0; break; 
			case 3 : keyCode = KeyCode.Joystick4Button0; break;
			default:
				return KeyCode.None; 
			}

		if ((keyId < 0) || (keyId >= MAX_INTERNAL_KEYS))
			return KeyCode.None;

		return keyCode + keyId;		
		}



//	// ---------------------
//	public enum GamepadOrderMode
//		{
//		STATIC,		// Gamepads will not change order 
//		REORDER		// Newly connected gamepads can change order.
//		}



	// -----------------------
	private CustomGamepadProfileBank 
		customProfileBank;

	//public bool autoConnectToRig;
	//public InputRig rig;
		
	//public GamepadOrderMode
	//	gamepadOrderMode;
		
	public bool
		dontDestroyOnLoad;

	public float
		connectionCheckInterval;	
	private float
		elapsedSinceLastConnectionCheck;
		

	[System.NonSerialized]
	private JoystickConfig
		fallbackDpadConfig,
		fallbackLeftStickConfig,
		fallbackRightStickConfig;

	[System.NonSerialized]
	private AnalogConfig
		fallbackLeftTriggerAnalogConfig,
		fallbackRightTriggerAnalogConfig;

	private List<Gamepad> 
		activeGamepads,
		freeGamepads,
		connectedGamepads;
		
	private Gamepad
		gamepadsCombined;

	private int 
		activeGamepadNum;

	//private JoystickConn[]	
	//	joyConnections;


	// -------------------
	//static public int NumberOfConnected { get { return 0; } }

		

	// -----------------------
	public GamepadManager() : base()
		{
		this.customProfileBank			= new CustomGamepadProfileBank();

		this.dontDestroyOnLoad			= false;

		this.connectionCheckInterval	= 1.0f;
		this.elapsedSinceLastConnectionCheck = 10000.0f;

		this.fallbackDpadConfig 				= new JoystickConfig();
		this.fallbackDpadConfig.stickMode		= JoystickConfig.StickMode.Digital8;
		this.fallbackDpadConfig.analogDeadZone	= 0.3f;

		this.fallbackLeftStickConfig				= new JoystickConfig();
		this.fallbackLeftStickConfig.stickMode		= JoystickConfig.StickMode.Analog;
		this.fallbackLeftStickConfig.analogDeadZone	= 0.3f;

		this.fallbackRightStickConfig				= new JoystickConfig();
		this.fallbackRightStickConfig.stickMode		= JoystickConfig.StickMode.Analog;
		this.fallbackRightStickConfig.analogDeadZone= 0.3f;
			

		this.fallbackLeftTriggerAnalogConfig				= new AnalogConfig();
		this.fallbackLeftTriggerAnalogConfig.analogDeadZone	= 0.2f;

		this.fallbackRightTriggerAnalogConfig 				= new AnalogConfig();
		this.fallbackRightTriggerAnalogConfig.analogDeadZone= 0.2f;


		//this.gamepadOrderMode = GamepadOrderMode.REORDER;

		this.gamepadsCombined = new Gamepad(this);

			
		this.freeGamepads 		= new List<Gamepad>(MAX_JOYSTICKS);
		this.activeGamepads 	= new List<Gamepad>(MAX_JOYSTICKS);
		this.connectedGamepads	= new List<Gamepad>(MAX_JOYSTICKS);
		
		this.activeGamepadNum = 0;
			

		for (int i = 0; i < MAX_JOYSTICKS; ++i)
			{
			this.freeGamepads.Add(new Gamepad(this));
			//this.connectedGamepads.Add(null);
			}
			


		}



	// ---------------------
	override protected void OnInitComponent()
		{

		this.customProfileBank.Load();


		
		}

		
	// ------------------
	override protected void OnDestroyComponent()	
		{}	

	// ------------------
	override protected void OnEnableComponent()		
		{
		if (GamepadManager.activeManager == null)
			{
			this.SetAsMain();

			if (this.dontDestroyOnLoad && !CFUtils.editorStopped)
				GameObject.DontDestroyOnLoad(this);
			}


		else if (GamepadManager.activeManager != this)
			{
			if (!CFUtils.editorStopped)
				this.enabled = false;
			}


		//this.ConnectJoysticks();
		} 

//! \endcond


	// ------------------
	//! Is this manager the main one?
	// ------------------
	public bool IsMain()
		{	
		return (GamepadManager.activeManager == this);
		}

	// --------------------
	//! Set this manager as the main one.
	// -------------------
	public void SetAsMain()
		{	
		this.enabled = true;

		if (this.IsMain())
			return;

		if (GamepadManager.activeManager != null)
			GamepadManager.activeManager.RemoveAsMain();

		GamepadManager.activeManager = this;
		CF2Input.onActiveRigChange += this.OnActiveRigChange;

		this.OnActiveRigChange();

		}

	// --------------------
	//! Remove this manager as the main one.
	// --------------------
	public void RemoveAsMain()
		{

		if (GamepadManager.activeManager == this)
			GamepadManager.activeManager = null;

		CF2Input.onActiveRigChange -= this.OnActiveRigChange;
		}




	// ---------------------
	//! Get this manager's custom gamepad profile bank.
	// --------------------
	public CustomGamepadProfileBank GetCustomProfileBank()
		{ return this.customProfileBank; }
		
		

//! \cond
	// ---------------
	override protected void OnDisableComponent()	
		{
		// Disconnect all gamepads..

		for (int i = 0; i < this.activeGamepads.Count; ++i)
			{
			Gamepad g = this.activeGamepads[i];
			if (g != null)
				this.DisconnectGamepad(g, DisconnectionReason.ManagerDisabled);
			}

		if (this.IsMain())
			{
			if (GamepadManager.onChange != null)		
				GamepadManager.onChange();

			this.RemoveAsMain();
			}

		}	


	// ---------------------
	private void OnActiveRigChange()
		{
		this.gamepadsCombined.SetRig(CF2Input.activeRig);
	
		for (int i = 0; i < this.freeGamepads.Count; ++i)
			this.freeGamepads[i].SetRig(CF2Input.activeRig);

		for (int i = 0; i < this.connectedGamepads.Count; ++i)
			this.connectedGamepads[i].SetRig(CF2Input.activeRig);
		}
		
	

	


	// -------------------
	void Update()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return;
#endif			

		this.CheckJoystickConnections();
			

			

		// Update gamepads...

		for (int i = 0; i < this.connectedGamepads.Count; ++i)
			{
			this.connectedGamepads[i].Update();	
			}

		for (int i = 0; i < this.activeGamepads.Count; ++i)
			{
			Gamepad g = this.activeGamepads[i];
			if ((g != null) && !g.IsBlocked())
				this.gamepadsCombined.ApplyGamepadState(g);
			}

		// Update combined gamepad...
			
		this.gamepadsCombined.Update();



			
		// Send state to active Input Rig...

		InputRig activeRig = CF2Input.activeRig;
		if (activeRig != null)
			this.ApplyToRig(activeRig);

		}
		




	// ----------------------
	private void ApplyToRig(InputRig rig)
		{
		if (rig == null)
			return;

		for (int i = 0; i < this.activeGamepads.Count; ++i)
			{
			Gamepad gamepad = this.activeGamepads[i];
			if ((gamepad == null) || gamepad.IsBlocked())
				continue;
				
			if (rig.gamepads.Length > i)
				rig.gamepads[i].SyncGamepad(gamepad, rig);


			}

		// Combine all gamepads into one...

		rig.anyGamepad.SyncGamepad(this.gamepadsCombined, rig);
		}


	// ---------------------
	private void CheckJoystickConnections()
		{
		this.CheckUnactivatedGamepads();

		if ((this.elapsedSinceLastConnectionCheck += Time.unscaledDeltaTime) < this.connectionCheckInterval)
			return;

		this.ConnectJoysticks();
		}
		

	// ---------------------------
	private string[] 
		deviceConnections;

	private string GetConnectedDeviceName(int internalJoyId)
		{
		if ((this.deviceConnections == null) || (internalJoyId < 0) || (internalJoyId >= this.deviceConnections.Length) ||
			(this.deviceConnections[internalJoyId] == null) || (this.deviceConnections[internalJoyId].Length == 0))
			return string.Empty;

		return this.deviceConnections[internalJoyId]; 
		}
		

	// -----------------
	private Gamepad AddConnectedGamepad()
		{
		if (this.freeGamepads.Count <= 0)
			return null;

		Gamepad g = this.freeGamepads[this.freeGamepads.Count - 1];
		this.freeGamepads.RemoveAt(this.freeGamepads.Count - 1);

		this.connectedGamepads.Add(g);

		return g;
		}


	// --------------------
	private Gamepad FindDisconnectedGamepad(int internalJoyId, string deviceName)
		{		
		Gamepad 
			firstGamepad		= null,
			matchingNameGamepad = null;
		
		for (int i = 0; i < this.connectedGamepads.Count; ++i)
			{
			Gamepad g = this.connectedGamepads[i];
			if (g.IsConnected())
				continue;

			if (firstGamepad == null)
				firstGamepad = g;

			if (g.GetInternalJoyName() == deviceName)
				{
				if (internalJoyId == g.GetInternalJoyId())
					return g;

				if (matchingNameGamepad == null)
					matchingNameGamepad = g;	
				}
			}

		return ((matchingNameGamepad != null) ? matchingNameGamepad : firstGamepad);
		}

	// --------------------
	private void ActivateGamepad(Gamepad g)
		{
		if (g == null)
			return;
			
		int slot = -1;

		// Try to connect to last used slot...

		int preferredSlot = g.GetSlot();
		if ((preferredSlot >= 0) && (preferredSlot < this.activeGamepads.Count) && (this.activeGamepads[preferredSlot] == null))
			{
			this.activeGamepads[preferredSlot] = g;
			slot = preferredSlot;
			}

		if (slot < 0)
			{
			// Find free slot...
	
			for (int i = 0; i < this.activeGamepads.Count; ++i)
				{
				if (this.activeGamepads[i] == null)
					{
					this.activeGamepads[i] = g;
					slot = i;
					break;
					}
				}
	
			// Add at the end...
		
			if (slot < 0)
				{
				slot = this.activeGamepads.Count;
				this.activeGamepads.Add(g);
				}
			}

		if (slot < 0)
			return;
			
		g.OnActivate(slot);
		//g.SetSlot(slot);

		this.CountActiveGamepads();
	
		if (this.IsMain() && (GamepadManager.onGamepadActivated != null))
			GamepadManager.onGamepadActivated(g);
		}
		
		
	// ------------------------
	private void DisactivateGamepad(Gamepad g, DisconnectionReason reason)
		{
		if ((g == null) || !g.IsActivated())
			return;

		if (this.IsMain() && (GamepadManager.onGamepadDisactivated != null))
			GamepadManager.onGamepadDisactivated(g, reason);
			
		g.OnDisactivate();

		int i = this.activeGamepads.IndexOf(g);
		if (i < 0)
			return;		

		this.activeGamepads[i] = null;

		this.CountActiveGamepads();
		}


	// ---------------------
	private void CountActiveGamepads()
		{
		this.activeGamepadNum = 0;

		for (int i = 0; i < this.activeGamepads.Count; ++i)
			{
			if (this.activeGamepads[i] != null)
				++this.activeGamepadNum;
			}
		}

	// ---------------------
	private void DisconnectGamepad(Gamepad g, DisconnectionReason reason)
		{
		if (g.IsActivated())
			this.DisactivateGamepad(g, reason);

		if (this.IsMain() && (GamepadManager.onGamepadDisconnected != null))
			GamepadManager.onGamepadDisconnected(g, reason);

		g.OnDisconnect();	

		}




	// ------------------
	private void ConnectJoysticks()
		{
		this.elapsedSinceLastConnectionCheck = 0;

		this.deviceConnections = Input.GetJoystickNames();
	
		bool anythingChanged = false;


		int usedHwConnBitset = 0;
		int disconnSlotBitset = 0;

		// Check disconnections...
		
		for (int i = 0; i < this.connectedGamepads.Count; ++i)
			{
			Gamepad g = this.connectedGamepads[i];
			if (!g.IsConnected())	
				{
				disconnSlotBitset |= (1 << i);
				continue;
				}					

			string hwName = this.GetConnectedDeviceName(g.GetInternalJoyId());
			if (hwName == g.GetInternalJoyName())
				{
				usedHwConnBitset |= (1 << g.GetInternalJoyId());
				}
			else
				{
				anythingChanged = true;

				this.DisconnectGamepad(g, DisconnectionReason.Disconnection);
				}
			}

	
		// Look to add new Connections...

		for (int joyi = 0; joyi < MAX_JOYSTICKS; ++joyi)
			{
			string deviceName = this.GetConnectedDeviceName(joyi);
			if (string.IsNullOrEmpty(deviceName) || ((usedHwConnBitset & (1 << joyi)) != 0))
				continue;
				
			// Try to find best match in disconncted active gamepads...

			Gamepad g = this.FindDisconnectedGamepad(joyi, deviceName);
			if (g == null)
				g = this.AddConnectedGamepad();

			if (g != null)
				{
				anythingChanged = true;

				g.ConnectToJoy(joyi, deviceName); //, this.GetProfileForDevice(deviceName));
				
				GamepadProfile profile = this.GetProfileForDevice(deviceName);
				if (profile != null)
					{
					g.SetProfile(profile);	
					}
	
				if (this.IsMain() && (GamepadManager.onGamepadConnected != null))
					GamepadManager.onGamepadConnected(g);

				}

	


			}


		if (anythingChanged)
			{
			if (this.IsMain() && (GamepadManager.onChange != null))
				GamepadManager.onChange();
			}


		}		


	// ------------------------
	private void CheckUnactivatedGamepads()
		{
		bool anythingChanged = false;

		// Check Activation...

		for (int i = 0; i < this.connectedGamepads.Count; ++i)
			{
			Gamepad g = this.connectedGamepads[i];
			if (g.IsConnected() && g.IsSupported() && !g.IsBlocked() && !g.IsActivated() && g.AnyInternalKeyOrAxisPressed())
				{
				anythingChanged = true;
				this.ActivateGamepad(g);
				}
			}

		if (this.IsMain() && anythingChanged && (GamepadManager.onChange != null))
			GamepadManager.onChange();
		}

//! \endcond


	// -------------------
	//! Get number of active gamepad slots. This is not always equal to number of active gamepads. 
	// ----------------
	public int GetGamepadSlotCount()	
		{ return this.activeGamepads.Count; }

	// ---------------------
	//! Get number of active gamepads.
	// ---------------------
	public int GetActiveGamepadCount() 
		{ return this.activeGamepadNum; }

	// ------------------
	//! Get active gamepad at slot or null if the slot is no longer active.
	// ------------------
	public Gamepad GetGamepadAtSlot(int slot)
		{
		return (((slot < 0) || (slot >= this.activeGamepads.Count)) ? null : this.activeGamepads[slot]);
		}
		
	// ----------------------
	//! Get combined state of all active gamepads.
	// ------------------
	public Gamepad GetCombinedGamepad()
		{ return this.gamepadsCombined; }

	// ------------------
	//! Get number of connected (activated or not) gamepads.
	// ------------------
	public int GetConnectedGamepadCount()
		{ return this.connectedGamepads.Count; }
		
	// -------------------
	//! Get connected gamepad at given index.
	// -------------------
	public Gamepad GetConnectedGamepad(int index)
		{ return (((index < 0) || (index >= this.connectedGamepads.Count)) ? null : this.connectedGamepads[index]); }


	// -----------------------
	//! Disactivate all gamepads.
	// ---------------------
	public void DisactivateGamepads()	
		{
		for (int i = 0; i < this.activeGamepads.Count; ++i)
			{
			Gamepad g = this.activeGamepads[i];
			if (g != null)
				this.DisactivateGamepad(g, DisconnectionReason.MassDisactivation);
			}
		
		if (this.IsMain() && (GamepadManager.onChange != null))
			GamepadManager.onChange();
		}


	// -------------------
	private GamepadProfile GetProfileForDevice(string joyDevice)
		{
		GamepadProfile profile = null;

		if (this.customProfileBank != null)
			profile = this.customProfileBank.GetProfile(joyDevice);

		if (profile == null)
			profile = BuiltInGamepadProfileBank.GetProfile(joyDevice);

		if (profile == null)
			profile = BuiltInGamepadProfileBank.GetGenericProfile();	//GamepadProfileBank.GetFallbackProfile();

		return profile;
		}


		
		


	// -------------------
	//! Gamepad State Class.
	// -------------------

	//[System.Serializable]
	public class Gamepad
		{
//! \cond
		private GamepadManager
			manager;
		private GamepadProfile	
			profile;
		private bool
			isActivated,
			isConnected,
			isBlocked;
		private string
			internalJoyName;
		private int
			internalJoyId,
			slot;

		public JoyState
			leftStick,
			rightStick,
			dpad;

		public KeyState[]
			keys;

		private InternalAxisState[]
			internalAxes;
		private InternalKeyState[]
			internalKeys;
	
	
		// ----------------
		public Gamepad(GamepadManager manager) //, bool assignAnalogConfigs)
			{
			this.manager = manager;

			// Allocate Internal axes and keys...

			this.internalAxes = new InternalAxisState[GamepadManager.MAX_INTERNAL_AXES];
			this.internalKeys = new InternalKeyState[GamepadManager.MAX_INTERNAL_KEYS];

			for (int i = 0; i < this.internalAxes.Length; ++i)
				this.internalAxes[i] = new InternalAxisState();

			for (int i = 0; i < this.internalKeys.Length; ++i)
				this.internalKeys[i] = new InternalKeyState();
		

			// Allocate Sticks...

			this.leftStick	= new JoyState(this, manager.fallbackLeftStickConfig);			
			this.rightStick	= new JoyState(this, manager.fallbackRightStickConfig);			
			this.dpad		= new JoyState(this, manager.fallbackDpadConfig);		
	

			// Allocate Keys...

			this.keys = new KeyState[(int)GamepadManager.GamepadKeyCount];

			for (int i = 0; i < this.keys.Length; ++i)
				{		
				if ((i == (int)GamepadKey.L1) || (i == (int)GamepadKey.L2))
					this.keys[i] = new KeyState(this, true, manager.fallbackLeftTriggerAnalogConfig);

				else if ((i == (int)GamepadKey.R1) || (i == (int)GamepadKey.R2))
					this.keys[i] = new KeyState(this, true, manager.fallbackRightTriggerAnalogConfig);

				else
					this.keys[i] = new KeyState(this, false);
				}
		

			//if (CF2Input.activeRig != null)
				//this.SetRig(CF2Input.activeRig);

			this.OnDisconnect();				
			}
			
//! \endcond
			

		// -------------------
		public bool 	IsConnected()			{ return this.isConnected; } //(this.profile != null); }
		public bool 	IsActivated()			{ return this.isActivated; }
		public bool 	IsSupported()			{ return (this.profile != null); }
		public int		GetSlot()				{ return this.slot; }
		//public void		SetSlot(int slot)		{ this.slot = slot; }
		public string	GetInternalJoyName()	{ return this.internalJoyName; }
		public int		GetInternalJoyId()		{ return this.internalJoyId; }
		public string	GetProfileName()		{ return ((this.profile != null) ? this.profile.name : string.Empty); }
			
		public void Block(bool block)			{ this.isBlocked = block; }
		public bool IsBlocked()					{ return this.isBlocked; }
			


//! \cond

		// ---------------------
		public void SetRig(InputRig rig)
			{
			this.leftStick	.SetConfig((rig != null) ? rig.leftStickConfig	: this.manager.fallbackLeftStickConfig);
			this.rightStick.SetConfig((rig != null) ? rig.rightStickConfig : this.manager.fallbackRightStickConfig);
			this.dpad		.SetConfig((rig != null) ? rig.dpadConfig		: this.manager.fallbackDpadConfig);

			this.keys[(int)GamepadKey.L1].SetConfig((rig != null) ? rig.leftTriggerAnalogConfig : this.manager.fallbackLeftTriggerAnalogConfig);
			this.keys[(int)GamepadKey.L2].SetConfig((rig != null) ? rig.leftTriggerAnalogConfig : this.manager.fallbackLeftTriggerAnalogConfig);

			this.keys[(int)GamepadKey.R1].SetConfig((rig != null) ? rig.rightTriggerAnalogConfig : this.manager.fallbackRightTriggerAnalogConfig);
			this.keys[(int)GamepadKey.R2].SetConfig((rig != null) ? rig.rightTriggerAnalogConfig : this.manager.fallbackRightTriggerAnalogConfig);
			}


		// ---------------------
		public void ConnectToJoy(int internalJoyId, string joyName) //, GamepadProfile profile)
			{
			//if ((internalJoyId != this.internalJoyId) || (this.joyName != joyName))
			//	this.connChanged = true;

			this.profile		= null; //profile;

			this.isActivated		= false;
			this.profile			= null;
			this.isBlocked			= false;
			
			if (internalJoyId >= 0) // profile != null)
				{
				this.internalJoyId		= internalJoyId;
				this.internalJoyName	= joyName;
				this.isConnected = true;
				}
			else
				{
				this.isConnected = false;
				}
				
			// Connect Internal Axes and Keys...

			for (int i = 0; i < this.internalAxes.Length; ++i)
				this.internalAxes[i].ConnectToHardwareAxis(internalJoyId, i);

			for (int i = 0; i < this.internalKeys.Length; ++i)
				this.internalKeys[i].ConnectToHardwareKey(internalJoyId, i);
				
			// Reset...


			this.Reset();
			}
				

		// ----------------------
		public void OnDisconnect()
			{
			this.ConnectToJoy(-1, string.Empty); //, null);
			
			}

		// -----------------
		public void OnActivate(int slot)
			{
			this.slot			= slot;
			this.isActivated 	= true;

			this.Reset();
			}

		// -----------------
		public void OnDisactivate()
			{
			this.isActivated = false;	
			this.Reset();
			}
//! \endcond



		// ---------------------
		public GamepadProfile GetProfile()
			{ return this.profile; }


		// -------------------
		public void SetProfile(GamepadProfile profile)
			{
			this.profile = profile;

			this.leftStick	.ConnectToJoy((profile != null) ? profile.leftStick 	: null);
			this.rightStick	.ConnectToJoy((profile != null) ? profile.rightStick 	: null);
			this.dpad		.ConnectToJoy((profile != null) ? profile.dpad			: null);

			for (int i = 0; i < this.keys.Length; ++i)
				this.keys[i].ConnectToJoy((profile != null) ? profile.GetKeySource(i) : null);

			this.Reset();
			}

			
		// --------------------
		public void Reset()
			{
			this.leftStick.Reset();
			this.rightStick.Reset();
			this.dpad.Reset();

			for (int i = 0; i < this.keys.Length; ++i)
				this.keys[i].Reset();
	
			for (int i = 0; i < this.internalAxes.Length; ++i)
				this.internalAxes[i].Reset();

			for (int i = 0; i < this.internalKeys.Length; ++i)
				this.internalKeys[i].Reset();
			}



		// -------------------
		public bool		GetKey		(GamepadKey key)	{ return this.keys[(int)key].GetDigital(); }
		public bool		GetKeyDown	(GamepadKey key)	{ return this.keys[(int)key].GetDigitalDown(); }
		public bool 	GetKeyUp	(GamepadKey key)	{ return this.keys[(int)key].GetDigitalUp(); }			
		public float	GetKeyAnalog(GamepadKey key)	{ return this.keys[(int)key].GetAnalog(); }
			
		
		public Vector2	GetStickVec	(GamepadStick s)	{ return this.GetStick(s).GetVector(); }
		public Dir		GetStickDir8(GamepadStick s)	{ return this.GetStick(s).GetDir8(); }
		public Dir		GetStickDir4(GamepadStick s)	{ return this.GetStick(s).GetDir4(); }
		public Dir		GetStickDir (GamepadStick s)	{ return this.GetStick(s).GetDir(); }

		// --------------------
		public JoystickState GetStick(GamepadStick s) 
			{
			switch (s)
				{
				case GamepadStick.LeftAnalog : return this.leftStick.state; 
				case GamepadStick.RightAnalog : return this.rightStick.state; 
				}
			return this.dpad.state;
			}	
			


		// --------------------
		public bool	 GetInternalKeyState			(int keyId)		{ return (((keyId >= 0) && (keyId < this.internalKeys.Length)) ? this.internalKeys[keyId].GetState() : false); }
		public float GetInternalAxisAnalog		(int axisId)	{ return (((axisId >= 0) && (axisId < this.internalAxes.Length)) ? this.internalAxes[axisId].GetVal() : 0); }
		public int   GetInternalAxisDigital		(int axisId)	{ return (((axisId >= 0) && (axisId < this.internalAxes.Length)) ? this.internalAxes[axisId].GetDigital() : 0); }
		public bool	 IsInternalAxisFullyAnalog	(int axisId)	{ return (((axisId >= 0) && (axisId < this.internalAxes.Length)) ? this.internalAxes[axisId].IsFullyAnalog() : false); }
			
		// ------------------
		public bool AnyInternalKeyOrAxisPressed()
			{ return (this.AnyInternalKeyPressed() || this.AnyInternalAxisPressed()); }

		// ------------------
		public bool AnyInternalKeyPressed()
			{
			for (int i = 0; i < this.internalKeys.Length; ++i)
				{
				if (this.internalKeys[i].GetState())
					return true;
				}
			return false;
			}	

		// ------------------
		public bool AnyInternalAxisPressed()
			{
			for (int i = 0; i < this.internalAxes.Length; ++i)
				{
				if (this.internalAxes[i].GetDigital() != 0)
					return true;
				}
			return false;
			}	

		
		// --------------------
		public int GetPressedInternalKey(int start = 0)
			{
			for (int i = start; i < this.internalKeys.Length; ++i)
				{
				if (this.internalKeys[i].GetState())
					return i;
				}

			return -1;
			}
	
		// --------------------
		public int GetPressedInternalAxis(out bool positiveSide, int start = 0)
			{
			for (int i = start; i < this.internalAxes.Length; ++i)
				{
				int digi = this.internalAxes[i].GetDigital();
				if (digi != 0)
					{
					positiveSide = (digi > 0);			
					return i;
					} 	
				}
	
			positiveSide = false;
			return -1;
			}
		


//! \cond


		// ----------------------
		public void Update()
			{
			if (this.IsConnected())
				{				
				// Update Internal state...
				
				for (int i = 0; i < this.internalKeys.Length; ++i)
					this.internalKeys[i].Update();
	
				for (int i = 0; i < this.internalAxes.Length; ++i)
					this.internalAxes[i].Update();
	
				}
			
			//if (!this.IsActivated())
			//	return;

			// Update end sticks and keys...

			this.leftStick.Update();
			this.rightStick.Update();
			this.dpad.Update();

			for (int i = 0; i < this.keys.Length; ++i)
				{
				this.keys[i].Update();
				}
		
			}

			
		// ----------------------	
		public void ApplyGamepadState(Gamepad g)
			{
			if (g == null)
				return;

			this.leftStick.state	.ApplyState(g.leftStick.state);		
			this.rightStick.state	.ApplyState(g.rightStick.state);		
			this.dpad.state			.ApplyState(g.dpad.state);	

			for (int i = 0; i < this.keys.Length; ++i)
				{
				this.keys[i].SetDigital(g.keys[i].GetDigitalRaw());	
				this.keys[i].SetAnalog(g.keys[i].GetAnalogRaw());	
				}
			}


	
		// ----------------------
		public class JoyState
			{
			private Gamepad
				gamepad;
			public JoystickState	
				state;

			private bool
				//isHardwareReadyForUse,
				isConnectedToHardware;
		
				
			private int	
				srcAxisIdL,
				srcAxisIdR,
				srcAxisIdU,
				srcAxisIdD;
			private bool
				srcAxisFlipL,
				srcAxisFlipR,
				srcAxisFlipU,
				srcAxisFlipD;
	
	
			private int //KeyCode
				srcKeyU,
				srcKeyR,
				srcKeyD,
				srcKeyL;
				

			

			// --------------------
			public JoyState(Gamepad gamepad, JoystickConfig config)
				{
				this.gamepad	= gamepad;
				this.state = new JoystickState(config);
				//this.isHardwareReadyForUse	= false;
				this.isConnectedToHardware	= false;

				}


			// ------------------
			public void SetConfig(JoystickConfig config)
				{
				this.state.SetConfig(config);
				}
				
			// ------------------
			public void Reset()
				{
				//this.isHardwareReadyForUse = false;	
				this.state.Reset();
				}

			// -----------------
			public void Update()
				{
				this.ReadHardwareState();		

				// Update joy state...

				this.state.Update();
				}
				
				
	
			// ---------------------	
			private float GetCompositeAnalogVal(int posAxisId, bool posFlip, int negAxisId, bool negFlip)
				{
				if ((posAxisId >= 0) && (posAxisId == negAxisId))	
					{
					float v = this.gamepad.GetInternalAxisAnalog(posAxisId);
					return (posFlip ? -v : v);
					}

				float posVal = this.gamepad.GetInternalAxisAnalog(posAxisId);
				if (posFlip)
					posVal = -posVal;

				float negVal = this.gamepad.GetInternalAxisAnalog(negAxisId);
				if (negFlip)
					negVal = -negVal;
	
				return (Mathf.Clamp(posVal, 0, 1) + Mathf.Clamp(negVal, -1, 0));
				}


			// --------------------
			private void ReadHardwareState()
				{
				if (!this.isConnectedToHardware)	
					return;

				// Read joystick state...
				
				Vector2 joyVec = Vector2.zero;
					
				joyVec.x = this.GetCompositeAnalogVal(this.srcAxisIdR, this.srcAxisFlipR, this.srcAxisIdL, this.srcAxisFlipL);
				joyVec.y = this.GetCompositeAnalogVal(this.srcAxisIdU, this.srcAxisFlipU, this.srcAxisIdD, this.srcAxisFlipD);




				bool 
					digiU = ((this.srcKeyU >= 0) ? this.gamepad.GetInternalKeyState(this.srcKeyU) : false),
					digiD = ((this.srcKeyD >= 0) ? this.gamepad.GetInternalKeyState(this.srcKeyD) : false),
					digiR = ((this.srcKeyR >= 0) ? this.gamepad.GetInternalKeyState(this.srcKeyR) : false),
					digiL = ((this.srcKeyL >= 0) ? this.gamepad.GetInternalKeyState(this.srcKeyL) : false);
			
					
				// Joystick is ready to be used only after it have been set to neutral first...

					{
					this.state.ApplyClampedVec(joyVec, JoystickConfig.ClampMode.Square);
					this.state.ApplyDigital(digiU, digiR, digiD, digiL);
					}
				}


			// -------------------
			public void ConnectToJoy(//int joyId, 
				GamepadProfile.JoystickSource joySrc)
				{

				this.isConnectedToHardware	= (joySrc != null);

				if (joySrc != null)
					{
					this.srcAxisIdL		=  joySrc.keyL.axisId;
					this.srcAxisFlipL	= !joySrc.keyL.axisSign;
					this.srcAxisIdR		=  joySrc.keyR.axisId;
					this.srcAxisFlipR	= !joySrc.keyR.axisSign;
					this.srcAxisIdU		=  joySrc.keyU.axisId;
					this.srcAxisFlipU	= !joySrc.keyU.axisSign;
					this.srcAxisIdD		=  joySrc.keyD.axisId;
					this.srcAxisFlipD	= !joySrc.keyD.axisSign;

					this.srcKeyL			= joySrc.keyL.keyId;
					this.srcKeyR			= joySrc.keyR.keyId;
					this.srcKeyU			= joySrc.keyU.keyId;
					this.srcKeyD			= joySrc.keyD.keyId;
					}

				this.Reset();
				} 

			
			// -----------------
			public void SyncJoyState(JoystickStateBinding bind, InputRig rig)
				{
				bind.SyncJoyState(this.state, rig);
				}
			}
				

		// -------------------
		//[System.Serializable]
		public class KeyState
			{
			private Gamepad
				gamepad;

			private bool
	 			isTrigger;
				
			private AnalogConfig
				analogConfig;

			public bool
				//isHardwareReadyForUse,
				isConnectedToHardware;

			private float
				frAnalog;
			private bool		
				frDigital;

			private float
				analogCur,
				analogRawCur;
			private bool
				digiCurRaw,
				digiCur,
				digiPrev;
		
			private int
				srcAxisId,
				srcKeyId;
			private bool 
				srcAxisSign;
			

	
			// --------------------
			public KeyState(Gamepad gamepad, bool isTrigger, AnalogConfig analogConfig = null)
				{
				this.gamepad				= gamepad;
				this.isTrigger				= isTrigger;		
				this.analogConfig			= analogConfig;
				this.isConnectedToHardware	= false;

				this.Reset();
				}
				


			// --------------------
			public void SetConfig(AnalogConfig config)
				{
				this.analogConfig = config;
				}


			// ----------------
			public void SetDigital(bool state)
				{
				if (state)
					this.frDigital = true;
				}

			// -----------------
			public void SetAnalog(float val)
				{
				if (val > this.frAnalog)
					this.frAnalog = Mathf.Clamp01(val); 
				}
				
		

			// ---------------
			public bool		GetDigitalRaw()	{ return this.digiCurRaw; } 
			public bool		GetDigital()	{ return (this.digiCur); }
			public bool		GetDigitalDown(){ return (this.digiCur && !this.digiPrev); }
			public bool		GetDigitalUp()	{ return (!this.digiCur && this.digiPrev); }

			public float	GetAnalog()		{ return (this.analogCur); }
			public float 	GetAnalogRaw()	{ return (this.analogRawCur); }
				
			public bool 	IsTrigger()		{ return this.isTrigger; }
				


			// ------------------
			public void Reset()
				{
				this.analogCur				= 0;
				this.analogRawCur 			= 0;
				//this.digiPrev			= false;
				this.digiCur				= false;
				this.digiPrev				= false;
				this.digiCurRaw				= false;
				this.frAnalog				= 0;
				this.frDigital				= false;
				}

			// ---------------------
			public void Update()
				{
				this.ReadHardwareState();
					
				this.digiPrev			= this.digiCur;
				
				this.digiCurRaw			= this.frDigital;
				this.analogRawCur		= this.frAnalog;		
				

				this.frAnalog = 0;
				this.frDigital = false;
							

				// Apply analog transform...

				if (this.analogConfig != null)
					{
					this.analogCur	= (this.analogConfig.GetAnalogVal(this.digiCurRaw ? 1.0f : this.analogRawCur));
					this.digiCur	= (this.digiCurRaw || this.analogConfig.GetDigitalVal(this.analogRawCur, this.digiPrev));
					}
				else
					{
					this.analogCur 	= (this.digiCurRaw ? 1.0f : this.analogRawCur);
					this.digiCur	= (this.digiCurRaw || (this.analogRawCur > 0.5f));					
					}
				}
				

			// ------------------
			private void ReadHardwareState()
				{
				if (!this.isConnectedToHardware)
					return;
					

				bool digiVal = ((this.srcKeyId >= 0) ? this.gamepad.GetInternalKeyState(this.srcKeyId) : false);
				float analogVal = 0;
				
				if (this.srcAxisId >= 0)
					{
					analogVal = this.gamepad.GetInternalAxisAnalog(this.srcAxisId);
					analogVal = Mathf.Clamp01(this.srcAxisSign ?analogVal : -analogVal);
					}
					

				this.SetAnalog(analogVal);
				this.SetDigital(digiVal);					
				}	


			// -------------------
			public void ConnectToJoy(//int joyId, 
				GamepadProfile.KeySource src)
				{
				this.isConnectedToHardware = (src != null);
				this.srcAxisId	= -1;
				this.srcKeyId = -1;
				
				if (src != null)
					{
					this.srcKeyId = src.keyId;
					this.srcAxisId = src.axisId;
					this.srcAxisSign = src.axisSign;
					} 

				this.Reset();
				} 
				


			// ---------------------
			public void SyncDigital(DigitalBinding bind, InputRig rig)
				{
				if (this.GetDigital())
					bind.Sync(true, rig);
				}

			// ----------------
			public void SyncAnalog(AxisBinding bind, InputRig rig)
				{
				bind.SyncFloat(this.GetAnalog(), InputRig.InputSource.Analog, rig);
				bind.SyncFloat((this.GetDigitalRaw() ? 1 : 0), InputRig.InputSource.Digital, rig);
				}

			}


		// ----------------------
		private class InternalKeyState
			{	
			private KeyCode
				key;
			public bool	
				stateCur,	
				statePrev;
			public bool
				isReadyToUse;


			// -----------------
			public InternalKeyState()
				{
				this.isReadyToUse	= false;
				this.stateCur		= false;
				this.stateCur 		= false;
				this.key			= KeyCode.None;
				}
				
				
			// -----------------
			public bool GetState()
				{
				return (this.isReadyToUse ? this.stateCur : false);
				}

			// -------------------
			public void Reset()	
				{
				this.stateCur = this.statePrev = false;
				this.isReadyToUse = false;
				}
				
			// ------------------
			public void Update()
				{
				this.statePrev = this.stateCur;
				this.ReadHardwareState();

				//if (!this.isReadyToUse && !this.statePrev)
				if (!this.isReadyToUse && !this.stateCur)
					this.isReadyToUse = true;					
				}

			// ------------------
			private void ReadHardwareState()
				{
				this.stateCur = ((this.key == KeyCode.None) ? false : UnityEngine.Input.GetKey(this.key));
				}

			// ------------------
			public void ConnectToHardwareKey(int joyId, int keyId)
				{
				this.key = GamepadManager.GetJoyKeyCode(joyId, keyId);
				this.Reset();
				}
			}

		// ----------------------
		private class InternalAxisState
			{	
			private string
				axisName;
			public float 
				valCur,
				valPrev;
			public bool
				isReadyToUse;
			public bool	
				trueAnalogRange;				

				
				
			// --------------------
			// Internal Axis State
			// --------------------
			public InternalAxisState()
				{
				this.isReadyToUse	= false;
				this.valCur			= this.valPrev = 0;
				this.axisName		= string.Empty;
				this.trueAnalogRange = false;
				}
				
				
			// -----------------
			public float GetVal()
				{
				return (this.isReadyToUse ? this.valCur : 0);
				}
				
			public int GetDigital()
				{
				return ((this.isReadyToUse && (Mathf.Abs(this.valCur) > 0.5f)) ? (this.valCur > 0) ? 1 : -1 : 0);
				}


			// -------------------
			public void Reset()	
				{
				this.valCur = this.valPrev = 0;
				this.isReadyToUse		= false;
				this.trueAnalogRange	= false;
				}
				
			// ------------------
			public void Update()
				{
				this.valPrev = this.valCur;
				this.ReadHardwareState();

				if (!this.isReadyToUse && (Mathf.Abs(this.valCur) < 0.1f))
					this.isReadyToUse = true;					
				}

		
			// ------------------
			public void SetVal(float v)
				{
				this.valCur = v;

				if (!this.trueAnalogRange && (Mathf.Abs(v) > 0.01f) && (Mathf.Abs(v) < 0.99f))
					this.trueAnalogRange = true; 
				}

			// ------------------
			private void ReadHardwareState()
				{
				this.SetVal((string.IsNullOrEmpty(this.axisName) ? 0 : UnityEngine.Input.GetAxisRaw(this.axisName))); 
				}

			// ------------------
			public void ConnectToHardwareAxis(int joyId, int axisId)
				{
				this.axisName = GamepadManager.GetJoyAxisName(joyId, axisId);
				this.Reset();
				}

			// ---------------------
			public bool IsFullyAnalog() 
				{ return this.trueAnalogRange; }
			}

//! \endcond


		}



	}
}

