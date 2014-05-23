using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

namespace AssemblyCSharp
{
	public class LeapObject
	{
		public enum StrokeType
		{
			WaitCurve,
			Pendulum,
			Pan,
			WaitRotate,
			Rotate,
			None
		};
	
		protected GameObject strokeLine = new GameObject ();
		protected LineRenderer strokeLineRenderer;
		
		protected GameObject eigenVectorLine = new GameObject ();
		protected LineRenderer eigenVectorRenderer;
		
		protected GameObject curveLine = new GameObject ();
		protected LineRenderer curveRenderer;
		
		protected GameObject panLine = new GameObject ();
		protected LineRenderer panRenderer;
		
		public double [] findMean (double [,] points)
		{
			int rows = points.GetLength (0);
			int columns = points.GetLength (1);
		
			double [] mean = new double[columns];
		
			for (int i=0; i<rows; ++i) {
				for (int j=0; j<columns; ++j) {
					mean [j] += points [i, j];
				}
			}
		
			for (int i=0; i<columns; ++i) {
				mean [i] /= rows;
			}
		
			return mean;
		}
	
		public void minus (ref double[,]points, double[] mean)
		{
			int rows = points.GetLength (0);
			int columns = points.GetLength (1);
		
			for (int i=0; i<rows; ++i) {
				for (int j=0; j<columns; ++j) {
					points [i, j] -= mean [j];
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
	
		public void drawPoints (LineRenderer line, List<Vector3> strokePoints)
		{	
			line.SetVertexCount (strokePoints.Count);
			for (int i=0; i<strokePoints.Count; ++i) {
				line.SetPosition (i, strokePoints [i]);
			}
		}
	
		public void drawPoints (LineRenderer line, List<Vector3> lastStrokePoints, List<Vector3> strokePoints)
		{
			line.SetVertexCount (lastStrokePoints.Count + strokePoints.Count);
			for (int i=0; i<lastStrokePoints.Count; ++i) {
				line.SetPosition (i, lastStrokePoints [i]);
			}
		
			for (int i=0; i<strokePoints.Count; ++i) {
				line.SetPosition (i + lastStrokePoints.Count, strokePoints [i]);
			}
		}
	
		public StrokeType getStrokeType ()
		{
			return strokeType;
		}
	
		public void setStrokeType (StrokeType type)
		{
			strokeType = type;
		}
		
		public List<Vector3> planeProjection (List<Vector3> strokePoints, ref Vector3 center, ref List<Vector3> planeVectors, ref List<Vector2> planePoints)
		{
			double [,] points = new double [strokePoints.Count, 3];
		
			//Save stroke points
			for (int i=0; i<strokePoints.Count; ++i) {
				points [i, 0] = strokePoints [i].x;
				points [i, 1] = strokePoints [i].y;
				points [i, 2] = strokePoints [i].z;
			}
		
			//find the mean point
			double [] mean = findMean (points);
			center = new Vector3 ((float)mean [0], (float)mean [1], (float)mean [2]);
		
			//minus the mean point
			minus (ref points, mean);
		
			//use PCA to find the eigen vectors and the eigen values
			int info;
			double [,] eigenVectors = new double[3, 3];
			double [] eigenValues = new double[3];
		
			alglib.pcabuildbasis (points, strokePoints.Count, 3, out info, out eigenValues, out eigenVectors);

			//Get the first eigen vector
			double[,] firstVector = {{eigenVectors [0, 0]},{eigenVectors [1, 0]},{eigenVectors [2, 0]}};
			double[,] secondVector = {{eigenVectors [0, 1]},{eigenVectors [1, 1]},{eigenVectors [2, 1]}};
		
			planeVectors.Clear ();
			planeVectors.Add (new Vector3 ((float)eigenVectors [0, 0], (float)eigenVectors [1, 0], (float)eigenVectors [2, 0]));
			planeVectors.Add (new Vector3 ((float)eigenVectors [0, 1], (float)eigenVectors [1, 1], (float)eigenVectors [2, 1]));
			planeVectors [0].Normalize ();
			planeVectors [1].Normalize ();
		
			//Project each point to the fitting line
			double[,] firstProjection = new double[strokePoints.Count, 1];
			double[,] secondProjection = new double[strokePoints.Count, 1];
			alglib.rmatrixgemm (strokePoints.Count, 1, 3, 1, points, 0, 0, 0, firstVector, 0, 0, 0, 0, ref firstProjection, 0, 0);
			alglib.rmatrixgemm (strokePoints.Count, 1, 3, 1, points, 0, 0, 0, secondVector, 0, 0, 0, 0, ref secondProjection, 0, 0);
		
			//Save projection points on the projection plane
			planePoints.Clear ();
			for (int i=0; i<strokePoints.Count; ++i) {
				planePoints.Add (new Vector2 ((float)firstProjection [i, 0], (float)secondProjection [i, 0]));
			}
		
			//Map each stroke point to a point on the fitting line
			double[,] resultPoint = new double[strokePoints.Count, 3];
			for (int i=0; i<strokePoints.Count; ++i) {
				resultPoint [i, 0] = mean [0] + firstProjection [i, 0] * firstVector [0, 0] + secondProjection [i, 0] * secondVector [0, 0];
				resultPoint [i, 1] = mean [1] + firstProjection [i, 0] * firstVector [1, 0] + secondProjection [i, 0] * secondVector [1, 0];
				resultPoint [i, 2] = mean [2] + firstProjection [i, 0] * firstVector [2, 0] + secondProjection [i, 0] * secondVector [2, 0];
			}
		
			//Save points of the fitting line
			List<Vector3> projectionPoints = new List<Vector3> ();
		
			for (int i=0; i<strokePoints.Count; ++i) {
				projectionPoints.Add (new Vector3 ((float)resultPoint [i, 0], (float)resultPoint [i, 1], (float)resultPoint [i, 2]));
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
		
		public float FitStroke2Ellipse (List<Vector2> strokePoints, out Vector2 center, out float valueA, out float valueB, out float[,] rotationMatrix)
		{
			float eccentricity = 0;
		
			List<float> X = new List<float> ();
			List<float> Y = new List<float> ();
		
			if (strokePoints.Count < 5) {
				center = new Vector2 (0, 0);
				valueA = 0;
				valueB = 0;
				rotationMatrix = new float[2, 2];
				return 0;
			}
			
		
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
		
			float X0 = 0;
			float Y0 = 0;
		
			if (test > 0) {
				if (a < 0) {
					a = -a;
					c = -c;
					d = -d;
					e = -e;
				
				}

				X0 = mean_x - d / 2 / a;
				Y0 = mean_y - e / 2 / c;
				
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
		
			center = new Vector2 (X0, Y0);
			valueA = a;
			valueB = b;
		
			rotationMatrix = new float[2, 2];
			rotationMatrix [0, 0] = cos_phi;
			rotationMatrix [0, 1] = sin_phi;
			rotationMatrix [1, 0] = -sin_phi;
			rotationMatrix [1, 1] = cos_phi;
		
			return eccentricity;
		}
		
		public void FitStroke2Circle (List<Vector2> strokePoints, out Vector2 center, out float circleRadius)
		{
			List<float> X = new List<float> ();
			List<float> Y = new List<float> ();
		
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
			
			double [,] D = new double[strokePoints.Count, 3];
		
			for (int i=0; i<D.GetLength(0); ++i) {
				D [i, 0] = (double)(X [i] * X [i] + Y [i] * Y [i]);
				D [i, 1] = (double)(X [i]);
				D [i, 2] = (double)(Y [i]);

			}
		
			double [,] sumD = new double[1, 3];
			for (int j=0; j<sumD.GetLength(1); ++j) {
				for (int i=0; i<strokePoints.Count; ++i) {
					sumD [0, j] += D [i, j];
				}
			}
		
			double [,] S = new double[3, 3];
			alglib.rmatrixgemm (3, 3, strokePoints.Count, 1, D, 0, 0, 1, D, 0, 0, 0, 0, ref S, 0, 0);
		
			int info;
			alglib.matinvreport report;
			alglib.rmatrixinverse (ref S, out info, out report);
		
			double [,] A = new double[1, 3];
			alglib.rmatrixgemm (1, 3, 3, 1, sumD, 0, 0, 0, S, 0, 0, 0, 0, ref A, 0, 0);
		
			float a = (float)A [0, 0];
			float b = (float)A [0, 1];
			float c = (float)A [0, 2];
		
			float radius = Mathf.Sqrt (1 / a);
			float X0 = mean_x - b / (2 * a);
			float Y0 = mean_y - c / (2 * a);

			center = new Vector2 (X0, Y0);
			circleRadius = radius;
		}
		
		public List<Vector3> fitLine (List<Vector3> strokePoints, out Vector3 strokeVector, out float cost)
		{
			double [,] points = new double [strokePoints.Count, 3];
		
			//Save stroke points
			for (int i=0; i<strokePoints.Count; ++i) {
				points [i, 0] = strokePoints [i].x;
				points [i, 1] = strokePoints [i].y;
				points [i, 2] = strokePoints [i].z;
			}
		
			//find the mean point
			double [] mean = findMean (points);
		
			//minus the mean point
			minus (ref points, mean);
		
			//use PCA to find the eigen vectors and the eigen values
			int info;
			double [,] eigenVectors = new double[3, 3];
			double [] eigenValues = new double[3];
		
			alglib.pcabuildbasis (points, strokePoints.Count, 3, out info, out eigenValues, out eigenVectors);
//		Debug.Log(eigenValues[0]+" "+eigenValues[1]+" "+eigenValues[2]);
//		Debug.Log("first "+new Vector3((float)eigenVectors[0,0],(float)eigenVectors[1,0],(float)eigenVectors[2,0]));
//		Debug.Log("second "+new Vector3((float)eigenVectors[0,1],(float)eigenVectors[1,1],(float)eigenVectors[2,1]));
			//Get the first eigen vector
			double[,] lineVector = {{eigenVectors [0, 0]},{eigenVectors [1, 0]},{eigenVectors [2, 0]}};
		
			//Project each point to the fitting line
			double[,] projection = new double[strokePoints.Count, 1];
			alglib.rmatrixgemm (strokePoints.Count, 1, 3, 1, points, 0, 0, 0, lineVector, 0, 0, 0, 0, ref projection, 0, 0);
		
			//Map each stroke point to a point on the fitting line
			double[,] resultPoint = new double[strokePoints.Count, 3];
			for (int i=0; i<strokePoints.Count; ++i) {
				resultPoint [i, 0] = mean [0] + projection [i, 0] * lineVector [0, 0];
				resultPoint [i, 1] = mean [1] + projection [i, 0] * lineVector [1, 0];
				resultPoint [i, 2] = mean [2] + projection [i, 0] * lineVector [2, 0];
			}
		
			//Save points of the fitting line
			List<Vector3> linePoints = new List<Vector3> ();
		
			for (int i=0; i<strokePoints.Count; ++i) {
				linePoints.Add (new Vector3 ((float)resultPoint [i, 0], (float)resultPoint [i, 1], (float)resultPoint [i, 2]));
			}
		
			//Save the stroke vector
			strokeVector = linePoints [strokePoints.Count - 1] - linePoints [0];
			strokeVector.Normalize ();
		
			cost = 0;
			for (int i=0; i<strokePoints.Count; ++i) {
				cost += Mathf.Pow (Vector3.Distance (strokePoints [i], linePoints [i]), 2);
			}
		
			cost /= 2 * strokePoints.Count;
		
			return linePoints;
		}
		
		protected StrokeType strokeType = StrokeType.None;
		

	}
	
	
}

