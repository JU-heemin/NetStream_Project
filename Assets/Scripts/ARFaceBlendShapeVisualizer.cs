using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

public class ARFaceBlendShapeVisualizer : MonoBehaviour
{
    //ARKit �����ϴ� �Ķ���ʹ� 0~1�ǰ��� ���´� but, blendshape ���� 0~100�̹Ƿ� *100 ���ش�.
    private const float CoefficientValueScale = 100f;

    public SkinnedMeshRenderer faceMeshRenderer;

    //���� ȭ��� �ν��� �Ǹ� ������ �ν�, ���� ������ ������ ��Ȱ��ȭ
    private Renderer[] _characterRenderers;

    private ARFace _arFace;
    private ARFaceManager _arFacemanager;        //ARKitFaceSubsystem�� �ڵ����� �������ְ� �������ش�.
    private ARKitFaceSubsystem _arKitFaceSubsystem;

    private const int BlendShapeIndexLeftEyeBlink = 6;
    private const int BlendShapeIndexRightEyeBlink = 7;
    private const int BlendShapeIndexLookUp = 14;
    private const int BlendShapeIndexLookDown = 15;
    private const int BlendShapeIndexLookLeft = 16;
    private const int BlendShapeIndexLookRight = 17;

    private const int BlendShapeIndexMouthA = 1;

    // ǥ���� �������϶� �ڽ��� ���� Tracking ���� �ʰ� �ϱ� ���� ����
    private bool _emoteEnabled = false;

    private const int BlendShapeIndexJoy = 8;
    private const int BlendShapeIndexAngry = 10;
    private const int BlendShapeIndexSorrow = 11;
    private const int BlendShapeIndexFun = 12;
    private const int BlendShapeIndexAnnyui = 13;

    //ARKitBlendShapeLocation �� ARKit���� ������ ������ Ű�� -> Ű�鿡 �����Ǵ� value�� float�� -> Table�� ����
    private readonly Dictionary<ARKitBlendShapeLocation, float> _arKitBlendShapeValueTable
        = new Dictionary<ARKitBlendShapeLocation, float>();
    // Start is called before the first frame update
    private void Start()
    {
        _characterRenderers = GetComponentsInChildren<Renderer>();
        _arFace = GetComponent<ARFace>();
        _arFacemanager = FindObjectOfType<ARFaceManager>();

        _arKitFaceSubsystem = _arFacemanager.subsystem as ARKitFaceSubsystem;


        SetupARKitBlendShapeTable();

        // arFace�� ������Ʈ �ɶ����� ����
        _arFace.updated += OnFaceUpdated;

        // ARSession�� ������ ����Ŭ�� ������ ���������� �޼��� ����
        ARSession.stateChanged += OnARSessionStateChanged;
    }
    

