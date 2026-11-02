// ICameraView.cs
using UnityEngine;

public interface ICameraView
{
    Quaternion PlanarRotation { get; }   // só a rotação horizontal (yaw) da câmara
    bool AlignsCharacterToView { get; }  // true em 1ª pessoa
}
