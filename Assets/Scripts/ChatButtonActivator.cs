using UnityEngine;
using UnityEngine.UI;

public class ChatButtonActivator : MonoBehaviour
{
    private Button chatButton;
    private bool isButtonActive = true;

    private void Awake()
    {
        chatButton = GetComponent<Button>();
        if (chatButton == null)
        {
            Debug.LogError("ChatButtonActivator: Button component not found!");
            return;
        }
    }

    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            UpdateButtonState(GameManager.instance.m_State);
        }
    }

    public void OnStateChanged()
    {
        if (GameManager.instance != null)
        {
            UpdateButtonState(GameManager.instance.m_State);
        }
    }

    private void UpdateButtonState(eState newState)
    {
        if (chatButton == null) return;

        bool shouldBeActive = newState != eState.Exp_Setting && newState != eState.Main_DataMenu;
        
        // 현재 상태와 동일하면 변경하지 않음
        if (isButtonActive == shouldBeActive) return;

        isButtonActive = shouldBeActive;
        chatButton.interactable = shouldBeActive;
        
        // 버튼의 시각적 상태 업데이트
        ColorBlock colors = chatButton.colors;
        colors.normalColor = shouldBeActive ? Color.white : Color.gray;
        chatButton.colors = colors;
    }
}
