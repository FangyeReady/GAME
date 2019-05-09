using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using System.Threading;

namespace Singleton
{
    #region 单例模式
    //=============================================单例===================================================
    public class CLS {
        public virtual void Print( CLS instance )
        {
            Console.WriteLine("CODE:" + instance.GetHashCode());
        }
    }
    /// <summary>
    /// 单例1
    /// </summary>
    public class CLS1 :CLS 
    {
        private static CLS1 instance = new CLS1();
        private CLS1()
        {
            Console.WriteLine("私有构造" + typeof(CLS1));
        }

        public static CLS1 getInstance()
        {
            return instance;
        }

    }

    /// <summary>
    /// 单例2
    /// </summary>
    public class CLS2 :CLS
    {
        private static CLS2 _instance;
        public static CLS2 Instance {
            get {
                if (null == _instance)
                {
                    _instance = new CLS2();
                }
                return _instance;
            }
        }


        private CLS2()
        {
            Console.WriteLine("私有构造" + typeof(CLS2));
        }

    }
    #endregion

    #region Build模式
    //=============================================Build===================================================
    /// <summary>
    /// Monster对象
    /// </summary>
    public abstract class Monster   
    {
        protected string _name;
        protected string _flag;

        public void Print()
        {
            Console.WriteLine("Monster name is ----->:" + this._name);
            Console.WriteLine("Monster flag is ----->:" + this._flag);
        }

        public abstract void SetData(string name, string flag);

        
    }


    /// <summary>
    /// Monster的构造者，利用Monster的数据来构造特定的Monster
    /// </summary>
    public abstract class MonsterBuilder
    {
        public MonsterBuilder(string name, string flag) {

            data.name = name;
            data.flag = flag;
        }

        public struct BuilData
        {
            public string name;
            public string flag;
        }
        protected BuilData data;
        public abstract Monster CreateMonster();
    }

    public class DamageMonster : Monster
    {
        public override void SetData(string name, string flag)
        {
            this._name = name;
            this._flag = flag;
        }

    }

    /// <summary>
    /// 此处有疑问，构造者模式应该是同类型的数据的不同导致的构造出来的对象的不同，那么builder应该可以通过改变数据的不同而构造出不同的产品
    /// </summary>
    public class DamageMonsterBuilder : MonsterBuilder
    {

        public DamageMonsterBuilder(string name, string flag) : base(name, flag) { }

        public override Monster CreateMonster()
        {
            var damageMonster = new DamageMonster();
            damageMonster.SetData(data.name, data.flag);
            return damageMonster;
        }
    }


    public class Master
    {
        private Master()
        {
            Console.WriteLine("MASTER 的 私有构造！");
        }

        private static Master _instance;
        public static Master Instance {
            get {
                if (null == _instance)
                {
                    _instance = new Master();
                }
                return _instance;
            }
        }

        public Monster CatchMoster(MonsterBuilder monsterBuilder)
        {
           return  monsterBuilder.CreateMonster();
        }

    }

    #endregion

    #region 工厂模式
    //=============================================工厂===================================================
    //目的：工厂父类定义一个创建对象的接口，让其子类（子工厂）自己决定实例化哪一个产品，工厂模式使其创建过程延迟到子类进行。
    //      Factory----->XXXFactory----->Product

    public enum ProductType
    {
        LittleCat = 0,
        BogDog,
    }

    /// <summary>
    /// 产品
    /// </summary>
    public abstract class Product {

        public abstract void Show();

        public virtual void Print()
        {
            Console.WriteLine("this is a product");
        }
    }

    /// <summary>
    /// 工厂
    /// </summary>
    public abstract class Factory
    {
        public abstract Product CreateProduct(ProductType type);//当然也可以有其它参数
    }

    /// <summary>
    /// 具体的产品
    /// </summary>
    public class Cat : Product
    {
        public override void Show()
        {
            Console.WriteLine("~~~~~cat show!~~~~~~");
        }
    }

    public class Dog : Product
    {
        public override void Show()
        {
            Console.WriteLine("~~~~~dog show!~~~~~");
        }
    }

    /// <summary>
    ///实现具体的实例构造过程
    /// </summary>
    public class AnimalFactory:Factory
    {
        public override Product CreateProduct( ProductType type )
        {
            Product product = null;

            switch (type)
            {
                case ProductType.LittleCat: product = new Cat();  //这里可以设计为传一个参数（数据）进去
                    break;
                case ProductType.BogDog: product = new Dog();
                    break;
                default:
                    break;
            }

            return product;      
        }
    }



    #endregion

    #region 抽象工厂

