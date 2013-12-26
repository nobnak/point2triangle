using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ContactSphere : MonoBehaviour {
	public DistanceField field;

	private Material _mat;

	void OnEnable() {
		_mat = renderer.sharedMaterial;
	}
	
	// Update is called once per frame
	void Update () {
		if (field == null)
			return;

		var dist = field.distance(transform.position);
		transform.localScale = 2f * Mathf.Abs(dist.w) * Vector3.one;
		_mat.color = (dist.w > 0f ? Color.red : Color.blue);
	}
}
