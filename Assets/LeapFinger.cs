using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

namespace AssemblyCSharp
{
	public class LeapFinger:LeapObject
	{
		//Leap finger object
		private Finger finger;
	
		//Finger sphere
		private GameObject fingerObject;
        
		//Finger id
		private int id;
	
		//Hand id
		private int handId;
	
		//Finger position
		private Vector3 position;
	
		//Finger last position
		private Vector3 lastPosition;
		
		//One euro filter 
		private OneEuroFilter oneEuroFilter;
		
		//Minimum cutoff frequence for one euro filter
		private const float minCutoff = 0.3f;
		
		//Beta for one euro filter
		private const float beta = 0.007f;
	
		//Finger speed
		private Vector3 speed;
	
		//Finger last speed
		private Vector3 lastSpeed;

		//Finger stroke
		private List<Vector3> fingerPoints = new List<Vector3> ();
	
		//Finger stroke
		private Stroke stroke = new Stroke ();
		
		
		//List of strokes
		private List<Stroke> strokes = new List<Stroke> ();

		//Finger line maxmimum point count
		private int fingerLineCount = 3000;
		
		//Time of the last frame in second
		float lastTime;
		
		//The gain for cycloPan
		float gain;
		
		//The ratio of cycloPan
		const float k = 2 / 3;
	
		//Projection center
		Vector3 projectionCenter = new Vector3 ();
	
		//Rotation center
		Vector2 rotationCenter = new Vector2 ();
	
		//Rotation radius
		float rotationRadius = 0;
	
		//Rotation axis
		Vector3 rotationAxis = new Vector3 ();
	
		//Rotation angle
		float rotationAngle;
	
		//Rotation direction
		bool clockWise = false;
		
		//Pendulum direction
		bool pendulumUp = false;
		
		//Pendulum center
		Vector3 pendulumCenter = new Vector3 ();
		List<Vector3> planeEigenVectors = new List<Vector3> ();
	
		//Last stroke direction
		Vector3 lastDirection = new Vector3 ();
		
		//Direction of cyclo pan gesutre
		Vector3 cycloDirection = new Vector3 ();
		List<Vector3> rawStroke = new List<Vector3> ();
		List<Vector3> filterStroke = new List<Vector3> ();
		int aaaIndex = 0;
			
		public LeapFinger (Finger finger)
		{
			this.finger = finger;
		
			oneEuroFilter = new OneEuroFilter (minCutoff, beta);
			
			fingerObject = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			fingerObject.transform.renderer.material.color = Color.white;
			fingerObject.transform.localScale = new Vector3 (10, 10, 10);
			

			
			position = new Vector3 (finger.TipPosition.x, finger.TipPosition.y, -finger.TipPosition.z);
			
			//One euro filter
			Vector3 filterPosition = oneEuroFilter.Filter (position, 0);
			//
			
			fingerObject.transform.position = position;

//			rawStroke.Add (position);
			filterStroke.Add (filterPosition);
			
			speed = new Vector3 (finger.TipVelocity.x, finger.TipVelocity.y, -finger.TipVelocity.z);
		
			id = finger.Id;
		
			if (finger.Hand.IsValid)
				handId = finger.Hand.Id;
			else
				handId = -1;

			stroke.setStartTime (Time.time);
			stroke.getStrokePoints ().Add (position);
			stroke.getStrokeSpeeds ().Add (speed);
			
			LineRenderer rawStrokeLine = GameObject.Find ("RawStroke").GetComponent<LineRenderer> ();
			rawStrokeLine.material = new Material (Shader.Find ("Particles/Additive"));
			rawStrokeLine.SetWidth (1, 1);
			rawStrokeLine.SetColors (Color.white, Color.white);
			LineRenderer oneEuroStrokeLine = GameObject.Find ("OneEuroStroke").GetComponent<LineRenderer> ();
			oneEuroStrokeLine.material = new Material (Shader.Find ("Particles/Additive"));
			oneEuroStrokeLine.SetWidth (1, 1);
			oneEuroStrokeLine.SetColors (Color.cyan, Color.cyan);
			
//			drawPoints (rawStrokeLine, rawStroke);
			drawPoints (oneEuroStrokeLine, filterStroke);
			
			strokeLine.transform.parent = fingerObject.transform;
			strokeLineRenderer = strokeLine.AddComponent<LineRenderer> ();
			strokeLineRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			strokeLineRenderer.SetWidth (1, 1);
			strokeLineRenderer.SetColors (Color.yellow, Color.yellow);
		
			eigenVectorLine.transform.parent = fingerObject.transform;
			eigenVectorRenderer = eigenVectorLine.AddComponent<LineRenderer> ();
			eigenVectorRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			eigenVectorRenderer.SetWidth (1, 1);
			eigenVectorRenderer.SetColors (Color.blue, Color.blue);
		
			curveLine.transform.parent = fingerObject.transform;
			curveRenderer = curveLine.AddComponent<LineRenderer> ();
			curveRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			curveRenderer.SetWidth (1, 1);
			curveRenderer.SetColors (Color.green, Color.green);
			
			panLine.transform.parent = fingerObject.transform;
			panRenderer = panLine.AddComponent<LineRenderer> ();
			panRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			panRenderer.SetWidth (1, 1);
			panRenderer.SetColors (Color.red, Color.red);
			
			lastTime = Time.time;
		}
	
		public Stroke getStroke ()
		{
			return stroke;
		}
	
		public void refreshFinger (Finger finger)
		{
			this.finger = finger;
			setPosition (finger.TipPosition);	
			setSpeed (finger.TipVelocity);
			
			
		}
	
