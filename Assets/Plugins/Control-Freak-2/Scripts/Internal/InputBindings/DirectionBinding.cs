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
/// Direction binding.
// ----------------------

[System.Serializable]
public class DirectionBinding : InputBindingBase
	{
	public enum BindMode
		{
		Normal,					///< Constnant update.
		OnChange,				///< Binds new direction when changed (for 1 frame).
		OnRelease,				///< Binds the last direction on release (for 1 frame).
		OriginalOnStart,		///< Binds the original direction (first non-neutral direction after reset) when changed (for 1 frame).
		OriginalUntilRelease,///< Binds the original direction (no matter what's the current direction) until release. 
		OriginalUntilChange	///< Binds the original direction until swipe direction changes.	
		}


	//public bool enabled;

	public bool 
		bindDiagonals;

	public BindMode
		bindMode;

	public DigitalBinding
		dirBindingU,
		dirBindingUR,
		dirBindingR,
		dirBindingDR,
		dirBindingD,
		dirBindingDL,
		dirBindingL,
		dirBindingUL,
		dirBindingN,
		dirBindingAny;

	// ------------------ 
	public DirectionBinding(InputBindingBase parent = null) : base(parent)
 		{
		//this.enabled = false;	
		this.bindDiagonals = true;

		this.dirBindingN	= new DigitalBinding(this);
		this.dirBindingAny	= new DigitalBinding(this);
		this.dirBindingU	= new DigitalBinding(this);
		this.dirBindingUR	= new DigitalBinding(this);
		this.dirBindingR	= new DigitalBinding(this);
		this.dirBindingDR	= new DigitalBinding(this);
		this.dirBindingD	= new DigitalBinding(this);
		this.dirBindingDL	= new DigitalBinding(this);
		this.dirBindingL	= new DigitalBinding(this);
		this.dirBindingUL	= new DigitalBinding(this);
		}
		

	// --------------------
	public void CopyFrom(DirectionBinding b)
		{
		if (this.enabled = b.enabled)
			{
			this.Enable();

			this.bindDiagonals	= b.bindDiagonals;
			this.bindMode		= b.bindMode; 

			this.dirBindingN	.CopyFrom(b.dirBindingN);
			this.dirBindingAny	.CopyFrom(b.dirBindingAny);
			this.dirBindingU	.CopyFrom(b.dirBindingU);	
			this.dirBindingUR	.CopyFrom(b.dirBindingUR);	
			this.dirBindingR	.CopyFrom(b.dirBindingR);	
			this.dirBindingDR	.CopyFrom(b.dirBindingDR);	
			this.dirBindingD	.CopyFrom(b.dirBindingD);	
			this.dirBindingDL	.CopyFrom(b.dirBindingDL);	
			this.dirBindingD	.CopyFrom(b.dirBindingL);	
			this.dirBindingUL	.CopyFrom(b.dirBindingUL);	
			}
		}


	// --------------------
	override protected void OnGetSubBindingDescriptions(BindingDescriptionList descList, //BindingDescription.BindingType typeMask, 
		Object undoObject, string parentMenuPath) //, bool addUnusedBindings, int axisSourceTypeMask)
		{
		descList.Add(this.dirBindingN, "Neutral", parentMenuPath, undoObject);
		descList.Add(this.dirBindingAny, "Any Non-Neutral", parentMenuPath, undoObject);

			descList.Add(this.dirBindingU, "Up", parentMenuPath, undoObject);
		if (descList.addUnusedBindings || this.bindDiagonals)
			descList.Add(this.dirBindingUR, "Up-Right", parentMenuPath, undoObject);

			descList.Add(this.dirBindingR, "Right", parentMenuPath, undoObject);
		if (descList.addUnusedBindings || this.bindDiagonals)
			descList.Add(this.dirBindingDR, "Down-Right", parentMenuPath, undoObject);

			descList.Add(this.dirBindingD, "Down", parentMenuPath, undoObject);
		if (descList.addUnusedBindings || this.bindDiagonals)
			descList.Add(this.dirBindingDL, "Down-Left", parentMenuPath, undoObject);

			descList.Add(this.dirBindingL, "Left", parentMenuPath, undoObject);
		if (descList.addUnusedBindings || this.bindDiagonals)
			descList.Add(this.dirBindingUL, "Up-Left", parentMenuPath, undoObject);
		}



	// -------------------
	public void SyncDirectionState(DirectionState dirState,	InputRig rig)
		{
		switch (this.bindMode)
			{
			case BindMode.Normal :
				this.SyncDirRaw(dirState.GetCur(), rig);
				break;

			case BindMode.OnChange :
				if (dirState.GetCur() != dirState.GetPrev())
					this.SyncDirRaw(dirState.GetCur(), rig);
				break;

			case BindMode.OnRelease :
				if ((dirState.GetCur() == Dir.N) && (dirState.GetPrev() != Dir.N))
					this.SyncDirRaw(dirState.GetPrev(), rig);
				break;

			case BindMode.OriginalOnStart :
				if ((dirState.GetOriginal() != Dir.N) && (dirState.GetPrevOriginal() != dirState.GetOriginal()))
					this.SyncDirRaw(dirState.GetOriginal(), rig);
				break;

			case BindMode.OriginalUntilChange :
				if ((dirState.GetOriginal() != Dir.N) && (dirState.GetOriginal() == dirState.GetCur()))
					this.SyncDirRaw(dirState.GetOriginal(), rig);
				break;

			case BindMode.OriginalUntilRelease :
				if ((dirState.GetOriginal() != Dir.N)) // && (dirCur != Dir.N))
					this.SyncDirRaw(dirState.GetOriginal(), rig);
				break;
			}
		}


	// -------------------
	public void SyncDirRaw(ControlFreak2.Dir dir, InputRig rig)
		{
		if ((rig == null) || !this.enabled)
			return;
			
		if (dir != Dir.N)
			this.dirBindingAny.Sync(true, rig);

		if (this.bindDiagonals)
			{
			switch (dir)
				{
				case ControlFreak2.Dir.N	: this.dirBindingN	.Sync(true, rig); break; 
				case ControlFreak2.Dir.U	: this.dirBindingU	.Sync(true, rig); break; 
				case ControlFreak2.Dir.UR	: this.dirBindingUR	.Sync(true, rig); break; 
				case ControlFreak2.Dir.R	: this.dirBindingR	.Sync(true, rig); break; 
				case ControlFreak2.Dir.DR	: this.dirBindingDR	.Sync(true, rig); break; 
				case ControlFreak2.Dir.D	: this.dirBindingD	.Sync(true, rig); break; 
				case ControlFreak2.Dir.DL	: this.dirBindingDL	.Sync(true, rig); break; 
				case ControlFreak2.Dir.L	: this.dirBindingL	.Sync(true, rig); break; 
				case ControlFreak2.Dir.UL	: this.dirBindingUL	.Sync(true, rig); break; 
				}
			}
		else
			{
			if ((dir == Dir.U) || (dir == Dir.UL) || (dir == Dir.UR))
				this.dirBindingU.Sync(true, rig);
			if ((dir == Dir.R) || (dir == Dir.UR) || (dir == Dir.DR))
				this.dirBindingR.Sync(true, rig);
			if ((dir == Dir.D) || (dir == Dir.DL) || (dir == Dir.DR))
				this.dirBindingD.Sync(true, rig);
			if ((dir == Dir.L) || (dir == Dir.UL) || (dir == Dir.DL))
				this.dirBindingL.Sync(true, rig);
			}
		}

	// --------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{
		if (!this.enabled)
			return false;

		return (
			this.dirBindingN	.IsBoundToAxis(axisName, rig) ||
			this.dirBindingAny	.IsBoundToAxis(axisName, rig) ||
			this.dirBindingU	.IsBoundToAxis(axisName, rig) ||
			this.dirBindingR	.IsBoundToAxis(axisName, rig) ||
			this.dirBindingD	.IsBoundToAxis(axisName, rig) ||
			this.dirBindingL	.IsBoundToAxis(axisName, rig) ||
			(this.bindDiagonals && (
				this.dirBindingUR	.IsBoundToAxis(axisName, rig) ||
				this.dirBindingDR	.IsBoundToAxis(axisName, rig) ||
				this.dirBindingDL	.IsBoundToAxis(axisName, rig) ||
				this.dirBindingUL	.IsBoundToAxis(axisName, rig)) ));
		}

	// -----------------
	override protected bool OnIsBoundToKey(KeyCode keyCode, InputRig rig)
		{
		if (!this.enabled)
			return false;

		return (
			this.dirBindingN	.IsBoundToKey(keyCode, rig) ||
			this.dirBindingAny	.IsBoundToKey(keyCode, rig) ||
			this.dirBindingU	.IsBoundToKey(keyCode, rig) ||
			this.dirBindingR	.IsBoundToKey(keyCode, rig) ||
			this.dirBindingD	.IsBoundToKey(keyCode, rig) ||
			this.dirBindingL	.IsBoundToKey(keyCode, rig) ||
			(this.bindDiagonals && (
				this.dirBindingUR	.IsBoundToKey(keyCode, rig) ||
				this.dirBindingDR	.IsBoundToKey(keyCode, rig) ||
				this.dirBindingDL	.IsBoundToKey(keyCode, rig) ||
				this.dirBindingUL	.IsBoundToKey(keyCode, rig)) ));
		}


	}

}

//! \endcond
