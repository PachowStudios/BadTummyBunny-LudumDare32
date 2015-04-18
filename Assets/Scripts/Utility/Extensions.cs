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

	#region Vector3
	public static Quaternion LookAt2D(this Vector3 parent, Vector3 target)
	{
		Vector3 targetPosition = target - parent;
		float angle = Mathf.Atan2(targetPosition.y, targetPosition.x) * Mathf.Rad2Deg;

		return Quaternion.Euler(new Vector3(0f, 0f, Quaternion.AngleAxis(angle, Vector3.forward).eulerAngles.z));
	}

	public static Vector3 DirectionToRotation2D(this Vector3 parent)
	{
		float angle = Mathf.Atan2(parent.y, parent.x) * Mathf.Rad2Deg;

		return Quaternion.AngleAxis(angle, Vector3.forward).eulerAngles;
	}

	public static float DistanceFrom(this Vector3 parent, Vector3 target)
	{
		return Mathf.Sqrt(Mathf.Pow(parent.x - target.x, 2) + Mathf.Pow(parent.y - target.y, 2));
	}
	#endregion

	#region Transform
	public static void Flip(this Transform parent)
	{
		parent.localScale = new Vector3(-parent.localScale.x, parent.localScale.y, parent.localScale.z);
	}

	public static void CorrectScaleForRotation(this Transform parent, Vector3 target, bool correctY = false)
	{
		bool flipY = target.z > 90f && target.z < 270f;

		target.y = correctY && flipY ? 180f : 0f;

		Vector3 newScale = parent.localScale;
		newScale.x = 1f;
		newScale.y = flipY ? -1f : 1f;
		parent.localScale = newScale;
		parent.rotation = Quaternion.Euler(target);
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
