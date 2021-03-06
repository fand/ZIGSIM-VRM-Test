﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using uOSC;

public class HeadController : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer _face;
    uOscServer _osc;
    float _smile;
    float _smileL;
    float _smileR;

    void Start()
    {
        _osc = GetComponent<uOscServer>();
        _osc.onDataReceived.AddListener(OnDataReceived);
    }

    void OnDataReceived(Message message)
    {
        var adr = Regex.Replace(message.address, @".*/", "");
        var args = message.values.Select(x => (float)x).ToList();

        if (adr == "faceposition")
        {
            transform.position = new Vector3(-args[0], args[1], args[2]);
        }
        else if (adr == "facerotation")
        {
            var rot = new Quaternion(args[0], args[1], args[2], args[3]);
            var euler = rot.eulerAngles;
            transform.rotation = Quaternion.Euler(euler.x, -euler.y, -euler.z);
        }
        else if (adr == "faceeyeblinkleft")
        {
            var x = args[0] * 1.4f;
            Face("eye_winkL", x * (1 - _smile));
            Face("eye_winkL2", x * _smile);
        }
        else if (adr == "faceeyeblinkright")
        {
            var x = args[0] * 1.4f;
            Face("eye_winkR", x * (1 - _smile));
            Face("eye_winkR2", x * _smile);
        }
        else if (adr == "facejawopen")
        {
            Face("mouth_a", args[0]);
        }
        else if (adr == "facemouthsmileleft")
        {
            _smileL = args[0];
        }
        else if (adr == "facemouthsmileright")
        {
            _smileR = args[0];
        }
    }

    void Update()
    {
        _smile = Mathf.Lerp(_smileL, _smileR, 0.5f) * 2;
        Face("mouth_smile", _smile);
    }

    void Face(string name, float value)
    {
        var index = _face.sharedMesh.GetBlendShapeIndex(name);
        _face.SetBlendShapeWeight(index, value * 100);
    }

}
