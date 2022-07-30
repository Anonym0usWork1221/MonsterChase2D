// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond

using UnityEngine;

namespace ControlFreak2
{

public static class CFUtils
	{
	public const float
		SqrtOf2			= 1.4142135623730950488016887242097f,
		SqrtOf3			= 1.7320508075688772935274463415059f,
		OneOverSqrtOf2	= 0.70710678118654752440084436210485f,
		OneOverSqrtOf3	= 0.57735026918962576450914878050196f,
		TanLenFor45Tri	= 0.7653668647301795434569199680608f,
		TanLenFor90Tri	= SqrtOf2;
	public const float	
		MAX_DELTA_TIME = 0.5f;
	private const float 
		MAX_LEFT_FACTOR = 0.75f;




	// -----------------------
	static public float realDeltaTime 
		{ get { return ((Time.captureFramerate <= 0) ? Time.unscaledDeltaTime : (1.0f / (float)Time.captureFramerate)); } }	

	// -----------------
	static public float realDeltaTimeClamped
		{ get { return Mathf.Min(CFUtils.realDeltaTime, MAX_DELTA_TIME); } }	
		
		
#if UNITY_EDITOR
	// ------------------------
	static public bool editorStopped { get { return !UnityEditor.EditorApplication.isPlaying; }  }
#else
	static public bool editorStopped { get { return false; } }
#endif		 


	// ------------------
#if CF_FORCE_MOBILE_MODE
	static public bool forcedMobileModeEnabled { get { return true; } }
#else	
	static public bool forcedMobileModeEnabled { get { return false; } }
#endif	


	// ------------------------
	static public string LogPrefixFull()
		{
#if UNITY_EDITOR
		return ("[" + (editorStopped ? "STOP" : "PLAY") + "][" + Time.frameCount + "] ");
#else
		return ("[" + Time.frameCount + "] ");
#endif
		}

	// ------------------------
	static public string LogPrefix()
		{
		return ("[" + Time.frameCount + "] ");
		}


	// ------------------
	static public float ApplyDeltaInput(float accum, float delta)
		{
		if ((accum >= 0) && (delta >= 0))
			return Mathf.Max(accum, delta);
		else if ((accum < 0) && (delta < 0))
			return Mathf.Min(accum, delta);
		else
			return (accum + delta);
		}
			
	// -----------------
	static public int ApplyDeltaInputInt(int accum, int delta)
		{
		if ((accum >= 0) && (delta >= 0))
			return Mathf.Max(accum, delta);
		else if ((accum < 0) && (delta < 0))
			return Mathf.Min(accum, delta);
		else
			return (accum + delta);
		}
		
	
	// ------------------
	static public float ApplyPositveDeltaInput(float positiveAccum, float delta)
		{
		return (((delta > 0) && (delta > positiveAccum)) ? delta : positiveAccum);
		}


	// ------------------
	static public float ApplyNegativeDeltaInput(float negativeAccum, float delta)
		{
		return (((delta < 0) && (delta < negativeAccum)) ? delta : negativeAccum);
		}



	// --------------------	
	static public void ApplySignedDeltaInput(float v, ref float plusAccum, ref float minusAccum)	
		{
		if (v >= 0)
			{
			if (v > plusAccum)
				plusAccum = v;
			}
		else if (v < minusAccum)
			minusAccum = v;
		}
		
	// --------------------	
	static public void ApplySignedDeltaInputInt(int v, ref int plusAccum, ref int minusAccum)	
		{
		if (v >= 0)
			{
			if (v > plusAccum)
				plusAccum = v;
			}
		else if (v < minusAccum)
			minusAccum = v;
		}
		
		

	// ---------------------
	static public int GetScrollValue(float drag, int prevScroll, float thresh, float magnet)
		{
		bool flippedAxis = (drag < 0);

				
		if (flippedAxis)
			{
			drag = -drag;	
			prevScroll = -prevScroll;
			}

		float scroll = drag / thresh;
		int scrollInt = Mathf.FloorToInt(scroll);	
		
		return (flippedAxis ? -scrollInt : scrollInt);
		}



	// ----------------
	static public float MoveTowards(float a, float b, float secondsPerUnit, float deltaTime, float epsilon)
		{
		if (Mathf.Abs(b - a) <= epsilon)
			return b; 
		return ((secondsPerUnit < 0.001f) || (secondsPerUnit <= deltaTime)) ? b : Mathf.MoveTowards(a, b, (deltaTime / secondsPerUnit)); 
		}


