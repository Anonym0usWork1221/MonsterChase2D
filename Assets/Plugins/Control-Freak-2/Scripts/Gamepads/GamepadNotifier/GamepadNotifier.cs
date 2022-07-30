// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------


//! \cond

using UnityEngine;
using System.Collections.Generic;


namespace ControlFreak2
{

public class GamepadNotifier : MonoBehaviour 
	{
	public int 
		maxNotifications = 8;


	public float
		msgDuration = 5.0f;


	public NotificationElementGUI
		notificationTemplate;

	public RectTransform 
		notificationListBox;

	private List<NotificationElementGUI> 
		notificationList;
		

	public string
		msgUnknownGamepadConnected	= "Unknown gamepad {0} has been connected!",
		msgKnownGamepadConnected	= "Gamepad {0} has been connected!",
		msgGamepadDisconnected		= "Gamepad {0} at slot {1} has been disconnected.",
		msgGamepadActivated			= "Gamepad {0} activated at slot {1}.",
		msgGamepadDisactivated		= "Gamepad {0} disactivated at slot {1}.";
		
	public Sprite
		iconUnknownGamepadConnected,
		iconKnownGamepadConnected,
		iconGamepadDisconnected,
		iconGamepadActivated,
		iconGamepadDisactivated;



	// -------------------
	public GamepadNotifier() : base()	
		{
		this.notificationList = new List<NotificationElementGUI>(8);
		}
		

	// ------------------
	void OnEnable()
		{
		if (CFUtils.editorStopped)
			return;

		this.maxNotifications = Mathf.Max(this.maxNotifications, 1);

		if (this.notificationTemplate == null)
			return;

		this.notificationTemplate.gameObject.SetActive(false);
		
		while (this.notificationList.Count < maxNotifications)
			this.notificationList.Add(null);

		for (int i = 0; i < this.notificationList.Count; ++i)
			{
			if (this.notificationList[i] == null)
				this.notificationList[i] = (NotificationElementGUI)NotificationElementGUI.Instantiate(this.notificationTemplate);

			this.notificationList[i].End();
			this.notificationList[i].transform.SetParent(this.notificationListBox, false);	
			}


		GamepadManager.onGamepadActivated += this.OnGamepadActivated;
		GamepadManager.onGamepadConnected += this.OnGamepadConnected;
		GamepadManager.onGamepadDisconnected += this.OnGamepadDisconnected;
		GamepadManager.onGamepadDisactivated += this.OnGamepadDisactivated;
		}
		
	// --------------------
	void OnDisable()
		{
		if (CFUtils.editorStopped)
			return;

		GamepadManager.onGamepadActivated 		-= this.OnGamepadActivated;
		GamepadManager.onGamepadConnected 		-= this.OnGamepadConnected;
		GamepadManager.onGamepadDisconnected	-= this.OnGamepadDisconnected;
		GamepadManager.onGamepadDisactivated 	-= this.OnGamepadDisactivated;
		}


	// -----------------
	void Update()
		{
		if (GamepadManager.activeManager != null)
			{
			
			}

		float dt = CFUtils.realDeltaTimeClamped;

		for (int i = 0; i < this.notificationList.Count; ++i)
			{
			if (this.notificationList[i] != null)
				this.notificationList[i].UpdateNotification(dt);
			}
		}



	// -------------------
	private void OnGamepadConnected(GamepadManager.Gamepad gamepad)
		{
		//this.AddNotification(true, gamepad);
		string name = gamepad.GetProfileName();

		if (string.IsNullOrEmpty(name))
			this.AddNotification(string.Format(this.msgUnknownGamepadConnected, gamepad.GetInternalJoyName()), this.iconUnknownGamepadConnected);
		else	
			this.AddNotification(string.Format(this.msgKnownGamepadConnected, gamepad.GetProfileName()), this.iconKnownGamepadConnected);

			//this.AddNotification("Gamepad (" + name + ") has been connected!"); //, Color.white);
		}

	// ------------------
	private void OnGamepadActivated(GamepadManager.Gamepad gamepad)
		{
		this.AddNotification(string.Format(this.msgGamepadActivated, gamepad.GetProfileName(), gamepad.GetSlot()), this.iconGamepadDisactivated); 
		//this.AddNotification("Gamepad " + gamepad.GetProfileName() + " activated at slot "  + gamepad.GetSlot() + "!"); //, Color.white);
		}

	// -------------------
	private void OnGamepadDisconnected(GamepadManager.Gamepad gamepad, GamepadManager.DisconnectionReason reason)
		{
		this.AddNotification(string.Format(this.msgGamepadDisconnected, gamepad.GetProfileName(), gamepad.GetSlot()), this.iconGamepadDisconnected); 
		}

	// -------------------
	private void OnGamepadDisactivated(GamepadManager.Gamepad gamepad, GamepadManager.DisconnectionReason reason)
		{
		//this.AddNotification(string.Format(this.msgGamepadDisconnected, gamepad.GetProfileName(), gamepad.GetSlot()), this.iconGamepadDisconnected); 
		}

		
	// -----------------
	private void AddNotification(string msg, Sprite icon) //, Color color, Texture2D icon = null)
		{
		if (string.IsNullOrEmpty(msg))
			return;

		NotificationElementGUI notification = null;
		
		for (int i = 0; i < this.notificationList.Count; ++i)
			{
			NotificationElementGUI n = this.notificationList[i]; 
			if (n == null)
				continue;

			if (!n.IsActive())
				{
				notification = n;
				break;
				}
			else if ((notification == null) || (n.GetTimeElapsed() > notification.GetTimeElapsed()))
				notification = n; 
			}

		if (notification == null)
			return;

		notification.End();
		notification.Show(msg, icon, this.notificationListBox, this.msgDuration);

		this.SortNotifications();
		}
		

	// -------------------
	private void SortNotifications()
		{
		this.notificationList.Sort(NotificationElementGUI.Compare);
		}



	}
}

//! \endcond
