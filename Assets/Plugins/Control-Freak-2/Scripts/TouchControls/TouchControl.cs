// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using ControlFreak2.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace ControlFreak2
{
// ---------------------
//! Touch Control Base class.
// ----------------------

[RequireComponent(typeof(RectTransform))]
public abstract class TouchControl : ControlFreak2.Internal.ComponentBase, IBindingContainer
	{
	//! Control Shape 
	public enum Shape
		{
		Rectangle,
		Square,
		Circle,
		Ellipse
		}
	

	//! Swipe Off Mode 
	public enum SwipeOffMode
		{
		Disabled,
		Enabled,
		OnlyIfSwipedOver
		}

	//! Swipe Over Others Mode 	
	public enum SwipeOverOthersMode
		{
		Disabled,
		Enabled,
		OnlyIfTouchedDirectly
		}

//! \cond	

	public bool 	
		ignoreFingerRadius,				// This control can be only touched by touch's center.
		cantBeControlledDirectly,		// Enable this for controls activated by other controls only.
		shareTouch,
		dontAcceptSharedTouches,
		canBeSwipedOver,	
		//swipeOverToOtherControls,
		restictSwipeOverTargets;

	public SwipeOverOthersMode
		swipeOverOthersMode	= SwipeOverOthersMode.OnlyIfTouchedDirectly;

	public SwipeOffMode
		swipeOffMode = SwipeOffMode.OnlyIfSwipedOver;




	
	public List<TouchControl>
		swipeOverTargetList;

	
	public Shape 
		shape;

	[System.NonSerialized]
	protected List<TouchControlAnimatorBase> 
		animatorList;


	public DisablingConditionSet
		disablingConditions;

	[System.NonSerializedAttribute]
	private int 
		hidingFlagsCur;


//! \endcond	
		

		
	[System.NonSerializedAttribute]
	private	InputRig				_rig;
	public 	InputRig				rig 	{ get { return this._rig; } protected set { this._rig = value; } }		//!< This control's Input rig.

	[System.NonSerializedAttribute] 
	private	Canvas				_canvas;
	public 	Canvas				canvas	{ get { return this._canvas; } protected set { this._canvas = value; } }	//!< This control's Canvas.

	[System.NonSerializedAttribute]
	private TouchControlPanel	_panel;
	public TouchControlPanel	panel { get { return this._panel; } protected set { this._panel = value; } }	//!< This control's Touch Control Panel.


//! \cond

	protected const int 
		HIDDEN_BY_USER				= 0,
		HIDDEN_BY_DISABLED_GO	= 1,
		//HIDDEN_BY_RIG_FLAGS			= 1,
		//HIDDEN_BY_GAMEPAD			= 2,
		//HIDDEN_BY_MOBILE_MODE		= 3,
		//HIDDEN_BY_LOCKED_CURSOR		= 4,
		HIDDEN_BY_CONDITIONS		= 4, 
		HIDDEN_BY_RIG				= 5, 
		HIDDEN_DUE_TO_INACTIVITY	= 6;

	protected const int
		HIDDEN_AND_DISABLED_MASK	= ~(1 << HIDDEN_DUE_TO_INACTIVITY);	//((1 << HIDDEN_BY_USER) | (1 << HIDDEN_BY_RIG_FLAGS) | (1 << HIDDEN_BY_MOBILE_MODE));


		
	
	[System.NonSerializedAttribute]
	private bool 
		isHidden;

	[System.NonSerializedAttribute]
	private bool 
		baseAlphaAnimOn;		
	[System.NonSerializedAttribute]
	private float
		baseAlphaStart,
		baseAlphaEnd,
		baseAlphaCur,
		baseAlphaAnimDur,
		baseAlphaAnimElapsed;


//! \endcond



	// ------------------
	public TouchControl() : base()
		{	
		this.disablingConditions = new DisablingConditionSet(null);
		//this.hidingConditions 	= new HidingConditions();
		this.animatorList		= new List<TouchControlAnimatorBase>(2);
		this.swipeOverTargetList = new List<TouchControl>(2);
	
		}

//! \cond

	// ---------------------
	abstract protected	void OnInitControl(); 
	abstract protected	void OnUpdateControl(); 
	abstract protected	void OnDestroyControl();

		
	public void GetSubBindingDescriptions(
		BindingDescriptionList			descList, 
		Object 							undoObject,
		string 							parentMenuPath)
		{
		this.OnGetSubBindingDescriptions(descList, undoObject, parentMenuPath); 
		}

	public 		bool IsBoundToAxis(string axisName, InputRig rig)	{ return this.OnIsBoundToAxis(axisName, rig); }
	public 		bool IsBoundToKey(KeyCode key, InputRig rig) 		{ return this.OnIsBoundToKey(key, rig); }
	public		bool IsEmulatingTouches		()								{ return this.OnIsEmulatingTouches(); }
	public		bool IsEmulatingMousePosition()							{ return this.OnIsEmulatingMousePosition(); }

	virtual public bool IsUsingKeyForEmulation(KeyCode key) 			{ return false; }

	virtual protected void OnGetSubBindingDescriptions(
		BindingDescriptionList			descList, 
		Object 							undoObject,
		string 							parentMenuPath)  {}

	virtual protected bool OnIsBoundToAxis(string axisName, InputRig rig) 	{ return false; }
	virtual protected bool OnIsBoundToKey(KeyCode key, InputRig rig) 			{ return false; }
	virtual protected	bool OnIsEmulatingTouches()									{ return false; }
	virtual protected	bool OnIsEmulatingMousePosition()							{ return false; }


		

	// ---------------
	public enum TouchStartType
		{
		DirectPress,
		ProxyPress,
		SwipeOver
		}
	
	// ---------------	
	public enum TouchEndType
		{
		Release,
		Cancel,
		SwipeOff
		}

//! \endcond


	//! Reset this control and release any touches controlling it.
	abstract public	void ResetControl();

	//! Release all touches controllign this control.
	abstract public void ReleaseAllTouches(); 

//! \cond
	abstract public bool OnTouchStart	(TouchObject touch, TouchControl sender, TouchStartType touchStartType); 
	abstract public bool OnTouchEnd		(TouchObject touch, TouchEndType touchEndType); 
	abstract public bool OnTouchMove		(TouchObject touch); 
	abstract public bool OnTouchPressureChange(TouchObject touch); 

	// ------------------
	protected bool CheckSwipeOff(TouchObject touchObj, TouchStartType touchStartType)
		{
		if ((touchObj == null) || (this.swipeOffMode == SwipeOffMode.Disabled) ||
			((this.swipeOffMode == SwipeOffMode.OnlyIfSwipedOver) && (touchStartType != TouchStartType.SwipeOver)))
			return false;


		if (!this.RaycastScreen(touchObj.screenPosCur, touchObj.cam))
			{
			touchObj.ReleaseControl(this, TouchEndType.SwipeOff);
			return true;
			}
		return false;
		}		



	// --------------------
	virtual public bool CanShareTouchWith(TouchControl c)		
		{ return true; }

	// ---------------------
	abstract public bool	CanBeSwipedOverFromNothing		(TouchObject touchObj);
	protected bool			CanBeSwipedOverFromNothingDefault(TouchObject touchObj)
		{
		return (this.canBeSwipedOver && this.IsActive());
		}

	// ---------------------
	abstract public bool	CanBeSwipedOverFromRestrictedList		(TouchObject touchObj);
	protected bool			CanBeSwipedOverFromRestrictedListDefault(TouchObject touchObj)
		{
		return (this.IsActive());
		}


	// --------------------
	abstract public bool	CanSwipeOverOthers		(TouchObject touchObj);
	protected bool 			CanSwipeOverOthersDefault(TouchObject touchObj, TouchObject myTouchObj, TouchStartType touchStartType)
		{
		return ((myTouchObj == touchObj) && ((this.swipeOverOthersMode == SwipeOverOthersMode.Enabled) || 
			((this.swipeOverOthersMode == SwipeOverOthersMode.OnlyIfTouchedDirectly) && (touchStartType != TouchStartType.SwipeOver))));

		}


	// -----------------------
	virtual public bool CanBeTouchedDirectly(TouchObject touchObj)
		{
		return (!this.cantBeControlledDirectly && this.IsActive());
		}

	// ----------------------
	virtual public bool CanBeActivatedByOtherControl(TouchControl c, TouchObject touchObj)
		{
		return (this.IsActive());
		}



	// -----------------
	static public int CompareByDepth(TouchControl a, TouchControl b)	
		{	
		if ((a == null) || (b == null))
			return (((a == null) && (b == null)) ? 0 : (a == null) ? 1 : -1);

		float za = a.transform.position.z;
		float zb = a.transform.position.z;

		return ((Mathf.Abs(za - zb) < 0.001f) ? 0 : (za < zb) ? -1 : 1);
		}


//! \endcond
	

		

	// ----------------------
	//! Manually show or hide this control. 
	// ------------------------ 
	public void ShowOrHideControl(bool show, bool noAnim = false)
		{ this.SetHidingFlag(HIDDEN_BY_USER, !show);  this.SyncBaseAlphaToHidingConditions(noAnim); }

	// ----------------------
	//! Manually unhide this control. 
	// ------------------------ 
	public void ShowControl(bool noAnim = false)				
		{ this.ShowOrHideControl(true, noAnim); }

	// ----------------------
	//! Manually hide this control. 
	// ------------------------ 
	public void HideControl(bool noAnim = false)
		{ this.ShowOrHideControl(false, noAnim); }

	// ----------------------
	//! Is this control manually hidden?
	// ------------------------ 
	public bool IsHiddenManually()			{ return ((this.hidingFlagsCur | HIDDEN_BY_USER) != 0);  }	

	
	// ----------------------
	//! Toggle visibility.
	// ------------------------ 
	public void ToggleVisibility(bool noAnim)			
		{ this.ShowOrHideControl(this.IsHiddenManually(), noAnim); }
	public void ToggleVisibility()			
		{ this.ShowOrHideControl(this.IsHiddenManually(), false); }

	// ----------------------
	//! Is this control active but invisble (sleeping, for example)?
	// ------------------------ 
	public bool IsActiveButInvisible()		{ return (this.IsActive() && (this.GetAlpha() <= 0.0001f)); } //this.hidingFlagsCur != 0); }

	// ----------------------
	//! Is this cotnrol active and visible?
	// ------------------------ 
	public bool IsActiveAndVisible()		{ return (this.IsActive() && (this.GetAlpha() > 0.0001f)); } //this.hidingFlagsCur != 0); }

	// ----------------------
	//! Is this control active?
	// ------------------------ 
	public bool IsActive()					{ return ( ((this.hidingFlagsCur & HIDDEN_AND_DISABLED_MASK) == 0)); }		
		


//! \cond
	// --------------------
	public void SetHidingFlag(int flagBit, bool state)
		{
		if ((flagBit < 0) || (flagBit > 31))
			return;

		this.hidingFlagsCur = (state ? (this.hidingFlagsCur | (1 << flagBit)) : (this.hidingFlagsCur & ~(1 << flagBit)));

		if (!this.IsActive())
			this.ReleaseAllTouches(); //false); 
		}


	// -------------------	
	public void SyncDisablingConditions(bool skipAnim)
		{
		this.SetHidingFlag(HIDDEN_BY_DISABLED_GO, (!this.enabled || !this.gameObject.activeInHierarchy)); 

		if (this.rig != null)
			{
			
			this.SetHidingFlag(TouchControl.HIDDEN_BY_CONDITIONS, 		this.disablingConditions.IsInEffect());
	
			this.SetHidingFlag(TouchControl.HIDDEN_BY_RIG,				this.rig.AreTouchControlsHiddenManually());
			}
	
// Debug.LogFormat("Syncing Hide Flags [{0}] = [{1}]", this.name, 
// ((this.hidingFlagsCur&(1<<TouchControl.HIDDEN_BY_DISABLED_GO))!=0?"GO|":"") + 
// ((this.hidingFlagsCur&(1<<TouchControl.HIDDEN_BY_RIG))!=0?"RIG|":"") + 
// ((this.hidingFlagsCur&(1<<TouchControl.HIDDEN_BY_CONDITIONS))!=0?"COND|":"") + 
// (((this.hidingFlagsCur&(1<<TouchControl.HIDDEN_BY_USER))!=0?"USER|":"") + 
// ((this.hidingFlagsCur&(1<<TouchControl.HIDDEN_DUE_TO_INACTIVITY))!=0?"Incact|":""))); 

		this.SyncBaseAlphaToHidingConditions(skipAnim);

		if (skipAnim)
			this.UpdateAnimators(skipAnim);
		}
		
		

	// ----------------
	public void AddAnimator(TouchControlAnimatorBase a)
		{
		if (this.animatorList.Contains(a))
			{
#if UNITY_EDITOR
			Debug.Log("[" + Time.frameCount + "] Control[" + this.name + "] already contains [" + a.name + "] animator!"); 
#endif
			return;
			}

		this.animatorList.Add(a);
		}
	
		
	// -------------------
	public void RemoveAnimator(TouchControlAnimatorBase a)
		{
		if (!this.animatorList.Contains(a))
			{
//#if UNITY_EDITOR
//			Debug.Log("[" + Time.frameCount + "] Attempting to remove [" + a.name + "] animator from control [" + this.name + "]!"); 
//#endif
			return;
			}

		this.animatorList.Remove(a);
		}
		

//! \endcond

	// --------------------
	//! Get this control's animator list. Don't modify this list!
	// -----------------
	public List<TouchControlAnimatorBase> GetAnimatorList()
			{ return this.animatorList; }

//! \cond
	
	// ---------------
	protected void UpdateAnimators(bool skipAnim)
		{
		for (int i = 0; i < this.animatorList.Count; ++i)
			{
			this.animatorList[i].UpdateAnimator(skipAnim);
			}
		}


	// -------------------
	private void SyncBaseAlphaToHidingConditions(bool noAnim)
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			{
			this.StartAlphaAnim(1, 0);
			return;
			}
#endif

		float targetAlpha = (this.hidingFlagsCur == 0) ? 1 : 0;
//CFUtils.Log(this.name + " " + ((targetAlpha < 1) ? "HIDEEEEEEEEEEEEE!" : "SHOW"));
		if (Mathf.Abs(targetAlpha - (this.baseAlphaAnimOn ? this.baseAlphaEnd : this.baseAlphaCur)) > 0.001f)	
			this.StartAlphaAnim(targetAlpha, (noAnim || (this.rig == null)) ? 0 : this.rig.controlBaseAlphaAnimDuration);
		}
		

	// --------------------
	private void StartAlphaAnim(float targetAlpha, float duration)
		{
		this.baseAlphaStart = this.baseAlphaCur;
		this.baseAlphaEnd = targetAlpha;

		if (duration <= 0.0001f)
			{
			this.baseAlphaAnimOn = false;
			this.baseAlphaCur = this.baseAlphaEnd;
			}	
		else
			{
			this.baseAlphaAnimOn = true;
			this.baseAlphaAnimElapsed = 0;
			this.baseAlphaAnimDur = duration;
			}	
		}
		
		

	// --------------------
	private void UpdateBaseAlpha()
		{
		if (this.baseAlphaAnimOn)
			{
	
			this.baseAlphaAnimElapsed += CFUtils.realDeltaTime;
			if (this.baseAlphaAnimElapsed >= this.baseAlphaAnimDur)
				{
				this.baseAlphaCur = this.baseAlphaEnd;
				this.baseAlphaAnimOn = false;
				}
			else
				{
				this.baseAlphaCur = Mathf.Lerp(this.baseAlphaStart, this.baseAlphaEnd, (this.baseAlphaAnimElapsed / this.baseAlphaAnimDur));
				}
			}		
		}
/** \endcond */

		
		
	// -----------------------
	//! Get control's base alpha value.
	// -----------------------
	public float GetBaseAlpha()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return 1.0f;
#endif
		return this.baseAlphaCur;
		}

	// -----------------------
	//! Get control's final alpha value.
	// -----------------------
	virtual public float GetAlpha()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			return 1.0f;
