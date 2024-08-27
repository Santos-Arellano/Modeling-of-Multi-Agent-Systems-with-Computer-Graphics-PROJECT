using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
public class ObjectMover : MonoBehaviour
{
    public string apiUrl = "http://localhost:5000/move";
    public GameObject objectToMove;
    public GameObject objectToMove2;

    void Start()
    {

        StartCoroutine(SendPostRequest());

        StartCoroutine(GetMovementData());
    }
    
    IEnumerator SendPostRequest()
    {
        // Crear un objeto JSON como string
        string jsonData = "{\"go\":true}";

        // Convertir el string a un array de bytes
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Crear el UnityWebRequest con el método POST
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");

        // Asignar el cuerpo de la solicitud
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        // Asignar un DownloadHandler para procesar la respuesta
        request.downloadHandler = new DownloadHandlerBuffer();

        // Establecer el tipo de contenido como JSON
        request.SetRequestHeader("Content-Type", "application/json");

        // Enviar la solicitud y esperar la respuesta
        yield return request.SendWebRequest();

        // Manejar errores
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la solicitud: " + request.error);
        }
        else
        {
            // Manejar la respuesta exitosa
            Debug.Log("Respuesta: " + request.downloadHandler.text);
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
                    // Procesar la respuesta de la API
                    var json = www.downloadHandler.text;
                    //Yvar data = JSON.Parse(json);
                    MovementData data = JsonUtility.FromJson<MovementData>(json);
                    foreach(List<int> item in data){
                        Debug.Log(item[0]);

                    }
                    //Vector3 newPosition = new Vector3(data.x, data.y, data.z);
                    // objectToMove.transform.position = newPosition;
                    // Vector3 newPosition2 = new Vector3(data.x*2, data.y*2, data.z*2);
                    // objectToMove2.transform.position = newPosition2;
                }
            }
            yield return new WaitForSeconds(1f); // Consulta cada segundo
        }
    }

    [System.Serializable]
    public class MovementData
    {
        public List<int> Robot1;
        public List<int> Robot2;
    }
}