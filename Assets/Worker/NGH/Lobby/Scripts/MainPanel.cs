using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainPanel : UIBInder
{
    //[SerializeField] GameObject menuPanel;
    //[SerializeField] GameObject createRoomPanel;
    //[SerializeField] TMP_InputField roomNameInputField;
    //[SerializeField] TMP_InputField maxPlayerInputField;

    private void Awake()
    {
        BindAll();

        AddEvent("CreateRoomButton", EventType.Click, CreateRoomMenu);
        AddEvent("RandomMatchingButton", EventType.Click, RandomMatching);
        AddEvent("CreateRoomConfirmButton", EventType.Click, CreateRoomConfirm);
        AddEvent("CreateRoomCancelButton", EventType.Click, CreateRoomCancel);
        AddEvent("LobbyButton", EventType.Click, JoinLobby);
        AddEvent("LogoutButton", EventType.Click, Logout);
        AddEvent("DeleteUserButton", EventType.Click, DeleteUser);
        AddEvent("DeleteUserCancelButton", EventType.Click, DeleteUserCancel);
        AddEvent("DeleteUserConfirmButton", EventType.Click, (eventData) =>
        {
            string email = GetUI<TMP_InputField>("DeleteUserEmailInputField").text;
            string password = GetUI<TMP_InputField>("DeleteUserPasswordInputField").text;

            DeleteUserConfirm(eventData, email, password);
        });
    }

    private void OnEnable()
    {
        GetUI("CreateRoomPanel").SetActive(false); //createRoomPanel.SetActive(false);
        GetUI("DeleteUserPanel").SetActive(false);
    }

    private void CreateRoomMenu(PointerEventData eventData)
    {
        GetUI("CreateRoomPanel").SetActive(true);

        GetUI<TMP_InputField>("RoomNameInputField").text = $"Room {Random.Range(1000, 10000)}"; //roomNameInputField.text = $"Room {Random.Range(1000, 10000)}";
        GetUI<TMP_InputField>("MaxPlayerInputField").text = "4"; //maxPlayerInputField.text = "8";
    }

    private void CreateRoomConfirm(PointerEventData eventData)
    {
        string roomName = GetUI<TMP_InputField>("RoomNameInputField").text;
        if (roomName == "")
        {
            Debug.LogWarning("�� �̸��� �����ؾ� ���� ������ �� �ֽ��ϴ�.");
            return;
        }

        int maxPlayer = int.Parse(GetUI<TMP_InputField>("MaxPlayerInputField").text);
        maxPlayer = Mathf.Clamp(maxPlayer, 4, 4);

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayer;

        PhotonNetwork.CreateRoom(roomName, options);
    }

    private void CreateRoomCancel(PointerEventData eventData)
    {
        GetUI("CreateRoomPanel").SetActive(false);
    }

    private void RandomMatching(PointerEventData eventData)
    {
        Debug.Log("���� ��Ī ��û");

        // ��� �ִ� ���� ������ ���� �ʴ� ���
        // PhotonNetwork.JoinRandomRoom();

        // ��� �ִ� ���� ������ ���� ���� ���� ���� ���
        string name = $"Room {Random.Range(1000, 10000)}";
        RoomOptions options = new RoomOptions() { MaxPlayers = 4 };
        PhotonNetwork.JoinRandomOrCreateRoom(roomName : name, roomOptions : options);
    }

    private void JoinLobby(PointerEventData eventData)
    {
        Debug.Log("�κ� ���� ��û");
        PhotonNetwork.JoinLobby();
    }

    private void Logout(PointerEventData eventData)
    {
        Debug.Log("�α׾ƿ� ��û");
        PhotonNetwork.Disconnect();
    }

    private void DeleteUser(PointerEventData eventData)
    {
        GetUI("DeleteUserPanel").SetActive(true);
    }

    private void DeleteUserCancel(PointerEventData eventData)
    {
        GetUI("DeleteUserPanel").SetActive(false);
    }

    private void DeleteUserConfirm(PointerEventData eventData, string email, string password)
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user is currently logged in.");
            return;
        }

        // ����� ���� ���� ����
        Credential credential = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);

        // ������ ����
        user.ReauthenticateAsync(credential)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Reauthentication was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("Reauthentication encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("Reauthentication successful. Proceeding with account deletion.");

                // ���� ����
                user.DeleteAsync()
                    .ContinueWithOnMainThread(deleteTask =>
                    {
                        if (deleteTask.IsCanceled)
                        {
                            Debug.LogError("DeleteAsync was canceled.");
                            return;
                        }
                        if (deleteTask.IsFaulted)
                        {
                            Debug.LogError("DeleteAsync encountered an error: " + deleteTask.Exception);
                            return;
                        }

                        Debug.Log("User deleted successfully.");
                        PhotonNetwork.Disconnect();
                    });
            });
    }
}