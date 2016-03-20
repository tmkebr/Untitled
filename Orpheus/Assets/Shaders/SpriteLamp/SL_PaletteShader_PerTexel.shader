

Shader "SpriteLamp/Palette_PerTexel"
{
    Properties
    {
        _MainTex ("Diffuse Texture", 2D) = "white" {}		//In future this won't be needed - it is here just for the alpha channel.
        _NormalDepth ("Normal Depth", 2D) = "bump" {} 		//Normal information in the colour channels, depth in the alpha channel.
        _SpecGloss ("Specular Gloss", 2D) = "" {}			//Specular colour in the colour channels, and glossiness in the alpha channel.
        _PaletteMap ("Palette Map", 2D) = "" {}				//The palette map - needn't resemble the dimensions of the other textures.
        _IndexMap ("Index Map", 2D) = "" {}					//Eventually this will have an alpha channel so the diffuse map can be gotten rid of entirely.
        
       
        _SpecExponent ("Specular Exponent", Range (1.0,50.0)) = 10.0		//Multiplied by the alpha channel of the spec map to get the specular exponent.
        _AmplifyDepth ("Amplify Depth", Range (0,1.0)) = 0.0	//Affects the 'severity' of the depth map - affects shadows (and shading to a lesser extent).
        _TextureRes("Texture Resolution", Vector) = (256, 256, 0, 0)	//Leave this to be set via a script.
        _LightWrap("Wraparound lighting", Range (0,1.0)) = 0.0	//Higher values of this will cause diffuse light to 'wrap around' and light the away-facing pixels a bit.
        
        _SpotlightHardness("Spotlight hardness", Range(1.0, 10.0)) = 2.0	//Higher number makes the edge of a spotlight harder.
        _AttenuationExponent("Attenuation exponent", Range(0.0, 4.0)) = 2.0	//Higher number makes attenuation dropoff faster at first.
        
  
    }

    SubShader
    {
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
			
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest NotEqual 0.0
		
        Pass
        {    
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag
            #pragma target 3.0
            #pragma glsl
			
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _NormalDepth;
            uniform sampler2D _IndexMap;
            uniform sampler2D _PaletteMap;
            uniform sampler2D _SpecGloss;
            
            uniform float4 _LightColor0;
            uniform float _SpecExponent;
            uniform float _AmplifyDepth;
            uniform float _CelShadingLevels;
            uniform float4 _TextureRes;
            uniform float _LightWrap;
            
            
            uniform float4x4 _LightMatrix0; // transformation

            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float4 posLight : TEXCOORD2;
            };

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;

                output.posWorld = mul(_Object2World, input.vertex);
                output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
                
                output.uv = input.uv.xy;
                output.color = input.color;
				output.posLight = mul(_LightMatrix0, output.posWorld);
				
                return output;
            }

            float4 frag(VertexOutput input) : COLOR
            {
                //Do texture reads first, because in theory that's a bit quicker...
                float4 diffuseColour = tex2D(_MainTex, input.uv);
				float4 normalDepth = tex2D(_NormalDepth, input.uv);				
				float4 specGlossValues = tex2D(_SpecGloss, input.uv);
				float4 indexValues = tex2D(_IndexMap, input.uv);
                //In the ambient pass we deliberately take the darkest value available and use
                //that, because if we use black the lit shader has to fade to black, which goes
                //against the whole point of the palette shader.
                float4 paletteLookup = tex2D (_PaletteMap, float2(indexValues.r, 0.0));
                
                float4 ambientColour = float4(paletteLookup.rgb, diffuseColour.a);
                
                
                //Now we do a pass with a light because Unity folds a light into the ambient pass.
                //MOST OF THIS HASN'T CHANGED FROM THE REGULAR SPRITE LAMP SHADER.
            	//Scroll to the end for the bit that's different.
            	
            	
				
				//Get the real vector for the normal, 
		        float3 normalDirection = (normalDepth.xyz - 0.5) * 2.0;
                normalDirection.z *= -1.0;
                normalDirection = normalize(normalDirection);

				
                float depthColour = normalDepth.a;
                                
                float2 roundedUVs = input.uv;
               
                float3 vertexToLightSource;
                float3 lightDirection;
                
            	//This handles directional lights
              	lightDirection = normalize(float3(_WorldSpaceLightPos0.xyz));
	                     
                
                float aspectRatio = _TextureRes.x / _TextureRes.y;
                
                //We calculate shadows here. Magic numbers incoming (FIXME).
                float shadowMult = 1.0;
                float3 moveVec = lightDirection.xyz * 0.006 * float3(1.0, aspectRatio, -1.0);
                float thisHeight = depthColour * _AmplifyDepth;
               
                float3 tapPos = float3(roundedUVs, thisHeight + 0.1);
                //This loop traces along the light ray and checks if that ray is inside the depth map at each point.
                //If it is, darken that pixel a bit.
                for (int i = 0; i < 8; i++)
				{
					tapPos += moveVec;
					float tapDepth = tex2D(_NormalDepth, tapPos.xy).a * _AmplifyDepth;
					if (tapDepth > tapPos.z)
					{
						shadowMult -= 0.125;
					}
				}
                shadowMult = clamp(shadowMult, 0.0, 1.0);
                
                
                
                

                // Compute diffuse part of lighting
                float normalDotLight = dot(normalDirection, lightDirection);
                
                //Slightly awkward maths for light wrap.
                float diffuseLevel = clamp(normalDotLight + _LightWrap, 0.0, _LightWrap + 1.0) / (_LightWrap + 1.0) * shadowMult;
                
                // Compute specular part of lighting
                float specularLevel;
                if (normalDotLight < 0.0)
                {
                    // Light is on the wrong side, no specular reflection
                    specularLevel = 0.0;
                }
                else
                {
                    // For the moment, since this is 2D, we'll say the view vector is always (0, 0, -1).
                    //This isn't really true when you're not using a orthographic camera though. FIXME.
                    float3 viewDirection = float3(0.0, 0.0, -1.0);
                    specularLevel = pow(max(0.0, dot(reflect(-lightDirection, normalDirection),
                        viewDirection)), _SpecExponent * specGlossValues.a);
                }



				//THIS IS THE BIT THAT HAS CHANGED TO MAKE USE OF THE PALETTE SYSTEM
				//We get the 'lightValue' as the average of the diffuse and specular levels.
				float lightValue = (diffuseLevel + specularLevel) * 0.5;
				
				//Here, we get the colour value from the palette map, and then subtract the darkest colour.
				//This is to compensate for the fact that the darkest colour has already come from the ambient pass
				//in Unity. Subtracting the darkest colour here, then adding the result, means that we end up
				//with the correct colour depicted by the palette map, so long as there are zero or one lights.
				//More lights should still look okay, but will technically violate the 'only colours from the palette map' rule.
				float3 paletteColour = tex2D(_PaletteMap, float2(indexValues.r, lightValue)).rgb - tex2D(_PaletteMap, float2(indexValues.r, 0.0)).rgb;
				float4 finalColour = float4(paletteColour, diffuseColour.a) + ambientColour;
				
                finalColour.a = diffuseColour.a;
                
                return finalColour;
            }

            ENDCG
        }

        Pass
        {    
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One // additive blending 

            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag 
			#pragma target 3.0
			#pragma glsl
			
			#pragma multi_compile_lightpass

            #include "UnityCG.cginc"

            // User-specified properties
            uniform sampler2D _MainTex;
            uniform sampler2D _NormalDepth;
            uniform sampler2D _SpecGloss;
            uniform sampler2D _IndexMap;
            uniform sampler2D _PaletteMap;
            
            uniform float4 _LightColor0;
            uniform float _SpecExponent;
            uniform float _AmplifyDepth;
            uniform float _CelShadingLevels;
            uniform float4 _TextureRes;
            uniform float _LightWrap;
            uniform float _SpotlightHardness;
            uniform float _AttenuationExponent;
            
            uniform float4x4 _LightMatrix0; // transformation

            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float4 posLight : TEXCOORD2;
            };

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;

                output.posWorld = mul(_Object2World, input.vertex);
                output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
                
                output.uv = input.uv.xy;
                output.color = input.color;
				output.posLight = mul(_LightMatrix0, output.posWorld);
				
                return output;
            }

            float4 frag(VertexOutput input) : COLOR
            {
            
            	//MOST OF THIS HASN'T CHANGED FROM THE REGULAR SPRITE LAMP SHADER.
            	//Scroll to the end for the bit that's different.
            	
            	//Do texture reads first, because in theory that's a bit quicker...
                float4 diffuseColour = tex2D(_MainTex, input.uv);
				float4 normalDepth = tex2D(_NormalDepth, input.uv);				
				float4 specGlossValues = tex2D(_SpecGloss, input.uv);
				float4 indexValues = tex2D(_IndexMap, input.uv);
				
				//Get the real vector for the normal, 
		        float3 normalDirection = (normalDepth.xyz - 0.5) * 2.0;
                normalDirection.z *= -1.0;
                normalDirection = normalize(normalDirection);

				
                float depthColour = normalDepth.a;
                                
                float2 roundedUVs = input.uv;
                
                #if (!(defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)))
                //For per-texel lighting, we recreate the world position based on the sprite's UVs...
                float2 positionOffset = input.uv;
                
                //Intervening here to round the UVs to the nearest 1.0/TextureRes to clamp the world position
                //to the nearest pixel...
                roundedUVs *= _TextureRes.xy;
                roundedUVs = floor(roundedUVs) + 0.5;
                roundedUVs /= _TextureRes.xy;
                
                
                //This is the per-texel stuff! It's a work in progress - that's why it's commented out right now.
                
                float2 uvCorrection = input.uv - roundedUVs;
                
                //Get tangent and bitangent stuff:
                float3 p_dx = ddx(input.posWorld.xyz);
				float3 p_dy = ddy(input.posWorld.xyz);
				//Rate of change of the texture coords
				float2 tc_dx = ddx(input.uv.xy);
				float2 tc_dy = ddy(input.uv.xy);
				//Initial tangent and bitangent
				
				float3 fragTangent = ( tc_dy.y * p_dx - tc_dx.y * p_dy ) / (length(p_dy) * length(p_dy));
				float3 fragBitangent = ( tc_dy.x * p_dx - tc_dx.x * p_dy ) / (length(p_dx) * length(p_dx));
				
				fragTangent /= (length(fragTangent) * length(fragTangent));
				fragBitangent /= (length(fragBitangent) * length(fragBitangent));
				
				//This corrects for nonuniform scales that reverse the handedness of the whole thing.
				float uvCross = clamp((cross(fragTangent, fragBitangent)).z * -10000000.0, -1.0, 1.0);
				
				float3 uVector = -normalize(fragTangent) * length(fragBitangent) * uvCross;
				float3 vVector = normalize(fragBitangent) * length(fragTangent) * uvCross;
                
                float3 posWorld = input.posWorld.xyz + uVector * -uvCorrection.x + vVector * -uvCorrection.y;
#else
				float3 posWorld = input.posWorld.xyz;
#endif
                
                
                float3 resultToDraw = posWorld * 10.0;

                
                
                posWorld.z -= depthColour * _AmplifyDepth;	//The fragment's Z position is modified based on the depth map value.
                float3 vertexToLightSource;
                float3 lightDirection;
                float attenuation;
                #if (defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE))
            	//This handles directional lights
              	lightDirection = normalize(float3(_WorldSpaceLightPos0.xyz));
       	   		attenuation = 1.0;
#else
            	//This code is for point/spot lights. Note that light cookies aren't yet handled for spot lights yet (FIXME)
            	float cookieAttenuation = 1.0;
         	    float normalisedDistance = 1.0;
        		vertexToLightSource = float3(_WorldSpaceLightPos0.xyz) - posWorld;
            	
            	float lightDistance = length(vertexToLightSource);
            	
#if SPOT
            	//This number, 'distance from centre', is the distance this fragment is from the centre line
            	//of the spot light. If it is greater than 1.0, this fragment is outside the light cone and shouldn't be
            	//illuminated.
            	float distanceFromCentre = length (float2(input.posLight.xy) / input.posLight.w) * 2.0;
            	normalisedDistance = input.posLight.z;
            	
            	//Fairly simplistic implementation of a default spotlight shape. Not total rubbish like the last one was,
            	//and doesn't require a texture lookup, but still probably not perfect.
            	cookieAttenuation = (1.0 - distanceFromCentre) * _SpotlightHardness * _SpotlightHardness;
            	cookieAttenuation = clamp(cookieAttenuation, 0.0, 1.0);
            	
#elif POINT_NOATT
	            normalisedDistance = 0.0;
	            cookieAttenuation = 1.0;
#else
	    		cookieAttenuation = 1.0;
	    		normalisedDistance = length(input.posLight.xyz);
            	
#endif
	        	lightDirection = float3(mul(float4(vertexToLightSource, 1.0), _Object2World).xyz);
	        	lightDirection = normalize(lightDirection);
                
                attenuation = 1.0 - normalisedDistance;
                attenuation = clamp(attenuation, 0.0, 1.0);
                attenuation  = pow(attenuation, _AttenuationExponent);
                
                attenuation *= cookieAttenuation;
	            
#endif
                                
                
                float aspectRatio = _TextureRes.x / _TextureRes.y;
                
                //We calculate shadows here. Magic numbers incoming (FIXME).
                float shadowMult = 1.0;
                float3 moveVec = lightDirection.xyz * 0.006 * float3(1.0, aspectRatio, -1.0);
                float thisHeight = depthColour * _AmplifyDepth;
               
                float3 tapPos = float3(roundedUVs, thisHeight + 0.1);
                //This loop traces along the light ray and checks if that ray is inside the depth map at each point.
                //If it is, darken that pixel a bit.
                for (int i = 0; i < 8; i++)
				{
					tapPos += moveVec;
					float tapDepth = tex2D(_NormalDepth, tapPos.xy).a * _AmplifyDepth;
					if (tapDepth > tapPos.z)
					{
						shadowMult -= 0.125;
					}
				}
                shadowMult = clamp(shadowMult, 0.0, 1.0);
                
                
                
                

                // Compute diffuse part of lighting
                float normalDotLight = dot(normalDirection, lightDirection);
                
                //Slightly awkward maths for light wrap.
                float diffuseLevel = clamp(normalDotLight + _LightWrap, 0.0, _LightWrap + 1.0) / (_LightWrap + 1.0) * attenuation * shadowMult;
                
                // Compute specular part of lighting
                float specularLevel;
                if (normalDotLight < 0.0)
                {
                    // Light is on the wrong side, no specular reflection
                    specularLevel = 0.0;
                }
                else
                {
                    // For the moment, since this is 2D, we'll say the view vector is always (0, 0, -1).
                    //This isn't really true when you're not using a orthographic camera though. FIXME.
                    float3 viewDirection = float3(0.0, 0.0, -1.0);
                    specularLevel = attenuation * pow(max(0.0, dot(reflect(-lightDirection, normalDirection),
                        viewDirection)), _SpecExponent * specGlossValues.a);
                }



				//THIS IS THE BIT THAT HAS CHANGED TO MAKE USE OF THE PALETTE SYSTEM
				//We get the 'lightValue' as the average of the diffuse and specular levels.
				float lightValue = (diffuseLevel + specularLevel) * 0.5;
				
				//Here, we get the colour value from the palette map, and then subtract the darkest colour.
				//This is to compensate for the fact that the darkest colour has already come from the ambient pass
				//in Unity. Subtracting the darkest colour here, then adding the result, means that we end up
				//with the correct colour depicted by the palette map, so long as there are zero or one lights.
				//More lights should still look okay, but will technically violate the 'only colours from the palette map' rule.
				float3 paletteColour = tex2D(_PaletteMap, float2(indexValues.r, lightValue)).rgb - tex2D(_PaletteMap, float2(indexValues.r, 0.0)).rgb;

                return float4(paletteColour * diffuseColour.a, diffuseColour.a);
                
             }

             ENDCG
        }
    }
    // The definition of a fallback shader should be commented out 
    // during development:
     Fallback "Transparent/Diffuse"
}