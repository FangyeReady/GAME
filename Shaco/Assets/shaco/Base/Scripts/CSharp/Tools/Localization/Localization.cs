using System.Collections;
using System.Collections.Generic;
using shaco.Base;

namespace shaco.Base
{
    public class Localization
    {
        private Dictionary<string, string> _localizationDictionary = new Dictionary<string, string>();
        private string _currentLanguage = string.Empty;
        private object _fontCache = null;

        private const string NULL_KEY = "[null_key]";
        private const string EMPTY_KEY = "[empty_key]";
        private const string MISSING_VALUE = "[missing:{0}]";
        private const string IGNORE_EMPTY_KEY = "//empty";

        static public bool LoadWithJsonPath(string path)
        {
            var instance = GameEntry.GetInstance<shaco.Base.Localization>();
            return instance.LoadWithJsonPathBase(path);
        }

        static public bool LoadWithJsonString(string json)
        {
            var instance = GameEntry.GetInstance<shaco.Base.Localization>();

            var jsonObjects = shaco.LitJson.JsonMapper.ToObject(json);
            if (0 == jsonObjects.Count)
            {
                Log.Error("Localization LoadWithJson error: no json data!");
                return false;
            }

            for (int i = jsonObjects.Count - 1; i >= 0; --i)
            {
                var jsonObjectTmp = jsonObjects[i];

                var key = jsonObjectTmp.ToProperty();
                var value = jsonObjectTmp.ToString();

                if (instance._localizationDictionary.ContainsKey(key))
                {
                    Log.Error("Localization LoadWithJson error: has same key=" + key);
                }
                else
                {
                    instance._localizationDictionary.Add(key, value);
                }
            }
            return true;
        }

        static public string GetText(string key, string defaultText = "")
        {
            if (null == key)
                return NULL_KEY;
            else if (key == string.Empty)
            {
                return EMPTY_KEY;
            }

            var instance = GameEntry.GetInstance<shaco.Base.Localization>();
            var retValue = string.Empty;
            if (!instance._localizationDictionary.ContainsKey(key))
            {
                retValue = string.IsNullOrEmpty(defaultText) ? key : defaultText;
            }
            else
            {
                retValue = instance._localizationDictionary[key];
            }

            if (string.IsNullOrEmpty(retValue))
                retValue = string.Format(MISSING_VALUE, key);

            if (IGNORE_EMPTY_KEY == retValue)
            {
                //Escape to empty character
                retValue = string.Empty;
            }
            return retValue;
        }

        static public string GetTextFormat(string key, params object[] param)
        {
            return GetTextFormatWithDefaultValue(key, string.Empty, param);
        }

        static public string GetTextFormatWithDefaultValue(string key, string defaultText = "", params object[] param)
        {
            if (null == key)
                return NULL_KEY;
            else if (key == string.Empty)
                return EMPTY_KEY;

            var instance = GameEntry.GetInstance<shaco.Base.Localization>();
            var retValue = string.Empty;
            if (!instance._localizationDictionary.ContainsKey(key))
            {
                retValue = string.IsNullOrEmpty(defaultText) ? key : defaultText;
            }
            else
            {
                var textTmp = instance._localizationDictionary[key];
                if (null != param && param.Length > 0)
                {
                    try
                    {
                        retValue = string.Format(textTmp, param);
                    }
                    catch (System.Exception)
                    {
                        retValue = textTmp;
                    }
                }
                else
                    retValue = textTmp;
            }

            if (string.IsNullOrEmpty(retValue))
                retValue = string.Format(MISSING_VALUE, key);

            if (IGNORE_EMPTY_KEY == retValue)
            {
                //Escape to empty character
                retValue = string.Empty;
            }
            return retValue;
        }

        static public void Clear()
        {
            var instance = GameEntry.GetInstance<shaco.Base.Localization>();
            instance._localizationDictionary.Clear();
            instance._fontCache = null;
        }

        static public string GetCurrentLanguageString()
        {
            return GameEntry.GetInstance<shaco.Base.Localization>()._currentLanguage;
        }

        static public void SetCurrentLanguageString(string language)
        {
            GameEntry.GetInstance<shaco.Base.Localization>()._currentLanguage = language;
        }

        static public int GetLocalizationCount()
        {
            return GameEntry.GetInstance<shaco.Base.Localization>()._localizationDictionary.Count;
        }

        static protected Dictionary<string, string> GetLocalizationDatas()
        {
            return GameEntry.GetInstance<shaco.Base.Localization>()._localizationDictionary;
        }

        static protected void SetFont(object font)
        {
            GameEntry.GetInstance<shaco.Base.Localization>()._fontCache = font;
        }

        static protected object GetFont()
        {
            return GameEntry.GetInstance<shaco.Base.Localization>()._fontCache;
        }

        static protected string GetJsonPathWithLanguage(string path)
        {
            string language = shaco.Base.GameEntry.GetInstance<shaco.Base.Localization>()._currentLanguage;
            if (string.IsNullOrEmpty(language)) return path;

            string retValue = string.Empty;
            string folderTmp = shaco.Base.FileHelper.GetFolderNameByPath(path);
            string fileNameTmp = shaco.Base.FileHelper.GetLastFileName(path);
            bool hasExtensionsTmp = shaco.Base.FileHelper.HasFileNameExtension(fileNameTmp);
            string fileNameWithNoExtensionsTmp = hasExtensionsTmp ? shaco.Base.FileHelper.RemoveExtension(fileNameTmp) : fileNameTmp;
            string extensionsTmp = hasExtensionsTmp ? shaco.Base.FileHelper.GetFilNameExtension(fileNameTmp) : string.Empty;

            if (!string.IsNullOrEmpty(fileNameWithNoExtensionsTmp))
            {
                fileNameWithNoExtensionsTmp += "_";
            }

            if (!hasExtensionsTmp)
            {
                extensionsTmp = "json";
            }

            retValue = folderTmp + fileNameWithNoExtensionsTmp + language + (!hasExtensionsTmp ? shaco.Base.FileDefine.DOT_SPLIT : string.Empty) + extensionsTmp;
            return retValue;
        }

        protected virtual bool LoadWithJsonPathBase(string path)
        {
            path = GetJsonPathWithLanguage(path);
            var readJson = FileHelper.ReadAllByUserPath(path);
            if (string.IsNullOrEmpty(readJson))
            {
                Log.Error("Localization LoadWithJsonPathBase error: not find json, path=" + path);
                return false;
            }

            return LoadWithJsonString(readJson);
        }
    }
}

