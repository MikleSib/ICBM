using UnityEngine;
using WorldMapStrategyKit;

public class AddSprite : MonoBehaviour
{
    public GameObject sprite;

    void Start()
    {
        WMSK map = WMSK.instance;
        Vector3 pos = map.GetCountry("France").center;
        GameObject go = Instantiate(sprite);
        map.AddMarker2DSprite(go, pos, 0.01f);
    }
}
