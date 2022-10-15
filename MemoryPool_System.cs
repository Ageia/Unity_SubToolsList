using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class MemoryPool_System : MonoBehaviour
{
    //사용법 : 
    //GetObj 함수로 오브젝트 불러오기.
    //사용하지 않는 오브젝트는 SetActive(false)
    ////////////////////////////////////////////////////////
    ///////////////     타입별 불러오기      ////////////////
    ////////////////////////////////////////////////////////
    public GameObject GetObj(GameObject TargetGameObject)
    {
        return CallObj(TargetGameObject, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), null); ;
    }
    public GameObject GetObj(GameObject TargetGameObject, Vector3 position)

    {
        return CallObj(TargetGameObject, position, Quaternion.Euler(0, 0, 0), null); ;
    }
    public GameObject GetObj(GameObject TargetGameObject, Vector3 position, Quaternion rotation)
    {
        return CallObj(TargetGameObject, position, rotation, null);
    }
    public GameObject GetObj(GameObject TargetGameObject, Vector3 position, Quaternion rotation, Transform parent)
    {
        return CallObj(TargetGameObject, position, rotation, parent);
    }
    public void OffObj(GameObject obj) //오브젝트 끌때 사용
    {
        obj.SetActive(false);
        obj.transform.position = new Vector3(0, 0, 0);
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = new Vector3(1, 1, 1);
        obj.transform.parent = ParentPool.transform;
    }
    public void ClearAll() //이 메모리풀에 존재 하는 모든 오브젝트를 제거 합니다.
    {
        foreach (var item in GetAllObject())
        {
            Destroy(item);
        }
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //싱글턴
    Dictionary<GameObject, List<GameObject>> ObjectList = new Dictionary<GameObject, List<GameObject>>(); //실제로 쓰는 데이터
    public static MemoryPool_System instance = null; //게임 매니저 객체
    GameObject ParentPool;
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Awake()
    {
        if (instance == null) //게임 매니저 객체가 없을 때
        {
            instance = this; //이 객체를 매니저로 지정
        }
        else if (instance != null) //만약 이미 있을 때 자신이 초기의 객체가 아닐 경우
        {
            Destroy(this.gameObject); //현재 객체를 제거
        }
        ParentPool = new GameObject("MemoryPool_ObjsParent");
        ParentPool.transform.parent = this.transform;
        DontDestroyOnLoad(this.gameObject); //씬이 넘어가도 이 오브젝트는 유지한다.
    }
    public List<GameObject> GetAllObject() //이 메모리풀 시스템에서 관리하는 모든 오브젝트를 가져옵니다. 게임 씬에 없는 오브젝트는 리스트에서 제거합니다.
    {
        List<GameObject> AllObject = new List<GameObject>();
        for (int i = 0; i < ObjectList.Count; i++)
        {
            List<GameObject> GetList = ObjectList[ObjectList.Keys.ToArray()[i]];
            foreach (var item in GetList)
            {
                AllObject.Add(item);
            }
        }
        return AllObject;
    }
    GameObject CallObj(GameObject TargetGameObject, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (ObjectList.ContainsKey(TargetGameObject) == false) //리스트 자체가 없을 경우
        {
            List<GameObject> TargetObjectList = new List<GameObject>();
            ObjectList[TargetGameObject] = TargetObjectList;
            GameObject obj = InstantiateObj(TargetGameObject, position, rotation, parent);
            return obj; //리스트도 새로 생성 하고 오브젝트도 새로 생성해서 추가
        }
        else //있을 경우
        {
            GameObject GetObject = FindObj_ActiveFalse(TargetGameObject);
            if (GetObject == null)
            {
                return InstantiateObj(TargetGameObject, position, rotation, parent); //해당 리스트에 새로운놈 추가 한 뒤에 리턴 받음
            }
            else //리스트도 있고 비활성 오브젝트도 있을 경우
            {
                GetObject.transform.position = position;
                GetObject.transform.rotation = rotation;
                GetObject.transform.parent = parent;
                GetObject.SetActive(true);
                return GetObject;
            }
        }
    }
    GameObject InstantiateObj(GameObject OriginalObject, Vector3 position, Quaternion rotation, Transform parent) 
    {
        GameObject NewObject = Instantiate(OriginalObject, position, rotation, parent); //새로운 오브젝트 생성
        ObjectList[OriginalObject].Add(NewObject); //리스트에 이 데이터 추가
        return NewObject; //선택된 오브젝트 바로 보내기
    }
    GameObject FindObj_ActiveFalse(GameObject GetObj) //리스트 안에 꺼져 있는 오브젝트 있는지 확인
    {
        List<GameObject> ObjList = ObjectList[GetObj];
        foreach (var item in ObjList)
        {
            if(item.activeSelf == false) return item;
        }
        return null;
    }
}
