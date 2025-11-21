Shader "Unlit/FeatheredDottedPlane"
{
    Properties
    {
        _PlaneColor("Background Color", Color) = (1,1,1,0)
        _DotColor("Dot Color", Color) = (1,1,1,1)
        _DotSpacing("Dot Spacing", Float) = 20.0
        _DotSize("Dot Size", Float) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float4 _PlaneColor;
            float4 _DotColor;
            float _ShortestUVMapping;
            float _DotSpacing;
            float _DotSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _DotSpacing;
                o.uv2 = v.uv2;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 grid = frac(i.uv);
                float2 dist = abs(grid - 0.5);
                float d = length(dist);

                float dotMask = 1.0 - smoothstep(_DotSize, _DotSize * 0.9, d);

                // Combine dot color and background color
                float alpha = dotMask;

                fixed3 color = lerp(_PlaneColor.rgb, _DotColor.rgb, alpha);
                float finalAlpha = lerp(_PlaneColor.a, _DotColor.a, alpha);

                // Feathering near the edges of the plane
                finalAlpha *= 1 - smoothstep(1, _ShortestUVMapping, i.uv2.x);

                return float4(color, finalAlpha);
            }
            ENDCG
        }
    }
}
