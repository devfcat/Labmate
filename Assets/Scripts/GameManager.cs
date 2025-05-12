using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 상태 목록
/// </summary>
public enum eState
{
    Splash = 0,
    Main_Menu = 1,
    Main_Setting = 2, // 장치 설정
    Main_Experiment = 3, // 실험 목록
    Main_DataMenu = 4, // 실험 데이터 목록
}

/// <summary>
/// 상태 관리를 진행하는 메인 스크립트
/// </summary>
public class GameManager : MonoBehaviour
{
    public eState m_State; // 현재 상태 변수
    public List<GameObject> panels; // 상태에 해당하는 패널들

    public Stack<eState> state_stack;

    public GameObject header_logo;
    public GameObject btn_header;
    public List<GameObject> header_list;

    public static GameManager instance;
    private void Awake()
    {
        if (instance == null) 
        {
            instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            if (instance != this) 
            Destroy(this.gameObject); 
        }
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        state_stack = new Stack<eState>();
        state_stack.Clear();
        SetState(eState.Splash);
    }

    public void Run()
    {
        SetState(eState.Main_Menu);
    }

    public void BackState()
    {
        if (state_stack.Count == 0)
        {
            Debug.Log("스택이 비어있어 더 이상 뒤로 갈 수 없음");
            return;
        }

        eState previous = state_stack.Pop();
        Debug.Log("뒤로가기: " + previous);
        SetState(previous, true);
    }

    public void SetState(eState state, bool isBack = false)
    {
        if (state == eState.Main_Menu)
        {
            state_stack.Clear();
        }
        
        if (!isBack && state != m_State && state != eState.Splash)
        {
            state_stack.Push(m_State); // 현재 상태를 저장
            Debug.Log("상태 저장: " + m_State.ToString());
        }
        m_State = state;

        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].SetActive(false);
        }

        SetHeader();

        switch (state)
        {
            case eState.Splash:
                panels[0].SetActive(true);
                break;
            case eState.Main_Menu:
                panels[1].SetActive(true);
                break;
            case eState.Main_Setting:
                panels[2].SetActive(true);
                break;
            case eState.Main_Experiment:
                panels[3].SetActive(true);
                break;
            case eState.Main_DataMenu:
                panels[4].SetActive(true);
                break;
            default:
                break;
        }
    }

    public void SetHeader()
    {
        header_logo.SetActive(false);
        btn_header.SetActive(false);
        for (int i = 0; i <  header_list.Count; i++)
        {
            header_list[i].SetActive(false);
        }

        switch(m_State)
        {
            case eState.Splash:
                break;
            case eState.Main_Menu:
                header_logo.SetActive(true);
                break;
            case eState.Main_Setting:
                btn_header.SetActive(true);
                header_list[0].SetActive(true);
                break;
            case eState.Main_Experiment:
                btn_header.SetActive(true);
                header_list[1].SetActive(true);
                break;
            case eState.Main_DataMenu:
                btn_header.SetActive(true);
                header_list[2].SetActive(true);
                break;
            default:
                break;
        }
    }
}
