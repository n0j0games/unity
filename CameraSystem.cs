using UnityEngine;
using UnityEngine.U2D;

//Requires Pixel-Perfect camera to be installed & added to the "Main Camera" GameObject (or some other camrea)
//The Camera can be zoomed with the mouse wheel and moved by dragging the middle mouse button

public class PixelCamera : MonoBehaviour
{
    /// <summary>The interval in which the camera will zoom in/out</summary>
    public int zoominterval = 2;
    /// <summary>Movement speed on camera drag</summary>
    public float movespeed = 2f;
    PixelPerfectCamera ppc;
    float zoom;
    Vector3 dragOrigin;

    void Start()
    {
        ppc = gameObject.GetComponent<PixelPerfectCamera>();
        zoom = ppc.assetsPPU;
    }

    void Update()
    {
        startZoom();
        movement();
    }

    void startZoom()
    {
        if (Input.mouseScrollDelta.y > 0)
            ppc.assetsPPU += zoominterval;
        if (Input.mouseScrollDelta.y < 0 && ppc.assetsPPU > zoominterval)
            ppc.assetsPPU -= zoominterval;
    }

    void movement()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            return;
        }
        if (!Input.GetMouseButton(2)) return;
        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x * -movespeed, pos.y * -movespeed, 0);
        transform.Translate(move, Space.World);
    }
}
