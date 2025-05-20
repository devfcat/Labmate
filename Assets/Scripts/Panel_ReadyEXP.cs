using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Panel_ReadyEXP : MonoBehaviour
{
    public TextMeshProUGUI title_tmp;

    public List<GameObject> btns_type;
    public List<GameObject> btns_period;
    public List<GameObject> btns_time;

    void OnEnable()
    {
        title_tmp.text = ExpManager.Instance.exp_name + " 프리셋";
        Init();
    }

    public void Init()
    {
        btns_type[(int)ExpManager.Instance.m_exp].SetActive(true);
        btns_period[(int)ExpManager.Instance.m_period].SetActive(true);
        btns_time[(int)ExpManager.Instance.m_expTime].SetActive(true);
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
    
