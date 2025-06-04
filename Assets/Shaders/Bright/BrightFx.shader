Shader "Custom/BrightFx"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _BrightFx ("Brightness Effect", Range(-1, 1)) = 0
        _TintColor ("Tint Color", Color) = (1, 1, 1, 1) // New Tint Color property
        _HueShift ("Hue Shift", Range(0, 1)) = 0 // Hue shift property
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _BrightFx;
            fixed4 _TintColor; // Declare Tint Color variable
            float _HueShift;   // Declare the HueShift variable

            // Function to convert RGB to HSL
            float3 RGBToHSL(fixed3 c)
            {
                float cMax = max(c.r, max(c.g, c.b));
                float cMin = min(c.r, min(c.g, c.b));
                float delta = cMax - cMin;

                float h = 0.0;
                if (delta > 0.0)
                {
                    if (cMax == c.r)
                        h = (c.g - c.b) / delta;
                    else if (cMax == c.g)
                        h = (c.b - c.r) / delta + 2.0;
                    else
                        h = (c.r - c.g) / delta + 4.0;
                }

                float l = (cMax + cMin) * 0.5;
                float s = (cMax == cMin) ? 0.0 : (cMax - cMin) / (1.0 - abs(2.0 * l - 1.0));

                h = frac(h / 6.0); // Ensure hue is between 0 and 1
                return float3(h, s, l);
            }

            // Function to convert HSL back to RGB
            fixed3 HSLToRGB(float3 hsl)
            {
                float3 rgb;
                float c = (1.0 - abs(2.0 * hsl.z - 1.0)) * hsl.y;
                float x = c * (1.0 - abs(fmod(hsl.x * 6.0, 2.0) - 1.0));
                float m = hsl.z - c * 0.5;

                if (hsl.x < 1.0 / 6.0)
                    rgb = float3(c, x, 0.0);
                else if (hsl.x < 2.0 / 6.0)
                    rgb = float3(x, c, 0.0);
                else if (hsl.x < 3.0 / 6.0)
                    rgb = float3(0.0, c, x);
                else if (hsl.x < 4.0 / 6.0)
                    rgb = float3(0.0, x, c);
                else if (hsl.x < 5.0 / 6.0)
                    rgb = float3(x, 0.0, c);
                else
                    rgb = float3(c, 0.0, x);

                return rgb + m;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture color
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Apply brightness effect
                if (_BrightFx >= 0.0)
                {
                    texColor.rgb = lerp(texColor.rgb, fixed3(1.0, 1.0, 1.0), _BrightFx);
                }
                else
                {
                    texColor.rgb = lerp(texColor.rgb, fixed3(0.0, 0.0, 0.0), -_BrightFx);
                }

                // Apply tint color (modulate with original texture color)
                texColor *= _TintColor;

                if (_HueShift > 0){
                    // Convert RGB to HSL
                    float3 hsl = RGBToHSL(texColor.rgb);

                    // Apply the hue shift (mod by 1.0 to wrap the hue)
                    hsl.x += _HueShift;
                    hsl.x = frac(hsl.x); // Ensure hue wraps around correctly

                    // Convert back to RGB
                    texColor.rgb = HSLToRGB(hsl);
                }

                return texColor;
            }
            ENDCG
        }
    }
}