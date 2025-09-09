Shader "UI/SaturationAndTint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Saturation ("Saturation", Range(0,1)) = 1
        _TintColor ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile __ ACCESSIBILITY_HIGH_CONTRAST
            #pragma multi_compile __ ACCESSIBILITY_COLORBLIND

            sampler2D _MainTex;
            fixed4 _Color;
            float _Saturation;
            fixed4 _TintColor;
            float _FlashReduction;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * _Color;

                float gray = dot(tex.rgb, float3(0.3, 0.59, 0.11));
                tex.rgb = lerp(float3(gray, gray, gray), tex.rgb, _Saturation);

                tex.rgb *= _TintColor.rgb;

#ifdef ACCESSIBILITY_HIGH_CONTRAST
                tex.rgb = saturate(tex.rgb * 2);
#endif

#ifdef ACCESSIBILITY_COLORBLIND
                float cb = dot(tex.rgb, float3(0.2126, 0.7152, 0.0722));
                tex.rgb = float3(cb, cb, cb);
#endif

                tex.rgb *= _FlashReduction;

                return tex;
            }
            ENDCG
        }
    }
}
