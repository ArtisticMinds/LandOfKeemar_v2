using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteCollection", menuName = "Collections/Sprite Collection", order = 100)]
public class SpriteCollection : ScriptableObject
{
    public List<Sprite> sprites = new List<Sprite>();
}