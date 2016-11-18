using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using System.Linq;

public class WorldServerController : MonoBehaviour
{
    public string listenhostname;
    public int listenport;
    public string loginserverhostname;
    public int loginserverport;
    private NetworkClient _loginclient;

    public string loginserver_username;
    public string loginserver_password;
    public string worldname;

    private string _loginservertoken = "";

    private Dictionary<int, string> _pendingconnectiontokens = new Dictionary<int, string>();
    private Dictionary<int, string> _connectiontokens = new Dictionary<int, string>();

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;

        NetworkServer.Listen(listenport);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnect);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        NetworkServer.RegisterHandler(MsgType.Error, OnError);
        NetworkServer.RegisterHandler(EverMsgType.WorldServerUserConnectionRequest, OnWorldServerUserConnectionRequest);

        // Login Server Client
        _loginclient = new NetworkClient();
        _loginclient.Connect(loginserverhostname, loginserverport);
        _loginclient.RegisterHandler(MsgType.Connect, OnConnect);
        _loginclient.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        _loginclient.RegisterHandler(MsgType.Error, OnError);
        _loginclient.RegisterHandler(EverMsgType.WorldServerLoginAuthenticationResponse, OnWorldServerLoginAuthenticationResponse);
        _loginclient.RegisterHandler(EverMsgType.WorldServerUserValidationResponse, OnWorldServerUserValidationResponse);
    }

    private void OnWorldServerUserConnectionRequest(NetworkMessage netMsg)
    {
        string connectionBundle = netMsg.reader.ReadString();
        MarkConnectionAsPending(netMsg.conn.connectionId, connectionBundle);
        _loginclient.Send(EverMsgType.WorldServerUserValidationRequest, new StringMessage(netMsg.conn.connectionId + "|" + connectionBundle));
    }

    public void MarkConnectionAsPending(int connectionId, string tokenBundle)
    {
        _pendingconnectiontokens[connectionId] = tokenBundle;
        InvalidateConnectionToken(connectionId);
    }

    private void InvalidateConnectionToken(int connId)
    {
        _connectiontokens.Remove(connId);
    }

    private void InvalidatePendingConnectionToken(int connId)
    {
        _pendingconnectiontokens.Remove(connId);
    }

    public void MarkConnectionAsValidated(int connectionId, string tokenBundle)
    {
        _connectiontokens[connectionId] = tokenBundle;
        InvalidatePendingConnectionToken(connectionId);
    }

    public string BuildConnectionRequestResponse(bool validated)
    {
        string suceeded = "0";
        string charselectzoneip = "";
        string charselectzoneport = "";
        if (validated == true)
        {
            suceeded = "1";
            charselectzoneip = GetCharSelectZoneIP();
            charselectzoneport = GetCharSelectZonePort();
        }

        return suceeded + "|" + charselectzoneip + "|" + charselectzoneport;
    }

    public string GetCharSelectZoneIP()
    {
        return "";
    }

    public string GetCharSelectZonePort()
    {
        return "";
    }

    private void OnWorldServerUserValidationResponse(NetworkMessage netMsg)
    {
        string response = netMsg.reader.ReadString();
        string[] responseData = response.Split('|');
        string tokenBundle = responseData[2] + "|" + responseData[3];

        if (responseData[0].Equals("1"))
        {
            MarkConnectionAsValidated(int.Parse(responseData[1]), tokenBundle);
        }
        else
        {
            InvalidatePendingConnectionToken(int.Parse(responseData[1]));
            NetworkServer.SendToClient(int.Parse(responseData[1]), EverMsgType.WorldServerUserConnectionResponse, new StringMessage(BuildConnectionRequestResponse(true)));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public string BuildLoginServerAuthenticationPacket()
    {
        string authenticationpacket = loginserver_username + "|" + loginserver_password + "|" + listenhostname + "|" + listenport + "|" + worldname;
        return authenticationpacket;
    }

    private void AuthToLoginServer()
    {
        Debug.Log("Authenticating to Login Server");
        _loginclient.Send(EverMsgType.WorldServerLoginAuthenticationRequest, new StringMessage(BuildLoginServerAuthenticationPacket()));
    }

    private void OnError(NetworkMessage netMsg)
    {
        Debug.Log("OnError");
    }

    private void OnDisconnect(NetworkMessage netMsg)
    {
        Debug.Log("OnDisconnect");
    }

    private void OnConnect(NetworkMessage netMsg)
    {
        Debug.Log("OnConnect");
        AuthToLoginServer();
    }

    private void OnWorldServerLoginAuthenticationResponse(NetworkMessage netMsg)
    {
        string[] worldServerLoginAuthenticationResponse = netMsg.reader.ReadString().Split('|');
        if (worldServerLoginAuthenticationResponse[0].Equals("1"))
        {
            Debug.Log("OnWorldServerAuthenticationRequestSucceeded");
            _loginservertoken = worldServerLoginAuthenticationResponse[1];
        }
        else
        {
            Debug.Log("OnWorldServerAuthenticationRequestFailed");
        }
    }
}
