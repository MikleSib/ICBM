using System.Collections.Generic;
using UnityEngine;
using WorldMapStrategyKit;


public class DemoPathOnUIElement : MonoBehaviour
{
    WMSK map;

    public GameObject endCap;

    void Start()
    {
        map = WMSK.instance;

        // zoom into France
        map.FlyToCountry("France", duration: 1f, zoomLevel: 0.15f);
        map.OnFlyEnd += Map_OnFlyEnd;
    }

    private void Map_OnFlyEnd() {
        // draw a ground path from Madrid to Paris with end cap
        City startCity = map.GetCity("Madrid", "Spain");
        City endCity = map.GetCity("Paris", "France");
        List<Vector2> route = map.FindRoute(startCity, endCity, terrainCapability: TERRAIN_CAPABILITY.Air);
        LineMarkerAnimator lma = map.AddLine(route, Color.yellow, arcElevation: 0, lineWidth: 0.1f);
        lma.drawingDuration = 2f;
        lma.endCap = endCap;
        lma.endCapOffset = 0.5f;
    }
}
