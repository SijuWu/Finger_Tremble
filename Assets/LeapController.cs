using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using System;
using AssemblyCSharp;


public class LeapController : MonoBehaviour
{
	List<LeapHand> leapHands=new List<LeapHand>();
	List<LeapFinger> leapFingers=new List<LeapFinger>();
	Frame frame;
	//Leap controller
	Controller controller;
	
	List<LeapFinger> strokeFingers=new List<LeapFinger>();
	
	// Use this for initialization
	void Start ()
	{
		controller = new Controller ();
	}

	// Update is called once per frame
	void Update ()
	{
		findPCA();
		
		frame=controller.Frame();
		if (!frame.Hands.IsEmpty) {

			updateHand(frame);
		}
		else{
			foreach(LeapHand leapHand in leapHands)
			{
				Destroy(leapHand.getHandObject());
			}
			leapHands.Clear();	
		}
		
		if(!frame.Fingers.IsEmpty)
		{
			updateFinger(frame);
		}
		else{
			foreach(LeapFinger leapFinger in leapFingers)
			{
				Destroy(leapFinger.getFingerObject());
			}
			leapFingers.Clear();
		}
		
		manipulateObject();
		
	}
	
	void manipulateObject()
	{
//		Debug.Log("StrokeFingers "+strokeFingers.Count);
		
		GameObject cube=GameObject.Find("Cube");
		
		if(strokeFingers.Count==0)
			cube.transform.renderer.material.color=Color.white;
		
		if(strokeFingers.Count==1)
		{
			LeapFinger finger=strokeFingers[0];
			
			if(finger.getStrokeType()==LeapObject.StrokeType.Pan)
			translateObject(finger,cube);
			
			if(finger.getStrokeType()==LeapObject.StrokeType.Rotate)
			rotateObject(finger,cube);

			if(finger.getStrokeType()==LeapObject.StrokeType.Pendulum)
			pendulumObject(finger,cube);
		}
		
		if(strokeFingers.Count==2)
		{
			LeapFinger finger1=strokeFingers[0];
			LeapFinger finger2=strokeFingers[1];
			
			Vector3 dragDirection1=finger1.getLastDirection();
			Vector3 dragDirection2=finger2.getLastDirection();
			
			float dot=Vector3.Dot(dragDirection1,dragDirection2);
			float angle=Mathf.Acos(dot)/(dragDirection1.magnitude*dragDirection2.magnitude)/Mathf.PI*180;
			
//			Debug.Log("two stroke ange "+angle);
		}
	
		
	}
	
	public void pendulumObject(LeapFinger finger,GameObject object_pendulum)
	{
		object_pendulum.transform.renderer.material.color=Color.blue;
		Vector3 center=finger.getPendulumCenter();
		float distance1=Vector3.Distance(center,finger.getLastPosition());
		float distance2=Vector3.Distance(center,finger.getPosition());
		float aveDistance=(distance1+distance2)/2;
		
		float motion=Vector3.Distance(finger.getLastPosition(),finger.getPosition());
		
		float angle=motion/aveDistance;
		
		if(finger.getPendulumDirection()==true)
		{
			Vector3 scale=object_pendulum.transform.localScale;
			if(angle!=0)
			{
				scale.x*=(1+angle/10);
			    scale.y*=(1+angle/10);
			    scale.z*=(1+angle/10);
			}
			object_pendulum.transform.localScale=scale;
			
		}
		else
		{
			Vector3 scale=object_pendulum.transform.localScale;
			if(angle!=0)
			{
				scale.x/=(1+angle/10);
			    scale.y/=(1+angle/10);
			    scale.z/=(1+angle/10);
			}
			object_pendulum.transform.localScale=scale;
		}
	}
	
	public void translateObject(LeapFinger finger,GameObject object_translation)
	{
		if(finger.getStrokeType()==LeapObject.StrokeType.Pan)
		{
			if(finger.getStroke().getStrokePoints().Count>5)
			{
//				if(Vector3.Distance(endPoint,startPoint)<2)
//			    {
//				   finger.setStrokeType(LeapObject.StrokeType.None);
//					Debug.Log("Drag stop");
//					return;
//			    }
			}
				
	        object_translation.transform.renderer.material.color=Color.yellow;
			float motion=(finger.getPosition()-finger.getLastPosition()).magnitude;
			Vector3 dragDirection=finger.getDragDirection();
		
		float xAbs=Mathf.Abs(dragDirection.x);
		float yAbs=Mathf.Abs(dragDirection.y);
		float zAbs=Mathf.Abs(dragDirection.z);
		
		if(zAbs==Mathf.Max(xAbs,Mathf.Max(yAbs,zAbs)))
		{
			dragDirection.x=0;
			dragDirection.y=0;
		}
		else
			dragDirection.z=0;
		//		dragDirection.z=0;
		object_translation.transform.position+=dragDirection*motion*finger.getGain()/10;
			
			}
	}
	
	public void rotateObject(LeapFinger finger,GameObject object_rotation)
	{
		 object_rotation.transform.renderer.material.color=Color.green;
		
		if(finger.getRotationDirection()==false)
		{
			object_rotation.transform.Rotate(finger.getRotationAxis(),finger.getRotationAngle(),Space.World);
		}
		else
		{
			object_rotation.transform.Rotate(finger.getRotationAxis(),-finger.getRotationAngle(),Space.World);
		}
	}
	
