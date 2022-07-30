// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2.Internal
{

[System.Serializable]
public abstract class InputBindingBase : IBindingContainer
	{
	public bool 
		enabled;

	[System.NonSerialized] 
	InputBindingBase 
		parent;
	

	// ------------------
	public InputBindingBase(InputBindingBase parent)
		{
		this.enabled	= false;
		this.parent		= parent;
		}


	// ------------------
	public InputBindingBase GetParent()
		{
		return this.parent;
		}
	
	// ------------------
	public void Enable()
		{
		for (InputBindingBase b = this; b != null; b = b.parent)
			b.enabled = true;	
		}

	// ----------------
	public bool IsEnabledInHierarchy()
		{
		InputBindingBase binding = this;
		do {
			if (!binding.enabled)
				return false;
			} while ((binding = binding.parent) != null);
		
		return true;
		}


	// -------------------
	public void GetSubBindingDescriptions(
		BindingDescriptionList			descList, 
		Object 							undoObject, 
		string 							parentMenuPath) 
		{
		this.OnGetSubBindingDescriptions(descList, undoObject, parentMenuPath);
		}
		
	// -------------------------
	virtual protected void OnGetSubBindingDescriptions(
		BindingDescriptionList			descList, 
		Object 							undoObject, 
		string 							parentMenuPath) 
		{
		}


	public bool IsBoundToKey			(KeyCode key, InputRig rig)			{ return this.OnIsBoundToKey(key, rig); }
	public bool IsBoundToAxis			(string axisName, InputRig rig)		{ return this.OnIsBoundToAxis(axisName, rig); }
	public bool IsEmulatingTouches		()						{ return this.OnIsEmulatingTouches(); }
	public bool IsEmulatingMousePosition()						{ return this.OnIsEmulatingMousePosition(); }
		
	virtual protected bool OnIsBoundToKey				(KeyCode key, InputRig rig)			{ return false; }
	virtual protected bool OnIsBoundToAxis				(string axisName, InputRig rig)		{ return false; }
	virtual protected bool OnIsEmulatingTouches			()						{ return false; }
	virtual protected bool OnIsEmulatingMousePosition	()						{ return false; }

	}
}

//! \endcond
