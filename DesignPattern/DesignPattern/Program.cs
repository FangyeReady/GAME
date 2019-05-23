using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using System.Threading;
using System.Collections;

namespace DesignPattern
{
    #region 单例模式
    //=============================================单例===================================================
    public class CLS
    {
        public virtual void Print(CLS instance)
        {
            Console.WriteLine("CODE:" + instance.GetHashCode());
        }
    }
    /// <summary>
    /// 单例1
    /// </summary>
    public class CLS1 : CLS
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
    public class CLS2 : CLS
    {
        private static CLS2 _instance;
        public static CLS2 Instance
        {
            get
            {
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
        public MonsterBuilder(string name, string flag)
        {

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
        public static Master Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new Master();
                }
                return _instance;
            }
        }

        public Monster CatchMoster(MonsterBuilder monsterBuilder)
        {
            return monsterBuilder.CreateMonster();
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
    public abstract class Product
    {

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
    public class AnimalFactory : Factory
    {
        public override Product CreateProduct(ProductType type)
        {
            Product product = null;

            switch (type)
            {
                case ProductType.LittleCat:
                    product = new Cat();  //这里可以设计为传一个参数（数据）进去
                    break;
                case ProductType.BogDog:
                    product = new Dog();
                    break;
                default:
                    break;
            }

            return product;
        }
    }



    #endregion

    #region 抽象工厂

    public enum FacType
    {
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
    /// 产品，提供一个外部接口用于得到需要的产品（根据实际情况扩展初始化等接口  甚至不需要提供外部接口 ）
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
                case FacType.items:
                    factory = new ListItemsFactory();
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
    public enum Items
    {
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
                case Items.shopItems:
                    product = new ShopItems();
                    break;
                case Items.equipItems:
                    product = new EquipItems();
                    break;
                case Items.playerItems:
                    product = new PlayerItems();
                    break;
                default:
                    break;
            }

            return product;
        }
    }


    #endregion

    #region 原型模式

    public abstract class ColorPrototype
    {

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
                case AudioType.MP4:
                    PlayMP4(audio);
                    break;
                case AudioType.FLAC:
                    PlayFlac(audio);
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
                case AudioType.MP3:
                    PlayMp3(audio);
                    break;
                case AudioType.MP4:
                    mediaAdapter.PlayMusic(type, audio);
                    break;
                case AudioType.FLAC:
                    mediaAdapter.PlayMusic(type, audio);
                    break;
                default:
                    break;
            }
        }
    }


    #endregion

    #region 桥接模式

    /// <summary>
    /// 抽象类
    /// </summary>
    public abstract class DarwApi
    {
        public abstract void Darw(int x, int y, int redius = 0);
    }

    /// <summary>
    /// 实现类：其中包含了抽象对象，用于调用其方法
    /// </summary>
    public class Shape
    {
        private DarwApi darwApi;
        public Shape(DarwApi api)
        {
            this.darwApi = api;
        }

