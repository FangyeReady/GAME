using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace shaco
{
    public interface IDataSave
    {

        //write functions ------ unity type
        void Write(string key, Vector2 value);
        void Write(string key, Vector3 value);
        void Write(string key, Vector4 value);
        void Write(string key, Color value);
        void Write(string key, Rect value);

        //read functions ------ unity type
        Vector2 ReadVector2(string key);
        Vector2 ReadVector2(string key, Vector2 defaultValue);
        Vector3 ReadVector3(string key);
        Vector3 ReadVector3(string key, Vector3 defaultValue);
        Vector4 ReadVector4(string key);
        Vector4 ReadVector4(string key, Vector3 defaultValue);
        Color ReadColor(string key);
        Color ReadColor(string key, Color defaultValue);
        Rect ReadRect(string key);
        Rect ReadRect(string key, Rect defaultValue);
    }
}