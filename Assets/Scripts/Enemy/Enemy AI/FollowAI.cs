using UnityEngine;
using System.Collections;

public sealed class FollowAI : Enemy
{
	#region Fields
	public Vector2 followSpeedRange = new Vector2(2.5f, 3.5f);
	public float followRange = 5f;
	public float attackRange = 0.5f;
	public float cooldownTime = 1f;

	private float defaultMoveSpeed;
	private float followSpeed;

	private float cooldownTimer = 0f;
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		defaultMoveSpeed = moveSpeed;
		followSpeed = followSpeedRange.RandomRange();
	}
	#endregion

	#region Internal AI Methods
	protected override void CalculateAI()
	{
		Walk();
		CheckAttack();
	}

	protected override void ApplyAnimation()
	{
		//animator.SetBool("Walking", horizontalMovement != 0f);
	}
	#endregion

	#region Internal Update Methods
	private void Walk()
	{
		moveSpeed = defaultMoveSpeed;

		if (horizontalMovement == 0f)
			horizontalMovement = Extensions.RandomSign();

		if (RelativePlayerLastGrounded != 0f)
		{
			CheckAtWall(true);
			CheckAtLedge(true);
		}
		else if (!CheckAtLedge())
		{
			if (RelativePlayerHeight < 0.5f && IsPlayerVisible(followRange))
			{
				FollowPlayer(attackRange);
				moveSpeed = followSpeed;
			}
			else
			{
				CheckAtWall(true);
				CheckAtLedge(true);
			}
		}
		else if (PlayerControl.Instance.IsGrounded)
		{
			CheckAtLedge(true);
		}
		else
		{
			horizontalMovement = 0f;
		}
	}

	private void CheckAttack()
	{
		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime && IsPlayerInRange(0f, attackRange))
		{
			Attack();
			cooldownTimer = 0f;
		}
	}
	#endregion

	#region Internal Helper Methods
	private void FollowPlayer(float range)
	{
		if (transform.position.x + range < PlayerControl.Instance.transform.position.x)
		{
			horizontalMovement = 1f;
		}
		else if (transform.position.x - range > PlayerControl.Instance.transform.position.x)
		{
			horizontalMovement = -1f;
		}
		else
		{
			horizontalMovement = 0f;
			FacePlayer();
		}
	}

	private void FacePlayer()
	{
		if ((PlayerIsOnRight && !FacingRight) || (!PlayerIsOnRight && FacingRight))
			transform.Flip();
	}

	private void Attack()
	{
		//animator.SetTrigger("Attack");
		PlayerHealth.Instance.TakeDamage(this);
	}
	#endregion
}
