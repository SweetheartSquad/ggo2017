using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yarn.Unity {
	public class Starter : MonoBehaviour {

		// Use this for initialization
		void Start () {
			FindObjectOfType<DialogueRunner> ().StartDialogue ("Start");
		}
		
		// Update is called once per frame
		void Update () {
			
		}
	}
}
