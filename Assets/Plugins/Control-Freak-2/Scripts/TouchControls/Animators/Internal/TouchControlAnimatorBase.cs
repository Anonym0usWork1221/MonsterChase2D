// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2.Internal
{
[ExecuteInEditMode()]
public abstract class TouchControlAnimatorBase : ControlFreak2.Internal.ComponentBase
	{
	public bool 
		autoConnectToSource;
	public TouchControl 
		sourceControl;

	protected System.Type
		sourceType;		

	// ----------------------
	public TouchControlAnimatorBase(System.Type sourceType) : base()
		{
		this.sourceType = sourceType;
		}

	// -----------------	
	abstract protected void OnUpdateAnimator(bool skipAnim);
		


	// ----------------------
	public void UpdateAnimator(bool skipAnim)
		{
		if (this.sourceControl == null)
			return;

		this.OnUpdateAnimator(skipAnim);		
		}


	// -----------------
	public void SetSourceControl(TouchControl c)
		{
		if (this.sourceControl != null)
			this.sourceControl.RemoveAnimator(this);
			
		if ((c != null) && !c.CanBeUsed())
			c = null;

		this.sourceControl = c;	

		if (this.sourceControl != null)
			this.sourceControl.AddAnimator(this);

		}

	// --------------------
	public System.Type GetSourceControlType()
		{
		return this.sourceType;
		}

	// --------------------
	public TouchControl FindAutoSource()
		{
		return (TouchControl)this.GetComponentInParent(this.sourceType);
		}
		
	// -----------------
	public void AutoConnectToSource()
		{
		TouchControl source = this.FindAutoSource();
		if (source != null)
			this.SetSourceControl(source);
		}

	// -----------------
	public bool IsIllegallyAttachedToSource()
		{
		return ((this.sourceControl != null) && (this.sourceControl.gameObject == this.gameObject));
		}

		
	// -----------------
	public void InvalidateHierarchy()
		{
		if (this.autoConnectToSource && (this.sourceControl == null))
			this.SetSourceControl(this.FindAutoSource()); 
		}
		



	// ----------------------
	override protected void OnInitComponent()
		{
		if (this.autoConnectToSource || (this.sourceControl == null))
			this.SetSourceControl(this.FindAutoSource()); 

		}

	
	// ------------------
	override protected void OnDestroyComponent()	{}	
	override protected void OnEnableComponent()	
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			ControlFreak2Editor.CFEditorUtils.AddOnHierarchyChange(this.InvalidateHierarchy);
#endif

		}
 
	override protected void OnDisableComponent()	
		{
#if UNITY_EDITOR
		if (CFUtils.editorStopped)
			ControlFreak2Editor.CFEditorUtils.RemoveOnHierarchyChange(this.InvalidateHierarchy);
#endif
		}	


		
#if UNITY_EDITOR
	void Update()
		{
		if (CFUtils.editorStopped)
			this.UpdateAnimator(true);
		}
#endif


	}
}


//! \endcond
