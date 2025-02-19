﻿using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;



public class JoinGame : MonoBehaviour {

    List<GameObject> roomList = new List<GameObject>();

    [SerializeField]
    private Text status;

    [SerializeField]
    private GameObject roomListItemPrefab;

    [SerializeField]
    private Transform roomListParent;

    private NetworkManager networkManager;

     void Start()
    {
        networkManager = NetworkManager.singleton;
        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        RefreshRoomList();
    }

    public void RefreshRoomList()
    {
        ClearRoomList();

        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

        networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
        status.text = "Loading...";
    }

    public void OnMatchList(bool success, string extendedInfo,List<MatchInfoSnapshot> matchList)
    {
        status.text = "";

        if( !success||matchList == null)
        {
            status.text = "Couldn't get room list.";
            return;
        }

        ClearRoomList();
        foreach(MatchInfoSnapshot match in matchList)
        {
            GameObject _roomListItemGO = Instantiate(roomListItemPrefab);
            _roomListItemGO.transform.SetParent(roomListParent);

            //Have a component sit on the gameobject
            RoomListItem _roomListItem = _roomListItemGO.GetComponent<RoomListItem>();
            if(_roomListItem != null)
            {
                _roomListItem.Setup(match,JoinRoom);
            }

            
            //as well as setting up a call back function that will join the game.

            roomList.Add(_roomListItemGO);
        }

        if(roomList.Count == 0)
        {
            status.text = "No rooms at the moment.";
        }
    }

    void ClearRoomList()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            Destroy(roomList[i]);
        }

        roomList.Clear();
    }

    public void JoinRoom(MatchInfoSnapshot _match)
    {
        networkManager.matchMaker.JoinMatch(_match.networkId, "", "","",0,0,
                                                networkManager.OnMatchJoined);
        StartCoroutine(WaitForJoin());
    }

    IEnumerator WaitForJoin()
    {
        ClearRoomList();

        int countdown = 60;
        while(countdown>0)
        {
            status.text = "Joining... (" + countdown+ ")";

            yield return new WaitForSeconds(1);

            countdown--;
        }

        // Fail to connect
        status.text = "Fail to connect";
        yield return new WaitForSeconds(2);

        MatchInfo matchInfo = networkManager.matchInfo;
        if(matchInfo != null)
        {
            networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0,
                                                             networkManager.OnDropConnection);
            networkManager.StopHost();
        }
        
        RefreshRoomList();

    }

}
