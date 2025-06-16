using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

/// <summary>
/// 장치 설정 패널의 장치 감지 및 UI 표기
/// </summary>
public class DeviceScanner : MonoBehaviour
{
    public static DeviceScanner Instance { get; private set; }

    public GameObject deviceButtonPrefab; // 버튼 프리팹
    public Transform contentHolder; // 버튼을 마구 생성할 곳

    private Dictionary<string, bool> connectedDevices = new Dictionary<string, bool>();
    private Dictionary<string, Button> deviceButtons = new Dictionary<string, Button>();
    private Queue<string> deviceOrder = new Queue<string>();
    private const int MAX_DEVICES = 5;
    private const float DEVICE_CHECK_INTERVAL = 5f;
    private float lastDeviceCheckTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            if (Instance != this)
                Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(ScanDevicesPeriodically());
    }

    private IEnumerator ScanDevicesPeriodically()
    {
        while (true)
        {
            // ESP32의 일반적인 IP 대역 스캔
            string[] ipRanges = new string[] { "192.168.4.", "192.168.1." };
            foreach (string ipRange in ipRanges)
            {
                for (int i = 1; i <= 254; i++)
                {
                    string ip = ipRange + i;
                    StartCoroutine(CheckDevice(ip));
                    yield return new WaitForSeconds(0.1f); // 너무 빠른 스캔 방지
                }
            }
            yield return new WaitForSeconds(DEVICE_CHECK_INTERVAL);
        }
    }

    private IEnumerator CheckDevice(string ip)
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://{ip}/status");
        www.timeout = 1; // 1초 타임아웃
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string response = www.downloadHandler.text;
                // ESP32의 응답 형식에 따라 파싱
                string deviceId = "ESP32_" + ip.Split('.')[3];
                if (!deviceButtons.ContainsKey(deviceId))
                {
                    AddDeviceToList(deviceId, ip);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"장치 응답 파싱 실패: {e.Message}");
            }
        }
    }

    private void AddDeviceToList(string id, string ip)
    {
        if (deviceButtons.ContainsKey(id))
        {
            return;
        }

        if (deviceButtons.Count >= MAX_DEVICES)
        {
            string oldestDevice = deviceOrder.Dequeue();
            RemoveDevice(oldestDevice);
            Debug.Log($"🗑️ 장치 제거됨 (최대 개수 초과): {oldestDevice}");
        }

        GameObject btn = Instantiate(deviceButtonPrefab, contentHolder);
        Button button = btn.GetComponent<Button>();
        TextMeshProUGUI tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"{id}\nIP: {ip}";
        }

        // 모든 자식 오브젝트 비활성화
        for (int i = 0; i < btn.transform.childCount; i++)
        {
            btn.transform.GetChild(i).gameObject.SetActive(false);
        }

        deviceButtons[id] = button;
        connectedDevices[id] = false;
        deviceOrder.Enqueue(id);

        button.onClick.AddListener(() => StartCoroutine(ConnectToDevice(ip, id)));
        Debug.Log($"➕ 장치 추가됨: {id} (IP: {ip})");
    }

    private void RemoveDevice(string deviceId)
    {
        if (deviceButtons.TryGetValue(deviceId, out Button button))
        {
            Destroy(button.gameObject);
            deviceButtons.Remove(deviceId);
            connectedDevices.Remove(deviceId);
        }
    }

    private IEnumerator ConnectToDevice(string ip, string deviceId)
    {
        if (connectedDevices[deviceId])
        {
            yield return StartCoroutine(DisconnectFromDevice(ip, deviceId));
            yield break;
        }

        Debug.Log($"🔌 {deviceId} 연결 시도 중... (IP: {ip})");
        UnityWebRequest www = UnityWebRequest.Get($"http://{ip}/connect");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ {deviceId} 연결 성공!");
            connectedDevices[deviceId] = true;
            UpdateButtonState(deviceId);

            // 연결 즉시 MEASURE 명령 전송
            yield return StartCoroutine(SendCommand(ip, "MEASURE"));
        }
        else
        {
            Debug.Log($"❌ {deviceId} 연결 실패: {www.error}");
        }
    }

    private IEnumerator DisconnectFromDevice(string ip, string deviceId)
    {
        Debug.Log($"🔌 {deviceId} 연결 해제 시도 중... (IP: {ip})");
        UnityWebRequest www = UnityWebRequest.Get($"http://{ip}/disconnect");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ {deviceId} 연결 해제 성공!");
            connectedDevices[deviceId] = false;
            UpdateButtonState(deviceId);
        }
        else
        {
            Debug.Log($"❌ {deviceId} 연결 해제 실패: {www.error}");
        }
    }

    private IEnumerator SendCommand(string ip, string command)
    {
        if (!connectedDevices.ContainsValue(true))
        {
            Debug.LogError("연결된 장치가 없습니다!");
            yield break;
        }

        Debug.Log($"📤 명령 전송: {command}");
        UnityWebRequest www = UnityWebRequest.Get($"http://{ip}/{command.ToLower()}");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string response = www.downloadHandler.text;
            Debug.Log($"📥 ESP32 응답: {response}");
        }
        else
        {
            Debug.LogError($"❌ 명령 전송 실패: {www.error}");
        }
    }

    private void UpdateButtonState(string deviceId)
    {
        if (deviceButtons.TryGetValue(deviceId, out Button button))
        {
            // 모든 자식 오브젝트 비활성화
            for (int i = 0; i < button.transform.childCount; i++)
            {
                button.transform.GetChild(i).gameObject.SetActive(false);
            }

            // 연결 상태에 따라 두 번째 자식 오브젝트 활성화
            if (connectedDevices[deviceId] && button.transform.childCount > 1)
            {
                button.transform.GetChild(1).gameObject.SetActive(true);
            }
            
            TextMeshProUGUI tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                string status = connectedDevices[deviceId] ? "연결됨" : "연결 안됨";
                tmpText.text = $"{deviceId}\n{status}";
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // contentHolder가 없는 경우 찾기
        if (contentHolder == null)
        {
            contentHolder = GameObject.Find("DeviceListContent")?.transform;
            if (contentHolder == null)
            {
                Debug.LogWarning("⚠️ DeviceListContent를 찾을 수 없습니다.");
                return;
            }
        }

        // 기존 UI 요소 제거
        foreach (var button in deviceButtons.Values)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        deviceButtons.Clear();
        connectedDevices.Clear();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            // 연결된 모든 장치 연결 해제
            foreach (var device in connectedDevices)
            {
                if (device.Value)
                {
                    string ip = device.Key.Split('_')[1];
                    StartCoroutine(DisconnectFromDevice(ip, device.Key));
                }
            }
        }
    }
}

