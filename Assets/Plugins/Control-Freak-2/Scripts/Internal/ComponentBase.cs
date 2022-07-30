// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ControlFreak2.Internal
{
[AddComponentMenu(""), ExecuteInEditMode()]
public abstract class ComponentBase : MonoBehaviour
	{
	[System.NonSerializedAttribute]
	private bool isReady;
	[System.NonSerializedAttribute]
	private bool isDestroyed;

	public bool IsInitialized		{ get { return this.isReady; } }
	public bool IsDestroyed			{ get { return this.isDestroyed; } }
		
	abstract protected void OnInitComponent();		
	abstract protected void OnDestroyComponent();	
	abstract protected void OnEnableComponent();
	abstract protected void OnDisableComponent();	


	// ---------------------
	public bool CanBeUsed()	
		{
		if (this.isDestroyed)
			return false;
	
		if (!this.isReady)
			{	
			this.isReady = true;	
			this.OnInitComponent();
			}

		return true;
		}


	// -----------------------
	public void Init()
		{
		if (this.isReady)
			return;
			
		this.isReady = true;

		this.OnInitComponent();
		}
	

	// ---------------------
	public void ForceInit()
		{
		this.isReady = true;	
		this.OnInitComponent();
		}

	
	// -----------------------
	// void Awake()		
		// { 
		// this.Init(); 
		// }

		
	// ----------------------
	void OnEnable()		
		{ 
#if UNITY_EDITOR

//		if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
//			this.ForceInit();
//		else
			this.Init(); 
#else
		this.Init(); 
#endif
		this.OnEnableComponent(); 
		}
		
	// -------------------
	void OnDisable()		
		{ 
		this.OnDisableComponent(); 
		}


	// ------------------------
	void OnDestroy()	
		{ 
		this.isDestroyed = true; 
		this.isReady = false;

		this.OnDestroyComponent(); 
		}

	}
}


//! \endcond