		public void setPosition (Leap.Vector pos)
		{
			lastPosition = position;
			position = new Vector3 (pos.x, pos.y, -pos.z);
			fingerObject.transform.position = position;
		
			stroke.getStrokePoints ().Add (position);
			
//			rawStroke.Add (position);
			
//			LineRenderer rawStrokeLine = GameObject.Find ("RawStroke").GetComponent<LineRenderer> ();
//			drawPoints (rawStrokeLine, rawStroke);
			
			
//			lastPosition=position;
////			
//			position = new Vector3 (pos.x, pos.y, -pos.z);
//			float rate=1/(Time.time-lastTime);
//			Vector3 filterPosition=oneEuroFilter.Filter(position,rate);
////			fingerObject.transform.position=position;
//			
//			float distance=Vector3.Distance(filterPosition,stroke.getStrokePoints()[stroke.getStrokePoints().Count-1]);
//			if(distance>1)
//			{
//				position=filterPosition;
//			
//			}
//			else
//			{
//				position=stroke.getStrokePoints()[stroke.getStrokePoints().Count-1];
//			}
//			fingerObject.transform.position=position;
//			stroke.getStrokePoints().Add(position);
		}
	
		public void setSpeed (Leap.Vector velocity)
		{
			lastSpeed = speed;
			speed = new Vector3 (velocity.x, velocity.y, -velocity.z);
			stroke.getStrokeSpeeds ().Add (speed);
		}
	
		public Vector3 getPosition ()
		{
			return position;
		}
	
		public Vector3 getLastPosition ()
		{
			return lastPosition;
		}
	
		public Vector3 getSpeed ()
		{
			return speed;
		}
	
		public Vector3 getLastSpeed ()
		{
			return lastSpeed;
		}
	
		public int getId ()
		{
			return id;
		}
	
		public int getHandId ()
		{
			return handId;
		}

		public GameObject getFingerObject ()
		{
			return fingerObject;
		}
	
		public float getGain ()
		{
			return gain;
		}
	
		public Vector3 getDragDirection ()
		{
			return cycloDirection;
		}
	
		public Vector3 getLastDirection ()
		{
			return lastDirection;
		}
	
		public Vector3 getRotationAxis ()
		{
			return rotationAxis;
		}
	
		public float getRotationAngle ()
		{
			return rotationAngle;
		}
	
		public bool getRotationDirection ()
		{
			return clockWise;
		}
		
		public bool getPendulumDirection ()
		{
			return pendulumUp;
		}
	
		public Vector3 getPendulumCenter ()
		{
			return pendulumCenter;
		}
		
		public float getAngleVariation (out bool change)
		{
			bool strokeChange = false;
			change = strokeChange;
			
			int indexDifference = 3;
			
			float averageAngle = 0;
			
			int pointCount = stroke.getStrokePoints ().Count;
			;
			
			if (stroke.getStrokePoints ().Count < (1 + 2 * indexDifference))
				return 180;
			
			Vector3 center = new Vector3 ();
			List<Vector3> planeVectors = new List<Vector3> ();
			List<Vector2> planePoints = new List<Vector2> ();
			
			planeProjection (stroke.getStrokePoints ().GetRange (pointCount - 1 - 2 * indexDifference, 1 + 2 * indexDifference), ref center, ref planeVectors, ref planePoints);
				
			Vector2 point1 = planePoints [planePoints.Count - 1 - 2 * indexDifference];
			Vector2 point2 = planePoints [planePoints.Count - 1 - indexDifference];
			Vector2 point3 = planePoints [planePoints.Count - 1];
			
			Vector2 vec1 = point1 - point2;
			Vector2 vec2 = point3 - point2;
			
			float dot = Vector2.Dot (vec1, vec2) / (vec1.magnitude * vec2.magnitude);
			float angle = Mathf.Acos (dot) / Mathf.PI * 180;
			
			if (angle < 100) {
				if (pointCount - 1 - aaaIndex > indexDifference * 2 + 1) {
					aaaIndex = pointCount - 1;
					strokeChange = true;
					change = strokeChange;
				}
				
			}
			
			return angle;
			
			 
		}

		public bool checkCorner ()
		{
			bool corner = false;
			
			
			int indexDifference = 2;
			
			if (stroke.getStrokePoints ().Count < (1 + 2 * indexDifference))
				return corner;

			
			int pointCount = stroke.getStrokePoints ().Count;
			int startIndex = pointCount - 1 - 2 * indexDifference;
			
			Vector3 center = new Vector3 ();
			List<Vector3> planeVectors = new List<Vector3> ();
			List<Vector2> planePoints = new List<Vector2> ();
			
			planeProjection (stroke.getStrokePoints ().GetRange (startIndex, 1 + 2 * indexDifference), ref center, ref planeVectors, ref planePoints);
				
			Vector2 point1 = planePoints [planePoints.Count - 1 - 2 * indexDifference];
			Vector2 point2 = planePoints [planePoints.Count - 1 - indexDifference];
			Vector2 point3 = planePoints [planePoints.Count - 1];
			
			Vector2 lineCenter = new Vector2 ((point1.x + point3.x) / 2, (point1.y + point3.y) / 2);
			
			float distance1 = Vector2.Distance (point1, point3);
			float distance2 = Vector2.Distance (point2, lineCenter);
			
			if (distance1 < 2 * distance2) {
				rawStroke.Clear ();
				corner = true;
//				Debug.Log ("corner");
			}
			return corner;
		}
		
