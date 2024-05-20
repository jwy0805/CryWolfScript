using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable Unity.InefficientPropertyAccess

public class UI_FloatingText : MonoBehaviour
{
    private Transform _text;
    private Camera _camera;
    
    private void Start()
    {
        _text = transform.GetChild(0);
        _camera = Camera.main;
        transform.position += SetTextPosition();
        
        Managers.Resource.Destroy(gameObject, 0.5f);
    }

    private void Update()
    {
        _text.transform.rotation = _camera.transform.rotation;
        transform.position += Vector3.up * Time.deltaTime;
    }

    private Vector3 SetTextPosition()
    {
        float radius = 0.5f;
        float angle = Random.Range(0, 2 * Mathf.PI);
        float x = radius * Mathf.Cos(angle);
        float z = radius * Mathf.Sin(angle);
        Vector3 textPos = new Vector3(x, 1.5f, z);

        return textPos;
    }
}
