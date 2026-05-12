using UnityEngine;
using UnityEngine.UI;

public class RedDotBlinking : MonoBehaviour
{
    public Image dotImage;
    public float blinkSpeed = 1.5f; //how fast the light blinks

    void Update()
    {
        if (dotImage == null) 
            return;
        //0 = invisible | 1 = opaque
        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f); //calculate the number that goes from 0 to 1 and back to 0
        Color c = dotImage.color;
        //set the transparency (alpha) to fading number
        c.a = alpha;
        dotImage.color = c;
    }
}
