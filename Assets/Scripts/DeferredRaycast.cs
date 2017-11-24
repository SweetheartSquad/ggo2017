using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeferredRaycast : StandaloneInputModule
{
	public Camera bookCam;
	public RawImage deferredCursor;
	public RenderTexture pageTex;
	public LayerMask raycastTargets;

	private Vector2 m_mouse;

	// Update is called once per frame
	void Update()
	{
		// whyyy
		Vector3 m = Input.mousePosition;
		m += new Vector3(16, -16, 0);
		m.x /= Screen.width;
		m.y /= Screen.height;
		Ray ray = Camera.main.ViewportPointToRay(m);

		Debug.DrawLine(ray.origin, ray.origin + ray.direction * Camera.main.farClipPlane, Color.red, 0, true);
		RaycastHit hit;

		deferredCursor.enabled = true;
		Cursor.visible = false;
		if(!Physics.Raycast(ray, out hit, raycastTargets)) {
			// didn't hit anything
			deferredCursor.enabled = false;
			Cursor.visible = true;
			return;
		}

		ray = bookCam.ViewportPointToRay(new Vector3(hit.textureCoord.x, hit.textureCoord.y, 0.0f));
		Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1.0f, Color.blue, 0, true);

		m_mouse = new Vector2(hit.textureCoord.x * pageTex.width, (hit.textureCoord.y) * pageTex.height);
		deferredCursor.rectTransform.anchoredPosition = m_mouse;
	}
	// Adapted from https://forum.unity.com/threads/fake-mouse-position-in-4-6-ui-answered.283748/
	protected override MouseState GetMousePointerEventData(int id = 0)
	{
		PointerEventData leftData;
		GetPointerData(kMouseLeftId, out leftData, true);
		PointerEventData rightData;
		GetPointerData(kMouseRightId, out rightData, true);
		PointerEventData middleData;
		GetPointerData(kMouseMiddleId, out middleData, true);

		Vector2 pos = m_mouse;
		leftData.delta = pos - leftData.position;
		leftData.position = pos;
		leftData.scrollDelta = Input.mouseScrollDelta;
		leftData.button = PointerEventData.InputButton.Left;
		eventSystem.RaycastAll(leftData, m_RaycastResultCache);
		RaycastResult raycast = FindFirstRaycast(m_RaycastResultCache);
		leftData.pointerCurrentRaycast = raycast;
		m_RaycastResultCache.Clear();

		// copy the apropriate data into right and middle slots
		CopyFromTo(leftData, rightData);
		rightData.button = PointerEventData.InputButton.Right;

		CopyFromTo(leftData, middleData);
		middleData.button = PointerEventData.InputButton.Middle;

		MouseState m = new MouseState();
		m.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
		m.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
		m.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);
		return m;
	}
}
