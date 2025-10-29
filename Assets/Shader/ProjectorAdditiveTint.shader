Shader "Projector/AdditiveTint"
{
    Properties
    {
        _Color       ("Tint Color", Color) = (1,1,1,1)
        _Attenuation ("Falloff", Range(0,1)) = 1
        _ShadowTex   ("Cookie", 2D) = "gray" {}
    }

    SubShader
    {
        // Projectors usually render transparent and don’t write depth
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        Pass
        {
            // Projector-friendly state
            ZWrite Off
            ZTest LEqual
            Cull Off
            // Additive blend (color adds; alpha drives intensity)
            Blend SrcAlpha One
            ColorMask RGB

            // slight offset to reduce z-fighting if needed
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _ShadowTex;
            float4    _ShadowTex_ST;
            fixed4    _Color;
            float     _Attenuation;

            // Unity sets this for Projector components
            float4x4 unity_Projector;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uvProj : TEXCOORD0; // projected UV (xy/w)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uvProj = mul(unity_Projector, v.vertex); // xyz/w
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // projective divide
                float2 uv = i.uvProj.xy / max(i.uvProj.w, 1e-6);

                // sample cookie; typically uses A as mask (but RGB works too)
                fixed4 cookie = tex2D(_ShadowTex, uv);

                // simple z-based falloff: fade with |proj z| against _Attenuation
                // (matches your decompiled code’s intention)
                float falloff = saturate(1.0 + (_Attenuation - abs(i.uvProj.z)));

                fixed4 col = _Color * cookie.a * falloff; // alpha of cookie drives intensity
                // additive pass returns RGB (alpha ignored by One blending)
                return col;
            }
            ENDCG
        }
    }

    // If this fails to compile on very old hardware, you can provide a basic fallback
    Fallback Off
}