#endif

		return this.baseAlphaCur;
		}

		


	// -------------------	
	//! Get control's canvas camera. 
	// --------------------
	public Camera GetCamera()
		{
		return (((this.canvas != null) && (this.canvas.renderMode != RenderMode.ScreenSpaceOverlay)) ? this.canvas.worldCamera : null);
		}


	
	
/** \cond */

	// ----------------------
	public void SetRig(InputRig rig) //TouchController ctrl)
		{
		if ((rig != null) && !rig.CanBeUsed())
			rig = null;

		if (this.rig != rig)
			{
		
			if (this.rig != null)
				this.rig.RemoveControl(this);
	
			this.rig = rig;
			if (this.rig != null)
				this.rig.AddControl(this);
			}

		this.disablingConditions.SetRig(this.rig);

		this.SyncDisablingConditions(true);
		}
		

	// ---------------------
	public void SetTouchControlPanel(TouchControlPanel panel)
		{
		if (this.panel == panel)
			return;
		
		if (this.panel != null)
			this.panel.RemoveControl(this);
	
		if ((panel != null) && !panel.CanBeUsed())
			panel = null;


		this.panel = panel;
		if (this.panel != null)
			this.panel.AddControl(this);
		}


/** \endcond */

	
	// -----------------------
	//! Invalidate Hierarchy after parent change.
	// ------------------------
	virtual public void InvalidateHierarchy()
		{
		InputRig			newRig			= null;
		TouchControlPanel	newPanel		= null;
		Canvas				newCanvas		= null;	
			
// TODO : check IsDestroyed()!!!

		for (Transform node = this.transform.parent; node != null; node = node.parent)
			{
			TouchControlPanel	panel	= node.GetComponent<TouchControlPanel>();
			InputRig			rig		= node.GetComponent<InputRig>();
			Canvas				canv	= node.GetComponent<Canvas>();


			if ((newPanel == null)	&& (panel != null))	newPanel = panel;
			if ((newRig == null) 	&& (rig != null))	newRig		= rig;			
			if ((newCanvas == null)	&& (canv != null))	newCanvas	= canv;
				

			if ((newRig != null) && (newPanel != null) && (newCanvas != null))
				break;
			}
			
		this.canvas = newCanvas;

		if (this.rig != newRig)
			this.SetRig(newRig);

		if (this.panel != newPanel)
			this.SetTouchControlPanel(newPanel);

		}

		
		
	// ------------------
	//! Get control's (pivot) position in world space.
	// -------------------
	public Vector3 GetWorldPos()
		{
		return this.transform.position;
		}

	// ------------------
	//! Set control's position in world space. 
	//! For dynamic controls, this will set the origin to given position and store it as the start position. 
	// -------------------
	virtual public void SetWorldPos(Vector2 pos2D)
		{
		this.SetWorldPosRaw(pos2D);
		}


	// ----------------------
	protected void SetWorldPosRaw(Vector2 pos2D)
		{
		Transform tr = this.transform;

		tr.position = new Vector3(pos2D.x, pos2D.y, tr.position.z);
		}


	// ------------------
	//! Get control's local rectangle.
	// -------------------
	virtual public Rect GetLocalRect()	
		{
		RectTransform rectTr = this.transform as RectTransform;
		if (rectTr == null) 
			return new Rect(0,0,1,1);

		Rect r = rectTr.rect;

		if ((this.shape == Shape.Circle) || (this.shape == Shape.Square))
			{
			Vector2 cen = r.center;
			float size = Mathf.Min (r.width, r.height);
			return new Rect(cen.x - (size * 0.5f), cen.y - (size * 0.5f), size, size); 
			}

		return r;
		}
		
	

