using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JulietteAnimationHandler : MonoBehaviour
{
    [SerializeField] JulietteAnimationInfo[] julietteAnimInfo;

    Dictionary<JulietteAnimations, GameObject> animInfo;
    Animator julietteAnim;

    private void Start()
    {
        EventBus<OnOpenDoorAnim>.OnEvent += OpenDoorAnim;
        EventBus<OnWalkAnim>.OnEvent += WalkAnim;
        EventBus<OnSitAnim>.OnEvent += SitAnim;
        EventBus<OnIdleAnim>.OnEvent += IdleAnim;

        for (int i = 0; i < julietteAnimInfo.Length; i++)
        {
            animInfo.Add(julietteAnimInfo[i].animationType, julietteAnimInfo[i].startPosition);
        }
    }

    private void OnDestroy()
    {
        EventBus<OnOpenDoorAnim>.OnEvent -= OpenDoorAnim;
        EventBus<OnWalkAnim>.OnEvent -= WalkAnim;
        EventBus<OnSitAnim>.OnEvent -= SitAnim;
        EventBus<OnIdleAnim>.OnEvent -= IdleAnim;
    }

    private void OpenDoorAnim(OnOpenDoorAnim evt)
    {
        julietteAnim.SetTrigger("OpenDoor");
        StartCoroutine(WaitForCurrentAnimation());

        if (animInfo[JulietteAnimations.OpenDoor] == null) { return; }

        transform.position = animInfo[JulietteAnimations.OpenDoor].transform.position;
        transform.rotation = animInfo[JulietteAnimations.OpenDoor].transform.rotation;
    }

    private void WalkAnim(OnWalkAnim evt)
    {
        julietteAnim.SetTrigger("Walk");
        StartCoroutine(WaitForCurrentAnimation());

        if (animInfo[JulietteAnimations.Walk] == null) { return; }

        transform.position = animInfo[JulietteAnimations.Walk].transform.position;
        transform.rotation = animInfo[JulietteAnimations.Walk].transform.rotation;
    }

    private void SitAnim(OnSitAnim evt)
    {
        julietteAnim.SetTrigger("SitDown");
        StartCoroutine(WaitForCurrentAnimation());

        if (animInfo[JulietteAnimations.Sit] == null) { return; }

        transform.position = animInfo[JulietteAnimations.Sit].transform.position;
        transform.rotation = animInfo[JulietteAnimations.Sit].transform.rotation;
    }

    private void IdleAnim(OnIdleAnim evt)
    {
        julietteAnim.SetTrigger("Idle");
        StartCoroutine(WaitForCurrentAnimation());

        if (animInfo[JulietteAnimations.IdleStand] == null) { return; }

        transform.position = animInfo[JulietteAnimations.IdleStand].transform.position;
        transform.rotation = animInfo[JulietteAnimations.IdleStand].transform.rotation;
    }

    Animator animator;

    private IEnumerator WaitForCurrentAnimation()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        EventBus<OnJulietteAnimFinished>.Publish(new OnJulietteAnimFinished());
    }
}
