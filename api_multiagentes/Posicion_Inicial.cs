using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Posicion_Inicial : MonoBehaviour
{
    public void SendPositions(List<GameObject> objects)
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogError("Objects list is empty. Nothing to send.");
            return;
        }

        string json = "{ \"positions\": [";
        for (int i = 0; i < objects.Count; i++)
        {
            Vector3 pos = objects[i].transform.position;
            string name = objects[i].name;

            // Cast coordinates to integers
            int x = Mathf.RoundToInt(pos.x);
            int z = Mathf.RoundToInt(pos.z);

            json += "{ \"x\": " + x + ", \"z\": " + z + ", \"name\": \"" + name + "\"}";
            if (i < objects.Count - 1)
            {
                json += ",";
            }
        }
        json += "]}";

        Debug.Log("Sending JSON: " + json);

        StartCoroutine(PostRequest("http://127.0.0.1:5000/", json));
    }

    IEnumerator PostRequest(string uri, string json)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest webRequest = new UnityWebRequest(uri, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            Debug.Log("Form upload complete! Response: " + webRequest.downloadHandler.text);
        }
    }
}
