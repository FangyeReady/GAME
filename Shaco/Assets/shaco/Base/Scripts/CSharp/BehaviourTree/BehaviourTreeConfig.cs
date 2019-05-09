using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class BehaviourTreeConfig
    {
        public const int INDEX_CHILDREN_ARRAY = 2;

        public class JsonInfo
        {
            public shaco.LitJson.JsonData jsonDataArray;
            private int dataIndex = 0;

            public string GetNextData()
            {
                if (dataIndex < 0 || dataIndex > jsonDataArray.Count - 1)
                {
                    Log.Error("BaseTree GetJsonStringInOrder error: out of range index=" + dataIndex + " count=" + jsonDataArray.Count);
                    return string.Empty;
                }
                else
                {
                    return jsonDataArray[dataIndex++].ToString();
                }
            }
        }

        static public bool LoadFromJsonPath(string path, BehaviourRootTree tree)
        {
            var jsonRead = FileHelper.ReadAllByUserPath(path);
            if (string.IsNullOrEmpty(jsonRead))
            {
                Log.Error("BehaviourTreeConfig InitWithJsonPath error: not find file by path=" + path);
                return false;
            }

            return LoadFromJson(jsonRead, tree);
        }

        static public bool LoadFromJson(string json, BehaviourRootTree tree)
        {
            if (string.IsNullOrEmpty(json))
            {
                Log.Error("BehaviourTreeConfig LoadFromJson error: json is empty !");
                return false;
            }

            if (null == tree)
            {
                Log.Error("BehaviourTreeConfig LoadFromJson error: tree is null!");
                return false;
            }

            //重置树节点数据
            tree.RemoveChildren();

            var jsonObjet = shaco.LitJson.JsonMapper.ToObject(json)[0];

            var typeName = jsonObjet[0].ToString();
            if (typeName != tree.GetType().FullName)
            {
                Log.Error("BehaviourTreeConfig LoadFromJson error: not BehaviourTree !");
                return false;
            }

            tree.FromJson(new JsonInfo() { jsonDataArray = jsonObjet });
            GetJsonArray(tree, jsonObjet);
            return true;
        }

        //format: [0]: type name
        //        [1]: name
        //        [2]: children     -> If no child, this field is not exist
        static public bool SaveToJson(BehaviourRootTree root, string path)
        {
            var jsonParent = new shaco.LitJson.JsonData();
            var jsonRoot = jsonParent;

            //set root
            var jsonData = BehaviourTreeToJsonArray(root);
            if (null != jsonData)
            {
                jsonParent.Add(jsonData);

                if (jsonData.Count > BehaviourTreeConfig.INDEX_CHILDREN_ARRAY)
                    jsonParent = jsonData[BehaviourTreeConfig.INDEX_CHILDREN_ARRAY];

                //set children
                SetJsonArray(root, jsonParent);
                FileHelper.WriteAllByUserPath(path, jsonRoot.ToJson());
            }

            return true;
        }

        static private shaco.LitJson.JsonData BehaviourTreeToJsonArray(BehaviourTree tree)
        {
            shaco.LitJson.JsonData retValue = null;
            var jsonArray = tree.ToJson();
            if (jsonArray.Count == 0)
                return retValue;
            else
                retValue = new shaco.LitJson.JsonData();

            for (int i = 0; i < jsonArray.Count; ++i)
            {
                retValue.Add(jsonArray[i]);
            }

            if (retValue.Count > BehaviourTreeConfig.INDEX_CHILDREN_ARRAY && string.IsNullOrEmpty(retValue[BehaviourTreeConfig.INDEX_CHILDREN_ARRAY].ToString()))
            {
                retValue[BehaviourTreeConfig.INDEX_CHILDREN_ARRAY].SetJsonType(shaco.LitJson.JsonType.Array);
            }

            return retValue;
        }

        static private void SetJsonArray(BehaviourTree parentTree, shaco.LitJson.JsonData parentJson)
        {
            parentTree.ForeachChildren((BehaviourTree tree) =>
            {
                var jsonData = BehaviourTreeToJsonArray(tree);
                if (null != jsonData)
                {
                    parentJson.Add(jsonData);
                    if (tree.child != null && jsonData.Count > BehaviourTreeConfig.INDEX_CHILDREN_ARRAY)
                        SetJsonArray(tree, jsonData[BehaviourTreeConfig.INDEX_CHILDREN_ARRAY]);
                }
                return true;
            });
        }

        static private void GetJsonArray(BehaviourTree parentTree, shaco.LitJson.JsonData parentJson)
        {
            if (parentJson.Count > BehaviourTreeConfig.INDEX_CHILDREN_ARRAY && parentJson[BehaviourTreeConfig.INDEX_CHILDREN_ARRAY].IsArray)
            {
                var jsonArray = parentJson[BehaviourTreeConfig.INDEX_CHILDREN_ARRAY];
                int count = jsonArray.Count;
                for (int i = 0; i < count; ++i)
                {
                    var arrayTmp = jsonArray[i];
                    var typeName = arrayTmp[0].ToString();
                    var newTree = (BehaviourTree)parentTree.GetType().Assembly.CreateInstance(typeName);
                    newTree.FromJson(new JsonInfo() { jsonDataArray = arrayTmp });
                    parentTree.AddChild(newTree);
                }

                var child = parentTree.child;
                for (int i = 0; i < count; ++i)
                {
                    GetJsonArray(child as BehaviourTree, jsonArray[i]);
                    child = child.next;
                }
            }
        }
    }
}