	void updateHand(Frame frame)
	{
		HandList hands = frame.Hands;

		foreach (Hand hand in hands) {
				//Check if the hand is in the list
			bool handInclude = false;
			int handIndex=-1;
			foreach (LeapHand leapHand in leapHands) {
				if (hand.Id == leapHand.getId()) {
					handInclude = true;
					handIndex=leapHands.IndexOf(leapHand);
					break;
				}
			}

			//Add the hand into the list if not included
			if (handInclude == false) {
				LeapHand leapHand = new LeapHand (hand);
				leapHands.Add (leapHand);
			}
			//Refresh its position if included
			else{
				LeapHand leapHand=leapHands[handIndex];
				leapHand.refreshHand(hand);
//				leapHand.drawTrajectory();
			}
		}

			for(int i=leapHands.Count-1;i>=0;--i)
			{
				LeapHand leapHand=leapHands[i];

				bool handInclude=false;
				foreach(Hand hand in hands)
				{
					if(hand.Id==leapHand.getId())
					{
						handInclude=true;
						break;
					}
				}

				if(handInclude==false)
				{
					Destroy(leapHand.getHandObject());
					leapHands.Remove(leapHand);

				}
			}
	}

	void updateFinger(Frame frame)
	{
		FingerList fingers=frame.Fingers;
		
		strokeFingers.Clear();
		
		foreach (Finger finger in fingers) 
		{
			//Check if the finger is in the list
			bool fingerInclude = false;
			int fingerIndex=-1;
			foreach (LeapFinger leapFinger in leapFingers) {
				if (finger.Id == leapFinger.getId()) {
					fingerInclude = true;
					fingerIndex=leapFingers.IndexOf(leapFinger);
					break;
				}
			}

			//Add the finger into the list if not included
			if (fingerInclude == false) {
				LeapFinger leapFinger = new LeapFinger (finger);
				leapFingers.Add (leapFinger);
			}
			//Refresh its position if included
			else
			{
				LeapFinger leapFinger=leapFingers[fingerIndex];
			 
				leapFinger.refreshFinger(finger);
				
//				LineRenderer speedLine=GameObject.Find("SpeedLine").GetComponent<LineRenderer>();
//				speedLine.SetColors(Color.gray,Color.blue);
//				speedLine.SetVertexCount(2);
//				speedLine.SetPosition(0,leapFinger.getPosition());
//				speedLine.SetPosition(1,leapFinger.getPosition()+leapFinger.getSpeed());
			    leapFinger.checkGesture();
				if(leapFinger.getStrokeType()==LeapFinger.StrokeType.Pan||
					leapFinger.getStrokeType()==LeapFinger.StrokeType.Rotate||leapFinger.getStrokeType()==LeapFinger.StrokeType.Pendulum)
				{
					strokeFingers.Add(leapFinger);
				}	
			}
		}
		
		
		for(int i=leapFingers.Count-1;i>=0;--i)
		{
			LeapFinger leapFinger=leapFingers[i];
			
			bool fingerInclude=false;
			foreach(Finger finger in fingers)
			{
				if(finger.Id==leapFinger.getId())					
				{
					fingerInclude=true;
					break;
				}
			}

			if(fingerInclude==false)
			{
				Destroy(leapFinger.getFingerObject());
				leapFingers.Remove(leapFinger);
			}
		}
	}
	
	
	
	void findPCA()
	{
		double [,]checkPoints=new double[21,3];
		
		for(int i=0;i<=20;++i)
		{
			checkPoints[i,0]=1*i;
			checkPoints[i,1]=2*i;
			checkPoints[i,2]=3*i;
		}
		
		double []mean=findMean(checkPoints);
		minus(ref checkPoints,mean);
		
		int info;
		double [,]eigenVectors=new double[3,3];
		double []eigenValues=new double[3];
		
		alglib.pcabuildbasis(checkPoints,21,3,out info,out eigenValues,out eigenVectors);
		
		
		double[,] lineVector={{eigenVectors[0,0]},{eigenVectors[1,0]},{eigenVectors[2,0]}};
		
		double[,]projection=new double[21,1];
		alglib.rmatrixgemm(21,1,3,1,checkPoints,0,0,0,lineVector,0,0,0,0,ref projection,0,0);
		
		double[,]result=new double[21,3];
		for(int i=0;i<21;++i)
		{
			result[i,0]=mean[0]+projection[i,0]*lineVector[0,0];
			result[i,1]=mean[1]+projection[i,0]*lineVector[1,0];
			result[i,2]=mean[2]+projection[i,0]*lineVector[2,0];
		}
	}
	
	double [] findMean(double [,] points)
	{
		int rows=points.GetLength(0);
		int columns=points.GetLength(1);
		
		double [] mean=new double[columns];
		
		for(int i=0;i<rows;++i)
		{
			for(int j=0;j<columns;++j)
			{
				mean[j]+=points[i,j];
			}
		}
		
		for(int i=0;i<columns;++i)
		{
			mean[i]/=rows;
		}
		
		return mean;
	}
	
	void minus(ref double[,]points, double[] mean)
	{
		int rows=points.GetLength(0);
		int columns=points.GetLength(1);
		
		for(int i=0;i<rows;++i)
		{
			for(int j=0;j<columns;++j)
			{
				points[i,j]-=mean[j];
			}
		}
	}
	
	
}
