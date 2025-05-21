using UnityEngine;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class MulitplayerGameManager : MonoBehaviourPunCallbacks
{
    //public static MulitplayerGameManager instance;
    [SerializeField] private TMP_InputField joinRoomField;
    [SerializeField] private TMP_Text generatedRoomCodeText;
    [SerializeField] private TMP_Text forlobbyloadingText;
    [SerializeField] private GameObject roomIDCreatePanel;
    bool gameStarted = false;

    private const int RoomCodeLength = 6;
   
    
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    //private const string Characters = "0123456789";

    private int retryAttempts = 0;
    private const int MaxRetryAttempts = 5;
    private string pendingRoomCode;



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
        joinedLobby = true;
        //SceneManager.LoadScene("Lobby");
    }

    public void CreateRoom()
    {
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
        if (!joinedLobby)
        {
            forlobbyloadingText.text = "Not in lobby yet. Wait before joining.";
            Debug.LogWarning("Not in lobby yet. Wait before joining.");
            return;
        }
        TryCreateUniqueRoom();
    }
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
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

    bool joinedLobby = false;
    public void JoinRoom()
    {
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
        if (!joinedLobby)
        {
            forlobbyloadingText.text = "Not in lobby yet. Wait before joining.";
            Debug.LogWarning("Not in lobby yet. Wait before joining.");
            return;
        }

        string code = joinRoomField.text.Trim().ToUpper();
        if (!string.IsNullOrEmpty(code))
        {
            PhotonNetwork.JoinRoom(code);
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

    /*  public override void OnJoinRoomFailed(short returnCode, string message)
      {

          Debug.LogWarning($"Join room failed: {message}  {returnCode}");
          // You can display a UI message like "Room not found"
      }
  */

    public TMP_Text errorText;

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Join room failed: {message}  {returnCode}");

        switch (returnCode)
        {
            case 32758:
                forlobbyloadingText.text = "Room does not exist. Please check the code.";
                break;
            case 32764:
                forlobbyloadingText.text = "Room is full. Try a different one.";
                break;
            default:
                forlobbyloadingText.text = $"Join failed: {message}  {returnCode}";
                break;
        }
    }
    public void CopyRoomCodeToClipboard()
    {
        GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
        Debug.Log("Room Code copied: " + PhotonNetwork.CurrentRoom.Name);
        Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
    }
}


