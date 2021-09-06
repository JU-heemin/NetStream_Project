using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

public class ARFaceBlendShapeVisualizer : MonoBehaviour
{
    //ARKit 제공하는 파라미터는 0~1의값이 들어온다 but, blendshape 값은 0~100이므로 *100 해준다.
    private const float CoefficientValueScale = 100f;

    public SkinnedMeshRenderer faceMeshRenderer;

    //얼굴이 화면상에 인식이 되면 렌더러 인식, 얼굴이 없으면 렌더러 비활성화
    private Renderer[] _characterRenderers;

    private ARFace _arFace;
    private ARFaceManager _arFacemanager;        //ARKitFaceSubsystem을 자동으로 생성해주고 제공해준다.
    private ARKitFaceSubsystem _arKitFaceSubsystem;

    private const int BlendShapeIndexLeftEyeBlink = 6;
    private const int BlendShapeIndexRightEyeBlink = 7;
    private const int BlendShapeIndexLookUp = 14;
    private const int BlendShapeIndexLookDown = 15;
    private const int BlendShapeIndexLookLeft = 16;
    private const int BlendShapeIndexLookRight = 17;

    private const int BlendShapeIndexMouthA = 1;

    // 표정이 구현중일때 자신의 얼굴을 Tracking 하지 않게 하기 위한 변수
    private bool _emoteEnabled = false;

    private const int BlendShapeIndexJoy = 8;
    private const int BlendShapeIndexAngry = 10;
    private const int BlendShapeIndexSorrow = 11;
    private const int BlendShapeIndexFun = 12;
    private const int BlendShapeIndexAnnyui = 13;

    //ARKitBlendShapeLocation 이 ARKit에서 측정한 각각의 키들 -> 키들에 대응되는 value는 float값 -> Table에 저장
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

        // arFace가 업데이트 될때마다 실행
        _arFace.updated += OnFaceUpdated;

        // ARSession의 라이프 사이클에 변경이 있을때마다 메서드 실행
        ARSession.stateChanged += OnARSessionStateChanged;
    }
    

    private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        // ARSession이 활성화 되고 AR Face가 동작 가능할 때만 동작시킴
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

    // 테이블 Setup
    private void SetupARKitBlendShapeTable()
    {
        // 우리가 측정하고 싶은 값, 초기값
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

    // 얼굴의 정보가 갱신될때마다 테이블의 키 밸류를 갱신
    private void UpdateARKitBlendShapeValues()
    {
        // 아이폰이 측정한 값들을 가져와서 갱신
        var blendShapeCoefficients = _arKitFaceSubsystem.GetBlendShapeCoefficients(_arFace.trackableId, Allocator.Temp);

        foreach (var blendShapeCoefficient in blendShapeCoefficients)
        {
            // 아이폰으로 Location 받아옴
            var blendShapeLocation = blendShapeCoefficient.blendShapeLocation;

            // 만약 Location값들이 있는 테이블에 갱신된 Location이 있으면
            if (_arKitBlendShapeValueTable.ContainsKey(blendShapeLocation))
            {
                // 테이블의 값을 갱신한다.
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

    // Bledn Shape Reset -> 모든 BlendShape의 값들을 0으로 초기화
    private void ResetBlendShape()
    {
        for(var i =0; i< faceMeshRenderer.sharedMesh.blendShapeCount; i++)
        {
            faceMeshRenderer.SetBlendShapeWeight(i, 0f);
        }
    }

    
    public void SetDisableEmote()
    {
        // 표정을 짓고 있다가 해제할때 사용
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
