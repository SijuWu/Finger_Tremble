using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using System;


public class Stroke
{
	public Stroke()
	{
		
	}
	
	public Stroke(List<Vector3> stroke)
	{
		foreach(Vector3 strokePoint in stroke)
		{
			strokePoints.Add(strokePoint);
		}
	}
	
	public List<Vector3> getStrokePoints ()
	{
		return strokePoints;
	}
	
	public float getStartTime ()
	{
		return startTime;
	}
	
	public void setStartTime (float time)
	{
		startTime = time;
	}
	
	public float getEndTime ()
	{
		return endTime;
	}
	
	public void setEndTime (float time)
	{
		endTime = time;
	}
	
	//List of stroke points
	List<Vector3> strokePoints=new List<Vector3>();
	//Start time of the stroke
	float startTime;
	//End time of the stroke
	float endTime;
}

public class LeapObject
{
	public enum StrokeType
{
	Pan,
	WaitZoom,
	Zoom,
	None
};
	
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
	
		public float mean (List<float> vector)
	{
		float mean = 0;
		foreach (float element in vector) {
			mean += element;
		}
		
		
		return mean / vector.Count;
	}
	
	public void reduceMean (List<float> vector, float mean)
	{
		for (int i=0; i<vector.Count; ++i) {
			vector [i] -= mean;
		}
	}
	
