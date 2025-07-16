using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// way to make ScriptableObjects easier to create in Unity
//ScriptableObjects are a special type of object in Unity used to store data.
[CreateAssetMenu (fileName="World", menuName ="World")]

//Used for storing data that is independent of scenes and GameObjects.
public class World : ScriptableObject
{
    public Level[] levels;
}