//! \cond 

	// -------------------
	protected Vector3 ScreenToWorldPos(Vector2 sp, Camera cam)
		{
		Transform tr = this.transform;

		Vector3 sp3 = sp;
		if (cam != null)
			{	
			// If the control is aligned to the camera...

			if (Mathf.Abs(Vector3.Dot(tr.forward, cam.transform.forward)) >= 0.99999f)
				{
				sp3 = cam.ScreenToWorldPoint(sp3);
				sp3.z = tr.position.z;				
				}
			else
				{
				float raycastDepth = 0;
				Ray camRay = cam.ScreenPointToRay(sp);
				if (new Plane(tr.forward, tr.position).Raycast(camRay, out raycastDepth))
					{
					sp3 = camRay.origin + (camRay.direction * raycastDepth);
					}	
				else
					{
					sp3 = cam.ScreenToWorldPoint(sp3);
					sp3.z = tr.position.z;							
					}
				}
			}
		else
			{
			sp3.z = tr.position.z;
			}

		return sp3;
		}
		

	// ----------------		
	protected Vector2 WorldToLocalPos(Vector3 wp, Vector2 worldOffset)
		{
		return (Vector2)this.transform.worldToLocalMatrix.MultiplyPoint3x4(wp + (Vector3)worldOffset);
		}

	protected Vector2 WorldToLocalPos(Vector3 wp) { return WorldToLocalPos(wp, Vector2.zero); }
 

	// ------------------
	protected Vector2 ScreenToLocalPos(Vector2 sp, Camera cam, Vector2 worldOffset)
		{
		return this.WorldToLocalPos(this.ScreenToWorldPos(sp, cam), worldOffset);
		}

	protected Vector2 ScreenToLocalPos(Vector2 sp, Camera cam)  { return ScreenToLocalPos(sp, cam, Vector2.zero); }


	// ------------------
	protected Vector2 ScreenToNormalizedPos(Vector2 sp, Camera cam, Vector2 worldOffset)
		{
		return this.LocalToNormalizedPos(this.WorldToLocalPos(this.ScreenToWorldPos(sp, cam), worldOffset));
		}

	protected Vector2 ScreenToNormalizedPos(Vector2 sp, Camera cam)  { return ScreenToNormalizedPos(sp, cam, Vector2.zero); }
		


	// ------------------
	protected Vector2 LocalToNormalizedPos(Vector2 lp)
		{
		Rect rect = this.GetLocalRect();
			
		lp -= rect.center;

		lp.x /= (rect.width * 0.5f);		
		lp.y /= (rect.height * 0.5f);		

		return lp;
		}


	// -------------------
	protected Vector2 WorldToNormalizedPos(Vector2 wp, Vector2 worldOffset)
		{
		return LocalToNormalizedPos(WorldToLocalPos(wp, worldOffset));
		}

	protected Vector2 WorldToNormalizedPos(Vector2 wp) { return WorldToNormalizedPos(wp, Vector2.zero); }


		
		
	// ------------------
	protected Vector2 NormalizedToLocalPos(Vector2 np)
		{
		Rect r = this.GetLocalRect();

		np.x *= r.width * 0.5f;
		np.y *= r.height * 0.5f;
		
		return np + r.center; 
		}
		
	// ------------------
	protected Vector2 NormalizedToLocalOffset(Vector2 np)
		{
		Rect r = this.GetLocalRect();

		np.x *= r.width * 0.5f;
		np.y *= r.height * 0.5f;
		
		return np; //+ r.center; 
		}
		
	



	// ------------------
	protected Vector3 NormalizedToWorldPos(Vector2 np)
		{
		Vector2 lp = NormalizedToLocalPos(np);
		
		return this.transform.localToWorldMatrix.MultiplyPoint3x4(lp);
		}

	
	// -------------------
	protected Vector2 NormalizedToWorldOffset(Vector2 np)
		{
		Vector2 lp = NormalizedToLocalOffset(np);
		
		return this.transform.localToWorldMatrix.MultiplyVector(lp);
		}


	// ------------------
	protected Vector3 WorldToScreenPos(Vector3 wp, Camera cam)
		{
		return ((cam != null) ? cam.WorldToScreenPoint(wp) : wp);
		}

	// -------------------
	protected Vector2 LocalToScreenPos(Vector2 lp, Camera cam)
		{
		return ((cam != null) ? cam.WorldToScreenPoint(this.transform.localToWorldMatrix.MultiplyPoint3x4(lp)) : 
			this.transform.localToWorldMatrix.MultiplyPoint3x4(lp));
		}


		

	// --------------------
	public Vector2 ScreenToOrientedPos(Vector2 sp, Camera cam)
		{
		Quaternion rot = Quaternion.identity;

		rot = Quaternion.Inverse(this.transform.rotation);

		return (rot * (Vector3)sp);
		}

	

		


		
	// ---------------------------
	public Vector3 GetWorldSpaceCenter()
		{
		Rect r = this.GetLocalRect();
		Vector2 c = r.center;

		if (c == Vector2.zero)
			return this.transform.position;
		else
			return this.transform.localToWorldMatrix.MultiplyPoint3x4(c);		
		}

	// ------------------------
	public Vector3 GetWorldSpaceSize()
		{
		return (this.GetWorldSpaceAABB().size);
		}
		

	// --------------------------
	public Bounds GetWorldSpaceAABB()
		{
		Rect r = this.GetLocalRect();
		return CFUtils.TransformRectAsBounds(r, this.transform.localToWorldMatrix, ((this.shape == Shape.Circle) || (this.shape == Shape.Ellipse)));
		}

		
	// --------------------------
	public Vector2 GetScreenSpaceCenter(Camera cam)
		{
		Vector2 worldCen = this.GetWorldSpaceCenter();

		return ((cam == null) ? worldCen : (Vector2)cam.WorldToScreenPoint(worldCen));
		}




	// -------------------------
	public Matrix4x4 GetWorldToNormalizedMatrix()
		{
		Rect r = this.GetLocalRect();
		Matrix4x4 m = (Matrix4x4.Scale(new Vector3(2.0f / r.width, 2.0f / r.height, 1.0f)) * Matrix4x4.TRS(-r.center, Quaternion.identity, Vector3.one) * this.transform.worldToLocalMatrix);
		return m;
		}


	// --------------------------
	public Matrix4x4 GetNormalizedToWorldMatrix()
		{
		Rect r = this.GetLocalRect();
		Matrix4x4 m = this.transform.localToWorldMatrix * Matrix4x4.TRS(r.center, Quaternion.identity, new Vector3(r.width*0.5f, r.height*0.5f, 1));
		return m;
		}


		


	// --------------------
	protected Vector3 GetFollowPos(Vector3 targetWorldPos, Vector2 worldOffset, out bool posWasOutside)
		{
		Vector3 np = this.WorldToNormalizedPos(targetWorldPos, worldOffset);
	
		if ((this.shape == TouchControl.Shape.Circle) || (this.shape == TouchControl.Shape.Ellipse))
			{
			if (np.sqrMagnitude <= 1.0f)
				{
				posWasOutside = false;
				return targetWorldPos;
				}

			np = CFUtils.ClampInsideUnitCircle(np);

			}
		else
			{
			if ((np.x >= -1) && (np.x <= 1) && (np.y >= -1) && (np.y <= 1))
				{
				posWasOutside = false;
				return targetWorldPos;
				}

			np = CFUtils.ClampInsideUnitSquare(np);
			}
			

		Vector3 clampedWorldPos = this.NormalizedToWorldPos(np);
		Vector3 centerWorldPos = this.GetWorldSpaceCenter();
		
		posWasOutside = true;

		return (targetWorldPos - (clampedWorldPos - centerWorldPos));
		}

	protected Vector3 GetFollowPos(Vector3 targetWorldPos, Vector3 worldOffset)
		{ bool v; return GetFollowPos(targetWorldPos, worldOffset, out v); }
		



	// ----------------------
	protected Vector3 ClampInsideCanvas(Vector3 targetWorldPos, Canvas limiterCanvas)
		{
		RectTransform canvasTr = null;
		if ((limiterCanvas == null) || ((canvasTr = (limiterCanvas.transform as RectTransform)) == null)) 
			return targetWorldPos;
			
		

		Rect localRect		= this.GetLocalRect();
		Rect limiterRect	= canvasTr.rect;

		Matrix4x4 localToLimiterSpace = limiterCanvas.transform.worldToLocalMatrix * CFUtils.ChangeMatrixTranl(this.transform.localToWorldMatrix, targetWorldPos);

		bool thisIsRound	= ((this.shape == Shape.Circle) || (this.shape == Shape.Ellipse));
			
		Rect rectInLimiterSpace = CFUtils.TransformRect(localRect, localToLimiterSpace, thisIsRound);
			

		Vector2 localOfs = CFUtils.ClampRectInside(rectInLimiterSpace, thisIsRound, limiterRect, false);
		if (localOfs == Vector2.zero)
			return targetWorldPos;


		return (targetWorldPos + limiterCanvas.transform.localToWorldMatrix.MultiplyVector(localOfs));
		}

	// -------------------
	protected Vector3 ClampInsideOther(Vector3 targetWorldPos, TouchControl limiter)
		{
		Rect localRect		= this.GetLocalRect();
		Rect limiterRect	= limiter.GetLocalRect();

		Matrix4x4 localToLimiterSpace = limiter.transform.worldToLocalMatrix * CFUtils.ChangeMatrixTranl(this.transform.localToWorldMatrix, targetWorldPos);

		bool thisIsRound	= ((this.shape == Shape.Circle) || (this.shape == Shape.Ellipse));
		bool limiterIsRound	= ((limiter.shape == Shape.Circle) || (limiter.shape == Shape.Ellipse));
			
		Rect rectInLimiterSpace = CFUtils.TransformRect(localRect, localToLimiterSpace, thisIsRound);
			

		Vector2 localOfs = CFUtils.ClampRectInside(rectInLimiterSpace, thisIsRound, limiterRect, limiterIsRound);
		if (localOfs == Vector2.zero)
			return targetWorldPos;

		return (targetWorldPos + limiter.transform.localToWorldMatrix.MultiplyVector(localOfs));	
		}

		

	// ---------------------
	public bool RaycastScreen(Vector2 screenPos, Camera cam)
		{
		return this.RaycastLocal(this.ScreenToLocalPos(screenPos, cam));
		}

	// -------------------
	public bool RaycastLocal(Vector2 localPos)
		{
		Rect r = this.GetLocalRect();

		switch (this.shape)
			{
			case Shape.Circle :
				float rad = r.width * 0.5f;
				return ((localPos - r.center).sqrMagnitude <= (rad * rad));

			case Shape.Ellipse :
				Vector2 scaledDelta = localPos - r.center;
				scaledDelta.x /= r.width * 0.5f;
				scaledDelta.y /= r.height * 0.5f;
				return (scaledDelta.sqrMagnitude <= 1.0f);
				
			case Shape.Rectangle :
			case Shape.Square :
				return ((localPos.x >= r.x) && (localPos.x <= r.xMax) && (localPos.y >= r.y) && (localPos.y <= r.yMax));
			}

		return false;
		}
		
		

	// --------------------------
	public bool HitTest(Vector2 sp, Camera cam, float fingerRadPx, Hit hit)
		{
		hit.Reset();
		

		bool raycastOnly = (this.ignoreFingerRadius || (fingerRadPx < 0.001f));

		Vector2 lp = this.ScreenToLocalPos(sp, cam);

		Rect r = this.GetLocalRect();

		bool directHit = false;
		Vector2 closestLocalPoint = Vector2.zero; 

		Vector2 delta = (lp - r.center);

		switch (this.shape)
			{
			case Shape.Circle :
				float rad = r.width * 0.5f;
				if (!(directHit = (delta.sqrMagnitude <= (rad * rad))))
					{	
					if (!raycastOnly)
						closestLocalPoint = delta.normalized * rad;
					}
				break;
					

			case Shape.Ellipse :
				Vector2 scaledDelta = delta;
				scaledDelta.x /= r.width * 0.5f;
				scaledDelta.y /= r.height * 0.5f;
				if (!(directHit = (scaledDelta.sqrMagnitude <= 1.0f)) && !raycastOnly)
					{
					if (!raycastOnly)	
						{
						closestLocalPoint = delta.normalized;
						closestLocalPoint.x *= r.width * 0.5f;
						closestLocalPoint.y *= r.height * 0.5f;			
						}
					}
				break;

			case Shape.Rectangle :
			case Shape.Square :
				if (!(directHit = ((lp.x >= r.x) && (lp.x <= r.xMax) && (lp.y >= r.y) && (lp.y <= r.yMax))))
					{
					if (!raycastOnly)
						closestLocalPoint = CFUtils.ClampInsideRect(lp, r);
					}
				break;
			}


		// If indirect hit's aren't supported, return false...
			
		if (raycastOnly && !directHit)
			return false;


		// Check for indirect hit..
			
		Vector2 closestScreenPoint = this.LocalToScreenPos(closestLocalPoint, cam);

		bool indirectHit = (directHit ? false : ((sp - closestScreenPoint).sqrMagnitude <= (fingerRadPx * fingerRadPx)));


		if (directHit || indirectHit)
			{
			hit.c						= this;
			hit.indirectHit 		= indirectHit;
			hit.depth				= this.transform.position.z;
			hit.localPos			= lp;
			hit.closestLocalPos	= (directHit ? lp : closestLocalPoint);
			hit.screenDistSqPx 	= (sp - this.LocalToScreenPos(r.center, cam)).sqrMagnitude;	
			hit.screenPos 			= sp;
			hit.closestScreenPos	= closestScreenPoint;

			return true;
			}
			
		return false;
		}


	// --------------------
	/// Hit test result data.
	// --------------------

	// --------------------
	public class Hit 
		{
		public TouchControl 	c;	
		public float 			depth;
		public bool				indirectHit;
		public Vector2			localPos;
		public Vector2			closestLocalPos;
		public Vector2			screenPos,
									closestScreenPos;
		public float 			screenDistSqPx;
			
		// -----------------
		public void Reset()
			{
			this.c = null;
			}

		// -----------------
		public bool IsEmpty()	
			{
			return (this.c == null);
			}
			
		
		// -------------------
		public void CopyFrom(Hit b)
			{
			this.c  				= b.c;	
			this.depth  			= b.depth;	
			this.indirectHit		= b.indirectHit;	
			this.localPos			= b.localPos;	
			this.closestLocalPos	= b.closestLocalPos;	
			this.screenPos		 	= b.screenPos;	
			this.closestScreenPos	= b.closestScreenPos;	
			this.screenDistSqPx		= b.screenDistSqPx;	
			}


		// ------------------
		public bool IsHigherThan(Hit r)
			{
			if ((this.c == null) != (r.c == null))
				return (this.c != null);


			if ((Mathf.RoundToInt(this.depth) <= Mathf.RoundToInt(r.depth)) && 
				!(this.c is DynamicRegion) && (r.c is DynamicRegion))
				{
				return true;
				}

			if (this.indirectHit != r.indirectHit)
				return (!this.indirectHit);

			if (Mathf.RoundToInt(this.depth) != Mathf.RoundToInt(r.depth))
				{
				return (this.depth < r.depth);
				}

				

			if (this.screenDistSqPx != r.screenDistSqPx)
				return (this.screenDistSqPx < r.screenDistSqPx);

			return false;			
			}


	
		}	





	// ----------------------------
	// Hit Pool for Raycasting...
	// ----------------------------
	public class HitPool : ControlFreak2.ObjectPoolBase<TouchControl.Hit>
		{
		private Hit
			tempHit;

		public delegate bool 
			TouchControlFilterFunc(TouchControl c);

		// -------------
		public HitPool() : base()
			{
			this.tempHit = new Hit();
			}



		// ------------------
		public bool HitTestAny(List<TouchControl> controlList, Vector2 screenPos, Camera cam, float fingerRadPx = 0, TouchControlFilterFunc filter = null)
			{
			this.EnsureCapacity(1);
			this.Clear();

			for (int i = 0; i < controlList.Count; ++i)
				{
				TouchControl c = controlList[i];
				if ((c == null) || ((filter != null) && !filter(c)))
					continue;

				if (c.HitTest(screenPos, cam, fingerRadPx, this.tempHit))
					{
					this.GetNewObject().CopyFrom(this.tempHit);					
					return true;
					}
				}

			return false;
			}



		// -----------------
		public int HitTest(List<TouchControl> controlList, Vector2 screenPos, Camera cam, int maxHits = 8, float fingerRadPx = 0, TouchControlFilterFunc filter = null)
			{
			if (maxHits < 1)
				maxHits = 1;

			this.Clear();
			this.EnsureCapacity(maxHits);

			for (int ci = 0; ci < controlList.Count; ++ci)
				{
				TouchControl c = controlList[ci];
				if ((c == null) || ((filter != null) && !filter(c)))
					continue;

				if (!c.HitTest(screenPos, cam, fingerRadPx, this.tempHit))
					continue;

				int insertPos = -1;
				for (int ti = 0; ti < this.GetList().Count; ++ti)
					{
					if (this.tempHit.IsHigherThan(this.GetList()[ti]))
						{
						insertPos = ti;
						break;
						}
					}

				// Add new hit to the list...

				if (this.GetUsedCount() < maxHits)
					this.GetNewObject(insertPos).CopyFrom(this.tempHit);

				// If the list is fulll recycle the last hit...
	
				else if (insertPos >= 0)
					{
					Hit lastHit = this.GetList()[maxHits - 1];
					this.GetList().RemoveAt(maxHits - 1);
					this.GetList().Insert(insertPos, lastHit);
	
					lastHit.CopyFrom(this.tempHit);
					}
				}
			

			return this.GetUsedCount();
			}


		// -----------------
		protected override Hit CreateInternalObject()
			{ return new Hit(); }
			

		}

		

	// -------------------

	override protected void OnInitComponent()
		{
	
		this.StartAlphaAnim(1, 0);	

		this.InvalidateHierarchy();
		

		this.OnInitControl();

		this.SyncDisablingConditions(true);
		}
		


	// ---------------------
	override protected void OnEnableComponent()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			ControlFreak2Editor.CFEditorUtils.AddOnHierarchyChange(this.InvalidateHierarchy);
