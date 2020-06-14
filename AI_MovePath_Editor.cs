#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(AI_MovePath))]
public class AI_MovePath_Editor : Editor
{
    public bool HideButton ;
    public bool HideGizmo ;
    public bool HideIndex ;

    GUIStyle StringStyle = new GUIStyle();
    GUIStyle StringStyle_AddPosition = new GUIStyle();
    GUIStyle StringStyle_RemovePosition = new GUIStyle();

    AI_MovePath Genarator;

    void OnSceneGUI()
    {
        Genarator = (AI_MovePath)target;

        if (Genarator.MovePosition != null && Genarator.MovePosition.Count > 0)
        {
            //기본 폰트 스타일
            StringStyle.normal.textColor = Color.green;
            StringStyle.fontStyle = FontStyle.Bold;
            StringStyle.fontSize = 30;
            StringStyle.alignment = TextAnchor.MiddleCenter;

            //버튼 스타일
            var ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Color.white;
            ButtonStyle.fontStyle = FontStyle.Bold;
            ButtonStyle.fontSize = 15;
            ButtonStyle.alignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < Genarator.MovePosition.Count; i++)
            {
                if(!HideGizmo)
                {
                    Genarator.MovePosition[i] = Handles.PositionHandle(Genarator.MovePosition[i], Quaternion.identity); //기즈모 표시
                }
                if (!HideIndex)
                {
                    Handles.Label(Genarator.MovePosition[i] + new Vector3(0, -0.25f, 0), i.ToString(), StringStyle); //배열 숫자 표시
                }

                if(!HideButton)
                {
                    Handles.BeginGUI();
                    //위치 추가
                    GUI.backgroundColor = new Color(0.3f, 0.5f, 2);
                    if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Genarator.MovePosition[i]).x, HandleUtility.WorldToGUIPoint(Genarator.MovePosition[i]).y - 65, 80, 20), "Add", ButtonStyle))
                    {
                        Genarator.MovePosition.Insert(i, new Vector3(Genarator.MovePosition[i].x, Genarator.MovePosition[i].y, Genarator.MovePosition[i].z));
                    }

                    //위치 제거
                    GUI.backgroundColor = new Color(2, 0.5f, 0.2f);
                    if (GUI.Button(new Rect(HandleUtility.WorldToGUIPoint(Genarator.MovePosition[i]).x, HandleUtility.WorldToGUIPoint(Genarator.MovePosition[i]).y - 40, 80, 20), "Remove", ButtonStyle))
                    {
                        Genarator.MovePosition.RemoveAt(i);
                    }
                    Handles.EndGUI();
                }
            }

            Handles.color = new Color(2, 0.5f, 0.2f, 5);
            Vector3[] Vector3Array = new Vector3[Genarator.MovePosition.Count];
            for (int i = 0; i < Vector3Array.Length; i++)
            {
                Vector3Array[i] = Genarator.MovePosition[i];
            }
            Handles.DrawAAPolyLine(10, Genarator.MovePosition.Count, Vector3Array);
        }
        else
        {
            Genarator.MovePosition = new List<Vector3>();
        }
    }


    //인스펙터 GUI
    ReorderableList reorderableList;
    void OnEnable()
    {
        //타겟 컴포넌트
        Genarator = (AI_MovePath)target;

        //리스트 갯수 받아올 데이터
        reorderableList = new ReorderableList(serializedObject,
                                 serializedObject.FindProperty("MovePosition"));

        reorderableList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "MovePosition");
        };

        //리스트 안에 GUI구성
        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
            Genarator.MovePosition[index] = EditorGUI.Vector3Field(rect, index.ToString(), Genarator.MovePosition[index]);
        };

        //리스트에 +버튼 눌렀을 때 처리
        reorderableList.onAddCallback += (list) => {
            if (Genarator.MovePosition.Count > 0)
            {
                //Genarator.MovePosition.Insert(list.index, new Vector3(Genarator.MovePosition[list.index -1].x, Genarator.MovePosition[list.index].y, Genarator.MovePosition[list.index].z));
                //Genarator.MovePosition.Insert(list.index, Genarator.MovePosition[list.index - 1]);
                Genarator.MovePosition.Add(Genarator.MovePosition[Genarator.MovePosition.Count - 1]);
            }
            else
            {
                Genarator.MovePosition.Add(Genarator.transform.position);
            }

        };

        //리스트에 -버튼 눌렀을 때 처리
        reorderableList.onRemoveCallback += (list) => {
            Genarator.MovePosition.RemoveAt(list.index);
        };
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        HideButton = GUILayout.Toggle(HideButton, "버튼 숨기기" );
        HideGizmo = GUILayout.Toggle(HideGizmo, "기즈모 숨기기");
        HideIndex = GUILayout.Toggle(HideIndex, "인덱스 숨기기");

        //초기화 버튼
        if (GUILayout.Button("ResetPath"))
        {
            Genarator.MovePosition.Clear();
        }

        //리오더블
        reorderableList.DoLayoutList();
    }
}

#endif