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

Shader "Noise/CellFast01" 
{
	Properties 
	{
		_Octaves ("Octaves", Float) = 8.0
		_Frequency ("Frequency", Float) = 1.0
		_Amplitude ("Amplitude", Float) = 1.0
		_Lacunarity ("Lacunarity", Float) = 1.92
		_Persistence ("Persistence", Float) = 0.8
		_Offset ("Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_Transparency ("Transparency", Range(0.0, 1.0)) = 1.0
		_Displacement("Displacement", Float) = 1.0		_AnimSpeed("Animation Speed", Float) = 1.0		_LowColor("Low Color", Vector) = (0.0, 0.0, 0.0, 1.0)
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
		   void FAST32_hash_3D( 	float3 gridcell,
								out float4 lowz_hash_0,
								out float4 lowz_hash_1,
								out float4 lowz_hash_2,
								out float4 highz_hash_0,
								out float4 highz_hash_1,
								out float4 highz_hash_2	)		//	generates 3 random numbers for each of the 8 cell corners
		{
			//    gridcell is assumed to be an integer coordinate
		
			//	TODO: 	these constants need tweaked to find the best possible noise.
			//			probably requires some kind of brute force computational searching or something....
			const float2 OFFSET = float2( 50.0, 161.0 );
			const float DOMAIN = 69.0;
			const float3 SOMELARGEFLOATS = float3( 635.298681, 682.357502, 668.926525 );
			const float3 ZINC = float3( 48.500388, 65.294118, 63.934599 );
		
			//	truncate the domain
			gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * ( 1.0 / DOMAIN )) * DOMAIN;
			float3 gridcell_inc1 = step( gridcell, float3( DOMAIN - 1.5, DOMAIN - 1.5, DOMAIN - 1.5 ) ) * ( gridcell + 1.0 );
		
			//	calculate the noise
			float4 P = float4( gridcell.xy, gridcell_inc1.xy ) + OFFSET.xyxy;
			P *= P;
			P = P.xzxz * P.yyww;
			float3 lowz_mod = float3( 1.0 / ( SOMELARGEFLOATS.xyz + gridcell.zzz * ZINC.xyz ) );
			float3 highz_mod = float3( 1.0 / ( SOMELARGEFLOATS.xyz + gridcell_inc1.zzz * ZINC.xyz ) );
			lowz_hash_0 = frac( P * lowz_mod.xxxx );
			highz_hash_0 = frac( P * highz_mod.xxxx );
			lowz_hash_1 = frac( P * lowz_mod.yyyy );
			highz_hash_1 = frac( P * highz_mod.yyyy );
			lowz_hash_2 = frac( P * lowz_mod.zzzz );
			highz_hash_2 = frac( P * highz_mod.zzzz );
		}
		//	convert a 0.0->1.0 sample to a -1.0->1.0 sample weighted towards the extremes
		float4 Cellular_weight_samples( float4 samples )
		{
			samples = samples * 2.0 - 1.0;
			//return (1.0 - samples * samples) * sign(samples);	// square
			return (samples * samples * samples) - sign(samples);	// cubic (even more variance)
		}
		//
		//	Cellular Noise 3D
		//	Based off Stefan Gustavson's work at http://www.itn.liu.se/~stegu/GLSL-cellular
		//	http://briansharpe.files.wordpress.com/2011/12/cellularsample.jpg
		//
		//	Speed up by using 2x2x2 search window instead of 3x3x3
		//	produces range of 0.0->1.0
		//
		float Cellular3D(float3 P)
		{
			//	establish our grid cell and unit position
			float3 Pi = floor(P);
			float3 Pf = P - Pi;
		
			//	calculate the hash.
			//	( various hashing methods listed in order of speed )
			float4 hash_x0, hash_y0, hash_z0, hash_x1, hash_y1, hash_z1;
			FAST32_hash_3D( Pi, hash_x0, hash_y0, hash_z0, hash_x1, hash_y1, hash_z1 );
		
			//	generate the 8 random points
			//	restrict the random point offset to eliminate artifacts
			//	we'll improve the variance of the noise by pushing the points to the extremes of the jitter window
			const float JITTER_WINDOW = 0.166666666;	// 0.166666666 will guarentee no artifacts. It is the intersection on x of graphs f(x)=( (0.5 + (0.5-x))^2 + 2*((0.5-x)^2) ) and f(x)=( 2 * (( 0.5 + x )^2) + x * x )
			hash_x0 = Cellular_weight_samples( hash_x0 ) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
			hash_y0 = Cellular_weight_samples( hash_y0 ) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);
			hash_x1 = Cellular_weight_samples( hash_x1 ) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
			hash_y1 = Cellular_weight_samples( hash_y1 ) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);
			hash_z0 = Cellular_weight_samples( hash_z0 ) * JITTER_WINDOW + float4(0.0, 0.0, 0.0, 0.0);
			hash_z1 = Cellular_weight_samples( hash_z1 ) * JITTER_WINDOW + float4(1.0, 1.0, 1.0, 1.0);
		
			//	return the closest squared distance
			float4 dx1 = Pf.xxxx - hash_x0;
			float4 dy1 = Pf.yyyy - hash_y0;
			float4 dz1 = Pf.zzzz - hash_z0;
			float4 dx2 = Pf.xxxx - hash_x1;
			float4 dy2 = Pf.yyyy - hash_y1;
			float4 dz2 = Pf.zzzz - hash_z1;
			float4 d1 = dx1 * dx1 + dy1 * dy1 + dz1 * dz1;
			float4 d2 = dx2 * dx2 + dy2 * dy2 + dz2 * dz2;
			d1 = min(d1, d2);
			d1.xy = min(d1.xy, d1.wz);
			return min(d1.x, d1.y) * ( 9.0 / 12.0 );	//	scale return value from 0.0->1.333333 to 0.0->1.0  	(2/3)^2 * 3  == (12/9) == 1.333333
		}
		float CellFast(float3 p, fixed octaves, float3 offset, float frequency, float amplitude, float lacunarity, float persistence)
		{
			float sum = 0;
			for (int i = 0; i < octaves; i++)
			{
				float h = 0;
				h = Cellular3D((p + offset) * frequency);
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
		fixed _Transparency;
		float _Displacement;		float _AnimSpeed;		fixed4 _LowColor;
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
			OUT.pos = float3(v.texcoord.xy, _Time.y * _AnimSpeed);
			float h = CellFast(OUT.pos, _Octaves, _Offset, _Frequency, _Amplitude, _Lacunarity, _Persistence);
			v.vertex.xyz += v.normal * h * _Displacement;
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			float h = CellFast(IN.pos, _Octaves, _Offset, _Frequency, _Amplitude, _Lacunarity, _Persistence);
			

			
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