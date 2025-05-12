using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Main : MonoBehaviour
{
    public void OnClick_Setting()
    {
        GameManager.instance.SetState(eState.Main_Setting);
    }

    public void OnClick_Experiment()
    {
        GameManager.instance.SetState(eState.Main_Experiment);
    }

    public void OnClick_DataView()
    {
        GameManager.instance.SetState(eState.Main_DataMenu);
    }
}
