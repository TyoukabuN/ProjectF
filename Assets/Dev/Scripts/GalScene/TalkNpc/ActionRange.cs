using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRange : MonoBehaviour
{
    public float range;

    public List<GameObject> rangeNpcs;
    Collider[] colliders;
    Dictionary<Collider, bool> rangeDic = new Dictionary<Collider, bool>();
    Vector3 startPoint;
    private void Update()
    {
        startPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + range);
        colliders = Physics.OverlapSphere(startPoint, range, LayerMask.GetMask("NPC"));
        
        foreach (var item in colliders)
        {
            item.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            InRangeEvent ire = item.TryGetComponent<InRangeEvent>();
            ire.isIn = true;
            ire.isDo = true;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPoint, range);
    }
}
