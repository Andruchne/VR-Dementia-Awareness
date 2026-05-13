using System.Collections.Generic;
using UnityEngine;

public class ExpressionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer targetMesh;

    [Header("Blendshape Indices")]
    [SerializeField] private int happyFullFaceIndex = 0;
    [SerializeField] private int happyIndex = 16;
    [SerializeField] private int sadIndex = 17;
    [SerializeField] private int confusedIndex = 18;

    [Header("Emotion & Mood Settings")]
    [SerializeField] private float emotionTransitionTime = 0.5f;
    [SerializeField] private float moodTransitionTime = 1.0f;

    private VoiceInteractionManager _voiceManager;
    private string _currentEmotion = "neutral";
    private string _lastEmotion = "neutral";
    private bool _isCurrentlyNostalgic = false;

    private float _currentHappyFullFaceWeight = 0f;
    private float _currentHappyWeight = 0f;
    private float _currentSadWeight = 0f;
    private float _currentConfusedWeight = 0f;

    private float _moodNeutral = 100f;
    private float _moodHappy = 0f;
    private float _moodSad = 0f;
    private float _moodNostalgic = 0f;
    private float _moodFurious = 0f;
    private float _moodAnxious = 0f;

    private void Start()
    {
        _voiceManager = GameManager.Instance.VoiceInterManager;
    }

    private void Update()
    {
        if (targetMesh == null) return;

        HandleEmotions();
        UpdatePostProcessingMoods();
    }

    private void HandleEmotions()
    {
        if (_voiceManager == null) return;

        bool isSpeaking = _voiceManager.IsSpeaking;

        if (isSpeaking)
        {
            float currentTime = _voiceManager.GetCurrentDialogueTime();
            var timeline = _voiceManager.CurrentEmotionTimeline;

            string evaluatedEmotion = "neutral";
            bool evaluatedNostalgic = false;

            for (int i = timeline.Count - 1; i >= 0; i--)
            {
                if (currentTime >= timeline[i].startTime)
                {
                    evaluatedEmotion = timeline[i].emotion;
                    evaluatedNostalgic = timeline[i].isNostalgic;
                    break;
                }
            }

            if (evaluatedEmotion != _currentEmotion || evaluatedNostalgic != _isCurrentlyNostalgic)
            {
                _currentEmotion = evaluatedEmotion;
                _lastEmotion = _currentEmotion;
                _isCurrentlyNostalgic = evaluatedNostalgic;
            }
        }
        else
        {
            _currentEmotion = _lastEmotion;
        }

        float targetHappyFull = 0f;
        float targetHappy = 0f;
        float targetSad = 0f;
        float targetConfused = 0f;

        switch (_currentEmotion)
        {
            case "happy":
                if (isSpeaking) targetHappy = 100f;
                else targetHappyFull = 100f;
                break;
            case "sad":
                targetSad = 100f;
                break;
            case "fearful":
            case "angry":
                targetConfused = 100f;
                break;
        }

        float emotionSpeed = 100f / emotionTransitionTime;

        _currentHappyFullFaceWeight = Mathf.MoveTowards(_currentHappyFullFaceWeight, targetHappyFull, emotionSpeed * Time.deltaTime);
        _currentHappyWeight = Mathf.MoveTowards(_currentHappyWeight, targetHappy, emotionSpeed * Time.deltaTime);
        _currentSadWeight = Mathf.MoveTowards(_currentSadWeight, targetSad, emotionSpeed * Time.deltaTime);
        _currentConfusedWeight = Mathf.MoveTowards(_currentConfusedWeight, targetConfused, emotionSpeed * Time.deltaTime);

        targetMesh.SetBlendShapeWeight(happyFullFaceIndex, _currentHappyFullFaceWeight);
        targetMesh.SetBlendShapeWeight(happyIndex, _currentHappyWeight);
        targetMesh.SetBlendShapeWeight(sadIndex, _currentSadWeight);
        targetMesh.SetBlendShapeWeight(confusedIndex, _currentConfusedWeight);
    }

    private void UpdatePostProcessingMoods()
    {
        float targetNeutral = 0f;
        float targetHappy = 0f;
        float targetSad = 0f;
        float targetNostalgic = 0f;
        float targetFurious = 0f;
        float targetAnxious = 0f;

        if (_isCurrentlyNostalgic)
        {
            targetNostalgic = 100f;
        }
        else
        {
            switch (_currentEmotion)
            {
                case "happy": targetHappy = 100f; break;
                case "sad": targetSad = 100f; break;
                case "angry": targetFurious = 100f; break;
                case "fearful": targetAnxious = 100f; break;
                default: targetNeutral = 100f; break;
            }
        }

        float moodSpeed = 100f / moodTransitionTime;

        UpdateSingleMood(Mood.Neutral, ref _moodNeutral, targetNeutral, moodSpeed);
        UpdateSingleMood(Mood.Happy, ref _moodHappy, targetHappy, moodSpeed);
        UpdateSingleMood(Mood.Sad, ref _moodSad, targetSad, moodSpeed);
        UpdateSingleMood(Mood.Nostalgic, ref _moodNostalgic, targetNostalgic, moodSpeed);
        UpdateSingleMood(Mood.Furious, ref _moodFurious, targetFurious, moodSpeed);
        UpdateSingleMood(Mood.Anxious, ref _moodAnxious, targetAnxious, moodSpeed);
    }

    private void UpdateSingleMood(Mood mood, ref float currentValue, float targetValue, float speed)
    {
        if (Mathf.Approximately(currentValue, targetValue)) return;

        int oldInt = Mathf.RoundToInt(currentValue);
        currentValue = Mathf.MoveTowards(currentValue, targetValue, speed * Time.deltaTime);
        int newInt = Mathf.RoundToInt(currentValue);

        if (oldInt != newInt)
        {
            GameManager.Instance.SetMoodPercentage(mood, newInt);
        }
    }
}