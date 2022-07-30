// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2020 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

//! \cond


#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 
	#define UNITY_PRE_5_0
#endif

#if UNITY_PRE_5_0 || UNITY_5_0 
	#define UNITY_PRE_5_1
#endif

#if UNITY_PRE_5_1 || UNITY_5_1 
	#define UNITY_PRE_5_2
#endif

#if UNITY_PRE_5_2 || UNITY_5_2 
	#define UNITY_PRE_5_3
#endif

#if UNITY_PRE_5_3 || UNITY_5_3 
	#define UNITY_PRE_5_4
#endif

using UnityEngine;


namespace ControlFreak2.Internal
{
public class CFGizmos 
	{
		
#if UNITY_EDITOR
		
		
		
	// ---------------
	static public Matrix4x4 GetCircleMatrix(Vector3 cen, Vector2 size)
		{
		return Matrix4x4.TRS(cen, Quaternion.identity, (Vector3)size * 0.5f);
		}


	// ----------------
	static public void DrawCircle(bool filled)
		{ DrawCircle(filled, true); }

	static public void DrawCircle(bool filled, bool lowRes /*= true*/)
		{
#if !UNITY_PRE_5_0
		Mesh m = (lowRes ? Inst().circleMeshLowRes : Inst().circleMeshHiRes);
					
		if (filled)
			Gizmos.DrawMesh(m, 0);
		else
			Gizmos.DrawWireMesh(m, 1);
#else
		if (filled)
			Gizmos.DrawSphere(Vector3.zero, 1);
		else
			Gizmos.DrawWireSphere(Vector3.zero, 1);
#endif
		}

		

	// ----------------
	static public void DrawCircle(Vector3 cen, float size, bool filled) 
		{ DrawCircle(cen, size, filled, true); } 

	static public void DrawCircle(Vector3 cen, float size, bool filled, bool lowRes /*= true*/) 
		{ DrawCircle(cen, Vector3.one * size, filled, lowRes); }

	static public void DrawCircle(Vector3 cen, Vector2 size, bool filled)
		{ DrawCircle(cen, size, filled, true); }

	static public void DrawCircle(Vector3 cen, Vector2 size, bool filled, bool lowRes /*= true*/)
		{
		Matrix4x4 initialMatrix = Gizmos.matrix;
		Gizmos.matrix = initialMatrix * GetCircleMatrix(cen, size);	
		
		DrawCircle(filled, lowRes);

		Gizmos.matrix = initialMatrix;
		}
		

	// --------------------
	static public void DrawOutlinedCircle()
		{ DrawOutlinedCircle(true); }

	static public void DrawOutlinedCircle(bool lowRes /*= true*/)
		{
		Color initialColor = Gizmos.color;

		Gizmos.color = new Color(0, 0, 0, 0);
		DrawCircle(true, lowRes);

		Gizmos.color = initialColor;
		DrawCircle(false, lowRes);
		}
		




	// -------------------
	static public void DrawRect(Rect r) { DrawRect(r, 0); }
	static public void DrawRect(Rect r, float depth )
		{
		Vector3 cen = r.center;
		cen.z = depth;
		Gizmos.DrawWireCube(cen, new Vector3(r.width, r.height, 0));
		}



	// ------------------
	static private void DrawArrowRawXY(float len, float headWidth, float headHeight)
		{
		headHeight = Mathf.Max(len * 0.5f, headHeight);
		headWidth = Mathf.Min(headHeight, headWidth);
		
		Vector3 tipPos		= new Vector3(0, len, 0);
		Vector3 armEndPos	= new Vector3(0, len - headHeight, 0);
		Vector3 headCornerA = new Vector3(-headWidth * 0.5f, len - headHeight, 0);
		Vector3 headCornerB = new Vector3(-headCornerA.x, headCornerA.y, 0);
		
		Gizmos.DrawLine(Vector3.zero, armEndPos);
		Gizmos.DrawLine(headCornerA, tipPos);
		Gizmos.DrawLine(headCornerB, tipPos);
		Gizmos.DrawLine(headCornerA, headCornerB);
		}