	// ----------------
	static private float GetLerpFactor(float smoothingTime, float deltaTime, float maxLerpFactor)
		{
		return Mathf.Min(maxLerpFactor,  ((smoothingTime <= deltaTime) ? 1 : (deltaTime / smoothingTime)));
		}


	// ----------------
	static public float SmoothTowards(float a, float b, float smoothingTime, float deltaTime, float epsilon, float maxLeftFactor = MAX_LEFT_FACTOR)
		{
		if (Mathf.Abs(b - a) <= epsilon)
			return b; 
		return ((smoothingTime < 0.001f)) ? b : Mathf.Lerp(a, b, GetLerpFactor(smoothingTime, deltaTime, maxLeftFactor)); 
		}

	// ----------------
		static public float SmoothTowardsAngle(float a, float b, float smoothingTime, float deltaTime, float epsilon, float maxLeftFactor = MAX_LEFT_FACTOR)
		{
		if (Mathf.Abs(Mathf.DeltaAngle(b, a)) <= epsilon)
			return b; 
		return ((smoothingTime < 0.001f)) ? b : 
			Mathf.MoveTowardsAngle(a, b, Mathf.Abs(b - a) * GetLerpFactor(smoothingTime, deltaTime, maxLeftFactor)); 
		}




	static public Vector2 SmoothTowardsVec2(Vector2 a, Vector2 b, float smoothingTime, float deltaTime, float sqrEpsilon, float maxLeftFactor = MAX_LEFT_FACTOR)
		{
		if ((sqrEpsilon != 0) && ((b - a).sqrMagnitude <= sqrEpsilon))
			return b;

		return ((smoothingTime < 0.001f)) ? b : Vector2.Lerp(a, b, GetLerpFactor(smoothingTime, deltaTime, maxLeftFactor)); 

		}
		

	static public Vector3 SmoothTowardsVec3(Vector3 a, Vector3 b, float smoothingTime, float deltaTime, float sqrEpsilon, float maxLeftFactor = MAX_LEFT_FACTOR)
		{
		if ((sqrEpsilon != 0) && ((b - a).sqrMagnitude <= sqrEpsilon))
			return b;

		return ((smoothingTime < 0.001f)) ? b : Vector3.Lerp(a, b, GetLerpFactor(smoothingTime, deltaTime, maxLeftFactor));
		}

	static public Color SmoothTowardsColor(Color a, Color b, float smoothingTime, float deltaTime, float maxLeftFactor = MAX_LEFT_FACTOR)
		{ return ((smoothingTime < 0.001f)) ? b : Color.Lerp(a, b, GetLerpFactor(smoothingTime, deltaTime, maxLeftFactor)); }
		
	static public Quaternion SmoothTowardsQuat(Quaternion a, Quaternion b, float smoothingTime, float deltaTime, float maxLeftFactor = MAX_LEFT_FACTOR)
		{ return ((smoothingTime < 0.001f)) ? b : Quaternion.Slerp(a, b, GetLerpFactor(smoothingTime, deltaTime, maxLeftFactor)); }
	


	// --------------------
	static public float SmoothDamp(float valFrom, float valTo, ref float vel, float smoothingTime, float deltaTime, float epsilon)
		{
		if ((smoothingTime < 0.001f) || (Mathf.Abs(valTo - valFrom) <= epsilon))
			{
			vel = 0;
			return valTo;
			}

		return Mathf.SmoothDamp(valFrom, valTo, ref vel, smoothingTime, 10000000, deltaTime);
		}
		

	// ----------------------
	static public Color ScaleColorAlpha(Color color, float alphaScale)
		{
		color.a *= alphaScale;
		return color;
		}
		

	// -----------------
	static public Component GetComponentHereOrInParent(Component comp, System.Type compType)
		{
		if (comp == null) 
			return null;
			
		Component c = null;
		return (((c = comp.GetComponent(compType)) != null) ? c : comp.GetComponentInParent(compType));
		}

		

	// -----------------------
	static public bool IsStretchyRectTransform(Transform tr)
		{
		RectTransform rectTr = (tr as RectTransform);
		return ((rectTr != null) && (rectTr.anchorMax != rectTr.anchorMin));
		}
	

