using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public class OneEuroFilter
	{
		public OneEuroFilter (float minCutoff, float beta)
		{
			firstTime = true;
			this.minCutoff = minCutoff;
			this.beta = beta;
			
			positionFilt = new LowPassFilter ();
			speedFilt = new LowPassFilter ();
			dcutoff = 1;
		}
		
		protected bool firstTime;
		protected float minCutoff;
		protected float beta;
		protected LowPassFilter positionFilt;
		protected LowPassFilter speedFilt;
		protected float dcutoff;
		
		public float MinCutoff {
			get { return minCutoff;}
			set { minCutoff = value;}
		}
		
		public float Beta {
			get { return beta;}
			set { beta = value;}
		}
		
		public Vector3 Filter (Vector3 rawPos, float rate)
		{
			Vector3 rawSpeed = new Vector3 ();
			if (firstTime)
				rawSpeed = new Vector3 (0, 0, 0);
			else
				rawSpeed = (rawPos - positionFilt.Last ()) * rate;
			
			if (firstTime)
				firstTime = false;
			
			Vector3 filterSpeed = speedFilt.Filter (rawSpeed, Alpha (rate, dcutoff));
			float cutoff = minCutoff + beta * filterSpeed.magnitude;
			
			return positionFilt.Filter (rawPos, Alpha (rate, cutoff));
		}
				
		protected float Alpha (float rate, float cutoff)
		{
			float tau = 1 / (2 * Mathf.PI * cutoff);
			float te = 1 / rate;
			return 1 / (1 + tau / te);
		}
	}
	
	public class LowPassFilter
	{
		public LowPassFilter ()
		{
			firstTime = true;
		}
		
		protected bool firstTime;
		protected Vector3 hatPrev;
		
		public Vector3 Last ()
		{
			return hatPrev;
		}
		
		public Vector3 Filter (Vector3 rawVector, float alpha)
		{
			Vector3 hat = new Vector3 (0, 0, 0);
			
			if (firstTime) {
				firstTime = false;
				hat = rawVector;
			} else
				hat = alpha * rawVector + (1 - alpha) * hatPrev;
			
			hatPrev = hat;
			
			return hatPrev;
		}
	}
}