	protected StrokeType strokeType=StrokeType.None;
}
public class LeapHand: LeapObject
{
	public LeapHand (Hand hand)
	{
		handObject = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		handObject.transform.renderer.material.color=Color.white;
		handObject.transform.localScale=new Vector3(10,10,10);
		handObject.transform.position = new Vector3 (hand.PalmPosition.x, hand.PalmPosition.y, -hand.PalmPosition.z);
		
		//Save id
		id = hand.Id;
		
		//Save spee
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

public class LeapFinger:LeapObject
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

		stroke.setStartTime(Time.time);
		stroke.getStrokePoints().Add(position);
		
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
		stroke.getStrokePoints().Add(position);
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
	
//	public List<Vector3> getFingerPoints()
//	{
//		return fingerPoints;
//	}
	
	public void drawTrajectory()
	{

		float time=Time.time;
		

		if(stroke.getStrokePoints().Count<2)
			return;
		

		Vector3 startPoint=stroke.getStrokePoints()[0];
		Vector3 lastEndPoint=stroke.getStrokePoints()[stroke.getStrokePoints().Count-2];
		Vector3 endPoint=stroke.getStrokePoints()[stroke.getStrokePoints().Count-1];
		
		
		
		//If the distance between endPoint and startPoint is shorter than that between lastEndPoint and startPoint
		//Clear fingerPoints and regenerate it
		if(Vector3.Distance(startPoint,lastEndPoint)>Vector3.Distance(startPoint,endPoint))
		{
		    
			//Try to fit the finger trajectory to a line
			float cost=0;
			Vector3 direction=new Vector3();
	     	List<Vector3> fitPoints=fitLine(stroke.getStrokePoints().GetRange(0,stroke.getStrokePoints().Count-1),out direction,out cost);
			float distance=Vector3.Distance(fitPoints[0],fitPoints[fitPoints.Count-1]);
			
			List<Vector2> planePoints=new List<Vector2>();
			List<Vector3> ellipsePoints=fitEllipse(stroke.getStrokePoints().GetRange(0,stroke.getStrokePoints().Count-1),ref planePoints);
			
//		    float eccentricity=FitStroke2Ellipse(planePoints);
//			Debug.Log("eccentricity "+eccentricity);

			drawEllipse();
		//Draw the fitting line 
		fittingLine.SetVertexCount(fitPoints.Count);
		for(int i=0;i<fitPoints.Count;++i)
		{
			fittingLine.SetPosition(i,fitPoints[i]);
		}
			
			//Get the mean speed of the stroke
			
			stroke.setEndTime(lastTime);
			

			meanSpeed=(lastEndPoint-startPoint)/(stroke.getEndTime()-stroke.getStartTime());

			
			float xVariation=lastEndPoint.x-startPoint.x;
			float yVariation=lastEndPoint.y-startPoint.y;
			float zVariation=lastEndPoint.z-startPoint.z;
			
//			if(Mathf.Abs(xVariation)>Mathf.Abs(yVariation)&&Mathf.Abs(xVariation)>Mathf.Abs(zVariation))
//			{
//				//left
//				if(xVariation<0)
//				{
//					if(cost>0.2&&cost<40&&meanSpeed.magnitude>140)
//					{
//						Debug.Log("left"+" cost "+cost+" meanspeed "+meanSpeed.magnitude);
//					}
//					if(cost>0.2&&cost>40)
//					{
//						Debug.Log("left" +"cost too large"+cost);
//					}
//					if(cost>0.2&&meanSpeed.magnitude<140)
//					{
//						Debug.Log("left "+"speed too slow"+meanSpeed.magnitude);
//					}
//				}
//				
//				if(xVariation>0)
//				{
//					if(cost>0.2&&cost<40&&meanSpeed.magnitude>140)
//					{
//						Debug.Log("right "+" cost "+cost+" meanspeed "+meanSpeed.magnitude);
//					}
//					
//					if(cost>0.2&&cost>40)
//					{
//						Debug.Log("right" +"cost too large"+cost);
//					}
//					if(cost>0.2&&meanSpeed.magnitude<140)
//					{
//						Debug.Log("right "+"speed too slow"+meanSpeed.magnitude);
//					}
//				}
//			}
			if(cycloPanStart==false)
			{
				if(Mathf.Abs(yVariation)>Mathf.Abs(xVariation)&&
				Mathf.Abs(yVariation)>Mathf.Abs(zVariation))
			{
				//up
				if(yVariation>0)
				{
					if(cost>0.2&&cost<40&&meanSpeed.magnitude>140)
						{
//							Debug.Log ("up "+"cost "+cost+" speed "+meanSpeed.magnitude);
					
					cycloPanStart=true;
					lastDirection=direction;
					cycloDirection=direction;
						}
						
//					
//					strokes.Add(stroke);
//					if(cost>40)
//						Debug.Log("up too large cost "+cost);
//					if(cost>0.2&&meanSpeed.magnitude<140)
//						Debug.Log("up too slow "+meanSpeed.magnitude);
				}
				//down
				if(yVariation<0)
				{
					if(cost>0.2&&cost<10&&meanSpeed.magnitude>140)
						{
//							Debug.Log ("down "+"cost "+cost+" speed "+meanSpeed.magnitude);
							cycloPanStart=true;
					lastDirection=direction;
					cycloDirection=direction;
						}
						
//					if(cost>10)
//						Debug.Log("down too large cost "+cost);
//					if(cost>0.2&&meanSpeed.magnitude<140)
//						Debug.Log("down too slow "+meanSpeed.magnitude);
					
				}
			}
				
				
				if(Mathf.Abs(xVariation)>Mathf.Abs(yVariation)&&Mathf.Abs(xVariation)>Mathf.Abs(zVariation))
			{
				//left
				if(xVariation<0)
				{
					if(cost>0.2&&cost<40&&meanSpeed.magnitude>140)
					{
//						Debug.Log("left"+" cost "+cost+" meanspeed "+meanSpeed.magnitude);
							cycloPanStart=true;
					lastDirection=direction;
					cycloDirection=direction;
					}
					
				}
				
				if(xVariation>0)
				{
					if(cost>0.2&&cost<40&&meanSpeed.magnitude>140)
					{
//						Debug.Log("right"+" cost "+cost+" meanspeed "+meanSpeed.magnitude);
							cycloPanStart=true;
					lastDirection=direction;
					cycloDirection=direction;
					}
					
					
				}
			}
				
			if(Mathf.Abs(zVariation)>Mathf.Abs(yVariation)&&Mathf.Abs(zVariation)>Mathf.Abs(xVariation))
			{
				//back
				if(zVariation<0)
				{
					if(cost>0.2&&cost<40&&meanSpeed.magnitude>140)
					{
//						Debug.Log("left"+" cost "+cost+" meanspeed "+meanSpeed.magnitude);
							cycloPanStart=true;
					lastDirection=direction;
					cycloDirection=direction;
					}
					
				}
				
				//front
				if(zVariation>0)
				{
					if(cost>0.2&&cost<40&&meanSpeed.magnitude>140)
					{
//						Debug.Log("right"+" cost "+cost+" meanspeed "+meanSpeed.magnitude);
							cycloPanStart=true;
					lastDirection=direction;
					cycloDirection=direction;
					}
					
					
				}
			}
			}
			
			else{
			
				float angle=Mathf.Acos(Vector3.Dot(lastDirection,direction)/(lastDirection.magnitude*direction.magnitude))*180/Mathf.PI;
			   
				if(distance<3||angle<160)
				{
					if(distance<3)
						Debug.Log("Stop distance");
					if(angle<160)
						Debug.Log("stop angle");
					cycloPanStart=false;
					
					strokes.Clear();
				}
				else
				{
					strokes.Add(stroke);
//					Debug.Log("distance "+distance);
					float mean_frequence=0;
					float frequenceCount=0;
					
					for(int i=strokes.Count-1;i>=0;--i)
					{
						float frequence=1/(2*(strokes[i].getEndTime()-strokes[i].getStartTime()));
						mean_frequence+=frequence;
						frequenceCount++;
						if(frequenceCount==3)
							break;
					}
					
					mean_frequence/=frequenceCount;
					
					gain=mean_frequence*2/3;
//					Debug.Log("mean f "+mean_frequence+" gain "+gain);
					
					
				}
			}
			

			ellipseStrokes.Add(stroke);

		
			
			stroke.getStrokePoints().Clear();
			stroke.getStrokePoints().Add(lastEndPoint);
			stroke.getStrokePoints().Add(endPoint);
			
	        lastMeanSpeed=meanSpeed;
			
		    startTime=lastTime;
			stroke.setStartTime(lastTime);
			lastDirection=direction;
			
			
			
		}
	
		if(cycloPanStart==true)
		{
			GameObject cube=GameObject.Find("Cube");
			float motion=(position-lastPosition).magnitude;
			cube.transform.position+=cycloDirection.normalized*motion*gain/10;
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
		

	}
	
	void drawEllipse()
	{
		
		
		if(strokeType==StrokeType.None)
		{
			List<Vector2> planePoints=new List<Vector2>();
		    List<Vector3> ellipsePoints=fitEllipse(stroke.getStrokePoints().GetRange(0,stroke.getStrokePoints().Count-1),ref planePoints);
			
	        float eccentricity=FitStroke2Ellipse(planePoints);
		    Debug.Log("eccentricity "+eccentricity);
			
			if(eccentricity<0.75)
			{
			strokeType=StrokeType.WaitZoom;
				
			}
			return;
		}
		
		if(strokeType==StrokeType.WaitZoom)
		{
			if(ellipseStrokes.Count<2)
				return;
			Stroke lastStroke=ellipseStrokes[ellipseStrokes.Count-1];
			Stroke combinedStroke=new Stroke();
			combinedStroke.getStrokePoints().AddRange(lastStroke.getStrokePoints());
			combinedStroke.getStrokePoints().AddRange(stroke.getStrokePoints().GetRange(0,stroke.getStrokePoints().Count-1));
			
			List<Vector2> planePoints=new List<Vector2>();
		    List<Vector3> ellipsePoints=fitEllipse(combinedStroke.getStrokePoints(),ref planePoints);
			
			float eccentricity=FitStroke2Ellipse(planePoints);
			Debug.Log("eccentricity "+eccentricity);
			
			if(eccentricity<0.75)
				strokeType=StrokeType.Zoom;
			else
				strokeType=StrokeType.None;
			
			return;
		}
		
		if(strokeType==StrokeType.Zoom)
		{
			if(ellipseStrokes.Count<2)
				return;
			Stroke lastStroke=ellipseStrokes[ellipseStrokes.Count-1];
			Stroke combinedStroke=new Stroke();
			combinedStroke.getStrokePoints().AddRange(lastStroke.getStrokePoints());
			combinedStroke.getStrokePoints().AddRange(stroke.getStrokePoints().GetRange(0,stroke.getStrokePoints().Count-1));
			
			List<Vector2> planePoints=new List<Vector2>();
		    List<Vector3> ellipsePoints=fitEllipse(combinedStroke.getStrokePoints(),ref planePoints);
			
			float eccentricity=FitStroke2Ellipse(planePoints);
			Debug.Log("eccentricity "+eccentricity);
			
			if(eccentricity>0.75)
				strokeType=StrokeType.None;
			
			return;
		}
		
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
	
	List<Vector3> fitEllipse(List<Vector3> strokePoints,ref List<Vector2> planePoints)
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

		//Get the first eigen vector
		double[,] firstVector={{eigenVectors[0,0]},{eigenVectors[1,0]},{eigenVectors[2,0]}};
		double[,] secondVector={{eigenVectors[0,1]},{eigenVectors[1,1]},{eigenVectors[2,1]}};
		
		//Project each point to the fitting line
		double[,]firstProjection=new double[strokePoints.Count,1];
		double[,]secondProjection=new double[strokePoints.Count,1];
		alglib.rmatrixgemm(strokePoints.Count,1,3,1,points,0,0,0,firstVector,0,0,0,0,ref firstProjection,0,0);
		alglib.rmatrixgemm(strokePoints.Count,1,3,1,points,0,0,0,secondVector,0,0,0,0,ref secondProjection,0,0);
		
		//Save projection points on the projection plane
		planePoints.Clear();
		for(int i=0;i<strokePoints.Count;++i)
		{
			planePoints.Add(new Vector2((float)firstProjection[i,0],(float)secondProjection[i,0]));
		}
		
		//Map each stroke point to a point on the fitting line
		double[,]resultPoint=new double[strokePoints.Count,3];
		for(int i=0;i<strokePoints.Count;++i)
		{
			resultPoint[i,0]=mean[0]+firstProjection[i,0]*firstVector[0,0]+secondProjection[i,0]*secondVector[0,0];
			resultPoint[i,1]=mean[1]+firstProjection[i,0]*firstVector[1,0]+secondProjection[i,0]*secondVector[1,0];
			resultPoint[i,2]=mean[2]+firstProjection[i,0]*firstVector[2,0]+secondProjection[i,0]*secondVector[2,0];
		}
		
		//Save points of the fitting line
		List<Vector3> projectionPoints=new List<Vector3>();
		
		for(int i=0;i<strokePoints.Count;++i)
		{
			projectionPoints.Add(new Vector3((float)resultPoint[i,0],(float)resultPoint[i,1],(float)resultPoint[i,2]));
		}
		
//		
//		cost=0;
//		for(int i=0;i<strokePoints.Count;++i)
//		{
//			cost+=Mathf.Pow(Vector3.Distance(strokePoints[i],linePoints[i]),2);
//		}
//		
//		cost/=2*strokePoints.Count;
		
		return projectionPoints;
	}
	
	float FitStroke2Ellipse (List<Vector2> strokePoints)
	{
		float eccentricity = 0;
		
		List<float> X = new List<float> ();
		List<float> Y = new List<float> ();
		
		if (strokePoints.Count < 5)
			return 0;
		
		foreach (Vector2 strokePoint in strokePoints) {
			float x = strokePoint.x;
			float y = strokePoint.y;
			
			X.Add (x);
			Y.Add (y);
		}
		
		float mean_x = mean (X);
		float mean_y = mean (Y);
		
		reduceMean (X, mean_x);
		reduceMean (Y, mean_y);
		
		double [,] D = new double[strokePoints.Count, 5];
		
		for (int i=0; i<D.GetLength(0); ++i) {
			D [i, 0] = (double)(X [i] * X [i]);
			D [i, 1] = (double)(X [i] * Y [i]);
			D [i, 2] = (double)(Y [i] * Y [i]);
			D [i, 3] = (double)(X [i]);
			D [i, 4] = (double)(Y [i]);
		}
		
		double [,] sumD = new double[1, 5];

		for (int j=0; j<sumD.GetLength(1); ++j) {
			for (int i=0; i<strokePoints.Count; ++i) {
				sumD [0, j] += D [i, j];
			}
		}
		
		double [,] S = new double[5, 5];
		alglib.rmatrixgemm (5, 5, strokePoints.Count, 1, D, 0, 0, 1, D, 0, 0, 0, 0, ref S, 0, 0);
		
		
		int info;
		alglib.matinvreport report;
		alglib.rmatrixinverse (ref S, out info, out report);
		
		double [,] A = new double[1, 5];
		alglib.rmatrixgemm (1, 5, 5, 1, sumD, 0, 0, 0, S, 0, 0, 0, 0, ref A, 0, 0);
		
		float a = (float)A [0, 0];
		float b = (float)A [0, 1];
		float c = (float)A [0, 2];
		float d = (float)A [0, 3];
		float e = (float)A [0, 4];
		
		float orientation_tolerance = 0.001f;
		
		float orientation_rad;
		float cos_phi;
		float sin_phi;
		
		if (Mathf.Min (Mathf.Abs (b / a), Mathf.Abs (b / c)) > orientation_tolerance) {
			orientation_rad = 0.5f * Mathf.Atan (b / (c - a));
			cos_phi = Mathf.Cos (orientation_rad);
			sin_phi = Mathf.Sin (orientation_rad);
			
			float atemp = a;
			float btemp = b;
			float ctemp = c;
			float dtemp = d;
			float etemp = e;
			
			a = atemp * cos_phi * cos_phi - btemp * cos_phi * sin_phi + ctemp * sin_phi * sin_phi;
			b = 0;
			c = atemp * sin_phi * sin_phi + btemp * cos_phi * sin_phi + ctemp * cos_phi * cos_phi;
			d = dtemp * cos_phi - etemp * sin_phi;
			e = dtemp * sin_phi + etemp * cos_phi;

			float mean_xtemp = mean_x;
			float mean_ytemp = mean_y;

			mean_x = cos_phi * mean_xtemp - sin_phi * mean_ytemp;
			mean_y = sin_phi * mean_xtemp + cos_phi * mean_ytemp;
		} else {
			orientation_rad = 0;
			cos_phi = Mathf.Cos (orientation_rad);
			sin_phi = Mathf.Sin (orientation_rad);
		}
		
		float test = a * c;
		
		if (test > 0) {
			if (a < 0) {
				a = -a;
				c = -c;
				d = -d;
				e = -e;
				
			}

			float X0 = mean_x - d / 2 / a;
			float Y0 = mean_y - e / 2 / c;
				
			float F = 1 + (d * d) / (4 * a) + (e * e) / (4 * c);
			a = Mathf.Sqrt (F / a);
			b = Mathf.Sqrt (F / c);

			float long_axis = 2 * Mathf.Max (a, b);
			float short_axis = 2 * Mathf.Min (a, b);
			
			float powB = Mathf.Pow ((0.5f * short_axis), 2);
			float powA = Mathf.Pow ((0.5f * long_axis), 2);
			
			eccentricity = Mathf.Sqrt (1 - powB / powA);
			
			float [,] R = new float[,]{
				{cos_phi,sin_phi},
				{-sin_phi,cos_phi}
			};
		}
		
		return eccentricity;
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
//		Debug.Log("first "+new Vector3((float)eigenVectors[0,0],(float)eigenVectors[1,0],(float)eigenVectors[2,0]));
//		Debug.Log("second "+new Vector3((float)eigenVectors[0,1],(float)eigenVectors[1,1],(float)eigenVectors[2,1]));
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
	
	private Vector3 meanSpeed;
	
	private Vector3 lastMeanSpeed;
	
	public float speedThreshold=120;
	//Finger stroke
	private List<Vector3> fingerPoints=new List<Vector3>();
	
	//Finger strokes
	private Stroke stroke=new Stroke();
	private List<Stroke> strokes=new List<Stroke>();
	private List<Stroke> ellipseStrokes=new List<Stroke>();
	
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
	
	float gain;
	
	float k=2/3;
	
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
		
		LineRenderer ellipseLine=GameObject.Find("EllipseLine").GetComponent<LineRenderer>();
		ellipseLine.material = new Material(Shader.Find("Particles/Additive"));
		ellipseLine.SetWidth(1,1);
		ellipseLine.SetColors(Color.blue,Color.blue);
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
