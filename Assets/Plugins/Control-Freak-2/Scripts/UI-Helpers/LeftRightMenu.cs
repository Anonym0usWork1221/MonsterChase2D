// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ControlFreak2.UI
{

public class LeftRightMenu : UnityEngine.UI.Selectable 
	{
	public UnityEngine.UI.Button
		buttonLeft,
		buttonRight,
		buttonBack;

	public RectTransform
		contentPlaceholder;
	
	public UnityEngine.UI.Text
		titleText;
		


	public delegate void OptionSwitchCallback(int dir);

	public System.Action
		onBackPressed;
	public OptionSwitchCallback
		onOptionSwitch;		

	public RectTransform[]
		optionItems;

		
	// -----------------
	override public void OnSelect(BaseEventData data)
		{
		base.OnSelect(data);
		}


	// -----------------
	override public void OnDeselect(BaseEventData data)
		{
		base.OnDeselect(data);
		}

	// ----------------
	override public void OnMove(AxisEventData data)
		{
		if ((data.moveDir == MoveDirection.Left) || (data.moveDir == MoveDirection.Right))
			{
			this.OnSwitch((data.moveDir == MoveDirection.Left) ? -1 : 1);
			data.Use();
			} 
		else
			{
			base.OnMove(data);
			}
		}

	// ----------------
	public int GetItemCount()
		{
		return this.optionItems.Length;
		}

	// ------------------
	public void SetItemActive(int id)
		{
		for (int i = 0; i < this.optionItems.Length; ++i)
			{
			RectTransform r = this.optionItems[i];
			if (r == null)
				continue;
			r.gameObject.SetActive((id == i));
			}
		}

		
		

	// ----------------
	public void SetTitle(string title)
		{
		if (this.titleText != null)	
			this.titleText.text = title;
		}



	// --------------------
	override protected void OnEnable()
		{	
		base.OnEnable();

		this.buttonLeft.onClick.AddListener(this.OnLeftPressed);
		this.buttonRight.onClick.AddListener(this.OnRightPressed);
		this.buttonBack.onClick.AddListener(this.OnBackPressed);

		
		// Prepare items....

		for (int i = 0; i < this.optionItems.Length; ++i)
			{
			RectTransform r = this.optionItems[i];
			if (r == null)
				continue;
	
			r.SetParent(this.contentPlaceholder, false); 
			r.gameObject.SetActive(false);
			}

		}
	
	// --------------------
	override protected void OnDisable()
		{
		this.buttonLeft.onClick.RemoveAllListeners();
		this.buttonRight.onClick.RemoveAllListeners();
		this.buttonBack.onClick.RemoveAllListeners();

		base.OnDisable();
		}

		
	// -----------------
	private void OnSwitch(int dir)
		{
		if (this.onOptionSwitch != null)
			this.onOptionSwitch(dir);
		}
		
	// ------------------
	private void OnBackPressed()
		{
		if (this.onBackPressed != null)
			this.onBackPressed();
	
		}
		


	// ------------------
	private void OnLeftPressed()	{ this.OnSwitch(-1); }
	private void OnRightPressed()	{ this.OnSwitch(1); }
	
	}
}

//! \endcond

