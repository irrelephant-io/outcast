namespace Irrelephant.Outcast.Server.Simulation.Components.Data;

public struct StateMachine<TStateEnum>
{
    public TStateEnum Current;

    public TStateEnum Previous;

    public bool DidStateChange;

    public void GoToState(TStateEnum state)
    {
        Previous = Current;
        Current = state;
        DidStateChange = true;
    }

    public void ClearStateChange()
    {
        DidStateChange = false;
    }
}
