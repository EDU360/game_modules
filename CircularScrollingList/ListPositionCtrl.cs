﻿/* Calculate and assign the final position for each ListBoxes.
 *
 * There are three controling modes:
 * 1. Free moving: Control the listBoxes with finger or mouse.
 *    You don't know where the ListBox would stop at.
 * 2. Align to center: It's the same as free moving
 *    but there always has a listBox positioning at the center.
 * 3. Control by button: Control the listBoxes by button on the screen.
 *    There always has a listBox positioning at the center.
 *
 * Author: LanKuDot <airlanser@gmail.com>
 *
 * As long as you retain this notice you can do whatever you want with this stuff.
 * If we meet some day, and you think this stuff is worth it,
 * you can buy me a coffee in return. LanKuDot
 */
using UnityEngine;
using UnityEngine.UI;

public class ListPositionCtrl : MonoBehaviour
{
	public enum Direction {
		VERTICAL,
		HORIZONTAL
	};

	public static ListPositionCtrl Instance;
	/* Initial settings.
	 *   Mode            controlByButton  alignToCenter
	 * --------------------------------------------------
	 *   Free moving          false           false
	 *   Align to center      false           true
	 *   Control by btn       true          Don't care
	 */
	public bool controlByButton = false;
	public bool alignToCenter = false;

	public ListBox[] listBoxes;

	public Button[] buttons;

	public Direction direction = Direction.VERTICAL;
	// For 3D camera, the distance between canvas plane and camera.
	public float canvasDistance = 100.0f;
	// Set the distance between each ListBox. The larger, the closer.
	public float divideFactor = 2.0f;
	// Set the sliding duration. The larger, the longer.
	public int slidingFrames = 35;
	// Set the sliding speed. The larger, the quicker.
	[Range( 0.0f, 1.0f )]
	public float slidingFactor = 0.2f;
	// Set the scrolling list curving to left/right, or up/down in HORIZONTAL mode.
	// Positive: Curve to right (up); Negative: Curve to left (down).
	[Range( -1.0f, 1.0f )]
	public float angularity = 0.3f;
	// Set the scale amount of the center listBox.
	public float scaleFactor = 0.32f;

	private bool _isTouchingDevice;

	private Vector3 _lastInputWorldPos;
	private Vector3 _currentInputWorldPos;
	private Vector3 _deltaInputWorldPos;

	void Awake()
	{
		Instance = this;

		switch (Application.platform) {
		case RuntimePlatform.WindowsEditor:
			_isTouchingDevice = false;
			break;
		case RuntimePlatform.Android:
			_isTouchingDevice = true;
			break;
		}
	}

	void Start()
	{
		if (!controlByButton)
			foreach (Button button in buttons)
				button.gameObject.SetActive( false );
	}

	void Update()
	{
		if (!controlByButton) {
			if (!_isTouchingDevice)
				storeMousePosition();
			else
				storeFingerPosition();
		}
	}

	/* Store the position of mouse when the player clicks the left mouse button.
	 */
	void storeMousePosition()
	{
		if (Input.GetMouseButtonDown( 0 )) {
			_lastInputWorldPos = Camera.main.ScreenToWorldPoint(
				new Vector3( Input.mousePosition.x, Input.mousePosition.y, canvasDistance ) );
		} else if (Input.GetMouseButton( 0 )) {
			_currentInputWorldPos = Camera.main.ScreenToWorldPoint(
				new Vector3( Input.mousePosition.x, Input.mousePosition.y, canvasDistance ) );
			_deltaInputWorldPos = _currentInputWorldPos - _lastInputWorldPos;
			foreach (ListBox listbox in listBoxes)
				listbox.updatePosition( _deltaInputWorldPos / transform.parent.localScale.x );

			_lastInputWorldPos = _currentInputWorldPos;
		} else if (Input.GetMouseButtonUp( 0 ))
			setSlidingEffect();
	}

	/* Store the position of touching on the mobile.
	 */
	void storeFingerPosition()
	{
		if (Input.GetTouch( 0 ).phase == TouchPhase.Began) {
			_lastInputWorldPos = Camera.main.ScreenToWorldPoint(
				new Vector3( Input.GetTouch( 0 ).position.x, Input.GetTouch( 0 ).position.y, canvasDistance ) );
		} else if (Input.GetTouch( 0 ).phase == TouchPhase.Moved) {
			_currentInputWorldPos = Camera.main.ScreenToWorldPoint(
				new Vector3( Input.GetTouch( 0 ).position.x, Input.GetTouch( 0 ).position.y, canvasDistance ) );
			_deltaInputWorldPos = _currentInputWorldPos - _lastInputWorldPos;
			foreach (ListBox listbox in listBoxes)
				listbox.updatePosition( _deltaInputWorldPos / transform.parent.localScale.x );

			_lastInputWorldPos = _currentInputWorldPos;
		} else if (Input.GetTouch( 0 ).phase == TouchPhase.Ended)
			setSlidingEffect();
	}

	/* If the touching is ended, calculate the distance to slide and
	 * assign to the listBoxes.
	 */
	void setSlidingEffect()
	{
		Vector3 deltaPos = _deltaInputWorldPos / transform.parent.localScale.x;

		if (alignToCenter)
			deltaPos = findDeltaPositionToCenter();

		foreach (ListBox listbox in listBoxes)
			listbox.setSlidingDistance( deltaPos );
	}

	/* Find the listBox which is the closest to the center position,
	 * And calculate the delta position of x or y between them.
	 */
	Vector3 findDeltaPositionToCenter()
	{
		float minDeltaPos = Mathf.Infinity;
		float deltaPos;

		switch (direction) {
		case Direction.VERTICAL:
			foreach (ListBox listBox in listBoxes) {
				deltaPos = -listBox.transform.localPosition.y;
				if (Mathf.Abs( deltaPos ) < Mathf.Abs( minDeltaPos ))
					minDeltaPos = deltaPos;
			}

			return new Vector3( 0.0f, minDeltaPos, 0.0f );

		case Direction.HORIZONTAL:
			foreach (ListBox listBox in listBoxes) {
				deltaPos = -listBox.transform.localPosition.x;
				if (Mathf.Abs( deltaPos ) < Mathf.Abs( minDeltaPos ))
					minDeltaPos = deltaPos;
			}

			return new Vector3( minDeltaPos, 0.0f, 0.0f );

		default:
			return Vector3.zero;
		}
	}

	/* controlByButton is enabled!
	 * When the next content button is pressed,
	 * move all listBoxes 1 unit up.
	 */
	public void nextContent()
	{
		foreach (ListBox listbox in listBoxes)
			listbox.unitMove( 1, true );
	}

	/* controlByButton is enabled!
	 * When the last content button is pressed,
	 * move all listBoxes 1 unit down.
	 */
	public void lastContent()
	{
		foreach (ListBox listbox in listBoxes)
			listbox.unitMove( 1, false );
	}
}
