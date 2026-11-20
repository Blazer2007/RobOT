using System;
using System.Collections.Generic;
using System.Linq;

// Enumerador que armazena o modo de verificação dos requisitos
public enum GroupMode { All, Any }

public class RequirementGroup : IRequirement
{
    private readonly List<IRequirement> requirements; // Lista de todos os requisitos(objetos com a interface "IRequirement")
    private readonly GroupMode mode; // Referencia ao enum criado acima
    private bool lastValue; // Ultimo valor registado no Evaluate(true se se cumprir(em) o(s) requisito(s)|Falso se não)
    // Usado para fazer verificações e impedir repetições de métodos
    private bool listening; // Se está á espera de ativações(se precisar que os requisitos sejam cumpridos)

    public event Action Changed; // o requisito mudou

    // Classe public para armazenamento de requisitos
    public RequirementGroup(IEnumerable<IRequirement> requirements, GroupMode mode)
    {
        this.requirements = new List<IRequirement>(requirements); // Armazena  uma lista de requisitos()
        this.mode = mode; // Armazena o modo de verificação(All ou Any)
        lastValue = Evaluate(); // Define o valor de partida dependendo das condições
    }

    public bool IsMet => Evaluate(); // Avalia se todos os requisitos estão cumpridos e associa o valor true/false respetivo

    /// <summary>
    /// Ouvir mudanças nos requisitos(se são ativos)
    /// </summary>
    public void StartListening() 
    {
        if (listening) return; // Se já estiver a ouvir deixa de executar este metodo(para não repetir subscrições)
        foreach (var r in requirements) r.Changed += OnChildChanged; // subscreve-se a mudanças dos componentes
        listening = true; // garante que o metodo não se repete
        lastValue = Evaluate(); // 
    }

    public void StopListening() // O contrário do startListening
    {
        if (!listening) return; // Se já não está a ouvir não precisa de dessubrescrever
        foreach (var r in requirements) r.Changed -= OnChildChanged; // deixa de ouvir os componentes
        listening = false; // Garante que não se repete o metodo
    }

    //Dependendo do modo escolhido para o objeto, verifica se o(s) requisito(s) estão cumpridos
    private bool Evaluate() =>
        mode == GroupMode.All ? requirements.All(r => r.IsMet) : requirements.Any(r => r.IsMet);
    //  Se for true-> Verifica se todos os requisitos, | Se for false-> Verifica se algum dos requirements
    //  cumprem a condição IsMet(se todos estão ativos)| cumpre a condição IsMet(se algum está ativo)

    //Chamado quando um componente muda(fica ativado/desativado)
    private void OnChildChanged()
    {
        bool value = Evaluate(); // Guarda true/false dependendo da revisão anterior
        if (value == lastValue) return;   // só avisa se o resultado final mudou
        lastValue = value; // Se mudou, o ultimo valor passa a ser o revisto no metodo "Evaluate"
        Changed?.Invoke(); // indica que foi feita uma mudança no grupo
    }
}