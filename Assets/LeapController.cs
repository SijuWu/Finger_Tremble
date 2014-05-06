using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using System;


public class leapObject
{
	public double [] findMean(double [,] points)
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
	
	public void minus(ref double[,]points, double[] mean)
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
public class LeapHand: leapObject
{
	public LeapHand (Hand hand)
	{
		handObject = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		handObject.transform.renderer.material.color=Color.white;
		handObject.transform.localScale=new Vector3(10,10,10);
		handObject.transform.position = new Vector3 (hand.PalmPosition.x, hand.PalmPosition.y, -hand.PalmPosition.z);
		
		//Save id
		id = hand.Id;
		
		//Save speed
		speed=new Vector3(hand.PalmVelocity.x,hand.PalmVelocity.y,-hand.PalmVelocity.z);
		
		//Save hand point to the trajectory
		handPoints.Add(handObject.transform.position);
		
		//Line renderer of hand trajectory
		handLine=handObject.AddComponent<LineRenderer>();
		handLine.material = new Material(Shader.Find("Particles/Additive"));
		handLine.SetWidth(1,1);
		handLine.SetColors(Color.yellow,Color.yellow);
		
	}
	
	public void refreshHand(Hand hand)
	{
		setPosition(hand.PalmPosition);
		setSpeed(hand.PalmVelocity);
	}
	
	public void setPosition (Leap.Vector position)
	{
		handObject.transform.position = new Vector3 (position.x, position.y, -position.z);
		handPoints.Add(handObject.transform.position);
	}
	
	public void setSpeed(Leap.Vector velocity)
	{
		speed=new Vector3(velocity.x,velocity.y,-velocity.z);
	}

	public Vector3 getPosition ()
	{
		return handObject.transform.position;
	}
	
	public Vector3 getSpeed()
	{
		return speed;
	}

	public int getId ()
	{
		return id;
	}

	public GameObject getHandObject()
	{
		return handObject;
	}
	
	public List<Vector3> getHandTrajectory()
	{
		return handPoints;
	}
	
	public void drawTrajectory()
	{
		if(handPoints.Count<handLineCount)
		{
			handLine.SetVertexCount(handPoints.Count);
		
		    for(int i=0;i<handPoints.Count;++i)
		    {
			    handLine.SetPosition(i,handPoints[i]);
		    }
		}
		else
		{
			handLine.SetVertexCount(handLineCount);
			for(int i=handPoints.Count-1;i>handPoints.Count-handLineCount-1;--i)
			{
				handLine.SetPosition(handPoints.Count-i-1,handPoints[i]);
			}	
		}
		
	}
	
	private GameObject handObject;
	private int id;
	private Vector3 speed;
	private List<Vector3> handPoints=new List<Vector3>();
	private LineRenderer handLine;
	private int handLineCount=30;
}

public class LeapFinger:leapObject
{
	public LeapFinger (Finger finger)
	{
		this.finger=finger;
		
		fingerObject = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		fingerObject.transform.renderer.material.color=Color.white;
		fingerObject.transform.localScale=new Vector3(10,10,10);
		
		position=new Vector3 (finger.TipPosition.x, finger.TipPosition.y, -finger.TipPosition.z);
		fingerObject.transform.position =position;
		
		speed=new Vector3(finger.TipVelocity.x,finger.TipVelocity.y,-finger.TipVelocity.z);
		
		id = finger.Id;
		fingerPoints.Add(position);
		
//		fingerLine=fingerObject.AddComponent<LineRenderer>();
//		fingerLine.material = new Material(Shader.Find("Particles/Additive"));
//		fingerLine.SetWidth(1,1);
//		fingerLine.SetColors(Color.green,Color.green);
		
		fittingLine=fingerObject.AddComponent<LineRenderer>();
		fittingLine.material = new Material(Shader.Find("Particles/Additive"));
		fittingLine.SetWidth(1,1);
		fittingLine.SetColors(Color.red,Color.red);
	}
	
	public void refreshFinger(Finger finger)
	{
		this.finger=finger;
		setPosition(finger.TipPosition);	
		setSpeed(finger.TipVelocity);
	}
	
	public void setPosition (Leap.Vector pos)
	{
		lastPosition=position;
		position=new Vector3(pos.x,pos.y,-pos.z);
		fingerObject.transform.position = position;
		fingerPoints.Add(position);
	}
	
	public void setSpeed(Leap.Vector velocity)
	{
		lastSpeed=speed;
		speed=new Vector3(velocity.x,velocity.y,-velocity.z);
	}
	
