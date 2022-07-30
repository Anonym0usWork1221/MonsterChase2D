using UnityEngine;


namespace ControlFreak2.DebugUtils
{
[ExecuteInEditMode]
public class GamepadHardwareTester : MonoBehaviour 
	{
	public GUISkin skin;
	
	public const int MAX_JOYSTICKS = 4;
	public const int MAX_JOYSTICK_AXES = 10;
		
	public const int MAX_JOYSTICK_KEYS = 20;

	// ----------------------
	static public string GetJoyAxisName(int joyId, int axisId)
		{
		return ("cfJ" + joyId + "" + axisId);
		}
		
		
	// --------------------
	static public KeyCode GetJoyKeyCode(int joyId, int keyId)
		{
		KeyCode keyCode = KeyCode.Joystick1Button0;

		switch (joyId)
			{
			case 0 : keyCode = KeyCode.Joystick1Button0; break; 
			case 1 : keyCode = KeyCode.Joystick2Button0; break; 
			case 2 : keyCode = KeyCode.Joystick3Button0; break; 
			case 3 : keyCode = KeyCode.Joystick4Button0; break;
			default:
				return KeyCode.None; 
			}

		if ((keyId < 0) || (keyId >= MAX_JOYSTICK_KEYS))
			return KeyCode.None;

		return keyCode + keyId;		
		}


	


	const float 
		AXIS_DEADZONE = 0.05f;	

	const int	
		AXIS_COUNT = 8;
	const int
			KEY_COUNT = 20;
	


	// -----------------
	private string GetAxisCode(int axis, bool positiveSide)
		{
		return ("Axis" + axis + (positiveSide ? "+":"-"));
		}
	

	private string GetAxisName(int axis, bool positiveSide)
		{
		return ("Axis[" + axis + "] " + (positiveSide ? "+" : "-"));	

		}
	
	
	private float GetAxisState(int axis)
		{
		return Input.GetAxis("JoyAxis" + axis);
		}



	// -------------
	private string GetKeyCode(int key)
		{
		return ("Key"+key);
		}

	private bool GetKeyState(int key)
		{
		return Input.GetKey(KeyCode.Joystick1Button0 + key);
		}


	// ------------
	void OnGUI()
		{
		GUI.skin = this.skin;

		string[] joyNames = Input.GetJoystickNames();

		for (int joy = 0; joy < 4; ++joy)
			{
			string msg = "";
	
			if ((joyNames == null) || (joyNames.Length <= joy))
				{
				msg = "No joystick connected!";
				}
			else
				{
		
				// Check axes...
	
				for (int i = 0; i < MAX_JOYSTICK_AXES; ++i)
					{
					float v = Input.GetAxisRaw(GetJoyAxisName(joy, i));
					if (Mathf.Abs(v) <= AXIS_DEADZONE)			
						continue;
	
					msg += GetAxisName(i, v>0) + "   [ "  + v.ToString("0.00") + "]\n"; 
					}
	
				// Check keys...
	
				for (int i = 0; i < MAX_JOYSTICK_KEYS; ++i)
					{
					if (!Input.GetKey(GetJoyKeyCode(joy, i)))
						continue;
	
					msg += "Key" + i + "\n"; 
					}
	
	
				if (msg == "")
					msg = "Nothing pressed";
	
				msg = joy.ToString() + ":[" + joyNames[joy] + "]\n\n" + msg;
				}

			
			float boxWidth = ((Screen.width - 20) / 4);

			GUI.Box(new Rect(10 + (joy * boxWidth) + 5, 10, boxWidth - 5, Screen.height-130), msg);
			}
		

		string keyMsg = "";
		if (Input.GetKey(KeyCode.Escape)) keyMsg += "Escape ";
		if (Input.GetKey(KeyCode.Return)) keyMsg += "Enter ";
		if (Input.GetKey(KeyCode.LeftShift)) keyMsg += "LShift ";
		if (Input.GetKey(KeyCode.RightShift)) keyMsg += "RShift ";
		if (Input.GetKey(KeyCode.LeftControl)) keyMsg += "LCtrl ";
		if (Input.GetKey(KeyCode.RightControl)) keyMsg += "RCtrl ";
		if (Input.GetKey(KeyCode.Space)) keyMsg += "Space ";

		GUI.Box(new Rect(10, Screen.height - 100, Screen.width - 20, 50), keyMsg);


		if (GUI.Button (new Rect(10, Screen.height-40, 100, 30), "Reset Axes"))
			Input.ResetInputAxes();
		if (GUI.Button (new Rect(10+100+5, Screen.height-40, 100, 30), "Exit"))
			Application.Quit();

		GUI.Box(new Rect(220, Screen.height-40,  Screen.width-230, 30), "Platform: " + Application.platform.ToString() + "  Unity Ver [" + Application.unityVersion + "]");
		}

	}
}
