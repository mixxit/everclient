using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;

public class EverClient : MonoBehaviour {

    public string hostname;
    public int port;
    public static EverClient Instance;
    private NetworkClient _client;
    public bool Initialised = false;
    private string _token = "";
    private string _username = "";

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
        _client.RegisterHandler(EverMsgType.AuthenticationFailed, OnAuthenticationFailed);
        _client.RegisterHandler(EverMsgType.AuthenticationSucceeded, OnAuthenticationSucceeded);
        _client.RegisterHandler(EverMsgType.AuthenticationUnavailable, OnAuthenticationUnavailable);
        _client.RegisterHandler(EverMsgType.ServerSelectionListResponse, OnServerSelectionListResponse);
        Initialised = true;
    }

    private void OnAuthenticationUnavailable(NetworkMessage netMsg)
    {
        Debug.Log("Authenticating: UNAVAILABLE");
    }

    private void OnAuthenticationSucceeded(NetworkMessage netMsg)
    {
        Debug.Log("Authenticating: SUCCESS");
        _token = netMsg.reader.ReadString();

        RequestServerListRefresh();
    }

    public void RequestServerListRefresh()
    {
        _client.Send(EverMsgType.ServerSelectionListRequest, new StringMessage(BuildUserTokenValidationPacket()));
    }

    private void OnAuthenticationFailed(NetworkMessage netMsg)
    {
        Debug.Log("Authenticating: FAILED");
    }

    public string BuildAuthenticationPacket(String username, String password)
    {
        // TODO: determine how messaging system uses SSL if at all
        // TODO: ssl web sockets instead?
        // TODO: salt this after this information is discovered
        string authenticationpacket = username + "|" + password;
        return authenticationpacket;
    }

    public string BuildUserTokenValidationPacket()
    {
        string usertokenvalidation = _username + "|" + _token;
        return usertokenvalidation;
    }

    public void Login(string user, string pass)
    {
        Debug.Log("Authenticating: " + user + "&" + pass);
        _username = user;
        _client.Send(EverMsgType.AuthenticationRequest, new StringMessage(BuildAuthenticationPacket(user,pass)));
    }

    private void OnError(NetworkMessage netMsg)
    {
        Debug.Log("Error connecting");
        GameObject.Find("MenuManager").GetComponent<MenuManager>().FallbackMessage("Error connecting");
        Initialised = false;
    }

    private void OnServerSelectionListResponse(NetworkMessage netMsg)
    {
        string data = netMsg.reader.ReadString();
        Debug.Log("Received ServerList Data: " + data);
        List<WorldServer> serverlist = new List<WorldServer>();

        if (!String.IsNullOrEmpty(data))
        {
            string[] serversdata = data.Split('^');
            foreach (string serverdata in serversdata)
            {
                string[] servercolumns = serverdata.Split('|');
                WorldServer worldserver = new WorldServer();
                worldserver.serverip = servercolumns[0];
                worldserver.serverport = Convert.ToDecimal(servercolumns[1]);
                worldserver.servername = servercolumns[2];
                serverlist.Add(worldserver);
            }
        }

        GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowServerSelect(serverlist);
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
