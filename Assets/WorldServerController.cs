using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.IsolatedStorage;

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

    private List<ZoneServer> _zoneaccounts = new List<ZoneServer>();

    private Dictionary<string, ActiveZoneServer> _activezoneservers = new Dictionary<string, ActiveZoneServer>();
    private Dictionary<int, ActiveZoneServer> _pendinguserzonebootup = new Dictionary<int, ActiveZoneServer>();

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
        LoadWorldServers();

        NetworkServer.Listen(listenport);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnect);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        NetworkServer.RegisterHandler(MsgType.Error, OnError);
        NetworkServer.RegisterHandler(EverMsgType.WorldServerUserConnectionRequest, OnWorldServerUserConnectionRequest);
        NetworkServer.RegisterHandler(EverMsgType.ZoneServerWorldAuthenticationRequest, OnZoneServerWorldAuthenticationRequest);
        NetworkServer.RegisterHandler(EverMsgType.ZoneServerWorldChangeSceneResponse, OnZoneServerWorldChangeSceneResponse);

        // Login Server Client
        _loginclient = new NetworkClient();
        _loginclient.Connect(loginserverhostname, loginserverport);
        _loginclient.RegisterHandler(MsgType.Connect, OnConnect);
        _loginclient.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        _loginclient.RegisterHandler(MsgType.Error, OnError);
        _loginclient.RegisterHandler(EverMsgType.WorldServerLoginAuthenticationResponse, OnWorldServerLoginAuthenticationResponse);
        _loginclient.RegisterHandler(EverMsgType.WorldServerUserValidationResponse, OnWorldServerUserValidationResponse);
    }

    private void OnZoneServerWorldChangeSceneResponse(NetworkMessage netMsg)
    {
        Debug.Log("Zone Server is changing scene");
    }

    private void OnZoneServerWorldAuthenticationRequest(NetworkMessage netMsg)
    {
        string[] requestdata = netMsg.reader.ReadString().Split('|');

        if (IsZoneAccountValid(requestdata[0], requestdata[1]))
        {
            ActiveZoneServer zoneserver = new ActiveZoneServer();
            zoneserver.username = requestdata[0];
            zoneserver.hostname = requestdata[2];
            zoneserver.port = Convert.ToDecimal(requestdata[3]);
            zoneserver.zonename = requestdata[4];
            zoneserver.connid = netMsg.conn.connectionId;

            Guid token = AssignNewZoneServerToken(zoneserver);
            NetworkServer.SendToClient(netMsg.conn.connectionId, EverMsgType.ZoneServerWorldAuthenticationResponse, new StringMessage("1|" + token.ToString()));

            // Locate any users pending this zone bootup and send them to it

            List<int> connidstoremove = new List<int>();
            foreach (KeyValuePair<int, ActiveZoneServer> pendinguser in _pendinguserzonebootup)
            {
                if (!pendinguser.Value.username.Equals(zoneserver.username))
                    continue;

                NetworkServer.SendToClient(pendinguser.Key, EverMsgType.WorldServerUserConnectToZoneRequest, new StringMessage(zoneserver.hostname + "|" + zoneserver.port));
                connidstoremove.Add(pendinguser.Key);
            }

            foreach (int connid in connidstoremove)
            {
                _pendinguserzonebootup.Remove(connid);
            }
        }
        else
        {
            NetworkServer.SendToClient(netMsg.conn.connectionId, EverMsgType.ZoneServerWorldAuthenticationResponse, new StringMessage("0|0"));
        }
    }

    private Guid AssignNewZoneServerToken(ActiveZoneServer zoneserver)
    {
        Guid newguid = Guid.NewGuid();
        zoneserver.token = newguid.ToString();
        _activezoneservers[zoneserver.username] = zoneserver;
        return newguid;
    }

    public bool IsZoneAccountValid(string username, string password)
    {
        if (_zoneaccounts.Where(a => a.username.Equals(username) && a.password.Equals(password)).ToList<ZoneServer>().Count > 0)
            return true;

        return false;
    }

    private void LoadWorldServers()
    {
        _zoneaccounts.Clear();

        try
        {
            String accounts = File.ReadAllText("zoneservers.txt");
            string[] accountlines = accounts.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string accountline in accountlines)
            {
                if (String.IsNullOrEmpty(accountline))
                    continue;

                Debug.Log("Parsing: " + accountline);
                string[] accountdetails = accountline.Split('|');
                ZoneServer zoneserver = new ZoneServer();
                zoneserver.username = accountdetails[0];
                zoneserver.password = accountdetails[1];
                _zoneaccounts.Add(zoneserver);
            }
        }
        catch (FileNotFoundException filenotfoundexception)
        {
            File.WriteAllText("zoneservers.txt", "zonetest|password" + Environment.NewLine);
            ZoneServer zoneserver = new ZoneServer();
            zoneserver.username = "zonetest";
            zoneserver.password = "password";
            _zoneaccounts.Add(zoneserver);
        }
        catch (IsolatedStorageException isolatedstoragexception)
        {
            File.WriteAllText("zoneservers.txt", "zonetest|password" + Environment.NewLine);
            ZoneServer zoneserver = new ZoneServer();
            zoneserver.username = "zonetest";
            zoneserver.password = "password";
            _zoneaccounts.Add(zoneserver);
        }
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

    public string BuildConnectionRequestResponse(bool validated, string zonehostname, decimal zoneport)
    {
        string suceeded = "0";
        if (validated == true)
        {
            suceeded = "1";
        }

        return suceeded + "|" + zonehostname + "|" + zoneport;
    }

    private void OnWorldServerUserValidationResponse(NetworkMessage netMsg)
    {
        string response = netMsg.reader.ReadString();
        string[] responseData = response.Split('|');
        string tokenBundle = responseData[2] + "|" + responseData[3];

        if (responseData[0].Equals("1"))
        {
            MarkConnectionAsValidated(int.Parse(responseData[1]), tokenBundle);

            // Send to char select
            MovePlayerToZoneServer(netMsg.conn.connectionId, "charselect");
        }
        else
        {
            InvalidatePendingConnectionToken(int.Parse(responseData[1]));
            NetworkServer.SendToClient(int.Parse(responseData[1]), EverMsgType.WorldServerUserConnectionResponse, new StringMessage(BuildConnectionRequestResponse(false, "", 0)));
        }
    }

    public void MovePlayerToZoneServer(int connectionid, string zonename)
    {
        ActiveZoneServer targetzoneserver = new ActiveZoneServer();

        bool locatedzone = false;

        foreach(KeyValuePair<string, ActiveZoneServer> keypair in _activezoneservers)
        {
            if (keypair.Value.zonename.Equals(zonename))
            {
                targetzoneserver = keypair.Value;
                locatedzone = true;
                break;
            }
        }

        if (!locatedzone)
        {
            // Boot up a zone
            foreach (KeyValuePair<string, ActiveZoneServer> keypair in _activezoneservers)
            {
                if (keypair.Value.zonename.Equals("inactivezoneserver"))
                {
                    NetworkServer.SendToClient(keypair.Value.connid, EverMsgType.ZoneServerWorldChangeSceneRequest, new StringMessage());

                    ActiveZoneServer activezoneserver = new ActiveZoneServer();
                    activezoneserver = keypair.Value;
                    activezoneserver.zonename = zonename;

                    _pendinguserzonebootup[connectionid] = activezoneserver;
                    break;
                }
            }
        } else
        {
            NetworkServer.SendToClient(connectionid, EverMsgType.WorldServerUserConnectionResponse, new StringMessage(BuildConnectionRequestResponse(false, targetzoneserver.hostname, targetzoneserver.port)));
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
