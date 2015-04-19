using UnityEngine;
using System.Collections;

public class Carrot : MonoBehaviour
{
	#region Fields
	private SpriteRenderer spriteRenderer;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	#endregion

	#region Public Methods
	public void Collect()
	{
		ExplodeEffect.Instance.Explode(transform, Vector3.zero, spriteRenderer.sprite);
		Destroy(gameObject);
	}
	#endregion
}
