using UnityEngine;

public class FovManager : MonoBehaviour {
    private Camera pCamera = null;
    public float flDefaultFov = 15;
    public float flMaxFov = 50;
    public float flMinFov = 10;
    public float flCameraSpeed = 5.0f;

    // Start is called before the first frame update
    void Start( ) {
        pCamera = Camera.main;
        pCamera.orthographicSize = flDefaultFov;
    }

    // Update is called once per frame
    void FixedUpdate( ) {
        if (pCamera == null)
            return;

        float flTempValue = 0.0f;

        if (Input.GetAxis( "Mouse ScrollWheel" ) > 0.0f)
            flTempValue -= flCameraSpeed * Time.deltaTime;
        if (Input.GetAxis( "Mouse ScrollWheel" ) < 0.0f)
            flTempValue += flCameraSpeed * Time.deltaTime;

        pCamera.orthographicSize += flTempValue;
        pCamera.orthographicSize = Mathf.Clamp( pCamera.orthographicSize, flMinFov, flMaxFov );
    }
}