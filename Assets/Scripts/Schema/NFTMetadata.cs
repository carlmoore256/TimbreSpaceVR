using System;
using System.Collections.Generic;
using UnityEngine;

public struct TsvrNftResources {
    string audio; // a list of audio files that will be loaded and concatenated in tsvr
    string config; // a link to a JSON file containing any additional configuration
}

public class NFTAttribute {
    public string trait_type;
    public string display_type;
    public object value; // make this infer the type
}

// hash the filename when storing in appdata
// so when it checks the cache it's easy to verify if it's there

// https://docs.opensea.io/docs/metadata-standards
public class NFTMetadata {
    public string name; // Name of the item.
    public string description; // A human readable description of the item. Markdown is supported.
    public string image; // This is the URL to the image of the item. Can be just about any type of image (including SVGs, which will be cached into PNGs by OpenSea), and can be IPFS URLs or paths. We recommend using a 350 x 350 image.
    public string external_url; // This is the URL that will appear below the asset's image on OpenSea and will allow users to leave OpenSea and view the item on your site.
    public string animation_url; // link to a video OR HTML page
    public string background_color; // Background color of the item on OpenSea. Must be a six-character hexadecimal without a pre-pended #.
    public string youtube_url; // link to a youtube video
    public NFTAttribute[] attributes; // These are the attributes for the item, which will show up on the OpenSea page for the item.
    public TsvrNftResources[] resources;

    public T ParseAttributes<T>() {
        return JsonHelper.AtomicJsonToObject<T>(attributes.ToString());
    }
}
