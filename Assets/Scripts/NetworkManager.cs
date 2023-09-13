using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    //포톤 뷰가 달린 오브젝트는 다른 클라이언트에도 달려있기때문에
    //포톤 뷰 컴포넌트가 똑같이 동기화를 해줌
    //근데 동기화 하고싶은 함수에
    //PunRPC(Remote Procedure Call)원격 프로시저(함수의 하위 개념) 호출
    //로 PhotonView.RPC("함수명" 이런식으로 호출해야 딴데서 서버가 실행하고 반환함
    //포톤 트랜스폼 뷰같은걸 달고 연결해주면 트랜스폼은 따로 안해줘도 동기화해줌

    [Header("DisconnectPanel")]
    public InputField IDInput;

    [Header("LobbyPanel")]
    public GameObject StartPanel;
    public GameObject LoginPanel;
    public GameObject LobbyPanel;
    public GameObject RankingPanel;
    public InputField RoomInput;
    public Text IDText;
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;

    [Header("RoomPanel")]
    public GameObject RoomPanel;
    public Text RoomInfoText;
    public Text RoomNameText;
    public Text[] ChatText;
    public InputField ChatInput;
    public Text logText;

    [Header("ETC")]
    public PhotonView PV;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    #region 네트워크 아닌거
    private void Awake()
    {
        //화면 사이즈
        Screen.SetResolution(800, 800, false);
    }

    public void SoloBtn()
    {
        SceneManager.LoadScene("Blackjack");
    }

    public void MultiMode()
    {
        StartPanel.SetActive(false);
        LoginPanel.SetActive(true);
    }

    public void BackStartBtn()
    {
        LoginPanel.SetActive(false);
        StartPanel.SetActive(true);
    }

    public void BackLobbyBtn()
    {
        RankingPanel.SetActive(false);
        LobbyPanel.SetActive(true);
    }

    public void RankingBtn()
    {
        LobbyPanel.SetActive(false);
        RankingPanel.SetActive(true);
    }

    //게임종료버튼
    public void EndApp()
    {
        Application.Quit();
    }
    #endregion

    #region 방리스트 갱신
    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    // 스위치같은 느낌으로 변수마다 다른 동작 
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        MyListRenewal();
    }

    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage > 1) ? true : false;
        NextBtn.interactable = (currentPage < maxPage) ? true : false;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    #endregion


    #region 서버연결

    //람다식은 익명 함수 결국 함수를 파라미터로 넣고싶을때 쓰는거
    //컴파일러가 알아서 유추해서 반환 타입도 필요없음
    //정의된곳에서만 쓰고 다른곳에서 호출할일이 없으니 접근제어자랑 이름도 필요없음

    //로그인 버튼
    public void Connect() 
    {
        if (IDInput.text == "")
        {
            logText.text = "아이디를 입력해주세요";
            return;
        }
        //서버에 연결
        PhotonNetwork.ConnectUsingSettings();
    }

    //마스터 서버에 연결시 로비로 연결
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    //로비 연결시
    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);
        PhotonNetwork.LocalPlayer.NickName = IDInput.text;
        IDText.text = "아이디 : " + PhotonNetwork.LocalPlayer.NickName;
        myList.Clear();
    }

    //서버 접속 종료
    //로비패널의 엑스버튼
    public void Disconnect() => PhotonNetwork.Disconnect();

    //서버 연결 종료시
    public override void OnDisconnected(DisconnectCause cause)
    {
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
    }
    #endregion


    #region 방

    //방만들기
    //방 이름 입력이 안되있으면 랜덤 생성
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });

    //빠른시작
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    //방나가기버튼
    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    //방 접속 메서드들 오버라이드
    //방 접속 성공시
    public override void OnJoinedRoom()
    {
        RoomPanel.SetActive(true);
        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
    }

    //방 생성 실패시(방제가 똑같을때나 서버연결이 불안정할떄)
    //다시 방 생성 시도
    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); } 

    //빠른 입장 실패시(들어갈 수 있는 방이 하나도 없을때)
    //다시 방 생성 시도
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    //방에 누가 들어올때 텍스트 갱신
    //누가 들어왔다고 알림 채팅
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    //방에서 누가 나갈때
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    void RoomRenewal()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            //"접속자 : " + PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        }
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        RoomInfoText.text = PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
    }
    #endregion


    #region 채팅

    //채팅 보내기 버튼
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.AllBuffered, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    
    
    [PunRPC]
    void ChatRPC(string msg)
    {
        bool isInput = false;
        //비어있는 텍스트칸에 채팅넣기
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++)
            {
                ChatText[i - 1].text = ChatText[i].text;
            }
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion
}
