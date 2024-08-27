using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Posicion_Inicial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
     StartCoroutine(GetRequest("http://127.0.0.1:5000/"));   
    }

    IEnumerator GetRequest(string uri){
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)){
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success){
                Debug.Log(webRequest.error);
            } else {
                // Show results as text
                Debug.Log(webRequest.downloadHandler.text);
                // Or retrieve results as binary data
                byte[] results = webRequest.downloadHandler.data;
            }
    }
}
}