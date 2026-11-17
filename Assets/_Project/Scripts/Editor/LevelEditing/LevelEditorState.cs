using UnityEngine;

namespace ColorBlockJam.Editor.LevelEditing
{
    public sealed class LevelEditorState : ScriptableObject
    {
        public LevelDraft Draft = new();
    }
}
