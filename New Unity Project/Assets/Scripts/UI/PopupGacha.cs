using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PopupGacha : PopupBase {

    public Text oneCost;
    public Text elevenCost;
    public Text playerCoin;
    private uint gachaCost = 0;

    private List<ServentInfo> gachaResult = new List<ServentInfo>();
    public List<ServentInfo> GachResult { get { return gachaResult; } }

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
        ServentInfo servent = GetGachaResult(Utility.GetRandomVal(1, 100));
        gachaResult.Add(servent);
        Player.Instance.PlayerInfos.Coin -= gachaCost;

        StartCoroutine("ShowGacha");
    }

    private void ElevenGachaClick()
    {
        gachaResult.Clear();
        for (int i = 0; i < 10; i++)
        {
            ServentInfo servent = GetGachaResult(Utility.GetRandomVal(1, 100));
            gachaResult.Add(servent);
        }

        Player.Instance.PlayerInfos.Coin -= gachaCost * 10;
        StartCoroutine("ShowGacha");
    }


    private IEnumerator ShowGacha()
    {
        int index = 0;
        bool goOn = false;
        Action acallback = () => { goOn = true; ++index; };
        while (index < gachaResult.Count)
        {
            goOn = false;
            UIManager.Instance.OpenWindow<PopupGachaItem>(window => {
                //window.Init(gachaResult[index], acallback);
                window.Init(gachaResult[index].ID, gachaResult[index].Name, gachaResult[index].star, acallback);
            });
            while (!goOn)
            {
                yield return null;
            }
        }
        yield return new WaitForSeconds(0.25f);
        UIManager.Instance.OpenWindow<PopupGachaResult>(window => window.CreateItems(gachaResult));
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

        int index = 0;
        int result = 0;
        GameSettingInfo info = GameManager.Instance.GameSettingInfos;

        ServentInfo servent = new ServentInfo();

        switch (star)
        {
            case Star.one:
                index = Utility.GetRandomVal(0, info.one.Count);
                result = info.one[index];
                break;                        
            case Star.two:                    
                index = Utility.GetRandomVal(0, info.two.Count);
                result = info.two[index];
                break;                        
            case Star.three:                  
                index = Utility.GetRandomVal(0, info.three.Count);
                result = info.three[index];
                break;                        
            case Star.four:                   
                index = Utility.GetRandomVal(0, info.four.Count);
                result = info.four[index];
                break;                       
            case Star.five:                  
                index = Utility.GetRandomVal(0, info.five.Count);
                result = info.five[index];
                break;                       
            case Star.none:                  
                index = Utility.GetRandomVal(0, info.none.Count);
                result = info.none[index];
                break;
            default:
                break;
        }
        ServentManager.Instance.SetServentInfo(star, result, ref servent);

        return servent;
    }


    private void OnCloseClick()
    {
        base.Close();
    }



}
