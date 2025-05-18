namespace Irrelephant.Outcast.Server.Simulation.Components.Data;

public struct StateMachine<TStateEnum> where TStateEnum : Enum
{
    public TStateEnum Current;

    public TStateEnum Previous;

    public bool DidStateChange;

    private int _ticksLeft;

    public bool EnterTick() => _ticksLeft-- > 0;

    public void GoToState(TStateEnum state)
    {
        _ticksLeft++;
        Previous = Current;
        Current = state;
        DidStateChange = DidStateChange || !Previous.Equals(state);
    }

    public void ClearStateChange()
    {
        _ticksLeft = 1;
        DidStateChange = false;
    }
}
