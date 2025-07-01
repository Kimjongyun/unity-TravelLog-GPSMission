using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectMenu : MonoBehaviour
{
    public void ClickDelivery()
    {
        SceneManager.LoadScene("Delivery");
    }
    public void ClickCat()
    {
        SceneManager.LoadScene("Cat");
    }
}
