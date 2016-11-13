using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class EverClient : MonoBehaviour {

    public string hostname;
    public int port;
    public static EverClient Instance;
    private NetworkClient _client;
    public bool Initialised = false;

	// Use this for initialization
	void Start () {
        Initialise();
	}

    public void Initialise()
    {
        if (Initialised == true)
            return;

        _client = new NetworkClient();
        _client.Connect(hostname, port);
        _client.RegisterHandler(MsgType.Connect, OnConnect);
        _client.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        _client.RegisterHandler(MsgType.Error, OnError);

        Initialised = true;
    }

    public void Login(string user, string pass)
    {
        Debug.Log("Authenticating: " + user + "&" + pass);
    }

    private void OnError(NetworkMessage netMsg)
    {
        Debug.Log("Error connecting");
        GameObject.Find("MenuManager").GetComponent<MenuManager>().FallbackMessage("Error connecting");
        Initialised = false;
    }

    private void OnDisconnect(NetworkMessage netMsg)
    {
        Debug.Log("Disconnected from server");
        GameObject.Find("MenuManager").GetComponent<MenuManager>().FallbackMessage("Disconnected from server");
        Initialised = false;
    }

    private void OnConnect(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
        GameObject.Find("MenuManager").GetComponent<MenuManager>().GoToLoginScreen();
        Initialised = true;
    }

    // Update is called once per frame
    void Update () {
	
	}
}
