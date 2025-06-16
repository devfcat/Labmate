using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class Panel_DataMenu : MonoBehaviour
{
    public GameObject prefab; // 버튼 프리팹
    public Transform contentHolder; // 버튼들을 생성할 위치
    public TextMeshProUGUI noDataText; // 데이터가 없을 때 표시할 텍스트
    public Panel_DataView dataView; // 데이터 뷰 패널 참조

    private List<string> dataFiles = new List<string>();
    private List<GameObject> dataButtons = new List<GameObject>();

    void OnEnable()
    {
        LoadDataFiles();
    }

    void OnDisable()
    {
        ClearDataButtons();
    }

    private void LoadDataFiles()
    {
        // 기존 버튼들 제거
        ClearDataButtons();

        // 데이터 파일 경로
        string dataPath = Application.persistentDataPath;
        
        // exp_data_로 시작하는 모든 JSON 파일 찾기
        dataFiles = Directory.GetFiles(dataPath, "exp_data_*.json")
            .OrderByDescending(f => File.GetLastWriteTime(f))
            .ToList();

        if (dataFiles.Count == 0)
        {
            if (noDataText != null)
            {
                noDataText.gameObject.SetActive(true);
            }
            return;
        }

        if (noDataText != null)
        {
            noDataText.gameObject.SetActive(false);
        }

        // 각 파일에 대해 버튼 생성
        foreach (string filePath in dataFiles)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                ExperimentDataWrapper wrapper = JsonUtility.FromJson<ExperimentDataWrapper>(jsonContent);

                if (wrapper.data != null && wrapper.data.Count > 0)
                {
                    // 첫 번째 데이터에서 실험 타입 가져오기
                    string experimentType = wrapper.data[0].experimentType;
                    string timestamp = wrapper.data[0].timestamp;
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    // 버튼 생성
                    GameObject buttonObj = Instantiate(prefab, contentHolder);
                    dataButtons.Add(buttonObj);

                    // 버튼의 자식 TMP 컴포넌트들 가져오기
                    TextMeshProUGUI[] tmpComponents = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
                    if (tmpComponents.Length >= 2)
                    {
                        // 첫 번째 TMP는 실험 제목
                        tmpComponents[0].text = experimentType;
                        // 두 번째 TMP는 타임스탬프
                        tmpComponents[1].text = timestamp;
                    }

                    // 버튼 클릭 이벤트 설정
                    Button button = buttonObj.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => OnDataButtonClick(filePath, wrapper.data));
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"파일 로드 오류 ({filePath}): {e.Message}");
            }
        }
    }

    private void ClearDataButtons()
    {
        foreach (GameObject button in dataButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        dataButtons.Clear();
    }

    private void OnDataButtonClick(string filePath, List<ExpData> expDataList)
    {
        if (expDataList != null && expDataList.Count > 0)
        {
            // 데이터 뷰 패널로 데이터 전달
            dataView.UpdateDataView(
                expDataList[0].experimentType,
                expDataList[0].timestamp,
                expDataList
            );
            
            // 데이터 뷰 화면으로 전환
            GameManager.instance.SetState(eState.DataMenu_View);
        }
    }
}

[System.Serializable]
public class ExperimentDataWrapper
{
    public List<ExpData> data;
}