    public enum FacType {
        items,
        animal,
        childs
    }

    /// <summary>
    /// 总工厂，提供一个接口用于得到需要的产品（根据实际情况扩展）
    /// </summary>
    public abstract class IFactory
    {
        public abstract IProduct CreateProduct(Items type);  //product type
        
    }

    /// <summary>
    /// 产品，提供一个外部接口用于得到需要的产品的嘻嘻（根据实际情况扩展初始化等接口  甚至不需要提供外部接口 ）
    /// </summary>
    public abstract class IProduct
    {
        public abstract void ShowInfo();
    }

    /// <summary>
    /// 总工厂，用于生成子工厂
    /// </summary>
    public class MainFactory
    {
        public static IFactory CreateFactory(FacType type)
        {
            IFactory factory = null;
            switch (type)
            {
                case FacType.items: factory = new ListItemsFactory();
                    break;
                case FacType.animal:
                    break;
                case FacType.childs:
                    break;
                default:
                    break;
            }
            return factory;
        }
    }

    //test:
    public enum Items {
        shopItems,
        equipItems,
        playerItems,
    }

    public class ShopItems : IProduct
    {
        public override void ShowInfo()
        {
            Console.WriteLine("这个是ShopItems~~~!");
        }
    }

    public class EquipItems : IProduct
    {
        public override void ShowInfo()
        {
            Console.WriteLine("这个是EquipItems~~~!");
        }
    }

    public class PlayerItems : IProduct
    {
        public override void ShowInfo()
        {
            Console.WriteLine("这个是PlayerItems~~~!");
        }
    }

    public class ListItemsFactory : IFactory
    {
        public override IProduct CreateProduct(Items type)
        {
            IProduct product = null;
            switch (type)
            {
                case Items.shopItems: product = new ShopItems();
                    break;
                case Items.equipItems:product = new EquipItems();
                    break;
                case Items.playerItems:product = new PlayerItems(); 
                    break;
                default:
                    break;
            }

            return product;
        }
    }


    #endregion

    #region 原型模式

    public abstract class ColorPrototype {

        public abstract ColorPrototype Clone();
    }

    public class Colors : ColorPrototype
    {
        private int _red;
        private int _green;
        private int _blue;

        public Colors(int red, int green, int blue)
        {
            this._red = red;
            this._green = green;
            this._blue = blue;
        }

        public override ColorPrototype Clone()
        {
            Console.WriteLine("克隆的颜色：  {0,3},{1,3},{2,3}", _red, _green, _blue);
            return this.MemberwiseClone() as ColorPrototype;
        }
    }


    public class ColorManager
    {
        private Dictionary<string, ColorPrototype> _colorDic = new Dictionary<string, ColorPrototype>();

        public ColorPrototype this[string key]
        {
            get { return _colorDic[key]; }
            set { _colorDic[key] = value; }
        }
    }


    #endregion

    #region 适配器模式

    public enum AudioType
    {
        MP3,
        MP4,
        FLAC,
    }

    /// <summary>
    /// 播放器，适配器都需要继承的类
    /// </summary>
    public abstract class MediaPlayer
    {
        public abstract void PlayMusic(AudioType type, string audio);
    }

    /// <summary>
    /// 牛逼一点的播放器
    /// </summary>
    public class SuperMediaPlayer : MediaPlayer
    {

        protected void PlayMP4(string audio)
        {
            Console.WriteLine("Play mp4 music:  {0,1}", audio);
        }

        protected void PlayFlac(string audio)
        {
            Console.WriteLine("Play flac music:  {0,1}", audio);
        }

