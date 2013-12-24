using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

public class DistanceField : MonoBehaviour {
	public GameObject targetObj;
	
	private Vector2[][] triVertsInLocal;
	private Matrix4x4[] transTriSpaces;
	private Matrix4x4[] transTriSpacesInv;
	private Vector3 dxInv;

	// Use this for initialization
	void Start () {
		var mf = (MeshFilter)targetObj.GetComponent("MeshFilter");
		var mesh = mf.mesh;
		var tr = targetObj.transform;
		var vertices = (from v in mesh.vertices select tr.TransformPoint(v)).ToArray();
		var triangles = mesh.triangles;
		var triVertices = (from i in Enumerable.Range(0, triangles.Length / 3) 
			select new Vector3[]{ vertices[triangles[3*i]],	vertices[triangles[3*i+1]], vertices[triangles[3*i+2]] }
			).ToArray();

		var angles = new float[triangles.Length];
		for (var i = 0; i < triVertices.Length; i++) {
			var tri = triVertices[i];
			for (var j = 0; j < 3; j++) {
				angles[3 * i + j] = Mathf.Acos(
					Vector3.Dot( (tri[(j+1)%3]-tri[j]).normalized, (tri[(j-1)%3]-tri[j]).normalized ));
			}
		}
		var normals = (from tri in triVertices select Vector3.Cross(tri[1]-tri[0], tri[2]-tri[0]).normalized).ToArray();
		var vnormals = new Vector3[vertices.Length];
		var enormals = new Dictionary<Edge, Vector3>();
		for (var i = 0; i < triVertices.Length; i++) {
			var tri = triVertices[i];
			for (var j = 0; j < 3; j++) {

			}
		}

		this.transTriSpaces = (from tri in triVertices select calcTriangleSpace(tri)).ToArray();
		this.transTriSpacesInv = (from m in transTriSpaces select m.inverse).ToArray();
		this.triVertsInLocal = (from i in Enumerable.Range(0, transTriSpaces.Length) select
			new Vector2[]{
				(Vector2)transTriSpaces[i].MultiplyPoint3x4(triVertices[i][0]),
				(Vector2)transTriSpaces[i].MultiplyPoint3x4(triVertices[i][1]),
				(Vector2)transTriSpaces[i].MultiplyPoint3x4(triVertices[i][2]) }).ToArray();
	}

	public Matrix4x4 calcTriangleSpace(Vector3[] triVerts) {
		Vector3 pos = triVerts[0];
		Vector3 edge01 = triVerts[1] - triVerts[0];
		Vector3 edge02 = triVerts[2] - triVerts[0];
		Vector3 x = edge01.normalized;
		Vector3 z = Vector3.Cross(x, edge02).normalized;
		Vector3 y = Vector3.Cross(z, x);
		
		Matrix4x4 local2world = Matrix4x4.TRS(pos, Quaternion.LookRotation(z, y), Vector3.one);
		return local2world.inverse;
	}
	
	public Vector4 distance(Vector3 point) {
		float minSqrDist = Mathf.Infinity;
		Vector3 point2nearestMesh = Vector3.zero;
		int iMin = -1;
		for (int i = 0; i < triVertsInLocal.Length; i++) {
			Vector2[] triangle = triVertsInLocal[i];
			Matrix4x4 m = transTriSpaces[i];
			Vector3 pointInLocal = m.MultiplyPoint3x4(point);
			Vector3 point2meshInLocal = distanceInLocal(pointInLocal, triangle);
			float tmpSqrDist = point2meshInLocal.sqrMagnitude;
			if (tmpSqrDist < minSqrDist) {
				minSqrDist = tmpSqrDist;
				point2nearestMesh = point2meshInLocal;
				iMin = i;
			}
		}
		Vector4 distInWorld = transTriSpacesInv[iMin].MultiplyVector(point2nearestMesh);
		distInWorld.w = (point2nearestMesh.z > 0 ? -1 : +1) * Mathf.Sqrt(minSqrDist);
		return distInWorld;
	}
	public Vector3 distanceInLocal(Vector3 pointInLocal, Vector2[] triIn2d) {
		Vector2 pIn2d = new Vector3(pointInLocal.x, pointInLocal.y, 0);
		Vector2[] edges = new Vector2[]{
			triIn2d[1] - triIn2d[0],
			triIn2d[2] - triIn2d[1],
			triIn2d[0] - triIn2d[2] };
		Vector2[] ps = new Vector2[]{
			pIn2d - triIn2d[0],
			pIn2d - triIn2d[1],
			pIn2d - triIn2d[2] };
		
		Vector3 p2tri;
		for (int i = 0; i < 3; i++) {
			if (det(edges[i], ps[i]) < 0) {
				float lenEdge = edges[i].magnitude;
				Vector2 dirEdge = edges[i] / lenEdge;
				float t = Vector2.Dot(ps[i], dirEdge);
				if (t < 0)
					p2tri = (Vector3)triIn2d[i % 3] - pointInLocal;
				else if (t <= lenEdge)
					p2tri = (Vector3)(t * dirEdge + triIn2d[i]) - pointInLocal;
				else
					p2tri = (Vector3)triIn2d[(i + 1) % 3] - pointInLocal;
				return p2tri;
			}
		}
		p2tri = new Vector3(0, 0, -pointInLocal.z);
		return p2tri;
	}
			
	public float det(Vector2 p, Vector2 e) {
		return p.x * e.y - p.y * e.x;
	}

	public struct Edge {
		public int v0;
		public int v1;

		public Edge(int v0, int v1) {
			if (v1 < v0) {
				var tmp = v0; v0 = v1; v1 = tmp;
			}
			this.v0 = v0;
			this.v1 = v1;
		}

		public override bool Equals (object obj) {
			if (obj.GetType() != typeof(Edge))
				return false;
			var b = (Edge)obj;
			return b.v0 == v0 && b.v1 == v1;
		}

		public override int GetHashCode () {
			return 251 * (v0 + 137 * v1);
		}
	}
}
