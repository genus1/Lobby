using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkLobbyHook : LobbyHook {

	public override void OnLobbyServerSceneLoadedForPlayer (NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) {
        LobbyPlayer carInLobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        SetupLocalPlayer car = gamePlayer.GetComponent<SetupLocalPlayer>();

        //The names in my SetupLocalPlayer script MUST be SyncVar so server can update them
        car.pName = carInLobby.playerName;
        car.pColour = ColorUtility.ToHtmlStringRGBA(carInLobby.playerColor);
	}
	
}
