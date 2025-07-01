using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class GPSManager : MonoBehaviour
{
    // 기존 text_ui 삭제: public Text text_ui; 

    public static GPSManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GPSManager>();
            }

            return m_instance;
        }
    }

    public float latitude { get; private set; }
    public float longitude { get; private set; }

    private float desiredAccuracyInMeters = 4f;
    private float updateDistanceInMeters = 4f;
    
    private static GPSManager m_instance;
    private LocationServiceStatus currentLocationStatus = LocationServiceStatus.Stopped;

    void Start()
    {
        StartCoroutine(InitializeLocationService());
    }

    void Update()
    {
        currentLocationStatus = Input.location.status;

        if (Input.location.status == LocationServiceStatus.Running)
        {
            UpdateLocationData();
            UIManager.instance.UpdateUIText();
        }
        else
        {
            // UIManager에서 GPSText_UI를 가져와서 사용
            if (UIManager.instance != null && UIManager.instance.GPSText_UI != null)
            {
                UIManager.instance.GPSText_UI.text = "Need to active GPS";
            }
        }
    }

    private IEnumerator InitializeLocationService()
    {
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null;
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        if (!Input.location.isEnabledByUser)
            Debug.Log("Location not enabled on device or app does not have permission to access location");

        Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude);

            // UIManager에서 UI를 사용
            if (UIManager.instance != null && UIManager.instance.GPSText_UI != null)
            {
                UIManager.instance.GPSText_UI.text =
                    Input.location.lastData.latitude + " / " + Input.location.lastData.longitude;
            }
        }
    }

    private void UpdateLocationData()
    {
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
    }

    public LocationServiceStatus GetLocationStatus()
    {
        return currentLocationStatus;
    }

}
