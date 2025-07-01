using UnityEngine;
using TMPro;

public class CheckDestination : MonoBehaviour
{
    public TextMeshProUGUI statusText;

    [Header("도착지 좌표")]
    public float targetLatitude = 37.4775921f;
    public float targetLongitude = 126.8612629f;

    [Header("반경 (도 단위)")]
    public float arrivalRadius = 0.00027f;

    bool hasArrived = false;

    void Update()
    {
        if (hasArrived || GPSManager.Instance == null) return ;

        float currentLat = GPSManager.Instance.latitude;
        float currentLon = GPSManager.Instance.longitude;

        if (IsWithinRadius(currentLat, currentLon, targetLatitude, targetLongitude, arrivalRadius))
        {
            hasArrived = true;
            statusText.text = "도착했습니다!";
        }
    }

    bool IsWithinRadius(float lat1, float lon1, float lat2, float lon2, float radius)
    {
        float dLat = Mathf.Abs(lat1 - lat2);
        float dLon = Mathf.Abs(lon1 - lon2);
        return dLat < radius && dLon < radius;
    }
}
