using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

public class Mood_Management_Script : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider M_Anxious_Slider;
    public Slider M_Furious_Slider;
    public Slider M_Happy_Slider;
    public Slider M_Nostalgic_Slider;
    public Slider M_Sad_Slider;
    [Header("FMOD Event")]
    public FMODUnity.EventReference musicEvent;

    [Header("Parameter Names (FMOD)")]
    public string param1Name = "M_Anxious";
    public string param2Name = "M_Furious";
    public string param3Name = "M_Happy";
    public string param4Name = "M_Nostalgic";
    public string param5Name = "M_Sad";

    private FMOD.Studio.EventInstance musicInstance;

    void OnDestroy()
    {
        // Clean up FMOD instance
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }

    void Start()
    {
        // Create and start FMOD event
        musicInstance = FMODUnity.RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    void Update()
    {
        // Update FMOD parameters based on slider values
        musicInstance.setParameterByName(param1Name, M_Anxious_Slider.value);
        musicInstance.setParameterByName(param2Name, M_Furious_Slider.value);
        musicInstance.setParameterByName(param3Name, M_Happy_Slider.value);
        musicInstance.setParameterByName(param4Name, M_Nostalgic_Slider.value);
        musicInstance.setParameterByName(param5Name, M_Sad_Slider.value);

    }
}
