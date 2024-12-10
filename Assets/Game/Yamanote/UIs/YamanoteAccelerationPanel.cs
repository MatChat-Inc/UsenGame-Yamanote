using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class YamanoteAccelerationPanel : MonoBehaviour
{
    public Image warningImage;
    public TextMeshProUGUI accelerationText;
    public PlayableDirector director;
    
    void Start()
    {
        
    }

    // private void OnEnable()
    // {
    //     accelerationText
    //         .DOFade(0, 0.5f)
    //         .SetLoops(-1, LoopType.Yoyo)
    //         .SetId("AccelerationText");
    // }
    //
    // private void OnDisable()
    // {
    //     DOTween.Kill("AccelerationText");
    // }

    public void StartAcceleration()
    {
        // Animate the warning image and text
        // warningImage.transform.DOMove()
    }
}
