using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

public class ZoneServerController : MonoBehaviour {
    public string listenhostname;
    public int listenport;
    private NetworkClient _worldclient;

    public string worldserverusername;
    public string worldserverpassword;
    public string worldserverhostname;
    public int worldserverport;

    private string _worldservertoken;

	// Use this for initialization
	void Start () {
        Application.runInBackground = true;
        NetworkServer.Listen(listenport);

        NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnect);
        NetworkServer.RegisterHandler(MsgType.Error, OnClientError);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnect);

        _worldclient = new NetworkClient();
        _worldclient.Connect(worldserverhostname, worldserverport);
        _worldclient.RegisterHandler(MsgType.Connect, OnWorldServerConnect);
        _worldclient.RegisterHandler(MsgType.Disconnect, OnWorldServerDisconnect);
        _worldclient.RegisterHandler(MsgType.Error, OnWorldServerError);
        _worldclient.RegisterHandler(EverMsgType.ZoneServerWorldAuthenticationResponse, OnZoneServerWorldAuthenticationResponse);
        _worldclient.RegisterHandler(EverMsgType.ZoneServerWorldChangeSceneRequest, OnZoneServerWorldChangeSceneRequest);
    }

    private void OnClientDisconnect(NetworkMessage netMsg)
    {
        Debug.Log("OnClientDisconnect");
    }

    private void OnClientError(NetworkMessage netMsg)
    {
        Debug.Log("OnClientError");
    }

    private void OnClientConnect(NetworkMessage netMsg)
    {
        Debug.Log("OnClientConnect");
        if (SceneManager.GetActiveScene().name.Equals("inactivezone"))
        {
            Debug.Log("Client attempt to connect when in sleep mode, disconnected client");
            netMsg.conn.Disconnect();
        }
    }

    private void OnZoneServerWorldChangeSceneRequest(NetworkMessage netMsg)
    {
        string targetscene = netMsg.reader.ReadString();
        bool doessceneexist = DoesSceneExist(targetscene);

        if (doessceneexist)
        {
            Debug.Log("Starting switch to zone");
            _worldclient.Send(EverMsgType.ZoneServerWorldChangeSceneResponse, new StringMessage("1"));
            LoadZone(targetscene);
        } else
        {
            _worldclient.Send(EverMsgType.ZoneServerWorldChangeSceneResponse, new StringMessage("0"));
        }
    }

    public void LoadZone(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }

    private bool DoesSceneExist(string scenename)
    {
        for(var i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name.Equals(scenename))
                return true;
        }

        return false;
    }

    private void OnWorldServerError(NetworkMessage netMsg)
    {
        Debug.Log("OnWorldServerError");
    }

    private void OnWorldServerDisconnect(NetworkMessage netMsg)
    {
        Debug.Log("OnWorldServerDisconnect");
    }

    private void OnWorldServerConnect(NetworkMessage netMsg)
    {
        Debug.Log("OnWorldServerConnect");
        _worldclient.Send(EverMsgType.ZoneServerWorldAuthenticationRequest, new StringMessage(worldserverusername + "|" + worldserverpassword + "|" + listenhostname + "|" + listenport + "|" + SceneManager.GetActiveScene().name));
    }

    private void OnZoneServerWorldAuthenticationResponse(NetworkMessage netMsg)
    {
        string[] responsedata = netMsg.reader.ReadString().Split('|');
        if (responsedata[0].Equals("1"))
        {
            // authed
            _worldservertoken = responsedata[1];
        } else
        {
            Debug.Log("Failed to authenticate to world server");
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
