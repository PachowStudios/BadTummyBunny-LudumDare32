using UnityEngine;
using System.Collections;

public sealed class PlayerControl : MonoBehaviour
{
	#region Fields
	private static PlayerControl instance;

	public float speed = 30f;
	public float maxSpeed = 5f;
	public float jumpForce = 5f;
	public float groundCheckDistance = 0.1f;
	public LayerMask collisionLayers;

	private float horizontal = 0f;
	private bool jump = false;
	#endregion

	#region Public Properties
	public static PlayerControl Instance
	{ get { return instance; } }

	public bool IsGrounded
	{
		get
		{
			RaycastHit2D groundCheck = Physics2D.Linecast(BottomPosition, BottomPosition - new Vector2(0f, groundCheckDistance), collisionLayers);
			return groundCheck.collider != null;
		}
	}

	public Vector2 BottomPosition
	{
		get
		{
			return new Vector2(transform.position.x,
							   transform.position.y - (transform.localScale.x / 2f));
		}
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		horizontal = Input.GetAxis("Horizontal");
		jump = jump || Input.GetButtonDown("Jump");
	}

	private void FixedUpdate()
	{
		if (horizontal * rigidbody2D.velocity.x < maxSpeed)
			rigidbody2D.AddForce(Vector2.right * horizontal * speed);

		if (Mathf.Abs(rigidbody2D.velocity.x) >= maxSpeed)
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x.Sign() * maxSpeed,
											   rigidbody2D.velocity.y);

		if (IsGrounded && jump)
		{
			rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
			jump = false;
		}
	}
	#endregion
}
