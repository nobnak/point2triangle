Shader "Custom/Normal" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			
			struct vsout {
				float4 vertex : POSITION;
				float3 normal : TEXCOORD0;
			};
			
			vsout vert(appdata_full i) {
				vsout o;
				o.vertex = mul(UNITY_MATRIX_MVP, i.vertex);
				o.normal = mul(UNITY_MATRIX_IT_MV, float4(i.normal, 0.0));
				return o;
			}
			
			fixed4 frag(vsout i) : COLOR {
				return fixed4(i.normal * 0.5 + 0.5, 1.0);
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