		bool checkCircle (out bool stop)
		{
			
			stop = false;
			
			if (stroke.getStrokePoints ().Count < 3) {
				return false;
			}
			
			Vector3 center = new Vector3 ();
			List<Vector3> planeVectors = new List<Vector3> ();
			List<Vector2> planePoints = new List<Vector2> ();
			
			planeProjection (stroke.getStrokePoints (), ref center, ref planeVectors, ref planePoints);
			
			int count = planePoints.Count;
			
			Vector2 circleCenter = new Vector2 ();
			float circleRadius = 0;
			
			FitStroke2Circle (planePoints, out circleCenter, out circleRadius);
			
			drawCircle (curveRenderer, circleCenter, circleRadius, center, planeVectors);
			
			float angle = 0;
			
			for (int i=0; i<count-1; ++i) {
				Vector2 point1 = planePoints [i];
				Vector2 point2 = planePoints [i + 1];
				
				Vector2 vec1 = point1 - circleCenter;
				Vector2 vec2 = point2 - circleCenter;
				
				float dot = Vector2.Dot (vec1, vec2) / (vec1.magnitude * vec2.magnitude);
				angle += Mathf.Acos (dot) / Mathf.PI * 180;		
			}
			
			
			if (angle >= 360) {
				Debug.Log ("Angle " + angle);
				projectionCenter = center;
				planeEigenVectors = planeVectors;
				rotationCenter = circleCenter;
				rotationRadius = circleRadius;
				
				string axis = checkRotationAxis (planeVectors);

				switch (axis) {
				case "X":
					rotationAxis = new Vector3 (1, 0, 0);
					break;
				case "Y":
					rotationAxis = new Vector3 (0, 1, 0);
					break;
				case "Z":
					rotationAxis = new Vector3 (0, 0, 1);
					break;
				}
				
				return true;
			} else {
				Vector3 lastEndPoint = stroke.getStrokePoints () [count - 2];
				Vector3 endPoint = stroke.getStrokePoints () [count - 1];
				float distance = Vector3.Distance (lastEndPoint, endPoint);
				
				if (distance < 0.5 && distance != 0)
					stop = true;
				return false;
			}
		}
		
		public void checkGesture ()
		{
			if (stroke.getStrokePoints ().Count < 2)
				return;
			
			float time = Time.time;
			
			Vector3 startPoint = stroke.getStrokePoints () [0];
			Vector3 lastEndPoint = stroke.getStrokePoints () [stroke.getStrokePoints ().Count - 2];
			Vector3 endPoint = stroke.getStrokePoints () [stroke.getStrokePoints ().Count - 1];
			
			Vector3 lastEndSpeed = stroke.getStrokeSpeeds () [stroke.getStrokeSpeeds ().Count - 2];
			Vector3 endSpeed = stroke.getStrokeSpeeds () [stroke.getStrokeSpeeds ().Count - 1];
			
			bool stop = false;
			if (strokeType == LeapObject.StrokeType.None || strokeType == LeapObject.StrokeType.Rotate) {
				if (checkCircle (out stop) == true) {
					stroke.setEndTime (lastTime);
				
					//Add stroke to the list
					Stroke strokeCopy = new Stroke (stroke);
					strokes.Add (strokeCopy);
					if (strokes.Count > 100)
						strokes.RemoveAt (0);
					//Regenerate the stroke
					stroke.getStrokePoints ().Clear ();
					stroke.getStrokePoints ().Add (lastEndPoint);
					stroke.getStrokePoints ().Add (endPoint);
					stroke.getStrokeSpeeds ().Add (lastEndSpeed);
					stroke.getStrokeSpeeds ().Add (endSpeed);
					stroke.setStartTime (lastTime);
				
					if (strokeType == LeapObject.StrokeType.None) {
						strokeType = LeapObject.StrokeType.Rotate;
					}
				} else {
					if (stop == true) {
						stroke.setEndTime (lastTime);
				
						//Add stroke to the list
						Stroke strokeCopy = new Stroke (stroke);
						strokes.Add (strokeCopy);
						if (strokes.Count > 100)
							strokes.RemoveAt (0);
						//Regenerate the stroke
						stroke.getStrokePoints ().Clear ();
						stroke.getStrokePoints ().Add (lastEndPoint);
						stroke.getStrokePoints ().Add (endPoint);
						stroke.getStrokeSpeeds ().Add (lastEndSpeed);
						stroke.getStrokeSpeeds ().Add (endSpeed);
						stroke.setStartTime (lastTime);
					
						strokeType = LeapObject.StrokeType.None;
					}
					
				}
			}
			
			
			if (checkCorner () == true) {
				stroke.setEndTime (lastTime);
				
				//Add stroke to the list
				Stroke strokeCopy = new Stroke (stroke);
				strokes.Add (strokeCopy);
				if (strokes.Count > 100)
					strokes.RemoveAt (0);
				//Regenerate the stroke
				stroke.getStrokePoints ().Clear ();
				stroke.getStrokePoints ().Add (lastEndPoint);
				stroke.getStrokePoints ().Add (endPoint);
				stroke.getStrokeSpeeds ().Add (lastEndSpeed);
				stroke.getStrokeSpeeds ().Add (endSpeed);
				stroke.setStartTime (lastTime);
				

				switch (strokeType) {
				case StrokeType.None:
					checkFirstStroke ();
					break;
				
				case StrokeType.Pan:
					checkDrag ();
					break;
				
				case StrokeType.WaitCurve:
					checkRotate ();
					break;
				
				case StrokeType.Pendulum:
					checkRotate ();
					break;
				}			
			}
			
			if (strokeType == StrokeType.Rotate) {
				getRotationInfo (lastEndPoint, endPoint);
			}
			lastTime = time;
		}
//		public void checkGesture ()
//		{
//			if (stroke.getStrokePoints ().Count < 2)
//				return;
//		
//			float time = Time.time;
//		
//			Vector3 startPoint = stroke.getStrokePoints () [0];
//			Vector3 lastEndPoint = stroke.getStrokePoints () [stroke.getStrokePoints ().Count - 2];
//			Vector3 endPoint = stroke.getStrokePoints () [stroke.getStrokePoints ().Count - 1];
//		    
//			Vector3 lastEndSpeed = stroke.getStrokeSpeeds () [stroke.getStrokeSpeeds ().Count - 2];
//			Vector3 endSpeed = stroke.getStrokeSpeeds () [stroke.getStrokeSpeeds ().Count - 1];
//
//			
//			if (Vector3.Distance (startPoint, lastEndPoint) > Vector3.Distance (startPoint, endPoint)) {
//				//Set the end time of the stroke
//				stroke.setEndTime (lastTime);
//			    Debug.Log(stroke.getStrokePoints().Count);
//				//Add the stroke to the stroke list
//
//				Stroke strokeCopy = new Stroke (stroke);
//				strokes.Add (strokeCopy);
//				if (strokes.Count > 100)
//					strokes.RemoveAt (0);
//				//Regenerate the stroke
//				stroke.getStrokePoints ().Clear ();
//				stroke.getStrokePoints ().Add (lastEndPoint);
//				stroke.getStrokePoints ().Add (endPoint);
//				stroke.getStrokeSpeeds ().Add (lastEndSpeed);
//				stroke.getStrokeSpeeds ().Add (endSpeed);
//				stroke.setStartTime (lastTime);
//			
//			    
//				panRenderer.SetVertexCount (0);
//				curveRenderer.SetVertexCount (0);
//				strokeLineRenderer.SetVertexCount (0);
//				
//				switch (strokeType) {
//				case StrokeType.None:
//					checkFirstStroke ();
//					break;
//				
//				case StrokeType.Pan:
//					checkDrag ();
//					break;
//				
//				case StrokeType.WaitCurve:
//					checkRotate ();
//					break;
//				
//				case StrokeType.Rotate:
//					checkRotate ();
//					break;
//				}
//			}
//		
//
//			if (strokeType == StrokeType.Rotate) {
//				getRotationInfo (lastEndPoint, endPoint);
//			}
//		
//			lastTime = time;
//		
//		}
	
