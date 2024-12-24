using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public InputAction cameraDrag;
    public InputAction zoom;
    float maxZoom = 20f;
    float minZoom = 2f;
    Vector3 cameraPivot;
    bool m_dragging = false;
    bool m_rotating = false;
    float m_rotationSpeed = 10f;
    float m_rotationStep = 0f;
    float m_zoomSpeed = 10f;
    float m_dragSpeed = 2f;
    Vector3 m_gridCenter;
    Vector3 lastMousePos;
    Vector3 mousePos;

    private void OnEnable()
    {
        cameraDrag.Enable();
        zoom.Enable();
    }
    private void OnDisable()
    {
        cameraDrag?.Disable();
        zoom?.Disable();

    }
    private void Start()
    {
        cameraDrag.performed += OnDrag;
        cameraDrag.canceled += OnDrag;
        zoom.performed += OnZoom;
        cameraPivot = BattleGrid.Instance.GetWorldCenter();
    }
    void OnDrag(InputAction.CallbackContext ctx)
    {
        m_dragging = ctx.ReadValue<float>() > 0;
        if (m_dragging) lastMousePos = Input.mousePosition;
    }
    void Update()
    {
        DragCamera();
        RotateCamera();
    }

    void OnZoom(InputAction.CallbackContext ctx)
    {
        bool zoomIn = ctx.ReadValue<float>() > 0;
        Vector3 zoomedIn = Vector3.MoveTowards(transform.position, cameraPivot, m_zoomSpeed * Time.deltaTime);
        if (zoomIn) { transform.position = zoomedIn; return; }
        Vector3 zoomedOut = Vector3.MoveTowards(transform.position, transform.position - transform.forward, m_zoomSpeed * Time.deltaTime);
        transform.position = zoomedOut;
    }

    void DragCamera()
    {
        if (!m_dragging) return;
        mousePos = Input.mousePosition;
        if (lastMousePos == null) { lastMousePos = mousePos; return; }
        float dX = mousePos.x - lastMousePos.x;
        float dP = dX / Camera.main.pixelWidth;
        m_rotationStep = 360 * dP * m_rotationSpeed * Time.deltaTime;
        lastMousePos = mousePos;
    }
    void RotateCamera()
    {
        Camera.main.transform.RotateAround(cameraPivot, Vector3.up, m_rotationStep);
    }


    void SetDragging(bool dragging)
    {
        m_dragging = dragging;
    }
}
