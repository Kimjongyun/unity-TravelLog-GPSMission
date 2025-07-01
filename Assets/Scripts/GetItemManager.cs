using UnityEngine;

public class GetItemManager : MonoBehaviour
{
    private RaycastHit hitInfo;
    public int getItemCount;
    public int maxItemCount = 5;
    
    void Start()
    {
        // UIManager에서 CompleteText를 가져와 비활성화
        if (UIManager.instance != null && UIManager.instance.completeText != null)
            UIManager.instance.completeText.enabled = false;
        
        UIManager.instance?.SetCompleteTextVisible(false);
        UIManager.instance?.UpdateItemText(getItemCount, maxItemCount);
    }
    void Update()
    {
        if (getItemCount == maxItemCount)
        {
            UIManager.instance.completeText.enabled = true;
        }
        if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Input.mousePosition;
                Ray screenRay = Camera.main.ScreenPointToRay(mousePos);

                if (Physics.Raycast(screenRay.origin, screenRay.direction * 1000f, out hitInfo))
                {
                    if (hitInfo.collider.CompareTag("Item"))
                    {
                        hitInfo.collider.gameObject.SetActive(false);
                        getItemCount++;

                        UIManager.instance?.UpdateItemText(getItemCount, maxItemCount);
                    }
                }
            }
    }
}
