using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Ferramenta de editor: põe a música de elevador na cabine (o objeto com o ElevatorMover) da cena aberta,
// já com o efeito "caixinha de som" (filtro passa-banda + um pouco de distorção + eco de sala).
// Menu: RobOT > Adicionar música ao elevador
// Tudo fica como componentes normais no Inspector, por isso dá para afinar à mão depois.
public static class AddElevatorMusic
{
    private const string ClipPath = "Assets/Sounds/OtherThings/Elevator/ikoliks_aj-elevator-elevator-music-641092.mp3";

    [MenuItem("RobOT/Adicionar música ao elevador")]
    public static void Add()
    {
        var elevator = Object.FindFirstObjectByType<ElevatorMover>();
        if (elevator == null)
        {
            EditorUtility.DisplayDialog("Música do elevador", "Não encontrei nenhum ElevatorMover na cena aberta.", "OK");
            return;
        }

        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
        if (clip == null)
        {
            EditorUtility.DisplayDialog("Música do elevador", $"Não encontrei a música em:\n{ClipPath}", "OK");
            return;
        }

        GameObject go = elevator.gameObject;
        if (go.GetComponent<AudioSource>() != null)
        {
            EditorUtility.DisplayDialog("Música do elevador", $"O '{go.name}' já tem uma AudioSource. Não mexi em nada.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Adicionar música ao elevador");
        int group = Undo.GetCurrentGroup();

        // Fonte: em loop, sempre a tocar, 3D (só se ouve perto do elevador)
        var source = Undo.AddComponent<AudioSource>(go);
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = true;
        source.volume = 0.5f;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 2f;
        source.maxDistance = 15f;

        // Corta os graves...
        var highPass = Undo.AddComponent<AudioHighPassFilter>(go);
        highPass.cutoffFrequency = 450f;
        highPass.highpassResonanceQ = 1f;

        // ...e os agudos: só ficam os médios, como num altifalante pequeno
        var lowPass = Undo.AddComponent<AudioLowPassFilter>(go);
        lowPass.cutoffFrequency = 3500f;
        lowPass.lowpassResonanceQ = 1f;

        // Um bocadinho de "altifalante barato"
        var distortion = Undo.AddComponent<AudioDistortionFilter>(go);
        distortion.distortionLevel = 0.15f;

        // Eco de sala pequena (a cabine)
        var reverb = Undo.AddComponent<AudioReverbFilter>(go);
        reverb.reverbPreset = AudioReverbPreset.Room;

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;
        Debug.Log($"AddElevatorMusic: música com efeito posta no '{go.name}'. Guarda a cena (Ctrl+S).", go);
    }
}
