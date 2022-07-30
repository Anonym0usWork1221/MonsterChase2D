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
//! Super Touch Zone Sprite Animator class.
// ---------------------

[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Image)), ExecuteInEditMode()]
public class SuperTouchZoneSpriteAnimator : TouchControlSpriteAnimatorBase, ISpriteAnimator
	{

//! \cond

	public SpriteConfig
		spriteRawPress,
		spriteNormalPress,
		spriteLongPress,
		spriteTap,
		spriteDoubleTap,
		spriteLongTap,
		spriteNormalScrollU,
		spriteNormalScrollR,
		spriteNormalScrollD,
		spriteNormalScrollL,
		spriteLongScrollU,
		spriteLongScrollR,
		spriteLongScrollD,
		spriteLongScrollL;



	// ----------------------
	public SuperTouchZoneSpriteAnimator() : base(typeof(ControlFreak2.SuperTouchZone))
		{
		this.spriteRawPress			= new SpriteConfig(true, false, 1.2f);
		this.spriteNormalPress		= new SpriteConfig(false, false, 1.2f);
		this.spriteLongPress		= new SpriteConfig(false, false, 1.2f);
		this.spriteTap				= new SpriteConfig(false, true, 1.2f);
		this.spriteDoubleTap		= new SpriteConfig(false, true, 1.2f);
		this.spriteLongTap			= new SpriteConfig(false, true, 1.2f);
		this.spriteNormalScrollU	= new SpriteConfig(false, true, 1.2f);
		this.spriteNormalScrollR	= new SpriteConfig(false, true, 1.2f);
		this.spriteNormalScrollD	= new SpriteConfig(false, true, 1.2f);
		this.spriteNormalScrollL	= new SpriteConfig(false, true, 1.2f);
		this.spriteLongScrollU		= new SpriteConfig(false, true, 1.2f);
		this.spriteLongScrollR		= new SpriteConfig(false, true, 1.2f);
		this.spriteLongScrollD		= new SpriteConfig(false, true, 1.2f);
		this.spriteLongScrollL		= new SpriteConfig(false, true, 1.2f);
		}
	
//! \endcond


	// ------------------
	//! Set unified sprite for all states.
	// -----------------
	public void SetSprite(Sprite sprite)	
		{
		for (ControlState i = ControlStateFirst; i < ControlStateCount; ++i)
			this.GetStateSpriteConfig(i).sprite = sprite;

		}

	// ------------------
	//! Set unified color for all states.
	// -----------------
	public void SetColor(Color color)	
		{
		for (ControlState i = ControlStateFirst; i < ControlStateCount; ++i)
			this.GetStateSpriteConfig(i).color = color;

		}




	// --------------------------
	//! Super Touch Zone states.
	// --------------------------
	public enum ControlState
		{
		Neutral,
		RawPress,
		NormalPress,
		LongPress,
		Tap,
		DoubleTap,
		LongTap,
		NormalScrollU,
		NormalScrollR,
		NormalScrollD,
		NormalScrollL,
		LongScrollU,
		LongScrollR,
		LongScrollD,
		LongScrollL,

		All
		}

	public const ControlState
		ControlStateFirst	= ControlState.Neutral,	
		ControlStateCount	= ControlState.All;


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
			case ControlState.Neutral		: return this.spriteNeutral;
			case ControlState.RawPress 		: return this.spriteRawPress;
			case ControlState.NormalPress	: return this.spriteNormalPress;
			case ControlState.LongPress 	: return this.spriteLongPress;
			case ControlState.Tap 			: return this.spriteTap;
			case ControlState.DoubleTap 	: return this.spriteDoubleTap;
			case ControlState.LongTap 		: return this.spriteLongTap;
			case ControlState.NormalScrollU : return this.spriteNormalScrollU;
			case ControlState.NormalScrollR	: return this.spriteNormalScrollR;
			case ControlState.NormalScrollD	: return this.spriteNormalScrollD;
			case ControlState.NormalScrollL	: return this.spriteNormalScrollL;
			case ControlState.LongScrollU	: return this.spriteLongScrollU;
			case ControlState.LongScrollR	: return this.spriteLongScrollR;
			case ControlState.LongScrollD	: return this.spriteLongScrollD;
			case ControlState.LongScrollL	: return this.spriteLongScrollL;
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
		ControlFreak2.SuperTouchZone zone = (ControlFreak2.SuperTouchZone)this.sourceControl;

		if ((zone == null) || (this.image == null))
			return;

		Vector2 scrollDelta = zone.GetScrollDelta(1);

		SpriteConfig sprite = null;

		if ((zone.JustTapped(1, 2) || zone.JustTapped(2, 2) || zone.JustTapped(3, 2)) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteDoubleTap;
		if ((zone.JustTapped(1, 1) || zone.JustTapped(2, 1) || zone.JustTapped(3, 1)) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteTap;

		if ((zone.JustLongTapped(1) || zone.JustLongTapped(2) || zone.JustLongTapped(3)) && ((sprite == null) || !sprite.enabled)) sprite = this.spriteLongTap;

		if ((zone.PressedLong(1) || zone.PressedLong(2) || zone.PressedLong(3)) && ((sprite == null) || !sprite.enabled))
			{
			if ((scrollDelta.x > 0) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteLongScrollR;
			if ((scrollDelta.x < 0) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteLongScrollL;
			if ((scrollDelta.y > 0) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteLongScrollU;
			if ((scrollDelta.y < 0) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteLongScrollD;

			if (((sprite == null) || !sprite.enabled))							{ sprite = this.spriteLongPress; } //Debug.Log(CFUtils.LogPrefix()  + "Long Press"); }
			}
		else if ((zone.PressedNormal(1) || zone.PressedNormal(2) || zone.PressedNormal(3)) && ((sprite == null) || !sprite.enabled))
			{
			if ((scrollDelta.x > 0) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteNormalScrollR;
			if ((scrollDelta.x < 0) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteNormalScrollL;
			if ((scrollDelta.y > 0) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteNormalScrollU;
			if ((scrollDelta.y < 0) && ((sprite == null) || !sprite.enabled))	sprite = this.spriteNormalScrollD;
			if (((sprite == null) || !sprite.enabled)) 							sprite = this.spriteNormalPress;
			}
		
		if ((zone.PressedRaw(1) || zone.PressedRaw(2) || zone.PressedRaw(3)) && ((sprite == null) || !sprite.enabled))	
			sprite = this.spriteRawPress;

		if ((sprite == null) || !sprite.enabled)
			sprite = this.spriteNeutral;


		this.BeginSpriteAnim(sprite, skipAnim);

		this.UpdateSpriteAnimation(skipAnim);
		}


		

	// ----------------------
	MonoBehaviour ISpriteAnimator.GetComponent() { return this; }

	// -------------------------
	void ISpriteAnimator.AddUsedSprites(ISpriteOptimizer optimizer)
		{
		for (ControlState i = ControlStateFirst; i < ControlStateCount; ++i)
			optimizer.AddSprite(this.GetStateSpriteConfig(i).sprite);
		}


	// ------------------
	void ISpriteAnimator.OnSpriteOptimization(ISpriteOptimizer optimizer)
		{
		for (ControlState i = ControlStateFirst; i < ControlStateCount; ++i)
			{
			SpriteConfig spr = this.GetStateSpriteConfig(i);
				spr.sprite = optimizer.GetOptimizedSprite(spr.sprite);
			}

		}
	
//! \endcond


	}
}