		public void translateObject (GameObject object_translation)
		{

			float motion = (position - lastPosition).magnitude;
			Vector3 dragDirection = cycloDirection;
		
			float xAbs = Mathf.Abs (dragDirection.x);
			float yAbs = Mathf.Abs (dragDirection.y);
			float zAbs = Mathf.Abs (dragDirection.z);
		
			if (zAbs == Mathf.Max (xAbs, Mathf.Max (yAbs, zAbs))) {
				dragDirection.x = 0;
				dragDirection.y = 0;
			} else
				dragDirection.z = 0;
			//		dragDirection.z=0;
			object_translation.transform.position += dragDirection * motion * gain / 10;
		}
	
		public void getRotationInfo (Vector3 lastPoint, Vector3 point)
		{
			//Project points to the projection plane
			Vector2 lastProjectionPoint = getPlaneProjection (lastPoint, projectionCenter, planeEigenVectors);
			Vector2 projectionPoint = getPlaneProjection (point, projectionCenter, planeEigenVectors);
		
		
//fsetpo
		
			//Get projected 3D points
			Vector3 lastPoint3D = projectionCenter + lastProjectionPoint.x * planeEigenVectors [0] + lastProjectionPoint.y * planeEigenVectors [1];
			Vector3 center3D = projectionCenter + rotationCenter.x * planeEigenVectors [0] + rotationCenter.y * planeEigenVectors [1];
			Vector3 point3D = projectionCenter + projectionPoint.x * planeEigenVectors [0] + projectionPoint.y * planeEigenVectors [1];
			
			//Vector from the circle center to the stroke point
			Vector3 lastVector3D = lastPoint3D - center3D;
			Vector3 vector3D = point3D - center3D;
			
			//Get the movement of the stroke
			float distance = Vector3.Distance (lastPoint3D, point3D);
		
			//Get the rotationAngle
			float ratio = 0.1f;
			float rotationAngle = distance / rotationRadius / Mathf.PI * 180 * ratio;
		
			//Get the cross vector 
			Vector3 crossVector = Vector3.Cross (lastVector3D, vector3D);
			
			bool clockWise = false;
		
			float absX = Mathf.Abs (crossVector.x);
			float absY = Mathf.Abs (crossVector.y);
			float absZ = Mathf.Abs (crossVector.z);
		
			if (absX == Mathf.Max (absX, Mathf.Max (absY, absZ))) {
				if (crossVector.x > 0)
					clockWise = false;
				else
					clockWise = true;
			}
		
			if (absY == Mathf.Max (absX, Mathf.Max (absY, absZ))) {
				if (crossVector.y > 0)
					clockWise = false;
				else
					clockWise = true;
			}
		
			if (absZ == Mathf.Max (absX, Mathf.Max (absY, absZ))) {
				if (crossVector.z > 0)
					clockWise = false;
				else
					clockWise = true;
			}
		
			this.clockWise = clockWise;
			this.rotationAxis = rotationAxis;
			this.rotationAngle = rotationAngle;
		}
	
