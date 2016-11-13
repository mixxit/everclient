using UnityEngine;
using System.Collections;

public class LExitButtonController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
