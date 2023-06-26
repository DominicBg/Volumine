Shader "Volumine/VolumetricTextureVisualiser"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Cull Off ZWrite Off ZTest Always

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler3D _VolumeTexture;
			float alpha;
			float3 camPos;
			float4 camRot;

			float3 qmul(float4 q, float3 v)
			{
				float3 a = 2 * cross(q.xyz, v);
				return v + q.w * a + cross(q.xyz, a);
			}

			bool AABBCollision(float3 ro, float3 invRd, float3 boxMin, float3 boxMax, out float distance)
			{
				float3 t1 = (boxMin - ro) * invRd;
				float3 t2 = (boxMax - ro) * invRd;

				float3 mins = min(t1, t2);
				float3 maxs = max(t1, t2);

				float tmin = max(mins.x, max(mins.y, mins.z));
				float tmax = min(maxs.x, min(maxs.y, maxs.z));

				distance = tmin;
				return tmax > tmin && tmax > 0;
			}

			void PerspectiveCam(float3 camPos, float4 camRot, float2 uv, float fov, float aspectRatio, out float3 ro, out float3 rd)
			{
				const float PI = 3.1416;
				uv.x = uv.x * tan(fov / 2 * PI / 180) * aspectRatio;
				uv.y = uv.y * tan(fov / 2 * PI / 180);

				float3 forward = normalize(float3(uv.x, uv.y, 1.0));
				ro = camPos;
				rd = qmul(camRot, forward);
			}

			float3 Remap(float3 fromMin, float3 fromMax, float3 toMin, float3 toMax, float3 value)
			{
				float3 t = (value - fromMin) / (fromMax - fromMin);
				return lerp(toMin, toMax, saturate(t));
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 ro, rd;
				float2 uv = i.uv * 2 - 1;
				PerspectiveCam(camPos, camRot, uv, 30, 1, ro, rd);

				float dist;
				float scale = 0.5;
				if (AABBCollision(ro, 1 / rd, -scale, +scale, dist))
				{
					float4 sum = 0;
					float3 pos = ro + rd * dist;
					float stepSize = 0.005;

					//cube max length = sqrt(x^2 + y^2 + z^3)
					int maxLength = sqrt(3);
					int maxStep = maxLength / stepSize;

					for (int i = 0; i < maxStep; i++)
					{
						pos += rd * stepSize;

						bool posInBox = all(pos <= scale) && all(pos >= -scale);
						if (!posInBox)
							continue;

						float3 uv3 = Remap(-0.5, 0.5, 0, 1, pos);

						float4 col = tex3D(_VolumeTexture, uv3);
						sum += col * stepSize * alpha;
					}

					return sum;
				}

				return 0;
			}
			ENDCG
		}
	}
}