		public void rotateObject (GameObject object_rotation, Vector3 lastPoint, Vector3 point)
		{
			//Project points to the projection plane
			Vector2 lastProjectionPoint = getPlaneProjection (lastPoint, projectionCenter, planeEigenVectors);
			Vector2 projectionPoint = getPlaneProjection (point, projectionCenter, planeEigenVectors);
		
		
			LineRenderer rotationLine = GameObject.Find ("RotationLine").GetComponent<LineRenderer> ();
			rotationLine.SetVertexCount (3);
			rotationLine.SetPosition (0, projectionCenter + lastProjectionPoint.x * planeEigenVectors [0] + lastProjectionPoint.y * planeEigenVectors [1]);
			rotationLine.SetPosition (1, projectionCenter + rotationCenter.x * planeEigenVectors [0] + rotationCenter.y * planeEigenVectors [1]);
			rotationLine.SetPosition (2, projectionCenter + projectionPoint.x * planeEigenVectors [0] + projectionPoint.y * planeEigenVectors [1]);
			rotationLine.SetColors (Color.red, Color.green);

			
			GameObject cube = GameObject.Find ("Cube");
		
			//Get projected 3D points
			Vector3 lastPoint3D = projectionCenter + lastProjectionPoint.x * planeEigenVectors [0] + lastProjectionPoint.y * planeEigenVectors [1];
			Vector3 center3D = projectionCenter + rotationCenter.x * planeEigenVectors [0] + rotationCenter.y * planeEigenVectors [1];
			Vector3 point3D = projectionCenter + projectionPoint.x * planeEigenVectors [0] + projectionPoint.y * planeEigenVectors [1];
			
			//Vector from the circle center to the stroke point
			Vector3 lastVector3D = lastPoint3D - center3D;
			Vector3 vector3D = point3D - center3D;
			
			//Get the movement of the stroke
			float distance = Vector3.Distance (lastPoint3D, point3D);
		
			//Get the rotationAngle
			float ratio = 0.1f;
			float rotationAngle = distance / rotationRadius / Mathf.PI * 180 * ratio;
		
			//Get the cross vector 
			Vector3 crossVector = Vector3.Cross (lastVector3D, vector3D);
			
			bool clockWise = false;
		
			float absX = Mathf.Abs (crossVector.x);
			float absY = Mathf.Abs (crossVector.y);
			float absZ = Mathf.Abs (crossVector.z);
		
			if (absX == Mathf.Max (absX, Mathf.Max (absY, absZ))) {
				if (crossVector.x > 0)
					clockWise = false;
				else
					clockWise = true;
			}
		
			if (absY == Mathf.Max (absX, Mathf.Max (absY, absZ))) {
				if (crossVector.y > 0)
					clockWise = false;
				else
					clockWise = true;
			}
		
			if (absZ == Mathf.Max (absX, Mathf.Max (absY, absZ))) {
				if (crossVector.z > 0)
					clockWise = false;
				else
					clockWise = true;
			}
		
			if (clockWise == false) {
				cube.transform.Rotate (rotationAxis, rotationAngle, Space.World);
			} else {
				cube.transform.Rotate (rotationAxis, -rotationAngle, Space.World);
			}
		}
	
		public Vector2 getPlaneProjection (Vector3 point, Vector3 projectionCenter, List<Vector3>planeVectors)
		{
			Vector3 translatedPoint = point - projectionCenter;
			Vector2 projectionPoint = new Vector2 ();
			projectionPoint.x = Vector3.Dot (translatedPoint, planeVectors [0]);
			projectionPoint.y = Vector3.Dot (translatedPoint, planeVectors [1]);
		
			return projectionPoint;
		}
	
		public void checkFirstStroke ()
		{
			//Fit projections points to an ellipse
			Vector2 ellipseCenter = new Vector2 ();
			float ellipseA = 0;
			float ellipseB = 0;
			float[,] rotationMatrix = new float[2, 2];
		
			Vector3 center = new Vector3 ();
			List<Vector3> planeVectors = new List<Vector3> ();
			List<Vector2> planePoints = new List<Vector2> ();
		
			Stroke lastStroke = strokes [strokes.Count - 1];
			//Fit the stroke to a plane
			List<Vector3> projectionPoints = planeProjection (lastStroke.getStrokePoints (), ref center, ref planeVectors, ref planePoints);
			
			//Fit the stroke to an ellipse
			float eccentricity = FitStroke2Ellipse (planePoints, out ellipseCenter, out ellipseA, out ellipseB, out rotationMatrix);
			
			Vector3 startPoint = lastStroke.getStrokePoints () [0];
			Vector3 endPoint = lastStroke.getStrokePoints () [lastStroke.getStrokePoints ().Count - 1];
			float distance = Vector3.Distance (startPoint, endPoint);
			
			float averageSpeed = distance / (lastStroke.getEndTime () - lastStroke.getStartTime ());
		
			Vector3 strokeVector = new Vector3 ();
			float cost = 0;
			fitLine (lastStroke.getStrokePoints (), out strokeVector, out cost);
		
			if (cost < 80) {
				if (eccentricity > 0.85 || eccentricity == 0) {
					if (averageSpeed > 60 && distance < 120) {
						cycloDirection = endPoint - startPoint;
						cycloDirection.Normalize ();
						strokeType = StrokeType.Pan;
					

						drawPoints (panRenderer, projectionPoints);
				
						lastDirection = cycloDirection;
						Debug.Log ("drag start id " + this.id);
					} 
				}
			} else {
				if (ellipseA > 4 && ellipseB > 4) {		
				
					strokeType = LeapObject.StrokeType.WaitCurve;
					Debug.Log ("Wait curve");
//				}
				}
			
			
//			if (eccentricity > 0.85 || eccentricity == 0) {
//				if (averageSpeed > 60 && distance < 120) {
//					cycloDirection = endPoint - startPoint;
//					cycloDirection.Normalize ();
//					strokeType = StrokeType.Pan;
//					
//
//					drawPoints (panRenderer, projectionPoints);
//				
//					lastDirection = cycloDirection;
//					Debug.Log ("drag start id " + this.id);
//				} else {
////				Debug.Log("drag not start id "+this.id+" speed "+averageSpeed+" distance "+distance);
//				}
//			}
//					
//			if (eccentricity < 0.85 && eccentricity != 0) {
//			
//				if (ellipseA > 4 && ellipseB > 4) {		
//				
//					strokeType = LeapObject.StrokeType.WaitCurve;
////				strokeType = StrokeType.WaitRotate;
//					Debug.Log ("Wait curve");
//				}		
//			}	
			}
		}
	
