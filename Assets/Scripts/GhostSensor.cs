using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class GhostSensor : MonoBehaviour
{
    private ConstraintSource source;

    [SerializeField]
    List<GameObject> sensors;

    [HideInInspector]
    public int XCoord;
    [HideInInspector]
    public int ZCoord;

    public void SetParentConstraint(Transform t)
    {
        source.sourceTransform = t;
        source.weight = 1;
        GetComponent<ParentConstraint>().AddSource(source);
    }

    public void ActivateSensor(int id, bool isActive)
    {
        sensors[id].gameObject.SetActive(isActive);
    }

    public void CheckSensorsToActivate()
    {
        ActivateSensor(0, true);
        ActivateSensor(1, true);
        ActivateSensor(2, true);
        ActivateSensor(3, true);
        WorldArea worldArea = GetComponentInParent<WorldArea>();
        if (worldArea.Area[XCoord - 1, ZCoord].FType == Field.FieldType.path)
        {
            ActivateSensor(0, false);
            worldArea.ghostSensors[XCoord - 1, ZCoord].ActivateSensor(1, false);
        }
        if (worldArea.Area[XCoord + 1, ZCoord].FType == Field.FieldType.path)
        {
            ActivateSensor(1, false);
            worldArea.ghostSensors[XCoord + 1, ZCoord].ActivateSensor(0, false);
        }
        if (worldArea.Area[XCoord, ZCoord - 1].FType == Field.FieldType.path)
        {
            ActivateSensor(2, false);
            worldArea.ghostSensors[XCoord, ZCoord - 1].ActivateSensor(3, false);
        }
        if (worldArea.Area[XCoord, ZCoord + 1].FType == Field.FieldType.path)
        {
            ActivateSensor(3, false);
            worldArea.ghostSensors[XCoord, ZCoord + 1].ActivateSensor(2, false);
        }
    }
    
    
}
