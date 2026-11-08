using UnityEngine;

// Troca a personagem controlada (Rob <-> O.T) com uma tecla
// e põe a câmara a seguir a personagem ativa
public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField] private ControllableCharacter[] characters; // ex.: [Rob, O.T]
    [SerializeField] private HybridCameraController cameraController;
    [SerializeField] private KeyCode switchKey = KeyCode.Tab;

    private int currentIndex = -1;

    public ControllableCharacter Current => currentIndex >= 0 ? characters[currentIndex] : null;

    private void Start()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError($"{name}: a lista Characters está vazia.", this);
            enabled = false;
            return;
        }
        if (cameraController == null)
            Debug.LogWarning($"{name}: falta o Camera Controller; a câmara não vai mudar de personagem.", this);

        // Começa na personagem marcada com "Start Controlled" (ou na primeira, se nenhuma estiver)
        int start = System.Array.FindIndex(characters, c => c != null && c.IsControlled);
        SwitchTo(start >= 0 ? start : 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey)) SwitchToNext();
    }

    public void SwitchToNext()
    {
        // Procura a próxima personagem disponível, dando a volta à lista
        for (int step = 1; step < characters.Length; step++)
        {
            int index = (currentIndex + step) % characters.Length;
            if (IsAvailable(characters[index]))
            {
                SwitchTo(index);
                return;
            }
        }
        Debug.Log($"{name}: não há outra personagem disponível (o O.T já foi ligado no Terminal?).", this);
    }

    private void SwitchTo(int index)
    {
        if (index < 0 || index >= characters.Length || characters[index] == null) return;

        // Só uma personagem pode estar controlada de cada vez
        for (int i = 0; i < characters.Length; i++)
            if (i != index && characters[i] != null) characters[i].LoseControl();

        currentIndex = index;
        characters[index].GainControl();

        if (cameraController != null) cameraController.SetTarget(characters[index].transform);
        Debug.Log($"{name}: a controlar {characters[index].name}.", this);
    }

    // O O.T só pode ser controlado depois de ser ligado (RobotPowerUp)
    private static bool IsAvailable(ControllableCharacter character)
    {
        if (character == null || !character.isActiveAndEnabled) return false;
        return !character.TryGetComponent(out RobotPowerUp powerUp) || powerUp.IsPowered;
    }
}
