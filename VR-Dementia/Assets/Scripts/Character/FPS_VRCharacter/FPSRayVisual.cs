using UnityEngine;
using Oculus.Interaction;

public class FPSRayVisual : MonoBehaviour
{
    public RayInteractor interactor;
    public LineRenderer lineRenderer;
    public float defaultLength = 5.0f;

    void LateUpdate()
    {
        lineRenderer.SetPosition(0, interactor.Origin);

        if (interactor.CollisionInfo.HasValue) { lineRenderer.SetPosition(1, interactor.CollisionInfo.Value.Point); }
        else { lineRenderer.SetPosition(1, interactor.Origin + interactor.Forward * defaultLength); }
    }
}