		public void checkDrag ()
		{
			Stroke stroke1 = strokes [strokes.Count - 2];
			Stroke stroke2 = strokes [strokes.Count - 1];
			
			Vector3 direction1;
			Vector3 direction2;
			
			float cost1;
			float cost2;
			List<Vector3> fitPoints1 = fitLine (stroke1.getStrokePoints (), out direction1, out cost1);
			List<Vector3> fitPoints2 = fitLine (stroke2.getStrokePoints (), out direction2, out cost2);
			
			List<Vector3> combinedPoints = new List<Vector3> ();
			combinedPoints.AddRange (fitPoints1);
			combinedPoints.AddRange (fitPoints2);
			

			drawPoints (panRenderer, combinedPoints);
			
			Vector3 v1 = fitPoints1 [0] - fitPoints1 [fitPoints1.Count - 2];
			Vector3 v2 = fitPoints2 [fitPoints2.Count - 1] - fitPoints2 [0];
			float dot = Vector3.Dot (v1, v2) / (v1.magnitude * v2.magnitude);
			float angle = Mathf.Acos (dot) / Mathf.PI * 180;
			
			float distance = Vector3.Distance (fitPoints2 [fitPoints2.Count - 1], fitPoints2 [0]);
			float averageSpeed = distance / (stroke2.getEndTime () - stroke2.getStartTime ());
			
			if (angle > 30) {
//			Debug.Log("stop angle "+angle);
				strokeType = StrokeType.None;
				return;
			}
			
			if (averageSpeed < 30) {
//			Debug.Log("stop speed "+averageSpeed);
				strokeType = StrokeType.None;
				return;
			}
			if (distance < 2) {
//			Debug.Log("stop distance "+distance);
				strokeType = StrokeType.None;
				return;
			}
			
			float mean_frequence = 0;
			float frequenceCount = 0;
			for (int i=strokes.Count-1; i>=0; --i) {
				float frequence = 1 / (2 * (strokes [i].getEndTime () - strokes [i].getStartTime ()));
				mean_frequence += frequence;
				frequenceCount++;
				if (frequenceCount == 3)
					break;
			}
					
			mean_frequence /= frequenceCount;
					
			gain = mean_frequence * 2 / 3;
//		Debug.Log("mean f "+mean_frequence+" gain "+gain);
		
			lastDirection = direction2;
			return;		
		}
	
		public void checkCircleOrPendulum (Stroke stroke1, Stroke stroke2)
		{	
			Vector3 center1 = new Vector3 ();
			List<Vector3> planeVectors1 = new List<Vector3> ();
			List<Vector2> planePoints1 = new List<Vector2> ();
		
			Vector3 center2 = new Vector3 ();
			List<Vector3> planeVectors2 = new List<Vector3> ();
			List<Vector2> planePoints2 = new List<Vector2> ();
		
			List<Vector3> projectionPoints1 = planeProjection (stroke1.getStrokePoints (), ref center1, ref planeVectors1, ref planePoints1);
			List<Vector3> projectionPoints2 = planeProjection (stroke2.getStrokePoints (), ref center2, ref planeVectors2, ref planePoints2);
		
			int count1 = stroke1.getStrokePoints ().Count;
			int count2 = stroke2.getStrokePoints ().Count;
		
			Vector3 startPoint3D1 = projectionPoints1 [0];
			Vector3 startPoint3D2 = projectionPoints2 [0];
			Vector3 centerPoint3D1 = projectionPoints1 [count1 - 1];
			Vector3 centerPoint3D2 = projectionPoints2 [count2 - 1];
		
			Vector3 vec1 = startPoint3D1 - center1;
			Vector3 vec2 = startPoint3D2 - center2;
			Vector3 vec3 = centerPoint3D1 - center1;
			Vector3 vec4 = centerPoint3D2 - center2;
		
			Vector3 vertical1 = Vector3.Cross (vec1, vec3);
			Vector3 vertical2 = Vector3.Cross (vec2, vec4);
		
//		Debug.Log("vertical1 "+vertical1.z+" vertical2 "+vertical2.z);
		}
		
