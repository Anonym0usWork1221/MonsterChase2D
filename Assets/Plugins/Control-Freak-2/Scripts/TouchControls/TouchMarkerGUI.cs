// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------


//! \cond 

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using ControlFreak2Editor;
#endif

namespace ControlFreak2
{

public class TouchMarkerGUI : MonoBehaviour 
	{
	public Texture2D
		fingerMarker,
		pinchHintMarker,
		twistHintMarker;


	public static TouchMarkerGUI
		mInst;


	// -------------------
	public TouchMarkerGUI()
		{
		}

	// --------------------
	void OnEnable()
		{
		mInst = this;

#if UNITY_EDITOR
		if (this.fingerMarker == null)
			this.fingerMarker = CFEditorStyles.Inst.texFinger;

		if (this.pinchHintMarker == null)
			this.pinchHintMarker = CFEditorStyles.Inst.texPinchHint;

		if (this.twistHintMarker == null)
			this.twistHintMarker = CFEditorStyles.Inst.texTwistHint;
#endif

		}

	// ----------------
	void OnDisable()
		{
		if (mInst == this)
			mInst = null;
		}

	// ------------------
	void OnGUI()
		{
		if (CF2Input.activeRig == null)
			return;

		List<TouchControl> cList = CF2Input.activeRig.GetTouchControls();
		for (int i = 0; i < cList.Count; ++i)
			{
			SuperTouchZone c = cList[i] as SuperTouchZone;
			if (c != null)
				c.DrawMarkerGUI();
			}
		}


	
	}
}

//! \endcond 

