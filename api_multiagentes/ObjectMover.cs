using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObjectMover : MonoBehaviour
{
    public string apiUrl = "http://127.0.0.1:5000/move";
    public Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    void Start()
    {
        // Definir posiciones fijas para cada objeto
        DefineInitialPositions();

        // Inicializar posiciones de los objetos
        InitializeObjects();

        // Iniciar la obtención de datos desde la API
        StartCoroutine(GetMovementData());
    }

    void DefineInitialPositions()
    {
        objects = new Dictionary<string, GameObject>
        {
            { "Robot1", GameObject.Find("Robot1") },
            { "Robot2", GameObject.Find("Robot2") },
            { "Robot3", GameObject.Find("Robot3") },
            { "Robot4", GameObject.Find("Robot4") },
            { "Robot5", GameObject.Find("Robot5") },
            { "Box6", GameObject.Find("Box6") },
            { "Box7", GameObject.Find("Box7") },
            { "Box8", GameObject.Find("Box8") },
            { "Box9", GameObject.Find("Box9") },
            { "Box10", GameObject.Find("Box10") },
            { "Box11", GameObject.Find("Box11") },
            { "Box12", GameObject.Find("Box12") },
            { "Box13", GameObject.Find("Box13") },
            { "Box14", GameObject.Find("Box14") },
            { "Box15", GameObject.Find("Box15") },
            { "Box16", GameObject.Find("Box16") },
            { "Box17", GameObject.Find("Box17") },
            { "Box18", GameObject.Find("Box18") },
            { "Box19", GameObject.Find("Box19") },
            { "Box20", GameObject.Find("Box20") }
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

    IEnumerator GetMovementData()
    {
        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    //===============================0
                    //aiuda, no apunta al objeto "Robot2", etc
                    // Procesar la respuesta de la API
                    //Para pruebas, hacer POST y GET a la API desde Postman
                    var json = www.downloadHandler.text;
                    Dictionary<string, float[]> data = JsonUtility.FromJson<Dictionary<string, float[]>>(json);
                    Debug.Log(json);
                    data.TryGetValue("Robot2", out float[] values);
                    Debug.Log(values);
                    // Actualizar posiciones en función de los datos recibidos
                    // UpdateObjectPositions(data);
                }
            }
            yield return new WaitForSeconds(1f); // Consulta cada segundo
        }
    }

    void UpdateObjectPositions(Dictionary<string, float[]> data)
    {
        foreach (var item in data)
        {
            if (objects.ContainsKey(item.Key))
            {
                GameObject obj = objects[item.Key];
                if (obj != null)
                {
                    if (item.Value.Length == 3)
                    {
                        // Convertir el float[] a Vector3
                        Vector3 newPosition = new Vector3(item.Value[0], item.Value[1], item.Value[2]);
                        obj.transform.position = newPosition;
                    }
                    else
                    {
                        Debug.LogWarning($"Data for {item.Key} does not contain exactly 3 elements.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Object with key {item.Key} found in dictionary but the GameObject is null.");
                }
            }
            else
            {
                Debug.LogWarning($"Key {item.Key} not found in the objects dictionary.");
            }
        }
    }
}