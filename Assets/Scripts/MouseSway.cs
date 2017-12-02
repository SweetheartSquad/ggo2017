using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSway : MonoBehaviour
{
	public float sway = 2.0f;
	[Range(0, 1)]
	public float lerp = 0.1f;
	public bool invertX = false;
	public bool invertY = false;

	Vector3 t = new Vector3(0, 0, 0);
	// Update is called once per frame
	void Update()
	{
		// whyyy
		t = Input.mousePosition;
		t.x /= Screen.width;
		t.y /= Screen.height;


		t.x = 0.5f - t.x;
		t.y = 0.5f - t.y;
		if(invertX) {
			t.x *= -1;
		}
		if(invertY) {
			t.y *= -1;
		}
		t.z = t.y;
		t.y = t.x / 4.0f;
		t *= sway;

		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(t.x, t.y, t.z), lerp);

		if(Input.GetKey(KeyCode.Escape)) {
			Application.Quit();
		}
	}
}
