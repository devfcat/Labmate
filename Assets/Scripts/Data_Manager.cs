using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 실험 데이터 클래스
public class exp_data
{
    public string title; // 실험 데이터차트 제목
    public string date; // 실험 날짜
    public exp_period period; // 실험 측정 주기
    public exp_time total_time; // 실험 총 시간 (도달하면 자동 종료)
    public float m_time; // 실제 측정한 시간
    public List<float> datas; // 측정값 리스트
    public float average; // 측정값 평균
    public float mid_value; // 측정값 중앙값
}

public class Data_Manager : MonoBehaviour
{
    public void Init()
    {
        title = ExpManager.Instance.exp_name;
        date = (DateTime.Now).ToString("yyyy년 MM월 dd일 HH시 mm분 ss초");
        total_time = ExpManager.Instance.m_expTime;
    }

    public void Start_Exp()
    {

    }

    public void End_Exp()
    {


    }

    public void Write_Data()
    {
        
    }
}