#endif
		this.ResetControl();

//TouchTrackPad tp = this as TouchTrackPad; if (tp != null) Debug.LogFormat("OnPostEnable : vec : {0}", tp.GetSwipeDelta());


		this.SyncDisablingConditions(true);

		}

	// --------------------
	override protected void OnDisableComponent()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			ControlFreak2Editor.CFEditorUtils.RemoveOnHierarchyChange(this.InvalidateHierarchy);
#endif

		this.SyncDisablingConditions(true);

		this.ReleaseAllTouches(); //true);
		this.ResetControl();
		}

	// -------------------
	override protected void OnDestroyComponent()
		{
		this.SetRig(null);
		this.SetTouchControlPanel(null);

		this.OnDestroyControl();

		}
		

	// ----------------------
	public void UpdateControl()
		{

		if (!this.CanBeUsed())
			return;
			

		this.UpdateBaseAlpha();

		this.OnUpdateControl();

		this.UpdateAnimators(false);
		}

	

		
	// ---------------------------
	protected void DrawDefaultGizmo(bool drawFullRect) 
		{ DrawDefaultGizmo(drawFullRect, 0.33f); }

	protected void DrawDefaultGizmo(bool drawFullRect, float fullRectColorShade /*= 0.33f*/)
		{ 
#if UNITY_EDITOR

		Matrix4x4	initialMatrix	= Gizmos.matrix;
		Color 		initialColor	= Gizmos.color;

		Gizmos.matrix = this.transform.localToWorldMatrix;

		Rect r = this.GetLocalRect();

		Gizmos.color = initialColor;
			
		switch (this.shape)
			{
			case Shape.Rectangle :
			case Shape.Square :
				Gizmos.DrawWireCube(r.center, r.size);	
				break;

			case Shape.Circle :
			case Shape.Ellipse :
				Gizmos.matrix = Gizmos.matrix * CFGizmos.GetCircleMatrix(r.center, new Vector2(r.width, ((this.shape == Shape.Ellipse) ? r.height : r.width))); //.matrix * Matrix4x4.TRS(r.center, Quaternion.identity, new Vector3(r.width, ((this.shape == Shape.ELLIPSE) ? r.height : r.width), 0.00001f));
				CFGizmos.DrawOutlinedCircle();
				break;				
			}
	
		Gizmos.matrix = initialMatrix;		
#endif
		}
		

	// ---------------------
	virtual protected void DrawCustomGizmos(bool selected)
		{
		Color		initialColor	= Gizmos.color;
			
		Gizmos.color = (selected ? Color.red : Color.white);

		this.DrawDefaultGizmo(true);

		Gizmos.color = initialColor;
		}


	// --------------------
	void OnDrawGizmos()
		{

		if (!this.IsInitialized)
			return;
			
		this.DrawCustomGizmos(false);

		}

	// --------------------
	void OnDrawGizmosSelected()
		{
		if (!this.IsInitialized)
			return;

		this.DrawCustomGizmos(true);
		}
		

//! \endcond 

	}
}