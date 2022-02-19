Shader "A2 Games/Advance Unlit"
{
    Properties
    {
        //Main texturing
        _MainTex ("Texture", 2D) = "white" {} //main texture to be used
        _TintColor ("Tint Color", Color) = (1,1,1,1) //tint color that gets blends over the main texture
        
        //Main texture secondary settings
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling Mode", Float) = 2 //Culling modes switch
        [HideInInspector] _AlphaCutoff ("Alpha Cutoff", Range(0,1.01)) = 0.5 //Alpha cutoff value

        //Specularity
        _SpecularTex ("Specular", 2D) = "white" {} //Texture for specularity - Emission map
        _SpecularColor ("Specular Color", Color) = (0,0,0,0) //Specular Color tint over specular texture

        //Use to switch between different rendering modes or blend modes
        [HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1 //Source blend value
        [HideInInspector] _DstBlend ("_DstBlend", Float) = 0 //Destination blend value
        [HideInInspector] _ZWrite ("_ZWrite", Float) = 1 //Z-buffer write value
    }
    SubShader
    {
        Tags {"IgnoreProjector"="True"}
        LOD 100

        Blend [_SrcBlend] [_DstBlend] //Switch blend modes according to values
        ZWrite [_ZWrite] //Switch this on or off according to value
        Cull [_Cull] //Switching between culling modes 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //For using render mode features mentioned in customGUI
            #pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT 

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //Variables intialization
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _TintColor;
            float _AlphaCutoff;

            sampler2D _SpecularTex;
            float4 _SpecularTex_ST;
            fixed4 _SpecularColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); //Transform textures

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _TintColor; //Applying texture and blending it with the tint color
                half3 emission = tex2D(_SpecularTex, i.uv).rgb * _SpecularColor.rgb; //Applying specularity and blending it with the specular color
                col.rgb += emission; //blending it with the original texture

                #if defined(_RENDERING_CUTOUT) //Only clip if rendering mode is cutout
                    clip(col.a - _AlphaCutoff); //Clipping
                #endif

                return col;
            }
            ENDCG
        }
    }
    CustomEditor "AdvanceUnlitCustomGUI" //Custom GUI Script class
}
