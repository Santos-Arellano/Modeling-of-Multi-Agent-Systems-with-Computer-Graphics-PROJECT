using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    void Start()
    {
        // Definir posiciones fijas para cada objeto
        DefineInitialPositions();

        // Inicializar posiciones de los objetos
        InitializeObjects();
    }

    void DefineInitialPositions()
    {
        objects = new Dictionary<string, GameObject>
        {
            { "agent1", GameObject.Find("agent1") },
            { "agent2", GameObject.Find("agent2") },
            { "agent3", GameObject.Find("agent3") },
            { "agent4", GameObject.Find("agent4") },
            { "agent5", GameObject.Find("agent5") },
            { "box1", GameObject.Find("box1") },
            { "box2", GameObject.Find("box2") },
            { "box3", GameObject.Find("box3") },
            { "box4", GameObject.Find("box4") },
            { "box5", GameObject.Find("box5") },
            { "box6", GameObject.Find("box6") },
            { "box7", GameObject.Find("box7") },
            { "box8", GameObject.Find("box8") },
            { "box9", GameObject.Find("box9") },
            { "box10", GameObject.Find("box10") }
        };
    }

    void InitializeObjects()
    {
        // Asignar posiciones iniciales fijas
        foreach (var item in objects)
        {
            if (item.Value != null)
            {
                item.Value.transform.position = new Vector3(0, 0, 0); // Inicia en posición (0, 0, 0)
            }
            else
            {
                Debug.LogWarning("GameObject not found: " + item.Key);
            }
        }
    }

    public void UpdateObjectPositions(Dictionary<string, float[]> data)
    {
        // Actualizar las posiciones de los objetos según los datos recibidos del API
        foreach (var obj in objects.Keys)
        {
            if (objects[obj] != null)
            {
                Vector3 newPosition = new Vector3(data[obj][0], data[obj][1], data[obj][2]);
                objects[obj].transform.position = newPosition;
                Debug.Log($"{obj} moved to {newPosition}");
            }
            else
            {
                Debug.LogWarning("GameObject not found: " + obj);
            }
        }
    }
}


