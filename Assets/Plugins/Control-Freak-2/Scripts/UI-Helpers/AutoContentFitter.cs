// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2.UI
{
[ExecuteInEditMode()]
public class AutoContentFitter : MonoBehaviour
	{
	public RectTransform 
		source;

	

	public bool 
		autoWidth = false,
		autoHeight = false;

	public float 
		horzPadding = 10,
		vertPadding = 10;

	

	// ------------------
	void Update()
		{
		this.UpdatePreferredDimensions();
		}


	// ----------------------
	public void UpdatePreferredDimensions()
		{
		if (!this.autoWidth && !this.autoHeight)
			return;
	
		if (this.source == null)
			return;
	
		RectTransform tr = (RectTransform)this.transform;

		if (this.autoHeight)
				tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.source.rect.height + this.vertPadding);
	
		if (this.autoWidth)
			tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.source.rect.width + this.horzPadding);

	
		}

	}
}

//! \endcond
