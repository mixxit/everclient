using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class ZoneServer : MonoBehaviour {
    public string zoneserverip;
    public int zoneserverport;
    private NetworkClient _worldclient;

    public string worldserverip;
    public int worldserverport;

	// Use this for initialization
	void Start () {
        _worldclient = new NetworkClient();
        _worldclient.Connect(worldserverip, worldserverport);
        _worldclient.RegisterHandler(EverMsgType.ZoneServerWorldAuthenticationResponse, OnZoneServerWorldAuthenticationResponse);
	}

    private void OnZoneServerWorldAuthenticationResponse(NetworkMessage netMsg)
    {
        
    }

    // Update is called once per frame
    void Update () {
	
	}
}
