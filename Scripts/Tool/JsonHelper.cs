using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class JsonHelper
{
    public static T ObjFromJson<T>(string path)
    {
        string json = File.ReadAllText(path);
        T obj = JsonUtility.FromJson<T>(json);
        return obj;
    }
    public static T[] ListFromJson<T>(string path)
    {
        string json = File.ReadAllText(path);
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }


    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }


    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

     
}
