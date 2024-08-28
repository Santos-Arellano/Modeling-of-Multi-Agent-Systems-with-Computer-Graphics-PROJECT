using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;


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
            { "Box20", GameObject.Find("Box20") },
            { "Goal21", GameObject.Find("pila21") },
            { "Goal22", GameObject.Find("pila22") },
            { "Goal23", GameObject.Find("pila23") },
            { "Goal24", GameObject.Find("pila24") },
            { "Goal25", GameObject.Find("pila25") }
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
        using (UnityWebRequest putRequest = new UnityWebRequest(apiUrl, "PUT"))
        {
            // Si deseas enviar datos en el cuerpo de la solicitud, los añades aquí
            string jsonData = ""; // Aquí pones los datos que deseas enviar como JSON
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            putRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            putRequest.downloadHandler = new DownloadHandlerBuffer();

            // Establecer el encabezado de la solicitud como 'application/json'
            putRequest.SetRequestHeader("Content-Type", "application/json");

            yield return putRequest.SendWebRequest();

            if (putRequest.result == UnityWebRequest.Result.ConnectionError || putRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // Manejar el error
                Debug.LogError(putRequest.error);
                yield return new WaitForSeconds(0.5f); // Consulta cada x tiempo
                
            }
            else
            {
                Debug.Log("PUT realizado exitosamente. Iniciando obtención de posiciones...");
            }
        }

        // (POST)
        using (UnityWebRequest postRequest = UnityWebRequest.PostWwwForm(apiUrl, ""))
        {
            yield return postRequest.SendWebRequest();

            if (postRequest.result == UnityWebRequest.Result.ConnectionError || postRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // Debug.LogError(postRequest.error);
                new WaitForSeconds(.5f); // Consulta cada x tiempo
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
                    if (getRequest.responseCode == 404)
                    {
                        break;
                    }
                    else
                    {
                        Debug.LogError(getRequest.error);
                    }
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

            yield return new WaitForSeconds(.5f); // Consulta cada x tiempo
        }
        
        QuitGame();

        // StopGameInEditor();
        // Application.Quit();
    }

    void QuitGame()
        {
            #if UNITY_EDITOR
                    EditorApplication.isPlaying = false; // Stops the game in the Unity Editor
            #else
                    Application.Quit(); // Quits the application in a standalone build
            #endif
        }
    void UpdateObjectPositions(Dictionary<string, float[]> data)
    {
        foreach (var item in objects)
        {
            if (data.ContainsKey(item.Key))
            {
                Vector3 newPosition = new Vector3(data[item.Key][0], 0, data[item.Key][1]);
                item.Value.transform.position = newPosition;
            }
            else
            {
                item.Value.transform.position = new Vector3(100, 0, 0); // Inicia en posición (0, 0, 0)

            }
        }
    }
}