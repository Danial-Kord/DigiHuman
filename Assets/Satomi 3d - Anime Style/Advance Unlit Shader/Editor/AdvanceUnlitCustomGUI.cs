
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class AdvanceUnlitCustomGUI : ShaderGUI
{
    enum RenderMode //All switchable render modes
    {
        Opaque, Cutout, Fade, Transparent
        //Opaque = opacity 100%
        //Cutout = Cutout only opaque parts and ignore the transparent parts
        //Fade = Partial transparency or translucent or semi-transparent objects
        //Trasnparent = Complete transparency 
    }

    struct RenderSettings //Render settings for all render modes
    {
        public RenderQueue queue;
        public string renderType;
        public BlendMode srcBlend, dstBlend;
        public bool zWrite;

        public static RenderSettings[] modes = {
            new RenderSettings() {
                //Opaque render mode
                queue = RenderQueue.Geometry,
                renderType = "",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.Zero,
                zWrite = true
            },
            new RenderSettings() {
                //Cutout render mode
                queue = RenderQueue.AlphaTest,
                renderType = "TransparentCutout",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.Zero,
                zWrite = true
            },
            new RenderSettings() {
                //Fade render mode
                queue = RenderQueue.Transparent,
                renderType = "Transparent",
                srcBlend = BlendMode.SrcAlpha,
                dstBlend = BlendMode.OneMinusSrcAlpha,
                zWrite = false
            },
            new RenderSettings() {
                //Transparent render mode
                queue = RenderQueue.Transparent,
                renderType = "Transparent",
                srcBlend = BlendMode.One,
                dstBlend = BlendMode.OneMinusSrcAlpha,
                zWrite = false
            }
        };
    }

    static GUIContent staticLabel = new GUIContent();
    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;
    bool showAlphaCutoff; //Determines whether to show alpha cutoff slider

    //Main GUI function
    public override void OnGUI(
        MaterialEditor editor, MaterialProperty[] properties
    )
    {
        this.target = editor.target as Material;
        this.editor = editor;
        this.properties = properties;

        RunRenderModes(); //First put render mode option in the inspector

        if (showAlphaCutoff)
        {
            SetAlphaCutoff(); //Set alpha cutoff if render mode is set to cutoff
        }

        base.OnGUI(editor, properties); //Render the default GUI in the end
    }

    //Switch render modes according to the user's choice
    void RunRenderModes()
    {
        RenderMode mode = RenderMode.Opaque;
        showAlphaCutoff = false;
        if (IsKeywordEnabled("_RENDERING_CUTOUT"))
        {
            mode = RenderMode.Cutout;
            showAlphaCutoff = true;
        }
        else if (IsKeywordEnabled("_RENDERING_FADE"))
        {
            mode = RenderMode.Fade;
        }
        else if (IsKeywordEnabled("_RENDERING_TRANSPARENT"))
        {
            mode = RenderMode.Transparent;
        }

        EditorGUI.BeginChangeCheck();
        mode = (RenderMode)EditorGUILayout.EnumPopup(
            MakeLabel("Render Mode"), mode
        );
        if (EditorGUI.EndChangeCheck())
        {
            RecordAction("Render Mode");
            SetKeyword("_RENDERING_CUTOUT", mode == RenderMode.Cutout);
            SetKeyword("_RENDERING_FADE", mode == RenderMode.Fade);
            SetKeyword(
                "_RENDERING_TRANSPARENT", mode == RenderMode.Transparent
            );

            RenderSettings settings = RenderSettings.modes[(int)mode];
            foreach (Material m in editor.targets)
            {
                m.renderQueue = (int)settings.queue;
                m.SetOverrideTag("RenderType", settings.renderType);
                m.SetInt("_SrcBlend", (int)settings.srcBlend);
                m.SetInt("_DstBlend", (int)settings.dstBlend);
                m.SetInt("_ZWrite", settings.zWrite ? 1 : 0);
            }
        }
    }

    //Manipulates the alpha cutoff value accoring to slider value
    void SetAlphaCutoff()
    {
        MaterialProperty slider = FindProperty("_AlphaCutoff");
        EditorGUI.indentLevel += 2;
        editor.ShaderProperty(slider, MakeLabel(slider));
        EditorGUI.indentLevel -= 2;
    }

    //Finds material property name
    MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, properties);
    }

    static GUIContent MakeLabel(string text, string tooltip = null)
    {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    static GUIContent MakeLabel(
        MaterialProperty property, string tooltip = null
    )
    {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    void SetKeyword(string keyword, bool state)
    {
        if (state)
        {
            foreach (Material m in editor.targets)
            {
                m.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material m in editor.targets)
            {
                m.DisableKeyword(keyword);
            }
        }
    }

    bool IsKeywordEnabled(string keyword)
    {
        return target.IsKeywordEnabled(keyword);
    }

    void RecordAction(string label)
    {
        editor.RegisterPropertyChangeUndo(label);
    }
}
