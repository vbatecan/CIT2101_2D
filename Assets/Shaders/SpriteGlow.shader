Shader "CaseClosed/SpriteGlow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HDR] _GlowColor ("Glow Color", Color) = (1.0, 0.88, 0.35, 1.0)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 0.0
        _GlowSpread ("Glow Spread (Pixels)", Range(1.0, 35.0)) = 12.0
        _GlowSoftness ("Glow Softness", Range(0.1, 4.0)) = 1.4
        _InnerHighlight ("Inner Highlight", Range(0, 1)) = 0.25
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex GlowVertex
            #pragma fragment GlowFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _GlowColor;
                float _GlowIntensity;
                float _GlowSpread;
                float _GlowSoftness;
                float _InnerHighlight;
            CBUFFER_END

            Varyings GlowVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 GlowFragment(Varyings input) : SV_Target
            {
                half4 mainTex = input.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                if (_GlowIntensity <= 0.001)
                {
                    return mainTex;
                }

                float2 texelScale = _MainTex_TexelSize.xy * _GlowSpread;
                float totalAlpha = 0.0;
                const int SAMPLES = 8;
                const float ANGLE_STEP = 6.28318530718 / 8.0;

                for (int i = 0; i < SAMPLES; i++)
                {
                    float angle = float(i) * ANGLE_STEP;
                    float2 offset = float2(cos(angle), sin(angle)) * texelScale * 0.45;
                    float a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + offset).a;
                    totalAlpha += a * 0.6;
                }

                for (int j = 0; j < SAMPLES; j++)
                {
                    float angle = float(j) * ANGLE_STEP + 0.3927;
                    float2 offset = float2(cos(angle), sin(angle)) * texelScale * 0.95;
                    float a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + offset).a;
                    totalAlpha += a * 0.4;
                }

                float avgAlpha = totalAlpha / float(SAMPLES);
                float glowWeight = saturate(pow(avgAlpha, 1.0 / max(0.01, _GlowSoftness)) * _GlowIntensity);

                // Outer glow aura
                float outerGlowAlpha = saturate(glowWeight * (1.0 - mainTex.a));
                half3 glowRgb = _GlowColor.rgb * _GlowIntensity;

                // Inner highlight on the item itself
                half3 spriteRgb = mainTex.rgb + (_GlowColor.rgb * _InnerHighlight * _GlowIntensity);

                // Blend main sprite with outer glow
                half3 finalRgb = (spriteRgb * mainTex.a) + (glowRgb * outerGlowAlpha);
                half finalAlpha = saturate(mainTex.a + outerGlowAlpha);

                return half4(finalRgb, finalAlpha);
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _RendererColor;
            fixed4 _Flip;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowSpread;
            float _GlowSoftness;
            float _InnerHighlight;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _AlphaTex;
            float _EnableExternalAlpha;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);
                #if ETC1_EXTERNAL_ALPHA
                if (_EnableExternalAlpha > 0.0)
                    color.a = tex2D(_AlphaTex, uv).a;
                #endif
                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 mainColor = SampleSpriteTexture(IN.texcoord) * IN.color;

                if (_GlowIntensity <= 0.001)
                {
                    return mainColor;
                }

                float2 texelScale = _MainTex_TexelSize.xy * _GlowSpread;
                float totalAlpha = 0.0;
                const int SAMPLES = 8;
                const float ANGLE_STEP = 6.28318530718 / 8.0;

                for (int i = 0; i < SAMPLES; i++)
                {
                    float angle = float(i) * ANGLE_STEP;
                    float2 offset = float2(cos(angle), sin(angle)) * texelScale * 0.45;
                    float a = SampleSpriteTexture(IN.texcoord + offset).a;
                    totalAlpha += a * 0.6;
                }

                for (int j = 0; j < SAMPLES; j++)
                {
                    float angle = float(j) * ANGLE_STEP + 0.3927;
                    float2 offset = float2(cos(angle), sin(angle)) * texelScale * 0.95;
                    float a = SampleSpriteTexture(IN.texcoord + offset).a;
                    totalAlpha += a * 0.4;
                }

                float avgAlpha = totalAlpha / float(SAMPLES);
                float glowWeight = saturate(pow(avgAlpha, 1.0 / max(0.01, _GlowSoftness)) * _GlowIntensity);

                float outerGlowAlpha = saturate(glowWeight * (1.0 - mainColor.a));
                fixed3 glowRgb = _GlowColor.rgb * _GlowIntensity;

                fixed3 spriteRgb = mainColor.rgb + (_GlowColor.rgb * _InnerHighlight * _GlowIntensity);

                fixed3 finalRgb = (spriteRgb * mainColor.a) + (glowRgb * outerGlowAlpha);
                fixed finalAlpha = saturate(mainColor.a + outerGlowAlpha);

                return fixed4(finalRgb, finalAlpha);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
