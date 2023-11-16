using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace WorldMapStrategyKit {
	public class DemoUnitFiring : MonoBehaviour {

		WMSK map;
		GUIStyle labelStyle;
		GameObject tank1;

		void Start() {
			// Get a reference to the World Map API:
			map = WMSK.instance;

			// UI Setup - non-important, only for this demo
			labelStyle = new GUIStyle();
			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.normal.textColor = Color.white;

			// setup GUI resizer - only for the demo
			GUIResizer.Init(800, 500); 

			// Create tank
			Vector2 parisLocation = map.GetCity("Paris", "France").unity2DLocation;
			tank1 = DropTankOnPosition(parisLocation);

			// Fly to Paris
			map.FlyToLocation(parisLocation, 2f, 0.1f);

			// Listen to events
			map.OnClick += MapClickHandler;

			// Disable right-click centers since we'll be using right mouse button to fire
			map.centerOnRightClick = false;

		}

		/// <summary>
		/// UI Buttons
		/// </summary>
		void OnGUI() {
			// Do autoresizing of GUI layer
			GUIResizer.AutoResize();
			GUI.Box(new Rect(10, 10, 460, 40), "Left click: look at. Right click: fire!", labelStyle);
		}

		// Create tank instance and add it to the map
		GameObject DropTankOnPosition(Vector2 mapPosition) {
			GameObject tankGO = Instantiate(Resources.Load<GameObject>("Tank/CompleteTank"));
			GameObjectAnimator tank = tankGO.WMSK_MoveTo(mapPosition);
			tank.autoRotation = true;
			return tankGO;
		}

		void MapClickHandler(float x, float y, int buttonIndex) {

			Vector2 targetPosition = new Vector2(x, y);
			if (buttonIndex == 0) { // left click invoke look at
				tank1.WMSK_LookAt(targetPosition);
			} else if (buttonIndex == 1) { // right click fires

				// Create bullet
				GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				bullet.GetComponent<Renderer>().material.color = Color.yellow;
				bullet.transform.localScale = Misc.Vector3one * 0.5f;

				// Animate bullet!
				Vector3 tankCannonAnchor = new Vector3(0f, 1.55f, 0.85f);	// this is the position relative to the tank pivot (note that the tank pivot is at bottom of tank per model definition)
				float bulletSpeed = 10.0f;
				float bulletArc = 1.1f;
				GameObjectAnimator bulletAnim = tank1.WMSK_Fire(bullet, tankCannonAnchor, targetPosition, bulletSpeed, bulletArc);

				// We use the OnMoveEnd event of the bullet to destroy it once it reaches its destination
				bulletAnim.OnMoveStart += BulletFired;	
				bulletAnim.OnMoveEnd += BulletImpact;
			}
		}

		/// <summary>
		/// You can use this event to draw some special effect on bullet position (like tiny explosion)
		/// </summary>
		void BulletFired(GameObjectAnimator bulletAnim) {
			Debug.Log("Bullet fired!");
		}

		/// <summary>
		/// You can use this event to process the impact damage
		/// </summary>
		void BulletImpact(GameObjectAnimator bulletAnim) {
			Destroy(bulletAnim.gameObject);
			Debug.Log("Bullet destroyed!");
		}
		
		
	}

}

