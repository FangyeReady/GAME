using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MyClass
{
    public int val = 10;
}

public class Earth : MonoBehaviour
{
    [SerializeField] Transform Sun;

    [Range(1, 99)]
    [SerializeField] float speed = 1;


    delegate void CalcFunc(ref MyClass mc);
    private CalcFunc clcFunc;

    private void Start() {

        clcFunc =  (ref MyClass f1) => print("Lambda:" + f1.val);

        MyClass mc = new MyClass();
        print("mc bf:" + mc.val+ "  " + mc.GetHashCode());
        clcFunc(ref mc);
        //Calc(ref mc);
        print("mc af:" + mc.val + "  " + mc.GetHashCode());
    }

    void Update()
    {
        transform.RotateAround(Sun.position, Sun.up, speed * Time.deltaTime);    
    }



    private void Calc(MyClass mc) //引用类型  作为 值参数  传递，不会修改引用本身，可以修改成员变量
    {
        mc.val += 9;
        print("mc in method~:" + mc.val + "  " + mc.GetHashCode());
        mc = new MyClass();//退出方法后该内存块会被设为垃圾内存
        print("mc in method~New:" + mc.val + "  " + mc.GetHashCode());  
    }


    private void Calc(ref MyClass mc)//引用类型  作为 引用参数  传递，会修改引用本身，也可以修改成员变量
    {
        mc.val += 9;
        print("mc in method~:" + mc.val + "  " + mc.GetHashCode());
        mc = new MyClass();//此时外面的引用和此处的已经都指向的一个新的地址
        print("mc in method~New:" + mc.val + "  " + mc.GetHashCode());
    }

}
