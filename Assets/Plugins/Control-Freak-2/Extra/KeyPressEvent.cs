
﻿using UnityEngine;
using UnityEngine.Events;
using ControlFreak2;

namespace ControlFreak2.Extra
{
public class KeyPressEvent : MonoBehaviour 
	{

	// ----------------	
	public enum EventType
		{
		OnPress,
		OnRelease,
		WhenPressed
		}
#if false

void XXX()
	{
	InputRig rig = CF2Input.activeRig;
	if (rig != null)
		{

		InputRig.AxisConfig config = rig.GetAxisConfig("YourAxisName");
		if (config != null)
			{
			config.keyboardPositive = KeyCode.A;
			config.keyboardPositiveAlt0 = KeyCode.B;
			config.keyboardPositiveAlt1 = KeyCode.C;
			config.keyboardPositiveAlt2 = KeyCode.D;
	
			config.keyboardPositive = KeyCode.E;
			config.keyboardPositiveAlt0 = KeyCode.F;
			config.keyboardPositiveAlt1 = KeyCode.G;
			config.keyboardPositiveAlt2 = KeyCode.H;
			}
		}
	}

#endif

	public KeyCode 
		keyCode = KeyCode.None;

	public EventType
		eventType;

	public UnityEvent 
		keyEvent;


	// -------------------
	public KeyPressEvent() : base()
		{
		this.keyEvent = new UnityEvent();
		}
	

	// ------------------
	void Update()
		{
		bool 
			invokeNow = false;

		switch (this.eventType)
			{
			case EventType.OnPress :
				invokeNow = CF2Input.GetKeyDown(this.keyCode); break;
			case EventType.OnRelease :
				invokeNow = CF2Input.GetKeyUp(this.keyCode); break;
			case EventType.WhenPressed :
				invokeNow = CF2Input.GetKey(this.keyCode); break;
			}

		if (invokeNow)
			this.keyEvent.Invoke();
		}


	}
	
}