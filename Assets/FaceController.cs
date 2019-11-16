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
    [SerializeField] Transform _camera;
    uOscServer _osc;
    float _smile;
    float _smileL;
    float _smileR;

    float _angry;
    float _sad;
    float _surprise;
    float _fun;

    Vector2 _targetBias = Vector2.zero;

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

        // face transform
        if (adr == "faceposition")
        {
            _root.position = Vector3.Lerp(_root.position, new Vector3(-args[0], args[1], args[2] * 3), 0.1f);
        }
        else if (adr == "facerotation")
        {
            var rot = new Quaternion(args[0], args[1], args[2], args[3]);
            var euler = rot.eulerAngles;
            _neck.rotation = Quaternion.Slerp(_neck.rotation, Quaternion.Euler(euler.x, -euler.y, -euler.z), 0.3f);
            euler *= 0.02f;
            _root.rotation = Quaternion.Slerp(_root.rotation, Quaternion.Euler(-euler.x, euler.y, euler.z), 0.03f);
        }

        // face blendshapes
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

        // eye movement
        else if (adr == "faceeyelookinleft" || adr == "faceeyelookoutright")
        {
            _targetBias.x = Mathf.Lerp(_targetBias.x, args[0], 0.2f);
        }
        else if (adr == "faceeyelookoutleft" || adr == "faceeyelookinright")
        {
            _targetBias.x = Mathf.Lerp(_targetBias.x, -args[0], 0.2f);
        }
        else if (adr == "faceeyelookupleft" || adr == "faceeyelookupright")
        {
            _targetBias.y = Mathf.Lerp(_targetBias.y, args[0], 0.2f);
        }
        else if (adr == "faceeyelookdownleft" || adr == "faceeyelookdownright")
        {
            _targetBias.y = Mathf.Lerp(_targetBias.y, -args[0], 0.2f);
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

        // move eye target
        var targetDiff = _neck.rotation * Quaternion.Euler(_targetBias.y * -120, _targetBias.x * 120, 0) * Vector3.forward;
        var eyePoint = _neck.transform.position + Vector3.up * 0.15f;
        _eyeTarget.position = eyePoint + targetDiff;

        // Set camera target to neck
        _camera.rotation = Quaternion.Slerp(_camera.rotation, Quaternion.LookRotation(eyePoint - _camera.position, _camera.up), 0.02f);
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
