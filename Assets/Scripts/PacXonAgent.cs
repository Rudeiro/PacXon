using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class PacXonAgent : Agent
{
    private enum MoveDir
    {
        up,
        down,
        left,
        right
    }
    [SerializeField]
    float moveSpeed = 5;
    [SerializeField]
    WorldArea worldArea;
    
    new private Rigidbody rigidbody;
    private Vector2 areaSize;
    private Vector3 moveTarget;
    private Vector3 position;
    private Vector3 globalPosition;
    private bool requestDecision = true;
    private MoveDir moveDir;
    private Field.FieldType currentPosFieldType;
    private bool canSwitchDir = true;
    private bool isDrawing = false;

    public bool isPathInfected;
    public EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        base.Initialize();
        rigidbody = GetComponent<Rigidbody>();
        worldArea = GetComponentInParent<WorldArea>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    public override void OnActionReceived(float[] vectorAction)
    {

        // Convert the first action to forward movement
        float forwardAmount = 0;
        // Convert the second action to turning left or right
        float turnAmount = 0f;
        float sideAmount = 0f;
        //Debug.LogError(vectorAction[0]);
        //moving up, down, left, right
        if (vectorAction[0] == 1f && canSwitchDir && position.x < -1 + globalPosition.x && (!isDrawing || isDrawing && moveDir != MoveDir.down))
        {
            moveDir = MoveDir.up;
            MoveUp();            
        }
        else if (vectorAction[0] == 2f && canSwitchDir && position.x > -areaSize.x +2 + globalPosition.x && (!isDrawing || isDrawing && moveDir != MoveDir.up))
        {
            moveDir = MoveDir.down;
            MoveDown();
        }
        else if (vectorAction[0] == 3f && canSwitchDir && position.z < -1 + globalPosition.z && (!isDrawing || isDrawing && moveDir != MoveDir.right))
        {
            moveDir = MoveDir.left;
            MoveLeft();
        }
        else if (vectorAction[0] == 4f && canSwitchDir && position.z > -areaSize.y + 2 + globalPosition.z && (!isDrawing || isDrawing && moveDir != MoveDir.left))
        {
            moveDir = MoveDir.right;
            MoveRight();
        }
        //Debug.LogError(sideAmount);
        
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0f; // moving forward/backward
       
        // moving sideways
        if (Input.GetKey(KeyCode.W))
        {
            // move up
            actionsOut[0] = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            // move down
            actionsOut[0] = 2f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // move left
            actionsOut[0] = 3f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // move right
            actionsOut[0] = 4f;
        }     
    }

    public override void OnEpisodeBegin()
    {
        globalPosition = new Vector3((int)worldArea.transform.position.x, (int)worldArea.transform.position.y, (int)worldArea.transform.position.z);
        position = new Vector3((int)worldArea.transform.position.x -1, 0.5f, (int)worldArea.transform.position.z - 1);
        worldArea.ResetWorldArea();
        requestDecision = true;        
        isDrawing = false;
        moveTarget = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        //sensor.AddObservation(worldArea.targetsCount);
        switch (moveDir)
        {
            case MoveDir.up:
                sensor.AddObservation(0);
                break;
            case MoveDir.down:
                sensor.AddObservation(1);
                break;
            case MoveDir.left:
                sensor.AddObservation(2);
                break;
            case MoveDir.right:
                sensor.AddObservation(3);
                break;
            default:
                break;
        }
        switch (currentPosFieldType)
        {
            case Field.FieldType.empty:
                sensor.AddObservation(0);
                break;
            case Field.FieldType.path:
                sensor.AddObservation(1);
                break;
            case Field.FieldType.player:
                sensor.AddObservation(2);
                break;
            default:
                break;
        }
        sensor.AddObservation(transform.position);
        sensor.AddObservation(worldArea.Ghosts[0].transform.position);
        sensor.AddObservation(Vector3.Distance(worldArea.Ghosts[0].transform.position, transform.position));
        sensor.AddObservation(worldArea.Ghosts[0].moveDirection);
        sensor.AddObservation(worldArea.Ghosts[1].transform.position);
        sensor.AddObservation(Vector3.Distance(worldArea.Ghosts[1].transform.position, transform.position));
        sensor.AddObservation(worldArea.Ghosts[1].moveDirection);
        sensor.AddObservation(worldArea.fieldsOwned);
        sensor.AddObservation(isDrawing);
        sensor.AddObservation(isPathInfected);
    }

    private void Start()
    {
        worldArea = GetComponentInParent<WorldArea>();
        Physics.IgnoreLayerCollision(13, 8);
        Physics.IgnoreLayerCollision(13, 9);
        Physics.IgnoreLayerCollision(13, 10);
        Physics.IgnoreLayerCollision(13, 11);
        areaSize = worldArea.AreaSize;
    }

    private void OnCollisionEnter(Collision collision)
    {       

    }  
    private void MoveDown()
    {
        requestDecision = false;
        /*transform.position = new Vector3(transform.position.x,
                                         transform.position.y,
                                         transform.position.z - 1);*/
        moveTarget = new Vector3(-1, 0, 0);

    }

    private void MoveUp()
    {
        requestDecision = false;
        moveTarget = new Vector3(1, 0, 0);
    }

    private void MoveLeft()
    {

        moveTarget = new Vector3(0, 0, 1);
        requestDecision = false;
    }
    private void MoveRight()
    {

        moveTarget = new Vector3(0, 0, -1);

        requestDecision = false;
    }

    private void ChangePositionOverTime()
    {
        float step = moveSpeed * Time.fixedDeltaTime;
        transform.position += moveTarget * step;

        switch (moveDir)
        {
            case MoveDir.up:
                if(transform.position.x >= position.x + 1)
                {
                    transform.position = new Vector3(position.x + 1, position.y, position.z);
                    
                    position = new Vector3(position.x + 1, position.y, position.z);
                    worldArea.Area[-((int)position.x - (int)globalPosition.x), -((int)position.z - (int)globalPosition.z)].ChangeFieldType(Field.FieldType.path);
                    requestDecision = true;
                }
                break;
            case MoveDir.down:
                if (transform.position.x <= position.x -1)
                {
                    requestDecision = true;
                    transform.position = new Vector3(position.x - 1, position.y, position.z );
                   
                    position = new Vector3(position.x - 1, position.y, position.z);
                    worldArea.Area[-((int)position.x - (int)globalPosition.x), -((int)position.z - (int)globalPosition.z)].ChangeFieldType(Field.FieldType.path);
                }
                break;
            case MoveDir.left:
                if (transform.position.z >= position.z + 1)
                {
                    requestDecision = true;
                    transform.position = new Vector3(position.x, position.y, position.z+1);
                   
                    position = new Vector3(position.x, position.y, position.z + 1);
                    worldArea.Area[-((int)position.x - (int)globalPosition.x), -((int)position.z - (int)globalPosition.z)].ChangeFieldType(Field.FieldType.path);
                }
                break;
            case MoveDir.right:
                if (transform.position.z <= position.z -1)
                {
                    requestDecision = true;
                    transform.position = new Vector3(position.x, position.y, position.z - 1);
                    
                    position = new Vector3(position.x, position.y, position.z - 1);
                    worldArea.Area[-((int)position.x - (int)globalPosition.x), -((int)position.z - (int)globalPosition.z)].ChangeFieldType(Field.FieldType.path);
                }
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        ChangePositionOverTime();
        currentPosFieldType = worldArea.Area[-((int)position.x - (int)globalPosition.x), -((int)position.z - (int)globalPosition.z)].FType;
        if (requestDecision)
        {            
            
            //Debug.LogError(currentPosFieldType);
            if(!isDrawing && currentPosFieldType == Field.FieldType.path)
            {
                isDrawing = true;
            }
            else if(isDrawing && currentPosFieldType == Field.FieldType.player)
            {
                worldArea.CalculateClosedArea();
                isPathInfected = false;
                isDrawing = false;
            }
            if(currentPosFieldType == Field.FieldType.player) moveTarget = Vector3.zero;
            RequestDecision();
            
        }

        if(currentPosFieldType == Field.FieldType.player)
        {
            AddReward(-0.005f);
        }

        if (currentPosFieldType == Field.FieldType.path)
        {
            AddReward(0.005f);
        }

        if (currentPosFieldType == Field.FieldType.path && worldArea.Area[-((int)position.x - (int)globalPosition.x), -((int)position.z - (int)globalPosition.z)].IsInfected)
        {
            AddReward(-1f);
            EndEpisode();
        }

        if (Vector3.Distance(position, transform.position) < 0.001f)
        {
            canSwitchDir = true;
        }
        else
        {
            canSwitchDir = false;
        }
    }
}
