using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float _amp;
    public float _freq;
    Quaternion _currentPos;
    void Start()
    {
        _currentPos = transform.rotation;
    }
    // Começa a girar o objeto de traz para a frente a partir da sua posição e rotação atual
    void Update()
    {
        transform.rotation = _currentPos * Quaternion.Euler(0, 0, Mathf.Sin(Time.time * _freq) * _amp);

    }
}
