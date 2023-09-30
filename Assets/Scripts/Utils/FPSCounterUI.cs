using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounterUI : MonoBehaviour
{
    private const float fpsMeasurePeriod = 0.5f;
    private float fpsAccumulator = 0;
    private float fpsNextPeriod = 0;
    private float currentFPS;
    private float maxFPS;
    private float minFPS;
    private const string display = "{0} Current FPS\n {1} Max FPS\n{2} Min FPS";
    private TMPro.TextMeshProUGUI _textMesh;
    void Awake()
    {
        _textMesh = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Start()
    {
        fpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
    }

    private void Update()
    {
        fpsAccumulator++;
        if (Time.realtimeSinceStartup > fpsNextPeriod)
        {
            currentFPS = fpsAccumulator / fpsMeasurePeriod;
            if (currentFPS > maxFPS)
                maxFPS = currentFPS;
            if (currentFPS < minFPS || minFPS == 0f)
                minFPS = currentFPS;
            
            fpsAccumulator = 0;
            fpsNextPeriod += fpsMeasurePeriod;
            _textMesh.text = string.Format(display, currentFPS, maxFPS, minFPS);
        }
    }
}
