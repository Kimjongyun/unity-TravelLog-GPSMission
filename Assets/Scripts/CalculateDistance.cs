using UnityEngine;

public class CalculateDistance : MonoBehaviour
{
    private double lastLat, lastLon;
    private bool isStarted = false;
    public float movedDistance;

    public float targetDistance = 100;
    float movementThreshold = 3f;
    float spikeThreshold = 20f;

    private float updateInterval = 1.0f;
    private float timeSinceLastUpdate = 0f;

    void Update()
    {
        // 대화가 끝나야만 이동/거리측정 가능
        if (!UIManager.instance || !UIManager.instance.dialogueFinished)
            return;

        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate < updateInterval)
            return;
        timeSinceLastUpdate = 0f;

        double curLat = GPSManager.Instance.latitude;
        double curLon = GPSManager.Instance.longitude;

        float delta = Haversine((float)lastLat, (float)lastLon, (float)curLat, (float)curLon);

        if (delta >= movementThreshold && delta < spikeThreshold)
            movedDistance += delta;

        lastLat = curLat;
        lastLon = curLon;

        UIManager.instance?.UpdateInfoUI(movedDistance);
    }

    // 이 함수만 public으로, UIManager가 이걸 호출함
    public void SetStartPoint()
    {
        lastLat = GPSManager.Instance.latitude;
        lastLon = GPSManager.Instance.longitude;
        movedDistance = 0;
        isStarted = true;
        //UIManager.instance?.SetDistanceInfoMessage("시작 위치가 저장되었습니다. 이동을 시작하세요!");
    }

    private float Haversine(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371000f;
        float dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        float dLon = (lon2 - lon1) * Mathf.Deg2Rad;

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(lat1 * Mathf.Deg2Rad) * Mathf.Cos(lat2 * Mathf.Deg2Rad) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1f - a));
        return R * c;
    }
}