// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;
using System.Collections.Generic;

using ControlFreak2;

namespace ControlFreak2.Internal
{
// ----------------------
/// Joystick state binding.
// ----------------------

[System.Serializable]
public class JoystickStateBinding : InputBindingBase
	{
	public AxisBinding 
		horzAxisBinding,
		vertAxisBinding;
		
	
	public DirectionBinding
		dirBinding;		

	// ------------------
	public JoystickStateBinding(InputBindingBase parent = null) : base(parent)
		{
		this.enabled = false;	

		this.horzAxisBinding = new AxisBinding(this);
		this.vertAxisBinding = new AxisBinding(this);

		this.dirBinding = new DirectionBinding(this);
		}
		


	// ---------------------
	public void CopyFrom(JoystickStateBinding b)
		{
		if (this.enabled = b.enabled)
			{
			this.Enable();
			this.dirBinding.CopyFrom(b.dirBinding);
			this.horzAxisBinding.CopyFrom(b.horzAxisBinding);
			this.vertAxisBinding.CopyFrom(b.vertAxisBinding);
			}
		}


	// -------------------
	public void SyncJoyState(JoystickState state, InputRig rig)
		{
		if ((state == null) || !this.enabled || (rig == null))
			return;
	
		Vector2 joyVec = state.GetVector();
			
		this.horzAxisBinding.SyncFloat(joyVec.x, InputRig.InputSource.Analog, rig);
		this.vertAxisBinding.SyncFloat(joyVec.y, InputRig.InputSource.Analog, rig);
	
		this.dirBinding.SyncDirectionState(state.GetDirState(), rig);
		}

	// --------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{
		if (!this.enabled)
			return false;

		return (
			this.horzAxisBinding.IsBoundToAxis(axisName, rig) ||
			this.vertAxisBinding.IsBoundToAxis(axisName, rig) ||	
			this.dirBinding.IsBoundToAxis(axisName, rig));
		}

	// -----------------
	override protected bool OnIsBoundToKey(KeyCode keyCode, InputRig rig)
		{
		if (!this.enabled)
			return false;

		return (
			this.horzAxisBinding.IsBoundToKey(keyCode, rig) ||
			this.vertAxisBinding.IsBoundToKey(keyCode, rig) ||
			this.dirBinding.IsBoundToKey(keyCode, rig));
		}


	// ---------------
	override protected void OnGetSubBindingDescriptions(
		BindingDescriptionList descList,
		Object	undoObject, 
		string	parentMenuPath)
		{
		descList.Add(this.horzAxisBinding, InputRig.InputSource.Analog, "Horizontal", parentMenuPath, undoObject); 
		descList.Add(this.vertAxisBinding, InputRig.InputSource.Analog, "Vertical", parentMenuPath, undoObject);
		
		descList.Add(this.dirBinding, "Direction", parentMenuPath, undoObject); 
		}



	}



}

//! \endcond
