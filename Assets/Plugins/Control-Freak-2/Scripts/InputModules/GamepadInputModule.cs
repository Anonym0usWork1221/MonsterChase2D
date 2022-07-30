// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;
using UnityEngine.EventSystems;

using ControlFreak2.Internal;

namespace ControlFreak2
{
public class GamepadInputModule : UnityEngine.EventSystems.BaseInputModule
	{
	public GamepadManager.GamepadKey
		submitGamepadButton		= GamepadManager.GamepadKey.FaceBottom,
		submitGamepadButtonAlt	= GamepadManager.GamepadKey.Start,
		cancelGamepadButton		= GamepadManager.GamepadKey.FaceRight,
		cancelGamepadButtonAlt	= GamepadManager.GamepadKey.Select;

	// -------------
	public override bool IsModuleSupported()
        {
			return true;

		//GamepadManager g = GamepadManager.activeManager;
		//return (g != null);
        }
		

	// ------------------
	private bool JustPressedSubmitButton()
		{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
			return true;

		GamepadManager g = GamepadManager.activeManager;
		if ((g != null) && (g.GetCombinedGamepad().GetKeyDown(this.submitGamepadButton) || g.GetCombinedGamepad().GetKeyDown(this.submitGamepadButtonAlt))) 
			return true;

		return false;
		}

	// ------------------
	private bool JustPressedCancelButton()
		{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
			return true;

		GamepadManager g = GamepadManager.activeManager;
		if ((g != null) && (g.GetCombinedGamepad().GetKeyDown(this.cancelGamepadButton) || g.GetCombinedGamepad().GetKeyDown(this.cancelGamepadButtonAlt))) 
			return true;

		return false;
		}
		
	// -------------------
	private UnityEngine.EventSystems.MoveDirection JustPressedDirectionKey()
		{
		bool 
			keyLeft		= UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow),
			keyRight	= UnityEngine.Input.GetKeyDown(KeyCode.RightArrow),
			keyUp		= UnityEngine.Input.GetKeyDown(KeyCode.UpArrow),
			keyDown 	= UnityEngine.Input.GetKeyDown(KeyCode.DownArrow);

		GamepadManager g = GamepadManager.activeManager;
		if (g != null)
			{
			keyLeft 	|= (g.GetCombinedGamepad().leftStick.state.JustPressedDir4(Dir.L) || g.GetCombinedGamepad().dpad.state.JustPressedDir4(Dir.L));
			keyRight 	|= (g.GetCombinedGamepad().leftStick.state.JustPressedDir4(Dir.R) || g.GetCombinedGamepad().dpad.state.JustPressedDir4(Dir.R));
			keyUp 		|= (g.GetCombinedGamepad().leftStick.state.JustPressedDir4(Dir.U) || g.GetCombinedGamepad().dpad.state.JustPressedDir4(Dir.U));
			keyDown 	|= (g.GetCombinedGamepad().leftStick.state.JustPressedDir4(Dir.D) || g.GetCombinedGamepad().dpad.state.JustPressedDir4(Dir.D));
			}

		if (keyLeft && keyRight)
			keyLeft = keyRight = false;

		if (keyUp && keyDown)
			keyUp = keyDown = false;
			
		return (
			keyUp	 ? UnityEngine.EventSystems.MoveDirection.Up :
			keyDown  ? UnityEngine.EventSystems.MoveDirection.Down :
			keyLeft  ? UnityEngine.EventSystems.MoveDirection.Left :
			keyRight ? UnityEngine.EventSystems.MoveDirection.Right :
					   UnityEngine.EventSystems.MoveDirection.None);
		}


	// --------------------
	



	// --------------
	public override bool ShouldActivateModule()
        {
		if (!base.ShouldActivateModule())
			return false;
			
		return (this.JustPressedCancelButton() || this.JustPressedSubmitButton() || (this.JustPressedDirectionKey() != MoveDirection.None));
		}

