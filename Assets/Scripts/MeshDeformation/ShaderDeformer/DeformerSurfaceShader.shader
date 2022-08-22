Shader "Custom/DeformerSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [PowerSlider(5.0)] _Speed ("Speed", Range (0.01, 100)) = 2
        [PowerSlider(5.0)] _Amplitude ("Amplitude", Range (0.01, 5)) = 0.25
        [PowerSlider(5.0)] _TangentMultiplier ("TangentMultiplier", Range (0.001, 2)) = 0.01
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Speed;
        float _Amplitude;
        float _TangentMultiplier;

        float getOffset( float3 position)
        {
            const float distance = 6.0 - length(position - float4(0, 0, 0, 0));
            return sin(_Time * _Speed + distance) * _Amplitude;
        }

        void vert(inout appdata_full data)
        {
            data.vertex.y = getOffset(data.vertex);
        
            float3 posPlusTangent = data.vertex + data.tangent * _TangentMultiplier;
            posPlusTangent.y = getOffset(posPlusTangent);
            float3 bitangent = cross(data.normal, data.tangent);
        
            float3 posPlusBitangent = data.vertex + bitangent * _TangentMultiplier;
            posPlusBitangent.y = getOffset(posPlusBitangent);
        
            float3 modifiedTangent = posPlusTangent - data.vertex;
            float3 modifiedBitangent = posPlusBitangent - data.vertex;
            float3 modifiedNormal = cross(modifiedTangent, modifiedBitangent);
            data.normal = normalize(modifiedNormal);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}