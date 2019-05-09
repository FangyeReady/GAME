#if !USE_UGUI
#if UNITY_4_6_OR_NEWER || UNITY_5_3_OR_NEWER
#define USE_UGUI
#endif
#endif

//--------------------------------------------------------------------------------
//Attention:
//if you want support NGUI, please add 'SUPPORT_NGUI' macro in BuildSettings -> Scripting Define Symbols
//--------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class LanguageManager : MonoBehaviour
    {
        public enum LanguageType
        {
            Chineses,
            English,
            //			Japanese,
            //			Korean,
            Count
        }

        public enum TargetType
        {
            PlaceHolder1 = -1,  //Only for placeholder, do not use
            PlaceHolder2 = -2,
            UnKnown = 0,
            GameObject = 1,

#if USE_UGUI
            UGUI_Image = 2,
            UGUI_Text = 3,
            shaco_RichText = 4,
#endif

#if SUPPORT_NGUI
            NGUI_SpriteAtals = 5,
            NGUI_Sprite2D = 6,
            NGUI_Label = 7
#endif
        }

        public enum TransfromType
        {
            None,
            Position,
            LocalPosition,
            Rotation,
            Scale,
        }

        [System.Serializable]
        public class GameObjectInfo
        {
            public bool isResourcePrefab = false;
            public bool isAutoDestroy = false;
            public GameObject targetGameObject = null;
        }

        [System.Serializable]
        public class Info
        {
            public bool isHide = false;

            public GameObject target = null;
            public TargetType type = TargetType.UnKnown;

            public bool isSetNativeSize = true;
            public Sprite[] sprites = null;
            public string[] texts = null;
            public GameObjectInfo[] targetGameObjects = null;

#if SUPPORT_NGUI
            public UIAtlas[] atals = null;
#endif

            public List<EventDelegateS> listCallBack = new List<EventDelegateS>();
        }

        [HideInInspector]
        public LanguageType CurrentLanguage = LanguageType.Chineses;

        [HideInInspector]
        public List<Info> ListInfo = new List<Info>();

        public EventDelegateS OnChangeLanguageCallBack = new EventDelegateS();

        void Awake()
        {
            changeLanguage(getDefaultSystemLanguage());
        }

        void OnDestroy()
        {
            shaco.Base.GameEntry.RemoveIntance<LanguageManager>();
        }

        static public void changeLanguage(LanguageType type)
        {
            var instace = shaco.GameEntry.GetComponentInstance<LanguageManager>(false);
            changeLanguage(type, instace);
        }

        static public void changeLanguage(LanguageType type, LanguageManager manager)
        {
            if ((LanguageType)manager.CurrentLanguage == type)
                return;

            manager.CurrentLanguage = type;

            for (int i = 0; i < manager.ListInfo.Count; ++i)
            {
                var infoTmp = manager.ListInfo[i];

                if (infoTmp == null || infoTmp.target == null || infoTmp.type == TargetType.UnKnown)
                {
                    continue;
                }

                //update taget
                manager.changeLanguage(infoTmp, type);

                //dispatch callback
                EventDelegateS.Execute(infoTmp.listCallBack);
            }

            manager.OnChangeLanguageCallBack.Execute();
        }

        static public LanguageType getCurrentLanguage()
        {
            return shaco.GameEntry.GetComponentInstance<LanguageManager>(false) ? (LanguageType)shaco.GameEntry.GetComponentInstance<LanguageManager>(false).CurrentLanguage : LanguageType.Chineses;
        }

        static public LanguageType getDefaultSystemLanguage()
        {
            var tmp = Application.systemLanguage.ToString();
            if (!tmp.Contains("Chinese"))
                return LanguageType.English;
            else
                return LanguageType.Chineses;
        }

        public bool isValidInfoParams(Info info)
        {
            bool ret = true;
            if (info == null)
            {
                ret = false;
                return ret;
            }

            switch (info.type)
            {
                case TargetType.GameObject:
                    {
                        if (info.targetGameObjects == null)
                            ret = false;
                        else
                        {
                            for (int i = 0; i < info.targetGameObjects.Length; ++i)
                            {
                                if (info.targetGameObjects[i].targetGameObject == null)
                                {
                                    ret = false;
                                    break;
                                }
                            }
                        }
                        break;
                    }
#if USE_UGUI
                case TargetType.UGUI_Image:
                    {
                        var imageTmp = info.target.GetComponent<UnityEngine.UI.Image>();
                        if (info.sprites == null || imageTmp == null)
                            ret = false;
                        else if (!UnityHelper.IsValidListValues<Sprite>(info.sprites))
                            ret = false;
                        break;
                    }
                case TargetType.UGUI_Text:
                    {
                        var textTmp = info.target.GetComponent<UnityEngine.UI.Text>();
                        if (info.texts == null || textTmp == null)
                            ret = false;
                        else if (!UnityHelper.IsValidListValues<string>(info.texts))
                            ret = false;

                        break;
                    }
                case TargetType.shaco_RichText:
                    {
                        var textTmp = info.target.GetComponent<shaco.RichText>();
                        if (info.texts == null || textTmp == null)
                            ret = false;
                        else if (!UnityHelper.IsValidListValues<string>(info.texts))
                            ret = false;

                        break;
                    }
#endif
#if SUPPORT_NGUI
                case TargetType.NGUI_SpriteAtals:
                    {
                        var imageTmp = info.target.GetComponent<UISprite>();
                        if (info.sprites == null || imageTmp == null)
                            ret = false;
                        else if (!UnityHelper.isValidListValues<UIAtlas>(info.atals) || !UnityHelper.isValidListValues<string>(info.texts))
                            ret = false;
                        break;
                    }
                case TargetType.NGUI_Sprite2D:
                    {
                        var imageTmp = info.target.GetComponent<UI2DSprite>();
                        if (info.sprites == null || imageTmp == null)
                            ret = false;
                        else if (!UnityHelper.isValidListValues<Sprite>(info.sprites))
                            ret = false;
                        break;
                    }
                case TargetType.NGUI_Label:
                    {
                        var textTmp = info.target.GetComponent<UILabel>();
                        if (info.texts == null || textTmp == null)
                            ret = false;
                        else if (!UnityHelper.isValidListValues<string>(info.texts))
                            ret = false;
                        break;
                    }
#endif
                default: ret = false; break;
            }
            return ret;
        }

        public Info updateTargetInfo(Info info)
        {
            if (info.target == null)
            {
                return info;
            }

            info.type = getTargetType(info.target);

            switch (info.type)
            {
                case TargetType.GameObject:
                    {
                        info.targetGameObjects = new GameObjectInfo[(int)LanguageType.Count];
                        for (int i = 0; i < info.targetGameObjects.Length; ++i)
                        {
                            info.targetGameObjects[i] = new GameObjectInfo();
                        }
                        info.targetGameObjects[(int)CurrentLanguage].isResourcePrefab = false;
                        info.targetGameObjects[(int)CurrentLanguage].targetGameObject = info.target;
                        break;
                    }
#if USE_UGUI
                case TargetType.UGUI_Image:
                    {
                        info.sprites = new Sprite[(int)LanguageType.Count];
                        info.sprites[(int)CurrentLanguage] = info.target.GetComponent<UnityEngine.UI.Image>().sprite;
                        break;
                    }
                case TargetType.UGUI_Text:
                    {
                        info.texts = new string[(int)LanguageType.Count];
                        info.texts[(int)CurrentLanguage] = info.target.GetComponent<UnityEngine.UI.Text>().text;
                        break;
                    }
                case TargetType.shaco_RichText:
                    {
                        info.texts = new string[(int)LanguageType.Count];
                        info.texts[(int)CurrentLanguage] = info.target.GetComponent<shaco.RichText>().text;
                        break;
                    }
#endif
#if SUPPORT_NGUI
                case TargetType.NGUI_SpriteAtals:
                    {
                        info.atals = new UIAtlas[(int)LanguageType.Count];
                        info.texts = new string[(int)LanguageType.Count];
                        info.atals[(int)CurrentLanguage] = info.target.GetComponent<UISprite>().atlas;
                        info.texts[(int)CurrentLanguage] = info.target.GetComponent<UISprite>().spriteName;
                        break;
                    }
                case TargetType.NGUI_Sprite2D:
                    {
                        info.sprites = new Sprite[(int)LanguageType.Count];
                        info.sprites[(int)CurrentLanguage] = info.target.GetComponent<UI2DSprite>().sprite2D;
                        break;
                    }
                case TargetType.NGUI_Label:
                    {
                        info.texts = new string[(int)LanguageType.Count];
                        info.texts[(int)CurrentLanguage] = info.target.GetComponent<UILabel>().text;
                        break;
                    }
#endif
                default: Log.Error("unsupport type"); break;
            }

            return info;
        }

        private TargetType getTargetType(GameObject target)
        {
            TargetType ret = TargetType.UnKnown;
            if (target == null)
            {
                Log.Error("getTargetType erorr: not file name target=" + target);
                return ret;
            }

#if USE_UGUI
            if (target.GetComponent<UnityEngine.UI.Image>()
                && target.GetComponent<UnityEngine.UI.Image>().enabled
                && target.GetComponent<UnityEngine.UI.Image>().sprite != null)
            {
                ret = TargetType.UGUI_Image;
            }
            else if (target.GetComponent<UnityEngine.UI.Text>()
                && target.GetComponent<UnityEngine.UI.Text>().enabled
                && target.GetComponent<UnityEngine.UI.Text>().text != null)
            {
                ret = TargetType.UGUI_Text;
            }
            else if (target.GetComponent<shaco.RichText>()
                && target.GetComponent<shaco.RichText>().enabled
                && target.GetComponent<shaco.RichText>().text != null)
                ret = TargetType.shaco_RichText;
#endif
#if SUPPORT_NGUI
            if (target.GetComponent<UI2DSprite>()
                && target.GetComponent<UI2DSprite>().enabled
                && target.GetComponent<UI2DSprite>().sprite2D != null)
            {
                ret = TargetType.NGUI_Sprite2D;
            }
            else if (target.GetComponent<UISprite>()
                && target.GetComponent<UISprite>().enabled
                && target.GetComponent<UISprite>().atlas != null
                && !string.IsNullOrEmpty(target.GetComponent<UISprite>().spriteName))
            {
                ret = TargetType.NGUI_SpriteAtals;
            }
            else if (target.GetComponent<UILabel>()
                && target.GetComponent<UILabel>().enabled
                && target.GetComponent<UILabel>().text != null)
            {
                ret = TargetType.NGUI_Label;
            }
#endif
            if (ret == TargetType.UnKnown)
                ret = TargetType.GameObject;
            return ret;
        }

        private Sprite getTarGetValueSprite(Info info, LanguageType type)
        {
            Sprite ret = null;
            if (info.target == null)
            {
                Log.Error("getTarGetValueImagePath error: target is null");
                return ret;
            }

            var valueTmp = info.sprites[(int)type];
            ret = valueTmp;

            return ret;
        }

        private string getTarGetValueText(Info info, LanguageType type)
        {
            string ret = string.Empty;
            if (info.target == null)
            {
                Log.Error("getTarGetValueText error: target is null");
                return ret;
            }

            var valueTmp = info.texts[(int)type];
            ret = valueTmp;

            return ret;
        }

        private GameObject getTarGetValueGameObject(Info info, LanguageType type)
        {
            GameObject ret = null;
            if (info.target == null)
            {
                Log.Error("getTarGetValueGameObject error: target is null");
                return ret;
            }

            var valueTmp = info.targetGameObjects[(int)type].targetGameObject;
            ret = valueTmp;

            return ret;
        }

        private void changeLanguage(Info info, LanguageType type)
        {
            switch (info.type)
            {
#if USE_UGUI
                case TargetType.UGUI_Image:
                    {
                        var imageTmp = info.target.GetComponent<UnityEngine.UI.Image>();
                        if (SceneManager.OpenDebugMode)
                        {
                            if (info.sprites == null || info.sprites[(int)type] == null || imageTmp == null)
                            {
                                Log.Error("changeLanguage erorr: invalid params");
                                break;
                            }
                        }
                        imageTmp.sprite = info.sprites[(int)type];

                        if (info.isSetNativeSize)
                            imageTmp.SetNativeSize();
#if UNITY_EDITOR
                        //todo: fixed in editor mode change language image sprites won't change
                        if (imageTmp.gameObject.activeInHierarchy)
                        {
                            imageTmp.gameObject.SetActive(false);
                            imageTmp.gameObject.SetActive(true);
                        }
#endif
                        break;
                    }
                case TargetType.UGUI_Text:
                    {
                        var textTmp = info.target.GetComponent<UnityEngine.UI.Text>();
                        if (SceneManager.OpenDebugMode)
                        {
                            if (info.texts == null || info.texts[(int)type] == null || textTmp == null)
                            {
                                Log.Error("changeLanguage erorr: invalid params");
                                break;
                            }
                        }
                        textTmp.text = info.texts[(int)type];
#if UNITY_EDITOR
                        //todo: fixed in editor mode change language image sprites won't change
                        if (textTmp.gameObject.activeInHierarchy)
                        {
                            textTmp.gameObject.SetActive(false);
                            textTmp.gameObject.SetActive(true);
                        }
#endif
                        break;
                    }
                case TargetType.GameObject:
                    {
                        if (SceneManager.OpenDebugMode)
                        {
                            if (info.targetGameObjects == null || info.targetGameObjects[(int)type] == null)
                            {
                                Log.Error("changeLanguage erorr: invalid params");
                                break;
                            }
                        }

                        //disable all gameobject 
                        for (int i = 0; i < info.targetGameObjects.Length; ++i)
                        {
                            info.targetGameObjects[i].targetGameObject.SetActive(false);
                        }

                        //new target
                        if (info.targetGameObjects[(int)type].isResourcePrefab && !info.targetGameObjects[(int)type].isAutoDestroy)
                        {
                            GameObject newObj = MonoBehaviour.Instantiate(info.targetGameObjects[(int)type].targetGameObject) as GameObject;
                            newObj.name = info.targetGameObjects[(int)type].targetGameObject.name + "(" + getCurrentLanguage() + "_Clone)";
                            newObj.transform.position = info.target.transform.position;
                            newObj.transform.rotation = info.target.transform.rotation;
                            newObj.transform.localScale = info.target.transform.localScale;

                            UnityHelper.ChangeParentLocalPosition(newObj, info.target.transform.parent.gameObject);

                            info.targetGameObjects[(int)type].targetGameObject = newObj;
                            info.targetGameObjects[(int)type].isAutoDestroy = true;
                        }

                        //set target gameobject active
                        info.targetGameObjects[(int)type].targetGameObject.SetActive(true);
                        break;
                    }
                case TargetType.shaco_RichText:
                    {
                        var textTmp = info.target.GetComponent<shaco.RichText>();
                        if (SceneManager.OpenDebugMode)
                        {
                            if (info.texts == null || info.texts[(int)type] == null || textTmp == null)
                            {
                                Log.Error("changeLanguage erorr: invalid params");
                                break;
                            }
                        }
                        textTmp.text = info.texts[(int)type];
#if UNITY_EDITOR
                        //todo: fixed in editor mode change language image sprites won't change
                        if (textTmp.gameObject.activeInHierarchy)
                        {
                            textTmp.gameObject.SetActive(false);
                            textTmp.gameObject.SetActive(true);
                        }
#endif
                        break;
                    }
#endif
#if SUPPORT_NGUI
                case TargetType.NGUI_SpriteAtals:
                    {
                        var imageTmp = info.target.GetComponent<UISprite>();
                        if (SceneManager.OpenDebugMode)
                        {
                            if (info.atals == null || info.atals[(int)type] == null || info.texts[(int)type] == null || imageTmp == null)
                            {
                                Log.Error("changeLanguage erorr: invalid params");
                                break;
                            }
                        }
                        imageTmp.atlas = info.atals[(int)type];
                        imageTmp.spriteName = info.texts[(int)type];

                        if (info.isSetNativeSize)
                            imageTmp.MakePixelPerfect();
#if UNITY_EDITOR
                        //todo: fixed in editor mode change language image sprites won't change
                        if (imageTmp.gameObject.activeInHierarchy)
                        {
                            imageTmp.gameObject.SetActive(false);
                            imageTmp.gameObject.SetActive(true);
                        }
#endif
                        break;
                    }
                case TargetType.NGUI_Sprite2D:
                    {
                        var imageTmp = info.target.GetComponent<UI2DSprite>();
                        if (SceneManager.OpenDebugMode)
                        {
                            if (info.sprites == null || info.sprites[(int)type] == null || imageTmp == null)
                            {
                                Log.Error("changeLanguage erorr: invalid params");
                                break;
                            }
                        }
                        imageTmp.sprite2D = info.sprites[(int)type];

                        if (info.isSetNativeSize)
                            imageTmp.MakePixelPerfect();
#if UNITY_EDITOR
                        //todo: fixed in editor mode change language image sprites won't change
                        if (imageTmp.gameObject.activeInHierarchy)
                        {
                            imageTmp.gameObject.SetActive(false);
                            imageTmp.gameObject.SetActive(true);
                        }
#endif
                        break;
                    }
                case TargetType.NGUI_Label:
                    {
                        var textTmp = info.target.GetComponent<UILabel>();
                        if (SceneManager.OpenDebugMode)
                        {
                            if (info.texts == null || info.texts[(int)type] == null || textTmp == null)
                            {
                                Log.Error("changeLanguage erorr: invalid params");
                                break;
                            }
                        }
                        textTmp.text = info.texts[(int)type];
#if UNITY_EDITOR
                        //todo: fixed in editor mode change language image sprites won't change
                        if (textTmp.gameObject.activeInHierarchy)
                        {
                            textTmp.gameObject.SetActive(false);
                            textTmp.gameObject.SetActive(true);
                        }
#endif
                        break;
                    }
#endif
                default: Log.Error("changeLanguage error: unspport type!"); break;
            }
        }
    }
}
