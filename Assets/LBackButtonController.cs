﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LBackButtonController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClick()
	{
		GameObject.Find ("MenuManager").GetComponent<MenuManager> ().LoginPanel.SetActive (false);
		GameObject.Find ("MenuManager").GetComponent<MenuManager> ().MainMenuPanel.SetActive (true);
	}
}