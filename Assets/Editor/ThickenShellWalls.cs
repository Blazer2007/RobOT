using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

// Ferramenta TEMPORÁRIA de editor: dá grossura às paredes de fora do Level0.
// Menu: RobOT > Engrossar paredes do Level0 (com a cena Level0 aberta)
//
// A concha (Level0_Shell) era só uma "pele" virada para dentro, sem espessura.
// Isto junta-lhe, na MESMA mesh ProBuilder, uma caixa exterior virada para fora, à volta de tudo:
// o espaço entre as duas fica "maciço". O interior (onde se joga) não muda nada.
// Depois de correr, podes apagar este script.
public static class ThickenShellWalls
{
    private const string ShellName = "Level0_Shell";
    private const float Thickness = 1f; // grossura das paredes de fora (m)

    [MenuItem("RobOT/Engrossar paredes do Level0")]
    public static void Thicken()
    {
        GameObject go = GameObject.Find(ShellName);
        ProBuilderMesh pb = go != null ? go.GetComponent<ProBuilderMesh>() : null;
        if (pb == null)
        {
            EditorUtility.DisplayDialog("Engrossar paredes", $"Não encontrei o '{ShellName}' na cena aberta. Abre a Level0.", "OK");
            return;
        }

        // Limites do interior, no espaço local da concha
        var positions = new List<Vector3>(pb.positions);
        Bounds inner = new Bounds(positions[0], Vector3.zero);
        foreach (var p in positions) inner.Encapsulate(p);

        // Se já tem faces viradas para fora (corrido antes), não faz outra vez
        if (HasOuterBox(pb, inner))
        {
            EditorUtility.DisplayDialog("Engrossar paredes", "As paredes já foram engrossadas.", "OK");
            return;
        }

        Undo.RecordObject(pb, "Engrossar paredes do Level0");

        Vector3 min = inner.min - Vector3.one * Thickness;
        Vector3 max = inner.max + Vector3.one * Thickness;
        Vector3 size = max - min;
        var faces = new List<Face>(pb.faces);

        // Caixa exterior: 6 faces viradas para FORA
        AddQuad(positions, faces, new Vector3(min.x, min.y, min.z), new Vector3(0, 0, size.z), new Vector3(0, size.y, 0), Vector3.left);
        AddQuad(positions, faces, new Vector3(max.x, min.y, min.z), new Vector3(0, 0, size.z), new Vector3(0, size.y, 0), Vector3.right);
        AddQuad(positions, faces, new Vector3(min.x, min.y, min.z), new Vector3(size.x, 0, 0), new Vector3(0, size.y, 0), Vector3.back);
        AddQuad(positions, faces, new Vector3(min.x, min.y, max.z), new Vector3(size.x, 0, 0), new Vector3(0, size.y, 0), Vector3.forward);
        AddQuad(positions, faces, new Vector3(min.x, min.y, min.z), new Vector3(size.x, 0, 0), new Vector3(0, 0, size.z), Vector3.down);
        AddQuad(positions, faces, new Vector3(min.x, max.y, min.z), new Vector3(size.x, 0, 0), new Vector3(0, 0, size.z), Vector3.up);

        pb.RebuildWithPositionsAndFaces(positions, faces);
        pb.SetMaterial(pb.faces, BuiltinMaterials.defaultMaterial);
        pb.ToMesh();
        pb.Refresh();

        // O MeshCollider tem de apanhar a mesh nova
        var meshCollider = go.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
        }

        EditorUtility.SetDirty(pb);
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log($"ThickenShellWalls: paredes de fora com {Thickness} m de grossura. Guarda a cena (Ctrl+S).", go);
    }

    // Quadrado com cantos o, o+u, o+v, o+u+v, virado para "normal"
    private static void AddQuad(List<Vector3> positions, List<Face> faces, Vector3 o, Vector3 u, Vector3 v, Vector3 normal)
    {
        if (Vector3.Dot(Vector3.Cross(u, v), normal) < 0f) (u, v) = (v, u);
        int i = positions.Count;
        positions.Add(o);
        positions.Add(o + u);
        positions.Add(o + v);
        positions.Add(o + u + v);
        faces.Add(new Face(new[] { i, i + 1, i + 2, i + 1, i + 3, i + 2 }));
    }

    // Há alguma face no limite exterior virada para fora? (então já foi engrossada)
    private static bool HasOuterBox(ProBuilderMesh pb, Bounds bounds)
    {
        foreach (var face in pb.faces)
        {
            Vector3 n = Math.Normal(pb, face);
            Vector3 p = pb.positions[face.indexes[0]];
            if (n.x < -0.9f && Mathf.Approximately(p.x, bounds.min.x)) return true;
        }
        return false;
    }
}
