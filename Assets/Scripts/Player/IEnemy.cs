using UnityEngine;

public interface IEnemy
{
    void Hurt();
    Transform transform { get; }
}