	// ----------------------
	static public Rect TransformRect(Rect r, Matrix4x4 tr, bool round)
		{
		Bounds bb = TransformRectAsBounds(r, tr, round);

		Vector3 min = bb.min;		
		Vector3 size = bb.size;

		return new Rect(min.x, min.y, size.x, size.y); 
		}
		
		
	// ------------------------
	static public Bounds TransformRectAsBounds(Rect r, Matrix4x4 tr, bool round)
		{
		Vector3 cen = tr.MultiplyPoint3x4(r.center);
		Vector3 vx = tr.MultiplyVector(new Vector3(1, 0, 0));
		Vector3 vy = tr.MultiplyVector(new Vector3(0, 1, 0));

		float w = r.width * 0.5f;
		float h = r.height * 0.5f;
			
		vx *= w;
		vy *= h;


		Vector3 min, max;
		Vector3 v;

		if (round)
			{
	
			// 4 points of a 180 degree slice...
			
			// Up and down.

			v = vy;
			min = Vector3.Min(v, -v); 
			max = Vector3.Max(v, -v);

			// Up-right and down-left...
			v = ((vx * 0.77f) + (vy * 0.77f));
			min = Vector3.Min(min, Vector3.Min (v, -v));
			max = Vector3.Max(max, Vector3.Max (v, -v));
				
			// Left and right...
			v = vx;
			min = Vector3.Min(min, Vector3.Min (v, -v));
			max = Vector3.Max(max, Vector3.Max(v, -v));

			
			// Up-left and down-right...
			v = ((vx * -0.77f) + (vy * 0.77f));
			min = Vector3.Min(min, Vector3.Min (v, -v));
			max = Vector3.Max(max, Vector3.Max(v, -v));
			}
		else
			{
			// 2 corners and their mirrors...
				
			// Up-right and down-left...

			v = vx + vy;
			min = Vector3.Min(v, -v); 
			max = Vector3.Max(v, -v);

			// Down-right and up-left...
			
			v = vx - vy;
			min = Vector3.Min(min, Vector3.Min (v, -v));
			max = Vector3.Max(max, Vector3.Max(v, -v));
			}

		return new Bounds(cen + ((min + max) * 0.5f), (max - min)); //new Bounds.(cen, size);
		}



	// ------------------------
	static public Matrix4x4 ChangeMatrixTranl(Matrix4x4 m, Vector3 newTransl)
		{
		m.SetColumn(3, new Vector4(newTransl.x, newTransl.y, newTransl.z, m.m33));
		return m;
		}



	// -----------------
	static public Bounds GetWorldAABB(Matrix4x4 tf, Vector3 center, Vector3 size)
		{
		Vector3 a = center - (size * 0.5f);
		Vector3 b = center + (size * 0.5f);

		Vector3 min, max, v;
 
		v = tf.MultiplyPoint3x4(new Vector3(a.x, a.y, a.z));	min = max = v;
		v = tf.MultiplyPoint3x4(new Vector3(a.x, a.y, b.z));	min = Vector3.Min(min, v); max = Vector3.Max(max, v);
		v = tf.MultiplyPoint3x4(new Vector3(a.x, b.y, a.z));	min = Vector3.Min(min, v); max = Vector3.Max(max, v);
		v = tf.MultiplyPoint3x4(new Vector3(a.x, b.y, b.z));	min = Vector3.Min(min, v); max = Vector3.Max(max, v);
		v = tf.MultiplyPoint3x4(new Vector3(b.x, a.y, a.z));	min = Vector3.Min(min, v); max = Vector3.Max(max, v);
		v = tf.MultiplyPoint3x4(new Vector3(b.x, b.y, a.z));	min = Vector3.Min(min, v); max = Vector3.Max(max, v);
		v = tf.MultiplyPoint3x4(new Vector3(b.x, a.y, b.z));	min = Vector3.Min(min, v); max = Vector3.Max(max, v);
		v = tf.MultiplyPoint3x4(new Vector3(b.x, b.y, b.z));	min = Vector3.Min(min, v); max = Vector3.Max(max, v);
		
		return new Bounds((min + max) * 0.5f, (max - min));
		}
		

	// --------------
	static public Bounds GetWorldAABB(Matrix4x4 tf, Bounds localBounds)
		{	
		return GetWorldAABB(tf, localBounds.center, localBounds.size);
		}


