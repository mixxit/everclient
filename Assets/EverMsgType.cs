using UnityEngine;
using System.Collections;

public class EverMsgType
{
    public const int ClientLoginAuthenticationRequest = 1000;
    public const int ClientLoginAuthenticationResponse = 1001;

    public const int WorldServerLoginAuthenticationRequest = 1100;
    public const int WorldServerLoginAuthenticationResponse = 1101;
    public const int WorldServerUserValidationRequest = 1105;
    public const int WorldServerUserValidationResponse = 1106;

    public const int WorldServerUserConnectionRequest = 1107;
    public const int WorldServerUserConnectionResponse = 1108;

    public const int ZoneServerClientAuthenticationRequest = 2001;
    public const int ZoneServerClientAuthenticationResponse = 2002;
    public const int ZoneServerWorldAuthenticationRequest = 2003;
    public const int ZoneServerWorldAuthenticationResponse = 2004;

    public const int ZoneServerWorldChangeSceneRequest = 2005;
    public const int ZoneServerWorldChangeSceneResponse = 2006;

    public const int ServerSelectionListRequest = 3001;
    public const int ServerSelectionListResponse = 3002;
}
