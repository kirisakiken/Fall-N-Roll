using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SS : MonoBehaviour
{

    private void Start()
    {
        string folderPath = Directory.GetCurrentDirectory() + "/Screenshots/";
        Debug.Log("folderPath " + folderPath);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            //Debug.Log("x");
            //if (Screen.height == 2436)
            //    TakeSS("X");//2436
            //else if (Screen.height == 2688)
            //{
            //Debug.Log("y");
            //    TakeSS("XR");//2688
            //}
            //else if (Screen.height == 2208)
            //    TakeSS("6");//2208
            //else if (Screen.height == 2732)
                TakeSS("1920");//2732
        }
    }
    void TakeSS(string name)
    {
        ScreenCapture.CaptureScreenshot($"{name} {GetTimeString()}.png");

        string GetTimeString()
        {
            return System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        }
    }
}
