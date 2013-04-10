using UnityEngine;
using System.Collections;

public class DistanceField : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Vector3[] triInWorld = new Vector3[]{
			new Vector3(1f, 0f, 1f),
			new Vector3(2f, 0f, 1f),
			new Vector3(1f, 0f, 2f) };
		Matrix4x4 w2l = convertIn2D(triInWorld);
		Vector3[] triIn2d = new Vector3[]{
			w2l.MultiplyPoint3x4(triInWorld[0]),
			w2l.MultiplyPoint3x4(triInWorld[1]),
			w2l.MultiplyPoint3x4(triInWorld[2]) };
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public Matrix4x4 convertIn2D(Vector3[] triVerts) {
		Vector3 pos = triVerts[0];
		Vector3 edge01 = triVerts[1] - triVerts[0];
		Vector3 edge02 = triVerts[2] - triVerts[0];
		Vector3 z = edge02.normalized;
		Vector3 x = Vector3.Cross(edge01, z).normalized;
		Vector3 y = Vector3.Cross(z, x);
		
		Matrix4x4 local2world = Matrix4x4.TRS(pos, Quaternion.LookRotation(z, y), Vector3.one);
		return local2world.inverse;
	}
	
	public Vector3 distance(Vector3 point, Matrix4x4 m2d, Vector2[] triIn2d) {
		Vector3 pInLocal = m2d.MultiplyPoint3x4(point);
		Vector2 pIn2d = new Vector3(0, pInLocal.y, pInLocal.z);
		Vector2 edge13 = triIn2d[2] - triIn2d[0];
		Vector2 edge32 = triIn2d[1] - triIn2d[2];
		Vector2 edge21 = triIn2d[0] - triIn2d[1];
		
		Vector2 v1p = pIn2d - triIn2d[0];
		Vector2 v2p = pIn2d - triIn2d[1];
		Vector2 v3p = pIn2d - triIn2d[2];
		
		if (det(v1p, edge13) <= 0) {
			if (det(v3p, edge32) <= 0) {
				if (det(v2p, edge21) <= 0) {
				} else {
				}
			} else {}
		} else {}				
		
		return Vector3.zero;
	}
			
	public float det(Vector2 p, Vector2 e) {
		return p.x * e.y - p.y * e.x;
	}
}
