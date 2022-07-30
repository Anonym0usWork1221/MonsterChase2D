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

// ---------------------------
/// Digital Source Binding
// ---------------------------
	
[System.Serializable]
public class DigitalBinding : InputBindingBase
	{
	public List<AxisElem>
		axisList;
	public List<KeyCode>
		keyList;

		
	// -----------------------
	// Digital Axis Binding Element
	// -----------------------
	[System.Serializable]
	public class AxisElem
		{
		public string	axisName;
		public bool 	axisPositiveSide;

		[System.NonSerialized]
		private int		cachedAxisId;


		// --------------------
		public AxisElem()
			{
			this.axisPositiveSide	= true;
			this.axisName			= string.Empty;
			}

		// --------------------
		 bool OnIsBoundToAxis(string axisName, InputRig rig)
			{
			return ((this.axisName == axisName));
			}

		// ----------------
		public void SetAxis(string axisName, bool positiveSide)
			{
			this.axisName 			= axisName;
			this.axisPositiveSide	= positiveSide;
			}

		// -----------------
		public void CopyFrom(AxisElem b)
			{
			this.axisName			= b.axisName;	
			this.axisPositiveSide	= b.axisPositiveSide;
			}

		// -----------------
		public void ApplyToRig(InputRig rig, bool skipIfTargetIsMuted)
			{
			InputRig.AxisConfig a = rig.GetAxisConfig(this.axisName, ref this.cachedAxisId);
			if (a == null)
				return;

			if (!skipIfTargetIsMuted || !a.IsMuted())
				a.SetSignedDigital(this.axisPositiveSide);
			//rig.SetAxisDigital(this.axisName, ref this.cachedAxisId, !this.axisPositiveSide);
			}
	
		}

	
	// -----------------
	private void BasicConstructor()
		{
		this.enabled		= false;

		this.keyList = new List<KeyCode>(1);
		this.axisList = new List<AxisElem>(1);
		}


	// -----------------
	public DigitalBinding(InputBindingBase parent = null) : base(parent)
		{
		this.BasicConstructor();
		}
		

	// -----------------
	public DigitalBinding(KeyCode key, bool bindToAxis, string axisName, bool axisNegSide, bool enabled, InputBindingBase parent = null) : base(parent)
		{	
		this.BasicConstructor();

		if (enabled)
			this.Enable();

		if (key != KeyCode.None)
			this.AddKey(key);

		if (!string.IsNullOrEmpty(axisName))
			this.AddAxis().SetAxis(axisName, !axisNegSide);

		}


	// -----------------
	public DigitalBinding(KeyCode key, string axisName, bool enabled, InputBindingBase parent = null) : base(parent)
		{	
		this.BasicConstructor();

		if (enabled)
			this.Enable();

		if (key != KeyCode.None)
			this.AddKey(key);

		if (!string.IsNullOrEmpty(axisName))
			this.AddAxis().SetAxis(axisName, true);

		}

	// -----------------
	public DigitalBinding(KeyCode key, bool enabled, InputBindingBase parent = null) : base(parent)
		{	
		this.BasicConstructor();

		if (enabled)
			this.Enable();

		if (key != KeyCode.None)
			this.AddKey(key);
		}

		
	// ----------------
	public void CopyFrom(DigitalBinding b)
		{
		if ((b == null)) 
			return;
		
		if ((this.enabled = b.enabled))
			{	
			this.Enable();
				
			// Copy axes...
		
			//if (this.bindToAxis = b.bindToAxis)
				{
				if (this.axisList.Count != b.axisList.Count)
					{
					this.axisList.Clear();
					for (int i = 0; i < b.axisList.Count; ++i)
						this.AddAxis();
					}					
				
				for (int i = 0; i < b.axisList.Count; ++i)
					this.axisList[i].CopyFrom(b.axisList[i]);
				}

			// Copy keys...
	
			//if (this.bindToKey = b.bindToKey)
				{
				if (this.keyList.Count != b.keyList.Count)
					{
					this.keyList.Clear();
					for (int i = 0; i < b.keyList.Count; ++i)
						this.AddKey(b.keyList[i]);
					}					
				else
					{
					for (int i = 0; i < b.keyList.Count; ++i)
						this.keyList[i] = b.keyList[i];
					}	
				}

			
			}
		}



	// -----------------
	public void Sync(bool state, InputRig rig, bool skipIfTargetIsMuted = false)
		{
		if ((state == false) || (rig == null) || !this.enabled)
			return;

		for (int i = 0; i < this.keyList.Count; ++i)
			rig.SetKeyCode(this.keyList[i]); 
	
		for (int i = 0; i < this.axisList.Count; ++i)
			{
			AxisElem a = this.axisList[i];	
			a.ApplyToRig(rig, skipIfTargetIsMuted);			
			}
		}


	// -------------------
	public void Clear()
		{
		this.ClearKeys();
		this.ClearAxes();
		}

	// --------------------
	public void ClearKeys()
		{
		this.keyList.Clear();
		}

	// -----------------
	public void ClearAxes()
		{
		this.axisList.Clear();
		}

		
	// --------------------
	public void AddKey(KeyCode code)
		{
		this.keyList.Add(code);
		}

	// --------------------
	public void RemoveLastKey()
		{
		if (this.keyList.Count > 0)
			this.keyList.RemoveAt(this.keyList.Count - 1);
		}

	// ----------------------	
	public void ReplaceKey(int keyElemId, KeyCode key)	
		{
		if ((keyElemId < 0) || (keyElemId >= this.keyList.Count))
			return;
		
		this.keyList[keyElemId] = key;
		}

	
	// --------------------	
	public AxisElem AddAxis()
		{
		AxisElem a = new AxisElem();
		this.axisList.Add(a);
		return a;
		}
		
	// -----------------------
	public AxisElem GetAxisElem(int id)
		{
		return (((id < 0) || (id >= this.axisList.Count)) ? null : this.axisList[id]);
		}


	// --------------------
	public void RemoveLastAxis()
		{
		if (this.axisList.Count > 0)
			this.axisList.RemoveAt(this.axisList.Count - 1);
		}


	// -----------------
	override protected bool OnIsBoundToKey(KeyCode keycode, InputRig rig)
		{
		return (this.enabled && (this.keyList.Count > 0) && (this.keyList.IndexOf(keycode) >= 0)); //(this.key == keycode));
		}

	// --------------------
	override protected bool OnIsBoundToAxis(string axisName, InputRig rig)
		{
		return (this.enabled && (this.axisList.Count > 0) && (this.axisList.FindIndex(x => (x.axisName == axisName)) >= 0)); //(this.axisName == axisName));
		}



	}



}

//! \endcond
