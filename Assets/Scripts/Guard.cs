using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    [SerializeField] FOV fieldOfView;
    [SerializeField] GameObject path;
    NavMeshAgent agent;

    public List<Transform> transforms;
    int transformIdx = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        transforms = path.GetComponentsInChildren<Transform>().ToList();
        transforms.RemoveAt(0);
        foreach (Transform t in transforms)
        {
            Debug.Log(t);
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // mouseWorldPos.z = 0f; // zero z
        Vector3 aimDir = (mouseWorldPos - transform.position).normalized;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            fieldOfView.SetAimDirection(fieldOfView.startingAngle + 1);
        }
    }

    void Update()
    {
        agent.SetDestination(transforms[transformIdx].position);
        Vector3 offset = transform.position - transforms[transformIdx].position;
        if (offset.magnitude <= 3)
        {
            transformIdx = (transformIdx + 1) % transforms.Count;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        fieldOfView.SetOrigin(transform.position);
    }
}
