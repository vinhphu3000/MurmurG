//
//	Code repository for GPU noise development blog
//	http://briansharpe.wordpress.com
//	https://github.com/BrianSharpe
//
//	I'm not one for copyrights.  Use the code however you wish.
//	All I ask is that credit be given back to the blog or myself when appropriate.
//	And also to let me know if you come up with any changes, improvements, thoughts or interesting uses for this stuff. :)
//	Thanks!
//
//	Brian Sharpe
//	brisharpe CIRCLE_A yahoo DOT com
//	http://briansharpe.wordpress.com
//	https://github.com/BrianSharpe
//
//===============================================================================
//  Scape Software License
//===============================================================================
//
//Copyright (c) 2007-2012, Giliam de Carpentier
//All rights reserved.
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met: 
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer. 
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNERS OR CONTRIBUTORS BE LIABLE 
//FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
//CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
//OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
//OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.;

Shader "Noise/ValueStatic01" 
{
	Properties 
	{
		_Octaves ("Octaves", Float) = 8.0
		_Frequency ("Frequency", Float) = 1.0
		_Amplitude ("Amplitude", Float) = 1.0
		_Lacunarity ("Lacunarity", Float) = 1.92
		_Persistence ("Persistence", Float) = 0.8
		_Offset ("Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_RidgeOffset ("Ridge Offset", Float) = 1.0
		_Transparency ("Transparency", Range(0.0, 1.0)) = 1.0
		_Displacement("Displacement", Float) = 1.0		_LowColor("Low Color", Vector) = (0.0, 0.0, 0.0, 1.0)
		_HighColor("High Color", Vector) = (1.0, 1.0, 1.0, 1.0)
		_LowTexture("Low Texture", 2D) = "" {}
		_HighTexture("High Texture", 2D) = "" {}

	}

	CGINCLUDE
		//
		//	FAST32_hash
		//	A very fast hashing function.  Requires 32bit support.
		//	http://briansharpe.wordpress.com/2011/11/15/a-fast-and-simple-32bit-floating-point-hash-function/
		//
		//	The hash formula takes the form....
		//	hash = mod( coord.x * coord.x * coord.y * coord.y, SOMELARGEFLOAT ) / SOMELARGEFLOAT
		//	We truncate and offset the domain to the most interesting part of the noise.
		//	SOMELARGEFLOAT should be in the range of 400.0->1000.0 and needs to be hand picked.  Only some give good results.
		//	3D Noise is achieved by offsetting the SOMELARGEFLOAT value by the Z coordinate
		//
		void FAST32_hash_3D( float3 gridcell, out float4 lowz_hash, out float4 highz_hash )	//	generates a random number for each of the 8 cell corners
		{
			//    gridcell is assumed to be an integer coordinate
		
			//	TODO: 	these constants need tweaked to find the best possible noise.
			//			probably requires some kind of brute force computational searching or something....
			const float2 OFFSET = float2( 50.0, 161.0 );
			const float DOMAIN = 69.0;
			const float SOMELARGEFLOAT = 635.298681;
			const float ZINC = 48.500388;
		
			//	truncate the domain
			gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * ( 1.0 / DOMAIN )) * DOMAIN;
			float3 gridcell_inc1 = step( gridcell, float3( DOMAIN - 1.5, DOMAIN - 1.5, DOMAIN - 1.5 ) ) * ( gridcell + 1.0 );
		
			//	calculate the noise
			float4 P = float4( gridcell.xy, gridcell_inc1.xy ) + OFFSET.xyxy;
			P *= P;
			P = P.xzxz * P.yyww;
			highz_hash.xy = float2( 1.0 / ( SOMELARGEFLOAT + float2( gridcell.z, gridcell_inc1.z ) * ZINC ) );
			lowz_hash = frac( P * highz_hash.xxxx );
			highz_hash = frac( P * highz_hash.yyyy );
		}
		//
		//	Interpolation functions
		//	( smoothly increase from 0.0 to 1.0 as x increases linearly from 0.0 to 1.0 )
		//	http://briansharpe.wordpress.com/2011/11/14/two-useful-interpolation-functions-for-noise-development/
		//
		float3 Interpolation_C2( float3 x ) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
		//
		//	Value Noise 3D
		//	Return value range of 0.0->1.0
		//	http://briansharpe.files.wordpress.com/2011/11/valuesample1.jpg
		//
		float Value3D( float3 P )
		{
			//	establish our grid cell and unit position
			float3 Pi = floor(P);
			float3 Pf = P - Pi;
		
			//	calculate the hash.
			//	( various hashing methods listed in order of speed )
			float4 hash_lowz, hash_highz;
			FAST32_hash_3D( Pi, hash_lowz, hash_highz );
		
			//	blend the results and return
			float3 blend = Interpolation_C2( Pf );
			float4 res0 = lerp( hash_lowz, hash_highz, blend.z );
			float2 res1 = lerp( res0.xy, res0.zw, blend.y );
			return lerp( res1.x, res1.y, blend.x );
		}
		float ValueRidged(float3 p, int octaves, float3 offset, float frequency, float amplitude, float lacunarity, float persistence, float ridgeOffset)
		{
			float sum = 0;
			for (int i = 0; i < octaves; i++)
			{
				float h = 0;
				h = 0.5 * (ridgeOffset - abs(4*Value3D((p + offset) * frequency)));
				sum += h*amplitude;
				frequency *= lacunarity;
				amplitude *= persistence;
			}
			return sum;
		}

	ENDCG

	SubShader 
	{
		Tags {"Queue"="Transparent"}

		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma glsl
		#pragma target 3.0
		
		fixed _Octaves;
		float _Frequency;
		float _Amplitude;
		float3 _Offset;
		float _Lacunarity;
		float _Persistence;
		float _RidgeOffset;
		fixed _Transparency;
		float _Displacement;		fixed4 _LowColor;
		fixed4 _HighColor;
		sampler2D _LowTexture;
		sampler2D _HighTexture;


		struct Input 
		{
			float3 pos;
			float2 uv_LowTexture;
			float2 uv_HighTexture;
		};

		void vert (inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);
			OUT.pos = v.vertex.xyz;
			float h = ValueRidged(OUT.pos, _Octaves, _Offset, _Frequency, _Amplitude, _Lacunarity, _Persistence, _RidgeOffset);
			v.vertex.xyz += v.normal * h * _Displacement;
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			float h = ValueRidged(IN.pos, _Octaves, _Offset, _Frequency, _Amplitude, _Lacunarity, _Persistence, _RidgeOffset);
			

			
			float4 color = float4(0.0, 0.0, 0.0, 0.0);
			float4 lowTexColor = tex2D(_LowTexture, IN.uv_LowTexture);
			float4 highTexColor = tex2D(_HighTexture, IN.uv_HighTexture);
			color = lerp(_LowColor * lowTexColor, _HighColor * highTexColor, h);

			o.Albedo = color.rgb;
			o.Alpha = h * _Transparency;
		}
		ENDCG
	}
	
	FallBack "Diffuse"
}