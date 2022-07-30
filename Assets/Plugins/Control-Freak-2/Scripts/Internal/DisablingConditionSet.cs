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

[System.Serializable]
public class DisablingConditionSet
	{
	public enum MobileModeRelation
		{
		EnabledOnlyInMobileMode,
		DisabledInMobileMode,
		AlwaysEnabled
		}

	public MobileModeRelation
		mobileModeRelation;

	public bool 
		disableWhenTouchScreenInactive,
		disableWhenCursorIsUnlocked;
		
	public	List<DisablingRigSwitch>	
		switchList;

	[System.NonSerialized]
	private InputRig
		rig;			


	// ---------------------
	public DisablingConditionSet(InputRig rig)
		{
		this.rig		= rig;
		this.switchList	= new List<DisablingRigSwitch>(32);

		
		this.mobileModeRelation			= MobileModeRelation.EnabledOnlyInMobileMode;
		this.disableWhenTouchScreenInactive= true;
		this.disableWhenCursorIsUnlocked	= false;
		}
		
	
	// ----------------------
	public void SetRig(InputRig rig)
		{
		this.rig = rig;
		}



	// ------------------
	public bool IsInEffect()
		{
		if ((this.mobileModeRelation != MobileModeRelation.AlwaysEnabled) &&
			(CF2Input.IsInMobileMode() ? 
				(this.mobileModeRelation == MobileModeRelation.DisabledInMobileMode) :
				(this.mobileModeRelation == MobileModeRelation.EnabledOnlyInMobileMode)))	
			return true;
		
		if (this.rig == null)
			return false;

		if (this.disableWhenTouchScreenInactive && this.rig.AreTouchControlsSleeping())
			return true;
		if (this.disableWhenCursorIsUnlocked && !CFScreen.lockCursor)
			return true;
		if (this.IsDisabledByRigSwitches())
			return true;

		return false;
		
		}

	
	// ---------------------
	public bool IsDisabledByRigSwitches()
		{	
		if (this.rig == null)	
			return false;
			
		for (int i = 0; i < this.switchList.Count; ++i)
			{
			if (this.switchList[i].IsInEffect(this.rig))
				return true;	
			}

		return false;
		}

		

	// -------------------
	[System.Serializable]
	public class DisablingRigSwitch
		{
		public string	name;
		public bool		disableWhenSwitchIsOff;
		private int		cachedId;
			
			
		// ---------------
		public DisablingRigSwitch()
			{
			this.name = "";
			}

		// ---------------------
		public DisablingRigSwitch(string name)
			{
			this.name = name;
			}

		// ---------------------
		public bool IsInEffect(InputRig rig)
			{
			return (rig.rigSwitches.GetSwitchState(this.name, ref this.cachedId, this.disableWhenSwitchIsOff) != this.disableWhenSwitchIsOff);
			}	
		
		}
	
	}
}

//! \endcond
