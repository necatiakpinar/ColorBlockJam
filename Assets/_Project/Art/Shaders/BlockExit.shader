// The look of a block while it leaves through its door: the same Blinn-Phong lighting as URP Simple Lit
// (which every resting block uses, with no texture and no specular), plus a world-space clip plane at the
// board's edge, so the block is cut away as it slides into the door instead of shrinking. Only an exiting
// block uses it (its own material copy carries the plane), so the resting blocks keep their batching.
// Back faces are drawn too, lit as if they faced out: where the plane opens the hollow mesh, the inside
// reads as a solid cut face instead of a hole showing the floor behind (seen on doors facing the camera).
Shader "ColorBlockJam/Block Exit"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _ClipPlane ("Clip Plane (world: normal, distance)", Vector) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                float4 _ClipPlane;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                half3 sh : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.sh = SampleSHVertex(output.normalWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                // Keep what is still on the board side of the door; the rest has gone in.
                clip(dot(_ClipPlane.xyz, input.positionWS) + _ClipPlane.w);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalize(input.normalWS) * IS_FRONT_VFACE(face, 1, -1);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSHPixel(input.sh, inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = _BaseColor.rgb;
                surface.alpha = 1;
                surface.smoothness = _Smoothness;
                surface.occlusion = 1;

                half4 color = UniversalFragmentBlinnPhong(inputData, surface);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }
}
