using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class PlayerHealth : MonoBehaviour
{
	#region Fields
	private static PlayerHealth instance;

	public float maxHealth = 100f;
	[Range(0f, 1f)]
	public float carrotHealthRechargePercent = 0.1f;
	public float falloutDamage = 25f;
	public float invincibilityPeriod = 2f;
	public float damage = 10f;
	public float damageRate = 3f;
	public Vector2 knockback = new Vector2(2f, 2f);

	public float fartRange = 10f;
	public Vector2 fartWidth = new Vector2(1f, 4f);
	[SerializeField]
	private Transform fartColliderTransform;

	private float health;
	private bool dead = false;

	private float damageTime;
	private float damageTimer;

	private bool invincible = false;
	private float lastHitTime;
	private float flashTimer = 0f;
	private float flashTime = 0.25f;
	private float smoothFlashTime;

	private RespawnPoint respawnPoint;

	private SpriteRenderer spriteRenderer;
	private PolygonCollider2D fartCollider;
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

	public float HealthPercent
	{ get { return Mathf.Clamp(health / maxHealth, 0f, 1f); } }

	public bool IsDead
	{ get { return dead; } }

	public float Damage
	{ get { return damage; } }

	public Vector2 Knockback
	{ get { return knockback; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		fartCollider = fartColliderTransform.gameObject.AddComponent<PolygonCollider2D>();
		fartCollider.isTrigger = true;
		fartCollider.enabled = false;
		SetFartCollider();

		health = maxHealth;
		damageTime = 1f / damageRate;
		damageTimer = damageTime;
		lastHitTime = Time.time - invincibilityPeriod;
	}

	private void Update()
	{
		if (!dead)
		{
			UpdateInvincibilityFlash();

			if (PlayerControl.Instance.Farting)
			{
				damageTimer += Time.deltaTime;

				if (damageTimer >= damageTime)
				{
					DamageTargets();
					damageTimer = 0f;
				}
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Enemy")
		{
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

	// Ewwwwwww fuck it only 9 hours left
	// I feel unclean for writing this
	private void DamageTargets()
	{
		Vector3 origin = fartColliderTransform.position;

		foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
		{
			if (enemy.collider2D != null)
			{
				fartCollider.enabled = true;

				if (fartCollider.OverlapPoint(enemy.collider2D.bounds.center))
				{
					RaycastHit2D linecast = Physics2D.Linecast(origin, enemy.collider2D.bounds.center, PlayerControl.Instance.CollisionLayers);
					
					if (linecast.collider == null)
						enemy.TakeDamageFromPlayer();
				}

				fartCollider.enabled = false;
			}
		}
	}
	#endregion

	#region Internal Helper Methods
	private void CheckDeath()
	{
		if (Health <= 0f && !dead)
		{
			dead = true;

			StartCoroutine(GameMenu.Instance.ShowGameOver(0f));
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

	private void SetFartCollider()
	{
		fartCollider.SetPath(0, new Vector2[] { fartColliderTransform.TransformPointLocal(new Vector2(0.75f, fartWidth.x / 2f)),
												fartColliderTransform.TransformPointLocal(new Vector2(0.75f, -(fartWidth.x / 2f))),
												fartColliderTransform.TransformPointLocal(new Vector2(-fartRange, (-fartWidth.y / 2f))),
												fartColliderTransform.TransformPointLocal(new Vector2(-fartRange, fartWidth.y / 2f)) });
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

			if (!dead && !PlayerControl.Instance.Farting && knockback != default(Vector2))
				StartCoroutine(PlayerControl.Instance.ApplyKnockback(knockback, knockbackDirection));
		}
	}
	#endregion
}
