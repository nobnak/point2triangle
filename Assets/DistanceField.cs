using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[ExecuteInEditMode]
public class DistanceField : MonoBehaviour {
	public enum Feature { Vertex, Edge, Face };

	private Vector2[][] triVertsInLocal;
	private Matrix4x4[] transTriSpaces;
	private Matrix4x4[] transTriSpacesInv;
	private Vector3 dxInv;

	private int[] triangles;
	private Vector3[] normals;
	private Vector3[] vnormals;
	private Dictionary<Edge, Vector3> enormals;

	// Use this for initialization
	void OnEnable () {
		var mf = GetComponent<MeshFilter>();
		var mesh = mf.sharedMesh;
		var tr = transform;
		var vertices = (from v in mesh.vertices select tr.TransformPoint(v)).ToArray();
		triangles = mesh.triangles;
		var triVertices = (from i in Enumerable.Range(0, triangles.Length / 3) 
			select new Vector3[]{ vertices[triangles[3*i]],	vertices[triangles[3*i+1]], vertices[triangles[3*i+2]] }
			).ToArray();

		var angles = new float[triangles.Length];
		for (var i = 0; i < triVertices.Length; i++) {
			var tri = triVertices[i];
			for (var j = 0; j < 3; j++) {
				var tan0 = (tri[(j+1)%3]-tri[j]).normalized;
				var tan1 = (tri[(j+2)%3]-tri[j]).normalized;
				angles[3 * i + j] = Mathf.Acos(	Vector3.Dot(tan0, tan1) );
			}
		}
		normals = (from tri in triVertices select Vector3.Cross(tri[1]-tri[0], tri[2]-tri[0]).normalized).ToArray();
		//this.normals = mesh.normals;
		vnormals = new Vector3[vertices.Length];
		enormals = new Dictionary<Edge, Vector3>();
		for (var i = 0; i < triVertices.Length; i++) {
			for (var j = 0; j < 3; j++) {
				var index = 3 * i + j;
				vnormals[triangles[index]] += angles[index] * normals[i];
				var edge = new Edge(triangles[index], triangles[3 * i + (j + 1) % 3]);
				var en = (enormals.ContainsKey(edge) ? enormals[edge] : Vector3.zero);
				en += normals[i];
				enormals[edge] = en;
			}
		}
		vnormals = (from vn in vnormals select vn.normalized).ToArray();
		mesh.normals = vnormals;
		mesh.colors = (from i in Enumerable.Range(0, vertices.Length) select EditorGUIUtility.HSVToRGB((float)i / vertices.Length, 1f, 1f)).ToArray();

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
		Vector3 p2mInLoal = Vector3.zero;
		int iMin = -1;
		Vector3 normal = Vector3.zero;
		Feature ftr = default(Feature);
		for (int iTriangle = 0; iTriangle < triVertsInLocal.Length; iTriangle++) {
			//Vector2[] triangle = triVertsInLocal[i];
			Matrix4x4 m = transTriSpaces[iTriangle];
			Vector3 pointInLocal = m.MultiplyPoint3x4(point);
			Vector3 tmpNormal;
			Feature tmpFtr;
			Vector3 tmpP2mInLocal = distanceInLocal(pointInLocal, iTriangle, out tmpNormal, out tmpFtr);
			float tmpSqrDist = tmpP2mInLocal.sqrMagnitude;
			if (tmpSqrDist < minSqrDist) {
				minSqrDist = tmpSqrDist;
				p2mInLoal = tmpP2mInLocal;
				iMin = iTriangle;
				normal = tmpNormal;
				ftr = tmpFtr;
			}
		}
		Vector4 p2m = transTriSpacesInv[iMin].MultiplyVector(p2mInLoal);
		p2m.w = (Vector3.Dot((Vector3)p2m, normal) > 0 ? -1 : +1) * Mathf.Sqrt(minSqrDist);
		//UnityEngine.Debug.Log(string.Format("{0}:n={1},p2m={2}", ftr, normal, p2m));
		return p2m;
	}
	public Vector3 distanceInLocal(Vector3 pointInLocal, int iTriangle, out Vector3 normal, out Feature ftr) {
		Vector2[] triIn2d = triVertsInLocal[iTriangle];
		Vector2 pIn2d = new Vector3(pointInLocal.x, pointInLocal.y, 0);
		Vector2[] edges = new Vector2[]{
			triIn2d[1] - triIn2d[0],
			triIn2d[2] - triIn2d[1],
			triIn2d[0] - triIn2d[2] };
		Vector2[] v2p = new Vector2[]{
			pIn2d - triIn2d[0],
			pIn2d - triIn2d[1],
			pIn2d - triIn2d[2] };

		Vector3 p2tri;
		for (int i = 0; i < 3; i++) {
			if (det(edges[i], v2p[i]) < 0) {
				float lenEdge = edges[i].magnitude;
				Vector2 dirEdge = edges[i] / lenEdge;
				var index0 = 3 * iTriangle + i;
				var index1 = 3 * iTriangle + (i + 1) % 3;
				float t = Vector2.Dot(v2p[i], dirEdge);
				if (t < 0) {
					ftr = Feature.Vertex;
					normal = vnormals[triangles[index0]];
					p2tri = (Vector3)triIn2d[i % 3] - pointInLocal;
				} else if (t <= lenEdge) {
					ftr = Feature.Edge;
					normal = enormals[new Edge(triangles[index0], triangles[index1])];
					p2tri = (Vector3)(t * dirEdge + triIn2d[i]) - pointInLocal;
				} else {
					ftr = Feature.Vertex;
					normal = vnormals[triangles[index1]];
					p2tri = (Vector3)triIn2d[(i + 1) % 3] - pointInLocal;
				}
				return p2tri;
			}
		}
		ftr = Feature.Face;
		normal = normals[iTriangle];
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
