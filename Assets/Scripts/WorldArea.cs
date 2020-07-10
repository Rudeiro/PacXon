using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldArea : MonoBehaviour
{
    [SerializeField]
    PacXonAgent pacXon;
    [SerializeField]
    Vector2 areaSize;
    [SerializeField]
    Field fieldPrefab;
    [SerializeField]
    GhostSensor ghostSensorPrefab;
    [SerializeField]
    List<Ghost> ghosts;
    private Field[,] area;
    public GhostSensor[,] ghostSensors;
    private bool[,] visited;

    public Vector2 AreaSize { get { return areaSize; } }
    public Field[,] Area { get { return area; } }
    public GhostSensor[,] GhostSensors { get { return ghostSensors; } }
    public List<Ghost> Ghosts { get { return ghosts; } }

    public int numberOfFields;
    public int fieldsOwned;

    void Start()
    {
        Field field;
        GhostSensor ghostSensor;
        area = new Field[(int)areaSize.x, (int)areaSize.y];
        ghostSensors = new GhostSensor[(int)areaSize.x, (int)areaSize.y];
        visited = new bool[(int)areaSize.x, (int)areaSize.y];
        for (int i = 0; i < (int)areaSize.x; i++)
        {
            for(int j = 0; j < (int)areaSize.y; j++)
            {
                field = area[i,j] = Instantiate(fieldPrefab, new Vector3(transform.position.x -i, 0,transform.position.z -j), Quaternion.identity);
                field.transform.parent = transform;
                field.XCoord = i;
                field.ZCoord = j;
            }
        }
        for (int i = 1; i < (int)areaSize.x - 1 ; i++)
        {
            for (int j = 1; j < (int)areaSize.y - 1; j++)
            {
                ghostSensor = ghostSensors[i, j] = Instantiate(ghostSensorPrefab, new Vector3(transform.position.x - i, 0, transform.position.z - j), Quaternion.identity);
                ghostSensor.transform.parent = pacXon.transform;
                ghostSensor.XCoord = i;
                ghostSensor.ZCoord = j;
                ghostSensor.SetParentConstraint(area[i,j].gameObject.transform);
            }
        }

        numberOfFields = ((int)areaSize.x -2) * ((int)areaSize.y - 2);
    }
    
    public void ResetWorldArea()
    {
        //setting borders
        for (int i = 0; i < areaSize.y; i++)
        {
            area[0, i].ChangeFieldType(Field.FieldType.border);
            area[(int)areaSize.x - 1, i].ChangeFieldType(Field.FieldType.border);
        }
        for (int i = 0; i < areaSize.x; i++)
        {
            area[i, 0].ChangeFieldType(Field.FieldType.border);
            area[i, (int)areaSize.y - 1].ChangeFieldType(Field.FieldType.border);
        }

        //setting initial player area
        for (int i = 1; i < areaSize.y - 1; i++)
        {
            area[1, i].ChangeFieldType(Field.FieldType.player);
            area[(int)areaSize.x - 2, i].ChangeFieldType(Field.FieldType.player);
        }
        for (int i = 1; i < areaSize.x - 1; i++)
        {
            area[i, 1].ChangeFieldType(Field.FieldType.player);
            area[i, (int)areaSize.y - 2].ChangeFieldType(Field.FieldType.player);
        }

        //clearing the rest of the area
        for(int i = 2; i < areaSize.x - 2; i++)
        {
            for(int j = 2; j < areaSize.y - 2; j++)
            {
                area[i, j].ChangeFieldType(Field.FieldType.empty);
            }
        }
        //area[10, 13].hasGhost = true;
        //AssignFieldGroups();
        pacXon.transform.position = new Vector3(transform.position.x -1, 0.5f, transform.position.z - 1);
        fieldsOwned = 0;
        foreach (var ghost in ghosts)
        {
            ghost.ResetGhost();
        }
        for (int i = 1; i < (int)areaSize.x - 1; i++)
        {
            for (int j = 1; j < (int)areaSize.y - 1; j++)
            {
                if (area[i, j].FType == Field.FieldType.player)
                {
                    area[i, j].ActivateWalls(Field.FieldType.player);
                }
            }
        }
    }

    public void CalculateClosedArea()
    {
        AssignFieldGroups();
    }

    private void AssignFieldGroups()
    {
        
        for (int i = 1; i < (int)areaSize.x - 1; i++)
        {
            for (int j = 1; j < (int)areaSize.y - 1; j++)
            {
                visited[i, j] = false;
                area[i, j].Group = 0;
            }
        }
        int group = 1;
        for (int i = 2; i < (int)areaSize.x - 2; i++)
        {
            for (int j = 2; j < (int)areaSize.y - 2; j++)
            {
                if (area[i, j].FType == Field.FieldType.empty && area[i, j].Group == 0)
                {
                    if(CalculateGroup(i, j, group))
                    {
                        OwnGroup(group);
                    }
                    group++;
                }
            }
        }
        for (int i = 2; i < (int)areaSize.x - 2; i++)
        {
            for (int j = 2; j < (int)areaSize.y - 2; j++)
            {
                if (area[i, j].FType == Field.FieldType.path)
                {
                    area[i, j].ChangeFieldType(Field.FieldType.player);
                    pacXon.AddReward(0.5f);
                    fieldsOwned++;
                }
            }
        }
        for (int i = 1; i < (int)areaSize.x - 1; i++)
        {
            for (int j = 1; j < (int)areaSize.y - 1; j++)
            {
                if (area[i, j].FType == Field.FieldType.player)
                {
                    area[i, j].ActivateWalls(Field.FieldType.player);
                }
            }
        }
        if (1.0f * fieldsOwned / numberOfFields >= pacXon.m_ResetParams.GetWithDefault("percent_needed", 0.5f))
        {
            pacXon.AddReward(10f);
            pacXon.EndEpisode();
        }

    }

    private bool CalculateGroup(int i, int j, int group)
    {
        Queue<Pair<int, int>> q = new Queue<Pair<int, int>>();
        q.Enqueue(new Pair<int, int>(i, j));
        Pair<int, int> p;
        bool shouldOwn = true;
        while(q.Count > 0)
        {
            p = q.Peek();
            q.Dequeue();
            area[p.First, p.Second].Group = group;
            if (area[p.First, p.Second].hasGhost) shouldOwn = false;
            visited[p.First, p.Second] = true;
            if(area[p.First-1, p.Second].FType == Field.FieldType.empty && !visited[p.First - 1, p.Second])
            {
                q.Enqueue(new Pair<int, int>(p.First-1, p.Second) );
                visited[p.First - 1, p.Second] = true;
            }
            if (area[p.First + 1, p.Second].FType == Field.FieldType.empty && !visited[p.First + 1, p.Second])
            {
                q.Enqueue(new Pair<int, int>(p.First + 1, p.Second));
                visited[p.First + 1, p.Second] = true;
            }
            if (area[p.First, p.Second - 1].FType == Field.FieldType.empty && !visited[p.First, p.Second - 1])
            {
                q.Enqueue(new Pair<int, int>(p.First, p.Second - 1));
                visited[p.First, p.Second - 1] = true;
            }
            if (area[p.First, p.Second + 1].FType == Field.FieldType.empty && !visited[p.First , p.Second + 1])
            {
                q.Enqueue(new Pair<int, int>(p.First, p.Second + 1));
                visited[p.First, p.Second + 1] = true;
            }
            
        }
        return shouldOwn;
    }

    private void OwnGroup(int group)
    {
        for (int i = 2; i < (int)areaSize.x - 2; i++)
        {
            for (int j = 2; j < (int)areaSize.y - 2; j++)
            {
                if(area[i, j].Group == group)
                {
                    area[i, j].ChangeFieldType(Field.FieldType.player);
                    pacXon.AddReward(0.5f);
                    fieldsOwned++;
                }
            }
        }        
    }

    public void EndEpisode()
    {
        pacXon.AddReward(-10f);
        pacXon.EndEpisode();
    }

    public void PathInfected()
    {
        pacXon.isPathInfected = true;
    }
}
