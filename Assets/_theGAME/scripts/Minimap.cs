using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Minimap : MonoBehaviour {

    TTGameManager gameManager;

    public Transform player;

    public Camera minimapCamera;
    public GameObject minimap;

    private float desiredZoom = 10;
    public float zoomSpeed = 12f;
    public float zoomLerpSpeed = 10f;
    public bool rotateMap;

    void Awake() {
        gameManager = FindObjectOfType<TTGameManager>();
    }

    void Start() {
        player = gameManager.localPlayer.transform;
        minimapCamera = GetComponent<Camera>();
        //minimap = 
    }

    void Update() {
        minimapCamera.orthographicSize = Mathf.Lerp(minimapCamera.orthographicSize, desiredZoom, Time.deltaTime * zoomLerpSpeed);
    }

    void LateUpdate() {
        // Check if we are scrolling over the Minimap
        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            float zoomDir = -Input.GetAxis("Mouse ScrollWheel");
            var pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);

            // Is the player interacting with the Minimap?
            if (hits.Any(hitUI => hitUI.gameObject.name == "Minimap")) {
                ZoomMap(zoomDir);
            }
        }

        Vector3 newPos = player.position;
        newPos.y = transform.position.y;
        transform.position = newPos;

        if(rotateMap) transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }

    public void ToggleMinimap(GameObject map) {
        map.SetActive(!map.activeSelf);
    }

    void ZoomMap(float zoomDir) {
        desiredZoom = Mathf.Clamp(desiredZoom += zoomDir * zoomSpeed, 5, 20);
    }
}

// Zoom in and out
// Adjust minimap size
// Turn minimap off
// Hide our location
// Reposition minimap