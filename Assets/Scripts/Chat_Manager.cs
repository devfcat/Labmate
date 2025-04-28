using System;
using System.Collections.Generic;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class Chat_Manager : MonoBehaviour
{
    [SerializeField]
    public string API_KEY; // 인스펙터에서 api_key 입력

    [Header("메세지 프리팹")]
    public GameObject textBox;

    [Header("내 메세지 프리팹")]
    public GameObject textBox_mine;

    [Header("메세지 화면")]
    public GameObject panel_textview;

    public TMP_InputField inputField;
    public Button btn_Send;

    private OpenAIAPI api;

    [Header("메세지 기록")]
    public List<string> message_record;
    private List<ChatMessage> messages;

    [Header("사전 구성")]
    private string prompts = "";
    
    private static Chat_Manager _instance;
    public static Chat_Manager Instance
    {
        get {
            if(!_instance)
            {
                _instance = FindObjectOfType(typeof(Chat_Manager)) as Chat_Manager;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            return _instance;
        }
    }

    void Start()
    {
        api = new OpenAIAPI(API_KEY);
        StartConversation();
        btn_Send.onClick.AddListener(() => GetResponse());
    }

    // 초기화 및 구동 설정
    private void StartConversation()
    {
        messages = new List<ChatMessage> { new ChatMessage(ChatMessageRole.System, prompts) };

        inputField.text = "";
        message_record.Clear();

        // 시작 인삿말을 불러옴
        string startString =
            "안녕하세요. 실험에 관해 궁금한 점이 있다면 질문해보세요!";
        Make_Msg(startString);
    }

    // 메세지박스를 뷰에 만들어주는 메서드
    public void Make_Msg(string msg, bool isPlayer = false)
    {
        GameObject new_textbox;
        if (!isPlayer)
        {
            new_textbox = Instantiate(textBox, panel_textview.transform);
        }
        else
        {
            new_textbox = Instantiate(textBox_mine, panel_textview.transform);
        }

        new_textbox.GetComponent<Fitting_Box>().SetText(msg);

        message_record.Add(msg);
    }

    // 엔터키를 눌러도 메세지가 전송되도록 함
    public void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            GetResponse();
        }
#endif
    }

    // 입력을 전송하고 대답을 받는 비동기 메서드
    private async void GetResponse()
    {
        if (inputField.text.Length < 10)
        {
            return;
        }
        // 버튼 Disable
        btn_Send.enabled = false;

        // 입력한 메세지를 가져옴
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text;
        //list에 메세지 추가
        messages.Add(userMessage);

        // 유저 메세지 전송
        Make_Msg(userMessage.Content, true);

        //inputField 초기화
        inputField.text = "";

        // 전체 채팅을 openAI 서버에전송하여 다음 메시지(응답)를 가져오도록
        var chatResult = await api.Chat.CreateChatCompletionAsync(
            new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                MaxTokens = 100,
                Messages = messages,
            }
        );

        //응답 가져오기
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = ChatMessageRole.Assistant;
        //responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;

        //응답을 message리스트에 추가
        messages.Add(responseMessage);

        string msg = (responseMessage.Content).Replace(Environment.NewLine, ""); // 개행문자를 없앰

        // 대답을 가져와서 적용
        Make_Msg(msg);

        // 전송버튼 활성화
        btn_Send.enabled = true;
    }
}
