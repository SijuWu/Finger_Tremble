using UnityEngine;
using System.Collections;

public class mouseControl : MonoBehaviour
{
	bool xDone=false;
	bool yDone=false;
	bool zDone=false;
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color=new Color(1,0,0);
//		GameObject cube=GameObject.Find("Cube");
//		Gizmos.DrawWireCube(cube.transform.position,new Vector3(20, 20, 20));
		GameObject cube=GameObject.Find("Cube");
		Gizmos.DrawWireSphere(cube.transform.position,20);
	}
}