	// ---------------------
	static public Rect GetWorldRect(Matrix4x4 tf, Bounds localBounds)
		{
		Bounds worldBounds = GetWorldAABB(tf, localBounds);	

		Vector3 min = worldBounds.min;
		Vector3 max = worldBounds.max;

		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		}

		

	// ----------------------
	static public Vector2 ClampRectInside(Rect rect, bool rectIsRound, Rect limiterRect, bool limiterIsRound)
		{
		if (limiterIsRound)
			{
			if (rectIsRound)
				return ClampEllipseInsideEllipse(rect, limiterRect);
			else
				return ClampRectInsideEllipse(rect, limiterRect);
			}
		else
			{
			return ClampRectInsideRect(rect, limiterRect);
			}
		} 
		

	// ------------------
	static public Vector2 ClampRectInsideEllipse(Rect rect, Rect limiterRect)
		{
		return ClampEllipseInsideEllipse(rect, limiterRect);		// TODO!!
		}

	// ------------------
	static public Vector2 ClampEllipseInsideEllipse(Rect rect, Rect limiterRect)
		{
		Vector2 rectRad		= rect.size * 0.5f;
		Vector2 limiterRad 	= limiterRect.size * 0.5f;
		Vector2 rectCen		= rect.center;
		Vector2 limiterCen	= limiterRect.center;
	

		if ((rectRad.x >= limiterRad.x) || (rectRad.y >= limiterRad.y))
			{
			Vector2 finalOfs;

			// Clamped is bigger on X axis...

			if (rectRad.x >= limiterRad.x)
				{
				finalOfs.x = (limiterCen.x - rectCen.x);
				}
			else
				{
				finalOfs.x = ((rectCen.x >= limiterCen.x) ? 
					Mathf.Min(0, ((limiterCen.x + limiterRad.x) - (rectCen.x + rectRad.x))) :
					Mathf.Max(0, ((limiterCen.x - limiterRad.x) - (rectCen.x - rectRad.x))) );
				}
				
			// Clamped is bigger on the Y axis..
 
			if (rectRad.y >= limiterRad.y)
				{
				finalOfs.y = (limiterCen.y - rectCen.y);
				}
			else
				{
				finalOfs.y = ((rectCen.y >= limiterCen.y) ? 
					Mathf.Min(0, ((limiterCen.y + limiterRad.y) - (rectCen.y + rectRad.y))) :
					Mathf.Max(0, ((limiterCen.y - limiterRad.y) - (rectCen.y - rectRad.y))) );
				}	
			
			return finalOfs;
			}


		Vector2	radDiff 	= (limiterRad - rectRad);	
		Vector2 originalOfs	= (rectCen - limiterCen);
		Vector2 ofs			= originalOfs;
			
		ofs.x /= radDiff.x;
		ofs.y /= radDiff.y;
			
		if (ofs.sqrMagnitude < 1)
			return Vector2.zero;

		ofs.Normalize();
		ofs.x *= radDiff.x;
		ofs.y *= radDiff.y;
			
		return (ofs - originalOfs);
		}


	// -----------------------
	static public Vector2 ClampRectInsideRect(Rect rect, Rect limiterRect)
		{
		Vector2 cen0 = rect.center;
		Vector2 cen1 = limiterRect.center;
		Vector2 rad0 = rect.size * 0.5f;
		Vector2 rad1 = limiterRect.size * 0.5f;

		Vector2 
			min0 = (cen0 - rad0), 
			max0 = (cen0 + rad0),
			min1 = (cen1 - rad1), 
			max1 = (cen1 + rad1);				
	
		Vector2 finalOfs = Vector2.zero;

		if (rad0.x >= rad1.x)
			{
			finalOfs.x = (cen1.x - cen0.x);
			}
		else
			{
			if (max0.x > max1.x)
				finalOfs.x = (max1.x - max0.x);
			else if (min0.x < min1.x)
				finalOfs.x = (min1.x - min0.x);
			}
			
		if (rad0.y >= rad1.y)
			{
			finalOfs.y = (cen1.y - cen0.y);
			}
		else
			{
			if (max0.y > max1.y)
				finalOfs.y = (max1.y - max0.y); 
			else if (min0.y < min1.y)
				finalOfs.y = (min1.y - min0.y);
			}

		return finalOfs;
		}

