// Flat, unlit colour drawn over everything (ZTest Always), so a held block's outline stays visible even
// where another object covers the block. The band traces the block's on-screen silhouette: the mesh lies
// at the block's bottom edge, and every vertex whose side faces away from the camera is lifted to the rim
// of the top face (uv0.x holds that lift, the normal is the side's outward direction). A side the camera
// sees is outlined along its bottom; a hidden side along the top rim, wherever the block is on screen.
Shader "ColorBlockJam/Selection Outline"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.96, 0.72, 1)
        _FacingBlend ("Facing Blend", Range(0.01, 1)) = 0.15
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+100" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "SelectionOutline"
            Tags { "LightMode" = "UniversalForward" }
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _FacingBlend;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 lift : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 baseWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float facing = dot(normalWS, normalize(_WorldSpaceCameraPos - baseWS));
                // 1 where the camera sees the side (stay at the bottom edge), 0 where it cannot (go up).
                float seen = smoothstep(-_FacingBlend, _FacingBlend, facing);
                float3 positionOS = input.positionOS.xyz + float3(0, input.lift.x * (1 - seen), 0);
                output.positionCS = TransformObjectToHClip(positionOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
