// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2
{
abstract public class GameState : MonoBehaviour 
	{
	protected GameState
		parentState,
		subState;
	protected bool
		isRunning;

		
		
	// -------------------
	public bool IsRunning()
		{ return this.isRunning; }


	// --------------------
	virtual protected void OnStartState(GameState parentState)
		{
		this.parentState	= parentState;		
		this.isRunning		= true;
		}


	// -------------------
	virtual protected void OnExitState()			
		{ 
		this.isRunning = false;

		if (this.subState != null) 
			this.subState.OnExitState();

		}

	virtual protected void OnPreSubStateStart(GameState prevState, GameState nextState) {}
	virtual protected void OnPostSubStateStart(GameState prevState, GameState nextState) {}

	virtual protected void OnUpdateState()			
		{ if (this.subState != null) this.subState.OnUpdateState(); }

	virtual protected void OnFixedUpdateState()	
		{ if (this.subState != null) this.subState.OnFixedUpdateState(); }


	// --------------------
	public void StartSubState(GameState state)
		{
		if (this.FindStateInHierarchy(state))
			{
			throw new System.Exception("Gamestate (" + this.name + ") tries to start sub state (" + state.name + ") that's already running!");
			}

		GameState oldState = this.subState;

		this.OnPreSubStateStart(oldState, state);

		if (this.subState != null)
			this.subState.OnExitState();

		if ((this.subState = state) != null)
			this.subState.OnStartState(this);
		
		this.OnPostSubStateStart(oldState, state);
		}

	// -----------------
	protected bool FindStateInHierarchy(GameState state)
		{
		if (state == null)
			return false;

		for (GameState s = this; s != null; s = s.parentState)
			{
			if (s == state)
				return true;
			}
	
		return false;
		}


	// ------------------
	public void EndState()
		{
		if (this.parentState != null)
			this.parentState.EndSubState();		
		}

	// -------------------
	public void EndSubState()
		{	
		this.StartSubState(null);
		}


	// -----------------
	public GameState GetSubState()
		{ return this.subState; }
		
	// -----------------	
	public bool IsSubStateRunning()
		{ return (this.subState != null); }




	}
}

//! \endcond

