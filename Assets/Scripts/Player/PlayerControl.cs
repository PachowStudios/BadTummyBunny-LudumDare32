using UnityEngine;
using System.Collections;

public sealed class PlayerControl : MonoBehaviour
{
	#region Fields
	private static PlayerControl instance;

	public float gravity = -60f;
	public float walkSpeed = 10f;
	public float jumpHeight = 5f;
	public float groundDamping = 10f;
	public float airDamping = 5f;

	private float horizontalMovement = 0f;
	private bool jump = false;

	private Vector3 velocity;

	private CharacterController2D controller;
	private SpriteRenderer spriteRenderer;
	#endregion

	#region Public Properties
	public static PlayerControl Instance
	{ get { return instance; } }

	public bool IsGrounded
	{ get { return controller.isGrounded; } }

	public Vector3 Velocity
	{ get { return velocity; } }
	#endregion

	#region Internal Properties
	private bool Right
	{ get { return horizontalMovement > 0f; } }

	private bool Left
	{ get { return horizontalMovement < 0f; } }

	private bool FacingRight
	{ get { return transform.localScale.x > 0f; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		controller = GetComponent<CharacterController2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	private void Update()
	{
		GetInput();
	}

	private void LateUpdate()
	{
		GetMovement();
		ApplyMovement();
	}
	#endregion

	#region Internal Update Methods
	private void GetInput()
	{
		horizontalMovement = Input.GetAxis("Horizontal");
		jump = jump || Input.GetButtonDown("Jump") && IsGrounded;
	}

	private void GetMovement()
	{
		if (Right && !FacingRight)
			transform.Flip();
		else if (Left && FacingRight)
			transform.Flip();

		if (jump && IsGrounded)
		{
			Jump(jumpHeight);
			jump = false;
		}
	}

	private void ApplyMovement()
	{
		float smoothedMovement = IsGrounded ? groundDamping : airDamping;

		velocity.x = Mathf.Lerp(velocity.x,
								horizontalMovement * walkSpeed,
								Time.deltaTime * smoothedMovement);
		velocity.y += gravity * Time.deltaTime;
		controller.move(velocity * Time.deltaTime);
		velocity = controller.velocity;

		if (IsGrounded)
			velocity.y = 0f;
	}
	#endregion

	#region Internal Helper Methods
	private void Jump(float height)
	{
		if (height > 0f)
		{
			velocity.y = Mathf.Sqrt(2f * height * -gravity);
		}
	}
	#endregion
}