	// ----------------------
	// Unit Cicrle - rad = 1.0
	// ---------------------- 
	static public Vector2 ClampInsideUnitCircle(Vector2 np)
		{
		return ((np.sqrMagnitude < 1) ? np : np.normalized); 
		}


	// --------------------
	// Unit Square - min = -1, max = 1
	// --------------------
	static public Vector2 ClampInsideUnitSquare(Vector2 np)
		{
		float t = 1;
		if ((np.x > 1) || (np.x < -1)) t = Mathf.Abs(1.0f / np.x);
		if ((np.y > 1) || (np.y < -1)) t = Mathf.Min(t, Mathf.Abs(1.0f / np.y));
		return ((t < 1) ? (np * t) : np);
		}

	// --------------------
	// Unit Square - min = -1, max = 1
	// --------------------
	static public Vector2 ClampPerAxisInsideUnitSquare(Vector2 np)
		{
		np.x = Mathf.Clamp(np.x, -1, 1);
		np.y = Mathf.Clamp(np.y, -1, 1);
		return np;
		}

	// ------------------
	static public Vector2 ClampInsideRect(Vector2 v, Rect r)
		{
		if (r.Contains(v))
			return v;

		Vector2 cen = r.center;
		Vector2 rad = r.size * 0.5f;
		
		v -= cen;

		float t = 1;

		if ((v.x > rad.x) || (v.x < -rad.x)) t = Mathf.Abs(rad.x / v.x);
		if ((v.y > rad.y) || (v.y < -rad.y)) t = Mathf.Min(t, Mathf.Abs(rad.y / v.y));

		return (cen + (v * t));
		}

		


	// ---------------------------
	/// \name Utils
	/// \{
	// ---------------------------

	// ---------------
	/// Return true if given direction is one of diagonal directions.
	// ---------------
	static public bool IsDirDiagonal(
		Dir dir		///< Direction code.
		)
		{
		return ((((int)dir - (int)Dir.U) & 1) == 1);
		}



	// ---------------
	/// Return positive angle for given normalized vector. Angles start from top position and go clockwise.
	// --------------- 
	static public float VecToAngle(Vector2 vec)
		{
		return NormalizeAnglePositive(Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg);
		}

	// ---------------
	/// Return positive angle for given unnormalized vector. Angles start from top position and go clockwise.
	// --------------- 
	static public float VecToAngle(Vector2 vec, float defaultAngle, float deadZoneSq)
		{
		float m = vec.sqrMagnitude;
		if (m < deadZoneSq)
			return defaultAngle;
	
		if (Mathf.Abs(m - 1.0f) > 0.0001f)
			vec.Normalize();			

		return NormalizeAnglePositive(Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg);
		}


	// -----------------
	/// Return angle in degrees for given direction code.
	// -----------------
	static public float DirToAngle(
		Dir d	///< Direction code.
		)
		{		
		if ((d < Dir.U) || (d > Dir.UL))
			return 0;
	
		return ((float)((int)d - (int)Dir.U) * 45.0f);	
		}

	// ----------------
	//! Get nearest direction for given angle.
	// ----------------
	static public Dir DirFromAngle(
		float 	ang,			//!< Angle in degrees 
		bool 		as8way		//!< If true nearest of 8-way directions will be returned, otherwise one of major 4-way directions.
		)	
		{
		ang += (as8way ? 22.5f : 45.0f);
		ang = NormalizeAnglePositive(ang);
		
		Dir dir = Dir.N;

		if (as8way)
			{
			if 		(ang < 45)	dir = Dir.U;
			else if (ang < 90)	dir = Dir.UR;
			else if (ang < 135)	dir = Dir.R;
			else if (ang < 180)	dir = Dir.DR;
			else if (ang < 225)	dir = Dir.D;
			else if (ang < 270)	dir = Dir.DL;
			else if (ang < 315)	dir = Dir.L;
			else 						dir = Dir.UL;
			}
		else
			{
			if 		(ang < 90)	dir = Dir.U;
			else if (ang < 180)	dir = Dir.R;
			else if (ang < 270)	dir = Dir.D;
			else 						dir = Dir.L;
			}

		return dir;
		}
			

