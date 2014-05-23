using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

namespace AssemblyCSharp
{
	public class LeapHand: LeapObject
	{
		public LeapHand (Hand hand)
		{
			handObject = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			handObject.transform.renderer.material.color = Color.white;
			handObject.transform.localScale = new Vector3 (10, 10, 10);
			handObject.transform.position = new Vector3 (hand.PalmPosition.x, hand.PalmPosition.y, -hand.PalmPosition.z);
		
			//Save id
			id = hand.Id;
		
			//Save spee
			speed = new Vector3 (hand.PalmVelocity.x, hand.PalmVelocity.y, -hand.PalmVelocity.z);
		
			//Save hand point to the trajectory
			handPoints.Add (handObject.transform.position);
		
//		//Line renderer of hand trajectory
//		handLine=handObject.AddComponent<LineRenderer>();
//		handLine.material = new Material(Shader.Find("Particles/Additive"));
//		handLine.SetWidth(1,1);
//		handLine.SetColors(Color.yellow,Color.yellow);
			
			strokeLine.transform.parent = handObject.transform;
			strokeLineRenderer = strokeLine.AddComponent<LineRenderer> ();
			strokeLineRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			strokeLineRenderer.SetWidth (1, 1);
			strokeLineRenderer.SetColors (Color.yellow, Color.yellow);
		
			eigenVectorLine.transform.parent = handObject.transform;
			eigenVectorRenderer = eigenVectorLine.AddComponent<LineRenderer> ();
			eigenVectorRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			eigenVectorRenderer.SetWidth (1, 1);
			eigenVectorRenderer.SetColors (Color.blue, Color.blue);
		
			curveLine.transform.parent = handObject.transform;
			curveRenderer = curveLine.AddComponent<LineRenderer> ();
		    curveRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			curveRenderer.SetWidth (1, 1);
			curveRenderer.SetColors (Color.green, Color.green);
			
			panLine.transform.parent = handObject.transform;
			panRenderer = panLine.AddComponent<LineRenderer> ();
			panRenderer.material = new Material (Shader.Find ("Particles/Additive"));
			panRenderer.SetWidth (1, 1);
			panRenderer.SetColors (Color.red, Color.red);
		}
	
		public void refreshHand (Hand hand)
		{
			setPosition (hand.PalmPosition);
			setSpeed (hand.PalmVelocity);
		}
	
		public void setPosition (Leap.Vector position)
		{
			handObject.transform.position = new Vector3 (position.x, position.y, -position.z);
			handPoints.Add (handObject.transform.position);
		}
	
		public void setSpeed (Leap.Vector velocity)
		{
			speed = new Vector3 (velocity.x, velocity.y, -velocity.z);
		}

		public Vector3 getPosition ()
		{
			return handObject.transform.position;
		}
	
		public Vector3 getSpeed ()
		{
			return speed;
		}

		public int getId ()
		{
			return id;
		}

		public GameObject getHandObject ()
		{
			return handObject;
		}
	
		public List<Vector3> getHandTrajectory ()
		{
			return handPoints;
		}
	
		public void drawTrajectory ()
		{
			if (handPoints.Count < handLineCount) {
			
				handLine.SetVertexCount (handPoints.Count);
		
				for (int i=0; i<handPoints.Count; ++i) {
					handLine.SetPosition (i, handPoints [i]);
				}
			} else {
				handLine.SetVertexCount (handLineCount);
				for (int i=handPoints.Count-1; i>handPoints.Count-handLineCount-1; --i) {
					handLine.SetPosition (handPoints.Count - i - 1, handPoints [i]);
				}	
			}
		
		}
	
		private GameObject handObject;
		private int id;
		private Vector3 speed;
		private List<Vector3> handPoints = new List<Vector3> ();
		private LineRenderer handLine;
		private int handLineCount = 30;
	}
}