		public void checkRotate ()
		{
			//Fit projections points to an ellipse
			Vector2 ellipseCenter = new Vector2 ();
			float ellipseA = 0;
			float ellipseB = 0;
			float[,] rotationMatrix = new float[2, 2];
		
			Vector3 center = new Vector3 ();
			List<Vector3> planeVectors = new List<Vector3> ();
			List<Vector2> planePoints = new List<Vector2> ();
		
			float eccentricity = 0;
		
			List<Vector3> combinedPoints = new List<Vector3> ();
			combinedPoints.AddRange (strokes [strokes.Count - 2].getStrokePoints ());
			combinedPoints.AddRange (strokes [strokes.Count - 1].getStrokePoints ());
			
			List<Vector3> projectionPoints = planeProjection (combinedPoints, ref center, ref planeVectors, ref planePoints);
			
			//Fit the stroke to an ellipse
			eccentricity = FitStroke2Ellipse (planePoints, out ellipseCenter, out ellipseA, out ellipseB, out rotationMatrix);
			
			drawEllipse (curveRenderer, ellipseCenter, ellipseA, ellipseB, rotationMatrix, center, planeVectors);
	
			List<Vector3> list1 = strokes [strokes.Count - 2].getStrokePoints ();
			List<Vector3> list2 = strokes [strokes.Count - 1].getStrokePoints ();
			
			float xd1 = list1 [0].x - list1 [list1.Count - 1].x;
			float xd2 = list2 [0].x - list2 [list2.Count - 1].x;
			
			Vector3 strokeCenter2 = list2 [(int)Mathf.Floor (list2.Count / 2)];
			
			float heightDifference = list2 [0].y - strokeCenter2.y;
			
			Debug.Log ("xd1 " + xd1 + " xd2 " + xd2);
	      
			if (strokeType == StrokeType.WaitCurve) {

				if (ellipseA > 4 && ellipseB > 4) {	
					if ((xd1 > 0 && xd2 < 0) || (xd1 < 0 && xd2 > 0)) {
						if (heightDifference > 0) {
							Debug.Log ("U UP");
							pendulumUp = true;
						} else {
							Debug.Log ("U DOWN");
							pendulumUp = false;
						}
						
						pendulumCenter = center + ellipseCenter.x * planeVectors [0] + ellipseCenter.y * planeVectors [1];
						Debug.Log ("U start ");
						strokeType = StrokeType.Pendulum;	
					} else {
						Debug.Log ("U not start cross ");
						strokeType = StrokeType.None;
					}
				} else {
					Debug.Log ("Rotate not start axis length");
					strokeType = StrokeType.None;
				}
				
			}
		
			if (strokeType == StrokeType.Pendulum) {
				//Fit two continuous strokes to an circle
			
			
				projectionCenter = center;
				planeEigenVectors = planeVectors;
			


				
			
//				if (cross1.z * cross2.z < 0) {
				if ((xd1 > 0 && xd2 < 0) || (xd1 < 0 && xd2 > 0)) {
					if (heightDifference > 0) {
						Debug.Log ("U UP");
						pendulumUp = true;
					} else {
						Debug.Log ("U DOWN");
						pendulumUp = false;
					}
					pendulumCenter = center + ellipseCenter.x * planeVectors [0] + ellipseCenter.y * planeVectors [1];
					Debug.Log ("U continue");
				} else {
					Debug.Log ("U stop");
					strokeType = LeapObject.StrokeType.None;
				}
			
			}
		

//		//Draw eigen vectors

//		drawEigenVectors(center,planeVectors);
		

//			drawPoints (strokeLineRenderer, strokes [strokes.Count - 1].getStrokePoints ());
	
		}
		
//		public void checkRotate ()
//		{
//			//Fit projections points to an ellipse
//			Vector2 ellipseCenter = new Vector2 ();
//			float ellipseA = 0;
//			float ellipseB = 0;
//			float[,] rotationMatrix = new float[2, 2];
//		
//			Vector3 center = new Vector3 ();
//			List<Vector3> planeVectors = new List<Vector3> ();
//			List<Vector2> planePoints = new List<Vector2> ();
//		
//			float eccentricity = 0;
//		
//            if (strokeType == StrokeType.WaitCurve)  {
////			if (strokeType == StrokeType.WaitRotate) {
//				//Fit two continuous strokes to an ellipse
//				List<Vector3> combinedPoints = new List<Vector3> ();
//				combinedPoints.AddRange (strokes [strokes.Count - 2].getStrokePoints ());
//				combinedPoints.AddRange (strokes [strokes.Count - 1].getStrokePoints ());
//			
//				List<Vector3> projectionPoints = planeProjection (combinedPoints, ref center, ref planeVectors, ref planePoints);
//			
//				//Fit the stroke to an ellipse
//				eccentricity = FitStroke2Ellipse (planePoints, out ellipseCenter, out ellipseA, out ellipseB, out rotationMatrix);
//			
//				drawEllipse (curveRenderer, ellipseCenter, ellipseA, ellipseB, rotationMatrix, center, planeVectors);
//			
//
//			
//			
////				Debug.Log ("eccetricity " + eccentricity);
//				if (eccentricity < 0.85 && eccentricity != 0) {
//					if (ellipseA > 4 && ellipseB > 4) {	
////					Debug.Log("Rotate start ");
//						strokeType = StrokeType.Rotate;
//					} else {
////					Debug.Log("Rotate not start axis");
//						strokeType = StrokeType.None;
//					}
//				} else {
////				Debug.Log("Rotate not start "+eccentricity);
//					strokeType = StrokeType.None;
//				}
//			}
//		
//			if (strokeType == StrokeType.Rotate) {
//				//Fit two continuous strokes to an circle
//				List<Vector3> combinedPoints = new List<Vector3> ();
//				combinedPoints.AddRange (strokes [strokes.Count - 2].getStrokePoints ());
//				combinedPoints.AddRange (strokes [strokes.Count - 1].getStrokePoints ());
//			
//				List<Vector3> projectionPoints = planeProjection (combinedPoints, ref center, ref planeVectors, ref planePoints);
//			
//				projectionCenter = center;
//				planeEigenVectors = planeVectors;
//			
//				//Fit the stroke to an circle
//				Vector2 circleCenter = new Vector2 ();
//				float circleRadius = 0;
//				FitStroke2Circle (planePoints, out circleCenter, out circleRadius);
//			
//			
//				string axis = checkRotationAxis (planeVectors);
//
//				switch (axis) {
//				case "X":
//					rotationAxis = new Vector3 (1, 0, 0);
//					break;
//				case "Y":
//					rotationAxis = new Vector3 (0, 1, 0);
//					break;
//				case "Z":
//					rotationAxis = new Vector3 (0, 0, 1);
//					break;
//				}
//			
//				if (circleRadius < 4 || circleRadius > 80) {
//					strokeType = StrokeType.None;
////				Debug.Log("Stop "+circleRadius);
//				} else {
//					rotationCenter = circleCenter;
//					rotationRadius = circleRadius;
//				
//					//Draw circle
//					drawCircle (curveRenderer, circleCenter, circleRadius, center, planeVectors);
//				}
//			
//			}
//		
//
////		//Draw eigen vectors
//
////		drawEigenVectors(center,planeVectors);
//		
//
//			drawPoints (strokeLineRenderer, strokes [strokes.Count - 1].getStrokePoints ());
//	
//		}
	
