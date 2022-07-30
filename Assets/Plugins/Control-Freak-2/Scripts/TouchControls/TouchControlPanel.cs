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
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace ControlFreak2
{

// ----------------------
//! Touch Control Panel class.
// --------------------
[ExecuteInEditMode()]

public class TouchControlPanel : ControlFreak2.Internal.ComponentBase
	{
//! \cond
	const float 
		staticFingerTimeout = 2; //10;

		
	public bool
		autoConnectToRig = true;
	public InputRig
		rig;
		
	[System.NonSerializedAttribute]
	protected List<TouchControl>
		controls;

	[System.NonSerializedAttribute]
	private List<SystemTouch> 
		touchList;

	
	[System.NonSerializedAttribute]
	private TouchControl.HitPool 
		hitPool;

			
	const int 
		MAX_SYSTEM_TOUCHES 	= 16,
		MAX_RAYCAST_HITS	= 8;
		
#if UNITY_EDITOR
	static public int 
		mSkipTouchScreenPixels = 0;
#endif		


	// -----------------
	public TouchControlPanel() : base()
		{
		this.controls = new List<TouchControl>(16);

		this.hitPool = new TouchControl.HitPool();
		this.hitPool.EnsureCapacity(MAX_RAYCAST_HITS);

		// Init system touch list...

		this.touchList = new List<SystemTouch>(MAX_SYSTEM_TOUCHES);	
			
		for (int i = 0; i < MAX_SYSTEM_TOUCHES; ++i)
			{
			SystemTouch t = new SystemTouch(this);
			
			this.touchList.Add(t);
			}

		}




	// --------------------
	override protected void OnInitComponent()
		{
		
		this.InvalidateHierarchy();
		}

	// --------------------
	override protected void OnDestroyComponent()
		{
		this.ReleaseAll(true);


		if (this.controls != null)
			{
			foreach (TouchControl c in this.controls)
				{
				if (c != null) 
					c.SetTouchControlPanel(null);
				}
			}
		}


	// ---------------------
	override protected void OnEnableComponent()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			ControlFreak2Editor.CFEditorUtils.AddOnHierarchyChange(this.InvalidateHierarchy);
#endif
		}
	
	// ----------------------
	override protected void OnDisableComponent()
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			ControlFreak2Editor.CFEditorUtils.RemoveOnHierarchyChange(this.InvalidateHierarchy);
#endif

		this.ReleaseAll(true);
		}
		


#if UNITY_EDITOR
	[UnityEditor.MenuItem("Control Freak 2/Tools/Emulate Low-Res TouchScreen/Off", true, 100)]
	static private bool IsTouchScreenEmuOff() { return !(mSkipTouchScreenPixels <= 0); }
	[UnityEditor.MenuItem("Control Freak 2/Tools/Emulate Low-Res TouchScreen/Off", false, 100)]
	static private void TurnOffTouchScreenDegradation() { mSkipTouchScreenPixels = 0; }

	[UnityEditor.MenuItem("Control Freak 2/Tools/Emulate Low-Res TouchScreen/On (4 pixels)", true, 100)]
	static private bool IsTouchScreenEmuOn() { return !(mSkipTouchScreenPixels == 3); }
	[UnityEditor.MenuItem("Control Freak 2/Tools/Emulate Low-Res TouchScreen/On (4 pixels)", false, 100)]
	static private void TurnOnTouchScreenDegradation() { mSkipTouchScreenPixels = 3; }

	[UnityEditor.MenuItem("Control Freak 2/Tools/Emulate Low-Res TouchScreen/On (8 pixels)", true, 100)]
	static private bool IsTouchScreenEmuOn8() { return !(mSkipTouchScreenPixels == 7); }
	[UnityEditor.MenuItem("Control Freak 2/Tools/Emulate Low-Res TouchScreen/On (8 pixels)", false, 100)]
	static private void TurnOnTouchScreenDegradation8() { mSkipTouchScreenPixels = 7; }

#endif

		
	// ----------------------
	void Update()
		{

		this.UpdatePanel();

		}

	// -------------------
	public void UpdatePanel()
		{
		this.UpdateTouches();
		}




		
	// --------------------
	public void InvalidateHierarchy()
		{
		if (this.autoConnectToRig || (this.rig == null))
			{
			this.rig = this.GetComponent<InputRig>();

			if (this.rig == null)
				this.rig = this.GetComponentInParent<InputRig>();
			}

		
		}


