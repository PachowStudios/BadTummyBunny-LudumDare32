using UnityEngine;
using System.Collections;

public sealed class PlayerHealth : MonoBehaviour
{
	#region Fields
	private static PlayerHealth instance;

	public float maxHealth = 100f;
	public float invincibilityPeriod = 2f;
	public Vector2 damageRange = new Vector2(5f, 25f);
	public Vector2 knockbackRange = new Vector2(1f, 3f);
	public float falloutDamage = 25f;

	private float health;
	private bool dead = false;

	private bool invincible = false;
	private float lastHitTime;
	private float flashTimer = 0f;
	private float flashTime = 0.25f;
	private float smoothFlashTime;

	private RespawnPoint respawnPoint;

	private SpriteRenderer spriteRenderer;
	#endregion

	#region Public Properties
	public static PlayerHealth Instance
	{ get { return instance; } }

	public float Health
	{
		get { return health; }
		set
		{
			if (value < health)
				lastHitTime = Time.time;

			health = Mathf.Clamp(value, 0f, maxHealth);
			CheckDeath();
		}
	}

	public bool IsDead
	{ get { return dead; } }

	public float Damage
	{ get { return damageRange.x; } }

	public Vector2 Knockback
	{ get { return new Vector2(knockbackRange.y, knockbackRange.y); } }

	public bool DamagesOnTouch
	{ get { return PlayerControl.Instance.Farting; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		spriteRenderer = GetComponentInChildren<SpriteRenderer>();

		health = maxHealth;
		lastHitTime = Time.time - invincibilityPeriod;
	}

	private void Update()
	{
		if (!dead)
		{
			UpdateInvincibilityFlash();
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Enemy")
		{
			if (!DamagesOnTouch)
				TakeDamage(other.GetComponent<Enemy>());
		}
		else if (other.tag == "Killzone")
		{
			Respawn();
		}
		else if (other.tag == "Respawn")
		{
			SetRespawnPoint(other.GetComponent<RespawnPoint>());
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		OnTriggerEnter2D(other);
	}
	#endregion

	#region Internal Update Methods
	private void UpdateInvincibilityFlash()
	{
		invincible = Time.time <= lastHitTime + invincibilityPeriod;

		if (invincible)
		{
			flashTimer += Time.deltaTime;
			smoothFlashTime = Mathf.Lerp(smoothFlashTime, 0.05f, 0.025f);

			if (flashTimer > smoothFlashTime)
			{
				SetRenderersEnabled(alternate: true);
				flashTimer = 0f;
			}
		}
		else
		{
			SetRenderersEnabled();
			smoothFlashTime = flashTime;
		}
	}
	#endregion

	#region Internal Helper Methods
	private void CheckDeath()
	{
		if (Health <= 0f && !dead)
		{
			dead = true;

			//GameOver();
			SetRenderersEnabled(false);
			collider2D.enabled = false;
			ExplodeEffect.Instance.Explode(transform, PlayerControl.Instance.Velocity, spriteRenderer.sprite);
			PlayerControl.Instance.DisableInput();
		}
	}

	private void Respawn()
	{
		if (dead)
			return;

		Health -= falloutDamage;

		if (!dead)
		{
			if (respawnPoint == null)
				transform.position = Vector3.zero;
			else
				transform.position = respawnPoint.Location;
		}
	}

	private void SetRespawnPoint(RespawnPoint newRespawnPoint)
	{
		if (respawnPoint != null && respawnPoint == newRespawnPoint)
			return;

		if (respawnPoint != null)
			respawnPoint.Deactivate();

		newRespawnPoint.Activate();
		respawnPoint = newRespawnPoint;
	}

	private void SetRenderersEnabled(bool enabled = true, bool alternate = false)
	{
		if (alternate)
			spriteRenderer.enabled = !spriteRenderer.enabled;
		else
			spriteRenderer.enabled = enabled;
	}
	#endregion

	#region Public Methods
	public void TakeDamage(Enemy enemy, float damage = 0f, Vector2 knockback = default(Vector2))
	{
		if (invincible || dead)
			return;

		float knockbackDirection = 1f;
		damage = (damage == 0f) ? enemy.damage : damage;
		knockback = (knockback == default(Vector2)) ? enemy.knockback : knockback;
		knockbackDirection = (transform.position.x - enemy.transform.position.x).Sign();

		if (damage != 0f)
		{
			Health -= damage;

			if (!dead && knockback != default(Vector2))
				StartCoroutine(PlayerControl.Instance.ApplyKnockback(knockback, knockbackDirection));
		}
	}
	#endregion
}
