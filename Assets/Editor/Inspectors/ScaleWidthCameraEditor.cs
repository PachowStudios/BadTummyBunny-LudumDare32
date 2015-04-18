using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(ScaleWidthCamera))]
public class ScaleWidthCameraEditor : Editor
{
	private AnimBool showWorldSpaceUI;

	private ScaleWidthCamera Target
	{
		get { return (ScaleWidthCamera)target; }
	}

	void OnEnable()
	{
		showWorldSpaceUI = new AnimBool(Target.useWorldSpaceUI);
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.LabelField("Current FOV", Target.FOV.ToString());
		EditorGUILayout.Space();

		Target.FOV = Target.defaultFOV = EditorGUILayout.IntField("Default FOV", Target.defaultFOV);

		showWorldSpaceUI.target = EditorGUILayout.Toggle("Use World Space UI", showWorldSpaceUI.target);
		Target.useWorldSpaceUI = showWorldSpaceUI.value;

		if (EditorGUILayout.BeginFadeGroup(showWorldSpaceUI.faded))
		{
			EditorGUI.indentLevel++;

			Target.worldSpaceUI = (RectTransform)EditorGUILayout.ObjectField("World Space UI", Target.worldSpaceUI, typeof(RectTransform), true);

			if (Target.worldSpaceUI == null)
			{
				EditorGUILayout.HelpBox("No world space UI selected!", MessageType.Error);
			}

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(Target);
		}

		serializedObject.ApplyModifiedProperties();
		Repaint();
	}
}
