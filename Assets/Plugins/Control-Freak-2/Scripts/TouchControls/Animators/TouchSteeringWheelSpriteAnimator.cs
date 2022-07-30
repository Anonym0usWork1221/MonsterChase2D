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
	
// ------------------
//! Touch Steering Wheel Sprite Animator class.
// ------------------
[RequireComponent(typeof(RectTransform))]
public class TouchSteeringWheelSpriteAnimator : TouchControlSpriteAnimatorBase, ISpriteAnimator
	{

	// ----------------
	//! Steering wheel animator's states.
	// -----------------
	public enum ControlState
		{
		Neutral,
		Pressed,	

		All
		}


//! \cond
	public SpriteConfig
		spritePressed;
		
	public float	
		rotationRange			= 45,
		rotationSmoothingTime	= 0.1f;




	// ----------------------
	public TouchSteeringWheelSpriteAnimator() : base(typeof(TouchSteeringWheel))
		{
		this.rotationRange = 45;
		this.rotationSmoothingTime = 0.05f;

		this.spritePressed	= new SpriteConfig(true, false, 1.2f);
		}
	
//! \endcond

		
	// ------------------
	//! Set unified sprite for all states.
	// -----------------
	public void SetSprite(Sprite sprite)
		{
		this.spriteNeutral.sprite = sprite;
		this.spritePressed.sprite = sprite;
		}


	// ------------------
	//! Set unified color for all states.
	// -----------------
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

		if ((this.image.sprite != null) && (this.spriteNeutral.sprite == null) && (this.spritePressed.sprite == null) )
			{
			this.spriteNeutral.sprite = this.image.sprite;
			this.spritePressed.sprite = this.image.sprite;
			}
		}
	


	// ----------------------
	override protected void OnUpdateAnimator(bool skipAnim)
		{
		if ((this.sourceControl == null) || (this.image == null))
			return;
			
		TouchSteeringWheel wheel = (TouchSteeringWheel)this.sourceControl;


		SpriteConfig sprite = null;

		if (wheel.Pressed() && ((sprite == null) || !sprite.enabled))
			sprite = this.spritePressed;
		
		if (((sprite == null) || !sprite.enabled))
			sprite = this.spriteNeutral;




		if (!CFUtils.editorStopped && !this.IsIllegallyAttachedToSource())
			this.extraRotation = CFUtils.SmoothTowardsAngle(this.extraRotation, -(wheel.GetValue() * 
				((wheel.wheelMode == TouchSteeringWheel.WheelMode.Swipe) ? this.rotationRange : wheel.maxTurnAngle)), 
				this.rotationSmoothingTime, CFUtils.realDeltaTimeClamped, 0.001f);
		else
			this.extraRotation = 0;


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
