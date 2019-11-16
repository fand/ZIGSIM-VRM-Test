using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using uOSC;
using VRM;

public class FaceController : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer _face;
    [SerializeField] VRMBlendShapeProxy _vrm;
    [SerializeField] Transform _root;
    [SerializeField] Transform _neck;
    [SerializeField] Transform _eyeTarget;
    uOscServer _osc;
    float _smile;
    float _smileL;
    float _smileR;

    float _angry;
    float _sad;
    float _surprise;
    float _fun;

    void Start()
    {
        _osc = GetComponent<uOscServer>();
        _osc.onDataReceived.AddListener(OnDataReceived);
    }

    void OnDataReceived(Message message)
    {
        var adr = Regex.Replace(message.address, @".*/", "");
        if (!adr.Contains("face")) { return; }

        var args = message.values.Select(x => (float)x).ToList();

        if (adr == "faceposition")
        {
            _root.position = new Vector3(-args[0], args[1], args[2]);
        }
        else if (adr == "facerotation")
        {
            var rot = new Quaternion(args[0], args[1], args[2], args[3]);
            var euler = rot.eulerAngles;
            _neck.rotation = Quaternion.Euler(euler.x, -euler.y, -euler.z);
            euler *= 0.02f;
            _root.rotation = Quaternion.Slerp(_root.rotation, Quaternion.Euler(-euler.x, euler.y, euler.z), 0.1f);
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
            Anim("A", args[0]);
        }
        else if (adr == "facemouthsmileleft")
        {
            _smileL = args[0];
        }
        else if (adr == "facemouthsmileright")
        {
            _smileR = args[0];
        }
        else if (adr == "facebrowdownleft" || adr == "facebrowdownright")
        {
            _angry = Mathf.Lerp(_angry, args[0], 0.2f);
        }
        else if (adr == "facemouthfrownleft" || adr == "facemouthfrownright")
        {
            _sad = Mathf.Lerp(_sad, args[0], 0.2f);
        }
        else if (adr == "facebrowinnerup")
        {
            _surprise = Mathf.Lerp(_surprise, args[0], 0.2f);
        }
        else if (adr == "facetongueout")
        {
            _fun = Mathf.Lerp(_fun, args[0], 0.2f);
        }
    }

    void Update()
    {
        _smile = Mathf.Lerp(_smileL, _smileR, 0.5f);
        Face("mouth_smile", _smile);

        Face("eyeblow_angry", _angry * 2);

        Face("eyebrow_trouble", _sad * 2);
        Face("mouth_lambda", _sad * 2 - 1);

        Face("eyeblow_up", _surprise * 2); // it's not typo! lol
        Face("eye_surprised", _surprise * 2);
        Face("mouth_shock", _surprise * 2 - 1);

        Anim("FUN", _fun * 2);
    }

    void Anim(string name, float value)
    {
        _vrm.ImmediatelySetValue(name, Mathf.Clamp(value, 0, 1));
    }

    void Face(string name, float value)
    {
        var index = _face.sharedMesh.GetBlendShapeIndex("bs_face." + name);
        _face.SetBlendShapeWeight(index, Mathf.Clamp(value * 100, 0, 100));
    }
}