	// ----------------
	//! Get nearest direction for given angle with respect to last direction.
	// ----------------
	static public Dir DirFromAngleEx(
		float 	ang,			//!< Angle in degrees 
		bool 		as8way,		//!< If true nearest of 8-way directions will be returned, otherwise one of major 4-way directions.
		Dir		lastDir,		//!< Last direction.
		float		magnetPow	//!< Normalized angular magnet power.
		)	
		{
		if (lastDir != Dir.N && (magnetPow > 0.001f))
			{
			if (Mathf.Abs(Mathf.DeltaAngle(ang, CFUtils.DirToAngle(lastDir))) < 
				((1.0f + (Mathf.Clamp01(magnetPow) * 0.5f)) * (as8way ? 22.5f : 45.0f)))
				return lastDir;
			}

		return DirFromAngle(ang, as8way);
		}

	
	// ---------------------
	static public int DirDeltaAngle(Dir dirFrom, Dir dirTo)
		{
		if ((dirFrom == Dir.N) || (dirTo == Dir.N))
			return 0;
		
		return Mathf.RoundToInt(Mathf.DeltaAngle(DirToAngle(dirFrom), DirToAngle(dirTo))); 
		}


	// -------------------------		
	static public Vector2 DirToNormal(Dir dir)
		{
		switch (dir)
			{
			case Dir.U	: return new Vector2(0, 1);
			case Dir.R	: return new Vector2(1, 0);
			case Dir.D	: return new Vector2(0, -1);
			case Dir.L	: return new Vector2(-1, 0);
			case Dir.UR	: return new Vector2(OneOverSqrtOf2, OneOverSqrtOf2);
			case Dir.DR	: return new Vector2(OneOverSqrtOf2, -OneOverSqrtOf2);
			case Dir.DL	: return new Vector2(-OneOverSqrtOf2, -OneOverSqrtOf2);
			case Dir.UL	: return new Vector2(-OneOverSqrtOf2, OneOverSqrtOf2);
			}

		return Vector2.zero;
		}


	// -------------------------		
	static public Vector2 DirToTangent(Dir dir)
		{
		switch (dir)
			{
			case Dir.U	: return new Vector2(1, 0);
			case Dir.R	: return new Vector2(0, -1);
			case Dir.D	: return new Vector2(-1, 0);
			case Dir.L	: return new Vector2(0, 1);
			case Dir.UR	: return new Vector2(OneOverSqrtOf2, -OneOverSqrtOf2);
			case Dir.DR	: return new Vector2(-OneOverSqrtOf2, -OneOverSqrtOf2);
			case Dir.DL	: return new Vector2(-OneOverSqrtOf2, OneOverSqrtOf2);
			case Dir.UL	: return new Vector2(OneOverSqrtOf2, OneOverSqrtOf2);
			}

		return Vector2.zero;
		}


	// -----------------------
	static public Dir GetOppositeDir(Dir dir)
		{
		switch (dir)
			{
			case Dir.U	: return Dir.D;
			case Dir.UR	: return Dir.DL;
			case Dir.R	: return Dir.L;
			case Dir.DR	: return Dir.UL;
			case Dir.D	: return Dir.U;
			case Dir.DL	: return Dir.UR;
			case Dir.L	: return Dir.R;
			case Dir.UL	: return Dir.DR;
			}

		return Dir.N;
		}

 


	// -----------------------
	static public Vector2 DirToVector(Dir dir, bool circular)
		{
		switch (dir)
			{
			case Dir.U	: return new Vector2(0, 1);
			case Dir.R	: return new Vector2(1, 0);
			case Dir.D	: return new Vector2(0, -1);
			case Dir.L	: return new Vector2(-1, 0);
			case Dir.UR	: return (circular ? (new Vector2(OneOverSqrtOf2, OneOverSqrtOf2)).normalized : (new Vector2(1, 1)));
			case Dir.DR	: return (circular ? (new Vector2(OneOverSqrtOf2, -OneOverSqrtOf2)).normalized : (new Vector2(1, -1)));
			case Dir.DL	: return (circular ? (new Vector2(-OneOverSqrtOf2, -OneOverSqrtOf2)).normalized : (new Vector2(-1, -1)));
			case Dir.UL	: return (circular ? (new Vector2(-OneOverSqrtOf2, OneOverSqrtOf2)).normalized : (new Vector2(-1, 1)));
			}

		return Vector2.zero;		
		}
		

