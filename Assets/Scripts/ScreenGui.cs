using TMPro;
using UnityEngine;

public class ScreenGui : MonoBehaviour
{
    static ScreenGui instance;
    public TextMeshProUGUI _matchText;

    public static TextMeshProUGUI matchText;

    void Awake()
    {
        instance = this;
        matchText = instance._matchText;
    }

}
