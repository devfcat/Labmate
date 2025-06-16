using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using XCharts.Runtime;

public class Panel_DataView : MonoBehaviour
{
    public TextMeshProUGUI tmp_title;
    public TextMeshProUGUI tmp_timeStamp;
    public TextMeshProUGUI tmp_average; // 평균값을 표시할 TextMeshProUGUI
    
    [Header("Table Settings")]
    public Transform tableContent; // 표의 내용이 들어갈 부모 오브젝트
    public GameObject rowPrefab; // 행 프리팹
    
    [Header("Graph Settings")]
    public LineChart lineChart; // 라인 차트 컴포넌트
    
    private List<GameObject> tableRows = new List<GameObject>();
    
    void Start()
    {
        if (lineChart != null)
        {
            InitializeChart();
        }
    }
    
    private void InitializeChart()
    {
        // 차트 기본 설정
        var title = lineChart.GetOrAddChartComponent<Title>();
        title.text = "";
        title.subText = "";
        
        // X축 설정
        lineChart.GetOrAddChartComponent<XAxis>().type = XCharts.Runtime.Axis.AxisType.Category;
        lineChart.GetOrAddChartComponent<XAxis>().axisName.name = "측정 순서";
        
        // Y축 설정
        lineChart.GetOrAddChartComponent<YAxis>().type = XCharts.Runtime.Axis.AxisType.Value;
        lineChart.GetOrAddChartComponent<YAxis>().axisName.name = "측정값";
        
        // 그리드 설정
        var grid = lineChart.GetOrAddChartComponent<GridCoord>();
        grid.left = 60;
        grid.right = 20;
        grid.top = 50;
        grid.bottom = 30;

    }
    
    public void UpdateDataView(string title, string timeStamp, List<ExpData> expDataList)
    {
        // 제목과 타임스탬프 업데이트
        if (tmp_title != null) tmp_title.text = title;
        if (tmp_timeStamp != null) tmp_timeStamp.text = timeStamp;
        
        // 기존 표 내용 제거
        ClearTable();
        
        // 실험 유형에 따라 적절한 데이터 표시
        List<float> values = new List<float>();
        string unit = "";
        
        foreach (ExpData data in expDataList)
        {
            float value = 0f;
            
            // 실험 유형에 따라 표시할 데이터 선택
            switch (data.experimentType)
            {
                case "전류 측정 실험":
                    value = data.current;
                    unit = "mA";
                    break;
                case "전압 측정 실험":
                    value = data.voltage;
                    unit = "V";
                    break;
                case "저항 측정 실험":
                    value = data.resistance;
                    unit = "Ω";
                    break;
                default:
                    continue;
            }
            
            values.Add(value);
            CreateTableRow(expDataList.IndexOf(data), value, unit);
        }
        
        // 평균값 계산 및 표시
        if (tmp_average != null)
        {
            if (values.Count > 0)
            {
                float sum = 0f;
                foreach (float value in values)
                {
                    sum += value;
                }
                float average = sum / values.Count;
                tmp_average.text = $"평균값: {average:F3} {unit}";
            }
            else
            {
                tmp_average.text = "평균값: -";
            }
        }
        
        // 그래프 업데이트
        UpdateChart(values, unit);
    }
    
    private void UpdateChart(List<float> values, string unit)
    {
        if (lineChart == null) return;
        
        // 기존 데이터 시리즈 제거
        lineChart.ClearData();
        
        // 새로운 데이터 시리즈 추가
        var serie = lineChart.AddSerie<Line>();
        serie.serieName = tmp_title != null ? tmp_title.text : "데이터";
        serie.symbol.type = XCharts.Runtime.SymbolType.Circle;
        serie.symbol.size = 5;
        serie.lineStyle.width = 2;
        
        // 데이터 포인트 추가
        for (int i = 0; i < values.Count; i++)
        {
            serie.AddData(i, values[i]);
        }
        
        // Y축 단위 업데이트
        lineChart.GetOrAddChartComponent<YAxis>().axisName.name = $"측정값 ({unit})";
        
        // 차트 갱신
        lineChart.RefreshChart();
    }
    
    private void CreateTableRow(int index, float value, string unit)
    {
        if (tableContent == null || rowPrefab == null) return;
        
        GameObject row = Instantiate(rowPrefab, tableContent);
        tableRows.Add(row);
        
        // 이미지 오브젝트들의 자식 TMP 컴포넌트들 가져오기
        Transform[] imageChildren = row.GetComponentsInChildren<Transform>();
        TextMeshProUGUI[] texts = new TextMeshProUGUI[2];
        int textIndex = 0;
        
        foreach (Transform child in imageChildren)
        {
            if (child.GetComponent<TextMeshProUGUI>() != null)
            {
                texts[textIndex] = child.GetComponent<TextMeshProUGUI>();
                textIndex++;
                if (textIndex >= 2) break;
            }
        }
        
        if (texts[0] != null && texts[1] != null)
        {
            texts[0].text = (index + 1).ToString();
            texts[1].text = $"{value:F3} {unit}";
        }
    }
    
    private void ClearTable()
    {
        foreach (GameObject row in tableRows)
        {
            if (row != null)
            {
                Destroy(row);
            }
        }
        tableRows.Clear();
    }
}
