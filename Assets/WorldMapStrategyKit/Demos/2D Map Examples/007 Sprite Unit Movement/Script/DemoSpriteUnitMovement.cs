using System.Collections.Generic;
using UnityEngine;

namespace WorldMapStrategyKit {

    public class DemoSpriteUnitMovement : MonoBehaviour {
        // WMSK Instance Reference
        WMSK map;

        int startCellIndex;
        float pathCost = 0;
        int destinationCellIndex;

        List<int> path = null;
        List<TextMesh> texts = new List<TextMesh>();

        // Space Shuttle References
        public GameObject ShuttlePrefabSprite;
        GameObjectAnimator shuttleGameObjectAnim;

        private void Start() {
            // Get a reference to the World Map API:
            map = WMSK.instance;

            // WMSK setup
            map = WMSK.instance;
            map.OnCellClick += HandleOnCellClick;
            map.OnCellEnter += HandleOnCellEnter;

            // Focus on Florida
            City city = map.GetCity("Orlando", "USA");
            map.FlyToLocation(city.unity2DLocation, duration: 2f, zoomLevel: 0.2f);

            // Creates a tank and positions it on the center of the hexagonal cell which contains Orlando
            Cell startCell = map.GetCell(city.unity2DLocation);
            DropShuttleOnMap(startCell.center);
            startCellIndex = map.GetCellIndex(startCell);
        }

        void OnGUI() {
            if (shuttleGameObjectAnim.maxSearchCost > 5 || ((int)Time.time) % 2 != 0) {
                GUI.Label(new Rect(10, 180, 250, 30), "Tank move points: " + shuttleGameObjectAnim.maxSearchCost);
            }
            if (shuttleGameObjectAnim.maxSearchCost < 5) {
                GUI.Label(new Rect(10, 200, 250, 30), "Press M to add more move points.");
            }
        }

        void Update() {
            if (map.input.GetKeyDown(KeyCode.M)) {
                shuttleGameObjectAnim.maxSearchCost += 10;
            }

            if (map.input.GetKeyDown(KeyCode.R)) {
                ShowMoveRange();
            }
        }

        // Create tank instance and add it to the map
        GameObjectAnimator DropShuttleOnMap(Vector2 mapPosition) {
            GameObject spaceShipGO = Instantiate(ShuttlePrefabSprite);
            spaceShipGO.transform.localScale = Misc.Vector3one * 0.5f;
            shuttleGameObjectAnim = spaceShipGO.WMSK_MoveTo(mapPosition);
            shuttleGameObjectAnim.autoRotation = true;
            shuttleGameObjectAnim.terrainCapability = TERRAIN_CAPABILITY.Any;
            shuttleGameObjectAnim.maxSearchCost = 20;
            return shuttleGameObjectAnim;
        }

        void HandleOnCellEnter(int destinationCellIndex) {
            if (startCellIndex >= 0 && startCellIndex != destinationCellIndex) {
                // Clear existing path
                ClearPreviousPath();
                // Find a cell path between starting cell and destination cell, only over ground, at any altitude and with a maximum traversing cost of tank.maxSearchCost
                path = map.FindRoute(map.GetCell(startCellIndex), map.GetCell(destinationCellIndex), out pathCost, shuttleGameObjectAnim.terrainCapability, shuttleGameObjectAnim.maxSearchCost);

                // If a path has been found, paint it!
                if (path != null) {
                    ShowPathAndCosts(path);
                } else {
                    // Otherwise, show it's not possible to reach that cell.
                    Debug.Log("Cell #" + destinationCellIndex + " is not reachable from cell #" + startCellIndex);
                }
            }
        }

        void HandleOnCellClick(int cellIndex, int buttonIndex) {
            if (path != null) {
                startCellIndex = cellIndex;
                shuttleGameObjectAnim.MoveTo(path, 0.5f);
                ClearPreviousPath();
            }
        }

        void ClearPreviousPath() {
            map.RestoreCellMaterials();
            texts.ForEach((t) => Destroy(t.gameObject));
            texts.Clear();
        }

        void ShowMoveRange() {
            int cellIndex = map.GetCellIndex(shuttleGameObjectAnim.currentMap2DLocation);
            if (cellIndex < 0)
                return;
            List<int> cells = shuttleGameObjectAnim.GetCellNeighbours();
            map.CellBlink(cells, Color.blue, 1f);
        }

        void ShowPathAndCosts(List<int> path) {

            // wait until shuttle stops at destination before showing the new path
            if (shuttleGameObjectAnim.isMoving) return;

            int steps = path.Count;

            Color pathColor = new Color(0.5f, 0.5f, 0, 0.5f);
            for (int k = 1; k < steps; k++) {   // ignore step 0 since this is current tank cell
                int cellIndex = path[k];

                // Color path cells
                map.SetCellTemporaryColor(cellIndex, pathColor);

                // Show the accumulated cost
                float accumCost = map.GetCellPathCost(cellIndex);
                Vector3 cellPosition = map.GetCellPosition(cellIndex);
                TextMesh text = map.AddMarker2DText(accumCost.ToString(), cellPosition);
                text.transform.localScale *= 0.8f;  // make font smaller
                texts.Add(text);
            }

        }
    }

}

