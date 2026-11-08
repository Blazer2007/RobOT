// Door.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Door : MonoBehaviour, IResetlable
{
    [SerializeField] private MonoBehaviour[] requirementSources;
    [SerializeField] private GroupMode mode = GroupMode.All;
    [SerializeField] private bool stayOpen;
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);
    [SerializeField] private float speed = 3f;

    private Rigidbody body;
    private RequirementGroup group;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool shouldBeOpen;

    public bool IsOpen => shouldBeOpen;
    public event Action<bool> OpenStateChanged;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        closedPosition = transform.position;
        openPosition = closedPosition + transform.rotation * openOffset;

        var list = new List<IRequirement>();
        foreach (var source in requirementSources)
        {
            IRequirement requirement = source != null ? source.GetComponent<IRequirement>() : null;
            if (requirement == null)
            {
                Debug.LogError($"{name}: um dos requisitos está vazio ou não tem IRequirement.", this);
                continue;
            }
            list.Add(requirement);
        }

        if (list.Count == 0)
            Debug.LogWarning($"{name}: sem requisitos válidos.", this);

        group = new RequirementGroup(list, mode);
    }

    private void OnEnable()
    {
        group.StartListening();
        group.Changed += OnRequirementsChanged;

        shouldBeOpen = group.IsMet;                                   // estado inicial, sem animação
        transform.position = shouldBeOpen ? openPosition : closedPosition;
    }

    private void OnDisable()
    {
        group.Changed -= OnRequirementsChanged;
        group.StopListening();
    }

    private void OnRequirementsChanged()
    {
        if (stayOpen && shouldBeOpen) return;
        SetOpen(group.IsMet);
    }
    public void ResetState()
    {
        shouldBeOpen = group.IsMet;   // volta a perguntar aos requisitos, não assume nada
        Vector3 position = shouldBeOpen ? openPosition : closedPosition;

        transform.position = position;
        body.position = position;
        Physics.SyncTransforms();
    }
    private void SetOpen(bool open)
    {
        if (open == shouldBeOpen) return;
        shouldBeOpen = open;
        OpenStateChanged?.Invoke(open);
    }

    private void FixedUpdate()
    {
        Vector3 target = shouldBeOpen ? openPosition : closedPosition;
        if ((body.position - target).sqrMagnitude < 0.000001f) return;

        body.MovePosition(Vector3.MoveTowards(body.position, target, speed * Time.fixedDeltaTime));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 end = transform.position + transform.rotation * openOffset;
        Gizmos.DrawLine(transform.position, end);
        Gizmos.DrawWireSphere(end, 0.15f);
    }
}