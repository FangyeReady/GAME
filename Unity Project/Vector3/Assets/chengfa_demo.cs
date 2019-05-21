using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
public class chengfa_demo : MonoBehaviour
{


    // Use this for initialization
    void Start()
    {

        string path = Application.dataPath + "/Resources/myjson.json";
        string txt = File.ReadAllText(path);
        JsonData jsonData = JsonMapper.ToObject<JsonData>(txt);


        foreach (var item in jsonData.Keys)
        {
            for (int i = 0; i < jsonData[item].Count; i++)
            {
                //Debug.Log(jsonData[item][i].ToString());
                foreach (var key in jsonData[item][i].Keys)
                {
                    Debug.Log("key:" + key.ToString() + "    data:" + jsonData[item][i][key]);
                }
            }
           
        }
        //Debug.LogError(jsonData.ToJson());

    }

 
}

