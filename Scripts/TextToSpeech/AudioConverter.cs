using System;
using System.Collections;
using System.IO;
using GoogleTextToSpeech.Scripts.Data;
using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics;

namespace GoogleTextToSpeech.Scripts
{
    public class AudioConverter : MonoBehaviour
    {
        private const string Mp3FileName = "audio.mp3";

        public static void SaveTextToMp3(AudioData audioData, string path)
        {
            var bytes = Convert.FromBase64String(audioData.audioContent);
            File.WriteAllBytes(path, bytes);
        }

        public void LoadClipFromMp3(Action<string,AudioClip> onClipLoaded, string path)
        {
            StartCoroutine(LoadClipFromMp3Cor(onClipLoaded, path));

            //LoadClipFromMp3Cor(onClipLoaded);
        }

        private static IEnumerator LoadClipFromMp3Cor(Action<string,AudioClip> onClipLoaded, string path)
        {
            var downloadHandler =
                new DownloadHandlerAudioClip("file://" + path,
                    AudioType.MPEG);
            downloadHandler.compressed = false;

            using var webRequest = new UnityWebRequest("file://" + path,
                "GET",
                downloadHandler, null);

            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode == 200)
            {
                string fileName = GetFileName(path);
                onClipLoaded.Invoke(fileName, downloadHandler.audioClip);
            }
            
            downloadHandler.Dispose();
        }

        private static string GetFileName(string path) 
        {
            string[] tmp = path.Split('/');
            return tmp[tmp.Length-1];
        }


    }


}