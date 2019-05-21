using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using shaco.Base;

namespace shaco
{
    public class XMLHelper
    {
        public class TimeInfo
        {
            public int Hour;
            public int Minute;
            public int Second;

            public TimeInfo(int hour, int minute, int second)
            {
                Hour = hour;
                Minute = minute;
                Second = second;
            }
            public TimeInfo()
            {
                Hour = 0;
                Minute = 0;
                Second = 0;
            }
        }

        public delegate void OnForeachElementCallBack(int index, XmlElement element);
        public bool OpenDebugMode = false;

        private Dictionary<string, XmlDocument> _mapXml = new Dictionary<string, XmlDocument>();
        private bool _isAutoSaveWhenValueChanged = true;

        private bool LoadXML(string xmlFilePath)
        {
            if (_mapXml.ContainsKey(xmlFilePath))
            {
				Log.Info("LoadXML warning: has loaded this xml before !");
                return false;
            }

            string fullpath = string.Empty;

            bool hasStoragePathFile = false;

            if (OpenDebugMode)
            {
                fullpath = GetXmlStoragePathByFileName(xmlFilePath);
                if (!File.Exists(fullpath))
                    fullpath = string.Empty;
                else
                {
                    hasStoragePathFile = true;

                    if (OpenDebugMode)
                    {
						Log.Info("loadXml path = " + fullpath);
                    }
                }
            }

            if (string.IsNullOrEmpty(fullpath))
                fullpath = GetXmlReadOnlyPathByFileName(xmlFilePath); ;


            //delete by shaco
            //todo: this function can't work on android
            //if (!System.IO.File.Exists(fullpath))
            //{
            //    Log.Error("LoadXML error: fullpath does not exist ! " + fullpath);
            //    return false;
            //}
            //delete end

            XmlDocument doc = new XmlDocument();
            XmlReaderSettings setTmp = new XmlReaderSettings();
            setTmp.IgnoreComments = true;

            try
            {
                if (Application.platform == RuntimePlatform.Android && !hasStoragePathFile)
                {
                    using (WWW wwwTmp = new WWW(fullpath))
					{
						while (!wwwTmp.isDone) { }
						doc.LoadXml(wwwTmp.text);
					}
                }
                else
                {
                    XmlReader reader = XmlReader.Create(fullpath, setTmp);
                    doc.Load(reader);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("LoadXML prase error!" + " Exception=" + ex);
                return false;
            }

            GameEntry.GetInstance<XMLHelper>()._mapXml.Add(xmlFilePath, doc);

            return true;
        }

        static public bool UnloadXml(string xmlFilePath)
        {
            if (GameEntry.GetInstance<XMLHelper>()._mapXml.ContainsKey(xmlFilePath))
            {
                Log.Error("UnloadXml error: can't find xml by xmlFilePath=" + xmlFilePath);
                return false;
            }

            GameEntry.GetInstance<XMLHelper>()._mapXml.Remove(xmlFilePath);
			Log.Info("UnloadXml success xmlFilePath=" + xmlFilePath);

            return true;
        }

        static public XmlElement GetElement(string xmlFilePath, string keyPath)
        {
            XmlElement ret = null;

            var doc = GetXmlDocumentWithAutoCreate(xmlFilePath);

            ret = GetElementByDocument(doc, keyPath);

            return ret;
        }

        static public XmlElement GetElement(string xmlFilePath, string keyPath, int elementIndex)
        {
            XmlElement parent = GetElement(xmlFilePath, keyPath);
            return GetElementByIndex(parent, elementIndex);
        }

        static public List<XmlElement> GetElementsByChild(XmlDocument document, string keyPath)
        {
            List<XmlElement> ret = new List<XmlElement>();
            var parentElement = GetElementByDocument(document, keyPath);
            if (parentElement == null)
                return ret;

            XmlElement firstElement = parentElement.FirstChild as XmlElement;
            while (null != firstElement)
            {
                ret.Add(firstElement);

                firstElement = firstElement.NextSibling as XmlElement;
            }

            return ret;
        }

        //----------------------------------------------------------------------------------------------------
        //get value functions begin
        //----------------------------------------------------------------------------------------------------
        static public string GetValueString(string xmlFilePath, string keyPath, string attribute = "value", string defaultValue= "")
        {
            XmlElement elementTmp = XMLHelper.GetElement(xmlFilePath, keyPath);
			if (elementTmp == null)
			{
				Log.Warning("XMLHelper Warning: GetValueString not Element xmlFilePath=" + xmlFilePath + " keyPath=" + keyPath + " attribute=" + attribute);
				return defaultValue;
			}
            return GetValueStringByElement(elementTmp, attribute, defaultValue);
        }

        static public string GetValueString(string xmlFilePath, string keyPath, int elementIndex, string attribute = "value", string defaultValue= "")
        {
            XmlElement parent = XMLHelper.GetElement(xmlFilePath, keyPath);
            XmlElement elementTmp = GetElementByIndex(parent, elementIndex);
			if (elementTmp == null)
			{
				Log.Warning("XMLHelper Warning: GetValueString not Element xmlFilePath=" + xmlFilePath + " elementIndex=" + elementIndex + " keyPath=" + keyPath + " attribute=" + attribute);
				return defaultValue;
			}
            return GetValueStringByElement(elementTmp, attribute, defaultValue);
        }

        static public string GetValueString(XmlElement element, string keyPath, string attribute = "value", string defaultValue= "")
        {
            XmlElement elementTmp = XMLHelper.GetElementByElement(element, keyPath);
			if (elementTmp == null)
			{
				Log.Warning("XMLHelper Warning: GetValueString not Element keyPath=" + keyPath + " attribute=" + attribute);
				return defaultValue;
			}
            return GetValueStringByElement(elementTmp, attribute, defaultValue);
        }

        static public int GetValueInt(string xmlFilePath, string keyPath, string attribute = "value", int defaultValue = 0)
        {
            var valueTmp = XMLHelper.GetValueString(xmlFilePath, keyPath, attribute, defaultValue.ToString());
            return valueTmp.ToInt(defaultValue);
        }

        static public int GetValueInt(string xmlFilePath, string keyPath, int elementIndex, string attribute = "value", int defaultValue = 0)
        {
            var valueTmp = XMLHelper.GetValueString(xmlFilePath, keyPath, elementIndex, attribute, defaultValue.ToString());
            return valueTmp.ToInt(defaultValue);
        }

        static public int GetValueInt(XmlElement element, string keyPath, string attribute = "value", int defaultValue = 0)
        {
            var valueTmp = XMLHelper.GetValueString(element, keyPath, attribute, defaultValue.ToString());
            return valueTmp.ToInt(defaultValue);
        }

        static public float GetValueFloat(string xmlFilePath, string keyPath, string attribute = "value", float defaultValue = 0)
        {
            var valueTmp = XMLHelper.GetValueString(xmlFilePath, keyPath, attribute, defaultValue.ToString());
            return valueTmp.ToFloat(defaultValue);
        }

        static public float GetValueFloat(string xmlFilePath, string keyPath, int elementIndex, string attribute = "value", float defaultValue = 0)
        {
            var valueTmp = XMLHelper.GetValueString(xmlFilePath, keyPath, elementIndex, attribute, defaultValue.ToString());
            return valueTmp.ToFloat(defaultValue);
        }

        static public float GetValueFloat(XmlElement element, string keyPath, string attribute = "value", float defaultValue = 0)
        {
            var valueTmp = XMLHelper.GetValueString(element, keyPath, attribute, defaultValue.ToString());
            return valueTmp.ToFloat(defaultValue);
        }

        static public bool GetValueBool(string xmlFilePath, string keyPath, string attribute = "value", bool defaultValue = false)
        {
            var valueTmp = XMLHelper.GetValueString(xmlFilePath, keyPath, attribute, defaultValue.ToString());
            return valueTmp.ToBool(defaultValue);
        }

        static public bool GetValueBool(string xmlFilePath, string keyPath, int elementIndex, string attribute = "value", bool defaultValue = false)
        {
            var valueTmp = XMLHelper.GetValueString(xmlFilePath, keyPath, elementIndex, attribute, defaultValue.ToString());
            return valueTmp.ToBool(defaultValue);
        }

        static public bool GetValueBool(XmlElement element, string keyPath, string attribute = "value", bool defaultValue = false)
        {
            var valueTmp = XMLHelper.GetValueString(element, keyPath, attribute, defaultValue.ToString());
            return valueTmp.ToBool(defaultValue);
        }
        //----------------------------------------------------------------------------------------------------
        //get value functions end
        //----------------------------------------------------------------------------------------------------

        //----------------------------------------------------------------------------------------------------
        //set value functions begin
        //----------------------------------------------------------------------------------------------------
        static public void SetValueString(string xmlFilePath, string keyPath, string attribute, string SetValue)
        {
            XmlDocument document = null;
            var element = SetElement(xmlFilePath, keyPath, ref document, true);

            if (document == null)
                Log.Error("SetValueString error: can't get XmlDocument by xmlFilePath = " + xmlFilePath + " keyPath = " + keyPath);
            else 
                SetValueString(xmlFilePath, document, element, attribute, SetValue);
        }

        static public void SetValueString(string xmlFilePath, string keyPath, int elementIndex, string attribute, string SetValue)
        {
            XmlDocument document = null;

            var element = SetElement(xmlFilePath, keyPath, elementIndex, ref document, true);

            if (document == null)
                Log.Error("SetValueString error: can't get XmlDocument by xmlFilePath = " + xmlFilePath + " elementIndex = " + elementIndex);
            else
                SetValueString(xmlFilePath, document, element, attribute, SetValue);
        }

        static public void SetValueInt(string xmlFilePath,  string keyPath, string attribute, int SetValue)
        {
            SetValueString(xmlFilePath, keyPath, attribute, SetValue.ToString());
        }

        static public void SetValueFloat(string xmlFilePath, string keyPath, string attribute, float SetValue)
        {
            SetValueString(xmlFilePath, keyPath, attribute, SetValue.ToString());
        }

        static public void SetValueBool(string xmlFilePath, string keyPath, string attribute, bool SetValue)
        {
            SetValueString(xmlFilePath, keyPath, attribute, SetValue.ToString());
        }

        static public void SetValueInt(string xmlFilePath, string keyPath, int elementIndex, string attribute, int SetValue)
        {
            SetValueString(xmlFilePath, keyPath, elementIndex, attribute, SetValue.ToString());
        }

        static public void SetValueFloat(string xmlFilePath, string keyPath, int elementIndex, string attribute, float SetValue)
        {
            SetValueString(xmlFilePath, keyPath, elementIndex, attribute, SetValue.ToString());
        }

        static public void SetValueBool(string xmlFilePath, string keyPath, int elementIndex, string attribute, bool SetValue)
        {
            SetValueString(xmlFilePath, keyPath, elementIndex, attribute, SetValue.ToString());
        }
        //----------------------------------------------------------------------------------------------------
        //set value functions end
        //----------------------------------------------------------------------------------------------------

        static public void ForeachDocument(string xmlFilePath, OnForeachElementCallBack callfunc)
        {
            ForeachDocument(xmlFilePath, string.Empty, callfunc);
        }

        static public void ForeachDocument(string xmlFilePath, string keyPath, OnForeachElementCallBack callfunc)
        {
            if (callfunc == null)
            {
                Log.Error("ForeachDocument error: callfunc is null");
                return;
            }

            var elementTmp = GetElement(xmlFilePath, keyPath);
			ForeachElement(elementTmp, callfunc);
        }

		static public void ForeachElement(XmlElement element, OnForeachElementCallBack callfunc)
		{
			if (callfunc == null)
			{
				Log.Error("ForeachDocument error: callfunc is null");
				return;
			}
			if (element == null)
			{
				Log.Error("ForeachDocument error: not find element is null");
				return;
			}

			var elementTmp = element;

			int index = 0;
			var firstChild = elementTmp.FirstChild as XmlElement;

			while (null != firstChild)
			{
				if (firstChild == null)
				{
					Log.Error("ForeachDocument error: child not 'XmlElement' type child=" + firstChild);
				}
				else
				{
					callfunc(index++, firstChild);
				}
				firstChild = firstChild.NextSibling as XmlElement;
			}
		}

        static public void SaveXml(string xmlFilePath)
        {
            XmlDocument document = GetXmlDocument(xmlFilePath);

            if (null == document)
            {
                Log.Error("not find xml by path = " + xmlFilePath);
            }
            else
                SaveXml(document, xmlFilePath);
        }

        static public void DeleteXml(string xmlFilePath)
        {
            var filePath = GetXmlStoragePathByFileName(xmlFilePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
				Log.Info("DeleteXml success: path = " + filePath);
            }
            else
				Log.Warning("DeleteXml error: not find xml by path = " + filePath);
        }

        //format string 10:10:10
        static public TimeInfo ParseTime(string time)
        {
            TimeInfo ret = new TimeInfo();
            var strs = time.Split(':');

            if (strs.Length > 0)
            {
                ret.Hour = strs[0].ToInt();
            }

            if (strs.Length > 1)
            {
                ret.Minute = strs[1].ToInt();
            }

            if (strs.Length > 2)
            {
                ret.Second = strs[2].ToInt();
            }

            return ret;
        }

        static public TimeInfo GetValueTime(string xmlFilePath, string keyPath, string attribute = "value")
        {
            var valueTmp = XMLHelper.GetValueString(xmlFilePath, keyPath, attribute, string.Empty);
            return XMLHelper.ParseTime(valueTmp);
        }

        static public bool IsContainTime(int hour, int minute, int second, int minHour, int minMinute, int minSecond, int maxHour, int maxMinute, int maxSecond)
        {
            int curSeconds = hour * 60 * 60 + minute * 60 + second;
            int minSeconds = minHour * 60 * 60 + minMinute * 60 + minSecond;
            int maxSeconds = maxHour * 60 * 60 + maxMinute * 60 + maxSecond;

            return curSeconds >= minSeconds && curSeconds <= maxSeconds;
        }

        static public bool IsContainTime(TimeInfo cur, TimeInfo min, TimeInfo max)
        {
            return XMLHelper.IsContainTime(cur.Hour, cur.Minute, cur.Second, min.Hour, min.Minute, min.Second, max.Hour, max.Minute, max.Second);
        }

        static public bool IsGreaterThan(int hour, int minute, int second, int hourOther, int minuteOther, int secondOther)
        {
            int curSeconds = hour * 60 * 60 + minute * 60 + second;
            int otherSeconds = hourOther * 60 * 60 + minuteOther * 60 + secondOther;

            return curSeconds > otherSeconds;
        }

        static public bool IsGreaterThan(TimeInfo cur, TimeInfo other)
        {
            return XMLHelper.IsGreaterThan(cur.Hour, cur.Minute, cur.Second, other.Hour, other.Minute, other.Second);
        }

        static public void PauseAutoSaveWhenValueChanged()
        {
            GameEntry.GetInstance<XMLHelper>()._isAutoSaveWhenValueChanged = false;
        }

        static public void ResumeAutoSaveWhenValueChanged()
        {
            GameEntry.GetInstance<XMLHelper>()._isAutoSaveWhenValueChanged = true;
        }

        static private XmlDocument GetXmlDocument(string xmlFilePath)
        { 
            if (!GameEntry.GetInstance<XMLHelper>()._mapXml.ContainsKey(xmlFilePath))
                return null;

            return GameEntry.GetInstance<XMLHelper>()._mapXml[xmlFilePath];
        }

        static private void SaveXml(XmlDocument document, string xmlFilePath)
        {
            var newFileTmp = xmlFilePath;
            var filePath = GetXmlStoragePathByFileName(newFileTmp);

            try
            {
                document.Save(filePath);
				Log.Info("Saved Xml file success: path = " + filePath);
            }
            catch (System.Exception ex)
            {
				Log.Error("SaveXml error: message = " + ex);
            }
        }

        static private string GetValueStringByElement(XmlElement element, string attribute = "value", string defaultValue= "")
        {
            if (element == null)
            {
				Log.Error("GetValueStringByElement error: not find element by element=" + element);
                return defaultValue;
            }
            else
            {
                if (GameEntry.GetInstance<XMLHelper>().OpenDebugMode)
                {
                    if (!element.HasAttribute(attribute))
                    {
						Log.Error("GetValueString error: not find atrribute=" + attribute + " in element=" + element.Name);
                        return defaultValue;
                    }
                    else
                    {
                        return element.GetAttribute(attribute);
                    }
                }
                else
                {
                    return element.GetAttribute(attribute);
                }
            }
        }

        static private void SetValueString(string xmlFilePath, XmlDocument document, XmlElement element, string attribute, string SetValue)
        {
            element.SetAttribute(attribute, SetValue);
            if (document != null && GameEntry.GetInstance<XMLHelper>()._isAutoSaveWhenValueChanged)
                SaveXml(document, xmlFilePath);
        }

        static private XmlDocument GetXmlDocumentWithAutoCreate(string xmlFilePath)
        {
            XmlDocument ret = GetXmlDocument(xmlFilePath);

            if (ret == null)
            {
                if (!GameEntry.GetInstance<XMLHelper>().LoadXML(xmlFilePath))
                {
					Log.Error("XMLHelper GetXmlDocumentWithAutoCreate error !");
                    return ret;
                }
            }

            ret = GetXmlDocument(xmlFilePath);
            return ret;
        }

        static private XmlElement SetElement(string xmlFilePath, string keyPath, ref XmlDocument document, bool autoNewElement)
        {
            XmlElement ret = null;
            document = GetXmlDocumentWithAutoCreate(xmlFilePath); 

            try
            {
                ret = document.SelectSingleNode(keyPath) as XmlElement;
            }
            catch (System.Exception ex)
            {
				Log.Error("SetElement error: keyPath=" + keyPath + " Exception=" + ex);
                return ret;
            }

            if (autoNewElement && null == ret)
            {
                ret = NewElement(document, keyPath);
            }

            return ret;
        }

        static private XmlElement SetElement(string xmlFilePath, string keyPath, int elementIndex, ref XmlDocument document, bool autoNewElement)
        {
            XmlElement ret = null;

            document = GetXmlDocumentWithAutoCreate(xmlFilePath);
            XmlElement parent = document.SelectSingleNode(keyPath) as XmlElement;

            if (parent == null)
            {
				Log.Error("can't find parent by keyPath = " + keyPath);
                return null;
            }

            var fistChild = parent.FirstChild as XmlElement;
            for (int i = 0; i < elementIndex; ++i)
            {
                if (fistChild == null)
                {
                    break;
                }
                else
                    fistChild = fistChild.NextSibling as XmlElement;
            }

            if (autoNewElement && fistChild == null)
                fistChild = NewElement(document, parent, elementIndex, "NewElement");

            ret = fistChild;

            return ret;
        }

        static private XmlElement NewElement(XmlDocument document, string keyPath)
        {
            XmlElement ret = null;
            XmlNode parent = null;
            string pathTmp = keyPath;
            List<XmlElement> listChild = new List<XmlElement>();

			var lastName = FileHelper.GetLastFileName(pathTmp, FileDefine.PATH_FLAG_SPLIT, false);
            ret = document.CreateElement(lastName);
            listChild.Add(ret);

            //set parent
            do
            {
                int flagIndex = pathTmp.LastIndexOf(FileDefine.PATH_FLAG_SPLIT);
                if (flagIndex == -1)
                {
                    break;
                }
                pathTmp = pathTmp.Remove(flagIndex, pathTmp.Length - flagIndex);
                var elementParent = GetElementByDocument(document, pathTmp);
                if (null != elementParent)
                {
                    parent = elementParent;
                    break;
                }
                else
                {
					lastName = FileHelper.GetLastFileName(pathTmp, FileDefine.PATH_FLAG_SPLIT, false);
                    listChild.Add(document.CreateElement(lastName));
                }

            } while (pathTmp.Length > 0);

            if (parent == null)
                parent = document.DocumentElement;

            for (int i = listChild.Count - 1; i >= 0; --i)
            {
                var childTmp = listChild[i];
                parent.AppendChild(childTmp);
                parent = childTmp;
            }

            return ret;
        }

        static private XmlElement NewElement(XmlDocument document, XmlElement parent, int elementIndex, string elementName)
        {
            XmlElement ret = document.CreateElement(elementName);

            var findElement = GetElementByIndex(parent, elementIndex);

            List<XmlElement> listElementTmp = new List<XmlElement>();
            while (null !=findElement)
            {
                listElementTmp.Add(findElement);
                parent.RemoveChild(findElement);
                findElement = findElement.NextSibling as XmlElement;
            }

            parent.AppendChild(ret);

            for (int i = 0; i < listElementTmp.Count; ++i)
                parent.AppendChild(listElementTmp[i]);

            return ret;
        }

        static private XmlElement GetElementByDocument(XmlDocument document, string keyPath)
        {
            XmlElement ret = null;
            if (string.IsNullOrEmpty(keyPath))
            {
                ret = document.FirstChild as XmlElement;
            }
            else
            {
                try
                {
                    ret = document.SelectSingleNode(keyPath) as XmlElement;
                }
                catch (System.Exception ex)
                {
					Log.Error("GetElement error: keyPath=" + keyPath + " Exception=" + ex);
                    return ret;
                }
            }
            return ret;
        }

        static private XmlElement GetElementByElement(XmlElement element, string keyPath)
        {
            XmlElement ret = null;
            if (string.IsNullOrEmpty(keyPath))
            {
                ret = element;
            }
            else
            {
                try
                {
                    ret = element.SelectSingleNode(keyPath) as XmlElement;
                }
                catch (System.Exception ex)
                {
					Log.Error("GetElement error: keyPath=" + keyPath + " Exception=" + ex);
                    return ret;
                }
            }
            return ret;
        }

        static private XmlElement GetElementByIndex(XmlElement parent, int elementIndex)
        {
            XmlElement ret = null;
            var firstChild = parent.FirstChild as XmlElement;
            for (int i = 0; i < elementIndex; ++i)
            {
                if (firstChild == null)
                {
                    break;
                }
                else
                    firstChild = firstChild.NextSibling as XmlElement;
            }

            ret = firstChild;
            return ret;
        }

        static private string GetXmlReadOnlyPathByFileName(string xmlFilePath)
        {
            var ret = Application.streamingAssetsPath + FileDefine.PATH_FLAG_SPLIT + xmlFilePath;
            return ret;
        }

        static private string GetXmlStoragePathByFileName(string xmlFilePath)
        {
            var ret = Application.persistentDataPath + FileDefine.PATH_FLAG_SPLIT + xmlFilePath;
            return ret;
        }
    }
}
