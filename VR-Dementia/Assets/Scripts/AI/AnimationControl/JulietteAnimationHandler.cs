using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JulietteAnimationHandler : MonoBehaviour
{
    [SerializeField] private JulietteAnimationInfo[] julietteAnimInfo;
    [SerializeField] private Animator doorAnim;

    private Dictionary<JulietteAnimations, GameObject> animInfo = new Dictionary<JulietteAnimations, GameObject>();
    private Animator julietteAnim;
    private Queue<AnimationQueueItem> animationQueue = new Queue<AnimationQueueItem>();
    private bool isPlayingQueue = false;

    private void Start()
    {
        julietteAnim = GetComponent<Animator>();

        EventBus<OnOpenDoorAnim>.OnEvent += OpenDoorAnim;
        EventBus<OnWalkAnim>.OnEvent += WalkAnim;
        EventBus<OnSitAnim>.OnEvent += SitAnim;
        EventBus<OnIdleAnim>.OnEvent += IdleAnim;

        if (julietteAnimInfo != null)
        {
            for (int i = 0; i < julietteAnimInfo.Length; i++)
            {
                if (julietteAnimInfo[i].startPosition != null)
                {
                    animInfo.Add(julietteAnimInfo[i].animationType, julietteAnimInfo[i].startPosition);
                }
            }
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
        QueueAnimation("OpenDoor", JulietteAnimations.OpenDoor);
    }

    private void WalkAnim(OnWalkAnim evt)
    {
        QueueAnimation("Walk", JulietteAnimations.Walk);
    }

    private void SitAnim(OnSitAnim evt)
    {
        QueueAnimation("SitDown", JulietteAnimations.Sit);
    }

    private void IdleAnim(OnIdleAnim evt)
    {
        QueueAnimation("Idle", JulietteAnimations.IdleStand);
    }

    private void QueueAnimation(string triggerName, JulietteAnimations animType)
    {
        animationQueue.Enqueue(new AnimationQueueItem(triggerName, animType));

        if (!isPlayingQueue)
        {
            StartCoroutine(ExecuteQueue());
        }
    }

    private IEnumerator ExecuteQueue()
    {
        isPlayingQueue = true;

        while (animationQueue.Count > 0)
        {
            AnimationQueueItem current = animationQueue.Dequeue();

            if (animInfo.ContainsKey(current.animType) && animInfo[current.animType] != null)
            {
                transform.position = animInfo[current.animType].transform.position;
                transform.rotation = animInfo[current.animType].transform.rotation;
            }

            if (julietteAnim != null)
            {
                yield return StartCoroutine(WaitForCurrentAnimation(current.animType, current.triggerName));
            }
        }

        isPlayingQueue = false;
    }

    private IEnumerator WaitForCurrentAnimation(JulietteAnimations animType, string triggerName)
    {
        if (julietteAnim == null) yield break;

        AnimatorStateInfo initialState = julietteAnim.GetCurrentAnimatorStateInfo(0);
        julietteAnim.SetTrigger(triggerName);

        // Ugly, but will do for now ;)
        if (triggerName == "OpenDoor") { doorAnim.SetTrigger(triggerName); }

        while (julietteAnim.GetCurrentAnimatorStateInfo(0).fullPathHash == initialState.fullPathHash && !julietteAnim.IsInTransition(0))
        {
            yield return null;
        }

        while (julietteAnim.IsInTransition(0))
        {
            yield return null;
        }

        while (julietteAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
        {
            yield return null;
        }

        EventBus<OnJulietteAnimFinished>.Publish(new OnJulietteAnimFinished(animType));
    }
}