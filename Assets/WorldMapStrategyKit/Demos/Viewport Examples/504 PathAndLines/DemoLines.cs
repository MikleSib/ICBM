using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace WorldMapStrategyKit {
	public class DemoLines : MonoBehaviour {

		public GameObject endCapSprite;
		public Material endCapMaterial;

		enum UNIT_TYPE {
			TANK = 1,
			SHIP = 2
		}

		WMSK map;
		GUIStyle labelStyle, labelStyleShadow, buttonStyle, sliderStyle, sliderThumbStyle;
		bool showRoutePath = false, showLinearPath = true, enableClickToMoveTank = true, showCircle = true, showEndCap = true;
		GameObjectAnimator tank;
		LineMarkerAnimator pathLine;
		GameObject circle;
		Material lineMaterialAerial, lineMaterialGround;

		float pathDrawingDuration = 1.5f;
		// 0 means instant drawing.
		float pathArcElevation = 2f;
		// 0 means ground-level (path will be part of the viewport texture). If you provide a value, path will be 3D.
		float pathLineWidth = 0.5f;
		float pathDashInterval = 0.0008461f;
		float pathDashAnimationDuration = 0.6f;
		float circleRadius = 100f, circleRingStart = 0, circleRingEnd = 1f;
		Color circleColor = new Color(0.2f, 1f, 0.2f, 0.75f);

		void Start() {
			// Get a reference to the World Map API:
			map = WMSK.instance;

			// UI Setup - non-important, only for this demo
			labelStyle = new GUIStyle();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;
			labelStyleShadow = new GUIStyle(labelStyle);
			labelStyleShadow.normal.textColor = Color.black;
			buttonStyle = new GUIStyle(labelStyle);
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			buttonStyle.normal.background = Texture2D.whiteTexture;
			buttonStyle.normal.textColor = Color.white;
			sliderStyle = new GUIStyle();
			sliderStyle.normal.background = Texture2D.whiteTexture;
			sliderStyle.fixedHeight = 4.0f;
			sliderThumbStyle = new GUIStyle();
			sliderThumbStyle.normal.background = Resources.Load<Texture2D>("GUI/thumb");
			sliderThumbStyle.overflow = new RectOffset(0, 0, 8, 0);
			sliderThumbStyle.fixedWidth = 20.0f;
			sliderThumbStyle.fixedHeight = 12.0f;

			// Prepare line texture
			lineMaterialAerial = Instantiate(Resources.Load<Material>("PathLine/aerialPath"));
			lineMaterialGround = Instantiate(Resources.Load<Material>("PathLine/groundPath"));

			// setup GUI resizer - only for the demo
			GUIResizer.Init(800, 500); 

			// plug our mouse click listener - it will be used to move the tank to target destination
			map.OnClick += (float x, float y, int buttonIndex) => { 
				if (enableClickToMoveTank) {
					MoveTankWithPathFinding(new Vector2(x, y));
				}
			};

			// plug our mouse move listener - it received the x,y map position of the mouse
			map.OnMouseMove += (float x, float y) => {
				// while tank is moving avoid showing paths
				if (tank.isMoving)
					return; 
				// if show linear path is enabled, then just show a straight (or curved line if arc elevation is specified) from tank to destination
				if (showLinearPath) {
					UpdateLinearPathLine(x, y);
				} else {
					// show route path is enabled, then we'll compute the path and draw a line that pass through those points
					UpdateRoutePathLine(x, y);
				}
			};

			map.CenterMap();

			// Drop the tank on the Tibet
			DropTankOnCity();

		}

        /// <summary>
        /// UI Buttons
        /// </summary>
        void OnGUI() {

			// Do autoresizing of GUI layer
			GUIResizer.AutoResize();

			GUI.Box(new Rect(0, 0, 160, 160), "");

			bool prev = showLinearPath;
			showLinearPath = GUI.Toggle(new Rect(10, 20, 150, 30), showLinearPath, "Show Linear Path");
			if (showLinearPath != prev && showLinearPath) {
				showRoutePath = false;
				pathArcElevation = 2f;
			}
			prev = showRoutePath;
			showRoutePath = GUI.Toggle(new Rect(180, 20, 150, 30), showRoutePath, "Show Route Path");
			if (showRoutePath != prev && showRoutePath) {
				showLinearPath = false;
				pathArcElevation = 0;
			}
			enableClickToMoveTank = GUI.Toggle(new Rect(350, 20, 200, 30), enableClickToMoveTank, "Enable Move Tank On Click");
			showCircle = GUI.Toggle(new Rect(570, 20, 120, 30), showCircle, "Show Circle");
			showEndCap = GUI.Toggle(new Rect(690, 20, 120, 30), showEndCap, "Show End Cap");

			// Path line controls
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			GUI.Button(new Rect(10, 50, 150, 30), "  Drawing Duration", buttonStyle);
			GUI.backgroundColor = Color.white;
			pathDrawingDuration = GUI.HorizontalSlider(new Rect(10, 85, 150, 35), pathDrawingDuration, 0, 3f, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);

			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			GUI.Button(new Rect(10, 120, 150, 30), "  Arc Elevation", buttonStyle);
			GUI.backgroundColor = Color.white;
			pathArcElevation = GUI.HorizontalSlider(new Rect(10, 155, 150, 35), pathArcElevation, 0, 10f, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);

			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			GUI.Button(new Rect(10, 190, 150, 30), "  Line Width", buttonStyle);
			GUI.backgroundColor = Color.white;
			pathLineWidth = GUI.HorizontalSlider(new Rect(10, 225, 150, 35), pathLineWidth, 0.05f, 1f, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);

			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			GUI.Button(new Rect(10, 260, 150, 30), "  Dash Size", buttonStyle);
			GUI.backgroundColor = Color.white;
			pathDashInterval = GUI.HorizontalSlider(new Rect(10, 295, 150, 35), pathDashInterval, 0, 0.01f, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			GUI.Button(new Rect(10, 330, 150, 30), "  Dash Speed", buttonStyle);
			GUI.backgroundColor = Color.white;
			pathDashAnimationDuration = GUI.HorizontalSlider(new Rect(10, 365, 150, 35), pathDashAnimationDuration, 0, 2f, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			GUI.Button(new Rect(GUIResizer.authoredScreenWidth - 190, 50, 150, 30), "  Circle Radius", buttonStyle);
			GUI.backgroundColor = Color.white;
			circleRadius = GUI.HorizontalSlider(new Rect(GUIResizer.authoredScreenWidth - 190, 85, 150, 35), circleRadius, 0, 1000f, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);

			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			GUI.Button(new Rect(GUIResizer.authoredScreenWidth - 190, 120, 150, 30), "  Circle Ring Start", buttonStyle);
			GUI.backgroundColor = Color.white;
			circleRingStart = GUI.HorizontalSlider(new Rect(GUIResizer.authoredScreenWidth - 190, 155, 150, 35), circleRingStart, 0, 1f, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);
			GUI.Button(new Rect(GUIResizer.authoredScreenWidth - 190, 190, 150, 30), "  Circle Ring End", buttonStyle);
			GUI.backgroundColor = Color.white;
			circleRingEnd = GUI.HorizontalSlider(new Rect(GUIResizer.authoredScreenWidth - 190, 225, 150, 35), circleRingEnd, 0, 1f, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);

		}

		/// <summary>
		/// Creates a tank instance and adds it to specified city
		/// </summary>
		void DropTankOnCity() {

			// Get a random big city
			int cityIndex = map.GetCityIndex("Lhasa", "China");

			// Get city location
			Vector2 cityPosition = map.cities[cityIndex].unity2DLocation;

			if (tank != null)
				DestroyImmediate(tank);
			GameObject tankGO = Instantiate(Resources.Load<GameObject>("Tank/CompleteTank"));
			tank = tankGO.WMSK_MoveTo(cityPosition);
			tank.type = (int)UNIT_TYPE.TANK;
			tank.autoRotation = true;
			tank.terrainCapability = TERRAIN_CAPABILITY.OnlyGround;

			// Zoom into tank
			map.FlyToLocation(cityPosition, 2.0f, 0.15f);
		}

		/// <summary>
		/// Moves the tank with path finding.
		/// </summary>
		void MoveTankWithPathFinding(Vector2 destination) {
			// Ensure tank is limited terrain, avoid water
			if (tank == null) {
				DropTankOnCity();
				return;
			}

			// If current path is visible then fade it.
			if (pathLine != null) {
				pathLine.FadeOut(1.0f);

			}

			tank.terrainCapability = TERRAIN_CAPABILITY.OnlyGround;
			tank.MoveTo(destination, 0.1f);
		}

		/// <summary>
		/// Used when show Linear Path toggle is checked
		/// </summary>
		void UpdateLinearPathLine(float x, float y) {

			if (pathLine != null) {	// remove existing line
				DestroyImmediate(pathLine.gameObject);
			}

			// destination of linear path
			Vector2 destination = new Vector2(x, y);

			// optionally choose a material for the line (you may simply pass a color instead)
			Material lineMat = pathArcElevation > 0 ? lineMaterialAerial : lineMaterialGround;

			// draw the line
			pathLine = map.AddLine(tank.currentMap2DLocation, destination, lineMat, pathArcElevation, pathLineWidth);
			pathLine.drawingDuration = pathDrawingDuration;
			pathLine.dashInterval = pathDashInterval;
			pathLine.dashAnimationDuration = pathDashAnimationDuration;
			if (showEndCap) {
				pathLine.endCap = endCapSprite;
				pathLine.endCapMaterial = endCapMaterial;
				pathLine.endCapScale = new Vector3 (1f, 1f, 2.5f);
				pathLine.endCapOffset = 4f;
                pathLine.endCapFlipDirection = true;
			}

			UpdateCircle(destination);
		}

		/// <summary>
		/// Used when show Route Path toggle is checked
		/// </summary>
		void UpdateRoutePathLine(float x, float y) {

			if (pathLine != null) {	// remove existing line
				DestroyImmediate(pathLine.gameObject);
			}

			// Find a route for this tank to destination
			Vector2 destination = new Vector2(x, y);
			List<Vector2> route = tank.FindRoute(destination);

			if (route == null) {
				// Draw a straight red line if no route is available
				pathLine = map.AddLine(tank.currentMap2DLocation, destination, Color.red, pathArcElevation, pathLineWidth);
			} else {
				pathLine = map.AddLine(route.ToArray(), Color.yellow, pathArcElevation, pathLineWidth);
			}
			pathLine.drawingDuration = pathDrawingDuration;
			pathLine.dashInterval = pathDashInterval;
			pathLine.dashAnimationDuration = pathDashAnimationDuration;

			UpdateCircle(destination);
		}


		void UpdateCircle(Vector2 position) {
			if (circle != null)
				Destroy(circle);
			if (!showCircle)
				return;
			circle = map.AddCircle(position, circleRadius, circleRingStart, circleRingEnd, circleColor);
		}


	}

}

