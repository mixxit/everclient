using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.IsolatedStorage;

public class LoginServerController : MonoBehaviour
{
    public int port;
    private List<Account> _accountlist = new List<Account>();
    private Dictionary<string, string> _tokens = new Dictionary<string, string>();

    private List<WorldServer> _worldaccounts = new List<WorldServer>();

    private Dictionary<string, ActiveWorldServer> _activeworldservers = new Dictionary<string, ActiveWorldServer>();

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;

        NetworkServer.Listen(port);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnect);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        NetworkServer.RegisterHandler(MsgType.Error, OnError);
        NetworkServer.RegisterHandler(EverMsgType.ClientLoginAuthenticationRequest, OnClientLoginAuthenticationRequest);

        NetworkServer.RegisterHandler(EverMsgType.WorldServerLoginAuthenticationRequest, OnWorldServerLoginAuthenticationRequest);

        NetworkServer.RegisterHandler(EverMsgType.ServerSelectionListRequest, OnServerSelectionListRequest);

        LoadAccounts();
        LoadWorldServers();
    }

    private void OnServerSelectionListRequest(NetworkMessage netMsg)
    {
        string[] request = netMsg.reader.ReadString().Split('|');
        Debug.Log("Checking ServerSelection Token for: " + request[0]);
        if (!IsTokenValid(request[0], request[1]))
        {
            Debug.Log("Ignoring Invalid Token message");
            return;
        }
        Debug.Log("Sending Server List");
        NetworkServer.SendToClient(netMsg.conn.connectionId, EverMsgType.ServerSelectionListResponse, new StringMessage(BuildServerListPacket()));
    }

    private string BuildServerListPacket()
    {
        string packet = "";

        foreach (ActiveWorldServer activeworld in _activeworldservers.Values)
        {
            String srvtxt = activeworld.hostname + "|" + activeworld.port + "|" + activeworld.name;
            Debug.Log("Adding Server Select: " + srvtxt);
            packet += srvtxt + "^";
        }

        if (!String.IsNullOrEmpty(packet))
        {
            packet = packet.Substring(0, packet.Length - 1);
        }

        return packet;
    }

    private bool IsWorldServerTokenValid(string username, string token)
    {
        return _activeworldservers[username].token.Equals(token);
    }

    private bool IsTokenValid(string username, string token)
    {
        Debug.Log(username + "|" + token);
        Debug.Log("vs count " + _tokens.Count);
        foreach (string key in _tokens.Keys)
        {
            Debug.Log(key + ":");
            Debug.Log(key + " " + _tokens[key]);
        }
        return _tokens[username].Equals(token);
    }

    private void LoadAccounts()
    {
        _accountlist.Clear();

        try
        {
            String accounts = File.ReadAllText("accounts.txt");
            string[] accountlines = accounts.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string accountline in accountlines)
            {
                if (String.IsNullOrEmpty(accountline))
                    continue;

                Debug.Log("Parsing: " + accountline);
                string[] accountdetails = accountline.Split('|');
                Account account = new Account();
                account.username = accountdetails[0];
                account.password = accountdetails[1];

                _accountlist.Add(account);
            }
        }
        catch (FileNotFoundException filenotfoundexception)
        {
            File.WriteAllText("accounts.txt", "admin|password" + Environment.NewLine);
            Account account = new Account();
            account.username = "admin";
            account.password = "password";
            _accountlist.Add(account);
        } catch (IsolatedStorageException isolatedstorageexception)
        {
            File.WriteAllText("accounts.txt", "admin|password" + Environment.NewLine);
            Account account = new Account();
            account.username = "admin";
            account.password = "password";
            _accountlist.Add(account);
        }
    }

    private void LoadWorldServers()
    {
        _worldaccounts.Clear();

        try
        {
            String accounts = File.ReadAllText("worldservers.txt");
            string[] accountlines = accounts.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string accountline in accountlines)
            {
                if (String.IsNullOrEmpty(accountline))
                    continue;

                Debug.Log("Parsing: " + accountline);
                string[] accountdetails = accountline.Split('|');
                WorldServer worldserver = new WorldServer();
                worldserver.username = accountdetails[0];
                worldserver.password = accountdetails[1];
                _worldaccounts.Add(worldserver);
            }
        }
        catch (FileNotFoundException filenotfoundexception)
        {
            File.WriteAllText("worldservers.txt", "worldtest|password" + Environment.NewLine);
            WorldServer worldserver = new WorldServer();
            worldserver.username = "worldtest";
            worldserver.password = "password";
            _worldaccounts.Add(worldserver);
        }
        catch (IsolatedStorageException isolatedstoragexception)
        {
            File.WriteAllText("worldservers.txt", "worldtest|password" + Environment.NewLine);
            WorldServer worldserver = new WorldServer();
            worldserver.username = "worldtest";
            worldserver.password = "password";
            _worldaccounts.Add(worldserver);
        }
    }

    public bool IsClientAccountValid(string username, string password)
    {
        if (_accountlist.Where(a => a.username.Equals(username) && a.password.Equals(password)).ToList<Account>().Count > 0)
            return true;

        return false;
    }

    private void OnError(NetworkMessage netMsg)
    {
        Debug.Log("Connection error");
    }

    private void OnDisconnect(NetworkMessage netMsg)
    {
        Debug.Log("Client disconnected");
    }

    private void OnConnect(NetworkMessage netMsg)
    {
        Debug.Log("Client connected");
    }

    private void OnConnect(StringMessage netMsg)
    {
        Debug.Log("Received Auth Request: " + netMsg.value);
    }

    private Guid AssignNewToken(string username)
    {
        Guid newguid = Guid.NewGuid();
        _tokens[username] = newguid.ToString();
        return newguid;
    }

    private Guid AssignNewWorldServerToken(ActiveWorldServer worldserver)
    {
        Guid newguid = Guid.NewGuid();
        worldserver.token = newguid.ToString();
        _activeworldservers[worldserver.username] = worldserver;
        return newguid;
    }

    private void OnWorldServerLoginAuthenticationRequest(NetworkMessage netMsg)
    {
        Debug.Log("OnWorldServerAuthenticationRequest");
        string[] request = netMsg.reader.ReadString().Split('|');
        //loginserver_username + "|" + loginserver_password + "|" + listenhostname + "|" + listenport + "|" + worldname

        Debug.Log("Received WorldAuth Request: " + request[0] + "|" + request[1] + "|" + request[2] + "|" + request[3] + "|" + request[4]);
        if (IsWorldAccountValid(request[0], request[1]))
        {
            ActiveWorldServer worldserver = new ActiveWorldServer();
            worldserver.username = request[0];
            worldserver.hostname = request[2];
            worldserver.port = Convert.ToDecimal(request[3]);
            worldserver.name = request[4];

            Guid token = AssignNewWorldServerToken(worldserver);
            NetworkServer.SendToClient(netMsg.conn.connectionId, EverMsgType.WorldServerLoginAuthenticationResponse, new StringMessage("1|" + token.ToString()));
        }
        else
        {
            NetworkServer.SendToClient(netMsg.conn.connectionId, EverMsgType.WorldServerLoginAuthenticationResponse, new StringMessage("0|0"));
        }
    }

    public bool IsWorldAccountValid(string username, string password)
    {
        if (_worldaccounts.Where(a => a.username.Equals(username) && a.password.Equals(password)).ToList<WorldServer>().Count > 0)
            return true;

        return false;
    }


    private void OnClientLoginAuthenticationRequest(NetworkMessage netMsg)
    {
        string[] request = netMsg.reader.ReadString().Split('|');
        Debug.Log("Received Auth Request: " + request[0] + "|" + request[1]);
        if (IsClientAccountValid(request[0], request[1]))
        {
            Guid token = AssignNewToken(request[0]);
            NetworkServer.SendToClient(netMsg.conn.connectionId, EverMsgType.ClientLoginAuthenticationResponse, new StringMessage("1|"+token.ToString()));
        }
        else
        {
            NetworkServer.SendToClient(netMsg.conn.connectionId, EverMsgType.ClientLoginAuthenticationResponse, new StringMessage("0|0"));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
