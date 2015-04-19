using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
	#region Fields
	public float gravity = -35f;
	public float moveSpeed = 5f;
	public float groundDamping = 10f;
	public float airDamping = 5f;

	public float maxHealth = 25f;
	public float damage = 5f;
	public Vector2 knockback = new Vector2(2f, 1f);
	public bool immuneToKnockback = false;

	public Color flashColor = new Color(1f, 0.47f, 0.47f, 1f);
	public float flashLength = 0.25f;

	[SerializeField]
	protected Transform frontCheck;
	[SerializeField]
	protected Transform ledgeCheck;

	protected float health;

	protected Vector3 velocity;
	protected Vector3 lastGroundedPosition;
	protected float horizontalMovement = 0f;

	protected CharacterController2D controller;
	protected Animator animator;
	protected SpriteRenderer spriteRenderer;
	#endregion

	#region Public Properties
	public float Health
	{
		get { return health; }
		set
		{
			health = Mathf.Clamp(value, 0f, maxHealth);
			CheckDeath();
		}
	}

	public bool IsGrounded
	{ get { return controller.isGrounded; } }
	#endregion

	#region Internal Properties
	protected bool Right
	{ get { return horizontalMovement > 0f; } }

	protected bool Left
	{ get { return horizontalMovement < 0f; } }

	protected bool FacingRight
	{ get { return transform.localScale.x > 0f; } }

	protected LayerMask CollisionLayers
	{ get { return controller.platformMask; } }

	protected bool PlayerIsOnRight
	{ get { return PlayerControl.Instance.transform.position.x > transform.position.x; } }

	protected float RelativePlayerLastGrounded
	{ get { return (lastGroundedPosition.y - PlayerControl.Instance.LastGroundedPosition.y).RoundToTenth(); } }

	protected float RelativePlayerHeight
	{ get { return transform.position.y - PlayerControl.Instance.transform.position.y; } }
	#endregion

	#region MonoBehaviour
	protected virtual void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();

		health = maxHealth;
	}

	protected virtual void Update()
	{
		CalculateAI();
		ApplyAnimation();
	}

	protected virtual void LateUpdate()
	{
		GetMovement();
		ApplyMovement();
	}

	protected virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (Health > 0f && other.tag == "Player" && PlayerHealth.Instance.DamagesOnTouch)
			TakeDamageFromPlayer();
	}

	protected virtual void OnTriggerStay2D(Collider2D other)
	{
		if (other.tag == "Killzone")
			KillNoPoints();
	}
	#endregion

	#region Abstract Internal AI Methods
	protected abstract void CalculateAI();
	protected abstract void ApplyAnimation();
	#endregion

	#region Internal Update Methods
	protected void GetMovement()
	{
		if (Right && !FacingRight)
			transform.Flip();
		else if (Left && FacingRight)
			transform.Flip();
	}

	protected void ApplyMovement()
	{
		float smoothedMovement = IsGrounded ? groundDamping : airDamping;

		velocity.x = Mathf.Lerp(velocity.x,
								horizontalMovement * moveSpeed,
								smoothedMovement * Time.deltaTime);
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
	protected virtual void Jump(float height)
	{
		if (height > 0f)
			velocity.y = Mathf.Sqrt(2f * height * -gravity);
	}

	protected bool CheckAtWall(bool flip = false)
	{
		Collider2D collision = Physics2D.OverlapPoint(frontCheck.position, CollisionLayers);
		bool atWall = collision != null;

		if (atWall && flip)
		{
			horizontalMovement *= -1f;

			if (horizontalMovement == 0f)
				horizontalMovement = 1f;
		}

		return atWall;
	}

	protected bool CheckAtLedge(bool flip = false)
	{
		if (!IsGrounded)
			return false;

		Collider2D collision = Physics2D.OverlapPoint(ledgeCheck.position, CollisionLayers);
		bool atLedge = collision == null;

		if (atLedge && flip)
		{
			horizontalMovement *= -1f;

			if (horizontalMovement == 0f)
				horizontalMovement = 1f;
		}

		return atLedge;
	}

	protected bool IsPlayerInRange(float min, float max)
	{
		int direction = FacingRight ? 1 : -1;
		Vector3 startPoint = new Vector3(transform.position.x + (min * direction), collider2D.bounds.center.y, 0f);
		Vector3 endPoint = startPoint + new Vector3((max - min) * direction, 0f, 0f);
		RaycastHit2D linecast = Physics2D.Linecast(startPoint, endPoint, LayerMask.GetMask("Player"));

		return linecast.collider != null;
	}

	protected bool IsPlayerVisible(float range = Mathf.Infinity)
	{
		RaycastHit2D linecast = Physics2D.Linecast(collider2D.bounds.center,
												   PlayerControl.Instance.collider2D.bounds.center,
												   CollisionLayers);

		return linecast.collider == null && linecast.distance <= range;
	}

	protected void CheckDeath()
	{
		if (Health <= 0f)
		{
			//ExplodeEffect

			Destroy(gameObject);
		}
	}

	protected IEnumerator ResetColor(float delay = 0f)
	{
		yield return new WaitForSeconds(delay);

		spriteRenderer.color = Color.white;
	}
	#endregion

	#region Public Methods
	public void KillNoPoints()
	{
		//ExplodeEffect
		Destroy(gameObject);
	}

	public void TakeDamageFromPlayer()
	{
		if (Health <= 0f)
			return;

		float damage = PlayerHealth.Instance.Damage;
		Vector2 knockback = PlayerHealth.Instance.Knockback;
		Vector2 knockbackDirection = PlayerControl.Instance.Direction;

		if (damage != 0f)
		{
			Health -= damage;

			if (Health > 0f)
			{
				ApplyKnockback(knockback, knockbackDirection);
				spriteRenderer.color = flashColor;
				StartCoroutine(ResetColor(flashLength));
			}
		}
	}

	public void ApplyKnockback(Vector2 knockback, Vector2 knockbackDirection)
	{
		if (immuneToKnockback)
			return;

		knockback.x += Mathf.Sqrt(Mathf.Abs(Mathf.Pow(knockback.x, 2) * -gravity));
		knockback.y += Mathf.Sqrt(Mathf.Abs(knockback.y * -gravity));
		knockback.Scale(knockbackDirection);

		if (knockback.x != 0f || knockback.y != 0f)
		{
			velocity += (Vector3)knockback;
			controller.move(velocity * Time.deltaTime);
			velocity = controller.velocity;
		}
	}
	#endregion
}
