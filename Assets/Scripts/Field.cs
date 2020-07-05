using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    public enum FieldType
    {
        empty,
        path,
        player,
        border
    }

    [SerializeField]
    Material playerFieldMat;
    [SerializeField]
    Material emptyFieldMat;
    [SerializeField]
    Material pathFieldMat;
    [SerializeField]
    Material infectedFieldMat;
    [SerializeField]
    GameObject ground;
    [SerializeField]
    BoxCollider playerFieldCollider;
    [SerializeField]
    BoxCollider borderCollider;
    [SerializeField]
    BoxCollider emptyFieldCollider;
    [SerializeField]
    BoxCollider pathFieldCollider;
    [SerializeField]
    BoxCollider NorthWall;
    [SerializeField]
    BoxCollider SouthWall;
    [SerializeField]
    BoxCollider WestWall;
    [SerializeField]
    BoxCollider EastWall;
    [SerializeField]
    GameObject walls;
    [SerializeField]
    float infectionTime;

    [HideInInspector]
    public int XCoord;
    [HideInInspector]
    public int ZCoord;

    public bool hasGhost;
    public FieldType fType;
    public int Group;
    

    private bool isDuringInfection;
    private float timeToInfect;
    private bool isInfected;

    public FieldType FType { get { return fType; } }
    public bool IsInfected { get { return isInfected; } }
   
    void Start()
    {
        
    }
    
    void FixedUpdate()
    {
        if(isDuringInfection)
        {
            if(timeToInfect <= 0)
            {
                isInfected = true;
                ground.GetComponent<MeshRenderer>().material = infectedFieldMat;
                InfectNeighbours();
            }
            else
            {
                timeToInfect -= Time.fixedDeltaTime;
            }
        }
    }

    public void ChangeFieldType(FieldType type)
    {
        switch (type)
        {
            case FieldType.player:
                ChangeToPlayerField();
                break;
            case FieldType.path:
                ChangeToPathField();
                break;
            case FieldType.empty:
                ChangeToEmptyField();
                break;
            case FieldType.border:
                ChangeToBorderField();
                break;
            default:
                break;
        }
    }

    private void ChangeToPlayerField()
    {
        playerFieldCollider.enabled = true;
        emptyFieldCollider.enabled = false;
        ground.GetComponent<MeshRenderer>().material = playerFieldMat;
        pathFieldCollider.enabled = false;
        fType = FieldType.player;
        walls.SetActive(true);
        isDuringInfection = false;
        isInfected = false;
        transform.gameObject.tag = "PlayerArea";
        //ActivateWalls(FieldType.player);

    }

    private void ChangeToPathField()
    {
        if (fType == FieldType.path)
        {
            GetComponentInParent<WorldArea>().EndEpisode();
        }
        else if (fType != FieldType.player)
        {
            ground.GetComponent<MeshRenderer>().material = pathFieldMat;
            emptyFieldCollider.enabled = false;
            fType = FieldType.path;
            walls.SetActive(true);
            ActivateWalls(FieldType.path);
            pathFieldCollider.enabled = true;
            transform.gameObject.tag = "PathArea";
            
        }
        

    }

    private void ChangeToEmptyField()
    {
        ground.GetComponent<MeshRenderer>().material = emptyFieldMat;
        emptyFieldCollider.enabled = true;
        playerFieldCollider.enabled = false;
        fType = FieldType.empty;
        walls.SetActive(false);
        pathFieldCollider.enabled = false;
        isDuringInfection = false;
        isInfected = false;
        transform.gameObject.tag = "EmptyArea";
    }

    private void ChangeToBorderField()
    {
        borderCollider.enabled = true;
        emptyFieldCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Ghost"))
        {
            hasGhost = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ghost"))
        {
            hasGhost = false; 
        }
    }

    public void InfectPath(float time)
    {
        if(!isDuringInfection)
        {
            timeToInfect = time;
            isDuringInfection = true;
        }
    }

    private void InfectNeighbours()
    {
        WorldArea worldArea = GetComponentInParent<WorldArea>();

        if(worldArea.Area[XCoord-1, ZCoord].FType == FieldType.path)
        {
            worldArea.Area[XCoord - 1, ZCoord].InfectPath(infectionTime);
        }
        if (worldArea.Area[XCoord + 1, ZCoord].FType == FieldType.path)
        {
            worldArea.Area[XCoord + 1, ZCoord].InfectPath(infectionTime);
        }
        if (worldArea.Area[XCoord, ZCoord - 1].FType == FieldType.path)
        {
            worldArea.Area[XCoord, ZCoord - 1].InfectPath(infectionTime);
        }
        if (worldArea.Area[XCoord, ZCoord + 1].FType == FieldType.path)
        {
            worldArea.Area[XCoord , ZCoord + 1].InfectPath(infectionTime);
        }
    }

    public void ActivateWalls(FieldType type)
    {
        WorldArea worldArea = GetComponentInParent<WorldArea>();
        NorthWall.enabled = true;
        SouthWall.enabled = true;
        WestWall.enabled = true;
        EastWall.enabled = true;
        if (worldArea.Area[XCoord - 1, ZCoord].FType == type)
        {
            NorthWall.enabled = false;
        }
        if (worldArea.Area[XCoord + 1, ZCoord].FType == type)
        {
            SouthWall.enabled = false;
        }
        if (worldArea.Area[XCoord, ZCoord - 1].FType == type)
        {
            EastWall.enabled = false;
        }
        if (worldArea.Area[XCoord, ZCoord + 1].FType == type)
        {
            WestWall.enabled = false;
        }
    }
}
