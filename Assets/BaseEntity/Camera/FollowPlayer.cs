using Unity.Mathematics;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {
    public Transform pTarget = null;
    private Vector3 vDelta;
    private Vector3 vTargetPos;

    public void Start( ) {
        if (pTarget == null)
            return;

        vDelta = transform.position - pTarget.position;
    }

    // Update is called once per frame
    void FixedUpdate( ) {
        if (pTarget == null)
            return;

        vTargetPos = pTarget.position + vDelta;
        float flDistance2D = Vector3.Distance( this.transform.position, vTargetPos );

        float flProgress = math.max( 1.0f, flDistance2D );

        this.transform.position = Vector3.LerpUnclamped(
            this.transform.position,
            vTargetPos,
            flProgress * 5 * Time.deltaTime
        );
    }
}
