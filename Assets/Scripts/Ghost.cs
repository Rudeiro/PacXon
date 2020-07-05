using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField]
    float moveSpeed;

    public Vector3 moveDirection;
    private float nextChangeDirTime;

    private void Start()
    {
        Physics.IgnoreLayerCollision(11, 9);
        
    }



    public void ResetGhost()
    {
        float angle = Random.Range(0.0f, 2.0f);
        moveDirection.x = Mathf.Cos(angle * Mathf.PI);
        moveDirection.z = Mathf.Sin(angle * Mathf.PI);
        while(moveDirection.x < 0.1 || moveDirection.z < 0.1)
        {
            angle = Random.Range(0.0f, 2.0f);
            moveDirection.x = Mathf.Cos(angle * Mathf.PI);
            moveDirection.z = Mathf.Sin(angle * Mathf.PI);
        }
        transform.position = new Vector3(GetComponentInParent<WorldArea>().gameObject.transform.position.x -10, 0.5f, GetComponentInParent<WorldArea>().gameObject.transform.position.z - 10);
    }
    
    void FixedUpdate()
    {
        transform.position += moveDirection * moveSpeed * Time.fixedDeltaTime;
       // if (nextChangeDirTime > 0) nextChangeDirTime -= Time.fixedDeltaTime;
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (nextChangeDirTime <= 0)
        {
            if (collision.transform.gameObject.CompareTag("SouthWall"))
            {
                moveDirection = new Vector3(-moveDirection.x, 0, moveDirection.z);
            }
            if (collision.transform.gameObject.CompareTag("NorthWall"))
            {
                moveDirection = new Vector3(-moveDirection.x, 0, moveDirection.z);
            }
            if (collision.transform.gameObject.CompareTag("EastWall"))
            {
                moveDirection = new Vector3(moveDirection.x, 0, -moveDirection.z);
            }
            if (collision.transform.gameObject.CompareTag("WestWall"))
            {
                moveDirection = new Vector3(moveDirection.x, 0, -moveDirection.z);
            }
        }
        
        if (collision.transform.gameObject.CompareTag("PathArea"))
        {
            collision.transform.gameObject.GetComponentInParent<Field>().InfectPath(0f);
            GetComponentInParent<WorldArea>().PathInfected();
        }
    }

    
}
