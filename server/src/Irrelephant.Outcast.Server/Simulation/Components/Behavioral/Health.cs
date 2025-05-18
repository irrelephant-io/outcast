namespace Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

public struct Health
{
    public int MaxHealth;
    public int CurrentHealth;

    public int HealthRecoveryPerRegenerationTick;
    public int RegenerationCooldown;
    public int RegenerationCooldownRemaining;

    public bool IsAlive() => CurrentHealth > 0;

    public int PercentHealth => Math.Max(0, CurrentHealth * 100 / MaxHealth);
    public bool HealthChangedThisTick;

    public void AddHealth(int amount)
    {
        var previousHealth = CurrentHealth;
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        HealthChangedThisTick = previousHealth != CurrentHealth;
    }
}
