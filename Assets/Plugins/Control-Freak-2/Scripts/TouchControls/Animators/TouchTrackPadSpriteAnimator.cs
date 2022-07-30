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
	
// --------------------
//! Touch Track Pad Sprite Animator class.
// --------------------
[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Image)), ExecuteInEditMode()]
public class TouchTrackPadSpriteAnimator : TouchControlSpriteAnimatorBase, ISpriteAnimator
	{
	//! Track Pad Animator Control State.
	public enum ControlState
		{
		Neutral,
		Pressed,	

		All
		}

//! \cond

	public SpriteConfig
		spritePressed;



	// ----------------------
	public TouchTrackPadSpriteAnimator() : base(typeof(TouchTrackPad))
		{
		this.spritePressed				= new SpriteConfig(true, false, 1.2f);
		}
	

//! \endcond

	// -----------------	
	//! Set unified sprite for all states.
	// -----------------
	public void SetSprite(Sprite sprite)
		{
		this.spriteNeutral.sprite = sprite;
		this.spritePressed.sprite = sprite;
		}

	// --------------------
	//! Set unified color for all states.
	// ----------------------	
	public void SetColor(Color color)	
		{
		this.spriteNeutral.color			= color;	
		this.spritePressed.color			= color;	
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


	// ----------------
	//! Get Sprite Config for given state.
	// -----------------
	public SpriteConfig GetStateSpriteConfig(ControlState state)
		{
		switch (state)
			{
			case ControlState.Neutral				: return this.spriteNeutral;
			case ControlState.Pressed 				: return this.spritePressed;
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
		TouchTrackPad trackPad = (TouchTrackPad)this.sourceControl;

		if ((trackPad == null) || (this.image == null))
			return;


		SpriteConfig sprite = null;

		if (trackPad.Pressed() && ((sprite == null) || !sprite.enabled))
			sprite = this.spritePressed;
		
		if (((sprite == null) || !sprite.enabled))
			sprite = this.spriteNeutral;



		this.BeginSpriteAnim(sprite, skipAnim);

		this.UpdateSpriteAnimation(skipAnim);

		}


		

	// ----------------------
	MonoBehaviour ISpriteAnimator.GetComponent() { return this; }

	// -------------------------
	void ISpriteAnimator.AddUsedSprites(ISpriteOptimizer optimizer)
		{
		optimizer.AddSprite(this.spriteNeutral.sprite);
		optimizer.AddSprite(this.spritePressed.sprite);
		}


	// ------------------
	void ISpriteAnimator.OnSpriteOptimization(ISpriteOptimizer optimizer)
		{
		this.spriteNeutral.sprite = optimizer.GetOptimizedSprite(this.spriteNeutral.sprite);
		this.spritePressed.sprite = optimizer.GetOptimizedSprite(this.spritePressed.sprite);
		}
	
//! \endcond

	}
}
