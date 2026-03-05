using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class PhysicsHandColliderCreator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The HandVisual gameObject, attached as child to the PhysicsHand")]
    public HandVisual handVisual;

    [Header("Collider Settings")]
    [Tooltip("Radius of Fingertip")]
    public float fingerRadius = 0.012f;
    [Tooltip("(Optional) PhysicsMaterial for Hands")]
    public PhysicsMaterial physicsMaterial;

    private bool _collidersGenerated = false;

    void Update()
    {
        // Wait until the HandVisual bones have been loaded
        if (!_collidersGenerated && handVisual != null && handVisual.Joints.Count > 0 && handVisual.Joints[0] != null)
        {
            GenerateFingerColliders();
            _collidersGenerated = true;
        }
    }

    private void GenerateFingerColliders()
    {
        // Iterate through all hand bones
        for (int i = (int)HandJointId.HandThumb1; i < (int)HandJointId.HandEnd; ++i)
        {
            HandJointId currentJoint = (HandJointId)i;
            HandJointId parentJoint = HandJointUtils.JointParentList[i];

            // Skip to next iteration if joint invalid
            if (parentJoint == HandJointId.Invalid) { continue; }

            // To order the colliders appropriately
            Transform currentTransform = handVisual.GetTransformByHandJointId(currentJoint);
            Transform parentTransform = handVisual.GetTransformByHandJointId(parentJoint);

            if (currentTransform == null || parentTransform == null) { continue; }

            // Instantiate all bone colliders as children
            CreateCapsuleForBone(parentJoint.ToString(), parentTransform, currentTransform.position);
        }
    }

    private void CreateCapsuleForBone(string boneName, Transform parentTransform, Vector3 targetPosition)
    {
        // Create GameObject and set collision layer appropriately
        GameObject capsuleObj = new GameObject($"{boneName}_PhysicsCapsule");
        capsuleObj.layer = LayerMask.NameToLayer("Physics Hands");

        // Set parent to be the animated bone
        capsuleObj.transform.SetParent(parentTransform, false);

        CapsuleCollider capsule = capsuleObj.AddComponent<CapsuleCollider>();
        if (physicsMaterial != null) capsule.sharedMaterial = physicsMaterial;

        Vector3 direction = targetPosition - parentTransform.position;
        float distance = direction.magnitude;

        // Position collider between the parent and the new bone
        capsuleObj.transform.position = parentTransform.position + (direction * 0.5f);
        capsuleObj.transform.rotation = Quaternion.LookRotation(direction);

        // Adjust dimensions (direction = 2 -> Make the capsule be horizontally aligned, instead of vertically)
        capsule.direction = 2;
        capsule.radius = fingerRadius;
        // Set capsule height to fit the bone distance
        capsule.height = distance + (capsule.radius * 2);
    }
}