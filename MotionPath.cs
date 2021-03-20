#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[ExecuteInEditMode]
public class MotionPath : MonoBehaviour
{
    public Animator Animator;
    public GameObject TargetObject;
    [HideInInspector] public float StartFrame = 0;
    [HideInInspector] public float EndFrame = 60;
    [HideInInspector] public float AnimationSlider;

    public Vector3[] PathPos = new Vector3[0];

    public AnimationClip[] AnimationClips;
    public string[] AniClipsName;
    public int SelectAniClip;
    public string PlayStateName; //재생할 스테이트 이름

    public int PathFrame = 120;

    public bool PathViewerSetting = false;
    public bool AutoUpdate = true;
    public Color PathColor = new Color(0, 1, 0);
    public float PathWidth = 2;

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += DrawSceneGUI;

    }
    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= DrawSceneGUI;
    }

    void DrawSceneGUI(SceneView sceneView)
    {
        MotionPath Ge = this;

        Handles.BeginGUI();
        Handles.EndGUI();
        Handles.color = PathColor;
        Handles.DrawAAPolyLine(PathWidth, Ge.PathPos.Length, Ge.PathPos);
        Handles.color = GUI.color;
    }
}

[CustomEditor(typeof(MotionPath))]
public class MotionPath_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        //GUILayout.Space(100);

        MotionPath Ge = (MotionPath)target;

        //애니메이터 
        EditorGUI.BeginChangeCheck();
        Ge.Animator = (Animator)EditorGUILayout.ObjectField("Animator", Ge.Animator, typeof(Animator));
        bool ChangeAniamtor = EditorGUI.EndChangeCheck(); //애니메이터 바뀜

        //추적할 오브젝트
        EditorGUI.BeginChangeCheck();
        Ge.TargetObject = (GameObject)EditorGUILayout.ObjectField("TargetObject", Ge.TargetObject, typeof(GameObject));
        if(EditorGUI.EndChangeCheck())
        {
            if(Ge.TargetObject != null)
            {
                //추적 오브젝트 변경 시 패스 재생성
                if(Ge.AutoUpdate)
                {
                    CreatePath();
                }
            }
        }

        //애니메이터가 있을 때
        if (Ge.Animator != null)
        {
            GUILayout.BeginVertical("GroupBox");
            DrawAniInfo();
            GUILayout.EndVertical();

            if (Ge.TargetObject != null)
            {
                GUILayout.BeginVertical("GroupBox");
                DrawPathInfo();
                GUILayout.EndVertical();
            }
        }

        //패스 정보
        void DrawPathInfo()
        {
            PathViewerSetting();
            Ge.AutoUpdate = EditorGUILayout.Toggle("Auto Update", Ge.AutoUpdate);

            EditorGUI.BeginChangeCheck();
            int PathDetail = EditorGUILayout.IntField("PathDetail (Frame)", Ge.PathFrame);
            PathDetail = Mathf.Max(PathDetail, 1); //최소값
            Ge.PathFrame = PathDetail;
            if(EditorGUI.EndChangeCheck()) //패스 디테일 수정하면 업데이트
            {
                if(Ge.AutoUpdate)
                {
                    CreatePath();
                }
            }

            if(!Ge.AutoUpdate)
            {
                if (GUILayout.Button("Create Path"))
                {
                    CreatePath();
                }
            }

        }

        //CreatePath (패스 생성)
        void CreatePath()
        {
            float FirstFrame = Ge.AnimationSlider; //현재 포즈 시간 백업
            List<Vector3> NewPathPosition = new List<Vector3>();
            for (float i = Ge.StartFrame; i < Ge.EndFrame; i += (1f / (float)Ge.PathFrame))
            {
                Ge.AnimationSlider = i; //포즈 시간 업데이트
                UpDatePos(); //포즈 업데이트
                NewPathPosition.Add(Ge.TargetObject.transform.position);
            }
            Ge.PathPos = NewPathPosition.ToArray();

            Ge.AnimationSlider = FirstFrame; //처음 설정한 포즈 시간으로 백업
            UpDatePos(); //포즈 업데이트

            Debug.Log("Create Path");
        }

        //패스 보기 설정
        void PathViewerSetting()
        {
            Ge.PathViewerSetting = EditorGUILayout.Toggle("Path View Setting", Ge.PathViewerSetting);
            if (Ge.PathViewerSetting)
            {
                GUILayout.BeginVertical("GroupBox");
                Ge.PathColor = EditorGUILayout.ColorField("Path Color", Ge.PathColor);
                Ge.PathWidth = EditorGUILayout.FloatField("Path Width", Ge.PathWidth);
                GUILayout.EndVertical();
            }
        }

        //애니메이션 정보
        void DrawAniInfo()
        {
            //애니메이터 바뀜
            if (ChangeAniamtor)
            {
                Ge.AnimationClips = Ge.Animator.runtimeAnimatorController.animationClips;
                Ge.AniClipsName = new string[Ge.AnimationClips.Length];
                for (int i = 0; i < Ge.AniClipsName.Length; i++)
                {
                    Ge.AniClipsName[i] = Ge.AnimationClips[i].name;
                }
            }

            float SelectClipLength = Ge.AnimationClips[Ge.SelectAniClip].length; //선택한 클립의 최대 길이


            //플레이할 애니메이션 선택
            EditorGUI.BeginChangeCheck();
            Ge.SelectAniClip = EditorGUILayout.Popup("SelectAni", Ge.SelectAniClip, Ge.AniClipsName);
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log("Change Animation");
                Ge.PlayStateName = GetStringFromAniClip(Ge.Animator, Ge.AnimationClips[Ge.SelectAniClip]); //재생할 애니메이션 스테이트 이름 가져오기

                //시작, 끝 프레임 기본 설정
                //Ge.StartFrame = 0;
                //Ge.EndFrame = Ge.AnimationClips[Ge.SelectAniClip].length;
                UpDatePos();

                if(Ge.AutoUpdate)
                {
                    CreatePath();
                }
            }

            EditorGUI.BeginChangeCheck(); //포즈 관련 변수 업데이트 되는지 확인
            //GUILayout.BeginHorizontal();
            //최소값
            float SetStartFrame = EditorGUILayout.FloatField("Start Frame", Ge.StartFrame);
            SetStartFrame = Mathf.Clamp(SetStartFrame, 0, Ge.EndFrame);
            Ge.StartFrame = SetStartFrame;

            //최대값
            float SetEndFrame = EditorGUILayout.FloatField("End Frame", Ge.EndFrame);
            SetEndFrame = Mathf.Clamp(SetEndFrame, Ge.StartFrame, SelectClipLength);
            Ge.EndFrame = SetEndFrame;

            //GUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider("Set Frame", ref Ge.StartFrame, ref Ge.EndFrame, 0, SelectClipLength);
            bool ChangeMinMax = EditorGUI.EndChangeCheck();
            if(ChangeMinMax) //패스랑 관련된 값들 변경되면 패스 새로 생성
            {
                if(Ge.AutoUpdate)
                {
                    CreatePath();
                }
            }

            //애니 재생
            EditorGUI.BeginChangeCheck(); //포즈 관련 변수 업데이트 되는지 확인
            Ge.AnimationSlider = EditorGUILayout.Slider("Ani Play", Ge.AnimationSlider, Ge.StartFrame, Ge.EndFrame);

            //포즈 관련된 변수들이 변했을 경우
            if (EditorGUI.EndChangeCheck() || ChangeMinMax)
            {
                //Debug.Log("애니메이션 포즈 업데이트 중");
                UpDatePos(); //애니메이션 포즈 업데이트
            }
        }

        //포즈 업데이트
        void UpDatePos()
        {
            Ge.Animator.Play(Ge.PlayStateName, -1, Ge.AnimationSlider);
            Ge.Animator.Update(Time.deltaTime);
        }
    }

    AnimatorState[] AllState;
    //클립으로 애니메이션 스테이트 이름 가져오기 (AnimatorPlay용 String 받아올 때 씀)
    string GetStringFromAniClip(Animator GetAnimator, AnimationClip Clip)
    {
        string OutString = "";
        AllState = GetAnimatorStates(GetAnimator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController); //모든 스테이트 가져오기   
        OutString = GetStateFromClip(AllState, Clip).name; //애니메이션 클립으로 부터 State가져 와서 이름 할당
        return OutString;
    }

    //모든 애니메이터 스테이트 가져오기
    AnimatorState[] GetAnimatorStates(UnityEditor.Animations.AnimatorController anicon)
    {
        List<AnimatorState> ret = new List<AnimatorState>();
        foreach (var layer in anicon.layers)
        {
            foreach (var subsm in layer.stateMachine.stateMachines)
            {
                foreach (var state in subsm.stateMachine.states)
                {
                    ret.Add(state.state);
                }
            }
            foreach (var s in layer.stateMachine.states)
            {
                ret.Add(s.state);
            }
        }
        return ret.ToArray();
    }

    //모든 애니메이터 스테이트 중에 애니메이션 클립에 해당하는스테이트 가져오기
    AnimatorState GetStateFromClip(AnimatorState[] StateList, AnimationClip GetClip)
    {
        AnimatorState OutState = null;
        for (int i = 0; i < StateList.Length; i++)
        {
            if (StateList[i].motion == GetClip)
            {
                OutState = StateList[i];
                break; //정지
            }
        }
        return OutState;
    }

}

#endif