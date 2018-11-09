
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;



public class Main : MonoBehaviour
{
    public EntityManager entityManager;
    public EntityArchetype entityArchetype;
    public int createPrefabCout;
    public GameObject prefab;
    public Mesh mesh;
    public Material material;

    // Use this for initialization
    void Start()
    {
        //创建实体管理器
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        //创建基础组件的原型
        entityArchetype = entityManager.CreateArchetype(typeof(Position), typeof(Rotation), typeof(RotationSpeed));
        if (prefab)
        {
            for (int i = 0; i < createPrefabCout; i++)
            {
                //创建实体
                Entity entities = entityManager.CreateEntity(entityArchetype);
                //设置组件
                entityManager.SetComponentData(entities, new Position { Value = UnityEngine.Random.insideUnitSphere * 100 });
                entityManager.SetComponentData(entities, new Rotation { Value = quaternion.identity });
                entityManager.SetComponentData(entities, new RotationSpeed { value = 100 });
                    
                //添加并设置组件
                entityManager.AddSharedComponentData(entities, new MeshInstanceRenderer
                {
                    mesh = this.mesh,
                    material = this.material,
                });
            }

            //NativeArray<Entity> entityArray = new NativeArray<Entity>(createPrefabCout, Allocator.Temp);
            //for (int i = 0; i < createPrefabCout; i++)
            //{
            //    entityManager.Instantiate(prefab, entityArray);
            //    entityManager.SetComponentData(entityArray[i], new Position { Value = UnityEngine.Random.insideUnitSphere*10 });
            //    entityManager.AddSharedComponentData(entityArray[i], new MeshInstanceRenderer
            //    {
            //        mesh = this.mesh,
            //        material = this.material,
            //    });
            //}
            //entityArray.Dispose();

        }

    }

}
