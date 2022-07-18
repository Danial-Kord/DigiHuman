using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class SlideShow : MonoBehaviour
{
    [SerializeField] private UnityEvent onMoveNext;
    [SerializeField] private UnityEvent onMoveLast;
    public Action<int,GameObject> onSelection; //add your actions to this function
    [Header("Nodes and properties")]
    [SerializeField] private float scrollDuration;
    [SerializeField] private float speed;
    [SerializeField] private float distanceBetweenNodes;
    [SerializeField] private Transform centerNodePos;
    public List<GameObject> nodes;
    public GameObject parent; // parent game object of all nodes
    private int index = 0;


    [Header("rotator")] 
    [SerializeField] private GameObject rotator;
    [SerializeField] private bool enableRotator;
    
    private void Awake()
    {
        nodes ??= new List<GameObject>();
        InitialSlideShowPanel();
    }

    private bool playingCoroutine;

    private void InitialSlideShowPanel()
    {
        if(nodes.Count == 0)
            return;
        GameObject node = nodes[0];
        if (enableRotator)
        {
            node = SetRotator(nodes[0]);
            node.transform.parent = parent.transform;
        }

        node.transform.position = centerNodePos.position;
        for (int i = 1; i < nodes.Count; i++)
        {
            node = nodes[i];
            if (enableRotator)
            {
                node = SetRotator(node);
                node.transform.parent = parent.transform;
            }
            Vector3 targetPos = nodes[i-1].transform.position + Vector3.right * distanceBetweenNodes;
            node.transform.position = targetPos; 
        }
    }
    

    public void AddNode(GameObject originalNode)
    {
        GameObject newNode = originalNode;
        if (enableRotator)
            newNode = SetRotator(originalNode);
        newNode.transform.parent = parent.transform;
        if (nodes.Count != 0)
        {
            Vector3 targetPos = nodes[nodes.Count - 1].transform.position + Vector3.right * distanceBetweenNodes;
            newNode.transform.position = targetPos;
        }
        else
        {
            newNode.transform.position = centerNodePos.position;
        }
        nodes.Add(originalNode);

    }

    private GameObject SetRotator(GameObject node)
    {
        GameObject rotate = Instantiate(rotator);
        node.transform.parent = rotate.transform;
        node.transform.localPosition = Vector3.zero;
        return rotate;
    }
    
    public void MoveLast()
    {
        if (!playingCoroutine && index > 0)
        {
            index--;
            playingCoroutine = true;
            onMoveLast.Invoke();
            StartCoroutine(PlayAnimation(true));
        }
    }
    public void MoveNext()
    {
        if (!playingCoroutine && index+1 < nodes.Count)
        {
            index++;
            playingCoroutine = true;
            onMoveNext.Invoke();
            StartCoroutine(PlayAnimation(false));
        }
    }

    public IEnumerator PlayAnimation(bool isMovingRight)
    {
        nodes[index].GetComponentInChildren<QualityData>()?.OnApplyQualityData();
        float move = distanceBetweenNodes;
        if (!isMovingRight)
            move = -move;
        Vector3 initialPos = parent.transform.position;
        Vector3 finalPos = parent.transform.position + Vector3.right * move;
        float timePassed = 0;
        float portion = 0.0f;
        while (true)
        {
            parent.transform.position = Vector3.Slerp(initialPos, finalPos, portion);
            if(portion >= 1)
                break;
            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime * speed;
            portion = (timePassed) / (scrollDuration);
        }
        playingCoroutine = false;
        yield break;
    }

    public void OnSelectItem()
    {
        onSelection(index,nodes[index]);
    }

    public void ClearNodes()
    {
        nodes ??= new List<GameObject>();
        nodes.Clear();
    }

    public void SetNodesArray(GameObject[] newNodes)
    {
        nodes ??= new List<GameObject>();
        for (int i = 0; i < newNodes.Length; i++)
        {
            nodes.Add(newNodes[i]);
        }
    }
}

[CustomEditor(typeof(SlideShow))]
[CanEditMultipleObjects]
public class LookAtPointEditor : Editor
{

    SerializedProperty nodes;
    void OnEnable()
    {
        nodes = serializedObject.FindProperty("nodes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        if (GUILayout.Button("Set All Nodes"))
        {
            ((SlideShow) (target)).ClearNodes();
            Transform p = ((SlideShow) (target)).parent.transform;
            GameObject[] t = new GameObject[p.childCount];
            
            for (int ID = 0; ID < p.childCount; ID++)
            {
                t[ID] = p.GetChild(ID).gameObject;
            }
            ((SlideShow) (target)).SetNodesArray(t);
            
        }
        serializedObject.ApplyModifiedProperties();
    }


}
