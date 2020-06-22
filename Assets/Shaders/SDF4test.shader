Shader "ConformalDecals/SDF4test"
{
    Properties
    {
        _MainTex("_MainTex (RGB spec(A))", 2D) = "white" {}
        _Color1("Color 1", Color) = (0,0,0,0)
        _Color2("Color 2", Color) = (0,0,0,0)
        _Color3("Color 3", Color) = (0,0,0,0)
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _Smoothness ("SDF smoothness", Range(0,1)) = 0.15

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Cull Back
        ZWrite On
        
        Pass
        {
             Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float _Smoothness;
            float _Cutoff;

            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
                half3 worldNormal : TEXCOORD2;
            };

            v2f vert(float4 vertex : POSITION, float2 uv : TEXCOORD0) {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.uv = uv;
                o.worldPosition = mul(unity_ObjectToWorld, vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 c = 0;
                float4 s = tex2D(_MainTex,(i.uv));

                s.rgb = s.rbg;

                c = lerp(c, _Color1, smoothstep(_Cutoff, saturate(_Smoothness+ _Cutoff), s.r + s.g + s.b));
                c = lerp(c, _Color2, smoothstep(_Cutoff, saturate(_Smoothness+ _Cutoff), s.g + s.b ));
                c = lerp(c, _Color3, smoothstep(_Cutoff, saturate(_Smoothness+ _Cutoff), s.b ));

                // if (s.r + s.g + s.b > 0.5) {
                //     if (s.r > s.g && s.r > s.b) c = _Color1;
                //     if (s.g > s.r && s.g > s.b) c = _Color2;
                //     if (s.b > s.g && s.b > s.r) c = _Color3;
                // }

                return c;
            }

            ENDCG
        } 
    }
}    