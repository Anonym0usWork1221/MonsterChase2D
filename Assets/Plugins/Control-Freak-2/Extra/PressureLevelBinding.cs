using UnityEngine;
using System.Collections.Generic;

namespace ControlFreak2.Extra
{
public class PressureLevelBinding : MonoBehaviour 
	{
	private InputRig rig;
	
	[Tooltip("Source axis name")]
	public string sourceAxis;
	private int sourceAxisId;

	public List<RangeConfig> rangeConfigList;
	
	

	// -----------------
	public PressureLevelBinding() : base()
		{
		this.rangeConfigList = new List<RangeConfig>(new RangeConfig[] { new RangeConfig() });
		}


	// -------------------
	void OnEnable()
		{
		if (this.rig == null)
			this.rig = this.GetComponentInParent<InputRig>();
	
		if (this.rig != null)
			this.rig.onAddExtraInput += this.UpdateRanges;
		}


	// ----------------
	void OnDisable()
		{
		if (this.rig != null)
			this.rig.onAddExtraInput -= this.UpdateRanges;
		}


	// -----------------
	protected void UpdateRanges()
		{
		if ((this.rig == null) || string.IsNullOrEmpty(this.sourceAxis))
			return;

		float sourceVal = this.rig.GetAxisRaw(this.sourceAxis, ref this.sourceAxisId);

		for (int i = 0; i < this.rangeConfigList.Count; ++i)
			this.rangeConfigList[i].Update(this.rig, sourceVal);	
		}


	// --------------------	
	[System.Serializable]
	public class RangeConfig
		{
		[Range(-1.0f, 1.0f)]
		public float 
			min = 0.5f,
			max = 1.0f;

		[Tooltip("Target key code.")]
		public KeyCode 
			keyTarget = KeyCode.None;
		[Tooltip("Target axis name.")]
		public string
			axisTarget = "";
		public bool 
			positiveAxisSide = true;
		private int 
			cachedAxisId;

		// ----------------
		public void Update(InputRig rig, float val)
			{	
			if ((val < this.min) || (val > this.max)) 
				return;

			if (this.keyTarget != KeyCode.None)
				rig.SetKeyCode(this.keyTarget);
		
			if (!string.IsNullOrEmpty(this.axisTarget))
				rig.SetAxisDigital(this.axisTarget, ref this.cachedAxisId, !this.positiveAxisSide);
			}
		
		}

	}
}
