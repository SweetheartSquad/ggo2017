using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderUpdate : MonoBehaviour
{
	SkinnedMeshRenderer meshRenderer;
	MeshCollider collider;

	private void Start()
	{
		Transform page = transform.FindDeepChild("animate_page");
		meshRenderer = page.GetComponent<SkinnedMeshRenderer>();
		meshRenderer.updateWhenOffscreen = true;
		collider = page.GetComponent<MeshCollider>();

		GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.SampleAnimation(gameObject, 1.0f);
		UpdateCollider();
		Destroy(this);
	}

	public void UpdateCollider()
	{
		Mesh colliderMesh = new Mesh();
		meshRenderer.BakeMesh(colliderMesh);
		collider.sharedMesh = null;
		collider.sharedMesh = colliderMesh;
	}
}
