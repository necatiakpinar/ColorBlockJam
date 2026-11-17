using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Editor.LevelEditing
{
    public enum SkinButton
    {
        Neutral = 1000,
        Primary = 2000,
        Success = 3000,
        Danger = 4000,
        Accent = 5000,
    }

    public sealed class LevelEditorSkin
    {
        const int TextureSize = 32;
        const int Radius = 10;
        const float NormalShade = 0.88f;
        const float ActiveShade = 0.72f;

        readonly bool _dark = EditorGUIUtility.isProSkin;
        Texture2D _normal;
        Texture2D _hover;
        Texture2D _active;
        GUIStyle _button;
        GUIStyle _smallButton;
        GUIStyle _card;
        GUIStyle _cardTitle;
        GUIStyle _headerTitle;
        GUIStyle _headerSubtitle;
        GUIStyle _listRow;
        GUIStyle _chip;
        GUIStyle _muted;
        GUIStyle _help;

        public Color WindowBack => _dark ? new Color(0.13f, 0.14f, 0.18f) : new Color(0.86f, 0.88f, 0.93f);
        public Color HeaderBack => _dark ? new Color(0.20f, 0.23f, 0.36f) : new Color(0.27f, 0.38f, 0.72f);
        public Color CardBack => _dark ? new Color(0.19f, 0.20f, 0.25f) : new Color(0.97f, 0.97f, 0.99f);
        public Color Accent => new(0.36f, 0.56f, 1f);
        public Color Text => _dark ? new Color(0.92f, 0.93f, 0.96f) : new Color(0.13f, 0.15f, 0.22f);
        public Color MutedText => _dark ? new Color(0.62f, 0.65f, 0.74f) : new Color(0.40f, 0.43f, 0.52f);
        public Color Good => new(0.27f, 0.74f, 0.42f);
        public Color Bad => new(0.92f, 0.33f, 0.36f);
        public Color Warn => new(0.96f, 0.68f, 0.22f);

        public GUIStyle CardTitle => Ensure()._cardTitle;
        public GUIStyle HeaderTitle => Ensure()._headerTitle;
        public GUIStyle HeaderSubtitle => Ensure()._headerSubtitle;
        public GUIStyle Muted => Ensure()._muted;
        public GUIStyle HelpText => Ensure()._help;

        public Color ButtonColor(SkinButton kind) => kind switch
        {
            SkinButton.Primary => Accent,
            SkinButton.Success => Good,
            SkinButton.Danger => Bad,
            SkinButton.Accent => new Color(0.66f, 0.42f, 0.98f),
            _ => _dark ? new Color(0.36f, 0.38f, 0.47f) : new Color(0.55f, 0.59f, 0.70f),
        };

        public bool Button(GUIContent content, SkinButton kind, params GUILayoutOption[] options) =>
            Tinted(ButtonColor(kind), () => GUILayout.Button(content, Ensure()._button, options));

        public bool SmallButton(GUIContent content, SkinButton kind, params GUILayoutOption[] options) =>
            Tinted(ButtonColor(kind), () => GUILayout.Button(content, Ensure()._smallButton, options));

        public bool Toggle(bool on, GUIContent content, params GUILayoutOption[] options) =>
            Tinted(on ? Accent : ButtonColor(SkinButton.Neutral), () => GUILayout.Toggle(on, content, Ensure()._smallButton, options));

        public bool ListRow(bool selected, string label, Color badge)
        {
            Ensure();
            Rect row = GUILayoutUtility.GetRect(GUIContent.none, _listRow, GUILayout.Height(30), GUILayout.ExpandWidth(true));
            bool hover = row.Contains(Event.current.mousePosition);
            Color back = selected ? Accent : hover ? Shade(CardBack, _dark ? 1.35f : 0.93f) : CardBack;
            RoundedRect(row, back, 8f);
            RoundedRect(new Rect(row.x + 6, row.y + 8, 6, row.height - 16), badge, 3f);
            var labelRect = new Rect(row.x + 18, row.y, row.width - 22, row.height);
            _listRow.normal.textColor = selected ? Color.white : Text;
            _listRow.fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;
            GUI.Label(labelRect, label, _listRow);
            return GUI.Button(row, GUIContent.none, GUIStyle.none);
        }

        public void Chip(string text, Color color)
        {
            Ensure();
            var content = new GUIContent(text);
            Rect rect = GUILayoutUtility.GetRect(content, _chip, GUILayout.ExpandWidth(true));
            RoundedRect(rect, new Color(color.r, color.g, color.b, 0.22f), 8f);
            RoundedRect(new Rect(rect.x, rect.y, 4, rect.height), color, 2f);
            _chip.normal.textColor = Text;
            GUI.Label(rect, content, _chip);
        }

        public void BeginCard(string title, params GUILayoutOption[] options)
        {
            Tinted(CardBack, () => GUILayout.BeginVertical(Ensure()._card, options));
            if (title == null)
                return;
            Rect line = GUILayoutUtility.GetRect(1, 22, GUILayout.ExpandWidth(true));
            RoundedRect(new Rect(line.x, line.y + 4, 4, 14), Accent, 2f);
            GUI.Label(new Rect(line.x + 10, line.y, line.width - 10, line.height), title, _cardTitle);
        }

        public void EndCard()
        {
            GUILayout.EndVertical();
            GUILayout.Space(6);
        }

        public void RoundedRect(Rect rect, Color color, float radius)
        {
            if (Event.current.type == EventType.Repaint)
                GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, radius);
        }

        public void Frame(Rect rect, Color color, float width, float radius)
        {
            if (Event.current.type == EventType.Repaint)
                GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, width, radius);
        }

        static Color Shade(Color color, float by) => new(color.r * by, color.g * by, color.b * by, color.a);

        static T Tinted<T>(Color color, System.Func<T> draw)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = color;
            try
            {
                return draw();
            }
            finally
            {
                GUI.backgroundColor = old;
            }
        }

        static void Tinted(Color color, System.Action draw)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = color;
            try
            {
                draw();
            }
            finally
            {
                GUI.backgroundColor = old;
            }
        }

        LevelEditorSkin Ensure()
        {
            if (_normal != null && _button != null)
                return this;

            _normal = RoundedTexture(NormalShade);
            _hover = RoundedTexture(1f);
            _active = RoundedTexture(ActiveShade);
            var border = new RectOffset(Radius, Radius, Radius, Radius);

            _button = new GUIStyle(GUI.skin.button)
            {
                border = border,
                padding = new RectOffset(12, 12, 7, 7),
                margin = new RectOffset(3, 3, 3, 3),
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                fixedHeight = 30,
                alignment = TextAnchor.MiddleCenter,
            };
            SetStates(_button, Color.white);
            _smallButton = new GUIStyle(_button) { fixedHeight = 24, fontSize = 11, padding = new RectOffset(8, 8, 4, 4) };
            _card = new GUIStyle
            {
                border = border,
                padding = new RectOffset(10, 10, 8, 10),
                margin = new RectOffset(4, 4, 2, 2),
            };
            _card.normal.background = _hover;
            _cardTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
            _cardTitle.normal.textColor = Text;
            _headerTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
            _headerTitle.normal.textColor = Color.white;
            _headerSubtitle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 11 };
            _headerSubtitle.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
            _listRow = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
            _chip = new GUIStyle(EditorStyles.wordWrappedLabel) { padding = new RectOffset(12, 8, 6, 6), margin = new RectOffset(0, 0, 2, 2) };
            _muted = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            _muted.normal.textColor = MutedText;
            _help = new GUIStyle(EditorStyles.wordWrappedLabel) { richText = true, fontSize = 11 };
            _help.normal.textColor = Text;
            return this;
        }

        void SetStates(GUIStyle style, Color text)
        {
            style.normal.background = _normal;
            style.hover.background = _hover;
            style.active.background = _active;
            style.focused.background = _normal;
            style.onNormal.background = _hover;
            style.onHover.background = _hover;
            style.onActive.background = _active;
            style.onFocused.background = _hover;
            style.normal.textColor = text;
            style.hover.textColor = text;
            style.active.textColor = text;
            style.focused.textColor = text;
            style.onNormal.textColor = text;
            style.onHover.textColor = text;
            style.onActive.textColor = text;
            style.onFocused.textColor = text;
        }

        static Texture2D RoundedTexture(float shade)
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color[TextureSize * TextureSize];
            float max = TextureSize - 1;
            for (int y = 0; y < TextureSize; y++)
                for (int x = 0; x < TextureSize; x++)
                {
                    float dx = Mathf.Max(Radius - x, 0f, x - (max - Radius));
                    float dy = Mathf.Max(Radius - y, 0f, y - (max - Radius));
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(Radius - distance + 0.5f);
                    float top = Mathf.Lerp(0.9f, 1f, y / max);
                    pixels[y * TextureSize + x] = new Color(shade * top, shade * top, shade * top, alpha);
                }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
