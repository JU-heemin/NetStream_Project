using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceEmoteSet : MonoBehaviour
{
    public void SetEmoteDisable()
    {
        FindObjectOfType<ARFaceBlendShapeVisualizer>().SetDisableEmote();
    }

    public void SetEmoteJoy()
    {
        FindObjectOfType<ARFaceBlendShapeVisualizer>().SetEmoteJoy();
    }

    public void SetEmoteAngry()
    {
        FindObjectOfType<ARFaceBlendShapeVisualizer>().SetEmoteAngry();
    }

    public void SetEmoteSorrw()
    {
        FindObjectOfType<ARFaceBlendShapeVisualizer>().SetEmoteSorrow();
    }

    public void SetEmoteFun()
    {
        FindObjectOfType<ARFaceBlendShapeVisualizer>().SetEmoteFun();
    }

    public void SetEmoteAnnyui()
    {
        FindObjectOfType<ARFaceBlendShapeVisualizer>().SetEmoteAnnyui();
    }



}
