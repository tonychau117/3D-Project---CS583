using UnityEngine;
using UnityEngine.UI;

public class RedDotBlinking : MonoBehaviour
{
    public Image dotImage;
    public float blinkSpeed = 1.5f;

    void Update()
    {
        if (dotImage == null) return;

        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        Color c = dotImage.color;
        c.a = alpha;
        dotImage.color = c;
    }
}
