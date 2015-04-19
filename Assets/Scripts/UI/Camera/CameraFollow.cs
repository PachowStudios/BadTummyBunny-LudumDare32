using UnityEngine;
using System.Collections;

public sealed class CameraFollow : MonoBehaviour
{
	#region Fields
	private static CameraFollow instance;

	public float defaultYOffset = 2f;
	public float minimumY = 0f;
	public float minimumYOffset = 3f;
	public float smoothing = 1f;
	public Transform followTarget;

	private float currentYOffset;
	private bool usePlayerY = false;
	private bool lockX = false;
	private Vector3 targetPosition = new Vector3();
	private Vector3 previousTargetPosition = new Vector3();
	private Vector3 previousPosition;
	#endregion

	#region Public Properties
	public static CameraFollow Instance
	{ get { return instance; } }

	public Vector3 DeltaMovement
	{ get { return transform.position - previousPosition; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		currentYOffset = defaultYOffset;

		targetPosition.y = followTarget.position.y + currentYOffset;
		previousPosition = transform.position;
	}

	private void Update()
	{
		if (Time.deltaTime > 0f)
		{
			previousPosition = transform.position;
			targetPosition.z = transform.position.z;
			previousTargetPosition = targetPosition;

			if (!lockX)
			{
				targetPosition.x = followTarget.position.x;
			}

			if (usePlayerY || followTarget.tag == "Player")
			{
				if (PlayerControl.Instance.IsGrounded ||
					(PlayerControl.Instance.Velocity.y < 0f &&
					 PlayerControl.Instance.transform.position.y + currentYOffset < targetPosition.y))
				{
					targetPosition.y = PlayerControl.Instance.transform.position.y + currentYOffset;
				}
			}
			else
			{
				targetPosition.y = followTarget.position.y + currentYOffset;
			}

			targetPosition.y = Mathf.Max(targetPosition.y, minimumY + minimumYOffset);
			transform.localPosition = Extensions.SuperSmoothLerp(transform.localPosition, previousTargetPosition, targetPosition, Time.deltaTime, smoothing);
		}
	}
	#endregion

	#region Public Methods
	public void FollowObject(Transform target, bool newUsePlayerY, float newYOffset = -1f, bool newLockX = false)
	{
		currentYOffset = newYOffset == -1f ? defaultYOffset : newYOffset;
		usePlayerY = newUsePlayerY;
		lockX = newLockX;
		followTarget = target;
		targetPosition.x = followTarget.position.x;
	}
	#endregion
}
