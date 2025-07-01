using UnityEngine;
using UnityEngine.UI; // UI 관련 코드
public class UIManager : MonoBehaviour
{
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
            }

            return m_instance;
        }
    }
    private static UIManager m_instance;
    public Text GPSText_UI;
    
    // 아이템 관련UI
    public Text itemText; 
    public Text completeText; // 완료시 텍스트 띄우기(다른거로 대체할 예정)
    
    public Text distanceInfoText; // 이동 거리 안내 UI
    public Button startDistanceButton; // 거리 측정 시작 버튼
    public DialogueManager dialogueManager;
    
    private CalculateDistance calculateDistance;
    public bool dialogueFinished = false;

    void Start()
    {
        calculateDistance = FindObjectOfType<CalculateDistance>();
        setUp();
        if (startDistanceButton != null)
        {
            startDistanceButton.onClick.RemoveAllListeners();
            startDistanceButton.onClick.AddListener(OnStartDistanceButtonClicked);
        }
    }

    private void setUp()
    {
        if (distanceInfoText != null)
            distanceInfoText.gameObject.SetActive(false);
        if (completeText != null)
            completeText.gameObject.SetActive(false);
        if (itemText != null)
            itemText.gameObject.SetActive(false);
        // 대화 패널도 항상 숨김!
        if (dialogueManager != null && dialogueManager.dialoguePanel != null)
            dialogueManager.dialoguePanel.SetActive(false);
    }

    private void OnStartDistanceButtonClicked()
    {
        // 1. Start 버튼 비활성화 (중복 클릭 방지)
        startDistanceButton.gameObject.SetActive(false);
        // 2. 대화 시작 (대화 끝나야 이동 가능)
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(OnDialogueFinished);
        }
    }

    // 3. 대화 끝났을 때 콜백 (여기서만 이동 활성화)
    private void OnDialogueFinished()
    {
        dialogueFinished = true;
        // UI 활성화
        if (distanceInfoText != null)
            distanceInfoText.gameObject.SetActive(true);
        if (itemText != null)
            itemText.gameObject.SetActive(true);

        // 실제 이동/거리측정 시작 신호
        if (calculateDistance != null)
        {
            calculateDistance.SetStartPoint();
        }
    }

    public void UpdateUIText()
    {
        if (GPSText_UI != null)
        {
            string lat = GPSManager.Instance.latitude.ToString("F6");
            string lon = GPSManager.Instance.longitude.ToString("F6");
            GPSText_UI.text = $"{lat} / {lon}";
        }
    }
    public void UpdateInfoUI(float movedDistance, float targetDistance)
    {
        if (distanceInfoText == null) return;
        distanceInfoText.text = $"이동 거리: {movedDistance:F1} m / {targetDistance} m";
        if (movedDistance >= targetDistance)
        {
            distanceInfoText.text += targetDistance + "m 이동 성공!";
        }
    }
    public void SetDistanceInfoMessage(string msg)
    {
        if (distanceInfoText != null)
            distanceInfoText.text = msg;
    }
    public void UpdateItemText(int getItemCount, int maxItemCount)
    {
        if (itemText != null)
            itemText.text = $"획득 아이템 수 : {getItemCount}/ {maxItemCount}";
        if (completeText != null)
            completeText.gameObject.SetActive(getItemCount == maxItemCount);
    }
    public void SetCompleteTextVisible(bool visible)
    {
        if (completeText != null)
            completeText.gameObject.SetActive(visible);
    }
}
