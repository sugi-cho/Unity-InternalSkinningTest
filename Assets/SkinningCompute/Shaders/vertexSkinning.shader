Shader "Unlit/vertexSkinning"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"


			struct SVertInSkin
			{
				float weight0,weight1,weight2,weight3;
				int index0,index1,index2,index3;
			};
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
			};

			StructuredBuffer<SVertInSkin> _Skin;
			StructuredBuffer<float4x4> _Bones;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v, uint vIdx : SV_VertexID)
			{
				SVertInSkin si = _Skin[vIdx];

				float3 vP = v.vertex.xyz;
				float3 vPacc = float3(0, 0, 0);

				vPacc += si.weight0*mul(_Bones[si.index0], float4(vP, 1)).xyz;
				vPacc += si.weight1*mul(_Bones[si.index1], float4(vP, 1)).xyz;
				vPacc += si.weight2*mul(_Bones[si.index2], float4(vP, 1)).xyz;
				vPacc += si.weight3*mul(_Bones[si.index3], float4(vP, 1)).xyz;

				v2f o;
				o.vertex = UnityObjectToClipPos(vPacc);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = half4(si.index0, si.index1, si.index2, si.index3) / 43.0;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = i.color;
				return col;
			}
			ENDCG
		}
	}
}
