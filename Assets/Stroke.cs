using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

namespace AssemblyCSharp
{
	public class Stroke
	{
		public Stroke ()
		{
		
		}
	
		public Stroke (List<Vector3> stroke)
		{
			foreach (Vector3 strokePoint in stroke) {
				strokePoints.Add (strokePoint);
			}	
		}
		
		public Stroke (Stroke strokeCopy)
		{
			foreach (Vector3 strokePoint in strokeCopy.getStrokePoints()) {
				strokePoints.Add (strokePoint);
			}
			
			foreach (Vector3 strokeSpeed in strokeCopy.getStrokeSpeeds()) {
				strokeSpeeds.Add (strokeSpeed);
			}
			
			startTime=strokeCopy.getStartTime();
			endTime=strokeCopy.getEndTime();
		}
	
		public List<Vector3> getStrokePoints ()
		{
			return strokePoints;
		}
	
		public List<Vector3> getStrokeSpeeds ()
		{
			return strokeSpeeds;
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
		List<Vector3> strokePoints = new List<Vector3> ();
	
		//List of speed
		List<Vector3> strokeSpeeds = new List<Vector3> ();
		//Start time of the stroke
		float startTime;
		//End time of the stroke
		float endTime;
	}
}

