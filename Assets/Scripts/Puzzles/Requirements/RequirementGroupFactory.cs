using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constrói um RequirementGroup a partir de componentes arrastados no Inspector.
/// Evita repetir em cada script o mesmo ciclo de validação (a Door faz o mesmo no Awake).
/// </summary>
public static class RequirementGroupFactory
{
    public static RequirementGroup Create(MonoBehaviour[] sources, GroupMode mode, Object context)
    {
        var list = new List<IRequirement>();

        if (sources != null)
        {
            foreach (MonoBehaviour source in sources)
            {
                IRequirement requirement = source != null ? source.GetComponent<IRequirement>() : null;
                if (requirement == null)
                {
                    Debug.LogError($"{context.name}: um ou mais requisitos estão vazios ou não têm IRequirement.", context);
                    continue;
                }
                list.Add(requirement);
            }
        }

        if (list.Count == 0)
            Debug.LogWarning($"{context.name}: sem requisitos válidos.", context);

        return new RequirementGroup(list, mode);
    }
}
