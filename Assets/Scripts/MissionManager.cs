using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MissionManager : MonoBehaviour
{
    // --- UI 요소 연결 ---
    [Header("UI References")]
    public Button startMissionButton;
    public TextMeshProUGUI statusText;
    public Slider progressBarSlider;
    public TextMeshProUGUI remainingDistanceText;

    // --- GPS 좌표 설정 ---
    [Header("Mission Coordinates")]
    [Tooltip("배달 시작 지점 (편의점)")]
    // public Vector2 startLocationCoords = new Vector2(37.481185f, 126.960088f);
    public Vector2 startLocationCoords = new Vector2(37.477597f, 126.861248f);

    [Tooltip("최종 목적지 (어떤 장소)")]
    // public Vector2 destinationCoords = new Vector2(37.480628f, 126.959449f);
    public Vector2 destinationCoords = new Vector2(37.477521f, 126.862443f);

    [Header("Mission Settings")]
    [Tooltip("도착 및 시작 반경 (미터)")]
    public float arrivalRadiusMeters = 10f;

    // --- 내부 상태 관리 ---
    private enum MissionState
    {
        InitializingGPS,       // GPS 초기화 중 (새로운 상태 추가)
        Idle,                  // 미션 시작 전
        GoToStartLocation,     // 배달 시작 지점으로 이동 중
        DeliveryInProgress,    // 배달 진행 중
        MissionCompleted       // 미션 완료
    }

    private MissionState currentMissionState = MissionState.InitializingGPS; // 초기 상태를 GPS 초기화 중으로 변경
    private float initialDeliveryDistance; // 배달 시작 지점부터 최종 목적지까지의 초기 거리 (미터)
    private bool hasMissionStarted = false; // 미션 시작 버튼 클릭 여부 (한 번만 시작되도록)

    // 지구 반지름 (미터) - Haversine 공식에 사용
    private const float EARTH_RADIUS_METERS = 6371000f;

    void Awake()
    {
        // 버튼 클릭 이벤트 리스너 추가
        if (startMissionButton != null)
        {
            startMissionButton.onClick.AddListener(OnStartMissionButtonClick);
        }
    }

    void Start()
    {
        // 초기 UI 상태 설정 (GPS 초기화 중 메시지 표시)
        UpdateUIForCurrentState();
        StartCoroutine(CheckGpsAndStartGame()); // GPS 상태를 확인하고 게임 시작 준비
    }

    // GPS가 준비될 때까지 기다리는 코루틴
    IEnumerator CheckGpsAndStartGame()
    {
        // GPSManager 인스턴스가 초기화될 때까지 기다림
        while (GPSManager.Instance == null)
        {
            statusText.text = "GPS 시스템 초기화 중...";
            yield return null;
        }

        // GPS 상태가 Running이 될 때까지 기다림
        while (GPSManager.Instance.GetLocationStatus() != LocationServiceStatus.Running)
        {
            LocationServiceStatus gpsStatus = GPSManager.Instance.GetLocationStatus();
            if (gpsStatus == LocationServiceStatus.Failed)
            {
                statusText.text = "위치 정보 초기화에 실패했습니다. 위치 권한을 확인하고 앱을 재시작해 주세요.";
                startMissionButton.interactable = false; // GPS 실패 시 버튼 비활성화
                yield break; // 실패했으므로 코루틴 종료
            }
            statusText.text = $"위치 정보를 가져오는 중입니다... ({gpsStatus})";
            yield return null;
        }

        // GPS가 정상적으로 작동할 때 Idle 상태로 전환
        SetMissionState(MissionState.Idle); // GPS 준비 완료 후 Idle 상태로 전환
    }

    void Update()
    {
        // GPS 초기화 중이거나 GPS가 실행 중이 아니면 아무것도 하지 않음
        if (currentMissionState == MissionState.InitializingGPS || 
            GPSManager.Instance == null || 
            GPSManager.Instance.GetLocationStatus() != LocationServiceStatus.Running)
        {
            return;
        }

        float currentLat = GPSManager.Instance.latitude;
        float currentLon = GPSManager.Instance.longitude;

        switch (currentMissionState)
        {
            case MissionState.Idle:
                // 버튼 클릭 대기 중
                break;

            case MissionState.GoToStartLocation:
                HandleGoToStartLocation(currentLat, currentLon);
                break;

            case MissionState.DeliveryInProgress:
                HandleDeliveryInProgress(currentLat, currentLon);
                break;

            case MissionState.MissionCompleted:
                // 미션 완료 상태, 더 이상 업데이트 없음
                break;
        }
    }

    // --- 상태별 핸들러 ---

    private void HandleGoToStartLocation(float currentLat, float currentLon)
    {
        float distanceFromStart = CalculateDistance(currentLat, currentLon, startLocationCoords.x, startLocationCoords.y);

        if (distanceFromStart <= arrivalRadiusMeters)
        {
            SetMissionState(MissionState.DeliveryInProgress);
        }
        else
        {
            statusText.text = $"배달 시작 지점(편의점)으로 이동하세요.\n남은 거리: {distanceFromStart:F2}m";
        }
    }

    private void HandleDeliveryInProgress(float currentLat, float currentLon)
    {
        float remainingDistance = CalculateDistance(currentLat, currentLon, destinationCoords.x, destinationCoords.y);

        if (remainingDistance <= arrivalRadiusMeters)
        {
            SetMissionState(MissionState.MissionCompleted);
        }
        else
        {
            float progressPercentage = 0f;
            if (initialDeliveryDistance > 0)
            {
                progressPercentage = (initialDeliveryDistance - remainingDistance) / initialDeliveryDistance;
            }
            
            if (remainingDistance > initialDeliveryDistance)
            {
                progressPercentage = 0f;
            }
            
            progressBarSlider.value = Mathf.Clamp01(progressPercentage);
            remainingDistanceText.text = $"남은 거리: {remainingDistance / 1000f:F2}km";

            statusText.text = $"배달 진행 중: {(progressBarSlider.value * 100):F0}%";
        }
    }

    // --- UI 업데이트 함수 ---
    private void UpdateUIForCurrentState()
    {
        // 모든 UI 요소를 기본적으로 숨기거나 비활성화
        startMissionButton.gameObject.SetActive(false);
        progressBarSlider.gameObject.SetActive(false);
        remainingDistanceText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(true); // 상태 텍스트는 항상 활성

        switch (currentMissionState)
        {
            case MissionState.InitializingGPS:
                // 텍스트는 CheckGpsAndStartGame 코루틴에서 설정
                startMissionButton.interactable = false; // GPS 초기화 중에는 버튼 비활성화
                break;

            case MissionState.Idle:
                startMissionButton.gameObject.SetActive(true);
                startMissionButton.interactable = true; // 버튼 활성화
                statusText.text = "GPS 준비 완료! 미션을 시작하려면 '배달 미션 시작' 버튼을 누르세요.";
                break;

            case MissionState.GoToStartLocation:
                // 텍스트는 HandleGoToStartLocation에서 실시간 업데이트
                startMissionButton.gameObject.SetActive(false);
                break;

            case MissionState.DeliveryInProgress:
                statusText.text = $"배달이 시작됩니다. 거리는 {initialDeliveryDistance / 1000f:F2}km입니다.";
                progressBarSlider.gameObject.SetActive(true);
                remainingDistanceText.gameObject.SetActive(true);
                progressBarSlider.value = 0; // 시작 시 0%로 초기화
                break;

            case MissionState.MissionCompleted:
                statusText.text = "목적지에 도착했습니다!";
                // progressBarSlider와 remainingDistanceText는 이미 비활성화
                break;
        }
    }

    // --- 버튼 클릭 이벤트 ---
    public void OnStartMissionButtonClick() // <<<< public으로 되어 있는지 다시 확인!
    {
        if (currentMissionState != MissionState.Idle)
        {
            Debug.LogWarning("미션은 Idle 상태에서만 시작할 수 있습니다.");
            return;
        }
        
        SetMissionState(MissionState.GoToStartLocation);
        hasMissionStarted = true;
    }

    // --- 상태 전환 함수 ---
    private void SetMissionState(MissionState newState)
    {
        currentMissionState = newState;
        Debug.Log($"Mission State Changed to: {newState}");

        if (newState == MissionState.DeliveryInProgress)
        {
            initialDeliveryDistance = CalculateDistance(startLocationCoords.x, startLocationCoords.y, destinationCoords.x, destinationCoords.y);
            Debug.Log($"Initial Delivery Distance: {initialDeliveryDistance:F2} meters");
            
            if (initialDeliveryDistance < 1f) 
            {
                Debug.LogWarning("시작 지점과 목적지가 너무 가까워 즉시 완료 처리합니다.");
                SetMissionState(MissionState.MissionCompleted);
                return;
            }
        }
        UpdateUIForCurrentState(); // 상태 전환 시 UI 업데이트
    }

    // --- 거리 계산 (Haversine 공식) ---
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

    private float ToRadians(float deg)
    {
        return deg * Mathf.PI / 180f;
    }
}