// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;
using UnityEngine.UI;


namespace ControlFreak2.UI
{
[ExecuteInEditMode()]
public class AutoPreferredSize : MonoBehaviour 
	{
	public UnityEngine.UI.LayoutElement target;
		
	public bool 
		autoWidth = false,
		autoHeight = false;

	public float 
		horzPadding = 10,
		vertPadding = 10;

	// -----------------------
	void OnEnable()
		{
		if (this.target == null)
			this.target = this.GetComponent<UnityEngine.UI.LayoutElement>();
		}

	// ------------------
	void Update()
		{
		this.UpdatePreferredDimensions();
		}

		public RectTransform source;

	// ----------------------
	public void UpdatePreferredDimensions()
		{
		if (!this.autoWidth && !this.autoHeight)
			return;

if (this.source == null) return;

		if (this.autoHeight)
			this.target.minHeight = this.source.rect.height + this.vertPadding;

		if (this.autoWidth)
				this.target.minWidth = this.source.rect.width + this.horzPadding;


		}

	}
}

//! \endcond
