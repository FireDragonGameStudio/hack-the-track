using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrogestureControl : MonoBehaviour {

    [Header("Main UI Visibility")]
    [SerializeField] private GameObject mainUi;

    [Header("Gesture References")]
    [SerializeField] private OVRMicrogestureEventSource leftGestureSource;
    [SerializeField] private OVRMicrogestureEventSource rightGestureSource;
    [SerializeField] private float gestureShowDuration = 1.5f;

    private bool isLeftThumbTapActive;
    private bool isRightThumbTapActive;

    private Dictionary<GameObject, Coroutine> actionCoroutines = new Dictionary<GameObject, Coroutine>();

    private void Start() {
        leftGestureSource.GestureRecognizedEvent.AddListener(gesture => OnGestureRecognized(OVRPlugin.Hand.HandLeft, gesture));
        rightGestureSource.GestureRecognizedEvent.AddListener(gesture => OnGestureRecognized(OVRPlugin.Hand.HandRight, gesture));
    }

    private void Update() {
        if (isLeftThumbTapActive && isRightThumbTapActive) {
            isLeftThumbTapActive = false;
            isRightThumbTapActive = false;

            mainUi.SetActive(!mainUi.activeSelf);
        }
    }

    private void HighlightGesture(OVRPlugin.Hand hand, OVRHand.MicrogestureType gesture) {
        if (gesture == OVRHand.MicrogestureType.ThumbTap) {
            if (hand == OVRPlugin.Hand.HandLeft) {
                ManageThumbTapCoroutines(leftGestureSource.gameObject);
                actionCoroutines.Add(leftGestureSource.gameObject, StartCoroutine(HighlightThumbTapLeft()));
            } else {
                ManageThumbTapCoroutines(rightGestureSource.gameObject);
                actionCoroutines.Add(rightGestureSource.gameObject, StartCoroutine(HighlightThumbTapRight()));
            }
        }
    }

    private void ManageThumbTapCoroutines(GameObject gestureSource) {
        if (actionCoroutines.TryGetValue(gestureSource, out Coroutine leftHandCoroutine)) {
            StopCoroutine(leftHandCoroutine);
            actionCoroutines.Remove(gestureSource);
        }
    }

    private void OnGestureRecognized(OVRPlugin.Hand hand, OVRHand.MicrogestureType gesture) {
        HighlightGesture(hand, gesture);
    }

    private IEnumerator HighlightThumbTapLeft() {
        isLeftThumbTapActive = true;
        yield return new WaitForSeconds(gestureShowDuration);
        isLeftThumbTapActive = false;
    }
    private IEnumerator HighlightThumbTapRight() {
        isRightThumbTapActive = true;
        yield return new WaitForSeconds(gestureShowDuration);
        isRightThumbTapActive = false;
    }
}