using UnityEditor;
using UnityEngine;

namespace Kiryonn.UnityAttributes.Editor
{
	[CustomPropertyDrawer(typeof(DisableInInspector))]
	public class DisableInInspectorPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false; // Disable editing
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = true; // Re-enable GUI after drawing
		}
	}
}
