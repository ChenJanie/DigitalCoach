using System;
using System.Collections.Generic;
using GoogleTextToSpeech.Scripts.Data;
using TMPro;
using UnityEngine;
using System.IO;
using Input = GoogleTextToSpeech.Scripts.Data.Input;
using System.IO.Enumeration;

namespace GoogleTextToSpeech.Scripts
{
    public class TextToSpeech : MonoBehaviour
    {
        [SerializeField] private string apiKey = "AIzaSyCNxfuCuRSIciCtKd2aBKwqBuzKwl9kQlg";
        [SerializeField] private VoiceScriptableObject operatorVoice;
        [SerializeField] private VoiceScriptableObject signallerVoice;
        [SerializeField] private VoiceScriptableObject hintVoice;
        [SerializeField] private PerformanceManager performanceManager;

        private Action<string> _actionRequestReceived;
        private Action<BadRequestData> _errorReceived;
        private Action<string,AudioClip> _audioClipReceived;

        private RequestService _requestService;
        private static AudioConverter _audioConverter;

        private static List<Pair<string, AudioClip>> FN_ACPairs=new List<Pair<string, AudioClip>>();

        public void GetSpeechAudioFromGoogle(string textToConvert, VoiceScriptableObject voice, string fileName, Action<string, AudioClip> audioClipReceived,  Action<BadRequestData> errorReceived)
        {
            _actionRequestReceived = (requestData => RequestReceived(requestData,fileName, audioClipReceived));

            if (_requestService == null)
                _requestService = gameObject.AddComponent<RequestService>();

            if (_audioConverter == null)
                _audioConverter = gameObject.AddComponent<AudioConverter>();

            var dataToSend = new DataToSend
            {
                input =
                    new Input()
                    {
                        text = textToConvert
                    },
                voice =
                    new Voice()
                    {
                        languageCode = voice.languageCode,
                        name = voice.name
                    },
                audioConfig =
                    new AudioConfig()
                    {
                        audioEncoding = "MP3",
                        pitch = voice.pitch,
                        speakingRate = voice.speed
                    }
            };

            Debug.Log($"fileName: {fileName}, content: {textToConvert}");

            RequestService.SendDataToGoogle("https://texttospeech.googleapis.com/v1/text:synthesize", dataToSend,
                apiKey, _actionRequestReceived, errorReceived);
        }

        private static void RequestReceived(string requestData, string fileName, Action<string,AudioClip> audioClipReceived)
        {
            var audioData = JsonUtility.FromJson<AudioData>(requestData);
            string audiosPath = GlobalVariables.Get<string>("expPath")+"/Audios";
            if (!Directory.Exists(audiosPath))
            {
                Directory.CreateDirectory(audiosPath);
            }
            AudioConverter.SaveTextToMp3(audioData, GetAudioPath(fileName));
            _audioConverter.LoadClipFromMp3(audioClipReceived, GetAudioPath(fileName));
        }

        private void AudioClipReceived(string fileName, AudioClip clip)
        {
            for(int i=0; i< FN_ACPairs.Count; i++)
            {
                if (FN_ACPairs[i].Item1 == fileName)
                {
                    FN_ACPairs[i].Item2 = clip;
                }
            }

            while (FN_ACPairs.Count!=0 && FN_ACPairs[0].Item2 != null )
            {
                performanceManager.ReceiveAudioClip(FN_ACPairs[0].Item2, GetAudioPath(FN_ACPairs[0].Item1));
                FN_ACPairs.RemoveAt(0);
            }

            
        }

        private void ErrorReceived(BadRequestData badRequestData)
        {
            Debug.Log($"Error {badRequestData.error.code} : {badRequestData.error.message}");
        }

        public void PlayTextToSpeech(Message message)
        {
            _errorReceived = ErrorReceived;
            _audioClipReceived = AudioClipReceived;

            string fileName=GenerateAudioName(message.activityPeriod);
            FN_ACPairs.Add(new Pair<string, AudioClip>(fileName, null));


            VoiceScriptableObject voice;
            switch (message.source)
            {
                case (Message.Source.Operator):
                {
                        voice = operatorVoice;
                        break;
                }
                case(Message.Source.Signaller):
                {
                        voice = signallerVoice;
                        break;
                }
                case (Message.Source.Hint):
                    {
                        voice= hintVoice; 
                        break;
                    }
                default:
                {
                        voice = signallerVoice;
                        break;
                }
            }

            GetSpeechAudioFromGoogle(message.content, voice, fileName, _audioClipReceived, _errorReceived);
        }

        public void PlayTextToSpeech(Message[] messages)
        {
            foreach(Message message in messages)
            {
                PlayTextToSpeech(message);

            }
        }

        public static string GenerateAudioName(TaskManager.ActivityPeriod activityPeriod)
        {
            int i = 0;
            while(FileNameExists(activityPeriod.ToString() + '_' + Time.timeSinceLevelLoad.ToString() + '_' + DateTime.Now.ToString("HH-mm-ss_yyyy-MM-dd") + $"_{i}.mp3"))
            {
                i++;
            }
            return activityPeriod.ToString() + '_' + Time.timeSinceLevelLoad.ToString()+'_'+DateTime.Now.ToString("HH-mm-ss_yyyy-MM-dd") + $"_{i}.mp3";
        }

        private static bool FileNameExists(string fileName)
        {
            for (int j = 0; j < FN_ACPairs.Count; j++)
            {
                if (FN_ACPairs[j].Item1 == fileName)
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetAudioPath(string fileName)
        {

            string expPath = GlobalVariables.Get<string>("expPath");
            return expPath + "/Audios/" + fileName;
        }
    }
}