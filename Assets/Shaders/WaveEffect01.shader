Shader "Custom/WaveEffect01" {
	Properties{
		_MainTex("Texture", 2D) = "white" { }
	_SecondTex("Texture", 2D) = "white" { }
	_Dim("Float", Float) = 0
		_Mouse("Float", Vector) = (0,0,0,0)
	}

		CGINCLUDE

#include "UnityCG.cginc"
#pragma target 3.0

		sampler2D _MainTex;
	sampler2D _SecondTex;

	float4 _MainTex_ST;
	float _Dim;
	float4 _Mouse;


	float4 frag_initialcondition(v2f_img i) : SV_TARGET{


		float fx = floor(i.uv.x*_Dim) / (_Dim - 1.0);
	float fy = floor(i.uv.y*_Dim) / (_Dim - 1.0);

	return float4(fx,fy,0.0,0.0);
	}

		float4 frag_initialconditionvel(v2f_img i) : SV_TARGET{

		float fx = floor(i.uv.x*_Dim) / (_Dim - 1.0);
	float fy = floor(i.uv.y*_Dim) / (_Dim - 1.0);

	return .1 * float4(fx - 0.5,fy - 0.5,0.0,0.0);

	}


		//Aqui MainTex es para la textura de posiciones
		float4 frag_updatevel(v2f_img i) : SV_TARGET{


		float2 xy = tex2D(_MainTex,i.uv).rg;
		float2 xa = tex2D(_MainTex,i.uv - float2(1.0 / (_Dim - 1.0),0.0)).rg;
		float2 xb = tex2D(_MainTex,i.uv + float2(1.0 / (_Dim - 1.0),0.0)).rg;
		float2 ya = tex2D(_MainTex,i.uv - float2(0.0,1.0 / (_Dim - 1.0))).rg;
		float2 yb = tex2D(_MainTex,i.uv + float2(0.0,1.0 / (_Dim - 1.0))).rg;


		float2 curvel = tex2D(_SecondTex,i.uv).rg;
		float2 v = (xy - _Mouse.xy);
		float l = length(v);

		if (_Mouse.x>0.0f) {
			curvel += 0.1*v*exp(-10.0*l*l);
		}

		float k = 10.0;
		float2 vel = curvel + 0.2*k*(-(xy - xa) + (xb - xy) - (xy - ya) + (yb - xy)) - 0.01*length(curvel)*normalize(curvel);
		return float4(vel.x,vel.y,0.0,0.0);
	}

		float4 frag_updatepos(v2f_img i) : SV_TARGET{

		if (i.uv.x<1.0 / _Dim || i.uv.x>(_Dim - 1.0) / _Dim || i.uv.y<1.0 / _Dim || i.uv.y>(_Dim - 1.0) / _Dim) {
			return tex2D(_MainTex,i.uv).rgba;
		}

	float2 xy = tex2D(_MainTex,i.uv).rg + 0.2*tex2D(_SecondTex,i.uv).rg;
	return float4(xy.x,xy.y,0.0,0.0);
	}


		//Aqui MainTex es para la textura de posiciones
		float4 frag_draw(v2f_img i) : COLOR{

		float width = 0.95;
	float freq = 20;

	float2 uv = tex2D(_MainTex,i.uv).rg;
	float f1 = sin(freq*2.0*3.14159*uv.x);
	f1 = smoothstep(width,1.0,f1);

	float f2 = sin(freq*2.0*3.14159*uv.y);
	f2 = smoothstep(width,1.0,f2);

	f1 = max(f1,f2);



	return lerp(tex2D(_SecondTex,uv).rgba,float4(0.3,0.3,0.3,1.0),smoothstep(0.3,0.8,i.uv.x))
		+ lerp(float4(0.0,0.0,0.0,0.0),f1*float4(0.0,191.0 / 255.0,1.0,0.0),smoothstep(0.3,0.7,i.uv.x));

	}





		ENDCG


		SubShader {
		Pass{

			CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag_initialcondition
			ENDCG
		}

			Pass{

			CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag_initialconditionvel
			ENDCG
		}

			Pass{

			CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag_updatevel
			ENDCG
		}

			Pass{

			CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag_updatepos
			ENDCG
		}

			Pass{

			CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag_draw
			ENDCG
		}

	}
}