using System;
using System.Collections.Generic;
using UnityEngine;

public static class TsvrNftMetadataParser {

    private static AudioFeature StringToAudioFeature(string feature) {
        return (AudioFeature)Enum.Parse(typeof(AudioFeature), feature);
    }

    public static GranularParameters NftAttributesToParameters(NFTAttribute[] attributes) {
        GranularParameters parameters = new GranularParameters();
        foreach (NFTAttribute attribute in attributes) {
            switch (attribute.trait_type) {
                case "x-feature":
                    parameters.xFeature = StringToAudioFeature(attribute.value.ToString());
                    break;
                case "y-feature":
                    parameters.yFeature = StringToAudioFeature(attribute.value.ToString());
                    break;
                case "z-feature":
                    parameters.zFeature = StringToAudioFeature(attribute.value.ToString());
                    break;
                case "hop-size":
                    parameters.hopSize = Convert.ToInt32(attribute.value);
                    break;
                case "position-axis-scale":
                    parameters.posAxisScale = (float[])attribute.value;
                    break;
                case "scale-exponential":
                    parameters.scaleExp = Convert.ToSingle(attribute.value);
                    break;
                case "use-hsv":
                    parameters.useHSV = Convert.ToBoolean(attribute.value);
                    break;
                default:
                    Debug.Log("Unknown attribute: " + attribute.trait_type);
                    break;
            }
        }
        return parameters;
    }

    public static NFTAttribute[] ParametersToNftAttributes(GranularParameters parameters) {
        return new NFTAttribute[] {
            new NFTAttribute {
                trait_type = "x-feature",
                value = parameters.xFeature.ToString()
            },
            new NFTAttribute {
                trait_type = "y-feature",
                value = parameters.yFeature.ToString()
            },
            new NFTAttribute {
                trait_type = "z-feature",
                value = parameters.zFeature.ToString()
            },
            new NFTAttribute {
                trait_type = "hop-size",
                value = parameters.hopSize
            },
            new NFTAttribute {
                trait_type = "position-axis-scale",
                value = parameters.posAxisScale
            },
            new NFTAttribute {
                trait_type = "scale-exponential",
                value = parameters.scaleExp
            },
            new NFTAttribute {
                trait_type = "use-hsv",
                value = parameters.useHSV
            },
        };
    }
}


// NFTMetadata metadata = new NFTMetadata();
// metadata.name = "TSVR NFT";
// metadata.description = "TSVR NFT";
// metadata.image = "https://i.imgur.com/4ZQZQ9I.png";
// metadata.external_url = "https://tsvr.io";
// metadata.animation_url = "https://tsvr.io";
// metadata.background_color = null;
// metadata.youtube_url = null;