namespace Irrelephant.Outcast.Server.Simulation.Diagnostics;

public class MovingAverageFloat(int windowSize = 128)
{
    private float _rollingSum;
    private readonly Queue<float> _samples = [];

    public float? Average =>
        _samples.Count > 0
            ? _rollingSum / _samples.Count
            : 0;

    public void Sample(float value)
    {
        if (_samples.Count == windowSize)
        {
            var firstValue = _samples.Dequeue();
            _rollingSum -= firstValue;
        }
        _rollingSum += value;
        _samples.Enqueue(value);
    }
}
