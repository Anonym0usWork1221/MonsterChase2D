// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;
using UnityEngine.UI;

using ControlFreak2.Internal;
using System;


namespace ControlFreak2
{
// ---------------------
//! Touch Joystick Sprite Animator class.
// ---------------------
[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Image)), ExecuteInEditMode()]
public class TouchJoystickSpriteAnimator : TouchControlSpriteAnimatorBase, ISpriteAnimator
	{

	// ---------------------
	//! Touch Joystick Animator Sprite Mode.
	// --------------------
	public enum SpriteMode
		{
		Simple,		//!< Simple pressed + released.
		FourWay,		//!< Separate states for 4-way digital.
		EightWay		//!< Separate states for 8-way digital.
		}


	// ---------------------
	//! Touch Joystick Animator Sprite Mode.
	// --------------------
	public enum RotationMode
		{
		Disabled,				//!< No rotation.
		SimpleHorizontal,		//!< Simple rotation controlled by horizontal analog value.
		SimpleVertical,		//!< Simple rotation controlled by vertical analog value.
		Compass					//!< Animator rotates in the direction of tilt.
		}



	// ---------------------
	//! Touch Joystick Animator Sprite States.
	// --------------------
	public enum ControlState
		{
		Neutral,
		NeutralPressed,

		U,
		UR,
		R,
		DR,
		D,
		DL,
		L,
		UL,	

		All
		}


//! \cond



	public SpriteMode spriteMode;

	//public SpriteConfig spriteNeutral;
	public SpriteConfig spriteNeutralPressed;
	public SpriteConfig spriteUp; 
	public SpriteConfig spriteUpRight; 
	public SpriteConfig spriteRight;
	public SpriteConfig spriteDownRight; 
	public SpriteConfig spriteDown; 
	public SpriteConfig spriteDownLeft; 
	public SpriteConfig spriteLeft; 
	public SpriteConfig spriteUpLeft; 
	
		

	public bool 	animateTransl = false;
	public Vector2	moveScale;

	public float
		translationSmoothingTime = 0.1f;
	
	

	public RotationMode
		rotationMode;

	public float 
		simpleRotationRange;

	public float
		rotationSmoothingTime = 0.1f;



	private float
		lastSafeCompassAngle;




	// -------------------
	public TouchJoystickSpriteAnimator() : base(typeof(TouchJoystick))
		{
		this.animateTransl 			= false;
		this.moveScale				= new Vector2(0.5f, 0.5f);

		this.rotationMode = RotationMode.Disabled;
		this.rotationSmoothingTime	= 0.01f;
		this.simpleRotationRange	= 45.0f;
		this.lastSafeCompassAngle	= 0;

		this.spriteNeutralPressed	= new SpriteConfig(true, false, 1.2f);
		this.spriteUp			 		= new SpriteConfig(false, false, 1.2f);
		this.spriteUpRight			= new SpriteConfig(false, false, 1.2f);
		this.spriteRight				= new SpriteConfig(false, false, 1.2f);
		this.spriteDownRight			= new SpriteConfig(false, false, 1.2f);
		this.spriteDown				= new SpriteConfig(false, false, 1.2f);
		this.spriteDownLeft			= new SpriteConfig(false, false, 1.2f);
		this.spriteLeft				= new SpriteConfig(false, false, 1.2f);
		this.spriteUpLeft			= new SpriteConfig(false, false, 1.2f);
		}

//! \endcond



	// ------------------
	//! Set unified sprite for all states.
	// -----------------
	public void SetSprite(Sprite sprite)
		{
		this.spriteNeutral.sprite		= sprite;
		this.spriteNeutralPressed.sprite= sprite;
		this.spriteUp.sprite			= sprite;
		this.spriteUpRight.sprite		= sprite;
		this.spriteRight.sprite			= sprite;
		this.spriteDownRight.sprite		= sprite;
		this.spriteDown.sprite			= sprite;
		this.spriteDownLeft.sprite		= sprite;
		this.spriteLeft.sprite			= sprite;
		this.spriteUpLeft.sprite		= sprite;
		}


	// ------------------
	//! Set unified color for all states.
	// -----------------
	public void SetColor(Color color)	
		{
		this.spriteNeutral.color		= color;
		this.spriteNeutralPressed.color	= color;
		this.spriteUp.color				= color;
		this.spriteUpRight.color		= color;
		this.spriteRight.color			= color;
		this.spriteDownRight.color		= color;
		this.spriteDown.color			= color;
		this.spriteDownLeft.color		= color;
		this.spriteLeft.color			= color;
		this.spriteUpLeft.color			= color;
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
			case ControlState.Neutral			: return this.spriteNeutral;
			case ControlState.NeutralPressed 	: return this.spriteNeutralPressed;
			case ControlState.U 				: return this.spriteUp;
			case ControlState.UR 				: return this.spriteUpRight;
			case ControlState.R 				: return this.spriteRight;
			case ControlState.DR 				: return this.spriteDownRight;
			case ControlState.D 				: return this.spriteDown;
			case ControlState.DL 				: return this.spriteDownLeft;
			case ControlState.L 				: return this.spriteLeft;
			case ControlState.UL 				: return this.spriteUpLeft;
			}

		return null;
		}
	

	// ----------------------
	override protected void OnInitComponent()
		{
		base.OnInitComponent();


		// Init animator's sprites if there's a sprite attached to the existing image component...

		if ((this.image != null) && (this.image.sprite != null) && (this.spriteNeutral.sprite == null))
			{
			this.spriteNeutral.sprite			= this.image.sprite;
			this.spriteNeutralPressed.sprite	= this.image.sprite;
			this.spriteUp.sprite				= this.image.sprite;
			this.spriteUpRight.sprite			= this.image.sprite;
			this.spriteRight.sprite				= this.image.sprite;
			this.spriteDownRight.sprite			= this.image.sprite;
			this.spriteDown.sprite				= this.image.sprite;
			this.spriteDownLeft.sprite			= this.image.sprite;
			this.spriteLeft.sprite				= this.image.sprite;
			this.spriteUpLeft.sprite			= this.image.sprite;
			}

		}


		


	// ----------------------
	override protected void OnUpdateAnimator(bool skipAnim)
		{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
			{
			//this.CheckHierarchy();
			//return;
			}
#endif
			
		TouchJoystick joystick = (TouchJoystick)this.sourceControl;

		if ((joystick == null) || (this.image == null))
			return;

			
		JoystickState joyState = joystick.GetState(); //false); //this.useVirtualJoystickState);
		





		SpriteConfig sprite = null;

		if ((this.spriteMode == SpriteMode.FourWay) || (this.spriteMode == SpriteMode.EightWay))
			{
			Dir curDir = Dir.N;
	
			if (this.spriteMode == SpriteMode.FourWay)
				curDir = joyState.GetDir4();
			else if (this.spriteMode == SpriteMode.EightWay)
				curDir = joyState.GetDir8();
	
			switch (curDir)
				{
				case Dir.U	: sprite = this.spriteUp; break; 
				case Dir.UR	: sprite = this.spriteUpRight; break; 
				case Dir.R	: sprite = this.spriteRight; break; 
				case Dir.DR	: sprite = this.spriteDownRight; break; 
				case Dir.D	: sprite = this.spriteDown; break; 
				case Dir.DL	: sprite = this.spriteDownLeft; break; 
				case Dir.L	: sprite = this.spriteLeft; break; 
				case Dir.UL	: sprite = this.spriteUpLeft; break; 
				}
			}

		
		if (joystick.Pressed() && ((sprite == null) || !sprite.enabled))
			sprite = this.spriteNeutralPressed;

		
		if (((sprite == null) || !sprite.enabled))
			sprite = this.spriteNeutral;

			
		if (!CFUtils.editorStopped && !this.IsIllegallyAttachedToSource())
			{
			Vector2 joyVec = joyState.GetVectorEx((joystick.shape == TouchControl.Shape.Rectangle) || (joystick.shape == TouchControl.Shape.Square));

			
			if (this.animateTransl)
				{
				this.extraOffset = CFUtils.SmoothTowardsVec2(this.extraOffset, Vector2.Scale(joyVec, this.moveScale), 
					this.translationSmoothingTime, CFUtils.realDeltaTimeClamped, 0.0001f);

				}
			else 
				this.extraOffset = Vector2.zero;


			if  (this.rotationMode != RotationMode.Disabled)
				{
				float targetAngle = 0;

				if (joystick.Pressed())
					{
					Vector2 v = joyState.GetVector();

					if (this.rotationMode == RotationMode.Compass)
						{
				
						if (v.sqrMagnitude > 0.0001f)
							this.lastSafeCompassAngle = joyState.GetAngle(); //CFUtils.VecToAngle(v.normalized);						
							
						targetAngle = -this.lastSafeCompassAngle; //targetRotation = Quaternion.Euler(0, 0, -this.lastSafeCompassAngle);
						}
					else
						{
						targetAngle = ((this.rotationMode == RotationMode.SimpleHorizontal) ? v.x : v.y) * -this.simpleRotationRange;
						}
					}
				else
					{
					this.lastSafeCompassAngle = 0;
					targetAngle = 0;
					}


				this.extraRotation = CFUtils.SmoothTowardsAngle(this.extraRotation, targetAngle,
					this.rotationSmoothingTime, CFUtils.realDeltaTimeClamped, 0.0001f);

				}

			}

			


		this.BeginSpriteAnim(sprite, skipAnim);

		this.UpdateSpriteAnimation(skipAnim);

		}



	// ----------------------
	MonoBehaviour ISpriteAnimator.GetComponent() { return this; }

	// -------------------------
	void ISpriteAnimator.AddUsedSprites(ISpriteOptimizer optimizer)
		{
		optimizer.AddSprite(this.spriteNeutral.sprite);
		optimizer.AddSprite(this.spriteNeutralPressed.sprite);
		optimizer.AddSprite(this.spriteUp.sprite);
		optimizer.AddSprite(this.spriteUpRight.sprite);
		optimizer.AddSprite(this.spriteRight.sprite);
		optimizer.AddSprite(this.spriteDownRight.sprite);
		optimizer.AddSprite(this.spriteDown.sprite);
		optimizer.AddSprite(this.spriteDownLeft.sprite);
		optimizer.AddSprite(this.spriteLeft.sprite);
		optimizer.AddSprite(this.spriteUpLeft.sprite);
		}


	// ------------------
	void ISpriteAnimator.OnSpriteOptimization(ISpriteOptimizer optimizer)
		{
		this.spriteNeutral.sprite			= optimizer.GetOptimizedSprite(this.spriteNeutral.sprite);
		this.spriteNeutralPressed.sprite	= optimizer.GetOptimizedSprite(this.spriteNeutralPressed.sprite);
		this.spriteUp.sprite				= optimizer.GetOptimizedSprite(this.spriteUp.sprite);
		this.spriteUpRight.sprite			= optimizer.GetOptimizedSprite(this.spriteUpRight.sprite);
		this.spriteRight.sprite				= optimizer.GetOptimizedSprite(this.spriteRight.sprite);
		this.spriteDownRight.sprite			= optimizer.GetOptimizedSprite(this.spriteDownRight.sprite);
		this.spriteDown.sprite				= optimizer.GetOptimizedSprite(this.spriteDown.sprite);
		this.spriteDownLeft.sprite			= optimizer.GetOptimizedSprite(this.spriteDownLeft.sprite);
		this.spriteLeft.sprite				= optimizer.GetOptimizedSprite(this.spriteLeft.sprite);
		this.spriteUpLeft.sprite			= optimizer.GetOptimizedSprite(this.spriteUpLeft.sprite);
		}
	}
}

