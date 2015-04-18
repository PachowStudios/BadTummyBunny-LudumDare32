using UnityEngine;
using System.Collections;

public static class Extensions
{
	#region Float
	public static int Sign(this float parent)
	{
		return (int)Mathf.Sign(parent);
	}
	#endregion

	#region Transform
	public static void Flip(this Transform parent)
	{
		parent.localScale = new Vector3(-parent.localScale.x, parent.localScale.y, parent.localScale.z);
	}
	#endregion

	#region Utility
	public static int ClampWrap(int value, int min, int max)
	{
		if (value > max)
		{
			value = min;
		}
		else if (value < min)
		{
			value = max;
		}

		return value;
	}

	public static int RandomSign()
	{
		return (Random.value < 0.5) ? -1 : 1;
	}

	public static float ConvertRange(float num, float oldMin, float oldMax, float newMin, float newMax)
	{
		float oldRange = oldMax - oldMin;
		float newRange = newMax - newMin;

		return (((num - oldMin) * newRange) / oldRange) + newMin;
	}

	public static float GetDecimal(float num)
	{
		string result;

		if (num.ToString().Split('.').Length == 2)
		{
			result = "0." + num.ToString().Split('.')[1];
		}
		else
		{
			result = "0";
		}

		return float.Parse(result);
	}

	public static Vector3 SuperSmoothLerp(Vector3 followOld, Vector3 targetOld, Vector3 targetNew, float elapsedTime, float lerpAmount)
	{
		Vector3 f = followOld - targetOld + (targetNew - targetOld) / (lerpAmount * elapsedTime);
		return targetNew - (targetNew - targetOld) / (lerpAmount * elapsedTime) + f * Mathf.Exp(-lerpAmount * elapsedTime);
	}

	public static IEnumerator WaitForRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;

		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}

	public static Vector3 Vector3Range(Vector3 min, Vector3 max)
	{
		return new Vector3(Random.Range(min.x, max.x),
						   Random.Range(min.y, max.y),
						   Random.Range(min.z, max.z));
	}
	#endregion
}
