using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageFlip : MonoBehaviour
{
	public GameObject pageFlip;
	public Material pageMat;
	// Use this for initialization
	void Start()
	{
	}

	public void Flip(bool dir, float delay, bool edge)
	{
		StartCoroutine(DelayedAnimation(dir, delay, edge));
	}

	IEnumerator DelayedAnimation(bool dir, float delay, bool edge)
	{
		// delay
		// create page
		// start flip
		// wait for flip to finish
		// destroy page
		yield return new WaitForSeconds(delay);
		GameObject page = Instantiate(pageFlip);
		if(edge) {
			page.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material = pageMat;
		}
		page.GetComponent<Animator>().SetTrigger(dir ? "FlipLeft" : "FlipRight");
		yield return new WaitForSeconds(2);
		Destroy(page);
	}
}
