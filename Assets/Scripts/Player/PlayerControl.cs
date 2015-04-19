using UnityEngine;
using System.Collections;

public sealed class PlayerControl : MonoBehaviour
{
	#region Fields
	private static PlayerControl instance;

	public float gravity = -60f;
	public float walkSpeed = 10f;
	public float jumpHeight = 5f;

	public float fartMaxAvailableTime = 10f;
	[Range(0f, 1f)]
	public float fartRechargeRate = 0.5f;
	public float fartMaxChargeTime = 3f;
	public Vector2 fartDistanceRange;
	public Vector2 fartSpeedRange;

	public float groundDamping = 10f;
	public float airDamping = 5f;
	public float fartDamping = 10f;

	[SerializeField]
	private Transform body;
	[SerializeField]
	private ParticleSystem fartParticles;

	private Vector3 velocity;
	private Vector3 lastGroundedPosition;
	private float horizontalMovement = 0f;
	private bool jump = false;
	private bool enableInput = true;

	private bool fart = false;
	private bool farted = false;

	private bool fartCharging = false;
	private bool previousFartCharging = false;
	private float fartAvailableTime;
	private float fartChargeTime = 0f;

	private float fartDistance = 0f;
	private float fartSpeed = 0f;
	private float initialFartTime = 0f;
	private float fartTime = 0f;
	private Vector2 fartDirection = Vector2.zero;

	private CharacterController2D controller;
	private Animator animator;
	#endregion

	#region Public Properties
	public static PlayerControl Instance
	{ get { return instance; } }

	public bool Farting
	{ get { return farted; } }

	public float AvailableFartPercent
	{ get { return Mathf.Clamp(fartAvailableTime / fartMaxAvailableTime, 0f, 1f); } }

	public Vector3 Velocity
	{ get { return velocity; } }

	public Vector3 LastGroundedPosition
	{ get { return lastGroundedPosition; } }

	public Vector2 Direction
	{ get { return velocity.normalized; } }

	public bool IsGrounded
	{ get { return controller.isGrounded; } }

	public LayerMask CollisionLayers
	{ get { return controller.platformMask; } }
	#endregion

	#region Internal Properties
	private bool Right
	{ get { return horizontalMovement > 0f; } }

	private bool Left
	{ get { return horizontalMovement < 0f; } }

	private bool FacingRight
	{ get { return body.localScale.x > 0f; } }

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
		animator = GetComponent<Animator>();

		fartAvailableTime = fartMaxAvailableTime;
	}

	private void Update()
	{
		GetInput();
		ApplyAnimation();
	}

	private void LateUpdate()
	{
		GetMovement();
		ApplyMovement();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (farted && initialFartTime - fartTime > 0.05f && CollisionLayers.ContainsLayer(other.gameObject))
			StopFart(!IsGrounded);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		OnTriggerEnter2D(other);
	}
	#endregion

	#region Internal Update Methods
	private void GetInput()
	{
		if (enableInput)
		{
			horizontalMovement = Input.GetAxis("Horizontal");
			jump = jump || Input.GetButtonDown("Jump") && IsGrounded;

			previousFartCharging = fartCharging;
			fartCharging = Input.GetButton("Fart");

			if (fartCharging && fartAvailableTime > 0f)
			{
				fartChargeTime = Mathf.Min(fartChargeTime + Time.deltaTime, fartMaxChargeTime);

				if (fartChargeTime < fartMaxChargeTime)
					fartAvailableTime = Mathf.Max(fartAvailableTime - Time.deltaTime, 0f);
			}
			else if (previousFartCharging)
			{
				Fart(fartChargeTime);
				fartChargeTime = 0f;
			}
			else
			{
				fartChargeTime = 0f;
				fartAvailableTime = Mathf.Min(fartAvailableTime + (Time.deltaTime * fartRechargeRate), fartMaxAvailableTime);
			}
		}
	}

	private void ApplyAnimation()
	{
		animator.SetBool("Walking", horizontalMovement != 0f && !fart);
		animator.SetBool("Grounded", IsGrounded);
		animator.SetBool("Falling", velocity.y < 0f);
	}

	private void GetMovement()
	{
		if (!farted)
		{
			if (Right && !FacingRight)
				body.Flip();
			else if (Left && FacingRight)
				body.Flip();
		}

		if (jump && IsGrounded)
		{
			Jump(jumpHeight);
			jump = false;
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

		if (farted)
		{
			body.CorrectScaleForRotation(velocity.DirectionToRotation2D());
		}
		else
		{
			float smoothedMovement = IsGrounded ? groundDamping : airDamping;

			velocity.x = Mathf.Lerp(velocity.x,
									horizontalMovement * walkSpeed,
									smoothedMovement * Time.deltaTime);
		}

		velocity.y += gravity * Time.deltaTime;
		controller.move(velocity * Time.deltaTime);
		velocity = controller.velocity;

		if (IsGrounded)
		{
			velocity.y = 0f;
			lastGroundedPosition = transform.position;
		}
	}
	#endregion

	#region Internal Helper Methods
	private void Jump(float height)
	{
		if (height > 0f)
		{
			velocity.y = Mathf.Sqrt(2f * height * -gravity);
			animator.SetTrigger("Jump");
		}
	}

	private void Fart(float chargeTime)
	{
		fartDistance = Extensions.ConvertRange(chargeTime, 0f, fartMaxChargeTime, fartDistanceRange.x, fartDistanceRange.y);
		fartSpeed = Extensions.ConvertRange(chargeTime, 0f, fartMaxChargeTime, fartSpeedRange.x, fartSpeedRange.y);
		initialFartTime = fartDistance / fartSpeed;
		fartTime = initialFartTime;
		fartDirection = MouseDirection;
		StartCoroutine(StartFartParticles());
		fart = farted = true;
	}

	private void StopFart(bool killXVelocity = true)
	{
		fart = farted = false;
		velocity.x = killXVelocity ? 0f : velocity.x;
		velocity.y = 0f;
		fartParticles.Stop();
		ResetOrientation();
	}

	private IEnumerator StartFartParticles()
	{
		yield return new WaitForFixedUpdate();

		fartParticles.Play();
	}

	private void ResetOrientation()
	{
		float zRotation = body.rotation.eulerAngles.z;
		bool flipX = zRotation > 90f && zRotation < 270f;
		body.localScale = new Vector3(flipX ? -1f : 1f, 1f, 1f);
		body.rotation = Quaternion.identity;
	}

	private void ResetInput()
	{
		horizontalMovement = 0f;
		jump = false;
		StopFart();
	}
	#endregion

	#region Public Methods
	public IEnumerator ApplyKnockback(Vector2 knockback, float knockbackDirection)
	{
		yield return new WaitForSeconds(0.1f);

		velocity.x = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(knockback.x, 2) * -gravity)) * knockbackDirection;

		if (IsGrounded)
			velocity.y = Mathf.Sqrt(knockback.y * -gravity);

		controller.move(velocity * Time.deltaTime);
		velocity = controller.velocity;
	}

	public void DisableInput()
	{
		enableInput = false;
		ResetInput();
	}
	#endregion
}