        public void Darw(int x, int y, int redius)
        {
            this.darwApi.Darw(x, y, redius);
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
        public SelectPersonBy_Age(string t) : base(t)
        {
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

    #region 装饰器模式
    public abstract class Computer
    {
        protected string _madeFrom;
        protected string _company;
        protected string _cpu;
        protected string _gpu;

        public abstract void ShowInfo();
    }


    public class LenovoComputer : Computer
    {
        public LenovoComputer(string madefrom, string company, string cpu, string gpu)
        {

            this._madeFrom = madefrom;
            this._company = company;
            this._cpu = cpu;
            this._gpu = gpu;

        }
        public override void ShowInfo()
        {
            Console.WriteLine("MADE:{0}, Company:{1}, CPU:{2}, GPU:{2}", this._madeFrom, this._company, this._cpu, this._gpu);
        }
    }

    /// <summary>
    /// 装饰器，与需要扩展功能的类继承自同一个父类，然后持有该类，扩展方法来调用
    /// </summary>
    public class ComputerDecorator : Computer
    {
        private Computer decorator;
        public ComputerDecorator(Computer computer)
        {
            this.decorator = computer;
        }

        public override void ShowInfo()
        {
            this.decorator.ShowInfo();
            this.NewFunctionBlock();
        }

        private void NewFunctionBlock()
        {
            Console.WriteLine("this is 装饰器新增的新方法~！");
        }
    }


    #endregion

    #region 外观模式
    public abstract class WG_Color
    {
        public abstract void Paint();
    }

    public class RedColor : WG_Color
    {
        public override void Paint()
        {
            Console.WriteLine("wg red~!");
        }
    }

    public class YellowColor : WG_Color
    {
        public override void Paint()
        {
            Console.WriteLine("wg yellow~!");
        }
    }

    public class BlueColor : WG_Color
    {
        public override void Paint()
        {
            Console.WriteLine("wg blue~!");
        }
    }

    /// <summary>
    /// 外观（其实就算是管理器？？？？）
    /// </summary>
    public class ColorMaker
    {
        private WG_Color red;
        private WG_Color yellow;
        private WG_Color blue;

        public ColorMaker()
        {
            red = new RedColor();
            yellow = new YellowColor();
            blue = new BlueColor();
        }

        public void PaintById(int id)
        {
            switch (id)
            {
                case 1: red.Paint(); break;
                case 2: yellow.Paint(); break;
                case 3: blue.Paint(); break;
                default:
                    break;
            }
        }

    }
    #endregion

    #region 享元模式
    //享元模式有个情况，就是所有单元其实是共享的，改变一个其它都会改变
    //所以享元应该用于  当所需要的对象是没有什么特性的对象时才能用，或者结合原型模式使用
    public abstract class Solider
    {
        protected string type;
        protected string atk;
        public abstract void ShowInfo();
    }


    public class SwardSolider : Solider
    {
        public SwardSolider(string type, string atk)
        {
            this.type = type;
            this.atk = atk;
        }

        public override void ShowInfo()
        {
            Console.WriteLine("兵种：{0}, 攻击力：{1}", type, atk);
        }
    }


    public class SoliderFactory
    {
        private Dictionary<string, Solider> soliderCache = new Dictionary<string, Solider>();

        public Solider GetSolider(string type)
        {
            Solider temp;
            if (soliderCache.ContainsKey(type))
            {
                temp = soliderCache[type];
            }
            else
            {
                temp = new SwardSolider(type, "1");//先随便写吧。。。

                soliderCache.Add(type, temp);

                Console.WriteLine("add~~~~");
            }

            return temp;

        }
    }


    #endregion

    #region 代理模式

    public interface MImage
    {
        void Display();
    }

    public class HeadImage : MImage
    {
        private string path = string.Empty;

        public HeadImage(string path)
        {
            this.path = path;
        }

        public void Display()
        {
            Console.WriteLine("show file image:" + this.path);
            Console.WriteLine("Hash Code:" + this.GetHashCode());
        }

        public void LoadImage()
        {
            Console.WriteLine("load from file~!" + this.path);
        }
    }

    /// <summary>
    /// 代理器~~~~~~
    /// </summary>
    public class DLImage : MImage
    {
        private string path = string.Empty;
        private HeadImage headImage;
        public DLImage(string path)
        {
            this.path = path;
            headImage = new HeadImage(this.path);
        }

        public void Display()
        {
            Console.WriteLine("Hash Code:" + this.GetHashCode());
            headImage.Display();
        }
    }

    #endregion

    #region 责任链模式

    public enum Duty
    {
        one,
        two,
        three,
        four
    }
    /// <summary>
    /// 创建一个类，它除了需要实现我们需要的功能外，还必须实现：1.可以设置责任者；2.传递任务
    /// 必须实现的这两点其实可以写成一个接口，由不同的类继承即可，所以实现任务链模式不一定全部需要同一个继承体系的类
    /// </summary>
    public abstract class Worker
    {
        protected Worker nextWorker;

        public Duty m_type;

        public void SerWorker(Worker wk)
        {
            if (wk != this)
                this.nextWorker = wk;
        }

        public void HandleWork(Duty workType, string work)
        {
            if (this.m_type == workType)
            {
                KillTheWork(work);
                return;
            }

            Console.WriteLine("There is not my work, my duty is :" + m_type.ToString());

            if (null != nextWorker && nextWorker != this)
            {
                nextWorker.HandleWork(workType, work);
            }
            else
            {
                Console.WriteLine("There is no Woker can deal this work~!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }

        }

        protected abstract void KillTheWork(string work);
    }

    /// <summary>
    /// 创建一个类，可以得到一个责任链
    /// </summary>
    public class DutyChainManager
    {
        //....算了，这个之后延伸
    }


    public class Worker1 : Worker
    {
        public Worker1() : base()
        {
            m_type = Duty.one;
        }

        protected override void KillTheWork(string work)
        {
            Console.WriteLine("i have finish the work:{0}, and my hashcode is:{1}, my duty Type is:{2}", work, this.GetHashCode(), m_type.ToString());
        }
    }

    public class Worker2 : Worker
    {
        public Worker2() : base()
        {
            m_type = Duty.two;
        }

        protected override void KillTheWork(string work)
        {
            Console.WriteLine("i have finish the work:{0}, and my hashcode is:{1}, my duty Type is:{2}", work, this.GetHashCode(), m_type.ToString());
        }
    }

    public class Worker3 : Worker
    {
        public Worker3() : base()
        {
            m_type = Duty.three;
        }

        protected override void KillTheWork(string work)
        {
            Console.WriteLine("i have finish the work:{0}, and my hashcode is:{1}, my duty Type is:{2}", work, this.GetHashCode(), m_type.ToString());
        }
    }


    public class Worker4 : Worker
    {
        public Worker4() : base()
        {
            m_type = Duty.four;
        }

        protected override void KillTheWork(string work)
        {
            Console.WriteLine("i have finish the work:{0}, and my hashcode is:{1}, my duty Type is:{2}", work, this.GetHashCode(), m_type.ToString());
        }
    }






    #endregion

    #region 命令模式
    /// <summary>
    /// 命令的接口
    /// </summary>
    public interface Order
    {
        void excute();
    }

    /// <summary>
    /// 请求行为。。。。这里看起来有点像具体实现命令的类？？？
    /// 应该也可以用继承的方式，实现多个请求行为，不同的行为不同的处理命令的方式
    /// </summary>
    public class Stock
    {
        public void Buy()
        {
            Console.WriteLine("Buy goods~~~~!");
        }

        public void Sell()
        {
            Console.WriteLine("Sell goods~~~~!");
        }
    }

    /// <summary>
    /// 接收和执行命令的类
    /// </summary>
    public class Border
    {
        List<Order> orderList = new List<Order>();

        public void TakeOrder(Order order)
        {
            orderList.Add(order);
        }

        public void DealOrder()
        {
            for (int i = 0; i < orderList.Count; i++)
            {
                orderList[i].excute();
            }
        }
    }

    public class BuyOrder : Order
    {
        private Stock stock;
        public BuyOrder(Stock stock)
        {
            this.stock = stock;
        }

        public void excute()
        {
            this.stock.Buy();
        }
    }

    public class SellOrder : Order
    {
        private Stock stock;
        public SellOrder(Stock stock)
        {
            this.stock = stock;
        }

        public void excute()
        {
            this.stock.Sell();
        }
    }


    #endregion

    #region 迭代器模式

    public interface Iterator
    {
        bool HasNext();
        Object Next();
    }

    public interface Container
    {
        Iterator GetIterator();
    }


    public class MyContainer : Container
    {
        private Dictionary<string, string> m_Dic = new Dictionary<string, string>();
        private MyDicIterator dicIterator;

        private class MyDicIterator : Iterator
        {
            private int index = 0;
            private Dictionary<string, string> dic;
            public MyDicIterator(Dictionary<string, string> container)
            {
                dic = container;
            }

            public bool HasNext()
            {
                return index < dic.Count;
            }

            public Object Next()
            {
                if (HasNext())
                {
                    string key = dic.Keys.ElementAt(index++);//emmmm....这种方式遍历字典么。。。

                    return dic[key];
                }

                return null;
            }
        }


        public MyContainer()
        {
            dicIterator = new MyDicIterator(m_Dic);
        }

        public Iterator GetIterator()
        {
            return dicIterator;
        }

        public void Add(string key, string val)
        {
            m_Dic.Add(key, val);
        }
    }

    #endregion

    #region 中介者模式

    /// <summary>
    /// 这个就是中介者，当然很简陋就是了
    /// </summary>
    public class ChatRoom
    {
        public static void SendMessage(User who, string message)
        {
            Console.WriteLine("{0} said: {1}", who.Name, message);
        }
    }

    public class User
    {
        private string _name;
        public string Name { get { return _name; } }

        public User(string name)
        {
            this._name = name;
        }

        public void Speak(string word)
        {
            ChatRoom.SendMessage(this, word);
        }
    }


    #endregion

    #region 备忘录模式

    public interface ISave
    {
        void SetState(string st);
        string GetState();
    }

    public class SaveData : ISave, ICloneable
    {
        private string state;

        public SaveData(string st = "")
        {
            this.state = st;
        }

        public void SetState(string st)
        {
            this.state = st;
        }

        public string GetState()
        {
            return this.state;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class SaveManager
    {
        private SaveManager()
        {
            saveDataList = new List<SaveData>();
        }

        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (null == _instance)
                    _instance = new SaveManager();

                return _instance;
            }
        }

        private List<SaveData> saveDataList;

        public void Add(SaveData data)
        {
            saveDataList.Add(data);
        }

        public SaveData GetSaveData(int conditions)
        {
            return saveDataList[conditions].Clone() as SaveData;
        }

    }


    public class MainThread
    {
        private SaveData state;

        public void SetState(string st)
        {
            if (null == this.state)
                this.state = new SaveData(st);
            else
                this.state.SetState(st);

            Console.WriteLine("当前关卡：~~~~" + this.state.GetState());
        }

        public SaveData GetState()
        {
            return this.state.Clone() as SaveData;
        }

        public void SaveState()
        {
            SaveManager.Instance.Add(state);
        }
    }

    #endregion

    #region 观察者模式

    /// <summary>
    /// 观察者
    /// </summary>
    public abstract class Observer
    {
        public abstract void Update();
    }

    public class ObserverOne : Observer
    {

        public ObserverOne(Subject sb)
        {
            sb.UpdateEvent += Update;
        }

        public override void Update()
        {
            Console.WriteLine(" 1 update");
        }
    }

    public class ObserverTwo : Observer
    {
        public ObserverTwo(Subject sb)
        {
            sb.UpdateEvent += Update;
        }

        public override void Update()
        {
            Console.WriteLine(" 2 update");
        }
    }


    public class ObserverThree : Observer
    {
        public ObserverThree(Subject sb)
        {
            sb.UpdateEvent += Update;
        }

        public override void Update()
        {
            Console.WriteLine(" 3 update");
        }
    }

    /// <summary>
    /// 被观察者
    /// </summary>
    public class Subject
    {
        public delegate void Update();
        public event Update UpdateEvent;

        private int curState = 0;
        public void ChangeState(int state)
        {
            if (state != curState)
                UpdateEvent();
        }

    }



    #endregion


    #region 状态模式

    public enum State
    {
        Walk,
        Run,
        Jump,
        fastMove,
        slowMove
    }

    public abstract class StateAction
    {
        public abstract void DoAction();
        public abstract void SetTarget(Player pl);
    }

    public class Player
    {
        private StateAction m_State;


        public Player(StateAction state)
        {
            SetState(state);
        }

        public void SetState(StateAction state)
        {
            this.m_State = state;
            this.m_State.SetTarget(this);
            PlayerMove();
        }

        private void PlayerMove()
        {
            m_State.DoAction();
        }

        public override string ToString()
        {
            return "i,m a fighter~!";
        }
    }

    public class WalkAction : StateAction
    {
        private Player player;
        public override void SetTarget(Player pl)
        {
            player = pl;
        }
        public override void DoAction()
        {
            if (null == player) return;
            Console.WriteLine("player walk~~~!" + player.ToString());
        }
    }

    public class RunAction : StateAction
    {
        private Player player;
        public override void SetTarget(Player pl)
        {
            player = pl;
        }
        public override void DoAction()
        {
            if (null == player) return;
            Console.WriteLine("player run~~~!" + player.ToString());
        }
    }

    public class JumpAction : StateAction
    {
        private Player player;
        public override void SetTarget(Player pl)
        {
            player = pl;
        }
        public override void DoAction()
        {
            if (null == player) return;
            Console.WriteLine("player jump~~~!" + player.ToString());
        }
    }

    #endregion

    class Program
    {
        static void Main(string[] args)
        {
            #region 单例
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
            #endregion

            #region 构造者模式
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
            #endregion

            #region 工厂
            //--------------------------------------------------------工厂：创建复杂的类----------------------------------------------
            //test:
            //AnimalFactory animalFactory = new AnimalFactory();
            //Product cat = animalFactory.CreateProduct(ProductType.LittleCat);
            //cat.Show();
            //cat.Print();

            //Product dog = animalFactory.CreateProduct(ProductType.BogDog);
            //dog.Show();
            //dog.Print();
            #endregion

            #region 抽象工厂
            //--------------------------------------------------------抽象工厂：创建复杂的一族类----------------------------------------------
            //如果需要扩展其它的工厂，则MainFactor需要新增代码，enum需要新增type(这样可能导致某个枚举变得特别大~~~~)
            //工厂和抽象工厂的区别就是：抽象工厂多了一个工厂生成器~~~~
            //抽象工厂模式和构建者（Builder）模式的区别:1.抽象工厂更关注整体，即一个工厂生产指定的一系列产品；Builder更关注细节，根据不同的蓝图（数据类型）生产出不同的产品。
            //                                       2.工厂构造产品就是通过工厂（或一个工厂的工厂创建出的工厂），而Builder需要一个Designer(Master)根据提供的不同Builder(包含了数据类型和特顶的方法)来构建。
            //                                       3.抽象工厂的封装性很好，一般只需要提供一个接口即可。而Builder模式则更灵活（Builder由用户自己定义），更易扩展（每次新增都新增一个Builder）即可。
            //IFactory factory = MainFactory.CreateFactory(FacType.items);
            //IProduct product = factory.CreateProduct(Items.equipItems);
            //product.ShowInfo();
            #endregion

            #region 原型
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

            #endregion

            #region 适配器
            //--------------------------------------------------------适配器模式：具体操作的什么类，由适配器决定----------------------------------------------
            //AudioPlayer audioPlayer = new AudioPlayer();
            //audioPlayer.PlayMusic(AudioType.MP3, "等你下课");
            //audioPlayer.PlayMusic(AudioType.MP4, "PRAY");
            //audioPlayer.PlayMusic(AudioType.FLAC, "My Immortal");
            #endregion

            #region 桥接
            //--------------------------------------------------------桥接模式：抽象类与实现类分离----------------------------------------------
            //Shape shape = new Shape(new DrawCircle());
            //shape.Darw(10, 10, 10);

            //shape = new Shape(new DrawRetengle());
            //shape.Darw(1,1,1);
            #endregion

            #region 过滤器
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

            #endregion


            #region 组合器
            //--------------------------------------------------------组合器模式：为了表达出同一类对象的层次结构----------------------------------------------
            //SWorker sWorker = new SWorker("老板", 1);

            //SWorker sWorker1 = new SWorker("市场主管", 2);
            //SWorker sWorker2 = new SWorker("人事主任", 2);

            //SWorker sWorker3 = new SWorker("市场经理1", 3);
            //SWorker sWorker4 = new SWorker("市场经理2", 3);

            //SWorker sWorker5 = new SWorker("人事专员1", 3);
            //SWorker sWorker6 = new SWorker("人事专员2", 3);

            //SWorker sWorker7 = new SWorker("码农1", 4);
            //SWorker sWorker8 = new SWorker("码农2", 4);
            //SWorker sWorker9 = new SWorker("码农3", 4);
            //SWorker sWorker10 = new SWorker("码农4", 4);

            //sWorker.Add(sWorker1);
            //sWorker.Add(sWorker2);

            //sWorker1.Add(sWorker3);
            //sWorker1.Add(sWorker4);

            //sWorker2.Add(sWorker5);
            //sWorker2.Add(sWorker6);

            //sWorker3.Add(sWorker7);
            //sWorker4.Add(sWorker8);
            //sWorker4.Add(sWorker9);
            //sWorker4.Add(sWorker10);


            //sWorker.PrintConstact();
            #endregion

            #region 装饰器
            //--------------------------------------------------------装饰器模式：允许向一个现有的对象添加新的功能，同时又不改变其结构----------------------------------------------
            //一般的，我们为了扩展一个类经常使用继承方式实现，由于继承为类引入静态特征，并且随着扩展功能的增多，子类会很膨胀。
            //装饰器的方式，虽然避免了多次继承导致子类膨胀，但是却新增了其子类的数量（创建了装饰器）
            //Computer computer = new LenovoComputer("China", "lenovo", "i9", "2080ti");
            //computer.ShowInfo();

            ////利用  装饰器 + 原已创建的类重新构造一个类，给这个类赋予了新的功能，而没有改变原来类的构造
            //Computer computer2 = new ComputerDecorator(computer);
            //computer2.ShowInfo();
            #endregion

            #region 外观
            //--------------------------------------------------------外观模式：......Manager??----------------------------------------------
            //ColorMaker colorMaker = new ColorMaker();
            //colorMaker.PaintById(1);
            //colorMaker.PaintById(2);
            //colorMaker.PaintById(3);
            #endregion

            #region 享元
            //--------------------------------------------------------享元模式：避免重复创建大量的重复对象----------------------------------------------
            //在有大量对象时，有可能会造成内存溢出，我们把其中共同的部分抽象出来，如果有相同的业务请求，直接返回在内存中已有的对象，避免重新创建

            //SoliderFactory soliderFac = new SoliderFactory();

            //string[] soName = { "长剑士", "重甲剑士", "飞剑士" };
            //Random random = new Random(DateTime.Now.Second);
            //for (int i = 0; i < 20; i++)
            //{
            //    int index = random.Next(0, soName.Length );
            //    var solider = soliderFac.GetSolider(soName[index]);
            //    solider.ShowInfo();
            //}
            #endregion

            #region 代理
            //--------------------------------------------------------代理模式：相当于一种封装，利用代理器间接访问想访问的类----------------------------------------------
            //例：火车票可以通过代理点购买，而不需要直接去火车北站买
            //DLImage dLImage = new DLImage("text.png");
            //dLImage.Display();
            #endregion

            #region 责任链
            //--------------------------------------------------------责任链模式：把任务发给任务链，直到有一个类可以处理这个类为止----------------------------------------------
            //Worker1 w1 = new Worker1();
            //Worker2 w2 = new Worker2();
            //Worker3 w3 = new Worker3();
            //Worker4 w4 = new Worker4();

            //w1.SerWorker(w2);
            //w2.SerWorker(w3);
            //w3.SerWorker(w4);

            //w1.HandleWork(Duty.three, "这是给任务者3的任务");

            //w3.HandleWork(Duty.four, "这是给任务者4的任务");
            #endregion

            #region 命令
            //--------------------------------------------------------命令模式：将一个请求封装成一个对象，从而使您可以用不同的请求对客户进行参数化----------------------------------------------
            //行为请求者与行为实现者通常是一种紧耦合的关系，但某些场合，比如需要对行为进行记录、撤销或重做、事务等处理时，这种无法抵御变化的紧耦合的设计就不太合适
            //Stock stock = new Stock();

            //BuyOrder buyOrder = new BuyOrder(stock);
            //SellOrder sellOrder = new SellOrder(stock);

            //Border border = new Border();
            //border.TakeOrder(buyOrder);
            //border.TakeOrder(sellOrder);

            //border.DealOrder();

            #endregion

            #region 迭代器模式
            //-------------------------------------------------------迭代器模式：访问想遍历类中的数据时，用迭代器模式----------------------------------------------
            //这样就不用返回整个容器出来，不需要暴露类中存储数据的方式
            //同一个容器可以根据迭代器的不同而有不同的遍历方式
            //缺点是当需要增加聚合类（新的容器对象）而需要遍历时，可能就需要新增一个迭代器类，这会增加系统的复杂度和维护难度

            //MyContainer myContainer = new MyContainer();
            //myContainer.Add("k1", "v1");
            //myContainer.Add("k2", "v2");
            //myContainer.Add("k3", "v3");
            //myContainer.Add("k4", "v4");
            //myContainer.Add("k5", "v5");
            //myContainer.Add("k6", "v6");
            //myContainer.Add("k7", "v7");
            //myContainer.Add("k8", "v8");

            //for (Iterator it = myContainer.GetIterator(); it.HasNext(); )
            //{
            //    Console.WriteLine("VALUE:" + it.Next());
            //}

            #endregion

            #region 中介者模式
            //-------------------------------------------------------中介者模式：对象与对象之间不直接交互，而是通过中介者----------------------------------------------
            //用一个中介对象来封装一系列的对象交互，中介者使各对象不需要显式地相互引用，从而使其耦合松散，而且可以独立地改变它们之间的交互
            //假设，A要对B施加一个BUFF
            //此时有个 “BUFF添加机”， A调用这个类，传入对象B，和Buff数据，即可向B添加BUFF
            //User AMan = new User("风少");
            //User BMan = new User("胡老板");
            //User CMan = new User("幽君");


            //AMan.Speak("胡老板又去约了~！");
            //BMan.Speak("你这个渣男~！");
            //CMan.Speak("参一个~~~！");
            #endregion

            #region 备忘录模式
            //-------------------------------------------------------备忘录模式：在不破坏封装的前提下保存对象，以便于之后恢复----------------------------------------------
            // 1、需要保存/恢复数据的相关状态场景。 2、提供一个可回滚的操作。 例如：游戏存档

            //MainThread mainThread = new MainThread();
            //mainThread.SetState("第一关~~~~");
            //SaveManager.Instance.Add(mainThread.GetState());

            //mainThread.SetState("第五关~~~！");
            //SaveManager.Instance.Add(mainThread.GetState());


            //mainThread.SetState(SaveManager.Instance.GetSaveData(1).GetState());


            #endregion

            #region 观察者模式
            //Subject sbj = new Subject();
            //ObserverOne sb1 = new ObserverOne(sbj);
            //ObserverTwo sb2 = new ObserverTwo(sbj);
            //ObserverThree sb3 = new ObserverThree(sbj);


            //sbj.ChangeState(2);

            #endregion


            #region 状态模式
            //-------------------------------------------------------状态模式：在状态模式（State Pattern）中，类的行为是基于它的状态改变的----------------------------------------------
            //听名字有点像状态机？？
            //允许对象在内部状态发生改变时改变它的行为
            //这种实现方式有个特点，增加状态，维护都很容易
           WalkAction walkState = new WalkAction();
           RunAction runAction = new RunAction();
           JumpAction jumpAction = new JumpAction();

           Player player = new Player(walkState);
           player.SetState(runAction);
           player.SetState(jumpAction);

        #endregion


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
