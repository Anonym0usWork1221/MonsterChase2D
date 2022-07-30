
﻿using UnityEngine;
using UnityEngine.Events;
using ControlFreak2;

namespace ControlFreak2.Extra
{
public class AxisEvent : MonoBehaviour 
	{

	// ----------------	
	public enum EventType
		{
		OnPress,
		OnRelease,
		WhenPressed
		}


	public string 
		axisName = "Horizontal";

	[Range(0.1f, 0.9f)]
	public float
		axisDeadzone = 0.5f;

	private int 
		axisId;

	private int
		prevAxisDir;

	public EventType
		eventType;

	public UnityEvent 
		positiveEvent,
		negativeEvent;


	// -------------------
	public AxisEvent() : base()
		{
		this.positiveEvent = new UnityEvent();
		this.negativeEvent = new UnityEvent();
		}
	

	// ----------------
	void OnEnable()
		{
		this.prevAxisDir = 0;
		}


	// ------------------
	void Update()
		{
		float axisVal = CF2Input.GetAxisRaw(this.axisName, ref this.axisId);
		int axisDir = (Mathf.Abs(axisVal) > this.axisDeadzone) ? ((axisVal > 0) ? 1 : -1) : 0;
		
		
		switch (this.eventType)
			{
			case EventType.OnPress : 
				if ((axisDir != 0) && (this.prevAxisDir != axisDir))
					{
					((axisDir > 0) ? this.positiveEvent : this.negativeEvent).Invoke();
					}
				break;

			case EventType.OnRelease : 
				if ((this.prevAxisDir != 0) && (axisDir != this.prevAxisDir))
					{
					((this.prevAxisDir > 0) ? this.positiveEvent : this.negativeEvent).Invoke();
					}
				break;

			case EventType.WhenPressed : 
				if ((axisDir != 0))
					{
					((axisDir > 0) ? this.positiveEvent : this.negativeEvent).Invoke();
					}
				break;
			}

		this.prevAxisDir = axisDir;
		}


	}
	
}