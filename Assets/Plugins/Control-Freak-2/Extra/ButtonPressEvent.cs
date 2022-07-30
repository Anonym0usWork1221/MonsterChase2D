
﻿using UnityEngine;
using UnityEngine.Events;
using ControlFreak2;

namespace ControlFreak2.Extra
{
public class ButtonPressEvent : MonoBehaviour 
	{

	// ----------------	
	public enum EventType
		{
		OnPress,
		OnRelease,
		WhenPressed
		}


	public string 
		buttonName = "Fire1";
	private int 
		buttonId;

	public EventType
		eventType;

	public UnityEvent 
		buttonEvent;


	// -------------------
	public ButtonPressEvent() : base()
		{
		this.buttonEvent = new UnityEvent();
		}
	

	// ------------------
	void Update()
		{
		bool 
			invokeNow = false;

		switch (this.eventType)
			{
			case EventType.OnPress :
				invokeNow = CF2Input.GetButtonDown(this.buttonName, ref this.buttonId); break;
			case EventType.OnRelease :
				invokeNow = CF2Input.GetButtonUp(this.buttonName, ref this.buttonId); break;
			case EventType.WhenPressed :
				invokeNow = CF2Input.GetButton(this.buttonName, ref this.buttonId); break;
			}

		if (invokeNow)
			this.buttonEvent.Invoke();
		}


	}
	
}