        public override void PlayMusic(AudioType type, string audio)
        {
            switch (type)
            {
                case AudioType.MP4: PlayMP4(audio);
                    break;
                case AudioType.FLAC: PlayFlac(audio);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 适配器, 针对不同的需求，会有不同的适配器
    /// </summary>
    public class MediaAdapter : MediaPlayer
    {
        SuperMediaPlayer superMedia;
        public MediaAdapter()
        {
            superMedia = new SuperMediaPlayer();
        }

        public override void PlayMusic(AudioType type, string audio)
        {
            superMedia.PlayMusic(type, audio);
        }
    }

    /// <summary>
    /// 弱鸡一点的播放器
    /// </summary>
    public class AudioPlayer : MediaPlayer
    {
        MediaAdapter mediaAdapter;

        public AudioPlayer()
        {
            mediaAdapter = new MediaAdapter();
        }

        protected void PlayMp3(string audio)
        {
            Console.WriteLine("Play mp3 music:  {0,1}", audio);
        }

        public override void PlayMusic(AudioType type, string audio)
        {
            switch (type)
            {
                case AudioType.MP3:PlayMp3(audio);
                    break;
                case AudioType.MP4:mediaAdapter.PlayMusic(type, audio);
                    break;
                case AudioType.FLAC:mediaAdapter.PlayMusic(type, audio);
                    break;
                default:
                    break;
            }
        }
    }


    #endregion

    #region 桥接模式

    public abstract class DarwApi
    {
        public abstract void Darw(int x, int y, int redius = 0);
    }

    public class Shape
    {
        private DarwApi darwApi;
        public Shape(DarwApi api)
        {
            this.darwApi = api;
        }

        public void Darw(int x, int y, int redius)
        {
            this.darwApi.Darw(x,y,redius);
        }
    }


    public class DrawCircle : DarwApi
    {
        public override void Darw(int x, int y, int redius = 0)
        {
            Console.WriteLine("DRAW 圆形：x:{0}  y:{1}  r{2}", x, y, redius);
        }
    }

    public class DrawRetengle : DarwApi
    {
        public override void Darw(int x, int y, int redius = 0)
        {
            Console.WriteLine("DRAW 长方形：x:{0}  y:{1}  r{2}", x, y, redius);
        }
    }

    #endregion

    #region 过滤器模式

    /// <summary>
    /// 用于过滤（筛选）Person的API
    /// </summary>
    public interface PersonGuoLvApi
    {
        List<PerSon> GuoLv(List<PerSon> list);
    }

    public abstract class ProtoptypeofGuoLv
    {
        protected string _tiaoJian;
        public ProtoptypeofGuoLv(string tiaojian)
        {
            this._tiaoJian = tiaojian;
        }

        public void SetTiaoJian(string t)
        {
            this._tiaoJian = t;
        }

        public abstract List<PerSon> GuoLvMethod(List<PerSon> list);

        public abstract ProtoptypeofGuoLv Clone();
    }

    public class PerSon
    {
        private string _name;
        private string _gender;
        private string _age;

        public PerSon(string name, string gender, string age)
        {
            this._name = name;
            this._gender = gender;
            this._age = age;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Gender
        {
            get { return _gender; }
        }

        public string Age
        {
            get { return _age; }
        }

        public void PrintInfo()
        {
            Console.WriteLine("Name:{0}, Gender:{1}, Age:{2}", this._name, this._gender, this._age);
        }
    }

    public class SelectPersonBy_Gender : ProtoptypeofGuoLv  //, PersonGuoLvApi   //基类必须在所有接口之前
    { 

        public SelectPersonBy_Gender(string t) : base(t) { }

        public override List<PerSon> GuoLvMethod(List<PerSon> perSons)
        {
            List<PerSon> newList = perSons.FindAll(t => t.Gender == this._tiaoJian);
            return newList;
        }


        public override ProtoptypeofGuoLv Clone()
        {
            //SelectPersonBy_Gender temp = new SelectPersonBy_Gender(this._tiaoJian);

            //return temp as ProtoptypeofGuoLv;

            return this.MemberwiseClone() as ProtoptypeofGuoLv;
        }
    }

    public class SelectPersonBy_Age : ProtoptypeofGuoLv  //, PersonGuoLvApi   //基类必须在所有接口之前
    {

        private int age_tiaojian = 0;
        public SelectPersonBy_Age(string t) : base(t) {
            age_tiaojian = int.Parse(t);
        }

        public override List<PerSon> GuoLvMethod(List<PerSon> perSons)
        {
            List<PerSon> temp = new List<PerSon>();
            for (int i = 0; i < perSons.Count; i++)
            {
                int age1 = int.Parse(perSons[i].Age);
                if (age1 <= age_tiaojian)
                {
                    temp.Add(perSons[i]);
                }
            }

            return temp;
        }


        public override ProtoptypeofGuoLv Clone()
        {
            //SelectPersonBy_Gender temp = new SelectPersonBy_Gender(this._tiaoJian);

            //return temp as ProtoptypeofGuoLv;

            return this.MemberwiseClone() as ProtoptypeofGuoLv;
        }
    }




    #endregion

    #region 合作模式

    /// <summary>
    /// 此处的抽象其实毫无意义。。。。。。
    /// </summary>
    public abstract class FOperate
    {
        protected int depth;
        protected string name;
        public string GetName() { return name; }
        public int GetDepth() { return depth; }
        public int GetCount() { return structList.Count; }
        protected List<FOperate> structList;
        public abstract void Add(FOperate operate);
        public abstract void PrintConstact();
    }

    public class SWorker : FOperate
    {
        public SWorker(string name, int depth)
        {
            this.name = name;
            this.depth = depth;
            structList = new List<FOperate>();
        }

        public override void Add(FOperate operate)
        {
            structList.Add(operate);
        }

        public override void PrintConstact()
        {
            for (int i = 0; i < structList.Count; i++)
            {
                Console.WriteLine("Name:{0}-----Depth:{1}", structList[i].GetName(), structList[i].GetDepth());
                if (structList[i].GetCount() > 0)
                {
                    structList[i].PrintConstact();
                }
            }
        }
    }

    #endregion


    class Program
    {
        static void Main(string[] args)
        {

            //--------------------------------------------------------单例--------------------------------------------------------------------------------
            //目的：全局有且只有一个该类

            //test:

            //CLS1 cLS1 = CLS1.getInstance();
            //CLS1 cLS2 = CLS1.getInstance();
            //cLS1.Print(cLS1);
            //cLS2.Print(cLS2);

            //CLS cLS3 = CLS2.Instance;
            //CLS cLS4 = CLS2.Instance;
            //cLS3.Print(cLS3);
            //cLS4.Print(cLS4);

            //--------------------------------------------------------Build : 适用于以前写的createItems之类的----------------------------------------------

            //目的:1. 将一个复杂对象的构造与它的表示分离，使得同样的构造过程可以创建不同的表示。
            //     2. 当构造过程必须允许被构造的对象有不同的表示时 : 同样的构造过程，不同的对象


            //例： shopItems,  playerItems, equipmentItems....在 InitMonster() 初始化中可以指定prefab, 图片等等

            //对比：为什么不每个对象单独一个类，比如红玉类，酒狐类等？  

            //其实这种方法也是单独写了一个类（builder），只不过Build模式将所有的数据都管理了起来，并且只适用于所有item都有一模一样的数据类型的情况

            //如果有很大的特异性的类，那就不要用Build模式

            //PS1：此处貌似可以不用创建那么多MonsterBuilder_Hongyu之类的，可以直接Monster， 当然如果有一些有特殊需求，仍可以这样做
            //PS2: 此处如果只用Monster可能会有一个问题，就是如果以后我有新增的特殊构造需求时，就需要更改Monster的代码，这违背了面向对象“只新增，不修改”的原则
            //    所以此处用MonsterBuilder_Hongyu是有道理的~~~~


            //test:
            //DamageMonsterBuilder damageMonsterBuilder = new DamageMonsterBuilder("红玉", "金系输出~！");
            //Monster monster = Master.Instance.CatchMoster(damageMonsterBuilder);
            //monster.Print();



            //--------------------------------------------------------工厂：创建复杂的类----------------------------------------------
            //test:
            //AnimalFactory animalFactory = new AnimalFactory();
            //Product cat = animalFactory.CreateProduct(ProductType.LittleCat);
            //cat.Show();
            //cat.Print();

            //Product dog = animalFactory.CreateProduct(ProductType.BogDog);
            //dog.Show();
            //dog.Print();


            //--------------------------------------------------------抽象工厂：创建复杂的一族类----------------------------------------------
            //如果需要扩展其它的工厂，则MainFactor需要新增代码，enum需要新增type(这样可能导致某个枚举变得特别大~~~~)
            //工厂和抽象工厂的区别就是：抽象工厂多了一个工厂生成器~~~~
            //抽象工厂模式和构建者（Builder）模式的区别:1.抽象工厂更关注整体，即一个工厂生产指定的一系列产品；Builder更关注细节，根据不同的蓝图（数据类型）生产出不同的产品。
            //                                       2.工厂构造产品就是通过工厂（或一个工厂的工厂创建出的工厂），而Builder需要一个Designer(Master)根据提供的不同Builder(包含了数据类型和特顶的方法)来构建。
            //                                       3.抽象工厂的封装性很好，一般只需要提供一个接口即可。而Builder模式则更灵活（Builder由用户自己定义），更易扩展（每次新增都新增一个Builder）即可。
            //IFactory factory = MainFactory.CreateFactory(FacType.items);
            //IProduct product = factory.CreateProduct(Items.equipItems);
            //product.ShowInfo();


            //--------------------------------------------------------原型模式：复制一个类----------------------------------------------
            //当你需要创建一个一模一样的对象而又不能影响已有的对象时，可以使用原型模式
            //原型模式的最重要缺点就是每一个类必须配备一个Clone方法，而且这个Clone方法需要对类的功能进行通盘考虑。这对全新的类来说不是很难，但对已有的类进行改造时，不一定是容易的事。
            //浅复制：在C#中调用 MemberwiseClone() 方法即为浅复制。如果字段是值类型的，则对字段执行逐位复制，如果字段是引用类型的，则复制对象的引用，而不复制对象，因此：原始对象和其副本引用同一个对象！
            //深复制：如果字段是值类型的，则对字段执行逐位复制，如果字段是引用类型的，则把引用类型的对象指向一个全新的对象！
            //ColorManager colorManager = new ColorManager();
            //colorManager["red"] = new Colors(255, 0, 0);
            //colorManager["green"] = new Colors(0, 255, 0);
            //colorManager["blue"] = new Colors(0, 0, 255);

            //Colors color = colorManager["red"].Clone() as Colors;

            //Console.WriteLine(colorManager["red"].GetHashCode());
            //Console.WriteLine(color.GetHashCode());


            //--------------------------------------------------------适配器模式：具体操作的什么类，由适配器决定----------------------------------------------
            //AudioPlayer audioPlayer = new AudioPlayer();
            //audioPlayer.PlayMusic(AudioType.MP3, "等你下课");
            //audioPlayer.PlayMusic(AudioType.MP4, "PRAY");
            //audioPlayer.PlayMusic(AudioType.FLAC, "My Immortal");



            //Shape shape = new Shape(new DrawCircle());
            //shape.Darw(10, 10, 10);

            //shape = new Shape(new DrawRetengle());
            //shape.Darw(1,1,1);

            //--------------------------------------------------------过滤器模式 + 原型模式：....----------------------------------------------
            //创建一个存在过滤方法的类，然后。。。使用它
            //List<PerSon> perSons = new List<PerSon>();
            //perSons.Add(new PerSon("1", "male", "10"));
            //perSons.Add(new PerSon("2", "male", "20"));
            //perSons.Add(new PerSon("3", "male", "30"));
            //perSons.Add(new PerSon("4", "male", "40"));
            //perSons.Add(new PerSon("5", "male", "50"));
            //perSons.Add(new PerSon("6", "female", "60"));
            //perSons.Add(new PerSon("7", "female", "10"));
            //perSons.Add(new PerSon("8", "female", "20"));
            //perSons.Add(new PerSon("9", "female", "30"));
            //perSons.Add(new PerSon("10", "male", "40"));


            //ProtoptypeofGuoLv guoLv1 = new SelectPersonBy_Gender("male");
            //var male_list = guoLv1.GuoLvMethod(perSons);

            //PrintPersonList(male_list);

            //ProtoptypeofGuoLv guolv2 = guoLv1.Clone();

            //Console.WriteLine(guoLv1.GetHashCode() + "-----------" + guolv2.GetHashCode());

            //guolv2.SetTiaoJian("female");

            //var female_list = guolv2.GuoLvMethod(perSons);

            //PrintPersonList(female_list);

            //Console.WriteLine("<---------------------------------------------------------------->");

            //ProtoptypeofGuoLv guoLv3 = new SelectPersonBy_Age("50");
            //var age_list =  guoLv3.GuoLvMethod(perSons);
            //PrintPersonList(age_list);

            //--------------------------------------------------------组合器模式：为了表达出同一类对象的层次结构----------------------------------------------
            SWorker sWorker = new SWorker("老板", 1);

            SWorker sWorker1 = new SWorker("市场主管", 2);
            SWorker sWorker2 = new SWorker("人事主任", 2);

            SWorker sWorker3 = new SWorker("市场经理1", 3);
            SWorker sWorker4 = new SWorker("市场经理2", 3);

            SWorker sWorker5 = new SWorker("人事专员1", 3);
            SWorker sWorker6 = new SWorker("人事专员2", 3);

            SWorker sWorker7 = new SWorker("码农1", 4);
            SWorker sWorker8 = new SWorker("码农2", 4);
            SWorker sWorker9 = new SWorker("码农3", 4);
            SWorker sWorker10 = new SWorker("码农4", 4);

            sWorker.Add(sWorker1);
            sWorker.Add(sWorker2);

            sWorker1.Add(sWorker3);
            sWorker1.Add(sWorker4);

            sWorker2.Add(sWorker5);
            sWorker2.Add(sWorker6);

            sWorker3.Add(sWorker7);
            sWorker4.Add(sWorker8);
            sWorker4.Add(sWorker9);
            sWorker4.Add(sWorker10);


            sWorker.PrintConstact();




            Console.ReadKey();
        }

        public static void PrintPersonList(List<PerSon> perSons)
        {
            for (int i = 0; i < perSons.Count; i++)
            {
                perSons[i].PrintInfo();
            }
        }
    }
}
