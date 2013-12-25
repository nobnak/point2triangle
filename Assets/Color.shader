Shader "Custom/Color" {
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
				float4 color : TEXCOORD0;
			};
			
			vsout vert(appdata_full i) {
				vsout o;
				o.vertex = mul(UNITY_MATRIX_MVP, i.vertex);
				o.color = i.color;
				return o;
			}
			
			fixed4 frag(vsout i) : COLOR {
				return i.color;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
