using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using UnityEngine.UI;

/// <summary>
/// 실험 프리셋 선택 및 실험 중 화면 (실질적인 제어를 조작하는 부분)
/// </summary>
public class Panel_ReadyEXP : MonoBehaviour
{
    public TextMeshProUGUI title_tmp;
    public List<GameObject> btns_type;
    public List<GameObject> btns_period;
    public List<GameObject> btns_time;

    public GameObject panel_exp_ing; // 실험 중 띄워주는 화면 (종료되면 꺼짐)
    public TextMeshProUGUI tmp_currentValue; // 실시간 측정값 표시용

    private float timer = 0f;
    public float experimentTimer = 0f;

    private List<ExpData> expDataList = new List<ExpData>();
    private string savePath;
    private StringBuilder jsonBuilder = new StringBuilder();
    private bool isFirstData = true;

    // 아두이노 통신 관련 변수
    private string connectedDeviceId = null;
    private string connectedDeviceIp = null;

    private float GetExperimentDuration()
    {
        switch (ExpManager.Instance.m_expTime)
        {
            case exp_time.fifteen:
                return 15f;
            case exp_time.thirty:
                return 30f;
            case exp_time.fourty_five:
                return 45f;
            case exp_time.one_min:
                return 60f;
            case exp_time.two_min:
                return 120f;
            case exp_time.three_min:
                return 180f;
            default:
                return 60f;
        }
    }

    private string GetExperimentTypeString()
    {
        switch (ExpManager.Instance.m_exp)
        {
            case exp_type.current:
                return "전류 측정 실험";
            case exp_type.voltage:
                return "전압 측정 실험";
            case exp_type.resist:
                return "저항 측정 실험";
            default:
                return "알 수 없음";
        }
    }

    void OnEnable()
    {
        title_tmp.text = ExpManager.Instance.exp_name + " 프리셋";
        Init();
        expDataList.Clear();
        savePath = Path.Combine(Application.persistentDataPath, $"exp_data_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        
        // JSON 파일 초기화
        try
        {
            jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\"data\":[");
            File.WriteAllText(savePath, jsonBuilder.ToString() + "]}");
            isFirstData = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 초기화 오류: {e.Message}");
        }
    }

    void OnDisable()
    {
        SaveAllData();
    }

    private void SaveAllData()
    {
        try
        {
            if (jsonBuilder != null)
            {
                jsonBuilder.Append("]}");
                File.WriteAllText(savePath, jsonBuilder.ToString());
                jsonBuilder = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"최종 데이터 저장 오류: {e.Message}");
        }
    }

    public void Init()
    {
        btns_type[(int)ExpManager.Instance.m_exp].SetActive(true);
        btns_period[(int)ExpManager.Instance.m_period].SetActive(true);
        btns_time[(int)ExpManager.Instance.m_expTime].SetActive(true);
    }

    // 실험 시작 버튼 클릭 시 실험 중 화면 활성화 및 실험 중 여부 변경
    public void OnExperiment()
    {
        ExpManager.Instance.Control_Exp();
        panel_exp_ing.SetActive(ExpManager.Instance.isIng);
        
        if (ExpManager.Instance.isIng)
        {
            experimentTimer = 0f;
        }
    }