	public Vector3 getPosition ()
	{
		return fingerObject.transform.position;
	}
	
	public Vector3 getLastPosition()
	{
		return lastPosition;
	}
	
	public Vector3 getSpeed()
	{
		return speed;
	}
	
	public Vector3 getLastSpeed()
	{
		return lastSpeed;
	}
	
	public int getId ()
	{
		return id;
	}

	public GameObject getFingerObject()
	{
		return fingerObject;
	}
	
	public List<Vector3> getFingerPoints()
	{
		return fingerPoints;
	}
	
	public void drawTrajectory()
	{
		float time=Time.time;
		
		if(fingerPoints.Count==1)
			startTime=time;
		
		Vector3 acceleration=(speed-lastSpeed/(time-lastTime));

//		if(acceleration.magnitude>800)
//		{
//			Debug.Log("acceleration");
//		}
		if(fingerPoints.Count<2)
			return;
		Vector3 startPoint=fingerPoints[0];
		Vector3 lastEndPoint=fingerPoints[fingerPoints.Count-2];
		Vector3 endPoint=fingerPoints[fingerPoints.Count-1];
		
		averageSpeed+=speed;
		
		//If the distance between endPoint and startPoint is shorter than that between lastEndPoint and startPoint
		//Clear fingerPoints and regenerate it
		if(Vector3.Distance(startPoint,lastEndPoint)>Vector3.Distance(startPoint,endPoint))
		{
		    
			endTime=lastTime;
			
			
		   			
			
//			Debug.Log("average speed "+averageSpeed+" magnitude "+averageSpeed.magnitude);
//			if(acceleration.magnitude>350)
//			{
//				Debug.Log("acceleration magnitude "+acceleration.magnitude+" lastspeed "+lastSpeed+" speed "+speed);
//				if(startPoint.y>lastEndPoint.y)
//					Debug.Log("Down");
//				if(startPoint.y<lastEndPoint.y)
//					Debug.Log("Up");
//			}
	     	
			
			Vector3 direction=new Vector3();
			
			float cost=0;
	     	List<Vector3> fitPoints=fitLine(fingerPoints.GetRange(0,fingerPoints.Count-1),out direction,out cost);
			float distance=Vector3.Distance(fitPoints[0],fitPoints[fitPoints.Count-1]);
		
			averageSpeed-=speed;
			averageSpeed/=(fingerPoints.Count-1);
			
			Vector3 meanSpeed=(lastEndPoint-startPoint)/(endTime-startTime);
			if(cost<5&&meanSpeed.magnitude>150)
			{
//				Debug.Log("Start");
//					if(fitPoints[0].y>fitPoints[fitPoints.Count-1].y)
//					Debug.Log("Down "+Mathf.Abs(startPoint.y-lastEndPoint.y)+" Cost "+cost+" meanSpeed "+meanSpeed.magnitude);
//				if(fitPoints[0].y<fitPoints[fitPoints.Count-1].y)
//					Debug.Log("Up "+Mathf.Abs(startPoint.y-lastEndPoint.y)+" Cost "+cost+" meanSpeed "+meanSpeed.magnitude);
				
				
//				Debug.Log("Cost "+cost+" dragDistance "+distance);
			}
			
		 
		fittingLine.SetVertexCount(fitPoints.Count);
		for(int i=0;i<fitPoints.Count;++i)
		{
			fittingLine.SetPosition(i,fitPoints[i]);
		}
			

			float directionDot=Vector3.Dot(direction,lastDirection)/(direction.magnitude*lastDirection.magnitude);
		    float angle=Mathf.Acos(directionDot)/Mathf.PI*180;
		
			


			
			
			
			
			
			
			if(cycloPanWait==false)
			{
				if(acceleration.magnitude>400)
				{
//					Debug.Log("distance "+distance);
//					Debug.Log("Wait");
//					Debug.Log("start direciton "+direction);
					cycloPanWait=true;
					cycloDirection=direction;
				}
			}
			else
			{
				if(cycloPanStart==false)
				{
					if(angle>165&&distance>2&&cost<20)
				  {
//					Debug.Log("Start");
//					Debug.Log (cycloDirection);
					cycloPanStart=true;
				  }
				else
				  {
					cycloPanWait=false;
				  }
				}
				else
				{
					if(angle<165||distance<=2||cost>20)
					{
//					    Debug.Log("angle "+angle+" distance "+distance+" cost "+cost);
//						Debug.Log("Stop");
					
						cycloPanStart=false;
						cycloPanWait=false;
					}
				}
				
			}
			
			
			averageSpeed=new Vector3();
			averageSpeed+=lastSpeed;
			averageSpeed+=speed;
			
			fingerPoints.Clear();
			fingerPoints.Add(lastEndPoint);
			fingerPoints.Add(endPoint);
	         
		    startTime=lastTime;
			
			lastDirection=direction;
			
		}
	
		lastTime=time;
//		Vector3 strokeVector=new Vector3();
//				List<Vector3> fitPoints=fitLine2D(fingerPoints,out strokeVector);
//		
//		fittingLine.SetVertexCount(fitPoints.Count);
//		for(int i=0;i<fitPoints.Count;++i)
//		{
//			fittingLine.SetPosition(i,fitPoints[i]);
//		}
		
//		//If the number of points in fingerpoints is smaller than the limit
//		if(fingerPoints.Count<fingerLineCount)
//		{
//			fingerLine.SetVertexCount(fingerPoints.Count);
//		
//		    for(int i=0;i<fingerPoints.Count;++i)
//		    {
//			    fingerLine.SetPosition(i,fingerPoints[i]);
//		    }
//		}
//		//If the number of points in fingerpoints is larger than the limit
//		else
//		{
//			fingerLine.SetVertexCount(fingerLineCount);
//			for(int i=fingerPoints.Count-1;i>fingerPoints.Count-fingerLineCount-1;--i)
//			{
//				fingerLine.SetPosition(fingerPoints.Count-i-1,fingerPoints[i]);
//			}	
//		}
		
//		List<Vector3> fitPoints=fitLine(fingerPoints);
//		
//		fittingLine.SetVertexCount(fitPoints.Count);
//		for(int i=0;i<fitPoints.Count;++i)
//		{
//			fittingLine.SetPosition(i,fitPoints[i]);
//		}
	}
	
