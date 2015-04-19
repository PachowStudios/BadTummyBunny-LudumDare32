using UnityEngine;
using System.Collections;

public class ExplodeEffect : MonoBehaviour
{
	#region Fields
	private static ExplodeEffect instance;

	[SerializeField]
	private SpriteExplosion explosionPrefab;
	#endregion

	#region Public Properties
	public static ExplodeEffect Instance
	{ get { return instance; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;
	}
	#endregion

	#region Public Methods
	public void Explode(Transform target, Vector3 velocity, Sprite sprite, Material material = null)
	{
		if (!sprite)
			return;

		SpriteExplosion explosionInstance = Instantiate(explosionPrefab, target.position, target.rotation) as SpriteExplosion;
		explosionInstance.transform.parent = transform;
		explosionInstance.transform.localScale = target.localScale;
		explosionInstance.material = material;
		explosionInstance.Explode(velocity, sprite);
	}
	#endregion
}
