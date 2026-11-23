
// Estado do alvo que a habilidade tem na mira (a mira do ecrã reage a isto)
public enum AbilityTargetState
{
    None,      // nada utilizável na mira
    Available, // alvo válido: clicar funciona
    Blocked,   // alvo reconhecido mas impossível (ex.: metal pesado demais)
    Active     // habilidade em uso (ex.: a segurar uma caixa)
}
/// <summary>
/// Interface pequena e opcional (ISP): só as habilidades que têm alvo a implementam.
/// Sem ela, o ModuleController usa CanUse() para decidir entre None e Available.
/// </summary>
public interface ITargetFeedback
{
    AbilityTargetState GetTargetState(AbilityContext context);
}