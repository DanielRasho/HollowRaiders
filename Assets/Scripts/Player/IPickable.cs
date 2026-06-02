using UnityEngine;

public interface IPickable
{
    bool canPick();
    bool canAutoPick();
    void Pick();
}
