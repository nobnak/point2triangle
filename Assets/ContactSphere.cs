using UnityEngine;
using System.Collections;

public class ContactSphere : MonoBehaviour {
	public DistanceField field;

	private Material _mat;

	// Use this for initialization
	void Start () {
		_mat = renderer.material;
	}
	
	// Update is called once per frame
	void Update () {
		var dist = field.distance(transform.position);
		transform.localScale = 2f * Mathf.Abs(dist.w) * Vector3.one;
		_mat.color = (dist.w > 0f ? Color.red : Color.blue);
	}

	void OnDestroy() {
		Destroy(_mat);
	}
}
