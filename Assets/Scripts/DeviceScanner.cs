using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeviceScanner : MonoBehaviour
{
    public GameObject deviceButtonPrefab;
    public Transform contentHolder;

    private UdpClient udpClient;
    private int listenPort = 4210;
    private Dictionary<string, bool> connectedDevices = new Dictionary<string, bool>();
    private Dictionary<string, Button> deviceButtons = new Dictionary<string, Button>();

    private void Start()
    {
        udpClient = new UdpClient(listenPort);
        ListenForDevices();
    }

    private async void ListenForDevices()
    {
        while (true)
        {
            var result = await udpClient.ReceiveAsync();
            string deviceID = Encoding.ASCII.GetString(result.Buffer);
            string senderIP = result.RemoteEndPoint.Address.ToString();

            Debug.Log($"받은 장치: {deviceID} from {senderIP}");

            if (!deviceButtons.ContainsKey(deviceID))
            {
                AddDeviceToList(deviceID, senderIP);
            }
        }
    }

    private void AddDeviceToList(string id, string ip)
    {
        GameObject btn = Instantiate(deviceButtonPrefab, contentHolder);
        Button button = btn.GetComponent<Button>();
        Text buttonText = btn.GetComponentInChildren<Text>();
        buttonText.text = id;

        deviceButtons[id] = button;
        connectedDevices[id] = false;

        button.onClick.AddListener(() =>
        {
            if (!connectedDevices[id])
            {
                StartCoroutine(ConnectToDevice(ip, id));
            }
            else
            {
                StartCoroutine(DisconnectFromDevice(ip, id));
            }
        });
    }

    private IEnumerator ConnectToDevice(string ip, string deviceId)
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://{ip}/connect");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"장치 연결 성공: {deviceId}");
            connectedDevices[deviceId] = true;
            UpdateButtonState(deviceId);
        }
        else
        {
            Debug.Log($"장치 연결 실패: {deviceId}");
        }
    }

    private IEnumerator DisconnectFromDevice(string ip, string deviceId)
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://{ip}/disconnect");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"장치 연결 해제: {deviceId}");
            connectedDevices[deviceId] = false;
            UpdateButtonState(deviceId);
        }
        else
        {
            Debug.Log($"장치 연결 해제 실패: {deviceId}");
        }
    }

    private void UpdateButtonState(string deviceId)
    {
        if (deviceButtons.TryGetValue(deviceId, out Button button))
        {
            ColorBlock colors = button.colors;
            colors.normalColor = connectedDevices[deviceId] ? Color.green : Color.white;
            button.colors = colors;
        }
    }

    private void OnApplicationQuit()
    {
        udpClient?.Close();
    }
}

