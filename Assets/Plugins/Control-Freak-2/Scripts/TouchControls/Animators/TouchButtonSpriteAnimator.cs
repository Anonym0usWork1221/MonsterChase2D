// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using UnityEngine.UI;

using ControlFreak2.Internal;

namespace ControlFreak2
{
	
// ---------------------
//! Touch Button Sprite Animator class.
// ---------------------
[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Image)), ExecuteInEditMode()]
public class TouchButtonSpriteAnimator : TouchControlSpriteAnimatorBase, ISpriteAnimator
	{

	// --------------------
	//! Touch Button Sprite States.
	// ----------------------
	public enum ControlState
		{
		Neutral,
		Pressed,	
		Toggled,
		ToggledAndPressed,

		All
		}

//! \cond



	public SpriteConfig
		spritePressed,
		spriteToggled,
		spriteToggledAndPressed;


	// ----------------------
	public TouchButtonSpriteAnimator() : base(typeof(TouchButton))
		{
		this.spritePressed				= new SpriteConfig(true, false, 1.2f);
		this.spriteToggled 				= new SpriteConfig(false, false, 1.1f);
		this.spriteToggledAndPressed	= new SpriteConfig(false, false, 1.3f);

		this.spritePressed.scale			= 1.25f;
		this.spriteToggled.scale			= 1.1f;
		this.spriteToggledAndPressed.scale 	= 1.3f;

		}
	
//! \endcond



	// ------------------
	//! Set unified sprite for all states.
	// -----------------
	public void SetSprite(Sprite sprite)	
		{
		this.spriteNeutral.sprite			= sprite;	
		this.spritePressed.sprite			= sprite;	
		this.spriteToggled.sprite			= sprite;	
		this.spriteToggledAndPressed.sprite	= sprite;	
	
		}

	// ------------------
	//! Set unified color for all states.
	// -----------------
	public void SetColor(Color color)
		{
		this.spriteNeutral.color			= color;	
		this.spritePressed.color			= color;	
		this.spriteToggled.color			= color;	
		this.spriteToggledAndPressed.color	= color;	
		}	


	// ------------------
	public void SetStateSprite(ControlState state, Sprite sprite)
		{
		SpriteConfig c = this.GetStateSpriteConfig(state);
		if (c == null)
			this.SetSprite(sprite);
		else
			c.sprite = sprite;
		}


	// ------------------
	public void SetStateColor(ControlState state, Color color)
		{
		SpriteConfig c = this.GetStateSpriteConfig(state);
		if (c == null)
			this.SetColor(color);	
		else
			c.color = color;
		}


	// -----------------
	public SpriteConfig GetStateSpriteConfig(ControlState state)
		{
		switch (state)
			{
			case ControlState.Neutral				: return this.spriteNeutral;
			case ControlState.Pressed 				: return this.spritePressed;
			case ControlState.Toggled 				: return this.spriteToggled;
			case ControlState.ToggledAndPressed	: return this.spriteToggledAndPressed;

			}
		return null;
		}
	



//! \cond



	// ----------------------
	override protected void OnInitComponent()
		{	
		base.OnInitComponent();
		}



	// ----------------------
	override protected void OnUpdateAnimator(bool skipAnim)	
		{
		TouchButton button = (TouchButton)this.sourceControl;

		if ((button == null) || (this.image == null))
			return;


		bool pressed = button.Pressed();	
		bool toggled = button.Toggled();
	
		SpriteConfig sprite = null;

		if ((pressed && toggled) && ((sprite == null) || !sprite.enabled))
			sprite = this.spriteToggledAndPressed;

		if (toggled && ((sprite == null) || !sprite.enabled))
			sprite = this.spriteToggled;

		if (pressed && ((sprite == null) || !sprite.enabled))
			sprite = this.spritePressed;

		if (((sprite == null) || !sprite.enabled))
			sprite = this.spriteNeutral;


		this.BeginSpriteAnim((sprite == null) ? this.spriteNeutral : sprite, false);

		this.UpdateSpriteAnimation(skipAnim);
		}


		

	// ----------------------
	MonoBehaviour ISpriteAnimator.GetComponent() { return this; }

	// -------------------------
	void ISpriteAnimator.AddUsedSprites(ISpriteOptimizer optimizer)
		{
		optimizer.AddSprite(this.spriteNeutral.sprite);
		optimizer.AddSprite(this.spritePressed.sprite);
		optimizer.AddSprite(this.spriteToggled.sprite);
		optimizer.AddSprite(this.spriteToggledAndPressed.sprite);
		}


	// ------------------
	void ISpriteAnimator.OnSpriteOptimization(ISpriteOptimizer optimizer)
		{
		this.spriteNeutral.sprite = optimizer.GetOptimizedSprite(this.spriteNeutral.sprite);
		this.spritePressed.sprite = optimizer.GetOptimizedSprite(this.spritePressed.sprite);
		this.spriteToggled.sprite = optimizer.GetOptimizedSprite(this.spriteToggled.sprite);
		this.spriteToggledAndPressed.sprite = optimizer.GetOptimizedSprite(this.spriteToggledAndPressed.sprite);
		}
	
//! \endcond


	}
}
