// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace ControlFreak2
{

// ---------------------
//! Touch Control Panel Unity5 UI Add-on. This component should be added to touch control panel when used with Unity UI.
// ---------------------
[ExecuteInEditMode()]

public class TouchControlPanelUnityUiAddOn : Graphic, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
//! \cond

	private TouchControlPanel panel;
	private TouchControlPanel.SystemTouchEventData	eventData;
		


	// ------------------
	public bool IsConnectedToPanel()
		{
		return (this.panel != null);
		}

	// -------------------
	public TouchControlPanelUnityUiAddOn() : base()
		{
		this.eventData = new TouchControlPanel.SystemTouchEventData(); 
		}

		
	// --------------------
	private TouchControlPanel.SystemTouchEventData TranslateEvent(PointerEventData data)
		{
		this.eventData.id			= data.pointerId;
		this.eventData.pos			= data.position;
		this.eventData.isMouseEvent	= (data.pointerId < 0);
		this.eventData.cam			= data.pressEventCamera;
	
		return this.eventData;
		}

	// -------------------------
	override protected void Awake()
		{
		base.Awake();
		this.panel = this.GetComponent<TouchControlPanel>();

#if UNITY_EDITOR
		int eventSysPresence = ControlFreak2Editor.TouchControlWizardUtils.IsThereEventSystemInTheScene();
		if (eventSysPresence == 0)
			Debug.LogWarning("There's no event system in the scene! If this is not intentional, create one to be able to use CF2 Touch Controls...");
		
#endif
		}
		

	// ----------------------
	override protected void OnEnable()
		{
		base.OnEnable();


		}


//! \cond 
	
	// ---------------------
	override public bool Raycast(Vector2 sp, Camera eventCamera)
		{
		if (this.panel == null) return false;
		
		if (this.panel.Raycast(sp, eventCamera))
			{
			return true;			
			}
		else
			{
			return false;
			}
		}

	// -------------------
	void IPointerDownHandler.OnPointerDown(PointerEventData data) 
		{
		if (this.panel != null)
			this.panel.OnSystemTouchStart(this.TranslateEvent(data));
		}		

	// ----------------------
	void IPointerUpHandler.OnPointerUp(PointerEventData data)
		{
		if (this.panel != null)
			this.panel.OnSystemTouchEnd(this.TranslateEvent(data));
		}

	// ----------------------
	void IDragHandler.OnDrag(PointerEventData data)
		{
		if (this.panel != null)
			this.panel.OnSystemTouchMove(this.TranslateEvent(data));
		}


	// -----------------
	override protected void UpdateGeometry()
		{
		if (this.canvasRenderer != null)
			this.canvasRenderer.Clear();

		}


	// ------------------------
	void OnDrawGizmos()			{ this.DrawGizmos(false); }
	void OnDrawGizmosSelected()	{ this.DrawGizmos(true); }


	// -------------------------
	private void DrawGizmos(bool selected)
		{
		RectTransform rectTr = this.transform as RectTransform;
		if (rectTr == null)
			return;
			
		Gizmos.color = (selected ? Color.red : Color.white);

		Gizmos.matrix = rectTr.localToWorldMatrix;
		Gizmos.DrawWireCube(rectTr.rect.center, rectTr.rect.size);
		}


//! \endcond

	}
}
