using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupGacha : PopupBase {

    public Text oneCost;
    public Text elevenCost;
    public Text playerCoin;
    private uint gachaCost = 0;

    private List<ServentInfo> gachaResult = new List<ServentInfo>();

    protected override void Init()
    {
        base.Init();
        gachaCost = GameManager.Instance.GameSettingInfos.GachaCost;
        oneCost.text = gachaCost.ToString() + "g";
        elevenCost.text = (gachaCost * 10).ToString() + "g";

        playerCoin.text = Player.Instance.PlayerInfos.Coin.ToString() + "g";
    }

    private void OneGachaClick()
    {
        gachaResult.Clear();
        ServentInfo servent = GetGachaResult(GetRandomVal(1, 100));
        gachaResult.Add(servent);
        Player.Instance.PlayerInfos.Coin -= gachaCost;

        UIManager.Instance.OpenWindow<PopupGachaResult>(window=>window.CreateItems(gachaResult));
    }

    private void ElevenGachaClick()
    {
        gachaResult.Clear();
        for (int i = 0; i < 10; i++)
        {
            ServentInfo servent = GetGachaResult(GetRandomVal(1, 100));
            gachaResult.Add(servent);
        }

        Player.Instance.PlayerInfos.Coin -= gachaCost * 10;
        UIManager.Instance.OpenWindow<PopupGachaResult>(window => window.CreateItems(gachaResult));

    }

    /// <summary>
    /// 得到一个范围内的随机数
    /// </summary>
    /// <param name="min">1，最小角色id</param>
    /// <param name="max">100，最大角色id</param>
    /// <returns></returns>
    private int GetRandomVal(int min, int max)
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        System.Random random = new System.Random(seed);

        int result = random.Next(min, max);//包含下限，不包含上限

        return result;
    }

    private ServentInfo GetGachaResult(int random)
    {
        //LoggerM.LogError("Random:" + random.ToString());
        Star star = Star.none;
        if (random <= 20) //20% 没有星级
        {
            star = Star.none;
        }
        else if (random > 20 && random <= 30) //10%为1星
        {
            star = Star.one;
        }
        else if( random > 30 && random <= 40)//10%为2星
        {
            star = Star.two;
        }
        else if (random > 40 && random <= 70)//30%为三星
        {
            star = Star.three;
        }
        else if (random > 70 && random <= 95)//25%为四星
        {
            star = Star.four;
        }
        else 
        {
            star = Star.five; //5%为五星
        }

        int result = 0;
        GameSettingInfo info = GameManager.Instance.GameSettingInfos;

        ServentInfo servent = new ServentInfo();

        switch (star)
        {
            case Star.one:
                result = GetRandomVal(info.one.min, info.one.max);      
                break;
            case Star.two: 
                result = GetRandomVal(info.two.min, info.two.max);
                break;
            case Star.three:
                result = GetRandomVal(info.three.min, info.three.max);
                break;
            case Star.four:
                result = GetRandomVal(info.four.min, info.four.max);
                break;
            case Star.five:
                result = GetRandomVal(info.five.min, info.five.max);
                break;
            case Star.none:
                result = GetRandomVal(info.none.min, info.none.max);
                break;
            default:
                break;
        }

        Debug.LogError("Result:" + result.ToString());
        ServentManager.Instance.SetServentInfo(star, result, ref servent);

        return servent;
    }


    private void OnCloseClick()
    {
        base.Close();
    }



}
