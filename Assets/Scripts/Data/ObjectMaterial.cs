using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Descreve um material (cartão, madeira, ferro...). Só dados de configuração:
/// a massa de cada objeto é calculada por instância (densidade x volume) no MaterialObject,
/// por isso este asset, partilhado por todas as caixas, nunca é alterado em runtime.
/// </summary> 
[CreateAssetMenu(menuName = "RobOT/Object Material", fileName = "ObjectMaterial")]
public class ObjectMaterial : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; } = "Metal";

    // Densidade na escala do jogo (não é a real). Referência: cartão ~10, madeira ~40, ferro ~100.
    // Uma caixa de meio metro passa a pesar +-1, 5 e 12,5 kg.
    [field: SerializeField] public float Density { get; private set; } = 100f;

    // Só materiais magnéticos são atraídos pelo braço magnético
    [field: SerializeField] public bool IsMagnetic { get; private set; } = true;
    [field: SerializeField] public Material MaterialVisual { get; private set; }
}