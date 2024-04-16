using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private FOV fieldOfView;
    private float speed = 15.0f;
    private float horizontalInput;
    private float verticalInput;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // zero z
        Vector3 aimDir = (mouseWorldPos - transform.position).normalized;
        if (Input.GetKeyDown(KeyCode.Q)){
            fieldOfView.SetAimDirection(fieldOfView.startingAngle + 1);
        }
        fieldOfView.SetOrigin(transform.position);

        // This is where we get player input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

         

        //Debug.Log("horizontal " + horizontalInput + " " + verticalInput);

        // Move the player right/left
        transform.Translate(Vector3.up * Time.deltaTime * speed * verticalInput);
        // Move the player up/down
        transform.Translate(Vector3.right * Time.deltaTime * speed * horizontalInput);

    }
}