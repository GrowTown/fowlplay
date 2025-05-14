using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    public static MultiplayerManager instance;
    public GameObject gameplayPanel;
    public GameObject lobbyPanel;
    public GameObject EndPanel;
    public Button joinRandomMatchButton;
    public Button checkRoomStatusButton;
    public InputField Player_One_Input_Field;
    public InputField Player_Two_Input_Field;
    public TMP_InputField creatRoomField;
    public TMP_InputField joinRoomField;
    public Button restartGame;
    public Button quitGame;
    public Text winnerText;
    private string Player_One_Name;
    private string Player_Two_Name;
    private bool isMyTurn = false;

    public const byte SetTurnEventCode = 1;
    public const byte StartGameEventCode = 2;
    public const byte RestartGameEventCode = 3;
    public const byte QuitGameEventCode = 4;
    public const byte EndGameEventCode = 5;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        ConnectToPhoton();
        gameplayPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void JoinRandomMatch()
    {
        joinRandomMatchButton.interactable = false;
        checkRoomStatusButton.interactable = false;
        PhotonNetwork.JoinRandomRoom();
    }

    public void CheckRoomStatus()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Connected to Photon");
            if (PhotonNetwork.InRoom)
            {
                Debug.Log("In a room" + PhotonNetwork.CurrentRoom.Name);
            }
            else
            {
                Debug.Log("Not in a room");
            }
        }
        else
        {
            Debug.Log("Not connected to Photon");
        }
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }


    /* public override void OnConnectedToMaster()
     {
         Debug.Log("Connected to Photon Master Server");
         joinRandomMatchButton.interactable = true;
         checkRoomStatusButton.interactable = true;
     }*/



    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a random room" + message);
        //CreateAndJoinRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room" + PhotonNetwork.CurrentRoom.Name);

        SceneManager.LoadScene("FowlPlay_Game");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            GameManager.instance.isFirstPlayer = true;
            Debug.Log("Player 1: Game Started");
            GameManager.instance.waitingPanel.SetActive(true);
            lobbyPanel.SetActive(false);
            isMyTurn = true;
            Player_One_Name = Player_One_Input_Field.text;
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("Plaeyr 2: Game Started");
            RaiseStartGameEvent();
            Player_Two_Name = Player_Two_Input_Field.text;
        }
    }

    public void CreatRoom()
    {
        //string roomName = "Room" + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(creatRoomField.text, options);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinRoomField.text);
    }

    public void OnSetTurn(int index)
    {
        if (!isMyTurn)
            return;

        string currentPlayer = GameManager.instance.GetPlayersTurn(GameManager.instance.isFirstPlayer);
        TileManager tileManager = GameManager.instance.tileManagerList[index];
        tileManager.Internal_Text.text = currentPlayer;
        tileManager.Interactive_Button.image.sprite = currentPlayer == "X" ? GameManager.instance.spriteX : GameManager.instance.spriteO;
        tileManager.Interactive_Button.interactable = false;
        GameManager.instance.EndTurn();
        isMyTurn = false;

        object[] eventData = new object[] { index, currentPlayer };
        RaiseSetTurnEvent(eventData);
    }

    private void RaiseSetTurnEvent(object[] eventData)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(SetTurnEventCode, eventData, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnSetTurnEventRecieved(EventData photonEvent)
    {
        object[] eventData = (object[])photonEvent.CustomData;
        int index = (int)eventData[0];
        string currentPlayer = (string)eventData[1];

        TileManager tileManager = GameManager.instance.tileManagerList[index];
        tileManager.Internal_Text.text = currentPlayer;
        tileManager.Interactive_Button.image.sprite = currentPlayer == "X" ? GameManager.instance.spriteX : GameManager.instance.spriteO;
        tileManager.Interactive_Button.interactable = false;
        GameManager.instance.EndTurn(true);
        isMyTurn = true;
    }

    private void RaiseStartGameEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(StartGameEventCode, null, raiseEventOptions, SendOptions.SendReliable);
        OnStartGameEventReceived(null);
    }

    private void OnStartGameEventReceived(EventData photonEvent)
    {
        GameManager.instance.waitingPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        isMyTurn = GameManager.instance.isFirstPlayer;
        if (isMyTurn)
        {
            Debug.Log("Your turn: " + GameManager.instance.GetPlayersTurn(GameManager.instance.isFirstPlayer));
        }
        else
        {
            Debug.Log("Opponent's turn: " + GameManager.instance.GetPlayersTurn(GameManager.instance.isFirstPlayer));
        }
    }
    public void RestartGame()
    {
        RaiseRestartGameEvent();
    }

    private void RaiseRestartGameEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(RestartGameEventCode, null, raiseEventOptions, SendOptions.SendReliable);
        OnRestartGameEventReceived(null);
    }

    private void OnRestartGameEventReceived(EventData photonEvent)
    {
        GameManager.instance.turnCount = 0;
        GameManager.instance.EndPanel.SetActive(false);
        for (int i = 0; i < GameManager.instance.tileManagerList.Length; i++)
        {
            GameManager.instance.tileManagerList[i].GetComponentInParent<TileManager>().ResetTile();
        }
    }

    public void QuitGame()
    {
        RaiseQuitGameEvent();
    }

    private void RaiseQuitGameEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(QuitGameEventCode, null, raiseEventOptions, SendOptions.SendReliable);
        OnQuitGameEventReceived(null);
    }

    private void OnQuitGameEventReceived(EventData photonEvent)
    {
        Application.Quit();
    }

    private void RaiseEndGameEvent()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(EndGameEventCode, null, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnEndGameEventReceived(EventData photonEvent)
    {
        GameManager.instance.EndTurn();
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == SetTurnEventCode)
        {
            OnSetTurnEventRecieved(photonEvent);
        }
        else if (photonEvent.Code == StartGameEventCode)
        {
            OnStartGameEventReceived(photonEvent);
        }
        else if (photonEvent.Code == RestartGameEventCode)
        {
            OnRestartGameEventReceived(photonEvent);
        }
        else if (photonEvent.Code == QuitGameEventCode)
        {
            OnQuitGameEventReceived(photonEvent);
        }
        else if (photonEvent.Code == EndGameEventCode)
        {
            OnEndGameEventReceived(photonEvent);
        }
    }
    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}