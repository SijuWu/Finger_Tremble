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
		Vector3 eulerAngle = gameObject.transform.rotation.eulerAngles;
		
		
		if (Input.GetMouseButton (2)) {
			if(yDone==true)
				return;
			gameObject.transform.Rotate (0, 1, 0, Space.World);
			float yAngle = gameObject.transform.rotation.eulerAngles.y;
			if (yAngle < 10)
			{
				gameObject.transform.Rotate (0, yAngle, 0, Space.World);
				yDone=true;
			}
			
		}
		
		
		if (Input.GetMouseButton (0)) {
			if(xDone==true)
				return;
			if(yDone==false)
				return;
			gameObject.transform.Rotate (1, 0, 0, Space.World);
			
			float xAngle = gameObject.transform.rotation.eulerAngles.x;
			if (xAngle < 10)
			{
				
				gameObject.transform.Rotate (xAngle, 0, 0, Space.World);
				xDone=true;
			}
			
		
		}
	
		if (Input.GetMouseButton (1)) {
			if(zDone==true)
				return;
			if(xDone==false)
				return;
			gameObject.transform.Rotate (0, 0, 1, Space.World);

			float zAngle = gameObject.transform.rotation.eulerAngles.z;
			if (zAngle < 10)
			{
				
				gameObject.transform.Rotate (0, 0, zAngle, Space.World);
				zDone=true;
			}
		}
		
	}
}
