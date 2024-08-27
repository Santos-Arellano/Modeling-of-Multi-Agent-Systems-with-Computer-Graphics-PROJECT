using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        // Iniciar la generación de posiciones y la obtención de datos desde la API
        StartCoroutine(AutoMoveObjects());
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

    IEnumerator AutoMoveObjects()
    {
        // (POST)
        using (UnityWebRequest postRequest = UnityWebRequest.PostWwwForm(apiUrl, ""))
        {
            yield return postRequest.SendWebRequest();

            if (postRequest.result == UnityWebRequest.Result.ConnectionError || postRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(postRequest.error);
                yield break;
            }
            else
            {
                Debug.Log("POST realizado exitosamente. Iniciando obtención de posiciones...");
            }
        }

        while (true)
        {
            // (GET)
            using (UnityWebRequest getRequest = UnityWebRequest.Get(apiUrl))
            {
                yield return getRequest.SendWebRequest();

                if (getRequest.result == UnityWebRequest.Result.ConnectionError || getRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(getRequest.error);
                }
                else
                {
                    // Procesar la respuesta de la API
                    var json = getRequest.downloadHandler.text;
                    Debug.Log("JSON recibido: " + json);

                    // Usar JObject para analizar el JSON y verificar el tipo de datos
                    JObject jsonData = JObject.Parse(json);
                    Dictionary<string, float[]> data = new Dictionary<string, float[]>();

                    foreach (var item in jsonData)
                    {
                        if (item.Value is JArray)
                        {
                            // Convertir JArray a float[]
                            float[] positions = item.Value.ToObject<float[]>();
                            data.Add(item.Key, positions);
                        }
                        else
                        {
                            Debug.LogWarning($"Valor inesperado para la clave {item.Key}: {item.Value}");
                        }
                    }

                    // Actualizar posiciones en función de los datos recibidos
                    UpdateObjectPositions(data);
                }
            }

            yield return new WaitForSeconds(0.2f); // Consulta cada x tiempo
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
                    if (item.Value.Length == 2) 
                    {
                        Vector3 newPosition = new Vector3(item.Value[0], 0, item.Value[1]);
                        obj.transform.position = newPosition;
                    }
                    else
                    {
                        Debug.LogWarning($"Data for {item.Key} does not contain exactly 2 elements.");
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