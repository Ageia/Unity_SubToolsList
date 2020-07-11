using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class MemoryPool_System : MonoBehaviour
{
    Dictionary<GameObject, List<GameObject>> ObjectList = new Dictionary<GameObject, List<GameObject>>(); //실제로 쓰는 데이터

    ////////////////////////////////////////////////////////
    ///////////////     타입별 불러오기      ////////////////
    ////////////////////////////////////////////////////////
    public GameObject CallObject(GameObject TargetGameObject)
    {
        return CallOObject_Active(TargetGameObject, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), null); ;
    }
    public GameObject CallObject(GameObject TargetGameObject, Vector3 position, Quaternion rotation)
    {
        CallOObject_Active(TargetGameObject, position, rotation, null);
        return null;
    }
    public GameObject CallObject(GameObject TargetGameObject, Vector3 position, Quaternion rotation, Transform parent)
    {
        CallOObject_Active(TargetGameObject, position, rotation, parent);
        return null;
    }
    public List<GameObject> GetTargetObject(GameObject GameObject)
    {
        List<GameObject> AllObject = new List<GameObject>();
        for (int i = 0; i < ObjectList.Count; i++)
        {
            List<GameObject> GetList = ObjectList[GameObject];

            for (int j = 0; j < GetList.Count; j++)
            {
                AllObject.Add(GetList[j]); //모든 오브젝트에 추가
            }
        }

        //오브젝트 없는지 확인 후 없을 경우 리스트에서 제거
        for (var i = AllObject.Count - 1; i > -1; i--)
        {
            if (AllObject[i] == null)
                AllObject.RemoveAt(i);
        }

        //리스트 중복 제거
        AllObject = AllObject.Distinct().ToList();

        return AllObject;
    }
    public List<GameObject> GetAllObject() //이 메모리풀 시스템에서 관리하는 모든 오브젝트를 가져옵니다. 게임 씬에 없는 오브젝트는 리스트에서 제거합니다.
    {
        List<GameObject> AllObject = new List<GameObject>();
        for (int i = 0; i < ObjectList.Count; i++)
        {
            List<GameObject> GetList = ObjectList[ObjectList.Keys.ToArray()[i]];

            for (int j = 0; j < GetList.Count; j++)
            {
                AllObject.Add(GetList[j]); //모든 오브젝트에 추가
            }
        }

        //오브젝트 없는지 확인 후 없을 경우 리스트에서 제거
        for (var i = AllObject.Count - 1; i > -1; i--)
        {
            if (AllObject[i] == null)
                AllObject.RemoveAt(i);
        }

        //리스트 중복 제거
        AllObject = AllObject.Distinct().ToList();

        return AllObject;
    }
    public void RemoveTargetObject(GameObject GameObject) //이 메모리풀에서 해당 오브젝트를 사용하는 모든 오브젝트를 제거 합니다.
    {
        foreach (var item in GetTargetObject(GameObject))
        {
            Destroy(item);
        }
    }
    public void RemoveAllObject() //이 메모리풀에 존재 하는 모든 오브젝트를 제거 합니다.
    {
        foreach (var item in GetAllObject())
        {
            Destroy(item);
        }
    }

    /////////////////////////////////////////////////////////////
    ///////////////     오브젝트 호출 시스템      ////////////////
    /////////////////////////////////////////////////////////////
    #region
    //타입별 불러오기 처리
    GameObject CallOObject_Active(GameObject TargetGameObject, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (ObjectList.ContainsKey(TargetGameObject) == false) //리스트 자체가 없을 경우
        {
            return CreateNewObjectList(TargetGameObject, position, rotation, parent); //리스트도 새로 생성 하고 오브젝트도 새로 생성해서 추가
        }
        else //있을 경우
        {
            GameObject GetObject = SearchListActive(TargetGameObject);
            if (GetObject == null) //리스트가 있는데 오브젝트가 없을 경우
            {
                return AddTargetDictionalyList(TargetGameObject, position, rotation, parent); //해당 리스트에 새로운놈 추가 한 뒤에 리턴 받음
            }
            else //리스트도 있고 비활성 오브젝트도 있을 경우
            {
                //오브젝트 위치, 회전값 적용 후 켜기
                GetObject.transform.position = position;
                GetObject.transform.rotation = rotation;
                GetObject.transform.parent = parent;
                GetObject.SetActive(true);
                return GetObject;
            }
        }
    }
    GameObject CreateNewObjectList(GameObject CreateTargetObject, Vector3 position, Quaternion rotation, Transform parent) //새로운 딕셔너리 리스트 정보 추가
    {
        //새로운 리스트 구성
        List<GameObject> TargetObjectList = new List<GameObject>();
        GameObject NewObject = Instantiate(CreateTargetObject, position, rotation, parent);
        //NewObject.SetActive(false); //오브젝트 끄기
        TargetObjectList.Add(NewObject); //리스트 추가 한뒤
        ObjectList.Add(CreateTargetObject, TargetObjectList); //딕셔너리 정보 추가 후
        return NewObject; //선택된 오브젝트 바로 보내기
    }
    GameObject AddTargetDictionalyList(GameObject OriginalObject, Vector3 position, Quaternion rotation, Transform parent) //딕셔너리 정보가 이미 있는 곳에 새로운 오브젝트 추가
    {
        GameObject NewObject = Instantiate(OriginalObject, position, rotation, parent); //새로운 오브젝트 생성
        ObjectList[OriginalObject].Add(NewObject); //리스트에 이 데이터 추가
        return NewObject; //선택된 오브젝트 바로 보내기
    }
    GameObject SearchListActive(GameObject TargetGameObject) //리스트 안에 꺼져 있는 오브젝트 있는지 확인
    {
        for (int i = 0; i < ObjectList[TargetGameObject].Count; i++)
        {
            if (ObjectList[TargetGameObject][i] == null)
            {
                ObjectList[TargetGameObject].Remove(ObjectList[TargetGameObject][i]); //만약에 대상이 씬에 없을 경우 리스트에서 제거
            }
            else if (ObjectList[TargetGameObject][i].activeSelf == false)
            {
                return ObjectList[TargetGameObject][i];
            }
        }
        return null;
    }
    #endregion

}

