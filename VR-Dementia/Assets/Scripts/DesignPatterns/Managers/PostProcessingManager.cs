using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    public VolumeCollection volumeCollection;

    [HideInInspector]
    public List<VolumeConfig> activeConfigs = new List<VolumeConfig>();

    private Dictionary<Mood, Volume> _volumeInstances = new Dictionary<Mood, Volume>();
    private Coroutine _currentTransition;

    private bool _requiresUpdate = true;

    private void Start()
    {
        if (volumeCollection == null) 
        {
            Debug.LogWarning("PostProcessingManager: Missing VolumeCollection Asset.");
            return; 
        }

        for (int i = 0; i < volumeCollection.entries.Count; i++)
        {
            // Get and check whether volume entry holds a GameObject
            VolumeEntry entry = volumeCollection.entries[i];
            if (entry.volume == null) { continue; }

            // Instantiate Volume as child from Manager Transform
            GameObject gameObject = Instantiate(entry.volume, Vector3.zero, Quaternion.identity, this.transform);
            gameObject.name = $"Volume_{entry.mood}";

            Volume volComponent = gameObject.GetComponent<Volume>();
            volComponent.priority = i;

            _volumeInstances.Add(entry.mood, volComponent);
        }

        InitializeActiveConfigs();
    }

    // Gets called when inspector values change inside the editor
    // The bool is used to update the weights of the volumes only upon change
    private void OnValidate()
    {
        SetDirty();
    }

    private void Update()
    {
        // Change volumes if inspector changes were made
        if (_requiresUpdate)
        {
            EvaluateWeights();
            _requiresUpdate = false;
        }
    }

    public void SetDirty()
    {
        _requiresUpdate = true;
    }

    public void InitializeActiveConfigs()
    {
        foreach (KeyValuePair<Mood, Volume> kvp in _volumeInstances)
        {
            // Add missing mood entries to the list, to prevent null reference issues
            Mood m = kvp.Key;
            if (!activeConfigs.Any(x => x.mood == m))
            {
                activeConfigs.Add(new VolumeConfig { mood = m, volumePercentage = 0 });
            }
        }
        // Sort volumes inside the list, to also fit the enum order of definition (to stay consistent)
        activeConfigs.Sort((a, b) => a.mood.CompareTo(b.mood));

        SetDirty();
    }

    private void EvaluateWeights()
    {
        // Calculate the total sum of all weight percentages
        float totalUserPercent = 0;
        foreach (VolumeConfig cfg in activeConfigs)
        {
            totalUserPercent += cfg.volumePercentage;
        }

        // Determine the scale factor
        // This is important, since the weights need to be distrubited accordingly:
        // > 100 = Weights need to be recalculated to fit the available space with their individual proportions
        // <= 100 = Weights stay the same, and the rest of the available percent stays empty
        float normalizationFactor = 1;
        if (totalUserPercent > 100)
        {
            normalizationFactor = 100 / totalUserPercent;
        }

        // To keep track of how much of the available space is already covered
        float currentCoverage = 0;

        // Start with the entry with the highest priority setting, up to the one with the lowest
        for (int i = volumeCollection.entries.Count - 1; i >= 0; i--)
        {
            Mood mood = volumeCollection.entries[i].mood;

            // Check whether the mood holds a volume instance - skip if not available
            if (!_volumeInstances.TryGetValue(mood, out Volume vol)) { continue; }

            // Get related config reference and obtain percentage (set to 0 if not available)
            VolumeConfig config = activeConfigs.Find(x => x.mood == mood);
            float userPercent = config != null ? config.volumePercentage : 0;

            // Safety Check: If Volume not high enough to notice in the first place - turn it off
            if (userPercent <= 0.01f)
            {
                vol.weight = 0;
                continue;
            }

            // Calculate the percent to cover the space (value from 0 to 1), and the remaining space for the next iteration
            float targetShare = (userPercent * normalizationFactor) / 100;
            float remainingSpace = 1 - currentCoverage;

            // Turn it off, if no space is practically left for it anyways
            if (remainingSpace <= 0.001f)
            {
                vol.weight = 0;
                continue;
            }

            // Calculate the actual weight to use for the volume
            float calculatedWeight = targetShare / remainingSpace;
            vol.weight = Mathf.Clamp01(calculatedWeight);

            // Update current coverage for next iteration
            currentCoverage += targetShare;
        }
    }

    public void SwitchMood(VolumeConfiguration targetConfig, float transitionTime)
    {
        // Stop transition, if one is already playing
        if (_currentTransition != null) StopCoroutine(_currentTransition);
        _currentTransition = StartCoroutine(TransitionMoodRoutine(targetConfig, transitionTime));
    }

    private IEnumerator TransitionMoodRoutine(VolumeConfiguration targetConfig, float duration)
    {
        // Make a copy of the current values as point of reference
        List<VolumeConfig> startValues = new List<VolumeConfig>();
        foreach (VolumeConfig current in activeConfigs)
        {
            startValues.Add(new VolumeConfig { mood = current.mood, volumePercentage = current.volumePercentage });
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            foreach (VolumeConfig activeCfg in activeConfigs)
            {
                // Get start value for the mood
                VolumeConfig startItem = startValues.Find(x => x.mood == activeCfg.mood);
                int startVal = startItem != null ? startItem.volumePercentage : 0;

                // Get target value for mood
                VolumeConfig targetItem = targetConfig.configs.Find(x => x.mood == activeCfg.mood);
                int targetVal = targetItem != null ? targetItem.volumePercentage : 0;

                // Set the resulting percent, based on the time passed
                activeCfg.volumePercentage = (int)Mathf.Lerp(startVal, targetVal, t);
            }

            EvaluateWeights();
            yield return null;
        }

        // Make sure the value are exactly the ones they should be
        foreach (VolumeConfig activeCfg in activeConfigs)
        {
            VolumeConfig targetItem = targetConfig.configs.Find(x => x.mood == activeCfg.mood);
            activeCfg.volumePercentage = targetItem != null ? targetItem.volumePercentage : 0;
        }

        EvaluateWeights();
        _currentTransition = null;
    }
}