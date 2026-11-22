using UnityEngine;
using System;
using Unity.VisualScripting;

public class PowercellDisplay : MonoBehaviour
{
    [SerializeField] private Material OnMaterial;
    [SerializeField] private Material OffMaterial;
    [SerializeField] private bool IsPowered;
    private void Awake()
    {
        this.gameObject.GetComponent<MeshRenderer>().material = OnMaterial;
        SwitchEnergyState(IsPowered);
    }
    private void OnEnable()
    {
        //OnEnergySwitch += SwitchEnergyState;
    }
    private void OnDisable()
    {
        //OnEnergySwitch -= SwitchEnergyState;
    }
    private void SwitchEnergyState(bool IsOn) 
    {
        this.gameObject.GetComponent<MeshRenderer>().material = IsOn ? OnMaterial : OffMaterial;
    }
}
