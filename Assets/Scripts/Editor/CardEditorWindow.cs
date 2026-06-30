using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ShootingGame.EditorTools
{
    /// <summary>
    /// 데이터 카드(ScriptableObject) 저작용 공용 에디터 윈도우.
    /// 좌측: 카드 목록 + 생성/삭제, 우측: 선택 카드의 인스펙터 편집.
    /// 기획자가 코드 없이 콘텐츠를 찍어내는 파이프라인. (CLAUDE.md §3.4)
    /// </summary>
    public abstract class CardEditorWindow<T> : EditorWindow where T : ScriptableObject
    {
        /// <summary>새 카드가 생성될 폴더 (Assets/...).</summary>
        protected abstract string AssetFolder { get; }
        /// <summary>새 카드 기본 이름 접두사 (예: "W", "E", "B", "S").</summary>
        protected abstract string NamePrefix { get; }
        /// <summary>좌측 패널 상단 설명.</summary>
        protected virtual string Description => "";

        readonly List<T> cards = new List<T>();
        T selected;
        Editor inspector;
        string newCardName = "";
        Vector2 listScroll, inspectorScroll;

        protected virtual void OnEnable() => Refresh();

        void OnDisable()
        {
            if (inspector != null) DestroyImmediate(inspector);
        }

        void Refresh()
        {
            cards.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                var card = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (card != null) cards.Add(card);
            }
            cards.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawList();
            DrawInspector();
            EditorGUILayout.EndHorizontal();
        }

        void DrawList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(240));
            EditorGUILayout.LabelField(titleContent.text, EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(Description))
                EditorGUILayout.LabelField(Description, EditorStyles.miniLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            newCardName = EditorGUILayout.TextField(newCardName);
            if (GUILayout.Button("＋ 생성", GUILayout.Width(70))) CreateCard();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("새로고침")) Refresh();
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"카드 {cards.Count}개", EditorStyles.miniLabel);

            listScroll = EditorGUILayout.BeginScrollView(listScroll);
            for (int i = 0; i < cards.Count; i++)
            {
                var c = cards[i];
                if (c == null) continue;
                EditorGUILayout.BeginHorizontal();
                var style = c == selected ? EditorStyles.boldLabel : EditorStyles.label;
                if (GUILayout.Button(c.name, style))
                {
                    selected = c;
                    Selection.activeObject = c;
                }
                if (GUILayout.Button("⌫", GUILayout.Width(26))) { DeleteCard(c); break; }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawInspector()
        {
            EditorGUILayout.BeginVertical();
            if (selected == null)
            {
                EditorGUILayout.HelpBox("왼쪽에서 카드를 선택하거나 ＋생성 하세요.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField(selected.name, EditorStyles.largeLabel);
                EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(selected), EditorStyles.miniLabel);
                EditorGUILayout.Space(4);

                if (inspector == null || inspector.target != selected)
                {
                    if (inspector != null) DestroyImmediate(inspector);
                    inspector = Editor.CreateEditor(selected);
                }
                inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);
                DrawHeaderExtras(selected);
                inspector.OnInspectorGUI();
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>인스펙터 위에 표시할 추가 UI(미리보기 등). 필요 시 오버라이드.</summary>
        protected virtual void DrawHeaderExtras(T card) { }

        void CreateCard()
        {
            if (!System.IO.Directory.Exists(AssetFolder))
                System.IO.Directory.CreateDirectory(AssetFolder);

            string baseName = string.IsNullOrEmpty(newCardName) ? NamePrefix + "_New" : newCardName;
            string path = AssetDatabase.GenerateUniqueAssetPath(AssetFolder + "/" + baseName + ".asset");
            var card = CreateInstance<T>();
            AssetDatabase.CreateAsset(card, path);
            AssetDatabase.SaveAssets();

            newCardName = "";
            Refresh();
            selected = card;
            Selection.activeObject = card;
        }

        void DeleteCard(T card)
        {
            if (!EditorUtility.DisplayDialog("카드 삭제", card.name + " 을(를) 삭제할까요?", "삭제", "취소")) return;
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(card));
            if (selected == card) selected = null;
            Refresh();
        }
    }
}
