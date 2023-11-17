using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldMapStrategyKit;

public class mapperClick : MonoBehaviour
{
    WMSK map;
    //Ru - 180
    void Start()
    {
         map = WMSK.instance;
        map.OnProvinceClick += Map_OnProvinceClick;
    }

    private void Map_OnProvinceClick(int provinceIndex, int regionIndex, int buttonIndex)
    {
        map.CountryTransferProvince(180, provinceIndex, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
