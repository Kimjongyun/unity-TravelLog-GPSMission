using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MissionManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startMissionButton;
    public TextMeshProUGUI statusText;

    // Reach ProgressBar (그대로 사용)
    public Michsky.UI.Reach.ProgressBar progressBar;

    public TextMeshProUGUI remainingDistanceText;

    [Header("Mission Coordinates")]
    public Vector2 startLocationCoords = new Vector2(37.477597f, 126.861248f);
    public Vector2 destinationCoords   = new Vector2(37.477521f, 126.862443f);

    [Header("Mission Settings")]
    [Tooltip("도착 및 시작 반경 (미터)")]
    public float arrivalRadiusMeters = 10f;

    // ───── 추가: 토스트 & 도시락 이미지 ─────
    [Header("Popup / Meal UI")]
    public Image mealImage;                 // 도시락 이미지 (UI Image)
    public TextMeshProUGUI popupText;      // 토스트 문구
    public CanvasGroup popupGroup;         // 페이드용
    public float popupDuration = 1.5f;     // 표시 유지 시간
    public float popupFade = 0.25f;        // 페이드 인/아웃 시간

    private enum MissionState { InitializingGPS, Idle, GoToStartLocation, DeliveryInProgress, MissionCompleted }
    private MissionState currentMissionState = MissionState.InitializingGPS;

    private float initialDeliveryDistance;
    private bool hasMissionStarted = false;

    private const float EARTH_RADIUS_METERS = 6371000f;
    private Coroutine popupCo;

    void Awake()
    {
        if (startMissionButton != null)
            startMissionButton.onClick.AddListener(OnStartMissionButtonClick);

        // 초기 UI 안전값
        if (mealImage != null) mealImage.gameObject.SetActive(false);
        if (popupText != null) popupText.gameObject.SetActive(false);
        if (popupGroup != null) popupGroup.alpha = 0f;
    }

    void Start()
    {
        UpdateUIForCurrentState();
        StartCoroutine(CheckGpsAndStartGame());
    }

    IEnumerator CheckGpsAndStartGame()
    {
        while (GPSManager.Instance == null)
        {
            if (statusText != null) statusText.text = "GPS 시스템 초기화 중...";
            yield return null;
        }

        while (GPSManager.Instance.GetLocationStatus() != LocationServiceStatus.Running)
        {
            var gpsStatus = GPSManager.Instance.GetLocationStatus();
            if (gpsStatus == LocationServiceStatus.Failed)
            {
                if (statusText != null) statusText.text = "위치 정보 초기화에 실패했습니다. 위치 권한을 확인하고 앱을 재시작해 주세요.";
                if (startMissionButton != null) startMissionButton.interactable = false;
                yield break;
            }
            if (statusText != null) statusText.text = $"위치 정보를 가져오는 중입니다... ({gpsStatus})";
            yield return null;
        }

        SetMissionState(MissionState.Idle);
    }

    void Update()
    {
        if (currentMissionState == MissionState.InitializingGPS ||
            GPSManager.Instance == null ||
            GPSManager.Instance.GetLocationStatus() != LocationServiceStatus.Running)
            return;

        float lat = GPSManager.Instance.latitude;
        float lon = GPSManager.Instance.longitude;

        switch (currentMissionState)
        {
            case MissionState.GoToStartLocation:
                HandleGoToStartLocation(lat, lon);
                break;

            case MissionState.DeliveryInProgress:
                HandleDeliveryInProgress(lat, lon);
                break;
        }
    }

    private void HandleGoToStartLocation(float currentLat, float currentLon)
    {
        float distanceFromStart = CalculateDistance(currentLat, currentLon, startLocationCoords.x, startLocationCoords.y);

        if (distanceFromStart <= arrivalRadiusMeters)
        {
            // 시작 지점 도달 → 도시락 이미지 ON + 토스트
            if (mealImage != null) mealImage.gameObject.SetActive(true);
            ShowPopup("미션 시작 지점에 도착했습니다!");

            SetMissionState(MissionState.DeliveryInProgress);
        }
        else
        {
            if (statusText != null)
                statusText.text = $"배달 시작 지점(편의점)으로 이동하세요.\n남은 거리: {distanceFromStart:F2}m";
        }
    }

    private void HandleDeliveryInProgress(float currentLat, float currentLon)
    {
        float remaining = CalculateDistance(currentLat, currentLon, destinationCoords.x, destinationCoords.y);

        if (remaining <= arrivalRadiusMeters)
        {
            // 목적지 도달 → 완료 토스트
            ShowPopup("엽전 도시락 배달이 완료되었습니다!");
            SetMissionState(MissionState.MissionCompleted);
        }
        else
        {
            float progress01 = 0f;
            if (initialDeliveryDistance > 0)
                progress01 = Mathf.Clamp01((initialDeliveryDistance - remaining) / initialDeliveryDistance);
            if (remaining > initialDeliveryDistance) progress01 = 0f;

            float percent100 = progress01 * 100f;

            if (progressBar != null)
            {
                progressBar.currentValue = percent100;
                progressBar.UpdateUI();
            }

            if (remainingDistanceText != null)
                remainingDistanceText.text = $"남은 거리: {remaining / 1000f:F2}km";

            if (statusText != null)
                statusText.text = $"배달 진행 중: {(progress01 * 100f):F0}%";
        }
    }

    private void UpdateUIForCurrentState()
    {
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (remainingDistanceText != null) remainingDistanceText.gameObject.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(true);

        switch (currentMissionState)
        {
            case MissionState.InitializingGPS:
                if (startMissionButton != null) startMissionButton.interactable = false;
                break;

            case MissionState.Idle:
                if (statusText != null)
                    statusText.text = "GPS 준비 완료! 미션을 시작하려면 '배달 미션 시작' 버튼을 누르세요.";
                if (startMissionButton != null)
                {
                    startMissionButton.gameObject.SetActive(true);
                    startMissionButton.interactable = true;
                }
                // 초기화
                if (mealImage != null) mealImage.gameObject.SetActive(false);
                HidePopupImmediate();
                break;

            case MissionState.GoToStartLocation:
                if (startMissionButton != null) startMissionButton.gameObject.SetActive(false);
                break;

            case MissionState.DeliveryInProgress:
                if (statusText != null)
                    statusText.text = $"배달이 시작됩니다. 거리는 {initialDeliveryDistance / 1000f:F2}km입니다.";
                if (progressBar != null)
                {
                    progressBar.gameObject.SetActive(true);
                    progressBar.currentValue = 0f;
                    progressBar.UpdateUI();
                }
                if (remainingDistanceText != null) remainingDistanceText.gameObject.SetActive(true);
                // mealImage는 켠 상태 유지
                break;

            case MissionState.MissionCompleted:
                if (statusText != null) statusText.text = "목적지에 도착했습니다!";
                // mealImage는 요구사항대로 유지 (원하면 여기서 끄면 됨)
                break;
        }
    }

    public void OnStartMissionButtonClick()
    {
        if (currentMissionState != MissionState.Idle)
        {
            Debug.LogWarning("미션은 Idle 상태에서만 시작할 수 있습니다.");
            return;
        }

        SetMissionState(MissionState.GoToStartLocation);
        hasMissionStarted = true;
    }

    private void SetMissionState(MissionState newState)
    {
        currentMissionState = newState;

        if (newState == MissionState.DeliveryInProgress)
        {
            initialDeliveryDistance = CalculateDistance(
                startLocationCoords.x, startLocationCoords.y,
                destinationCoords.x,   destinationCoords.y);

            if (initialDeliveryDistance < 1f)
            {
                SetMissionState(MissionState.MissionCompleted);
                return;
            }
        }

        UpdateUIForCurrentState();
    }

    // ───── Popup helpers ─────
    void ShowPopup(string msg)
    {
        if (popupText == null) return;

        if (popupCo != null) StopCoroutine(popupCo);
        popupCo = StartCoroutine(CoShowPopup(msg));
    }

    IEnumerator CoShowPopup(string msg)
    {
        popupText.text = msg;
        popupText.gameObject.SetActive(true);

        // fade in
        if (popupGroup != null)
        {
            float t = 0f;
            while (t < popupFade)
            {
                t += Time.unscaledDeltaTime;
                popupGroup.alpha = Mathf.Clamp01(t / popupFade);
                yield return null;
            }
            popupGroup.alpha = 1f;
        }

        // hold
        yield return new WaitForSecondsRealtime(popupDuration);

        // fade out
        if (popupGroup != null)
        {
            float t = 0f;
            while (t < popupFade)
            {
                t += Time.unscaledDeltaTime;
                popupGroup.alpha = 1f - Mathf.Clamp01(t / popupFade);
                yield return null;
            }
            popupGroup.alpha = 0f;
        }

        popupText.gameObject.SetActive(false);
    }

    void HidePopupImmediate()
    {
        if (popupText != null) popupText.gameObject.SetActive(false);
        if (popupGroup != null) popupGroup.alpha = 0f;
        if (popupCo != null) { StopCoroutine(popupCo); popupCo = null; }
    }

    // ───── 거리 계산 ─────
    private float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float dLat = ToRadians(lat2 - lat1);
        float dLon = ToRadians(lon2 - lon1);

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(ToRadians(lat1)) * Mathf.Cos(ToRadians(lat2)) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return EARTH_RADIUS_METERS * c;
    }

    private float ToRadians(float deg) => deg * Mathf.PI / 180f;
}
