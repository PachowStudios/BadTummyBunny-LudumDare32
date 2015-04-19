using UnityEngine;
using System.Collections;
using DG.Tweening;

[ExecuteInEditMode]
public sealed class ScaleWidthCamera : MonoBehaviour
{
	#region Fields
	private static ScaleWidthCamera instance;

	public int defaultFOV = 500;
	public bool useWorldSpaceUI = false;
	public RectTransform worldSpaceUI;

	public int FOV;
	#endregion

	#region Public Properties
	public static ScaleWidthCamera Instance
	{ get { return instance; } }
	#endregion

	#region MonoBehaviour
	private void OnEnable()
	{
		instance = this;

		FOV = defaultFOV;
	}

	private void OnPreRender()
	{
		camera.orthographicSize = FOV / 32f / camera.aspect;

		if (useWorldSpaceUI && worldSpaceUI != null)
		{
			worldSpaceUI.sizeDelta = new Vector2(FOV / 16f, FOV / 16f / camera.aspect);
		}
	}
	#endregion

	#region Public Methods
	public void AnimateFOV(int newFOV, float time)
	{
		DOTween.To(() => FOV, x => FOV = x, newFOV, time)
			.SetEase(Ease.OutQuint);
	}
	#endregion
}
