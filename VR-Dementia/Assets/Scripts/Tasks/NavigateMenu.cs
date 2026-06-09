using UnityEngine;

public class NavigateMenu : SimulationTask
{
    void Start()
    {
        // Deactivate handheld menu - it will be activated during InputChecker activeness
        EventBus<OnChangePalmMenuActive>.Publish(new OnChangePalmMenuActive(false));
    }

    private void OnDestroy()
    {
        EventBus<OnStartSimulation>.OnEvent -= SimulationStarted;
    }

    public override void StartTask()
    {
        base.StartTask();
        EventBus<OnStartSimulation>.OnEvent += SimulationStarted;
    }

    private void SimulationStarted(OnStartSimulation evt)
    {
        EventBus<OnStartSimulation>.OnEvent -= SimulationStarted;
        FinishTask();
    }
}
