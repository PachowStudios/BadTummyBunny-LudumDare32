using UnityEngine;
using System.Collections;

public sealed class PlayerControl : MonoBehaviour
{
	#region Fields
	private static PlayerControl instance;

	public float gravity = -60f;
	public float walkSpeed = 10f;
	public float jumpHeight = 5f;

	public Vector2 fartDistanceRange;
	public Vector2 fartSpeedRange;

	public float groundDamping = 10f;
	public float airDamping = 5f;
	public float fartDamping = 10f;

	private float horizontalMovement = 0f;
	private bool jump = false;

	private bool fart = false;
	private bool previousFart = false;
	private bool fartStart = false;
	private bool farted = false;

	private float fartDistance = 0f;
	private float fartSpeed = 0f;
	private float fartTime = 0f;
	private Vector2 fartDirection = Vector2.zero;

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

	private Vector3 MouseDirection
	{
		get
		{
			return transform.position.LookAt2D(Camera.main.ScreenToWorldPoint(Input.mousePosition)) * Vector3.right;
		}
	}
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

		previousFart = fart;
		fart = fart || Input.GetButtonDown("Fart") && IsGrounded;
		fartStart = fart && !previousFart;

		if (fartStart)
			fartDirection = MouseDirection;
	}

	private void GetMovement()
	{
		if (!farted)
		{
			if (Right && !FacingRight)
				transform.Flip();
			else if (Left && FacingRight)
				transform.Flip();
		}

		if (jump && IsGrounded)
		{
			Jump(jumpHeight);
			jump = false;
		}

		if (farted && IsGrounded)
			farted = false;

		if (fartStart && IsGrounded)
		{
			farted = true;
			fartDistance = fartDistanceRange.x;
			fartSpeed = fartSpeedRange.x;
			fartTime = fartDistance / fartSpeed;
		}
	}

	private void ApplyMovement()
	{
		if (fart)
		{
			velocity = fartDirection * fartSpeed;
			fartTime -= Time.deltaTime;

			if (fartTime <= 0f)
				fart = false;
		}
		else if (!farted)
		{
			float smoothedMovement = IsGrounded ? groundDamping : airDamping;

			velocity.x = Mathf.Lerp(velocity.x,
									horizontalMovement * walkSpeed,
									Time.deltaTime * smoothedMovement);
		}

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
