using UnityEngine;

namespace ColorBlockJam.Editor.LevelEditing
{
    /// <summary>
    /// Holds the draft the level editor works on as an object Unity's undo can record, so Ctrl+Z and Ctrl+Y
    /// step back and forth through every edit. Never saved as an asset.
    /// </summary>
    public sealed class LevelEditorState : ScriptableObject
    {
        public LevelDraft Draft = new();
    }
}
