using UnityEngine;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class MulitplayerGameManager : MonoBehaviourPunCallbacks
{
    public static MulitplayerGameManager instance;
    [SerializeField] private TMP_InputField joinRoomField;
    [SerializeField] private TMP_Text generatedRoomCodeText;
    [SerializeField] private GameObject roomIDCreatePanel;
    bool gameStarted = false;

    private const int RoomCodeLength = 6;
   // private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string Characters = "0123456789";

    private int retryAttempts = 0;
    private const int MaxRetryAttempts = 5;
    private string pendingRoomCode;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        ConnectToPhoton();
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            // Prevent repeated scene loads
            if (!gameStarted)
            {
                gameStarted = true;
                SceneManager.LoadScene("FowlPlay_Game");
            }
        }
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
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

    public void CreateRoom()
    {
        TryCreateUniqueRoom();
    }

    private void TryCreateUniqueRoom()
    {
        pendingRoomCode = GenerateUniqueRoomCode();

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(pendingRoomCode, options);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created successfully: " + pendingRoomCode);
        if (generatedRoomCodeText != null)
        {
            generatedRoomCodeText.text = "YourRoomID : " + pendingRoomCode;
            roomIDCreatePanel.SetActive(true);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Room creation failed: {message}");

        retryAttempts++;
        if (retryAttempts <= MaxRetryAttempts)
        {
            Debug.Log($"Retrying... attempt {retryAttempts}");
            TryCreateUniqueRoom();
        }
        else
        {
            Debug.LogError("Max retry attempts reached. Could not create a unique room.");
            retryAttempts = 0; 
        }
    }

    private string GenerateUniqueRoomCode()
    {
        StringBuilder builder = new StringBuilder();
        System.Random random = new System.Random();

        for (int i = 0; i < RoomCodeLength; i++)
        {
            builder.Append(Characters[random.Next(Characters.Length)]);
        }

        return builder.ToString();
    }


    public void JoinRoom()
    {
        if (!string.IsNullOrEmpty(joinRoomField.text))
        {
            PhotonNetwork.JoinRoom(joinRoomField.text.ToUpper()); 
        }
        else
        {
            Debug.LogWarning("JoinRoom field is empty.");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined the room: " + PhotonNetwork.CurrentRoom.Name);
        //SceneManager.LoadScene("FowlPlay_Game");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Join room failed: {message}");
        // You can display a UI message like "Room not found"
    }

    public void CopyRoomCodeToClipboard()
    {
        GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
        Debug.Log("Room Code copied: " + PhotonNetwork.CurrentRoom.Name);
    }
}


