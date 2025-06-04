using System;
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
    [SerializeField] public bool isING; // 실험 중인가

    public exp_data m_exp_data; // 현재 실험 데이터

    public string title;
    public string date;
    public exp_time total_time;

    private float timer = 0f;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        title = ExpManager.Instance.exp_name;
        date = (DateTime.Now).ToString("yyyy년 MM월 dd일 HH시 mm분 ss초");
        total_time = ExpManager.Instance.m_expTime;
    }

    void Update()
    {
        // 실험 중인 경우
        if (isING)
        {
            timer += Time.deltaTime;
            if (timer > Get_period(m_exp_data.period))
            {
                
            }
        }
    }

    public float Get_period(exp_period m_mod)
    {
        switch (m_mod)
        {
            default:
                return 1f;
        }
    }

    /// <summary>
    /// 실험 시작 버튼을 눌렀을 때
    /// </summary>
    public void Start_Exp()
    {
        isING = true;

    }

    /// <summary>
    /// 실험 종료 버튼을 눌렀거나 실험 시간이 종료되어 실험이 끝났을 때
    /// </summary>
    public void End_Exp()
    {
        isING = false;

    }

    /// <summary>
    /// 실험 데이터를 받아와 파일로 stream하는 메서드
    /// </summary>
    public void Write_Data()
    {


    }
}