    private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        // ARSession�� Ȱ��ȭ �ǰ� AR Face�� ���� ������ ���� ���۽�Ŵ
        if (args.state > ARSessionState.Ready && _arFace.trackingState == TrackingState.Tracking)
        {
            foreach(var characterRenderer in _characterRenderers)
            {
                characterRenderer.enabled = true;
            }
        }else
        {
            foreach (var characterRenderer in _characterRenderers)
            {
                characterRenderer.enabled = false;
            }
        }
    }

    // ���̺� Setup
    private void SetupARKitBlendShapeTable()
    {
        // �츮�� �����ϰ� ���� ��, �ʱⰪ
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeBlinkLeft, 0f);
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeBlinkRight, 0f);

        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeLookInLeft, 0f);
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeLookOutLeft, 0f);
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeLookUpLeft , 0f);
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeLookDownLeft, 0f);
        
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeLookInRight, 0f);
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeLookOutRight, 0f);
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeLookUpRight, 0f);
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.EyeLookDownRight, 0f);

        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.JawOpen, 0);
        _arKitBlendShapeValueTable.Add(ARKitBlendShapeLocation.MouthClose, 0);
    }

    private void OnFaceUpdated(ARFaceUpdatedEventArgs args)
    {
        UpdateARKitBlendShapeValues();
    }

    // ���� ������ ���ŵɶ����� ���̺��� Ű ����� ����
    private void UpdateARKitBlendShapeValues()
    {
        // �������� ������ ������ �����ͼ� ����
        var blendShapeCoefficients = _arKitFaceSubsystem.GetBlendShapeCoefficients(_arFace.trackableId, Allocator.Temp);

        foreach (var blendShapeCoefficient in blendShapeCoefficients)
        {
            // ���������� Location �޾ƿ�
            var blendShapeLocation = blendShapeCoefficient.blendShapeLocation;

            // ���� Location������ �ִ� ���̺� ���ŵ� Location�� ������
            if (_arKitBlendShapeValueTable.ContainsKey(blendShapeLocation))
            {
                // ���̺��� ���� �����Ѵ�.
                _arKitBlendShapeValueTable[blendShapeLocation] = blendShapeCoefficient.coefficient * CoefficientValueScale;
            }
        }
    }

    private void Update()
    {
        Apply();
    }

    private void Apply()
    {
        ApplyEyeMovement();

        if (_emoteEnabled)
        {
            return;
        }
        ResetBlendShape();

        ApplyEyeBlink();
        ApplyMouth();
    }

    private void ApplyEyeBlink()
    {
        var leftBlinkValue = _arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeBlinkLeft];
        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexLeftEyeBlink, leftBlinkValue);
        
        var rightBlinkValue = _arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeBlinkRight];
        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexRightEyeBlink, rightBlinkValue);
    }

    private void ApplyEyeMovement()
    {
        var lookUpValue = (_arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeLookUpLeft]
                            + _arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeLookUpRight]) * 0.5f;

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexLookUp, lookUpValue);

        var lookDownValue = (_arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeLookDownLeft]
                            + _arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeLookDownRight]) * 0.5f;

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexLookDown, lookDownValue);

        var lookLeftValue = (_arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeLookOutLeft]
                            + _arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeLookInRight]) * 0.5f;

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexLookLeft, lookLeftValue);

        var lookRightValue = (_arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeLookInLeft]
                            + _arKitBlendShapeValueTable[ARKitBlendShapeLocation.EyeLookOutRight]) * 0.5f;

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexLookRight, lookRightValue);
    }

    private void ApplyMouth()
    {
        var mouthOpenValue = (_arKitBlendShapeValueTable[ARKitBlendShapeLocation.JawOpen] -
                               _arKitBlendShapeValueTable[ARKitBlendShapeLocation.MouthClose]);

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexMouthA, mouthOpenValue); 
    }

    // Bledn Shape Reset -> ��� BlendShape�� ������ 0���� �ʱ�ȭ
    private void ResetBlendShape()
    {
        for(var i =0; i< faceMeshRenderer.sharedMesh.blendShapeCount; i++)
        {
            faceMeshRenderer.SetBlendShapeWeight(i, 0f);
        }
    }

    
    public void SetDisableEmote()
    {
        // ǥ���� ���� �ִٰ� �����Ҷ� ���
        _emoteEnabled = false;
        ResetBlendShape();
    }

    public void SetEmoteAngry()
    {
        _emoteEnabled = true;
        ResetBlendShape();

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexAngry, 100f);
    }

    public void SetEmoteJoy()
    {
        _emoteEnabled = true;
        ResetBlendShape();

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexJoy, 100f);
    }

    public void SetEmoteSorrow()
    {
        _emoteEnabled = true;
        ResetBlendShape();

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexSorrow, 100f);
    }

    public void SetEmoteFun()
    {
        _emoteEnabled = true;
        ResetBlendShape();

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexFun, 100f);
    }

    public void SetEmoteAnnyui()
    {
        _emoteEnabled = true;
        ResetBlendShape();

        faceMeshRenderer.SetBlendShapeWeight(BlendShapeIndexAnnyui, 100f);
    }




}
