using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    [SerializeField] Transform Target;

    private NavMeshAgent m_Agent;

    private Ray posRay;
    void Start()
    {
        m_Agent = this.gameObject.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MoveToCorsur();
        }
        Debug.DrawRay(posRay.origin, posRay.direction * 100, Color.red);
    }

    private void MoveToCorsur()
    {
        posRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        bool hasHit = Physics.Raycast(posRay, out hitInfo);

        if (hasHit)
        {
            m_Agent.SetDestination(hitInfo.point);
        }
    }

    
}