	List<Vector3> fitLine2D(List<Vector3> strokePoints,out Vector3 strokeVector)
	{
		double [,] points=new double [strokePoints.Count,2];
		
		for(int i=0;i<strokePoints.Count;++i)
		{
			points[i,0]=strokePoints[i].x;
			points[i,1]=strokePoints[i].y;
		}
		
		//find the mean point
		double []mean=findMean(points);
		
		//minus the mean point
		minus (ref points,mean);
		
		//use PCA to find the eigen vectors and the eigen values
		int info;
		double [,]eigenVectors=new double[2,2];
		double []eigenValues=new double[2];
		
		alglib.pcabuildbasis(points,strokePoints.Count,2,out info,out eigenValues,out eigenVectors);
		
		//Get the first eigen vector
		double[,] lineVector={{eigenVectors[0,0]},{eigenVectors[1,0]}};
		
		//Save the stroke vector
		strokeVector=new Vector3((float)eigenVectors[0,0],(float)eigenVectors[1,0],0);
		
		//Project each point to the fitting line
		double[,]projection=new double[strokePoints.Count,1];
		alglib.rmatrixgemm(strokePoints.Count,1,2,1,points,0,0,0,lineVector,0,0,0,0,ref projection,0,0);
		
		//Map each stroke point to a point on the fitting line
		double[,]resultPoint=new double[strokePoints.Count,2];
		for(int i=0;i<strokePoints.Count;++i)
		{
			resultPoint[i,0]=mean[0]+projection[i,0]*lineVector[0,0];
			resultPoint[i,1]=mean[1]+projection[i,0]*lineVector[1,0];
		}
		
		//Save points of the fitting line
		List<Vector3> linePoints=new List<Vector3>();
		
		for(int i=0;i<strokePoints.Count;++i)
		{
			linePoints.Add(new Vector3((float)resultPoint[i,0],(float)resultPoint[i,1],0));
		}
		
		return linePoints;
	}
	
