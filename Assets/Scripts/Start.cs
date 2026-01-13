
using System;
using UnityEngine;
public class StartScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        try
        {
            Debug.Log("[StartScene] Game started");
        }
        catch (Exception ex)
        {
            Debug.LogError("[StartScene] ❌ Exception in Start()");
            Debug.LogException(ex);
        }
    }

    // Update is called once per frame

}
