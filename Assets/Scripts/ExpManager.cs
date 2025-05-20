using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum exp_type
{
    current = 0,
    voltage = 1,
    resist = 2
}

public enum exp_period
{
    half_second = 0,
    one = 1,
    two = 2,
    five = 3
}

public enum exp_time
{
    fifteen = 0,
    thirty = 1,
    fourty_five = 2,
    one_min = 3,
    two_min = 4,
    three_min = 5
}

public class ExpManager : MonoBehaviour
{
    public exp_type m_exp;
    public exp_period m_period;
    public exp_time m_expTime;

    public string exp_name;
    public TextMeshProUGUI m_exp_tmp;

    private static ExpManager _instance;
    public static ExpManager Instance
    {
        get {
            if(!_instance)
            {
                _instance = FindObjectOfType(typeof(ExpManager)) as ExpManager;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            return _instance;
        }
    }

    public void Make_Exp(string name)
    {
        exp_name = name;
        m_exp_tmp.text = exp_name;

        m_expTime = exp_time.one_min;
        m_period = exp_period.one;

        switch (exp_name)
        {
            case "전류 측정 실험":
                Set_Exp(exp_type.current);
                break;
            case "전압 측정 실험":
                Set_Exp(exp_type.voltage);
                break;
            case "저항 측정 실험":
                Set_Exp(exp_type.resist);
                break;
            case "옴의 법칙 실험":
                Set_Exp(exp_type.resist);
                break;
            case "키르히호프의 법칙 실험":
                Set_Exp(exp_type.resist);
                break;
            default:
                break;
        }
    }

    // 실험 설정 프리셋을 불러온다
    public void Set_Exp(exp_type exp)
    {
        m_exp = exp;

        switch (m_exp)
        {
            case exp_type.current:
                break;
            case exp_type.voltage:
                break;
            case exp_type.resist:
                break;
            default: break;
        }
    }
}
