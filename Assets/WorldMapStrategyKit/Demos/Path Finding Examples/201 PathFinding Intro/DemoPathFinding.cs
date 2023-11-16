using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace WorldMapStrategyKit {
    public class DemoPathFinding : MonoBehaviour {

        WMSK map;
        bool enableToggleOwnership, enableClickToMoveTank = true;
        GameObjectAnimator tank;
        List<Country> europeanCountries = new List<Country>();
        Color player1Color, player2Color;
        bool canCross = true;
        List<GameObject> blocks = new List<GameObject>();

        void Start() {
            // Get a reference to the World Map API:
            map = WMSK.instance;

            // setup GUI resizer - only for the demo
            GUIResizer.Init(800, 500);

            // Get list of European countries
            europeanCountries = new List<Country>();
            for (int k = 0; k < map.countries.Length; k++) {
                Country country = map.countries[k];
                if (country.continent.Equals("Europe")) {
                    europeanCountries.Add(country);
                    // Distribute countries between 2 players
                    if (country.center.x < 0.04f) {
                        country.attrib["player"] = 1;
                    } else {
                        country.attrib["player"] = 2;
                    }
                }
            }

            // Colors
            player1Color = new Color(1, 0.5f, 0, 0.65f);
            player2Color = new Color(0, 0.5f, 1, 0.65f);

            // Setup map rect
            map.windowRect = new Rect(-0.0587777f, 0.1964018f, 0.1939751f, 0.1939751f);
            map.SetZoomLevel(0.1939751f);
            map.CenterMap();

            // Paint countries
            PaintCountries();

            // Drop our tester tank
            DropTankOnCity();

            // On map click listener
            map.OnClick += (float x, float y, int buttonIndex) => {
                if (enableToggleOwnership)
                    ChangeCountryOwnerShip(x, y);
                else if (enableClickToMoveTank) {
                    MoveTankWithPathFinding(new Vector2(x, y));
                }
            };

            // Enable custom cost path finding matrix (we'll setup this matrix when moving the unit)
            map.pathFindingEnableCustomRouteMatrix = true;
            UpdatePathFindingMatrixCost();

        }

        private void Update() {
            // Increase duration of the unit path by 10 seconds
            if (map.input.GetKeyDown(KeyCode.F)) {
                tank.ChangeDuration(10f);
            }
            // Decrease duration of the unit path by 10 seconds
            if (map.input.GetKeyDown(KeyCode.G)) {
                tank.ChangeDuration(-10f);
            }
        }

        /// <summary>
        /// UI Buttons
        /// </summary>
        void OnGUI() {
            // Do autoresizing of GUI layer
            GUIResizer.AutoResize();

            // Switches between tank move and change country ownership on click
            bool prev = enableToggleOwnership;
            enableToggleOwnership = GUI.Toggle(new Rect(10, 20, 200, 30), enableToggleOwnership, "Change Ownership on Click");
            if (enableToggleOwnership && prev != enableToggleOwnership) {
                enableClickToMoveTank = false;
            }

            prev = enableClickToMoveTank;
            enableClickToMoveTank = GUI.Toggle(new Rect(230, 20, 150, 30), enableClickToMoveTank, "Move Tank On Click");
            if (enableClickToMoveTank && prev != enableClickToMoveTank) {
                enableToggleOwnership = false;
            }

            // Show debug info
            map.pathFindingVisualizeMatrixCost = GUI.Toggle(new Rect(500, 20, 200, 30), map.pathFindingVisualizeMatrixCost, "Debug Matrix Cost");

            // Block / clear frontier between Spain and France
            if (GUI.Button(new Rect(10, 50, 250, 28), "Block/Clear Spain-France Frontier")) {
                ToggleFrontierSpainFrance();
            }
        }

        /// <summary>
        /// Creates a tank instance and adds it to specified city
        /// </summary>
        void DropTankOnCity() {

            // Get a random big city
            int cityIndex = map.GetCityIndex("Paris", "France");

            // Get city location
            Vector2 cityPosition = map.cities[cityIndex].unity2DLocation;

            GameObject tankGO = Instantiate(Resources.Load<GameObject>("Tank/CompleteTank"));
            tank = tankGO.WMSK_MoveTo(cityPosition);
            tank.autoRotation = true;
            tank.terrainCapability = TERRAIN_CAPABILITY.OnlyGround;

            // Set tank ownership
            tank.attrib["player"] = 1;
        }



        /// <summary>
        /// Moves the tank with path finding.
        /// </summary>
        void MoveTankWithPathFinding(Vector2 destination) {
            bool canMove = tank.MoveTo(destination, 0.1f);

            if (!canMove) {
                Debug.Log("Can't move to destination!");
            }
        }


        void ChangeCountryOwnerShip(float x, float y) {
            int countryIndex = map.GetCountryIndex(new Vector2(x, y));
            if (countryIndex < 0)
                return;
            Country country = map.countries[countryIndex];
            if (country.attrib["player"] == 1) {
                country.attrib["player"] = 2;
            } else {
                country.attrib["player"] = 1;
            }
            UpdatePathFindingMatrixCost();  // need to update cost matrix

            PaintCountries();
            map.BlinkCountry(countryIndex, player1Color, player2Color, 1f, 0.1f);
        }


        void PaintCountries() {
            europeanCountries.ForEach((country) => {
                if (country.attrib["player"] == 1) {
                    map.ToggleCountrySurface(country.name, true, player1Color);
                } else {
                    map.ToggleCountrySurface(country.name, true, player2Color);
                }
            });
        }

        void UpdatePathFindingMatrixCost() {
            // Setup custom route matrix - first we reset it
            map.PathFindingCustomRouteMatrixReset();

            //  Then set a cost of 0 (unbreakable) on those location belonging to a different player to prevent the tank move over those non-controlled zones.
            int tankPlayer = tank.attrib["player"];
            europeanCountries.ForEach((country) => {
                int countryPlayer = country.attrib["player"];
                if (countryPlayer != tankPlayer) {
                    map.PathFindingCustomRouteMatrixSet(country, 0);
                }

            });
        }

        void ToggleFrontierSpainFrance() {
            List<Vector2> blockedPositions = map.GetCountryFrontierPoints(map.GetCountryIndex("France"), map.GetCountryIndex("Spain"), 1);
            canCross = !canCross;
            map.PathFindingCustomRouteMatrixSet(blockedPositions, canCross ? -1 : 0);
            if (canCross) {
                for(int k=0;k<blocks.Count;k++) {
                    Destroy(blocks[k]);
                }
                blocks.Clear();
            } else {
                blocks.Clear();
                foreach(Vector2 blockPosition in blockedPositions) {
                    GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    map.AddMarker3DObject(o, blockPosition, 0.05f);
                    blocks.Add(o);
                }
            }
        }

    }

}

