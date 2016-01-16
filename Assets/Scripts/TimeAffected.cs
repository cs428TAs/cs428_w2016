using UnityEngine;
using System.Collections;

public class TimeAffected : MonoBehaviour
{
	int updateDelay = 1;
	protected bool canUpdatePast = false;
	public bool isParent = true;
	TimeAffected otherSelf;
	SpriteRenderer rend;
	Transform oldTransform;
	Component[] components = new Component[0];

	int counter = 0;

	Vector2[] previousPositions;

	// Use this for initialization
	public void initialize ()
	{
		rend = GetComponent<SpriteRenderer>();
		previousPositions = new Vector2[60*updateDelay];

		if (isParent)
		{
			oldTransform = transform;
			Invoke("toggleCanUpdatePast", updateDelay);
			GameObject otherGO = (GameObject) Instantiate(gameObject, transform.position, transform.localRotation);
			otherSelf = otherGO.GetComponent<TimeAffected>();
			otherSelf.isParent = false;
			otherSelf.initialize();
			otherSelf.toggleReality();
		}
	}

	protected void step()
	{
		previousPositions[counter % (previousPositions.Length)] = (Vector2) transform.position;
		counter++;

		if (canUpdatePast)
			otherSelf.transform.position = previousPositions[(counter + previousPositions.Length) % previousPositions.Length];
	}
	
	void toggleCanUpdatePast ()
	{
		canUpdatePast = !canUpdatePast;
	}

	public void toggleReality()
	{
		if (components.Length == 0)
		{
			components = GetComponents<Component>();
			for (int i=0; i<components.Length; i++)
			{
				if (!(components[i] is Renderer || components[i] is TimeAffected) || components[i] is Transform)
					Destroy(components[i]);
			}
		}
		toggleOpacity();
	}

	void toggleOpacity()
	{
		if (rend.color.a == .5f)
			rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 1f);
		else
			rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, .5f);
	}
}