		public string checkRotationAxis (List<Vector3> planeVectors)
		{
			string axis = "";
			Vector3 verticalVector = Vector3.Cross (planeVectors [0], planeVectors [1]);
			if (Mathf.Abs (verticalVector.x) > Mathf.Abs (verticalVector.y)
			&& Mathf.Abs (verticalVector.x) > Mathf.Abs (verticalVector.z)) {
				axis = "X";
			}
			
			if (Mathf.Abs (verticalVector.y) > Mathf.Abs (verticalVector.x)
			&& Mathf.Abs (verticalVector.y) > Mathf.Abs (verticalVector.z)) {
				axis = "Y";
			}
			
			if (Mathf.Abs (verticalVector.z) > Mathf.Abs (verticalVector.x)
			&& Mathf.Abs (verticalVector.z) > Mathf.Abs (verticalVector.y)) {
				axis = "Z";
			}
		
			return axis;
		}
	
		public void drawEigenVectors (Vector3 center, List<Vector3> planeVectors)
		{
			//Draw eigen vectors
			LineRenderer eigenVectorLine = GameObject.Find ("EigenVectorLine").GetComponent<LineRenderer> ();
			eigenVectorLine.SetVertexCount (3);
			eigenVectorLine.SetPosition (0, (center + 30 * planeVectors [0]));
			eigenVectorLine.SetPosition (1, center);
			eigenVectorLine.SetPosition (2, (center + 30 * planeVectors [1]));
		}
	
		public void drawCircle (LineRenderer circleRenderer, Vector2 circleCenter, float radius, Vector3 center, List<Vector3> planeVectors)
		{
			List<Vector2> circle2DPoints = new List<Vector2> ();
			for (int i=0; i<360; ++i) {
				Vector2 circlePoint = new Vector2 (circleCenter.x + radius * Mathf.Cos (i * Mathf.PI / 180), circleCenter.y + radius * Mathf.Sin (i * Mathf.PI / 180));				
				circle2DPoints.Add (circlePoint);
			}
		
			List<Vector3> circle3DPoints = new List<Vector3> ();
			foreach (Vector2 point2D in circle2DPoints) {
				Vector3 point3D = center + planeVectors [0] * point2D.x + planeVectors [1] * point2D.y;
				circle3DPoints.Add (point3D);
			}
		
			
			circleRenderer.SetVertexCount (circle3DPoints.Count);
			for (int i=0; i<circle3DPoints.Count; ++i) {
				circleRenderer.SetPosition (i, circle3DPoints [i]);
			}
		}
	
		public void drawEllipse (LineRenderer ellipseRenderer, Vector2 ellipseCenter, float ellipseA, float ellipseB, float[,]rotationMatrix, Vector3 center, List<Vector3>planeVectors)
		{
			List<Vector2> ellipse2DPoints = new List<Vector2> ();
			for (int i=0; i<360; ++i) {
				Vector2 initialPoint = new Vector2 (ellipseCenter.x + ellipseA * Mathf.Cos (i * Mathf.PI / 180), ellipseCenter.y + ellipseB * Mathf.Sin (i * Mathf.PI / 180));				
				Vector2 rotatedPoint = new Vector2 (rotationMatrix [0, 0] * initialPoint.x + rotationMatrix [0, 1] * initialPoint.y, rotationMatrix [1, 0] * initialPoint.x + rotationMatrix [1, 1] * initialPoint.y);
				ellipse2DPoints.Add (rotatedPoint);
			}
			
			List<Vector3> ellipse3DPoints = new List<Vector3> ();
			foreach (Vector2 point2D in ellipse2DPoints) {
				Vector3 point3D = center + planeVectors [0] * point2D.x + planeVectors [1] * point2D.y;
				ellipse3DPoints.Add (point3D);
			}
			
			
			ellipseRenderer.SetVertexCount (ellipse3DPoints.Count);
			for (int i=0; i<ellipse3DPoints.Count; ++i) {
				ellipseRenderer.SetPosition (i, ellipse3DPoints [i]);
			}
		}


	

	}
}

