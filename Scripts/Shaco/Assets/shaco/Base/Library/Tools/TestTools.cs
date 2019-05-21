using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTools : MonoBehaviour
{
    public GameObject moveTarget;

    void Start()
    {
        var moveAction = shaco.MoveBy.Create(new Vector3(1, 0, 0), 0.2f);
        var actionTmp = shaco.Repeat.CreateRepeatForver(shaco.Sequeue.Create(moveAction, moveAction.Reverse()));
        actionTmp.RunAction(moveTarget.gameObject);
    }

    void OnGUI()
    {
        if (GUILayout.Button("Test Coroutine Foreach"))
        {
            var listTmp = new List<int>();
            for (int i = 0; i < 1000; ++i)
            {
                listTmp.Add(i);
            }
            shaco.Base.Coroutine.Foreach(listTmp, (object data) =>
            {
                System.Threading.Thread.Sleep(new System.TimeSpan(10000));
                return true;
            },
            (float percent) =>
            {
                Debug.Log("loading percent=" + percent);
            }, 0.001f);
        }

        if (GUILayout.Button("Test Coroutine Sequeue"))
        {
            float time = 0;
            float time2 = 0;
            var c1 = new shaco.Base.Coroutine.SequeueCallBack(() =>
            {
                Debug.Log("do1");
                time += Time.deltaTime;
            }, () =>
            {
                return time > 1.0;
            });
            var c2 = new shaco.Base.Coroutine.SequeueCallBack(() =>
            {
                Debug.Log("do2");
                time2 += Time.deltaTime;
            }, () =>
            {
                return time2 > 1.0;
            });
            shaco.Base.Coroutine.Sequeue(c1, c2);
        }

        if (GUILayout.Button("Test Coroutine While"))
        {
            int time = 0;
            shaco.Base.Coroutine.While(() =>
            {
                return time <= 1000;
            }, () =>
            {
                time += 1;
                Debug.Log("time=" + time);
                System.Threading.Thread.Sleep(5);
            }, 3);
        }

        if (GUILayout.Button("Test Zip File"))
        {
            var outputPath = Application.dataPath + "/Resources/package_tmp_1.zip";
            shaco.Base.ZipHelper.Zip(Application.dataPath + "/Resources/1.png", outputPath);
            shaco.Base.FileHelper.DeleteByUserPath(outputPath);
        }

        if (GUILayout.Button("Test Zip Directory"))
        {
            var outputPath = Application.dataPath + "/Resources/package_tmp_2.zip";
            shaco.Base.ZipHelper.Zip(shaco.Base.GlobalParams.GetShacoFrameworkRootPath().ContactPath("/Scripts/Unuse"), outputPath);
            shaco.Base.FileHelper.DeleteByUserPath(outputPath);
        }

        if (GUILayout.Button("Test UnZip"))
        {
            var outputPath = Application.dataPath + "/Resources/package_tmp_3";
            shaco.Base.ZipHelper.UnZip(Application.dataPath + "/Resources/package.zip", outputPath, (float progress) =>
            {
                Debug.Log("progress=" + progress);

                if (progress >= 1)
                {
                    shaco.Base.FileHelper.DeleteByUserPath(outputPath);
                }
            });
        }

        if (GUILayout.Button("Test Bad Words"))
        {
            shaco.Base.BadWordsFilter.LoadFromFile(Application.dataPath + "/Resources/Shielded font library.txt", (progress) =>
            {
                Debug.Log("p=" + progress);

                if (progress >= 1)
                {
                    Debug.Log("have bad word=" + shaco.Base.BadWordsFilter.HasBadWords("fuck---_*"));
                    Debug.Log("get bad word=" + shaco.Base.BadWordsFilter.Filter("fuck你好"));
                }
            });
            // shaco.Base.BadWordsFilter.LoadFromFile(Application.dataPath + "/Resources/Shielded font library.txt");
            // Debug.Log("have bad word=" + shaco.Base.BadWordsFilter.HasBadWords("fuck---_*"));
            // Debug.Log("get bad word=" + shaco.Base.BadWordsFilter.Filter("fuck你好"));
        }

        if (GUILayout.Button("Test Write File Async"))
        {
            var data = shaco.Base.FileHelper.ReadAllByUserPath(Application.dataPath.ContactPath("Resources/bad_words.txt"));
            shaco.Base.FileHelper.WriteAllByteByUserPathAsync("/Users/liuchang/Desktop/1.txt", data.ToByteArray(), false, (bool success) =>
            {
                Debug.Log("write end, success=" + success);
            }, (float percent) =>
            {
                shaco.Log.Info("write percent=" + percent);
            });
        }

        if (GUILayout.Button("Test Read File Async"))
        {
            // var data = shaco.Base.FileHelper.ReadAllByUserPath(Application.dataPath.ContactPath("Resources/bad_words.txt"));
            shaco.Base.FileHelper.ReadAllByteByUserPathAsync("/Users/liuchang/Desktop/1.txt", 0, 2561, (byte[] bytes) =>
            {
                if (!bytes.IsNullOrEmpty())
                {
                    Debug.Log("read end, bytes.Length=" + bytes.Length);

                    shaco.Base.FileHelper.ReadAllByteByUserPathAsync("/Users/liuchang/Desktop/1.txt", 2561, 2000, (byte[] bytes2) =>
                    {
                        if (!bytes2.IsNullOrEmpty())
                        {
                            Debug.Log("read end, bytes.Length2=" + bytes2.Length);

                            var list = new List<byte>();
                            list.AddRange(bytes);
                            list.AddRange(bytes2);
                            shaco.Base.FileHelper.WriteAllByteByUserPath("/Users/liuchang/Desktop/2.txt", list.ToArray());
                        }

                    }, (float percent) =>
                    {
                        shaco.Log.Info("read percent2=" + percent);
                    });
                }

            }, (float percent) =>
            {
                shaco.Log.Info("read percent=" + percent);
            });
        }
    }
}
