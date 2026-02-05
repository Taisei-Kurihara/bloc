Shader "Custom/AttackInstanced"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            // インスタンスデータ構造体.
            struct InstanceData
            {
                float4 positionScale;  // xyz = position, w = scale
                float4 rotationColor;  // x = rotation, yzw = color RGB
            };

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                StructuredBuffer<InstanceData> _InstanceBuffer;
                int _InstanceOffset;
            #endif

            void setup()
            {
            }

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 worldPos = v.vertex.xyz;
                float4 color = _Color;

                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    InstanceData data = _InstanceBuffer[unity_InstanceID + _InstanceOffset];

                    float3 position = data.positionScale.xyz;
                    float scale = data.positionScale.w;
                    float rotation = data.rotationColor.x;
                    float3 rgb = data.rotationColor.yzw;

                    // 回転適用.
                    float cosR = cos(rotation);
                    float sinR = sin(rotation);
                    float2 rotated;
                    rotated.x = v.vertex.x * cosR - v.vertex.y * sinR;
                    rotated.y = v.vertex.x * sinR + v.vertex.y * cosR;

                    // スケールと位置適用.
                    worldPos.xy = rotated * scale + position.xy;
                    worldPos.z = position.z;

                    // カラー設定.
                    color = float4(rgb, 1.0);
                #endif

                o.vertex = UnityObjectToClipPos(float4(worldPos, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
