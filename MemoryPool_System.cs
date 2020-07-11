using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class MemoryPool_System : MonoBehaviour
{
    Dictionary<GameObject, List<GameObject>> ObjectList = new Dictionary<GameObject, List<GameObject>>(); //������ ���� ������

    ////////////////////////////////////////////////////////
    ///////////////     Ÿ�Ժ� �ҷ�����      ////////////////
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
                AllObject.Add(GetList[j]); //��� ������Ʈ�� �߰�
            }
        }

        //������Ʈ ������ Ȯ�� �� ���� ��� ����Ʈ���� ����
        for (var i = AllObject.Count - 1; i > -1; i--)
        {
            if (AllObject[i] == null)
                AllObject.RemoveAt(i);
        }

        //����Ʈ �ߺ� ����
        AllObject = AllObject.Distinct().ToList();

        return AllObject;
    }
    public List<GameObject> GetAllObject() //�� �޸�Ǯ �ý��ۿ��� �����ϴ� ��� ������Ʈ�� �����ɴϴ�. ���� ���� ���� ������Ʈ�� ����Ʈ���� �����մϴ�.
    {
        List<GameObject> AllObject = new List<GameObject>();
        for (int i = 0; i < ObjectList.Count; i++)
        {
            List<GameObject> GetList = ObjectList[ObjectList.Keys.ToArray()[i]];

            for (int j = 0; j < GetList.Count; j++)
            {
                AllObject.Add(GetList[j]); //��� ������Ʈ�� �߰�
            }
        }

        //������Ʈ ������ Ȯ�� �� ���� ��� ����Ʈ���� ����
        for (var i = AllObject.Count - 1; i > -1; i--)
        {
            if (AllObject[i] == null)
                AllObject.RemoveAt(i);
        }

        //����Ʈ �ߺ� ����
        AllObject = AllObject.Distinct().ToList();

        return AllObject;
    }
    public void RemoveTargetObject(GameObject GameObject) //�� �޸�Ǯ���� �ش� ������Ʈ�� ����ϴ� ��� ������Ʈ�� ���� �մϴ�.
    {
        foreach (var item in GetTargetObject(GameObject))
        {
            Destroy(item);
        }
    }
    public void RemoveAllObject() //�� �޸�Ǯ�� ���� �ϴ� ��� ������Ʈ�� ���� �մϴ�.
    {
        foreach (var item in GetAllObject())
        {
            Destroy(item);
        }
    }

    /////////////////////////////////////////////////////////////
    ///////////////     ������Ʈ ȣ�� �ý���      ////////////////
    /////////////////////////////////////////////////////////////
    #region
    //Ÿ�Ժ� �ҷ����� ó��
    GameObject CallOObject_Active(GameObject TargetGameObject, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (ObjectList.ContainsKey(TargetGameObject) == false) //����Ʈ ��ü�� ���� ���
        {
            return CreateNewObjectList(TargetGameObject, position, rotation, parent); //����Ʈ�� ���� ���� �ϰ� ������Ʈ�� ���� �����ؼ� �߰�
        }
        else //���� ���
        {
            GameObject GetObject = SearchListActive(TargetGameObject);
            if (GetObject == null) //����Ʈ�� �ִµ� ������Ʈ�� ���� ���
            {
                return AddTargetDictionalyList(TargetGameObject, position, rotation, parent); //�ش� ����Ʈ�� ���ο�� �߰� �� �ڿ� ���� ����
            }
            else //����Ʈ�� �ְ� ��Ȱ�� ������Ʈ�� ���� ���
            {
                //������Ʈ ��ġ, ȸ���� ���� �� �ѱ�
                GetObject.transform.position = position;
                GetObject.transform.rotation = rotation;
                GetObject.transform.parent = parent;
                GetObject.SetActive(true);
                return GetObject;
            }
        }
    }
    GameObject CreateNewObjectList(GameObject CreateTargetObject, Vector3 position, Quaternion rotation, Transform parent) //���ο� ��ųʸ� ����Ʈ ���� �߰�
    {
        //���ο� ����Ʈ ����
        List<GameObject> TargetObjectList = new List<GameObject>();
        GameObject NewObject = Instantiate(CreateTargetObject, position, rotation, parent);
        //NewObject.SetActive(false); //������Ʈ ����
        TargetObjectList.Add(NewObject); //����Ʈ �߰� �ѵ�
        ObjectList.Add(CreateTargetObject, TargetObjectList); //��ųʸ� ���� �߰� ��
        return NewObject; //���õ� ������Ʈ �ٷ� ������
    }
    GameObject AddTargetDictionalyList(GameObject OriginalObject, Vector3 position, Quaternion rotation, Transform parent) //��ųʸ� ������ �̹� �ִ� ���� ���ο� ������Ʈ �߰�
    {
        GameObject NewObject = Instantiate(OriginalObject, position, rotation, parent); //���ο� ������Ʈ ����
        ObjectList[OriginalObject].Add(NewObject); //����Ʈ�� �� ������ �߰�
        return NewObject; //���õ� ������Ʈ �ٷ� ������
    }
    GameObject SearchListActive(GameObject TargetGameObject) //����Ʈ �ȿ� ���� �ִ� ������Ʈ �ִ��� Ȯ��
    {
        for (int i = 0; i < ObjectList[TargetGameObject].Count; i++)
        {
            if (ObjectList[TargetGameObject][i] == null)
            {
                ObjectList[TargetGameObject].Remove(ObjectList[TargetGameObject][i]); //���࿡ ����� ���� ���� ��� ����Ʈ���� ����
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

