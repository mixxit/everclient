using UnityEngine;
using System.Collections;

public class ServerListRefreshButtonController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public GameObject serverselectpanel;

    public void RefreshServerSelect()
    {
        serverselectpanel.GetComponent<ServerSelectController>().RequestServerListRefresh();
    }
}
