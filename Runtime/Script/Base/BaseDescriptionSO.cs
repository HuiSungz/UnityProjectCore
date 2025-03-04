
using UnityEngine;

public abstract class BaseDescriptionSO : ScriptableObject
{
    [SerializeField] [TextArea] 
    private string _description = "Input your comment.";
}