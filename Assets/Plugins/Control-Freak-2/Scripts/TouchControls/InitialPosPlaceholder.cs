using UnityEngine;

namespace ControlFreak2
{
[AddComponentMenu(""), RequireComponent(typeof(RectTransform))]
public class InitialPosPlaceholder : MonoBehaviour 
	{
	// Component used to force RectTransform creation as a workaround for Unity bug.
	}
}