	// ------------------
    public override void ActivateModule()
    	{
      base.ActivateModule();
			
	 	GameObject toSelect = eventSystem.currentSelectedGameObject;

		if (toSelect != null)
			{
			this.eventSystem.SetSelectedGameObject(null, this.GetBaseEventData());
			this.eventSystem.SetSelectedGameObject(toSelect, this.GetBaseEventData());

			this.SendMoveEventToSelectedObject();
			}
		else
			{				
	
	        if (toSelect == null)
	            toSelect = eventSystem.firstSelectedGameObject;
			else
				toSelect = this.FindFirstSelectableInScene();
	
	        if (toSelect != null)
				{
				this.eventSystem.SetSelectedGameObject(null, this.GetBaseEventData());
				this.eventSystem.SetSelectedGameObject(toSelect, this.GetBaseEventData());
				}
			}
		}
		
		

	// -----------------
	private GameObject FindFirstSelectableInScene()
		{
		UnityEngine.UI.Selectable topMost = null;

		UnityEngine.UI.Selectable[] selectableList = (UnityEngine.UI.Selectable[])GameObject.FindObjectsOfType(typeof(UnityEngine.UI.Selectable));

		for (int i = 0; i < selectableList.Length; ++i)
			{
			UnityEngine.UI.Selectable s = selectableList[i];
			if (s.navigation.mode == UnityEngine.UI.Navigation.Mode.None)
				continue;

			topMost = s;		// TODO : compare
			}

		return ((topMost != null) ? topMost.gameObject : null);
		}

	// ----------------
	public override void DeactivateModule()
        {
        base.DeactivateModule();
        }

		
	// ----------------
	public override void Process()
        {
		bool usedEvent = SendUpdateEventToSelectedObject();

        if (eventSystem.sendNavigationEvents)
	        {
            if (!usedEvent)
                usedEvent |= SendMoveEventToSelectedObject();

            if (!usedEvent)
                SendSubmitEventToSelectedObject();
    	    }

        //ProcessMouseEvent();
        }


	// ------------
	protected bool SendSubmitEventToSelectedObject()
    	{
        if (eventSystem.currentSelectedGameObject == null)
            return false;
			
		GamepadManager g = GamepadManager.activeManager;
		if (g == null)
			return false;
		
		BaseEventData data = this.GetBaseEventData();

        if (this.JustPressedSubmitButton())
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
			
		if (this.JustPressedCancelButton())
			ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);

        return data.used;
        }

	
	// ---------------	
	protected bool SendMoveEventToSelectedObject()
        {	
		GamepadManager g = GamepadManager.activeManager;
		if (g == null)
			return false;

		JoystickState 
			leftStick	= g.GetCombinedGamepad().GetStick(GamepadManager.GamepadStick.LeftAnalog),
			dpad		= g.GetCombinedGamepad().GetStick(GamepadManager.GamepadStick.Dpad);
			
		Dir dir = Dir.N;
		
		if (leftStick.JustReleasedDir4(Dir.N))
			dir = leftStick.GetDir4();
		else if (dpad.JustReleasedDir4(Dir.N))
			dir = dpad.GetDir4(); 
		
		Vector2 dirVec = CFUtils.DirToVector(dir, false);
			
		Vector2 keyboardVec = new Vector2(
			(UnityEngine.Input.GetKeyDown(KeyCode.RightArrow)	? 1.0f : UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) ? -1.0f : 0),
			(UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) 		? 1.0f : UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) ? -1.0f : 0));


		dirVec.x = CFUtils.ApplyDeltaInput(dirVec.x, keyboardVec.x);
		dirVec.y = CFUtils.ApplyDeltaInput(dirVec.y, keyboardVec.y);
	
		if (dirVec.sqrMagnitude < 0.00001f)
			return false;
		

		var axisEventData = GetAxisEventData(dirVec.x, dirVec.y, 0.3f);

		ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
			
		if ((this.eventSystem.currentSelectedGameObject != null))
			{
			this.eventSystem.firstSelectedGameObject = this.eventSystem.currentSelectedGameObject;
			}
			
		return axisEventData.used;
		}


		
	// ---------------------
	protected bool SendUpdateEventToSelectedObject()
        {
		if (eventSystem.currentSelectedGameObject == null)
			return false;
		
		var data = GetBaseEventData();
		ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
		return data.used;
		}


	}
}

//! \endcond
