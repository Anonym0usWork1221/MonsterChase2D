// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

using ControlFreak2;

namespace ControlFreak2.Internal
{
// ------------------------
/// Mouse Position binding.
// -------------------------

[System.Serializable]
public class EmuTouchBinding : InputBindingBase
	{
	private int emuTouchId;

	// ----------------------
	public EmuTouchBinding(InputBindingBase parent = null) : base(parent)
		{
		this.enabled		= false;
		this.emuTouchId 	= -1;
		}

		
	// ----------------
	public void CopyFrom(EmuTouchBinding b)
		{
		if ((b == null)) 
			return;
		
		if ((this.enabled = b.enabled))
			{
			this.Enable();
			}
		}		



	// ----------------------
	public void SyncState(TouchGestureBasicState touchState, InputRig rig)
		{
		if (!this.enabled || (rig == null))
			return;
	
		rig.SyncEmuTouch(touchState, ref this.emuTouchId);
		}
		
	// ------------------
	override protected bool OnIsEmulatingTouches() 
		{ return this.enabled; }

	
	}



}

//! \endcond
