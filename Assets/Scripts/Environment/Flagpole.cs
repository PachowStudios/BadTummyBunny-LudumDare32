using UnityEngine;
using System.Collections;

public class Flagpole : MonoBehaviour
{
	#region Fields
	private bool activated = false;

	private Animator animator;
	#endregion

	#region Public Properties
	public bool Activated
	{ get { return activated; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		animator = GetComponent<Animator>();
	}
	#endregion

	#region Public Methods
	public void Activate()
	{
		if (activated)
			return;

		activated = true;
		animator.SetTrigger("Activate");
	}
	#endregion
}
