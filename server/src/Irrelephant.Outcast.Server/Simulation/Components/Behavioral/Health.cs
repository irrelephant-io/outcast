namespace Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

public struct Health
{
    public int MaxHealth;
    public int CurrentHealth;

    public bool IsAlive() => CurrentHealth > 0;
}