    public void Update()
    {
        // 실험 중일 때, 1초마다 데이터를 받아옴
        if (ExpManager.Instance.isIng)
        {
            experimentTimer += Time.deltaTime;
            float duration = GetExperimentDuration();
            
            // 실험 시간이 종료되면 자동으로 실험 중지
            if (experimentTimer >= duration)
            {
                ExpManager.Instance.Control_Exp();
                panel_exp_ing.SetActive(false);
                SaveAllData();
                return;
            }

            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer = 0f;

                /* 아두이노로부터 데이터를 받아오는 코드 (현재는 주석 처리)
                // 연결된 장치가 있는지 확인
                if (DeviceScanner.Instance != null)
                {
                    // 연결된 장치 찾기
                    foreach (var device in DeviceScanner.Instance.connectedDevices)
                    {
                        if (device.Value) // 연결된 장치가 있다면
                        {
                            connectedDeviceId = device.Key;
                            connectedDeviceIp = device.Key.Split('_')[1]; // IP 주소 추출
                            break;
                        }
                    }

                    if (connectedDeviceId != null)
                    {
                        // MEASURE 명령 전송
                        StartCoroutine(DeviceScanner.Instance.SendCommand(connectedDeviceIp, "MEASURE\n"));
                        
                        // 응답 대기 (실제로는 비동기 처리가 필요할 수 있음)
                        yield return new WaitForSeconds(0.1f);
                        
                        // 응답 데이터 파싱 (예시: "V:3.25,I:32.5" 형식)
                        string response = DeviceScanner.Instance.LastReceivedData;
                        string[] dataParts = response.Split(',');
                        float voltage = 0f;
                        float current = 0f;

                        foreach (string part in dataParts)
                        {
                            string[] keyValue = part.Split(':');
                            if (keyValue.Length == 2)
                            {
                                switch (keyValue[0])
                                {
                                    case "V":
                                        float.TryParse(keyValue[1], out voltage);
                                        break;
                                    case "I":
                                        float.TryParse(keyValue[1], out current);
                                        break;
                                }
                            }
                        }

                        // 전력과 저항 계산
                        float power = voltage * current;
                        float resistance = voltage / (current / 1000f);
                        float error = 0f; // 오차 계산 로직 추가 필요

                        ExpData data = new ExpData(voltage, current, power, resistance, error, GetExperimentTypeString());
                        expDataList.Add(data);
                    }
                    else
                    {
                        Debug.LogWarning("연결된 장치가 없습니다.");
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning("DeviceScanner가 초기화되지 않았습니다.");
                    return;
                }
                */

                // 임시로 더미 데이터를 생성합니다
                float voltage = UnityEngine.Random.Range(3.2f, 3.3f);
                float current = UnityEngine.Random.Range(30f, 35f);
                float power = voltage * current;
                float resistance = voltage / (current / 1000f);
                float error = UnityEngine.Random.Range(-2f, 2f);

                ExpData data = new ExpData(voltage, current, power, resistance, error, GetExperimentTypeString());
                expDataList.Add(data);

                // 실험 종류에 따라 표시할 값 선택
                string valueText = "";
                switch (GetExperimentTypeString())
                {
                    case "전류 측정 실험":
                        valueText = $"현재 측정된 전류: {current:F3} mA";
                        break;
                    case "전압 측정 실험":
                        valueText = $"현재 측정된 전압: {voltage:F3} V";
                        break;
                    case "저항 측정 실험":
                        valueText = $"현재 측정된 저항: {resistance:F3} Ω";
                        break;
                    default:
                        valueText = "현재 측정값: -";
                        break;
                }
                if (tmp_currentValue != null)
                    tmp_currentValue.text = valueText;

                // JSON 데이터 추가
                try
                {
                    string jsonData = JsonUtility.ToJson(data);
                    if (!isFirstData)
                    {
                        jsonBuilder.Append(",");
                    }
                    jsonBuilder.Append(jsonData);
                    isFirstData = false;

                    // 메모리에 있는 데이터를 파일에 저장
                    string currentContent = jsonBuilder.ToString() + "]}";
                    File.WriteAllText(savePath, currentContent);

                    Debug.Log($"데이터 저장됨: {jsonData} (남은 시간: {duration - experimentTimer:F1}초)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"데이터 저장 오류: {e.Message}");
                }
            }
        }
    }

    public void OnClick_btn_type(int selected)
    {
        ExpManager.Instance.Set_Exp((exp_type)selected);
        for (int i = 0; i < btns_type.Count; i++)
        {
            if (i == selected)
            {
                btns_type[i].gameObject.SetActive(true);
            }
            else { btns_type[i].gameObject.SetActive(false); }
        }
    }

    public void OnClick_btn_period(int selected)
    {
        ExpManager.Instance.m_period = (exp_period)selected;
        for (int i = 0; i < btns_period.Count; i++)
        {
            if (i == selected)
            {
                btns_period[i].gameObject.SetActive(true);
            }
            else { btns_period[i].gameObject.SetActive(false); }
        }
    }

    public void OnClick_btn_time(int selected)
    {
        ExpManager.Instance.m_expTime = (exp_time)selected;
        for (int i = 0; i < btns_time.Count; i++)
        {
            if (i == selected)
            {
                btns_time[i].gameObject.SetActive(true);
            }
            else { btns_time[i].gameObject.SetActive(false); }
        }
    }
}
    
