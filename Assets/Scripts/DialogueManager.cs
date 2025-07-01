using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Button nextButton;

    [TextArea(2, 5)]
    public string[] lines; // 대사 배열 (에디터에서 입력)

    private int currentLine = 0;
    public Action onDialogueEnd; // 대화 종료 콜백
    
    void Start()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false); // 시작할 때 항상 숨김
        }
    }

    public void StartDialogue(Action onDialogueEndCallback = null)
    {
        if (lines == null || lines.Length == 0) return;
        currentLine = 0;
        dialoguePanel.SetActive(true);
        ShowLine();
        onDialogueEnd = onDialogueEndCallback;
    }

    private void ShowLine()
    {
        dialogueText.text = lines[currentLine];
    }

    private void OnNextClicked()
    {
        currentLine++;
        if (currentLine < lines.Length)
        {
            ShowLine();
        }
        else
        {
            dialoguePanel.SetActive(false);
            onDialogueEnd?.Invoke();
        }
    }
}