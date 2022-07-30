// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------


//! \cond


#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 
	#define UNITY_PRE_5_0
#endif

#if UNITY_PRE_5_0 || UNITY_5_0 
	#define UNITY_PRE_5_1
#endif

#if UNITY_PRE_5_1 || UNITY_5_1 
	#define UNITY_PRE_5_2
#endif


using UnityEngine;
using UnityEngine.UI;


namespace ControlFreak2.Internal
{
[ExecuteInEditMode()]
public abstract class TouchControlSpriteAnimatorBase : TouchControlAnimatorBase 
	{
	protected RectTransform
		rectTr;
	protected Image 
		image;
	protected CanvasGroup 
		canvasGroup;
	
	protected Vector3
		initialTransl,
		initialScale;
	protected Quaternion 
		initialRotation;

	public SpriteConfig
		spriteNeutral;


	[System.NonSerialized]
	private SpriteConfig
		curSprite,
		nextSprite;

	private float
		spriteAnimElapsed;


	protected Vector2
		animOffsetStart,
		animOffsetCur;
	protected Vector2 
		animScaleStart,
		animScaleCur;
	protected float
		animRotationStart,
		animRotationCur;	
	protected Color
		animColorStart,
		animColorCur;
		

	protected Vector2 
		extraOffset,
		extraScale;
	protected float
		extraRotation;






	// --------------------
	public TouchControlSpriteAnimatorBase(System.Type sourceType) : base(sourceType)
		{
		this.spriteNeutral = new SpriteConfig();
		this.spriteNeutral.color = new Color(1, 1, 1, 0.33f);

		}

	
	// ------------------
	override protected void OnInitComponent()
		{
		base.OnInitComponent();
			

		// Get Rect Transform...
	
		this.rectTr = this.GetComponent<RectTransform>();


		// Get or add Image component...			

		if (this.image == null)
			{
			this.image = this.GetComponent<UnityEngine.UI.Image>();
			if (this.image == null)
				{
				this.image = this.gameObject.AddComponent<Image>();			
				}
			}

#if !UNITY_PRE_5_2
		if (this.image != null)
			{
			this.image.raycastTarget = false;
			}
#endif


		// Get the Canvas Group...

		this.canvasGroup = this.gameObject.GetComponent<CanvasGroup>();

		// Get Initial Transforms...

		Transform tr = this.transform;
	
		this.initialTransl		= tr.localPosition;
		this.initialScale		= tr.localScale;
		this.initialRotation	= tr.localRotation;

		this.animOffsetCur		= this.animOffsetStart		= this.extraOffset	= Vector2.zero;
		this.animScaleCur		= this.animScaleStart		= this.extraScale	= Vector2.one;
		this.animRotationCur	= this.animRotationStart	= this.extraRotation = 0;
		this.animColorStart		= this.animColorCur			= Color.white;

		
		this.BeginSpriteAnim(this.spriteNeutral, true, true);
		}



	// -----------------
	protected override void OnDisableComponent ()
		{
		if (!CFUtils.editorStopped)
			{
			this.transform.localPosition	= this.initialTransl;
			this.transform.localScale		= this.initialScale;
			this.transform.localRotation	= this.initialRotation;
			}

		base.OnDisableComponent ();
		}



	// -----------------
	protected void BeginSpriteAnim(SpriteConfig spriteConfig, bool skipAnim, bool forceStart = false)
		{
		if ((this.curSprite == spriteConfig) && !spriteConfig.oneShotState && !skipAnim)
			return;

		if ((this.curSprite != null) && this.curSprite.oneShotState && !skipAnim && !forceStart)
			{
			this.nextSprite = spriteConfig;
			return;
			}


		this.curSprite = spriteConfig;	
		this.spriteAnimElapsed = 0;
		
		if (!skipAnim)
			{
			this.animColorStart	= this.animColorCur;
			}
		else	
			{
			this.animColorStart	= this.animColorCur	= spriteConfig.color;
			}

		
		if (CFUtils.editorStopped)	
			return;

		if (!skipAnim)
			{
			this.animOffsetStart	= this.animOffsetCur;
			this.animScaleStart		= this.animScaleCur;
			this.animRotationStart	= this.animRotationCur;

			if (this.curSprite.resetScale)
				this.animScaleStart = Vector2.one;
			if (this.curSprite.resetOffset)
				this.animOffsetStart = Vector2.zero;
			if (this.curSprite.resetRotation)
				this.animRotationStart = 0;

			}
		else	
			{
			this.animOffsetStart	= this.animOffsetCur	= spriteConfig.offset;
			this.animScaleStart		= this.animScaleCur 	= Vector2.one * spriteConfig.scale;
			this.animRotationStart	= this.animRotationCur	= spriteConfig.rotation;

			this.ApplySpriteAnimation();
			}
		}



