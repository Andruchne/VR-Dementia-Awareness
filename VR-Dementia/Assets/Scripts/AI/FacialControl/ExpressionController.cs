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

    [Header("Breathing Settings")]
    [SerializeField] private SkinnedMeshRenderer breathingMesh;
    [SerializeField] private int breathingIndex = 0;
    [SerializeField] private float minIntensity = 65f;
    [SerializeField] private float maxIntensity = 80f;
    [SerializeField] private float minBreathDuration = 1.4f;
    [SerializeField] private float maxBreathDuration = 2.2f;
    [SerializeField] private float minRestDuration = 0.3f;
    [SerializeField] private float maxRestDuration = 0.7f;

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

    private float _breathingTimer = 0f;
    private float _currentBreathingDuration = 1.5f;
    private float _currentBreathingWeight = 0f;
    private float _startBreathingWeight = 0f;
    private float _targetBreathingWeight = 0f;
    private int _breathingState = 0;

    private void Start()
    {
        _voiceManager = GameManager.Instance.VoiceInterManager;

        _currentBreathingDuration = Random.Range(minBreathDuration, maxBreathDuration);
        _targetBreathingWeight = Random.Range(minIntensity, maxIntensity);
    }

    private void Update()
    {
        HandleBreathing();

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

    private void HandleBreathing()
    {
        if (breathingMesh == null) return;

        _breathingTimer += Time.deltaTime;

        // Switch to next breathing state
        if (_breathingTimer >= _currentBreathingDuration)
        {
            _breathingTimer = 0f;
            _startBreathingWeight = _currentBreathingWeight;
            _breathingState = (_breathingState + 1) % 3;

            switch (_breathingState)
            {
                case 0:
                    _currentBreathingDuration = Random.Range(minBreathDuration, maxBreathDuration);
                    _targetBreathingWeight = Random.Range(minIntensity, maxIntensity);
                    break;
                case 1:
                    _currentBreathingDuration = Random.Range(minBreathDuration, maxBreathDuration);
                    _targetBreathingWeight = 0f;
                    break;
                case 2:
                    _currentBreathingDuration = Random.Range(minRestDuration, maxRestDuration);
                    _targetBreathingWeight = 0f;
                    break;
            }
        }

        // Calculate smooth weight
        float progress = _breathingTimer / _currentBreathingDuration;
        _currentBreathingWeight = Mathf.Lerp(_startBreathingWeight, _targetBreathingWeight, Mathf.SmoothStep(0f, 1f, progress));

        breathingMesh.SetBlendShapeWeight(breathingIndex, _currentBreathingWeight);
    }
}