	// ----------------------
	static public Dir VecToDir(Vector2 vec, bool as8way)
		{
		return DirFromAngle(VecToAngle(vec), as8way);
		}

	// ---------------------	
	static public Dir VecToDir(Vector2 vec, Dir defaultDir, float deadZoneSq, bool as8way)
		{
		float mSq = vec.sqrMagnitude;
		if (mSq <= deadZoneSq)
			return defaultDir;

		if (Mathf.Abs(mSq - 1.0f) > 0.00001f)
			vec.Normalize();

		return DirFromAngle(VecToAngle(vec), as8way);
		}

	// -------------------------
	// Returns angle between 0 and 360
	// -----------------------	
	static public float NormalizeAnglePositive(float a)
		{
		if (a >= 360.0f) 
			return Mathf.Repeat(a, 360.0f);
		if (a >= 0)
			return a;
		if (a <= -360.0f)
			a = Mathf.Repeat(a, 360.0f);
		return (360.0f + a);
		}



	// ---------------------
	static public float SmartDeltaAngle(float startAngle, float curAngle, float lastDelta)
		{
		float frameDelta = Mathf.DeltaAngle(startAngle + lastDelta, curAngle);
		return lastDelta + frameDelta;
		}


	// --------------------
	static public Dir DigitalToDir(
		bool digiU,
		bool digiR,
		bool digiD,
		bool digiL)
		{
		if (digiU && digiD)	digiU = digiD = false;
		if (digiR && digiL)	digiR = digiL = false;

		if (digiU)
			return (digiR ? Dir.UR : digiL ? Dir.UL : Dir.U);

		else if (digiD)
			return (digiR ? Dir.DR : digiL ? Dir.DL : Dir.D);

		else
			return (digiR ? Dir.R : digiL ? Dir.L : Dir.N);
		}
		



	// ---------------------
	static public Vector2 CircularToSquareJoystickVec(Vector2 circularVec)
		{
		if (circularVec.sqrMagnitude < 0.00001f)
        	return Vector2.zero;
     
		Vector2 v = circularVec; 
  		Vector2 n = circularVec.normalized;
		
		v *= (Mathf.Abs(n.x) + Mathf.Abs(n.y));
     
		v.x = Mathf.Clamp(v.x, -1.0f, 1.0f);
		v.y = Mathf.Clamp(v.y, -1.0f, 1.0f);

		return v;
		}


	// ---------------------
	static public Vector2 SquareToCircularJoystickVec(Vector2 squareVec)
		{
		if (squareVec.sqrMagnitude < 0.00001f)
        	return Vector2.zero;
     
		Vector2 v = squareVec; 
  		Vector2 n = squareVec.normalized;
		
		v /= (Mathf.Abs(n.x) + Mathf.Abs(n.y));
     
		v.x = Mathf.Clamp(v.x, -1.0f, 1.0f);
		v.y = Mathf.Clamp(v.y, -1.0f, 1.0f);

		return v;
		}


	/// \}



	// --------------------
	static public int GetLineNumber(string str, int index)
		{
		int curLine = 1;
		int lastLineStart = 0;
		
		int nextLineStart = -1;
		while (((nextLineStart = str.IndexOf('\n', lastLineStart)) >= 0) && (nextLineStart < index))
			{ 
			curLine++;	
			lastLineStart = nextLineStart + 1;
			}
			
		return curLine;
		}

	// --------------------
	static public int GetEnumMaxValue(System.Type enumType)
		{
		int maxVal = 0;

		foreach (int v in System.Enum.GetValues(enumType))
			maxVal = ((maxVal == 0) ? v : Mathf.Max(maxVal, v));

		return maxVal;
		}


	// ----------------
	static public int CycleInt(int curId, int dir, int maxId)
		{
		curId += dir;
		if (curId < 0)
			curId = maxId;
		else if (curId > maxId)
			curId = 0;

		return curId;
		}


	// ------------------
	static public void SetEventSystemSelectedObject(GameObject o)
		{
		UnityEngine.EventSystems.EventSystem s = UnityEngine.EventSystems.EventSystem.current;
		if (s != null)
			{
			s.firstSelectedGameObject = o;

			if (s.currentInputModule is ControlFreak2.GamepadInputModule)
				{
				s.SetSelectedGameObject(null, null);
				s.SetSelectedGameObject(o, null);
				}
			}
		}
	}
}

//! \endcond

