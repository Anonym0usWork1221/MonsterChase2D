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

// ------------------------
/// Analog axis binding.
// ------------------------

[System.Serializable]
public class AxisBinding : InputBindingBase
	{
	public List<TargetElem>
		targetList;		



		
	// -------------------------
	private void BasicConstructor()
		{
		this.enabled			= false;
		this.targetList		= new List<TargetElem>(1);	

		}

		
	// -------------------
	public AxisBinding(InputBindingBase parent = null) : base(parent)
		{
		this.BasicConstructor();
		}

	// -----------------------
	public AxisBinding(string singleName, bool enabled, InputBindingBase parent = null) : base(parent)
		{
		this.BasicConstructor();
		this.AddTarget().SetSingleAxis(singleName, false);

		if (enabled)
			this.Enable();
		}
		
		

	// ----------------------
	public void CopyFrom(AxisBinding b)
		{
		if (this.enabled = b.enabled)
			{
			this.Enable();

			if (this.targetList.Count != b.targetList.Count)
				{
				this.targetList.Clear();
				for (int i = 0; i < b.targetList.Count; ++i)
					this.AddTarget();
				}

			for (int i = 0; i < b.targetList.Count; ++i)
				this.targetList[i].CopyFrom(b.targetList[i]);

			}
		}

		
	// ---------------------
	public void Clear()
		{
		this.targetList.Clear();
		}

	// ---------------------
	public TargetElem AddTarget()
		{
		TargetElem t = new TargetElem();
		this.targetList.Add(t);
		return t; 
		}
		
	// --------------------------
	public void RemoveLastTarget()
		{
		if (this.targetList.Count > 0)
			this.targetList.RemoveAt(this.targetList.Count - 1);
		}

	// ---------------------
	public TargetElem GetTarget(int axisElemId)
		{
		return (((axisElemId < 0) || (axisElemId >= this.targetList.Count)) ? null : this.targetList[axisElemId]);
		}

	// ---------------------
	public void SyncFloat(float val, InputRig.InputSource source, InputRig rig)
		{
		if ((rig == null) || !this.enabled)
			return;

		for (int i = 0; i < this.targetList.Count; ++i)
			this.targetList[i].SyncFloat(val, source, rig);

		}

		
	// ---------------------
	public void SyncScroll(int val, InputRig rig)
		{
		if ((rig == null) || !this.enabled)
			return;
		
		for (int i = 0; i < this.targetList.Count; ++i)
			{
			this.targetList[i].SyncScroll(val, rig);
			}
	
		//rig.SetAxisScroll(this.singleAxis, ref this.singleAxisId, (this.reverseSingleAxis ? -val : val)); 
		}



	// --------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{
		if (!this.enabled)
			return false;

		for (int i = 0; i < this.targetList.Count; ++i)
			{
			if (this.targetList[i].IsBoundToAxis(axisName))
				return true;
			}

		return false;
		}

	// -----------------
	override protected bool OnIsBoundToKey(KeyCode keycode, InputRig rig)
		{
		return false;
		}

		

	// --------------------
	public float GetAxis(InputRig rig)
		{
		if (!this.enabled)	
			return 0;
		if (rig == null)
			rig = CF2Input.activeRig;
		if (rig == null)
			return 0; 

		if ((this.targetList == null) || (this.targetList.Count == 0))
			return 0;
			
		return this.targetList[0].GetAxis(rig);
		}
		



	// -----------------------
	// Target Element Class.
	// -----------------------
	[System.Serializable]
	public class TargetElem
		{
		public bool		separateAxes;
		
		public string	singleAxis;
		public bool		reverseSingleAxis;
		
		public string	positiveAxis;
		public string	negativeAxis;
		public bool		positiveAxisAsPositive;
		public bool		negativeAxisAsPositive;
		
		private int		singleAxisId;
		private int		positiveAxisId;
		private int		negativeAxisId;

		// -------------------
		public TargetElem()
			{
			this.separateAxes		= false;
			
			this.singleAxis			= "";
			this.reverseSingleAxis	= false;
		
			this.positiveAxis		= "";
			this.negativeAxis		= "";
			
			this.positiveAxisAsPositive	= true;
			this.negativeAxisAsPositive	= true;
	
			this.singleAxisId		= 0;
			this.positiveAxisId		= 0;
			this.negativeAxisId		= 0;
			}

	
		// -----------------
		public void SetSingleAxis(string name, bool flip)
			{
			this.separateAxes		= false;
			this.singleAxis			= name;	
			this.reverseSingleAxis	= flip;
			}

		// -----------------
		public void SetSeparateAxis(string name, bool positiveSide, bool asPositive)
			{
			this.separateAxes = true;
			if (positiveSide)
				{
				this.positiveAxis			= name;
				this.positiveAxisAsPositive	= asPositive;
				}
			else
				{
				this.negativeAxis			= name;
				this.negativeAxisAsPositive	= asPositive;
				}
			}
	
		// ---------------------
		public void SyncFloat(float val, InputRig.InputSource source, InputRig rig)
			{
			if (this.separateAxes)
				{
				if (val >= 0)
					rig.SetAxis(this.positiveAxis, ref this.positiveAxisId, (this.positiveAxisAsPositive ? val : -val), source);
				else
					rig.SetAxis(this.negativeAxis, ref this.negativeAxisId, (this.negativeAxisAsPositive ? -val : val), source);
				}
			else
				{
				rig.SetAxis(this.singleAxis, ref this.singleAxisId, (this.reverseSingleAxis ? -val : val), source);
				}
			}
	
			
		// ---------------------
		public void SyncScroll(int val, InputRig rig)
			{
			if (!this.separateAxes)
				{
				rig.SetAxisScroll(this.singleAxis, ref this.singleAxisId, (this.reverseSingleAxis ? -val : val)); 
				}
			}
	
	
		// --------------------
		public float GetAxis(InputRig rig)
			{
			if (rig == null)
				return 0; 
	
			if (this.separateAxes)
				{
				return (((this.positiveAxisAsPositive ? 1.0f : -1.0f) * rig.GetAxis(this.positiveAxis, ref this.positiveAxisId)) - 
						((this.negativeAxisAsPositive ? 1.0f : -1.0f) * rig.GetAxis(this.negativeAxis, ref this.negativeAxisId)));
				}
			else
				return rig.GetAxis(this.singleAxis, ref this.singleAxisId);
			}
			

		// -------------------
		public bool IsBoundToAxis(string axisName)
			{
			return (this.separateAxes ? ((this.positiveAxis == axisName) || (this.negativeAxis == axisName)) : (this.singleAxis == axisName));
			}
		

		// -------------------
		public void CopyFrom(TargetElem elem)
			{
			this.separateAxes		= elem.separateAxes;
			this.singleAxis			= elem.singleAxis;
			this.reverseSingleAxis	= elem.reverseSingleAxis;

			this.positiveAxis			= elem.positiveAxis;
			this.positiveAxisAsPositive	= elem.positiveAxisAsPositive;
			this.negativeAxis			= elem.negativeAxis;	
			this.negativeAxisAsPositive	= elem.negativeAxisAsPositive;
			}

		}

	
	}



}

//! \endcond
