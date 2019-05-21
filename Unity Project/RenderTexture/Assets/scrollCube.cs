using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrollCube : MonoBehaviour
{
    public GameObject cubePrefab;
    public RenderTexture texture;

    private List<GameObject> cubeList = new List<GameObject>();

    private Vector3[] vector3s = new Vector3[] {

        new Vector3(0,0,0),
        new Vector3(-2,0,0),
        new Vector3(-4,0,0)

    };
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = Instantiate(cubePrefab, this.transform);
            obj.transform.localPosition = vector3s[i];
            obj.SetActive(true);
            var render = obj.GetComponent<MeshRenderer>();
            render.sharedMaterial.color = new Color(Random.Range(0, 255f) / 255, Random.Range(0, 255f) / 255, Random.Range(0, 255f) / 255);
            cubeList.Add(obj);
        }


        StartCoroutine("ScrollCubePos");
    }


    private float speed = 1f;
    IEnumerator ScrollCubePos()
    {
        while (true)
        {
            yield return null;
            for (int i = 0; i < cubeList.Count; i++)
            {
                Vector3 nowPos = cubeList[i].transform.localPosition;
                Vector3 newPos = nowPos + new Vector3(2, 0, 0);
                if (newPos.x >= 4)
                {
                    cubeList[i].transform.localPosition = new Vector3(-4, 0, 0);
                    continue;
                }
                float startTime = Time.time;
                float length = Vector3.Distance(cubeList[i].transform.localPosition, newPos);
                float frac = 0f;
                while (frac < 1.0f)
                {
                    float dist = (Time.time - startTime) * speed;
                    frac = dist / length;
                    cubeList[i].transform.localPosition = Vector3.Lerp(cubeList[i].transform.localPosition, newPos, frac);
                    yield return null;
                    texture.Release();
                }

            }
        }


    }

   
}