//! \cond 


	// ---------------------
	public void AddControl(TouchControl c)
		{
		if (!this.CanBeUsed())
			return;

		if (this.controls.Contains(c))
			{
#if UNITY_EDITOR
			Debug.LogError("Ctrl(" + this.name + ") already contains " + ((c != null) ? c.name : "NULL"));
#endif
			return;  
			}

		this.controls.Add(c);
		}

	// --------------------
	public void RemoveControl(TouchControl c)
		{
		if (!this.CanBeUsed())
			return;

		if (this.controls != null)
			this.controls.Remove(c);
		}	


		
	// ----------------------
	public List<TouchControl> GetControlList()	
		{
		return this.controls;
		}


	// ---------------------
	
	private float fingerRadInPx 	= 0;	
		


	// -------------------
	private void Prepare()
		{
		this.fingerRadInPx = (((this.rig != null) ? this.rig.fingerRadiusInCm : 0.1f) *  CFScreen.dpcm); //this.storedDPCm;
		}



	// ---------------------
	public bool Raycast(Vector2 sp, Camera eventCamera)
		{
		if (this.rig != null)
			{
			if (this.rig.AreTouchControlsHiddenManually())
				return false;

			if (this.rig.AreTouchControlsSleeping())
				return true;

			if (this.rig.swipeOverFromNothing)
				return true;
			}


		this.Prepare();

		bool anyHit = this.hitPool.HitTestAny(this.controls, sp, eventCamera, this.fingerRadInPx, this.RaycastControlFilter);		 

		return anyHit;
		}


	// ---------------------
	private bool RaycastControlFilter(TouchControl c)
		{
		return ((c != null) && c.CanBeTouchedDirectly(null)); 
		}

		
	// -------------------
	public void OnSystemTouchStart(SystemTouchEventData data) 
		{
		if (!this.IsInitialized)	
			return;

		if (this.rig != null)
			this.rig.WakeTouchControlsUp();

		SystemTouch t = this.StartNewTouch(data);
		if (t == null)
			return;


		this.Prepare();
	
		if (this.hitPool.HitTest(this.controls, data.pos, data.cam, MAX_RAYCAST_HITS, this.fingerRadInPx, 
				t.touch.DirectTouchControlFilter) > 0)
			{


			for (int i = 0; i < this.hitPool.GetList().Count; ++i)
				{
				TouchControl.Hit hit = this.hitPool.GetList()[i];
				if ((hit == null) || (hit.c == null)) 
					continue;
					
					
				// If this touch is already shared by other controls...

				if (hit.c.dontAcceptSharedTouches && (t.touch.GetControlCount() > 0))
					{
					continue;
					}

				if (hit.c.OnTouchStart(t.touch, null, TouchControl.TouchStartType.DirectPress))
					{	
					// Stop sharing this touch if a control doesn't allow it...

					if (!hit.c.shareTouch)
						break;
					}
				else
					{
					}
			

				}

			}
		else
			{	
			}

		}		

	// ----------------------
	public void OnSystemTouchEnd(SystemTouchEventData data)
		{
		if (!this.IsInitialized)	
			return;

		if (this.rig != null)
			this.rig.WakeTouchControlsUp();
			
	
		SystemTouch t = this.FindTouch(data.id);
		if (t == null)
			return;
			
		t.touch.End(false);			

		}
		

	// ----------------------
	public void OnSystemTouchMove(SystemTouchEventData data)
		{
		if (!this.IsInitialized)	
			return;

		if (this.rig != null)
			this.rig.WakeTouchControlsUp();
			
		SystemTouch t = this.FindTouch(data.id);
		if (t == null)
			return;

				
		t.WakeUp();

		Vector2 pos = data.pos;

#if UNITY_EDITOR
		mSkipTouchScreenPixels = Mathf.Clamp(mSkipTouchScreenPixels, 0, 10);
		if (mSkipTouchScreenPixels > 0)	
			{
			pos.x = (float)((mSkipTouchScreenPixels + 1) * (Mathf.RoundToInt(pos.x) / (mSkipTouchScreenPixels + 1)));
			pos.y = (float)((mSkipTouchScreenPixels + 1) * (Mathf.RoundToInt(pos.y) / (mSkipTouchScreenPixels + 1)));
			}
#endif

		t.touch.Move(pos, data.cam);



		// Handle swipe over...

		List<TouchControl> 
			restrictedSwipeOverTargetList = t.touch.GetRestrictedSwipeOverTargetList();
		List<TouchControl> 
			swipeOverTargetList = ((restrictedSwipeOverTargetList != null) ? restrictedSwipeOverTargetList : this.controls);

		

		if ((swipeOverTargetList.Count > 0) && 
			(this.hitPool.HitTest(swipeOverTargetList, t.touch.screenPosCur, t.touch.cam, MAX_RAYCAST_HITS, 0) > 0)) 
			{
			for (int ci = 0; ci < this.hitPool.GetList().Count; ++ci)
				{
				TouchControl c = this.hitPool.GetList()[ci].c;
				if (!c.IsActive())
					continue;

				if ((restrictedSwipeOverTargetList == null) ? c.CanBeSwipedOverFromNothing(t.touch) : c.CanBeSwipedOverFromRestrictedList(t.touch))
					{

					if (c.OnTouchStart(t.touch, null, TouchControl.TouchStartType.SwipeOver))
						{
						if (!c.shareTouch)
							break;
						}
					}
				}
			}

		}
		

		

	// ----------------------
	// Class used to send touch parameters from low-level systems.
	// ----------------------
	public class SystemTouchEventData 
		{
		public Vector2	pos;	
		public Camera	cam;	
		public int 		id;	
		public bool 	isMouseEvent;	
		public int		touchId;
		}
		


		
	// -----------------------
	// System touch list.
	// -----------------------
		

		// -----------------
		public void UpdateTouches()
			{
			for (int i = 0; i < this.touchList.Count; ++i)
				{				
				this.touchList[i].Update();
				}

			// Read touch pressure...

			this.UpdateTouchPressure();

			}
			



		// -----------------
		private bool IsTouchPressureSensitive(int touchId, out float pressureOut)	
			{
			pressureOut = 1;

#if !UNITY_PRE_5_3
			if (UnityEngine.Input.touchPressureSupported)
				{
				for (int i = 0; i < UnityEngine.Input.touchCount; ++i)
					{
					UnityEngine.Touch t = UnityEngine.Input.GetTouch(i);
					if ((t.phase == TouchPhase.Canceled) || (t.phase == TouchPhase.Ended))
						continue;

					if (t.fingerId == touchId)	
						{
						pressureOut = (t.pressure / t.maximumPossiblePressure);
						return true;
						}
					}
				}
#endif
			return false;
			}

		// ---------------
		private void UpdateTouchPressure()
			{
#if !UNITY_PRE_5_3

			if (UnityEngine.Input.touchPressureSupported)
				{
				for (int i = 0; i < UnityEngine.Input.touchCount; ++i)
					{
					UnityEngine.Touch t = UnityEngine.Input.GetTouch(i);
					if ((t.phase == TouchPhase.Canceled) || (t.phase == TouchPhase.Ended))
						continue;

					SystemTouch st = this.FindTouch(t.fingerId);
					if (st == null)
						continue;

					st.touch.SetPressure(t.pressure, t.maximumPossiblePressure);
					}
				}
#endif
			}



		// --------------
		private void ReleaseAll(bool cancel)
			{
			for (int i = 0; i < this.touchList.Count; ++i)
				{
				this.touchList[i].touch.End(cancel);
				}
			}
			
		// --------------
		private SystemTouch FindTouch(int hwId)
			{
			for (int i = 0; i < this.touchList.Count; ++i)
				{	
				SystemTouch t = this.touchList[i];
				if (t.touch.IsOn() && (t.hwId == hwId))
					return t;
				}
		
			return null;
			}
			


		// --------------
		private SystemTouch StartNewTouch(SystemTouchEventData data)
			{
			SystemTouch newTouch = null;

			for (int i = 0; i < this.touchList.Count; ++i)
				{	
				SystemTouch t = this.touchList[i];
				if (!t.touch.IsOn())
					{			
					newTouch = t;
					}
				else if (t.hwId == data.id)
					{			
#if UNITY_EDITOR
			//		Debug.LogWarning("[" + Time.frameCount + "] System tries to start a duplicate touch (" + data.id + ")! Ending the old one (started at frame: " + t.startFrame + ").");
#endif
					t.touch.End(true);
					}
				}


			if (newTouch != null)
				{
				newTouch.elapsedSinceLastAction	= 0;
				newTouch.hwId					= data.id;	
				newTouch.startFrame				= Time.frameCount;

				float pressure = 1;
				bool isPressureSensitive = (!data.isMouseEvent && this.IsTouchPressureSensitive(newTouch.hwId, out pressure));

				newTouch.touch.Start(data.pos, data.pos, data.cam, data.isMouseEvent, isPressureSensitive, pressure);
			
				return newTouch;
				}
		
			return null;
			}


		// ----------------------
		public int GetActiveTouchCount()
			{
			int count = 0;

			for (int i = 0; i < this.touchList.Count; ++i)
				{
				if (this.touchList[i].touch.IsOn())
					++count;
				}
			return count;				
			}


		
		
	// -------------------------
	// System touch state.
	// -------------------------
	private class SystemTouch 
		{
		public TouchObject
			touch;
		//private TouchControlPanel 
		//	panel; 
		public int		
			hwId;
		public float			
			elapsedSinceLastAction;
		public int
			startFrame;
			


		// -------------
		public SystemTouch(TouchControlPanel panel)
			{
			//this.panel		= panel;
			this.touch		= new TouchObject();
			this.hwId 		= 0;
			this.elapsedSinceLastAction = 0;
			}

		
		// ----------------
		public void WakeUp()
			{
			this.elapsedSinceLastAction = 0;
			}
 



		// ---------------
		public void Update()
			{
			if (!this.touch.IsOn())
				return;

			if ((this.elapsedSinceLastAction += Time.unscaledDeltaTime) > TouchControlPanel.staticFingerTimeout) //this.panel.staticFingerTimeout)
				{
				if (!UnityEngine.Input.GetMouseButton(0) && 
					!UnityEngine.Input.GetMouseButton(1) &&
					!UnityEngine.Input.GetMouseButton(2) && 
					(UnityEngine.Input.touchCount == 0))
					{ 
#if UNITY_EDITOR
			//		Debug.Log("Ending touch [" + this.hwId + "] due to inactivity! (" + this.panel.name + ") Started at frame: " + this.startFrame);
#endif
					this.touch.End(true);
					}
				}

			}
			
		}


//! \endcond

	}
}
 