	List<Vector3> fitLine(List<Vector3> strokePoints,out Vector3 strokeVector,out float cost)
	{
		double [,] points=new double [strokePoints.Count,3];
		
		//Save stroke points
		for(int i=0;i<strokePoints.Count;++i)
		{
			points[i,0]=strokePoints[i].x;
			points[i,1]=strokePoints[i].y;
			points[i,2]=strokePoints[i].z;
		}
		
		//find the mean point
		double []mean=findMean(points);
		
		//minus the mean point
		minus (ref points,mean);
		
		//use PCA to find the eigen vectors and the eigen values
		int info;
		double [,]eigenVectors=new double[3,3];
		double []eigenValues=new double[3];
		
		alglib.pcabuildbasis(points,strokePoints.Count,3,out info,out eigenValues,out eigenVectors);
//		Debug.Log(eigenValues[0]+" "+eigenValues[1]+" "+eigenValues[2]);
		Debug.Log("first "+new Vector3((float)eigenVectors[0,0],(float)eigenVectors[1,0],(float)eigenVectors[2,0]));
		Debug.Log("second "+new Vector3((float)eigenVectors[0,1],(float)eigenVectors[1,1],(float)eigenVectors[2,1]));
		//Get the first eigen vector
		double[,] lineVector={{eigenVectors[0,0]},{eigenVectors[1,0]},{eigenVectors[2,0]}};
		
		//Project each point to the fitting line
		double[,]projection=new double[strokePoints.Count,1];
		alglib.rmatrixgemm(strokePoints.Count,1,3,1,points,0,0,0,lineVector,0,0,0,0,ref projection,0,0);
		
		//Map each stroke point to a point on the fitting line
		double[,]resultPoint=new double[strokePoints.Count,3];
		for(int i=0;i<strokePoints.Count;++i)
		{
			resultPoint[i,0]=mean[0]+projection[i,0]*lineVector[0,0];
			resultPoint[i,1]=mean[1]+projection[i,0]*lineVector[1,0];
			resultPoint[i,2]=mean[2]+projection[i,0]*lineVector[2,0];
		}
		
		//Save points of the fitting line
		List<Vector3> linePoints=new List<Vector3>();
		
		for(int i=0;i<strokePoints.Count;++i)
		{
			linePoints.Add(new Vector3((float)resultPoint[i,0],(float)resultPoint[i,1],(float)resultPoint[i,2]));
		}
		
		//Save the stroke vector
		strokeVector=linePoints[strokePoints.Count-1]-linePoints[0];
		strokeVector.Normalize();
		
		cost=0;
		for(int i=0;i<strokePoints.Count;++i)
		{
			cost+=Mathf.Pow(Vector3.Distance(strokePoints[i],linePoints[i]),2);
		}
		
		cost/=2*strokePoints.Count;
		
		return linePoints;
	}
	
	//Leap finger object
	private Finger finger;
	
	//Finger sphere
	private GameObject fingerObject;
    
	//Finger id
	private int id;
	
	//Finger position
	private Vector3 position;
	
	//Finger last position
	private Vector3 lastPosition;
	
	//Finger speed
	private Vector3 speed;
	
	//Finger last speed
	private Vector3 lastSpeed;
	
	private Vector3 averageSpeed;
	
	//Finger stroke
	private List<Vector3> fingerPoints=new List<Vector3>();
	
	//Finger strokes
	private List<List<Vector3>> strokes=new List<List<Vector3>>();
	
	//Line used to draw finger trajectory
	private LineRenderer fingerLine=new LineRenderer();
	
	//Line used to draw fitting line
	private LineRenderer fittingLine=new LineRenderer();
	
	//Finger line maxmimum point count
	private int fingerLineCount=3000;
	
    //Indicate if to wait the cyclo pan gesture
	bool cycloPanWait=false;
	
	//Indicate if to start the cyclo pan gesture
	bool cycloPanStart=false;
	
	float lastTime;
	
	float startTime;
	
	float endTime;
	
	//Last stroke direction
	Vector3 lastDirection=new Vector3();
	//Direction of cyclo pan gesutre
	Vector3 cycloDirection=new Vector3();
}

public class LeapController : MonoBehaviour
{
	List<LeapHand> leapHands=new List<LeapHand>();
	List<LeapFinger> leapFingers=new List<LeapFinger>();
	Frame frame;
	//Leap controller
	Controller controller;
	

	
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

			updateFinger(frame);
			
			
		}
		else{
			foreach(LeapHand leapHand in leapHands)
			{
				Destroy(leapHand.getHandObject());
			}
			leapHands.Clear();

			foreach(LeapFinger leapFinger in leapFingers)
			{
				Destroy(leapFinger.getFingerObject());
			}
			leapFingers.Clear();
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
				leapHand.drawTrajectory();
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

		foreach (Finger finger in fingers) {
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
				else{
					LeapFinger leapFinger=leapFingers[fingerIndex];
				  
					leapFinger.refreshFinger(finger);
				    leapFinger.drawTrajectory();
//				    print ("time "+Time.time+" count "+leapFinger.getFingerPoints().Count);
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
