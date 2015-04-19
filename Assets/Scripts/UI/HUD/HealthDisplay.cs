using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthDisplay : MonoBehaviour
{
	#region Fields
	private static HealthDisplay instance;

	public Mask healthMask;
	public Mask fartMask;
	public float barDamping = 0.5f;

	private float healthPercent;
	private float originalHealthWidth;
	private Vector2 healthVelocity = Vector2.zero;

	private float fartPercent;
	private float originalFartWidth;
	private Vector2 fartVelocity = Vector2.zero;
	#endregion

	#region Public Properties
	public static HealthDisplay Instance
	{ get { return instance; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		originalHealthWidth = healthMask.rectTransform.sizeDelta.x;
		originalFartWidth = fartMask.rectTransform.sizeDelta.x;
	}

	private void Update()
	{
		healthPercent = Mathf.Clamp(PlayerHealth.Instance.Health / PlayerHealth.Instance.maxHealth, 0f, 1f);
		//Fart Percent

		healthMask.rectTransform.sizeDelta = Vector2.SmoothDamp(healthMask.rectTransform.sizeDelta,
																new Vector2(originalHealthWidth * healthPercent, healthMask.rectTransform.sizeDelta.y),
																ref healthVelocity,
																barDamping);
		/*
		fartMask.rectTransform.sizeDelta = Vector2.SmoothDamp(fartMask.rectTransform.sizeDelta, 
															  new Vector2(originalFartWidth * fartPercent, fartMask.rectTransform.sizeDelta.y),
															  ref fartVelocity,
															  barDamping);
		 */
	}
	#endregion
}
