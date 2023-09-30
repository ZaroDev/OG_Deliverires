using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Button : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    
    private void Awake()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _textMesh.text = name;
    }
}
