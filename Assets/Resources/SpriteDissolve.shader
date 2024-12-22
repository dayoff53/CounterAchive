Shader "Custom/SpriteDissolve"
{
    Properties
    {
        //스프라이트 텍스처를 지정하세요.
        _MainTex("Main Sprite", 2D) = "white" {}

        //디졸브를 위한 노이즈 텍스처를 지정하세요.
        _NoiseTex("Noise Texture", 2D) = "white" {}

        //스프라이트 베이스 컬러(乘연산)
        _Color("Tint Color", Color) = (1,1,1,1)

        //디졸브되는 경계선의 색상(HDR 가능)
        [HDR]_EdgeColor("Edge Color", Color) = (1,1,1,1)

        //디졸브 시작 지점(노이즈를 얼마나 잘라낼 것인지)
        _Cutoff("Cut Off", Range(0, 1)) = 0.25

        //디졸브 경계의 두께
        _EdgeWidth("Edge Width", Range(0, 1)) = 0.05

        //Cull Mode
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"         // 투명 렌더 큐
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
            }

            Cull[_Cull]
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                // alphaTest(실질적으로 clip()을 통해 수행), nofog, 등 옵션은 상황에 맞춰 추가

                #include "UnityCG.cginc"

                // 텍스처 샘플러
                sampler2D _MainTex;
                sampler2D _NoiseTex;

                // 프로퍼티
                float4 _Color;
                float4 _EdgeColor;
                float _Cutoff;
                float _EdgeWidth;

                // UV 정보를 전달할 구조체
                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                // 픽셀 셰이더로 전달할 구조체
                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                // 정점 셰이더
                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                // 픽셀(프래그먼트) 셰이더
                fixed4 frag(v2f i) : SV_Target
                {
                    // 스프라이트 텍스처 샘플링
                    fixed4 mainCol = tex2D(_MainTex, i.uv) * _Color;

                // 노이즈 텍스처 샘플링
                float noiseVal = tex2D(_NoiseTex, i.uv).r;

                // 디졸브 영역 판단
                // noiseVal < _Cutoff 이면 clip
                clip(noiseVal >= _Cutoff ? 1 : -1);

                // 경계선 계산
                // _Cutoff ~ (_Cutoff + _EdgeWidth) 구간을 경계로 사용
                float edgeStart = _Cutoff;
                float edgeEnd = _Cutoff + _EdgeWidth;
                float edgeFactor = 1.0 - smoothstep(edgeStart, edgeEnd, noiseVal);

                // 경계선 색상 보정
                fixed4 finalCol = mainCol;

                // 경계가 될 부분에 _EdgeColor를 덧입힘
                // edgeFactor가 1이면 경계선, 0이면 경계선 아님
                fixed4 edgeCol = lerp(fixed4(0,0,0,0), _EdgeColor, edgeFactor);

                finalCol.rgb += edgeCol.rgb * edgeCol.a;

                // 투명도 결정 (알파)
                // 만약 디졸브가 진행된 부분은 alpha=0, 남은 부분 alpha=1
                // clip()을 이미 했으므로, 경계 부분 제외하면 사실 1 또는 버려짐(clip)이다.
                // 경계선 부분만 약간 alpha를 적용하고 싶다면 아래처럼 조절 가능
                finalCol.a *= (1.0 - edgeFactor * 0.5);

                return finalCol;
            }
            ENDCG
        }
        }
            FallBack "Unlit/Transparent"
}
