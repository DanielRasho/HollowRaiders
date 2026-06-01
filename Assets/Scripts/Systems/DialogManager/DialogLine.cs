using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class DialogLine
{
    public CharacterID characterToShow;
    public CharacterExpression characterExpression;
    public CharacterPosition characterPosition;
    public string LineText;
    
}