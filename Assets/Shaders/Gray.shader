Shader "Custom/MaskCutout_Gray"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MaskWorldBounds ("Mask World Bounds (xmin,ymin,xmax,ymax)", Vector) = ( -16, -9, 16, 9 )
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            sampler2D _MaskTex;
            float4 _MaskWorldBounds;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                float4 worldPos4 = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos4.xy;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 baseC = tex2D(_MainTex, i.uv) * _Color;

                float xmin = _MaskWorldBounds.x;
                float ymin = _MaskWorldBounds.y;
                float xmax = _MaskWorldBounds.z;
                float ymax = _MaskWorldBounds.w;

                float u = (i.worldPos.x - xmin) / max(0.0001, (xmax - xmin));
                float v = (i.worldPos.y - ymin) / max(0.0001, (ymax - ymin));

                if (u < 0.0 || u > 1.0 || v < 0.0 || v > 1.0)
                {
                    float g = dot(baseC.rgb, float3(0.299, 0.587, 0.114));
                    baseC.rgb = float3(g, g, g);
                    return baseC;
                }

                float4 m = tex2D(_MaskTex, float2(u, v));

                if (m.a > 0.01)
                {
                    return float4(baseC.rgb, 0.0);
                }
                
                float gray = dot(baseC.rgb, float3(0.299, 0.587, 0.114));
                gray *= 0.7;
                baseC.rgb = float3(gray, gray, gray);

                return baseC;
            }
            ENDCG
        }
    }
}
