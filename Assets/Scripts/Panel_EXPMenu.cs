using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_EXPMenu : MonoBehaviour
{
    public void OnClick_EXP(string name)
    {
        ExpManager.Instance.Make_Exp(name);
        GameManager.instance.SetState(eState.Exp_Setting);
    }
}
