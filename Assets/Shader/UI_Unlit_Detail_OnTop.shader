Shader "Custom/UI/Unlit Detail (OnTop)"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color        ("Tint", Color) = (1,1,1,1)

        _DetailTex    ("Detail", 2D) = "gray" {}
        _DetailScale  ("Detail Strength", Range(0,2)) = 1

        // --- UI Masking (same layout as Unity's UI/Default) ---
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID", Float) = 0
        _StencilOp        ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask", Float) = 255
        _ColorMask        ("Color Mask", Float) = 15

        // Toggle alpha clip (for masked sprites)
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        // ZTest control: 4=LEqual (default), 8=Always (always on top)
        [Enum(UnityEngine.Rendering.CompareFunction)]
        _ZTestMode ("ZTest", Float) = 4
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [_ZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            sampler2D _DetailTex;
            float4 _DetailTex_ST;
            float  _DetailScale;

            float4 _ClipRect; // provided by Unity UI for RectMask2D

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float2 uvDetail : TEXCOORD1;
                fixed4 color    : COLOR;
                float4 worldPos : TEXCOORD2;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.uv       = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uvDetail = TRANSFORM_TEX(v.texcoord, _DetailTex);
                o.color    = v.color * _Color;
                o.worldPos = v.vertex; // used by UI clip
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainCol   = tex2D(_MainTex,   i.uv) * i.color;

                // Detail multiply (scaled strength)
                fixed3 detailRgb = tex2D(_DetailTex, i.uvDetail).rgb;
                mainCol.rgb *= lerp(1.0, detailRgb, saturate(_DetailScale));

                #ifdef UNITY_UI_CLIP_RECT
                    mainCol.a *= UnityGet2DClipping(i.worldPos, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(mainCol.a - 0.001);
                #endif

                return mainCol;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
