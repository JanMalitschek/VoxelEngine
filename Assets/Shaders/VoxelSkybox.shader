Shader "VoxelEngine/VoxelSkybox"
{
    Properties
    {
        _BottomColor("Bottom Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_HorizonColor("Horizon Color", Color) = (0.5, 0.5, 0.5, 1.0)
		_SkyColor("Sky Color", Color) = (1.0, 1.0, 1.0, 1.0)

		_CloudTex("Cloud Texture", 2D) = "black" {}
		_CloudScale("Cloud Scale", Float) = 1.0
		_CloudFalloff("Cloud Falloff", Range(0.01, 1.0)) = 1.0
		_CloudIntensity("Cloud Intensity", Range(0.0, 1.0)) = 1.0
		_CloudSpeed("Cloud Speed", Vector) = (0.0, 0.0, 0.0, 0.0)

		_SunColor("Sun Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_SunGlowIntensity("Sun Glow Intensity", Range(0.0, 1.0)) = 0.5
		_SunSize("Sun Size", Range(0.01, 0.1)) = 0.01
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
			#include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
				float4 position : SV_POSITION;
				float3 uv : TEXCOORD0;
            };

			fixed4 _BottomColor;
			fixed4 _HorizonColor;
			fixed4 _SkyColor;

			sampler2D _CloudTex;
			float _CloudScale;
			float _CloudFalloff;
			float _CloudIntensity;
			float4 _CloudSpeed;

			float4 _SunColor;
			float _SunGlowIntensity;
			float _SunSize;

            v2f vert (appdata v)
            {
                v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				//Sky Gradient
				float skyFactor = dot(i.uv, float3(0.0, 1.0, 0.0));
				float bottomFactor = dot(i.uv, float3(0.0, -1.0, 0.0));
				fixed4 col = fixed4(0.0, 0.0, 0.0, 1.0);
				if(skyFactor >= 0.0)
					col += lerp(_HorizonColor, _SkyColor, skyFactor);
				if(bottomFactor > 0.0)
					col += lerp(_HorizonColor, _BottomColor, bottomFactor);

				//Clouds
				if (_CloudIntensity > 0.0) {
					float k = 1.0 / i.uv.y;
					if (k > 0.0) {
						float3 ceiling = i.uv * k;
						col += tex2D(_CloudTex, ceiling.xz * _CloudScale + float2(_CloudSpeed.x, _CloudSpeed.z) * _Time.y)
							* saturate(1.0 - (length(ceiling) - 1.0) * _CloudFalloff)
							* _CloudIntensity;
					}
				}

				//Sun
				float sunFactor = saturate(1.0 - distance(i.uv, _WorldSpaceLightPos0.xyz));
				if (sunFactor > 0.0) {
					col += _LightColor0 * _SunGlowIntensity * sunFactor;
					if (sunFactor > 1.0 - _SunSize)
						col += _LightColor0 * ((sunFactor - 1.0 + _SunSize) / _SunSize);
				}
                return col;
            }
            ENDCG
        }
    }
}
