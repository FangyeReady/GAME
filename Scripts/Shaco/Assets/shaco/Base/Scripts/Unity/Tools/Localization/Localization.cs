using UnityEngine;
using System.Collections;

namespace shaco
{
    public class Localization : shaco.Base.Localization
    {
        private const string LOCALIZATION_LANGUAGE_KEY = "localizaiton_language";
        static private readonly SystemLanguage[] DEFAULT_LANGUAGES = new SystemLanguage[] { SystemLanguage.Japanese, SystemLanguage.English, SystemLanguage.Chinese };

        static public bool LoadWithJsonResourcesOrLocalPath(string relativePath, bool useOnlineFirst)
        {
            return LoadWithJsonResourcesOrLocalPath(relativePath, string.Empty, useOnlineFirst);
        }

        static public bool LoadWithJsonResourcesOrLocalPath(string relativePath, string multiVersionControlRelativePath, bool useOnlineFirst)
        {
            relativePath = GetJsonPathWithLanguage(relativePath);
            if (shaco.Base.FileHelper.HasFileNameExtension(relativePath))
                relativePath = shaco.Base.FileHelper.RemoveExtension(relativePath);

            string loadString = string.Empty;

            if (useOnlineFirst)
            {
                //load online resource
                var pathDiskLower = shaco.Base.FileHelper.ContactPath(shaco.ResourcesEx.DEFAULT_PREFIX_PATH_LOWER, relativePath).ToLower();
                var readObj = shaco.HotUpdateDataCache.Read(pathDiskLower, multiVersionControlRelativePath, pathDiskLower);

                if (!readObj.IsNull())
                {
                    loadString = shaco.HotUpdateHelper.AssetToString(readObj);
                }
                //load local resource
                else
                {
                    loadString = Resources.Load<Object>(relativePath).ToString();
                }
            }
            else
            {
                loadString = shaco.ResourcesEx.LoadResourcesOrLocal<Object>(relativePath, multiVersionControlRelativePath).ToString();
            }

            if (string.IsNullOrEmpty(loadString))
            {
                Log.Error("Localization LoadWithJsonResourcesPath error: not find json, relativePath=" + relativePath + " useOnlineFirst=" + useOnlineFirst);
                return false;
            }

            return LoadWithJsonString(loadString);
        }

        static public T GetResource<T>(string key, string multiVersionControlRelativePath = "", string defaultText = "") where T : UnityEngine.Object
        {
            var pathTmp = string.Empty;
            if (!shaco.Base.Localization.GetLocalizationDatas().ContainsKey(key))
            {
                pathTmp = string.IsNullOrEmpty(defaultText) ? key : defaultText;
            }
            else
            {
                pathTmp = shaco.Base.Localization.GetLocalizationDatas()[key];
            }

            if (!string.IsNullOrEmpty(pathTmp))
            {
                return shaco.ResourcesEx.LoadResourcesOrLocal<T>(pathTmp, multiVersionControlRelativePath);
            }
            else
            {
                return null;
            }
        }

        static public void SetUnityFont(UnityEngine.Font font)
        {
            SetFont((object)font);
        }

        static public UnityEngine.Font GetUnityFont()
        {
            return (UnityEngine.Font)GetFont();
        }

        /// <summary>
        /// Load current language from local save data, and set it
        /// If you have not previously set up a language, we will default to the system language
        /// If the system language does not support the configured language, the default is to configure the language of index 0
        /// </summary>
        static public void AutoSetCurrentLanuageWithLocalSave()
        {
            var languageString = shaco.DataSave.Instance.ReadString(LOCALIZATION_LANGUAGE_KEY);
            if (!string.IsNullOrEmpty(languageString))
                shaco.Base.Localization.SetCurrentLanguageString(languageString);
            else if (DEFAULT_LANGUAGES.Length > 0)
            {
                bool isSupportLanguage = false;
                string setLanguage = string.Empty;

                for (int i = 0; i < DEFAULT_LANGUAGES.Length; ++i)
                {
                    if (DEFAULT_LANGUAGES[i] == Application.systemLanguage)
                    {
                        isSupportLanguage = true;
                        setLanguage = Application.systemLanguage.ToString();
                        break;
                    }
                }

                if (!isSupportLanguage)
                    setLanguage = DEFAULT_LANGUAGES[0].ToString();

                shaco.Base.Localization.SetCurrentLanguageString(setLanguage);
            }
            else
                Log.Error("Localizaiton AutoSetCurrentLanuageWithLocalSave error: no support language ! languageString=" + languageString + " Application.systemLanguage=" + Application.systemLanguage);
        }

        static public void SetCurrentLanguage(SystemLanguage language)
        {
            var languageString = language.ToString();
            shaco.Base.Localization.SetCurrentLanguageString(languageString);
            shaco.DataSave.Instance.Write(LOCALIZATION_LANGUAGE_KEY, languageString);
        }

        static public SystemLanguage GetCurrentLanguage()
        {
            var language = shaco.Base.Localization.GetCurrentLanguageString();
            return language.ToEnum<SystemLanguage>();
        }

        static public void RealoadAllLocalizationComponentsInScene()
        {
            var allCompnents = Resources.FindObjectsOfTypeAll<shaco.LocalizationComponent>();
            for (int i = allCompnents.Length - 1; i >= 0; --i)
            {
                allCompnents[i].UpdateLocalization();
            }
        }
    }
}

