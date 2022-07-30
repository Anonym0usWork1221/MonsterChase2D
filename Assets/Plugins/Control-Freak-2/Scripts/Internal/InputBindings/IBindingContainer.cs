// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond


using UnityEngine;
using System.Collections.Generic;

namespace ControlFreak2.Internal
{


// ---------------
public interface IBindingContainer
	{		

	void GetSubBindingDescriptions(
		BindingDescriptionList				descList, 
		Object 								undoObject,
		string 								parentMenuPath);

	bool IsBoundToKey				(KeyCode key, InputRig rig);
	bool IsBoundToAxis			(string axisName, InputRig rig);
	bool IsEmulatingTouches		();
	bool IsEmulatingMousePosition();
	
	}
	

	

// ----------------
public struct BindingDescription
	{
	[System.Flags]
	public enum BindingType 
		{
		Digital			= (1 << 0),
		Axis				= (1 << 1),
		//Direction		= (1 << 2),
		//JoystickState	= (1 << 3),
		//GestureState	= (1 << 4),	
		EmuTouch		= (1 << 5),
		MousePos		= (1 << 6),
		All				= ((1 << 7) - 1)		 
		}

	public BindingType
		type;
	public string				
		name,
		nameFormatted,
		menuPath;
	public InputBindingBase
		binding;
	public InputRig.InputSource
		axisSource;
	public Object
		undoObject;


	}


// -----------------
public class BindingDescriptionList : List<BindingDescription>
	{
	public BindingDescription.BindingType
		typeMask;
	public bool
		addUnusedBindings;
	public int
		axisInputSourceMask;
	public NameFormatter
		menuNameFormatter;

	// ----------------------
	public delegate string NameFormatter(InputBindingBase bind, string name);


	// ---------------------
	public BindingDescriptionList(
		BindingDescription.BindingType	typeMask, 
		bool							addUnusedBindings,
		int								axisInputSourceMask,
		NameFormatter					menuNameFormatter) : base(16)
		{
		this.Setup(typeMask, addUnusedBindings, axisInputSourceMask, menuNameFormatter);
		}


	// --------------------
	public void Setup(
		BindingDescription.BindingType	typeMask, 
		bool							addUnusedBindings,
		int								axisInputSourceMask,
		NameFormatter					menuNameFormatter)
		{
		this.typeMask				= typeMask;
		this.addUnusedBindings		= addUnusedBindings;
		this.menuNameFormatter		= menuNameFormatter;
		this.axisInputSourceMask	= axisInputSourceMask;
		}



	// -----------------
	public void Add(
		InputBindingBase	binding,
		string				name,
		string menuPath, 
		Object undoObject) 
		{
		BindingDescription.BindingType bindingType =
			(binding is AxisBinding) 			? BindingDescription.BindingType.Axis :
			(binding is DigitalBinding) 		? BindingDescription.BindingType.Digital :
			//(binding is DirectionBinding) 		? BindingDescription.BindingType.Direction :
			(binding is EmuTouchBinding) 		? BindingDescription.BindingType.EmuTouch :
			//(binding is JoystickStateBinding) 	? BindingDescription.BindingType.JoystickState :
			(binding is MousePositionBinding)	? BindingDescription.BindingType.MousePos :
			//(binding is TouchGestureStateBinding)		? BindingDescription.BindingType.GestureState :
			//(binding is ScrollDeltaBinding)			? BindingDescription.BindingType.S.GestureState :
			 BindingDescription.BindingType.All;

		//if (bindingType == BindingDescription.BindingType.All)
		//	return;

		string nameFormatted = ((this.menuNameFormatter != null) ? this.menuNameFormatter(binding, name) : name);

		if (((bindingType & this.typeMask) == bindingType) && (bindingType != BindingDescription.BindingType.All))
			{
			BindingDescription desc = new BindingDescription();
	
			desc.type			= bindingType;
			desc.name			= name;
			desc.nameFormatted	= nameFormatted;
			desc.menuPath		= menuPath;
			desc.undoObject		= undoObject;
			desc.binding		= binding;	
			
			this.Add(desc);
			}


		// Format menu path...

		menuPath = menuPath + nameFormatted + "/";

		binding.GetSubBindingDescriptions(this, undoObject, menuPath);
		}		

		

	// ------------------
	public void Add(
		AxisBinding				binding, 
		InputRig.InputSource	sourceType,
		string					name,
		string 					menuPath,
		Object					undoObject) 
		{
		if ((this.typeMask & BindingDescription.BindingType.Axis) == 0)
			return;
			
		if ((this.axisInputSourceMask & (1 << (int)sourceType)) == 0)
			return;

		string nameFormatted = ((this.menuNameFormatter != null) ? this.menuNameFormatter(binding, name) : name);

		BindingDescription desc = new BindingDescription();

		desc.type			= BindingDescription.BindingType.Axis;
		desc.axisSource		= sourceType;
		desc.name			= name;
		desc.nameFormatted	= nameFormatted;
		desc.menuPath		= menuPath;
		desc.undoObject		= undoObject;
		desc.binding		= binding;	
	
		this.Add(desc);			
		}
		
	}


}

//! \endcond
