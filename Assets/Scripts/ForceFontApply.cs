using TMPro;
using UnityEngine;

public class ForceFontApply : MonoBehaviour
{
    public TMP_FontAsset koreanFont;

    void Start()
    {
        var texts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            t.font = koreanFont;
        }
    }
}
