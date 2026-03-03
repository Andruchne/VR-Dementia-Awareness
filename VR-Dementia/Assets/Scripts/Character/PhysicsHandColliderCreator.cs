using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class PhysicsHandColliderCreator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Das HandVisual-Skript des sichtbaren Hand-Modells (Child der LHand/RHand).")]
    public HandVisual handVisual;

    [Header("Collider Settings")]
    [Tooltip("Dicke der Finger")]
    public float fingerRadius = 0.012f;
    [Tooltip("Optionales Physik-Material (z.B. Zero-Friction, damit die Hand nicht an Wänden kleben bleibt)")]
    public PhysicsMaterial physicMaterial;

    private bool _collidersGenerated = false;

    void Update()
    {
        // Wir warten in der Update-Schleife, bis Meta das HandVisual und die Knochen geladen hat
        if (!_collidersGenerated && handVisual != null && handVisual.Joints.Count > 0 && handVisual.Joints[0] != null)
        {
            GenerateFingerColliders();
            _collidersGenerated = true;
        }
    }

    private void GenerateFingerColliders()
    {
        // Wir gehen alle Meta-Handgelenke durch
        for (int i = (int)HandJointId.HandThumb1; i < (int)HandJointId.HandEnd; ++i)
        {
            HandJointId currentJoint = (HandJointId)i;
            HandJointId parentJoint = HandJointUtils.JointParentList[i];

            // Wenn es kein gültiges Elterngelenk gibt, überspringen
            if (parentJoint == HandJointId.Invalid) continue;

            Transform currentTransform = handVisual.GetTransformByHandJointId(currentJoint);
            Transform parentTransform = handVisual.GetTransformByHandJointId(parentJoint);

            if (currentTransform == null || parentTransform == null) continue;

            CreateCapsuleForBone(parentJoint.ToString(), parentTransform, currentTransform.position);
        }

        Debug.Log($"[VRHandColliderBuilder] Finger-Collider für {gameObject.name} erfolgreich generiert!");
    }

    private void CreateCapsuleForBone(string boneName, Transform parentTransform, Vector3 targetPosition)
    {
        // 1. Neues GameObject für den Collider erstellen
        GameObject capsuleObj = new GameObject($"{boneName}_PhysicsCapsule");

        // 2. WICHTIG: Wir ordnen es dem animierten Knochen unter!
        capsuleObj.transform.SetParent(parentTransform, false);

        CapsuleCollider capsule = capsuleObj.AddComponent<CapsuleCollider>();
        if (physicMaterial != null) capsule.sharedMaterial = physicMaterial;

        // 3. Vektor-Mathematik für Länge und Ausrichtung
        Vector3 direction = targetPosition - parentTransform.position;
        float distance = direction.magnitude;

        // Kapsel exakt zwischen Parent-Knochen und aktuellem Knochen platzieren
        capsuleObj.transform.position = parentTransform.position + (direction * 0.5f);
        capsuleObj.transform.rotation = Quaternion.LookRotation(direction);

        // 4. Maße anpassen
        capsule.direction = 2; // 2 entspricht der Z-Achse (Vorwärts-Richtung nach LookRotation)
        capsule.radius = fingerRadius;

        // Die Kapselhöhe ist die Distanz plus die Rundungen (Radius * 2) an den Enden
        capsule.height = distance + (capsule.radius * 2f);
    }
}