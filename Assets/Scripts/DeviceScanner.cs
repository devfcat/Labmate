using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DeviceScanner : MonoBehaviour
{
    public GameObject deviceButtonPrefab;
    public Transform contentHolder;

    private UdpClient udpClient;
    private int listenPort = 4210;

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

            AddDeviceToList(deviceID, senderIP);
        }
    }

    private void AddDeviceToList(string id, string ip)
    {
        // 중복 방지 로직 생략 가능
        GameObject btn = Instantiate(deviceButtonPrefab, contentHolder);
        btn.GetComponentInChildren<Text>().text = id;

        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log($"장치 선택됨: {id} / {ip}");
            // 여기에 연결 시도 로직 추가
        });
    }

    private void OnApplicationQuit()
    {
        udpClient?.Close();
    }
    
    IEnumerator SendConnectRequest(string ip)
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://{ip}/connect");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
            Debug.Log("연결 성공!");
        else
            Debug.Log("연결 실패");
    }
}