	// ---------------
	static private void DrawArrowXY(Vector3 pos, float angle, float len) { DrawArrowXY(pos, angle, len, 0.1f, 0.2f); }
	static private void DrawArrowXY(Vector3 pos, float angle, float len, float headWidth /*= 0.1f*/, float headHeight/* = 0.2f*/)
		{
		Matrix4x4 m = Gizmos.matrix;			
		Gizmos.matrix = Gizmos.matrix * Matrix4x4.TRS(pos, Quaternion.Euler(0,0, angle), Vector3.one);
		
		DrawArrowRawXY(len, headWidth, headHeight);

		Gizmos.matrix = m;
		}


	// ---------------
	static public void DrawDoubleRect(Rect curRect, Rect targetRect) { DrawDoubleRect(curRect, targetRect, 0); }
	static public void DrawDoubleRect(Rect curRect, Rect targetRect, float depth)
		{
		DrawRect(curRect, depth);
		DrawRect(targetRect, depth);

		if (curRect.xMin != targetRect.xMin)
			{
			// TODO : DrawArrowXY(
			}

		if (curRect.xMax != targetRect.xMax)
			{
			}

		if (curRect.yMin != targetRect.yMin)
			{
			}

		if (curRect.yMax != targetRect.yMax)
			{
			}


		}
		



	// ---------------

	static private CFGizmos mInst;
		

	private Mesh
		_circleMeshLowRes,
		_circleMeshHiRes;

	public Mesh circleMeshLowRes
		{ get { return ((_circleMeshLowRes == null) ? (_circleMeshLowRes = CreateCircleMesh(1, 16)) : _circleMeshLowRes); } }

	public Mesh circleMeshHiRes
		{ get { return ((_circleMeshHiRes == null) ? (_circleMeshHiRes = CreateCircleMesh(1, 32)) : _circleMeshHiRes); } }
		

	// -----------------------
	static private CFGizmos Inst()
		{
		return ((mInst != null) ? mInst : (mInst = new CFGizmos()));		
		}

		
	// -------------------------
	static private void DrawLineStrip(Vector3[] points)
		{	
		for (int i = 0; i < (points.Length - 1); ++i)
			Gizmos.DrawLine(points[i], points[i+1]);
		}

	// -------------------------
	static private void DrawLineSegments(Vector3[] points)
		{	
		for (int i = 0; i < (points.Length - 1); i += 2)
			Gizmos.DrawLine(points[i], points[i+1]);
		}


	// --------------------
	static private Vector3[] CreateCirclePoints(float rad, int segs)
		{ return CreateCirclePoints(rad, segs, false); }

	static private Vector3[] CreateCirclePoints(float rad, int segs, bool createOriginVert /*= false*/)
		{
		segs = Mathf.Max(segs + 1, 4);
		Vector3[] points = new Vector3[segs + (createOriginVert ? 1 : 0)];
			
		int vOfs = (createOriginVert ? 1 : 0);

		if (createOriginVert)
			points[0] = Vector3.zero;

		for (int i = 0; i < segs; ++i)
			{
			float a = ((float)i / (float)(segs - 1)) * 360.0f * Mathf.Deg2Rad;
			points[vOfs + i] = new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0) * rad;
			}

		return points;
		}

	// --------------------
	static private Mesh CreateCircleMesh(float rad, int segs)
		{
		Vector3[] verts = CreateCirclePoints(rad, segs, true);
			

		int triCount = (verts.Length - 2);
		int[] triInds = new int[triCount * 3];

		int vOfs = 0;

		for (int i = 0; i < triCount; ++i)
			{
			triInds[vOfs++] = 0;
			triInds[vOfs++] = (i + 1);
			triInds[vOfs++] = (i + 2);			
			}
	
		int[] outlineInds = new int[verts.Length - 1];
		for (int i = 0; i < outlineInds.Length; ++i)
			outlineInds[i] = (i + 1);


		Mesh m = new Mesh();

		m.vertices = verts;
		m.normals = verts;
			
		m.subMeshCount = 2;

		m.SetIndices(triInds, MeshTopology.Triangles, 0);
		m.SetIndices(outlineInds, MeshTopology.LineStrip, 1);

		m.UploadMeshData(true);

		return m;
		}

		

#endif


	}
}

//! \endcond