	// --------------------
	protected void UpdateSpriteAnimation(bool skipAnim)
		{
		this.spriteAnimElapsed += CFUtils.realDeltaTimeClamped;
	
		if (this.curSprite.oneShotState && (this.spriteAnimElapsed >= this.curSprite.duration))
			this.BeginSpriteAnim((this.nextSprite != null) ? this.nextSprite : this.spriteNeutral, skipAnim, true);


		this.animColorCur = Color.Lerp(this.animColorStart, this.curSprite.color, 
			this.GetAnimLerpFactor(skipAnim ? 0 : (this.curSprite.colorTransitionFactor * this.curSprite.baseTransitionTime)));



		// Scale alpha...

		this.animColorCur = CFUtils.ScaleColorAlpha(this.animColorCur, this.sourceControl.GetAlpha());


		// Animate transforms...

		if (!CFUtils.editorStopped && !this.IsIllegallyAttachedToSource())
			{
			this.animOffsetCur = Vector2.Lerp(this.animOffsetStart, this.curSprite.offset, 
				this.GetAnimLerpFactor(skipAnim ? 0 : (this.curSprite.offsetTransitionFactor * this.curSprite.baseTransitionTime)));
	
			this.animScaleCur = Vector2.Lerp(this.animScaleStart, Vector2.one * this.curSprite.scale, 
				this.GetAnimLerpFactor(skipAnim ? 0 : (this.curSprite.scaleTransitionFactor * this.curSprite.baseTransitionTime)));
	
			this.animRotationCur = Mathf.LerpAngle(this.animRotationStart, this.curSprite.rotation, 
				this.GetAnimLerpFactor(skipAnim ? 0 : (this.curSprite.rotationTransitionFactor * this.curSprite.baseTransitionTime)));
	
			}


		this.ApplySpriteAnimation();
		}


	// -------------------
	public void ApplySpriteAnimation()
		{
		if ((this.animColorCur.a > 0.00001f) != this.image.enabled)
			this.image.enabled = !this.image.enabled;

		Color c = this.animColorCur;

		if (this.canvasGroup != null)	
			{
			this.canvasGroup.alpha = this.animColorCur.a;
			c.a = 1.0f;
			}


		this.image.color = c;
		this.image.sprite = (this.curSprite.sprite == null) ? this.spriteNeutral.sprite : this.curSprite.sprite;

		// Apply transforms...

		if (!CFUtils.editorStopped && !this.IsIllegallyAttachedToSource())
			{
			Rect baseRect = this.sourceControl.GetLocalRect();

			this.transform.localPosition	= this.initialTransl + (Vector3)Vector2.Scale(baseRect.size * 0.5f, (this.animOffsetCur + this.extraOffset));
			this.transform.localRotation	=  Quaternion.Euler(new Vector3(0, 0, (-this.animRotationCur + this.extraRotation))) * this.initialRotation;
			this.transform.localScale		= Vector3.Scale(this.initialScale, Vector2.Scale(this.animScaleCur, this.extraScale));
			}
		}



	// ----------------
	private float GetAnimLerpFactor(float duration)
		{
		if ((this.spriteAnimElapsed > duration) || (duration < 0.00001f))
			return 1;

		return (this.spriteAnimElapsed / duration);
		}

	}
}

//! \endcond
