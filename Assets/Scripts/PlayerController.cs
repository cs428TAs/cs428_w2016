using UnityEngine;
using System.Collections;

public class PlayerController : TimeAffected
{

	public static int zone = 1;


	// Use this for initialization
	void Start ()
	{
		base.initialize();
	}
	
	// Update is called once per frame
	void Update ()
	{
		base.step();
	}
}
