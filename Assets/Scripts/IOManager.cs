using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

namespace BubbleShooter
{
    public class IOManager : MonoBehaviour
    {
        public static IOManager instance;

        private void Awake()
        {
            instance = this;
        }

        public void LoadJsonLocal(string filePath, string jsonType)
        {
            string json = null;
            Debug.Log("IO: commencing IO operation read, type " + jsonType);

            filePath = GetPath(filePath);
            Debug.Log("File path " + filePath);
            if (File.Exists(filePath))
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    json = r.ReadToEnd();
                    while (json.Length == 0)
                    {
                        json = r.ReadToEnd();
                        Debug.Log("IO: json length 0, waiting for file...");
                    }
                    Debug.Log("IO: json length not 0 continue");
                    switch (jsonType)
                    {
                        case "balls":
                            DataObject.instance.loadedBalls = new List<Ball>();
                            DataObject.instance.loadedBalls = JsonConvert.DeserializeObject<List<Ball>>(json);
                            break;
                        default:
                            break;
                    }

                }
            }
            else
            {
                Debug.Log("File not found!");
            }

        }
        private string GetPath(string filePath)
        {
            return Path.Combine(Application.persistentDataPath, filePath);
        }

        private string GetStreamingPath(string filePath)
        {
            return Path.Combine(Application.streamingAssetsPath, filePath);
        }
    }

}

