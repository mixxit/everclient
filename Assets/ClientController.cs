using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;

public class ClientController : MonoBehaviour {

    public string hostname;
    public int port;
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
        _client.RegisterHandler(EverMsgType.ClientLoginAuthenticationResponse, OnClientLoginAuthenticationResponse);
        _client.RegisterHandler(EverMsgType.ServerSelectionListResponse, OnServerSelectionListResponse);
        _client.RegisterHandler(EverMsgType.WorldServerUserConnectToZoneRequest, OnWorldServerUserConnectToZoneRequest);
        Initialised = true;
    }

    private void OnWorldServerUserConnectToZoneRequest(NetworkMessage netMsg)
    {
        string rawdata = netMsg.reader.ReadString();
        string[] sceneloaddata = rawdata.Split('|');
        Debug.Log("Changing Client Scene and connecting to Zone Server");
        Scenes.Load(sceneloaddata[2], "onClientLoad", _token + "|" + rawdata);
    }

    private void OnClientLoginAuthenticationResponse(NetworkMessage netMsg)
    {
        string[] clientLoginAuthenticationResponse = netMsg.reader.ReadString().Split('|');
        Debug.Log("Server sent back login result of : " + clientLoginAuthenticationResponse[0]);
        if (clientLoginAuthenticationResponse[0].Equals("1"))
        {
            Debug.Log("Authenticating: SUCCESS");
            _token = clientLoginAuthenticationResponse[1];
            RequestServerListRefresh();
        } else
        {
            Debug.Log("Authenticating: FAILED");
        }
    }

    public void RequestServerListRefresh()
    {
        _client.Send(EverMsgType.ServerSelectionListRequest, new StringMessage(BuildUserTokenValidationPacket()));
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
        _client.Send(EverMsgType.ClientLoginAuthenticationRequest, new StringMessage(BuildAuthenticationPacket(user,pass)));
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
        List<ActiveWorldServer> serverlist = new List<ActiveWorldServer>();

        if (!String.IsNullOrEmpty(data))
        {
            string[] serversdata = data.Split('^');
            foreach (string serverdata in serversdata)
            {
                string[] servercolumns = serverdata.Split('|');
                ActiveWorldServer worldserver = new ActiveWorldServer();
                worldserver.hostname = servercolumns[0];
                worldserver.port = Convert.ToInt32(servercolumns[1]);
                worldserver.name = servercolumns[2];
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
