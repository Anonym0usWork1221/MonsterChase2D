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
public class MousePositionBinding : InputBindingBase
	{
	public int 		priority;

	// ----------------------
	public MousePositionBinding(InputBindingBase parent = null) : base(parent)
		{
		this.enabled		= false;
		this.priority 		= 0;
		}

	// ----------------------
	public MousePositionBinding(int prio, bool enabled, InputBindingBase parent = null) : base(parent)
		{
		this.enabled		= enabled;
		this.priority 		= prio;
		}


	// ----------------
	public void CopyFrom(MousePositionBinding b)
		{
		if ((b == null)) 
			return;
		
		if ((this.enabled = b.enabled))
			{
			this.Enable();
			this.priority = b.priority;
			}
		}		

		

	// ----------------------
	public void SyncPos(Vector2 pos, InputRig rig)
		{
		if (!this.enabled || (rig == null))
			return;
	
		rig.mouseConfig.SetPosition(pos, this.priority);
		}


	// ---------------------
	override protected bool OnIsEmulatingMousePosition()
		{ return this.enabled; }
	
	}



}

//! \endcond
