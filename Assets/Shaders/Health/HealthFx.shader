Shader "Custom/HealthFx"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _FillAmount ("Fill Amount", Range(0,1)) = 0.5
        _WaveAmplitude ("Wave Amplitude", Float) = 0.05
        _WaveFrequency ("Wave Frequency", Float) = 10.0
        _TimeValue ("Time", Float) = 0.0
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _FillAmount;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _TimeValue;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Time-based sine wave effect (wave moves over time)
                float waveOffset = sin(i.uv.x * _WaveFrequency + _TimeValue) * _WaveAmplitude;
                if (_FillAmount >= 1 || _FillAmount <= 0)
                {
                    waveOffset = 0;
                }

                // Apply sine wave to the fill threshold
                float fillThreshold = step(i.uv.y, _FillAmount + waveOffset);

                // Blend original color with red based on threshold
                fixed4 fillColor = lerp(texColor, fixed4(1, 0, 0, texColor.a), fillThreshold);

                return fillColor;
            }
            ENDCG
        }
    }
}
