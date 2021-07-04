Shader "VoxelEngine/Voxel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TopColor("Top Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _SideColorA("Side Color A", Color) = (0.9, 0.9, 0.9, 1.0)
        _SideColorB("Side Color B", Color) = (0.9, 0.9, 0.9, 1.0)
        _BottomColor("Bottom Color", Color) = (0.8, 0.8, 0.8, 1.0)
        _LightColor("Light Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _DarkColor("Dark Color", Color) = (0.2, 0.2, 0.2, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                fixed4 color : COLOR;
                float fog : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _TopColor;
            fixed4 _SideColorA, _SideColorB;
            fixed4 _BottomColor;
            fixed4 _LightColor;
            fixed4 _DarkColor;

            uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;

            v2f vert (appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = v.normal.xyz;
                o.color = v.color;
                
                float distance = length(mul(UNITY_MATRIX_MV, v.vertex));
				o.fog = max(min((unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart), 1.0), 0.0);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - 0.5);

                float NDotL = dot(i.normal, float3(0.0, 1.0, 0.0));
                fixed4 lighting = fixed4(1.0, 1.0, 1.0, 1.0);
                fixed4 sideColor = lerp(_SideColorA, _SideColorB, abs(dot(i.normal, float3(1.0, 0.0, 0.0))));
                if(NDotL >= 0)
                    lighting = lerp(sideColor, _TopColor, NDotL);
                else
                    lighting = lerp(_BottomColor, sideColor, NDotL + 1);
                col *= lighting * lerp(_DarkColor, _LightColor, i.color.r);
                col = lerp(unity_FogColor, col, i.fog);
                return col;
            }
            ENDCG
        }
    }
}
