using UnityEditor;
using UnityEngine;
using ShootingGame.Weapon;
using ShootingGame.Enemy;
using ShootingGame.Boss;
using ShootingGame.Stage;

namespace ShootingGame.EditorTools
{
    /// <summary>무기 에디터 — WeaponData 카드 저작. (§3.2)</summary>
    public class WeaponEditorWindow : CardEditorWindow<WeaponData>
    {
        [MenuItem("ASTRA/① 무기 에디터")]
        static void Open() => GetWindow<WeaponEditorWindow>("무기 에디터");

        protected override string AssetFolder => "Assets/ScriptableObjects/Weapons";
        protected override string NamePrefix => "W";
        protected override string Description => "무기-출력 Lv.1~4 강화 곡선";

        protected override void DrawHeaderExtras(WeaponData card)
        {
            var rect = EditorGUILayout.GetControlRect(false, 16f, GUILayout.Width(60));
            EditorGUI.DrawRect(rect, card.bulletColor);
            EditorGUILayout.LabelField("관통: " + (card.isPiercing ? "O" : "X") +
                                       "  발사모드: " + card.fireMode, EditorStyles.miniLabel);
            EditorGUILayout.Space(4);
        }
    }

    /// <summary>적기 에디터 — EnemyData 카드 저작. (편대 FormationData는 합류 예정, §5.1)</summary>
    public class EnemyEditorWindow : CardEditorWindow<EnemyData>
    {
        [MenuItem("ASTRA/② 적기 에디터")]
        static void Open() => GetWindow<EnemyEditorWindow>("적기 에디터");

        protected override string AssetFolder => "Assets/ScriptableObjects/Enemies";
        protected override string NamePrefix => "E";
        protected override string Description => "이동 4종 + 발사 + 드롭";

        protected override void DrawHeaderExtras(EnemyData card)
        {
            EditorGUILayout.LabelField($"HP {card.hp} · 점수 {card.score} · 이동 {card.moveType}" +
                                       (card.canFire ? " · 발사O" : ""), EditorStyles.miniLabel);
            EditorGUILayout.Space(4);
        }
    }

    /// <summary>편대 에디터 — FormationData 카드(적 무리 구성) 저작. (§5.1)</summary>
    public class FormationEditorWindow : CardEditorWindow<FormationData>
    {
        [MenuItem("ASTRA/②-B 편대 에디터")]
        static void Open() => GetWindow<FormationEditorWindow>("편대 에디터");

        protected override string AssetFolder => "Assets/ScriptableObjects/Formations";
        protected override string NamePrefix => "F";
        protected override string Description => "적 무리 배열(라인/V/컬럼)";

        protected override void DrawHeaderExtras(FormationData card)
        {
            string enemy = card.enemyData != null ? card.enemyData.name : "(미지정)";
            EditorGUILayout.LabelField($"{card.pattern} · {card.count}기 · 간격 {card.spacing} · 적 {enemy}",
                                       EditorStyles.miniLabel);
            EditorGUILayout.Space(4);
        }
    }

    /// <summary>보스 에디터 — BossData 카드(파츠+페이즈+기믹) 저작. (§5.2)</summary>
    public class BossEditorWindow : CardEditorWindow<BossData>
    {
        [MenuItem("ASTRA/③ 보스 에디터")]
        static void Open() => GetWindow<BossEditorWindow>("보스 에디터");

        protected override string AssetFolder => "Assets/ScriptableObjects/Bosses";
        protected override string NamePrefix => "B";
        protected override string Description => "파츠 + 페이즈 FSM + 탄막";

        protected override void DrawHeaderExtras(BossData card)
        {
            int parts = card.parts != null ? card.parts.Length : 0;
            int phases = card.phases != null ? card.phases.Length : 0;
            EditorGUILayout.LabelField($"코어 HP {card.coreHp} · 파츠 {parts}개 · 페이즈 {phases}개",
                                       EditorStyles.miniLabel);
            EditorGUILayout.Space(4);
        }
    }

    /// <summary>레벨 에디터 — StageData 카드(배경 다중 스크롤 + 구간/스폰) 저작. (§6.1)</summary>
    public class StageEditorWindow : CardEditorWindow<StageData>
    {
        [MenuItem("ASTRA/④ 레벨 에디터")]
        static void Open() => GetWindow<StageEditorWindow>("레벨 에디터");

        protected override string AssetFolder => "Assets/ScriptableObjects/Stages";
        protected override string NamePrefix => "S";
        protected override string Description => "배경 다중 스크롤(시차)";

        protected override void DrawHeaderExtras(StageData card)
        {
            int layers = card.layers != null ? card.layers.Length : 0;
            EditorGUILayout.LabelField($"스크롤 {card.baseScrollSpeed} · 레이어 {layers}개", EditorStyles.miniLabel);
            EditorGUILayout.Space(4);
        }
    }
}
