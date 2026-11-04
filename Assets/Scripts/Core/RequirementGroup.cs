using System;
using System.Collections.Generic;
using System.Linq;

public enum GroupMode { All, Any }

public class RequirementGroup : IRequirement
{
    private readonly List<IRequirement> requirements;
    private readonly GroupMode mode;
    private bool lastValue;
    private bool listening;

    public event Action Changed;

    public RequirementGroup(IEnumerable<IRequirement> requirements, GroupMode mode)
    {
        this.requirements = new List<IRequirement>(requirements);
        this.mode = mode;
        lastValue = Evaluate();
    }

    public bool IsMet => Evaluate();

    public void StartListening()
    {
        if (listening) return;
        foreach (var r in requirements) r.Changed += OnChildChanged;
        listening = true;
        lastValue = Evaluate();
    }

    public void StopListening()
    {
        if (!listening) return;
        foreach (var r in requirements) r.Changed -= OnChildChanged;
        listening = false;
    }

    private bool Evaluate() =>
        mode == GroupMode.All ? requirements.All(r => r.IsMet) : requirements.Any(r => r.IsMet);

    private void OnChildChanged()
    {
        bool value = Evaluate();
        if (value == lastValue) return;   // só avisa se o resultado final mudou
        lastValue = value;
        Changed?.Invoke();
    }
}