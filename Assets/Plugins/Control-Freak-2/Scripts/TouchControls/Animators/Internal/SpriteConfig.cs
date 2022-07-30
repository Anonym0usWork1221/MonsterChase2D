// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2.Internal
{
[System.Serializable]
public class SpriteConfig
	{
	public bool		enabled;
	public Sprite	sprite;
	public Color	color;
	public float	scale;
	public float	rotation;
	public Vector2	offset;

	public bool
		resetOffset,
		resetRotation,
		resetScale;

	[System.NonSerialized]
	public bool 	
		oneShotState;
	public float	
		duration,
		baseTransitionTime,
		colorTransitionFactor,	
		scaleTransitionFactor,	
		rotationTransitionFactor,
		offsetTransitionFactor;

		
	// -------------------
	public SpriteConfig()
		{
		this.enabled	= true;
		this.color		= Color.white;
		this.rotation	= 0;
		this.scale		= 1;
		this.duration					= 0.1f;
		this.baseTransitionTime			= 0.1f;
		this.colorTransitionFactor		= 1.0f;	
		this.scaleTransitionFactor		= 1.0f;	
		this.rotationTransitionFactor	= 1.0f;	
		this.offsetTransitionFactor		= 1.0f;	
		this.oneShotState				= false;
		}


	// --------------------
	public SpriteConfig(Sprite sprite, Color color) : this()
		{
		this.sprite		= sprite;
		this.color		= color;
		}


	// -----------------------
	public SpriteConfig(bool enabled, bool oneShot, float scale) : this()
		{	
		this.scale			= scale;
		this.enabled		= enabled;
		this.oneShotState	= oneShot;
		this.resetOffset	= oneShot;
		this.resetScale		= oneShot;
		this.resetRotation	= oneShot;
		}

		


	}
}

//! \endcond
