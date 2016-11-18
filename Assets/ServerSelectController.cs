using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ServerSelectController : MonoBehaviour {

    public GameObject contentarea;
    public GameObject serverlistbutton;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ClearServerEntries()
    {
        for (var i = contentarea.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(contentarea.transform.GetChild(i).gameObject);
        }
    }

    public void BuildServerSelectPanel(List<ActiveWorldServer> worldservers)
    {
        ClearServerEntries();

        GameObject abutton = Instantiate(serverlistbutton);
        abutton.transform.SetParent(contentarea.transform, false);
        abutton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Admin Panel";

        foreach (ActiveWorldServer worldserver in worldservers)
        {
            GameObject button = Instantiate(serverlistbutton);
            button.transform.SetParent(contentarea.transform, false);
            button.GetComponentInChildren<UnityEngine.UI.Text>().text = worldserver.name;
        }
    }

    public void RequestServerListRefresh()
    {
        GameObject.Find("EverClient").GetComponent<ClientController>().RequestServerListRefresh();
    }
}
