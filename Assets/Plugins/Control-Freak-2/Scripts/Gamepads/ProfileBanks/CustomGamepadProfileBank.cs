// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace ControlFreak2
{

public class CustomGamepadProfileBank //: MonoBehaviour 
	{
	// ------------------
	public List<GamepadProfile>
		profileList;
		

	// --------------------
	public CustomGamepadProfileBank() : base()
		{
		this.profileList = new List<GamepadProfile>(16);
		}

	

	// --------------------
	public int FindDuplicate(GamepadProfile profile)
		{
		for (int i = 0; i < this.profileList.Count; ++i)
			{
			if (this.profileList[i].IsDuplicateOf(profile))
				return i;
			} 

		return -1;
		}
	
	// -------------------
	public GamepadProfile GetProfileById(int i)
		{
		if ((i < 0) || (i >= this.profileList.Count))
			return null;

		return this.profileList[i];
		}

		


	// -------------------
	public GamepadProfile GetProfile(string deviceName)
		{
		for (int i = 0; i < this.profileList.Count; ++i)
			{
			GamepadProfile profile = this.profileList[i];
			if (profile.IsCompatible(deviceName))
				return profile;
			}

		return null;
		}


	// -------------------
	public void GetCompatibleProfiles(string deviceName, List<GamepadProfile> targetList)
		{
		for (int i = 0; i < this.profileList.Count; ++i)
			{
			GamepadProfile profile = this.profileList[i];
			if (profile.IsCompatible(deviceName))
				{
				if (!targetList.Contains(profile))
					targetList.Add(profile);
				}
			}		
		}



	// -------------------
	public GamepadProfile AddProfile(GamepadProfile profile)
		{
		if (profile == null)
			return null;

		int duplicateId = this.FindDuplicate(profile);
		if (duplicateId >= 0)
			{
			GamepadProfile originalProfile = this.profileList[duplicateId];
			this.profileList.RemoveAt(duplicateId);
			this.profileList.Insert(0, originalProfile);

			originalProfile.AddSupportedVersion(profile.unityVerTo);

			return originalProfile;
			}
			
		this.profileList.Insert(0, profile);

		return profile;
		}
		


	// -------------------
	public void Load()
		{
		this.LoadFromPlayerPrefs();
		}

	// --------------------
	public void Save()
		{
		this.SaveToPlayerPrefs();
		}


	public const string 
		GAMEPAD_PROFILE_LIST_KEY = "CF2GamepadProfiles";

	// -------------------
	private bool LoadFromPlayerPrefs()
		{
		string profileStr = PlayerPrefs.GetString(GAMEPAD_PROFILE_LIST_KEY, null);
		if ((profileStr == null) || (profileStr.Length == 0))
			return false;

		TextReader textReader = new StringReader(profileStr);
		System.Xml.XmlTextReader src = new System.Xml.XmlTextReader(textReader);
		return this.LoadFromStream(src);
		}

	// -----------------
	private bool SaveToPlayerPrefs()
		{
		TextWriter textWriter = new StringWriter();
		System.Xml.XmlTextWriter stream = new System.Xml.XmlTextWriter(textWriter);

		if (!this.SaveToStream(stream))
			return false;

			
		string str = textWriter.ToString();

//#if UNITY_EDITOR
//		Debug.Log(str);
//		Debug.Log("String len:" + str.Length);
//#endif
			
		try {
			PlayerPrefs.SetString(GAMEPAD_PROFILE_LIST_KEY, str);
			}
		catch (System.Exception e)	
			{
#if UNITY_EDITOR
			Debug.LogError("Error while saving to plater prefs!\n" + e.Message);
#endif
			return false;
			}	

		return true;
		}


	// ------------------
	private XmlSerializer CreateSerializer()
		{
		return (new XmlSerializer(typeof(List<GamepadProfile>), new System.Type[]{ typeof(GamepadProfile), 
				typeof(GamepadProfile.JoystickSource), typeof(GamepadProfile.KeySource) }));
		}

	// -----------------
	private bool SaveToStream(System.Xml.XmlTextWriter stream)
		{
		if (stream == null)
			return false;
			
		try {
			XmlSerializer xml = this.CreateSerializer();
			xml.Serialize(stream, this.profileList);
			}
		catch (System.Exception e)
			{
#if UNITY_EDITOR
			Debug.LogError("Error while serializing  profile list!\n" + e.Message);
#endif				
			return false;
			}

		return true;
		}

	// -----------------
	private bool LoadFromStream(System.Xml.XmlTextReader stream)
		{
		if (stream == null)
			return false;

		List<GamepadProfile> loadedList = null;
			
		try {
			XmlSerializer xml = this.CreateSerializer();
			loadedList = (List<GamepadProfile>)xml.Deserialize(stream);
			}
		catch (System.Exception e)
			{
#if UNITY_EDITOR
			Debug.LogError("Error while deserializing  profile list!\n" + e.Message);
#endif				
			return false;
			}

		for (int i = 0; i < loadedList.Count;  ++i)
			{
			// TODO : verify!

			Debug.Log("LoadedProfile[" + i + "] : [" + loadedList[i].name + "] {" + loadedList[i].joystickIdentifier + "}!");
			}

		return true;
		}

/*		
#if UNITY_EDITOR
	[ContextMenu("TEST SAVE gamepads!")]
	private void SaveXXX()
		{
		if (this.profileList.Count == 0)
			{
			for (int i = 0; i < 2; ++i)
				{
				this.AddProfile(BuiltInGamepadProfileBank.GetGenericProfile());
				}		
			}
			
Debug.Log("ProfileCount:"  + this.profileList.Count);

		Debug.Log("Save : " + this.SaveToPlayerPrefs());
		}

	[ContextMenu("TEST Load gamepads!")]
	private void LoadXXX()
		{
			

		Debug.Log("Load: " + this.LoadFromPlayerPrefs());
		}


#endif
*/
	}
}

//! \endcond

